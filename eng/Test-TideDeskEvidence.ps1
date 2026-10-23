$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$execution = Get-Content (Join-Path $root 'docs/verification/PIN-788.execution.evidence.json') -Raw | ConvertFrom-Json
foreach ($sample in $execution.samples) {
  foreach ($input in $sample.inputs.PSObject.Properties) {
    if ((Get-FileHash -LiteralPath (Join-Path $root $input.Name)).Hash.ToLowerInvariant() -cne $input.Value) { throw "Changed executed input: $($input.Name)" }
  }
  $directory = Split-Path (Join-Path $root $sample.sample)
  $target = if ($sample.sample -match 'net10.0') { 'net10.0' } else { 'net11.0' }
  foreach ($file in $sample.artifacts.PSObject.Properties) {
    $suffix = if ($file.Name.StartsWith('_framework/', [StringComparison]::Ordinal)) { 'wwwroot/' + $file.Name } else { $file.Name }
    $path = Join-Path $directory "bin/Release/$target/$suffix"
    if ((Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant() -cne $file.Value) { throw "Changed executed artifact: $path" }
  }
}
foreach ($probe in $execution.probes) {
  if ((Get-FileHash -LiteralPath (Join-Path $root $probe.path)).Hash.ToLowerInvariant() -cne $probe.sha256) { throw "Changed probe: $($probe.path)" }
}
Write-Host 'PASS: exact executed inputs, managed/browser artifacts and probe hashes remain equal.'
