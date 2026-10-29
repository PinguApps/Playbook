[CmdletBinding()]
param(
  [string]$ResponseDirectory = (Join-Path $PSScriptRoot '../artifacts/web-resources/inspection'),
  [string]$Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$PackageRoot = 'C:/Users/pingu/.nuget/packages'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$output = Join-Path ([IO.Path]::GetTempPath()) ('playbook-web-consumers-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output | Out-Null
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','NUGET_PACKAGES','HTTP_PROXY','HTTPS_PROXY','ALL_PROXY','NO_PROXY') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:NUGET_PACKAGES = $PackageRoot
  $env:HTTP_PROXY = $env:HTTPS_PROXY = $env:ALL_PROXY = 'http://127.0.0.1:1'
  $env:NO_PROXY = '127.0.0.1,localhost'
  foreach ($entry in @(
    @('hosting.configure-http3-budgets','EstuaryBudgets'),
    @('json-patch.serialize-operation-intent','GlasshousePatch'),
    @('testing.configure-web-application-builder','StrataTests'))) {
    $response = Get-Content (Join-Path $ResponseDirectory ($entry[0] + '.json')) -Raw | ConvertFrom-Json
    $recipe = $response.result.structuredContent.recipe
    if ($recipe.id -cne $entry[0] -or $recipe.edition -cne '11.0' -or $recipe.markdown.Contains('{{snippet:')) { throw 'Incomplete/wrong installed guidance.' }
    foreach ($release in '10','11') {
      $label = if ($release -eq '10') { 'Older' } else { 'Newer' }
      $body = [regex]::Match($recipe.markdown, "(?ms)^## $label approach[^\n]*\n(.*?)(?=^## |\z)").Groups[1].Value
      $blocks = [regex]::Matches($body, '(?ms)^### ([^\r\n]+)\r?\n\s*```[^\r\n]*\r?\n(.*?)^```')
      if ($blocks.Count -lt 6) { throw 'Missing complete canonical files.' }
      $directory = Join-Path $output "$($entry[1])-$release"
      New-Item -ItemType Directory -Path $directory | Out-Null
      foreach ($block in $blocks) {
        $path = [IO.Path]::GetFullPath((Join-Path $directory $block.Groups[1].Value))
        if (-not $path.StartsWith($directory + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'Returned file escapes consumer.' }
        New-Item -ItemType Directory -Force -Path (Split-Path $path) | Out-Null
        [IO.File]::WriteAllText($path,$block.Groups[2].Value.Replace("`r`n","`n"))
      }
      '<configuration><packageSources><clear/></packageSources></configuration>' | Set-Content (Join-Path $directory 'NuGet.Config')
      $hostDotnet = if ($release -eq '10') { $Dotnet } else { $Net11Dotnet }
      Push-Location $directory
      try {
        # Initial restore creates locks from the exact direct pins and prepared
        # cache; no repository source file or remote feed can supply omitted code.
        & $hostDotnet restore ($entry[1] + '.csproj') --configfile NuGet.Config
        & $hostDotnet build -c Release --no-restore
        & $hostDotnet "bin/Release/net$release.0/$($entry[1]).dll"
      } finally { Pop-Location }
    }
  }
} finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
Write-Host "PASS six consumers reconstructed only from installed Markdown at $output; no remote feeds, unchanged PATH scopes. Proxy independence is not network isolation."
