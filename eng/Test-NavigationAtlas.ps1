[CmdletBinding()]
param(
  [Parameter(Mandatory)][uri]$BaseUri,
  [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$base = $BaseUri.AbsoluteUri.TrimEnd('/') + '/'
$pathBase = ([uri]$base).AbsolutePath
$checks = foreach ($panel in @('overview', 'notes', 'checklist')) {
  $response = Invoke-WebRequest ($base + "collections/kelp/$panel`?sort=date")
  if ($response.StatusCode -ne 200 -or $response.Content -notmatch "Tide archive: kelp / $panel") {
    throw "Route failed: $panel"
  }
  if ($response.Content -notmatch ('<base href="' + [regex]::Escape($pathBase) + '"')) {
    throw "Expected request-derived base $pathBase on $panel"
  }
  [ordered]@{ panel = $panel; status = [int]$response.StatusCode; baseHref = $pathBase }
}
$asset = Invoke-WebRequest ($base + '_framework/blazor.web.js')
if ($asset.StatusCode -ne 200 -or $asset.Content.Length -lt 1000) { throw 'Browser asset did not load below the application base.' }
$result = [ordered]@{
  kind = 'execution'; scope = 'Real loopback HTTP route/base/browser-asset requests; no browser navigation claim.'
  recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); baseUri = $base; routes = @($checks)
  browserAssetStatus = [int]$asset.StatusCode
}
if ($OutputPath) { $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding utf8 }
Write-Host "PASS: three nested routes, request-derived base $pathBase and browser asset."
