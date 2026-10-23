[CmdletBinding()]
param(
  [Parameter(Mandatory)][uri]$BaseUri,
  [Parameter(Mandatory)][ValidateSet(10, 11)][int]$TargetMajor,
  [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$checks = [Collections.Generic.List[object]]::new()
function Check-Response([string]$Path, [int]$Status, [string]$Expected) {
  $response = Invoke-WebRequest ([uri]::new($BaseUri, $Path)) -SkipHttpErrorCheck
  if ($response.StatusCode -ne $Status -or -not $response.Content.Contains($Expected, [StringComparison]::Ordinal)) {
    throw "Unexpected response for $Path : status $($response.StatusCode), expected $Status / $Expected"
  }
  $checks.Add([ordered]@{ path = $Path; status = [int]$response.StatusCode; expected = $Expected })
  return $response.Content
}

$html = Check-Response '/' 200 'Prepared for reading desk Cedar.'
foreach ($text in @('Harbour ledger, box 27', 'Keep the ledger on its padded support.', 'Request another reading')) {
  if (-not $html.Contains($text, [StringComparison]::Ordinal)) { throw "Missing $text" }
}
if ($TargetMajor -eq 10) {
  $null = Check-Response '/probe/direct' 500 "Cannot pass the parameter 'ChildContent'"
}
else {
  $null = Check-Response '/probe/generic' 500 "Cannot pass RenderFragment<T> parameter 'Template'"
  $null = Check-Response '/probe/delegate' 500 "Cannot pass the parameter 'Callback'"
  $null = Check-Response '/probe/no-prerender' 500 'disabled prerendering or conditional rendering'
  $null = Check-Response '/probe/unrendered' 500 'RenderFragment was not invoked during rendering'
  $null = Check-Response '/probe/discarded-handler' 200 'Parent callbacks: 0'
}
if ($OutputPath) {
  $sample = "samples/net$TargetMajor.0/ArchiveCards/ArchiveCards.csproj"
  $artifacts = [ordered]@{}
  foreach ($name in @('ArchiveCards.dll', 'ArchiveCards.deps.json', 'ArchiveCards.runtimeconfig.json')) {
    $path = "samples/net$TargetMajor.0/ArchiveCards/bin/Release/net$TargetMajor.0/$name"
    $artifacts[$path] = (Get-FileHash -LiteralPath (Join-Path $root $path) -Algorithm SHA256).Hash.ToLowerInvariant()
  }
  [ordered]@{
    kind = 'real HTTP execution'; recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    target = "net$TargetMajor.0"; baseUri = $BaseUri.AbsoluteUri; checks = $checks
    inputs = Get-EvidenceInputs $root $sample; artifacts = $artifacts
    limits = 'Development exception diagnostics required. HTTP proves SSR and boundary refusal, not circuit activation; see separate real browser report.'
  } | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $OutputPath
}
Write-Host "PASS: .NET $TargetMajor original SSR content and actual render-boundary restrictions."
