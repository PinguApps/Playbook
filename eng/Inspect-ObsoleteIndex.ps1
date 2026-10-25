[CmdletBinding()]
param(
  [string]$Sources = "$PSScriptRoot/../docs/planning/audits/obsolete-index.sources.json",
  [string]$Ownership = "$PSScriptRoot/../docs/planning/audits/obsolete-index.ownership.json"
)

$ErrorActionPreference = 'Stop'
$manifest = Get-Content -LiteralPath $Sources -Raw | ConvertFrom-Json
$map = Get-Content -LiteralPath $Ownership -Raw | ConvertFrom-Json
$root = Split-Path $PSScriptRoot -Parent
$texts = @{}
foreach ($source in $manifest.sources) {
  if ($texts.ContainsKey($source.id)) { throw "Duplicate source: $($source.id)" }
  if ($source.url -notmatch '^https://raw\.githubusercontent\.com/dotnet/(docs|runtime)/[a-f0-9]{40}/') {
    throw "Source must be immutable official text: $($source.id)"
  }
  $response = Invoke-WebRequest -Uri $source.url
  $bytes = $response.RawContentStream.ToArray()
  $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
  if ($hash -cne $source.sha256) { throw "Source hash mismatch: $($source.id)" }
  $texts[$source.id] = [Text.Encoding]::UTF8.GetString($bytes)
}

function Get-IndexIds([string]$Text) {
  @([regex]::Matches($Text, '(?m)^\| \[(SYSLIB\d{4}|EXTOBS\d{4})\]\(') |
    ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
}

function Assert-SameIds([string[]]$Expected, [string[]]$Actual, [string]$Name) {
  if (@(Compare-Object $Expected $Actual).Count) { throw "Index ownership differs: $Name" }
}

function Assert-Evidence([string]$Link) {
  $parts = $Link -split '#', 2
  $file = Join-Path $root $parts[0]
  if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Missing evidence: $Link" }
  if ($parts.Count -eq 2) {
    $anchors = @(Get-Content -LiteralPath $file | Where-Object { $_ -match '^#{1,6} ' } |
      ForEach-Object { ($_ -replace '^#{1,6} ', '').ToLowerInvariant() -replace '[^\p{L}\p{N}_\- ]', '' -replace ' ', '-' })
    if ($parts[1] -cnotin $anchors) { throw "Missing evidence heading: $Link" }
  }
}

$owners = @{}
foreach ($owner in $map.owners) {
  if ($owners.ContainsKey($owner.issue) -or $owner.issue -notmatch '^PIN-\d+$') {
    throw "Invalid or duplicate owner: $($owner.issue)"
  }
  if ($owner.teamId -ne '62f80270-b641-4fa3-b60c-46be4c4df2c8' -or
    $owner.projectId -ne 'b22380c7-6fde-49f4-8826-e5631dc5df7a') { throw 'Owner outside verified Linear boundary' }
  Assert-Evidence $owner.evidence
  $owners[$owner.issue] = $owner
}

$ids = @()
foreach ($entry in $map.entries) {
  if ($entry.id -in $ids) { throw "Duplicate diagnostic ownership: $($entry.id)" }
  if (-not $owners.ContainsKey($entry.owner)) { throw "Unowned diagnostic: $($entry.id)" }
  if (-not $texts.ContainsKey($entry.id)) { throw "Missing diagnostic reference: $($entry.id)" }
  if ($entry.outcome -notin @('historical', 'grouped-delivery-outstanding', 'grouped-proposal-outstanding', 'delivered')) {
    throw "Unknown disposition: $($entry.id)"
  }
  if ([string]::IsNullOrWhiteSpace($entry.subject) -or [string]::IsNullOrWhiteSpace($entry.evidence)) {
    throw "Missing disposition evidence: $($entry.id)"
  }
  Assert-Evidence $entry.evidence
  $ids += $entry.id
}
Assert-SameIds (Get-IndexIds $texts['global-index']) $ids 'global SYSLIB/EXTOBS index'
Assert-SameIds @('SYSLIB0064', 'SYSLIB0066') (Get-IndexIds $texts['core-11']) '.NET 11 core summary'
Assert-SameIds @('SYSLIB0065') (Get-IndexIds $texts['crypto-11']) '.NET 11 crypto summary'
$registryIds = @([regex]::Matches($texts['runtime-registry'], '(?m)^\|\s+__`(SYSLIB0\d{3})`__') |
  ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
Assert-SameIds @($ids | Where-Object { $_ -like 'SYSLIB*' }) $registryIds 'runtime obsolete registry'
Assert-SameIds @('SYSLIB0063', 'SYSLIB0064', 'SYSLIB0065', 'SYSLIB0066') @(
  $map.entries | Where-Object outcome -ne 'historical' | ForEach-Object id
) 'new .NET 11 obligations'
if ($ids.Count -ne 68 -or $manifest.sources.Count -ne 72) { throw 'Unexpected bounded index/source size' }

# The registry also allocates generator, suppression and experimental IDs. They are
# not obsolete APIs; retain exact ownership without mistaking reserved slots for features.
$otherIds = @([regex]::Matches($texts['runtime-registry'], '(?m)^\|\s+__`(SYSLIB(?:\d{4}|SUPPRESS\d{4}))`__') |
  ForEach-Object { $_.Groups[1].Value } | Where-Object { $_ -notin $registryIds } | Sort-Object -Unique)
$mappedOther = @()
foreach ($group in $map.otherDiagnostics) {
  if (-not $owners.ContainsKey($group.owner)) { throw "Unknown diagnostic-family owner: $($group.owner)" }
  $mappedOther += @($group.ids)
}
if (@($mappedOther | Sort-Object -Unique).Count -ne $mappedOther.Count) { throw 'Duplicate non-obsolete diagnostic ownership' }
Assert-SameIds $otherIds $mappedOther 'non-obsolete registry boundary'

$map.entries | Where-Object outcome -ne 'historical' | Format-Table id, subject, owner, outcome -AutoSize
$map.entries | Group-Object outcome | Select-Object Name, Count | Format-Table -AutoSize
"PASS: 72 immutable sources; 68 obsolete entries; $($otherIds.Count) non-obsolete registry entries; no unowned or duplicate IDs."
'Source/index ownership only. Recipe behavior, native execution and GA/publication readiness are not established.'
