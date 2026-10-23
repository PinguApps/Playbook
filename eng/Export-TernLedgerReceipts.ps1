[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$OriginHost,
  [Parameter(Mandatory)][string]$OutputPrefix,
  [switch]$Replay
)
$ErrorActionPreference = 'Stop'
$runs = @()
$network = [ordered]@{}
$receipts = [ordered]@{}
$stages = @('initial', 'hintedBefore', 'hintedAfter', 'controlBefore', 'controlAfter', 'warmAfter', 'full', 'fullAfter')
foreach ($port in @(5991, 5992)) {
  $rawPath = "$OutputPrefix-$port.raw.json"
  $raw = if ($Replay) { [IO.File]::ReadAllText([IO.Path]::GetFullPath($rawPath)) } else { (Invoke-WebRequest "http://localhost:$port/proof").Content }
  $server = $raw | ConvertFrom-Json
  $snapshots = [ordered]@{}
  foreach ($request in $server.requests) {
    if (-not $request.path.StartsWith('/proof?capture=', [StringComparison]::Ordinal)) { continue }
    $json = [uri]::UnescapeDataString($request.path.Substring('/proof?capture='.Length))
    $capture = $json | ConvertFrom-Json
    if (([uri]$capture.url).Host -cne $OriginHost) { continue }
    if ($snapshots.Contains($capture.stage)) { throw "Duplicate capture $port/$($capture.stage)" }
    $snapshots[$capture.stage] = $capture
  }
  if ($snapshots.Count -ne $stages.Count -or @($stages | Where-Object { -not $snapshots.Contains($_) }).Count) { throw "Missing actual recorded browser stage on $port" }
  $runs += $snapshots
  $selected = @($server.requests | Where-Object { [DateTimeOffset]$_.at -ge [DateTimeOffset]$snapshots.initial.at -and -not $_.path.StartsWith('/proof?capture=', [StringComparison]::Ordinal) })
  $network["$port"] = @{ full = @{ runtime = $server.runtime; requests = $selected } }
  if (-not $Replay) { [IO.File]::WriteAllText([IO.Path]::GetFullPath($rawPath), $raw) }
  $receipts["$port"] = @{ rawPath = $rawPath; rawSha256 = (Get-FileHash -LiteralPath $rawPath -Algorithm SHA256).Hash.ToLowerInvariant(); selectedRequests = $selected.Count }
}
[IO.File]::WriteAllText([IO.Path]::GetFullPath("$OutputPrefix-browser.json"), (($runs | ConvertTo-Json -Depth 30).Replace("`r`n", "`n") + "`n"))
[IO.File]::WriteAllText([IO.Path]::GetFullPath("$OutputPrefix-network.json"), (($network | ConvertTo-Json -Depth 30).Replace("`r`n", "`n") + "`n"))
$expression = (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'TernLedger.browser.js') -Raw).TrimEnd()
$record = @{
  capturedAt = [DateTimeOffset]::UtcNow.ToString('O')
  originHost = $OriginHost
  scope = 'Actual Chrome snapshots self-recorded as diagnostic query receipts. Browser JSON is decoded mechanically; selected network objects retain exact original fields from the accepted run, excluding observer requests. Original raw payloads include historical failed observer attempts and are retained separately. No mocked navigation or manually transcribed snapshot.'
  payloads = $receipts
  observerSource = $expression
  evaluatedExpressions = @($stages | ForEach-Object { $expression + '(' + (ConvertTo-Json $_ -Compress) + ')' })
  exporterSource = Get-Content -LiteralPath $PSCommandPath -Raw
  verifierSource = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Test-TernLedgerReceipts.ps1') -Raw
  browserSha256 = (Get-FileHash -LiteralPath "$OutputPrefix-browser.json" -Algorithm SHA256).Hash.ToLowerInvariant()
  networkSha256 = (Get-FileHash -LiteralPath "$OutputPrefix-network.json" -Algorithm SHA256).Hash.ToLowerInvariant()
  replayFromOriginalRawPayloads = [bool]$Replay
} | ConvertTo-Json -Depth 30
[IO.File]::WriteAllText([IO.Path]::GetFullPath("$OutputPrefix-recording.json"), $record.Replace("`r`n", "`n") + "`n")
Write-Host "PASS: sixteen actual browser snapshots decoded from server receipts."
