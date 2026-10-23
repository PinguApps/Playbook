[CmdletBinding()]
param(
  [string]$Recipe = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin796/inspected-recipe.json'),
  [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$response = Get-Content -LiteralPath $Recipe -Raw | ConvertFrom-Json
$body = $response.markdown
$blocks = [regex]::Matches($body, '(?ms)^```[^\n]*\n(.*?)^```$')
$files = @(
  'net10.0/QuaysideRoutes/global.json', 'net10.0/QuaysideRoutes/Directory.Build.props', 'net10.0/QuaysideRoutes/Directory.Build.targets', 'net10.0/QuaysideRoutes/Directory.Packages.props',
  'net10.0/QuaysideRoutes/Client/QuaysideRoutes.Client.csproj', 'net10.0/QuaysideRoutes/Client/Program.cs',
  'net10.0/QuaysideRoutes/Host/QuaysideRoutes.Host.csproj', 'net10.0/QuaysideRoutes/Host/Program.cs', 'net10.0/QuaysideRoutes/Host/appsettings.json',
  'net11.0/QuaysideRoutes/global.json', 'net11.0/QuaysideRoutes/Directory.Build.props', 'net11.0/QuaysideRoutes/Directory.Build.targets', 'net11.0/QuaysideRoutes/Directory.Packages.props',
  'net11.0/QuaysideRoutes/Client/QuaysideRoutes.Client.csproj', 'net11.0/QuaysideRoutes/Client/Program.cs', 'net11.0/QuaysideRoutes/Client/appsettings.json',
  'net11.0/QuaysideRoutes/Defaults/QuaysideRoutes.Defaults.csproj', 'net11.0/QuaysideRoutes/Defaults/Extensions.cs', 'net11.0/QuaysideRoutes/Defaults/wwwroot/QuaysideRoutes.Defaults.lib.module.js',
  'net11.0/QuaysideRoutes/Client/App.razor', 'net11.0/QuaysideRoutes/Client/wwwroot/index.html',
  'net10.0/QuaysideRoutes/Backend/QuaysideRoutes.Backend.csproj', 'net11.0/QuaysideRoutes/Backend/QuaysideRoutes.Backend.csproj', 'net11.0/QuaysideRoutes/Backend/Program.cs',
  'net10.0/QuaysideRoutes/Client/Berth.razor', 'net11.0/QuaysideRoutes/Client/Berth.razor'
)
if ($blocks.Count -ne $files.Count) { throw "Incomplete expanded body: $($blocks.Count) code fences, expected $($files.Count)." }
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-pin796-consumer-' + [guid]::NewGuid().ToString('N'))
for ($index=0; $index -lt $files.Count; $index++) {
  $path = Join-Path $standalone $files[$index]
  New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
  [IO.File]::WriteAllText($path, $blocks[$index].Groups[1].Value)
}
# Only files already extracted from the returned Markdown are shared/copied.
foreach ($file in @('Client/App.razor','Client/wwwroot/index.html','Backend/Program.cs')) {
  New-Item -ItemType Directory -Path (Split-Path (Join-Path $standalone "net10.0/QuaysideRoutes/$file")) -Force | Out-Null
  Copy-Item (Join-Path $standalone "net11.0/QuaysideRoutes/$file") (Join-Path $standalone "net10.0/QuaysideRoutes/$file")
}
foreach ($project in Get-ChildItem "$standalone/net*/QuaysideRoutes/*/*.csproj") {
  Copy-Item (Join-Path (Split-Path $project.DirectoryName) 'global.json') (Join-Path $project.DirectoryName 'global.json')
}
$results = Join-Path $root 'artifacts/pin796/consumer-hosts'
& (Join-Path $PSScriptRoot 'Test-QuaysideRoutes.ps1') -Samples $standalone -Results $results -KeepRunning:$KeepRunning
$projects = @()
foreach ($project in Get-ChildItem "$standalone/net*/QuaysideRoutes/*/*.csproj") {
  $relative = [IO.Path]::GetRelativePath($standalone, $project.FullName).Replace('\','/')
  $expected = @($response.evidence | Where-Object { $_.sample -ceq "samples/$relative" })
  if ($expected.Count -ne 1) { throw "No unique served compilation contract for $relative" }
  $assets = Get-Content (Join-Path $project.DirectoryName 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
  $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
  if (($dependencies | ConvertTo-Json -Compress) -cne (@($expected[0].dependencies) | ConvertTo-Json -Compress)) { throw "Reconstructed dependency closure differs for $relative" }
  [xml]$xml = Get-Content $project.FullName -Raw
  if ($xml.Project.PropertyGroup.TargetFramework -cne $expected[0].target -or $xml.Project.PropertyGroup.LangVersion -cne $expected[0].language) { throw "Reconstructed target/language differs for $relative" }
  $targetPath = Join-Path $project.DirectoryName "bin/Release/$($expected[0].target)/$($project.BaseName).dll"
  $projects += @{ path=$relative; target=$expected[0].target; sdk=$expected[0].sdk; language=$expected[0].language; dependencies=$dependencies; dllSha256=(Get-FileHash $targetPath -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$inputs = [ordered]@{}
foreach ($file in Get-ChildItem $standalone -Recurse -File | Sort-Object FullName) {
  if ($file.FullName -match '\\(bin|obj)\\') { continue }
  $inputs[[IO.Path]::GetRelativePath($standalone,$file.FullName).Replace('\','/')] = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
}
@{ kind='standalone-reconstruction-from-real-stdio-detail'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); directory=$standalone; recipeSha256=(Get-FileHash $Recipe -Algorithm SHA256).Hash.ToLowerInvariant(); bodySha256=[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($body))); verifierSha256=(Get-FileHash $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant(); hostVerifierSha256=(Get-FileHash (Join-Path $PSScriptRoot 'Test-QuaysideRoutes.ps1') -Algorithm SHA256).Hash.ToLowerInvariant(); projects=$projects; inputs=$inputs; scope='All six projects and complete assets/configuration come solely from expanded returned Markdown, including its shared-file instructions. No canonical checkout source/configuration copied. Both real entry/backend paths execute; separate browser receipts establish WASM behavior.' } | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $root 'artifacts/pin796/consumer.evidence.json')
Write-Host "PASS: six standalone projects reconstructed, compiled and served from $standalone."
