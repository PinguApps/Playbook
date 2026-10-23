[CmdletBinding()]
param([switch]$CaptureBinding)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
function Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
foreach ($version in @(10, 11)) {
  $sample = "samples/net$version.0/CultureHarbour/Host"
  $directory = Join-Path $root $sample
  $manifest = Get-Content (Join-Path $directory "obj/Release/net$version.0/staticwebassets.build.json") -Raw | ConvertFrom-Json
  $artifacts = [ordered]@{}
  foreach ($mode in @('aligned', 'independent')) {
    $report = Get-Content (Join-Path $directory "browser-$mode.evidence.json") -Raw | ConvertFrom-Json
    if ($report.kind -cne 'real-browser-webassembly' -or $report.confirmations -ne 1 -or
        $report.independent -ne ($mode -eq 'independent') -or $report.edition -ne "$version") { throw 'Wrong execution record.' }
    $before = @($report.events | Where-Object kind -CEQ 'before-run')[0]
    $render = @($report.events | Where-Object kind -CEQ 'first-render')[0]
    if ($before.culture -cne 'en-US' -or $before.ui -cne 'en-US' -or
        $report.satellite.start -le $before.at -or $report.satellite.end -gt $render.at -or
        $report.satellite.bytes -le 0) { throw 'Missing actual different-culture satellite timing.' }
    $culture = if ($mode -eq 'independent') { 'de-DE' } else { 'en-GB' }
    $ui = if ($mode -eq 'independent') { 'de-DE' } else { 'fr-FR' }
    if ($report.hydrated.culture -cne $culture -or $report.hydrated.ui -cne $ui -or
        $report.prerendered.culture -cne 'en-GB' -or $report.prerendered.ui -cne 'fr-FR') { throw 'Wrong server/client culture policy.' }
    foreach ($artifact in $report.artifacts) {
      if ($artifact.url -match '/culture.js$') { $local = Join-Path $directory 'wwwroot/culture.js' }
      else {
        $base64 = [Convert]::ToBase64String([Convert]::FromHexString($artifact.sha256))
        $matches = @($manifest.Assets | Where-Object { $_.Integrity -ceq $base64 -and $_.AssetRole -cne 'Alternative' })
        if ($matches.Count -ne 1) { throw "Executed artifact has no unique current build asset: $($artifact.url)" }
        $local = $matches[0].Identity
      }
      if ((Hash $local) -cne $artifact.sha256) { throw "Executed browser artifact changed: $local. Re-execute the real browser, never relabel old proof." }
      $artifacts[[IO.Path]::GetRelativePath($root, $local).Replace('\', '/')] = $artifact.sha256
    }
  }
  foreach ($name in @('CultureHarbour.Host.dll', 'CultureHarbour.Host.deps.json', 'CultureHarbour.Host.runtimeconfig.json')) {
    $path = "$sample/bin/Release/net$version.0/$name"
    $artifacts[$path] = Hash (Join-Path $root $path)
  }
  $inputs = Get-EvidenceInputs $root "$sample/Host.csproj"
  $bindingPath = Join-Path $directory 'execution-binding.evidence.json'
  if ($CaptureBinding) {
    $record = [ordered]@{
      kind = 'post-execution-artifact-equality'
      recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      sourceCommit = (& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
      sourceDirty = [bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain)
      provenance = 'Original dirty-checkout browser captures retain their own timestamps. This later binding fingerprints settled inputs and verifies every fetched artifact against actual current build bytes. It does not relabel execution as clean or later execution.'
      evaluatedExpression = 'Exact UTF-8 eng/CultureHarbour.browser.js, without transformation, passed to Chrome extension CDP Runtime.evaluate with awaitPromise/returnByValue.'
      expressionSha256 = Hash (Join-Path $PSScriptRoot 'CultureHarbour.browser.js')
      verifierSha256 = Hash $PSCommandPath
      inputs = $inputs
      artifacts = $artifacts
      reports = [ordered]@{
        aligned = Hash (Join-Path $directory 'browser-aligned.evidence.json')
        independent = Hash (Join-Path $directory 'browser-independent.evidence.json')
      }
    }
    [IO.File]::WriteAllText($bindingPath, ($record | ConvertTo-Json -Depth 20).Replace("`r`n", "`n") + "`n")
  }
  else {
    $saved = Get-Content $bindingPath -Raw | ConvertFrom-Json -AsHashtable
    foreach ($field in @('inputs', 'artifacts')) {
      $current = if ($field -eq 'inputs') { $inputs } else { $artifacts }
      if ($saved[$field].Count -ne $current.Count) { throw "Changed execution $field set." }
      foreach ($key in $current.Keys) { if ($saved[$field][$key] -cne $current[$key]) { throw "Stale execution $field : $key" } }
    }
    if ($saved.verifierSha256 -cne (Hash $PSCommandPath) -or
        $saved.expressionSha256 -cne (Hash (Join-Path $PSScriptRoot 'CultureHarbour.browser.js'))) { throw 'Changed browser verifier.' }
    foreach ($mode in @('aligned', 'independent')) {
      if ($saved.reports[$mode] -cne (Hash (Join-Path $directory "browser-$mode.evidence.json"))) { throw 'Changed browser report.' }
    }
  }
  Write-Host "PASS .NET $version real browser records, culture policies, satellite timing and exact executed artifact/input binding."
}
