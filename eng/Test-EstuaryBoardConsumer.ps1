[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$RecipeResponse,
  [Parameter(Mandatory)][string]$OutputDirectory,
  [string]$Dotnet10 = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Dotnet11 = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$responsePath = (Resolve-Path -LiteralPath $RecipeResponse).Path
$recipe = (Get-Content -LiteralPath $responsePath -Raw | ConvertFrom-Json).result.structuredContent.recipe
if ($recipe.id -cne 'blazor.supply-browser-configuration' -or $recipe.edition -cne '11.0') { throw 'Wrong returned recipe.' }
$markdown = $recipe.markdown.Replace("`r`n", "`n")
if ($markdown.Contains('{{snippet:')) { throw 'Unexpanded returned Markdown.' }
$blocks = [regex]::Matches($markdown, '(?ms)^```[^\n]*\n(.*?)^```$')
if ($blocks.Count -ne 16) { throw 'Review the reconstruction map after changing canonical files.' }
$pins = [regex]::Matches($markdown, 'global.json containing (\{[^\n]+?\})\.')
if ($pins.Count -ne 2) { throw 'Missing complete SDK pins in returned Markdown.' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'Use a new disposable consumer directory.' }
New-Item -ItemType Directory -Path $output | Out-Null
$beforePaths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $beforePaths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previousToolsPath = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$receipt = [ordered]@{
  recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
  responseSha256 = (Get-FileHash -LiteralPath $responsePath -Algorithm SHA256).Hash.ToLowerInvariant()
  verifierSha256 = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant()
  scope = 'Actual standalone restore and untrimmed interpreted publish from returned expanded Markdown only; browser receipts are separate.'
  outputDirectory = $output
  samples = [ordered]@{}
}
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in @(10, 11)) {
    $directory = Join-Path $output "net$major.0"
    New-Item -ItemType Directory -Path (Join-Path $directory 'wwwroot'), (Join-Path $directory 'environments') | Out-Null
    $start = if ($major -eq 10) { 0 } else { 3 }
    $files = [ordered]@{
      'EstuaryBoard.csproj' = $start
      'Directory.Packages.props' = $start + 1
      'Program.cs' = $start + 2
      'ConfigurationTrace.cs' = 6
      'Dashboard.razor' = 7
      'wwwroot/index.html' = 8
      'wwwroot/board-start.js' = 9
      'wwwroot/appsettings.json' = 10
      'wwwroot/appsettings.Testing.json' = 11
      'wwwroot/appsettings.Production.json' = 12
      'environments/Testing.json' = 13
      'environments/Production.json' = 14
      'serve.mjs' = 15
    }
    foreach ($file in $files.Keys) { [IO.File]::WriteAllText((Join-Path $directory $file), $blocks[$files[$file]].Groups[1].Value.TrimEnd() + "`n") }
    [IO.File]::WriteAllText((Join-Path $directory 'global.json'), $pins[$major - 10].Groups[1].Value + "`n")
    $inputs = [ordered]@{}
    foreach ($file in (Get-ChildItem -LiteralPath $directory -Recurse -File | Sort-Object FullName)) {
      $inputs[[IO.Path]::GetRelativePath($directory, $file.FullName).Replace('\', '/')] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $dotnet = if ($major -eq 10) { $Dotnet10 } else { $Dotnet11 }
    Push-Location $directory
    try {
      $sdk = (& $dotnet --version | Out-String).Trim()
      if ($sdk -cne (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version) { throw 'Wrong actual SDK.' }
      $restore = (& $dotnet restore | Out-String).Trim()
      $publish = (& $dotnet publish -c Release --no-restore -p:PublishTrimmed=false | Out-String).Trim()
      $receipt.samples["net$major.0"] = [ordered]@{
        sdk = $sdk
        inputs = $inputs
        resolvedDependencies = @((Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable).libraries.Keys | Sort-Object -CaseSensitive)
        applicationDllSha256 = (Get-FileHash -LiteralPath "bin/Release/net$major.0/EstuaryBoard.dll" -Algorithm SHA256).Hash.ToLowerInvariant()
        restore = $restore
        publish = $publish
      }
      Write-Host "PASS: standalone .NET $major consumer from actual returned Markdown."
    }
    finally { Pop-Location }
  }
  [IO.File]::WriteAllText((Join-Path $output 'consumer.evidence.json'), ($receipt | ConvertTo-Json -Depth 30).Replace("`r`n", "`n") + "`n")
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousToolsPath
  foreach ($scope in $beforePaths.Keys) {
    if ($beforePaths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings before changing them." }
  }
}
