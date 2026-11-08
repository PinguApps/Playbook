[CmdletBinding()]
param([string]$PackagePath)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$ledger = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/content-readiness.inventory.json') -Raw | ConvertFrom-Json -AsHashtable

foreach ($input in @($ledger.inputs) + @($ledger.audits)) {
    $path = Join-Path $root $input.path
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $input.sha256) {
        throw "Readiness checkpoint input changed: $($input.path). Reconcile the affected coverage before refreshing the checkpoint."
    }
}
foreach ($row in $ledger.currentRows) {
    $line = @(Get-Content -LiteralPath (Join-Path $root $row.path))[$row.line - 1]
    if ($line -cne $row.disposition) { throw "Coverage row drift: $($row.path):$($row.line)" }
    foreach ($id in $row.resolvedRecipeMatches) {
        if (-not (Test-Path -LiteralPath (Join-Path $root "content/recipes/$id") -PathType Container)) {
            throw "Missing coverage destination: $id"
        }
    }
}
$expected = @(
    'csharp.extend-uneditable-types', 'csharp.validate-property-assignment',
    'aspnet-core.validate-requests', 'ef.include-archived-with-tenant-filter',
    'linq.retain-unmatched-rows', 'json.reject-malformed-contracts',
    'linq.transform-async-stream', 'sdk.run-file-utility'
)
$actual = @($ledger.foundations | ForEach-Object { $_.ids[0] })
if (@(Compare-Object $expected $actual).Count -ne 0 -or $actual.Count -ne 8) { throw 'The exact eight foundation identities changed.' }
foreach ($id in $expected) {
    if (-not (Test-Path -LiteralPath (Join-Path $root "content/recipes/$id/10.0/recipe.json"))) { throw "Missing foundation edition: $id/10.0" }
}
$pending = @(Get-ChildItem -LiteralPath (Join-Path $root 'content/recipes') -Recurse -Filter recipe.json | ForEach-Object {
    $recipe = Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
    if ($recipe.humanProof.status -ceq 'pending') { "$($recipe.id)/$($recipe.edition)" }
})
$savedPending = @($ledger.pendingProof | ForEach-Object { "$($_.id)/$($_.edition)" })
if (@(Compare-Object $pending $savedPending).Count -ne 0) { throw 'Pending proof changed; reconcile the checkpoint rather than treating it as current.' }
if ($ledger.previewPolicy) {
    $policyPath = Join-Path $root $ledger.previewPolicy.path
    if ((Get-FileHash -LiteralPath $policyPath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $ledger.previewPolicy.sha256) {
        throw 'Preview approval changed; reconcile the checkpoint.'
    }
    $policy = Get-Content -LiteralPath $policyPath -Raw | ConvertFrom-Json
    foreach ($exception in $policy.exceptions) {
        $directory = Join-Path $root "content/recipes/$($exception.id)/$($exception.edition)"
        if ((Get-FileHash -LiteralPath (Join-Path $directory 'recipe.json') -Algorithm SHA256).Hash.ToLowerInvariant() -cne $exception.metadataSha256 -or
            (Get-FileHash -LiteralPath (Join-Path $directory 'recipe.md') -Algorithm SHA256).Hash.ToLowerInvariant() -cne $exception.markdownSha256) {
            throw "Preview approval inputs changed: $($exception.id)/$($exception.edition)"
        }
    }
}
if ($PackagePath) {
    if ((Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $ledger.candidatePackage.sha256) {
        throw 'The supplied artifact is not the audited candidate.'
    }
}
Write-Host "PASS: checkpoint input hashes, $($ledger.currentRows.Count) row locations/destinations, exact foundations, preview approval and $($pending.Count) pending gates match. Stable readiness remains gated."
