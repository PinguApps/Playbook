[CmdletBinding()]
param(
  [string]$Recipe = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin798/inspected-recipe.json'),
  [switch]$KeepRunning,
  [bool]$AllowOldInline = $false
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$response = Get-Content -LiteralPath $Recipe -Raw | ConvertFrom-Json
if ($response.id -cne 'blazor.toggle-navigation-with-csp' -or $response.edition -cne '11.0') { throw 'Wrong returned recipe/edition.' }
$body = $response.markdown
$files = [regex]::Matches($body, '(?ms)^### File: BrackenMenu(10|11)/([^\r\n]+)\r?\n\s*```[^\n]*\n(.*?)^```$')
if ($files.Count -ne 50 -or [regex]::Matches($body, '(?m)^```[^\n]+$').Count -ne 50) { throw "Expected 50 complete named files in actual expanded stdio Markdown; got $($files.Count)." }
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-pin798-consumer-' + [guid]::NewGuid().ToString('N'))
$written = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($file in $files) {
  $relative = "net$($file.Groups[1].Value).0/BrackenMenu/$($file.Groups[2].Value)"
  $path = [IO.Path]::GetFullPath((Join-Path $standalone $relative))
  if (-not $path.StartsWith($standalone + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or -not $written.Add($relative)) { throw 'Unsafe or duplicate returned file path.' }
  New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
  [IO.File]::WriteAllText($path, $file.Groups[3].Value)
}
$results = Join-Path $root 'artifacts/pin798/consumer-hosts'
& (Join-Path $PSScriptRoot 'Test-BrackenMenu.ps1') -Samples $standalone -Results $results -AllowOldInline:$AllowOldInline -KeepRunning:$KeepRunning
$projects = foreach ($project in Get-ChildItem "$standalone/net*/BrackenMenu/*/*.csproj") {
  $relative = [IO.Path]::GetRelativePath($standalone, $project.FullName).Replace('\', '/')
  $expected = @($response.evidence | Where-Object { $_.sample -ceq "samples/$relative" })
  if ($expected.Count -ne 1) { throw "No unique served compilation contract for $relative" }
  $assets = Get-Content -LiteralPath (Join-Path $project.DirectoryName 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
  $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
  if (($dependencies | ConvertTo-Json -Compress) -cne (@($expected[0].dependencies) | ConvertTo-Json -Compress)) { throw "Reconstructed dependency closure differs: $relative" }
  [xml]$xml = Get-Content -LiteralPath $project.FullName -Raw
  if ($xml.Project.PropertyGroup.TargetFramework -cne $expected[0].target -or $xml.Project.PropertyGroup.LangVersion -cne $expected[0].language) { throw "Wrong reconstructed target/language: $relative" }
  $dll = Join-Path $project.DirectoryName "bin/Release/$($expected[0].target)/$($project.BaseName).dll"
  @{ project = $relative; target = $expected[0].target; language = $expected[0].language; sdk = $expected[0].sdk; dependencies = $dependencies; dllSha256 = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$inputs = [ordered]@{}
foreach ($file in Get-ChildItem -LiteralPath $standalone -Recurse -File | Sort-Object FullName) {
  if ($file.FullName -match '\\(bin|obj)\\') { continue }
  $inputs[[IO.Path]::GetRelativePath($standalone, $file.FullName).Replace('\', '/')] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
}
@{ kind = 'standalone-reconstruction-from-real-stdio-detail'; capturedAt = [DateTimeOffset]::UtcNow.ToString('O'); directory = $standalone; sourceResponseSha256 = (Get-FileHash -LiteralPath $Recipe -Algorithm SHA256).Hash.ToLowerInvariant(); markdownSha256 = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($body))); verifierSha256 = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant(); hostVerifierSha256 = (Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Test-BrackenMenu.ps1') -Algorithm SHA256).Hash.ToLowerInvariant(); projects = @($projects); inputs = $inputs; scope = 'Four actual projects and all required configuration/assets reconstructed solely from 50 expanded returned-Markdown blocks. No canonical checkout file copied. Real HTTP checks are separate from browser navigation/enforcement receipts.' } | ConvertTo-Json -Depth 25 | Set-Content -LiteralPath (Join-Path $root 'artifacts/pin798/consumer.evidence.json')
Write-Host "PASS: four reconstructed real consumers compiled from returned Markdown at $standalone."
