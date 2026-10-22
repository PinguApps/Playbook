[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'dotnet',
  [string]$Net11Dotnet = 'dotnet'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($release in @('10', '11')) {
    $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in @('PackageTimestamps', 'NuGetClientHost')) {
      Push-Location (Join-Path $root "samples/net$release.0/$name")
      try {
        & $dotnet build "$name.csproj" -c Release -p:RestoreLockedMode=true
        # Timestamp checks launch the same CLI for pack; use the DLL, not an apphost.
        & $dotnet "./bin/Release/net$release.0/$name.dll"
      }
      finally { Pop-Location }
    }
  }
  Push-Location (Join-Path $root 'samples/net10.0/NuGetClientCurrent')
  try { & $Net10Dotnet run --project NuGetClientCurrent.csproj -c Release -p:RestoreLockedMode=true }
  finally { Pop-Location }
  foreach ($case in @(
    @{ Path = 'net10.0/NuGetClientHost'; Target = 'net10.0'; Version = '7.9.0'; Asset = 'net8.0' },
    @{ Path = 'net11.0/NuGetClientHost'; Target = 'net11.0'; Version = '8.0.0-rc.605'; Asset = 'net10.0' },
    @{ Path = 'net10.0/NuGetClientCurrent'; Target = 'net10.0'; Version = '8.0.0-rc.605'; Asset = 'net10.0' }
  )) {
    $assets = Get-Content (Join-Path $root "samples/$($case.Path)/obj/project.assets.json") -Raw | ConvertFrom-Json -AsHashtable
    $selected = $assets.targets[$case.Target]["NuGet.Packaging/$($case.Version)"]
    foreach ($kind in @('compile', 'runtime')) {
      if (@($selected[$kind].Keys) -cnotcontains "lib/$($case.Asset)/NuGet.Packaging.dll") { throw "Wrong $kind asset for $($case.Path)" }
    }
    Write-Host "PASS $($case.Path): compile/runtime lib/$($case.Asset)/NuGet.Packaging.dll"
  }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings." }
  }
}
