[CmdletBinding()]
param([switch]$GuardsOnly)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$pwsh = (Get-Command pwsh -CommandType Application | Select-Object -First 1).Source
$run = Join-Path $root ('artifacts/preview-policy-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $run
$builder = Join-Path $PSScriptRoot 'Build-Content.ps1'
$policyPath = Join-Path $root 'content/preview-evidence.json'
function Reject([string[]]$Arguments, [string]$Expected) {
    $output = & $pwsh -NoProfile -File $builder -OutputPath (Join-Path $run 'rejected.json') @Arguments 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or -not $output.Contains($Expected)) { throw "Readiness guard failed: $Expected`n$output" }
}
Reject @('-PreviewReady','-PackageVersion','1.0.0') 'Preview readiness requires a preview.N package version.'
Reject @('-PreviewReady','-PackageVersion',"1.0.0-preview.1`n") 'Preview readiness requires a preview.N package version.'
Reject @('-PreviewReady','-ReleaseReady','-PackageVersion','1.0.0-preview.1') 'Choose stable or preview readiness'
$policy = Get-Content -LiteralPath $policyPath -Raw | ConvertFrom-Json -AsHashtable
$first = @($policy.exceptions | Sort-Object id)[0]
$policy.exceptions = @($policy.exceptions | Where-Object { $_.id -cne $first.id })
$fixture = Join-Path $run 'policy.json'
$policy | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $fixture
Reject @('-PreviewReady','-PackageVersion','1.0.0-preview.1','-PreviewPolicyPath',$fixture) 'Unapproved pending preview evidence'
$policy = Get-Content -LiteralPath $policyPath -Raw | ConvertFrom-Json -AsHashtable
@($policy.exceptions | Sort-Object id)[0].markdownSha256 = '0' * 64
$policy | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $fixture
Reject @('-PreviewReady','-PackageVersion','1.0.0-preview.1','-PreviewPolicyPath',$fixture) 'Preview exception inputs changed'
Reject @('-ReleaseReady') 'Pending Human proof blocks release readiness'
if ($GuardsOnly) {
    Write-Host 'PASS: preview/stable channel, unapproved pending proof and changed exception inputs are guarded.'
    $global:LASTEXITCODE = 0
    return
}
& $pwsh -NoProfile -File $builder -OutputPath (Join-Path $run 'preview.json') -PreviewReady -PackageVersion '1.0.0-preview.1'
if ($LASTEXITCODE -ne 0) { throw 'Approved preview content failed validation.' }
$served = Get-Content -LiteralPath (Join-Path $run 'preview.json') -Raw | ConvertFrom-Json
foreach ($exception in (Get-Content -LiteralPath $policyPath -Raw | ConvertFrom-Json).exceptions) {
    $edition = @($served | Where-Object { $_.id -ceq $exception.id -and $_.edition -ceq $exception.edition })
    if ($edition.Count -ne 1 -or $edition[0].humanProof.status -cne 'pending') { throw 'Preview approval cleared or lost pending proof.' }
}
Write-Host 'PASS: approved preview validates; stable versions, mixed modes, unapproved pending proof and changed exception inputs are rejected; pending proof remains visible.'
