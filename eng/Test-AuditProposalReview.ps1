[CmdletBinding()]
param(
    [string] $ReviewPath = (Join-Path $PSScriptRoot '../docs/planning/audit-proposal-review.md')
)

$ErrorActionPreference = 'Stop'
$planningDirectory = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../docs/planning'))
$drafts = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($file in Get-ChildItem -LiteralPath (Join-Path $planningDirectory 'audits') -Filter '*.md') {
    $source = Get-Content -LiteralPath $file.FullName -Raw
    $headings = [regex]::Matches($source, '(?m)^(?:### ([A-Z][0-9]*)[. —]+|## ([ABC])\. )(.+)$')
    $runtimeRows = [regex]::Matches($source, '(?m)^\| \*\*(R[0-9]+) `')
    foreach ($match in $headings) {
        $key = if ($match.Groups[1].Success) { $match.Groups[1].Value } else { $match.Groups[2].Value }
        if (-not $drafts.Add("$($file.BaseName):$key")) { throw "Duplicate source draft: $($file.BaseName):$key" }
    }
    foreach ($match in $runtimeRows) {
        if (-not $drafts.Add("$($file.BaseName):$($match.Groups[1].Value)")) { throw "Duplicate runtime draft: $($match.Value)" }
    }
}
if ($drafts.Count -eq 0) { throw 'No source drafts were found' }

$review = Get-Content -LiteralPath $ReviewPath -Raw
$rows = [regex]::Matches($review, '(?m)^\| `(?<key>[a-z0-9-]+:[A-Z][0-9]*)` \| (?<goal>[^|]+) \| (?<decision>[^|]+) \| \[(?<owner>PIN-[0-9]+)\]\(https://linear\.app/pinguapps/issue/\k<owner>\) \| (?<size>[SML]) \((?<value>[235])\) \| (?<dependencies>[^|]+) \|$')
$mapped = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$newOwners = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$approved = 0
$retained = 0
foreach ($row in $rows) {
    $key = $row.Groups['key'].Value
    if (-not $mapped.Add($key)) { throw "Duplicate review draft: $key" }
    if (-not $drafts.Contains($key)) { throw "Review names a nonexistent source draft: $key" }
    if ([string]::IsNullOrWhiteSpace($row.Groups['goal'].Value)) { throw "Missing reusable goal: $key" }
    $scale = @{ S = '2'; M = '3'; L = '5' }
    if ($scale[$row.Groups['size'].Value] -ne $row.Groups['value'].Value) { throw "Wrong native estimate: $key" }
    $decision = $row.Groups['decision'].Value.Trim()
    if ($decision -in @('approved', 'approved; availability gated')) {
        if (-not $newOwners.Add($row.Groups['owner'].Value)) { throw "Two approved slices share a new owner: $key" }
        $approved++
    }
    elseif ($decision -in @('already covered', 'grouped')) { $retained++ }
    else { throw "Missing or unsupported decision: $key ($decision)" }
}
$missing = @($drafts | Where-Object { -not $mapped.Contains($_) } | Sort-Object)
if ($missing.Count -gt 0) { throw "Untracked audit drafts: $($missing -join ', ')" }
if ($mapped.Count -ne $drafts.Count) { throw 'Source/review draft counts disagree' }
if ($review -match 'Ticket read-back: \*\*IN PROGRESS\*\*') { throw 'Live ticket read-back is not complete' }
foreach ($match in [regex]::Matches($review, '\]\((audits/[^)#]+\.md)(?:#[^)]*)?\)')) {
    if (-not (Test-Path -LiteralPath (Join-Path $planningDirectory $match.Groups[1].Value) -PathType Leaf)) {
        throw "Missing source audit: $($match.Groups[1].Value)"
    }
}
Write-Host "PASS: $($drafts.Count) source drafts; $approved approved delivery owners; $retained retained owners; no unmapped or duplicate drafts."
