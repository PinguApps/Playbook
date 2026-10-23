[CmdletBinding()]
param(
  [string]$Recipe = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin799/inspection/recipe.json'),
  [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$recipePath = [IO.Path]::GetFullPath($Recipe)
$response = Get-Content -LiteralPath $recipePath -Raw | ConvertFrom-Json
$body = $response.markdown
$blocks = [regex]::Matches($body, '(?ms)^### \.NET (10|11): `([^`]+)`\n\n```[^\n]*\n(.*?)^```$')
if ($blocks.Count -ne 20) { throw "Expected all 20 complete source/configuration files; got $($blocks.Count)." }
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-pin799-consumer-' + [guid]::NewGuid().ToString('N'))
$inputs = [ordered]@{}
foreach ($block in $blocks) {
  $relative = "net$($block.Groups[1].Value).0/CairnSignals/$($block.Groups[2].Value)"
  $path = Join-Path $standalone $relative
  New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
  [IO.File]::WriteAllText($path,$block.Groups[3].Value)
  $inputs[$relative] = (Get-FileHash $path).Hash.ToLowerInvariant()
}
$results = Join-Path $root 'artifacts/pin799/consumer'
& (Join-Path $PSScriptRoot 'Test-CairnSignals.ps1') -Samples $standalone -Results $results -KeepRunning:$KeepRunning
$projects = @()
foreach ($major in @(10,11)) {
  $directory = Join-Path $standalone "net$major.0/CairnSignals"
  $sample = "samples/net$major.0/CairnSignals/CairnSignals.csproj"
  $expected = @($response.evidence | Where-Object sample -CEQ $sample)
  if ($expected.Count -ne 1) { throw "No unique served evidence for $sample" }
  $assets = Get-Content (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
  $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
  if (($dependencies | ConvertTo-Json -Compress) -cne (@($expected[0].dependencies) | ConvertTo-Json -Compress)) { throw "Dependency closure differs: $sample" }
  [xml]$project = Get-Content (Join-Path $directory 'CairnSignals.csproj') -Raw
  $sdk = Get-Content (Join-Path $directory 'global.json') -Raw | ConvertFrom-Json
  if ($project.Project.PropertyGroup.TargetFramework -cne $expected[0].target -or $project.Project.PropertyGroup.LangVersion -cne $expected[0].language -or $sdk.sdk.version -cne $expected[0].sdk) { throw "Returned setup does not match evidence: $sample" }
  $projects += @{sample=$sample;target=$expected[0].target;language=$expected[0].language;sdk=$expected[0].sdk;dependencies=$dependencies}
}
@{kind='standalone-reconstruction-from-real-stdio-markdown';recordedAt=[DateTimeOffset]::UtcNow.ToString('O');directory=$standalone;recipeSha256=(Get-FileHash $recipePath).Hash.ToLowerInvariant();bodySha256=[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($body)));verifierSha256=(Get-FileHash $PSCommandPath).Hash.ToLowerInvariant();inputs=$inputs;projects=$projects;scope='All 20 source/setup files extracted solely from the expanded installed-tool response, without canonical checkout copying. Real builds and served assets; separate browser receipts establish native event behavior.'} | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $results 'reconstruction.evidence.json')
Write-Host "PASS: both complete consumers reconstructed only from returned Markdown: $standalone"
