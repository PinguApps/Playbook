[CmdletBinding()]
param(
  [string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
  [switch]$RecordEvidence
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($release in '10','11') {
    foreach ($name in 'ObservatoryLogins','ObservatoryFreshness') {
      $sample = "samples/net$release.0/$name/$name.csproj"
      Push-Location -LiteralPath (Join-Path $root (Split-Path $sample))
      try {
        & $Dotnet restore --locked-mode
        & $Dotnet run --configuration Release --no-restore
        if ($name -eq 'ObservatoryFreshness') {
          & $Dotnet run --configuration Release --no-build -- --observe-builtin
        }
      } finally { Pop-Location }
      if ($RecordEvidence) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $Dotnet }
    }
  }
  Write-Host 'PASS paired real SQLite and signed-token OIDC consumers; explicit guards and built-in observations'
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
  foreach ($scope in 'Process','User','Machine') {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $paths[$scope]) { throw "PATH changed: $scope" }
  }
}
