[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Browser,
  [Parameter(Mandatory)][string]$Network
)
$ErrorActionPreference = 'Stop'
$browserRuns = @(Get-Content -LiteralPath $Browser -Raw | ConvertFrom-Json)
$networkRuns = Get-Content -LiteralPath $Network -Raw | ConvertFrom-Json -AsHashtable
if ($browserRuns.Count -ne 2) { throw 'Expected actual old/new browser captures.' }
for ($index = 0; $index -lt 2; $index++) {
  $run = $browserRuns[$index]
  $port = 5991 + $index
  $origin = ([uri]$run.initial.url).GetLeftPart([UriPartial]::Authority)
  $document = $run.initial.state.documentId
  if (-not $document -or $run.initial.state.evaluations.Count) { throw 'Invalid initial document.' }
  foreach ($stage in @('hintedBefore', 'hintedAfter', 'controlBefore', 'controlAfter', 'warmAfter')) {
    if ($run.$stage.state.documentId -cne $document -or $run.$stage.state.enhancedLoads -le 0) { throw "Not enhanced navigation: $stage" }
  }
  if ($run.hintedBefore.hints.Count -ne 2 -or $run.hintedBefore.state.evaluations.Count -ne 0) { throw 'Hints must arrive before module evaluation.' }
  $preload = @($run.hintedBefore.hints | Where-Object rel -CEQ preload)
  $module = @($run.hintedBefore.hints | Where-Object rel -CEQ modulepreload)
  if ($preload.Count -ne 1 -or $preload[0].as -cne 'fetch' -or $preload[0].type -cne 'application/json' -or $preload[0].crossOrigin -cne 'anonymous' -or $preload[0].href -cne "$origin/ledger-data.json?view=hinted" -or $module.Count -ne 1 -or $module[0].href -cne "$origin/ledger-module.js?view=hinted") { throw 'Incorrect authored hints.' }
  if ($run.controlBefore.hints.Count -or @($run.controlBefore.resources | Where-Object name -Like '*view=control').Count) { throw 'Control resources were loaded before consumption.' }
  foreach ($stage in @('hintedAfter', 'controlAfter', 'warmAfter', 'fullAfter')) {
    if ($run.$stage.result -cne '7 dispatches ready for Tern Quay') { throw "Actual application behavior failed: $stage" }
  }
  if ($run.hintedAfter.state.evaluations.Count -ne 1 -or $run.controlAfter.state.evaluations.Count -ne 2 -or $run.warmAfter.state.evaluations.Count -ne 2) { throw 'Unexpected module evaluation lifetime.' }
  if ($run.full.state.documentId -ceq $document -or $run.full.state.enhancedLoads -ne 0 -or $run.full.state.evaluations.Count) { throw 'Full-navigation opt-out did not replace the document.' }
  $requests = $networkRuns["$port"].full.requests
  $runtime = if ($index -eq 0) { '.NET 10.0.13' } else { '.NET 11.0.0-rc.2.26504.105' }
  if ($networkRuns["$port"].full.runtime -cne $runtime) { throw 'Wrong executed runtime.' }
  foreach ($route in @('/ledger/hinted', '/ledger/control')) {
    if (-not @($requests | Where-Object { $_.path -ceq $route -and $_.enhanced -eq $true -and $_.accept -ceq 'text/html; blazor-enhanced-nav=on' -and $_.destination -ceq 'empty' }).Count) { throw "Missing actual enhanced fetch for $route" }
  }
  foreach ($resource in @('/ledger-data.json?view=hinted', '/ledger-module.js?view=hinted')) {
    $hits = @($requests | Where-Object { $_.path -ceq $resource -and [DateTimeOffset]$_.at -le [DateTimeOffset]$run.hintedBefore.at })
    if (-not $hits.Count) { throw "No real resource arrival before consumption: $resource" }
  }
  foreach ($resource in @('/ledger-data.json?view=control', '/ledger-module.js?view=control')) {
    $hits = @($requests | Where-Object path -CEQ $resource)
    if (-not $hits.Count -or @($hits | Where-Object { [DateTimeOffset]$_.at -le [DateTimeOffset]$run.controlBefore.at }).Count) { throw "Invalid real no-hint control: $resource" }
  }
  if (-not @($requests | Where-Object { $_.path -ceq '/ledger/full' -and $_.enhanced -eq $false -and $_.destination -ceq 'document' }).Count) { throw 'Missing ordinary document request.' }
  $warm = @($run.warmAfter.resources | Where-Object { $_.name -ceq "$origin/ledger-data.json?view=hinted" -and $_.transferSize -eq 0 })
  if (-not $warm.Count) { throw 'This recorded warm-cache run lacks the observed zero-transfer reuse.' }
  Write-Host "PASS: $runtime actual enhanced fetches, authored hints, deferred execution, no-hint control, warm reuse and full-navigation opt-out."
}
