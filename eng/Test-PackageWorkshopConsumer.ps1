[CmdletBinding()]
param(
  [string]$ResponseDirectory = (Join-Path $PSScriptRoot '../artifacts/batch03/inspection'),
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('gannet-returned-' + [guid]::NewGuid().ToString('N'))
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','MSBUILDUSESERVER','MSBUILDDISABLENODEREUSE') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:MSBUILDUSESERVER = '0'
  $env:MSBUILDDISABLENODEREUSE = '1'
  New-Item -ItemType Directory -Path $fixture -Force | Out-Null
  $written = @{}
  foreach ($id in 'nuget.reset-reused-host-state','nuget.own-plugin-launch-diagnostics') {
    $recipe = (Get-Content -LiteralPath (Join-Path $ResponseDirectory "$id.json") -Raw | ConvertFrom-Json).result.structuredContent.recipe
    $metadata = Get-Content -LiteralPath (Join-Path $root "content/recipes/$id/11.0/recipe.json") -Raw | ConvertFrom-Json
    $blocks = @([regex]::Matches($recipe.markdown, '(?ms)^```[^\n]*\n(.*?)^```') | ForEach-Object { $_.Groups[1].Value.TrimEnd() })
    foreach ($item in $metadata.snippets.PSObject.Properties) {
      $snippet = $item.Value
      $canonical = (Get-Content -LiteralPath (Join-Path $root $snippet.path) -Raw).Replace("`r`n","`n")
      $region = [regex]::Escape($snippet.region)
      $expected = [regex]::Match($canonical, "(?ms)^[ \t]*(?://|<!--) snippet:${region}(?: -->)?\n(.*?)^[ \t]*(?://|<!--) endsnippet:${region}(?: -->)?$").Groups[1].Value.TrimEnd()
      $returned = @($blocks | Where-Object { $_ -ceq $expected })
      if (-not $returned.Count) { throw "Canonical source missing from actual returned Markdown: $($snippet.path)" }
      if ($written.ContainsKey($snippet.path)) { continue }
      $content = $returned[0]
      if ($snippet.path.EndsWith('/GannetWorkshop.csproj')) { $content = '<Project Sdk="Microsoft.NET.Sdk">' + "`n" + $content + "`n</Project>" }
      $destination = Join-Path $fixture $snippet.path
      New-Item -ItemType Directory -Path (Split-Path $destination) -Force | Out-Null
      [IO.File]::WriteAllText($destination,$content + "`n")
      $written[$snippet.path] = $true
    }
  }
  foreach ($major in 10,11) {
    $directory = Join-Path $fixture "samples/net$major.0/GannetWorkshop"
    $version = if ($major -eq 10) { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    [IO.File]::WriteAllText((Join-Path $directory 'global.json'), (@{sdk=@{version=$version;rollForward='disable';allowPrerelease=($major -eq 11)}} | ConvertTo-Json))
    $props = if ($major -eq 11) { 'Shared/Directory.Build.props' } else { '../../net11.0/GannetWorkshop/Shared/Directory.Build.props' }
    [IO.File]::WriteAllText((Join-Path $directory 'Directory.Build.props'), '<Project><Import Project="' + $props + '" /></Project>')
    [IO.File]::WriteAllText((Join-Path $directory 'Directory.Packages.props'), '<Project />')
    [IO.File]::WriteAllText((Join-Path $directory 'NuGet.Config'), '<configuration><packageSources><clear/></packageSources></configuration>')
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    Push-Location -LiteralPath $directory
    try {
      & $dotnet build GannetWorkshop.csproj -c Release -p:UseSharedCompilation=false
      & $dotnet "./bin/Release/net$major.0/GannetWorkshop.dll"
    }
    finally { Pop-Location }
  }
  Write-Host 'PASS actual installed-returned Markdown reconstructs complete default net10/net11 consumers; real same-process builds/configuration and benign child execution. Full analyzer/negative/lifecycle suite is separately captured by Record-PackageWorkshop.'
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  $resolved = [IO.Path]::GetFullPath($fixture)
  $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
  if (-not $resolved.StartsWith($temporaryRoot,[StringComparison]::OrdinalIgnoreCase) -or
      [IO.Path]::GetFileName($resolved) -notmatch '^gannet-returned-[0-9a-f]{32}$') { throw 'Refusing cleanup outside this owned temporary consumer.' }
  if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
  foreach ($scope in $before.Keys) {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
