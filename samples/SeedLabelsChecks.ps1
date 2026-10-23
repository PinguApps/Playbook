[CmdletBinding()]
param(
  [Parameter(Mandatory)][uri]$BaseUri,
  [Parameter(Mandatory)][ValidateSet(10, 11)][int]$TargetMajor,
  [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $root 'eng/Evidence.ps1')
$response = Invoke-WebRequest $BaseUri
foreach ($text in @('Collect from shelf Fern', 'Return unused packets', '<strong>Keep the envelopes dry</strong>', 'Sea kale: 4 packets', 'Reserve at the desk')) {
  if (-not $response.Content.Contains($text, [StringComparison]::Ordinal)) { throw "Missing SSR output: $text" }
}
$checks = @('HTTP 200; literal text, bound text, bound fragment, batch and reservation render')
if ($TargetMajor -eq 11) {
  $before = (Invoke-RestMethod ([uri]::new($BaseUri, '/probe/results'))).fragmentPersistenceRejections
  $probe = Invoke-WebRequest ([uri]::new($BaseUri, '/probe/fragment'))
  $after = (Invoke-RestMethod ([uri]::new($BaseUri, '/probe/results'))).fragmentPersistenceRejections
  if ($after -ne $before + 1 -or -not $probe.Content.Contains('Local markup only')) { throw 'Actual component persistence did not reject the fragment union.' }
  $boundary = Invoke-WebRequest ([uri]::new($BaseUri, '/probe/boundary')) -SkipHttpErrorCheck
  if ($boundary.StatusCode -ne 500 -or -not $boundary.Content.Contains('RenderFragment')) { throw 'Union-wrapped fragment unexpectedly crossed the SSR boundary.' }
  $checks += 'Real component PersistAsJson rejects RenderFragment; SSR boundary rejects union-wrapped fragment (HTTP 500)'
}
if ($OutputPath) {
  $sample = "samples/net$TargetMajor.0/SeedLabels/SeedLabels.csproj"
  $artifacts = [ordered]@{}
  foreach ($name in @('SeedLabels.dll', 'SeedLabels.deps.json', 'SeedLabels.runtimeconfig.json')) {
    $path = "samples/net$TargetMajor.0/SeedLabels/bin/Release/net$TargetMajor.0/$name"
    $artifacts[$path] = (Get-FileHash -LiteralPath (Join-Path $root $path) -Algorithm SHA256).Hash.ToLowerInvariant()
  }
  [ordered]@{
    kind = 'real HTTP execution'; recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    target = "net$TargetMajor.0"; baseUri = $BaseUri.AbsoluteUri; checks = $checks
    inputs = Get-EvidenceInputs $root $sample; artifacts = $artifacts
    limits = 'Development exception diagnostics required. Browser activation/interop/restore are separate real Chrome checks, not established by HTTP.'
  } | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $OutputPath
}
Write-Host "PASS: .NET $TargetMajor original labels and focused real component/HTTP checks."
