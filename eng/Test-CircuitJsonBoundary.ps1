[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-circuit-json-' + [guid]::NewGuid().ToString('N'))
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in 10,11) {
    $directory = Join-Path $scratch "net$major.0"
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $root "samples/net$major.0/CedarPlanetarium/global.json") -Destination $directory
    [IO.File]::WriteAllText((Join-Path $directory 'Probe.csproj'), "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net$major.0</TargetFramework><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup><FrameworkReference Include=`"Microsoft.AspNetCore.App`" /></ItemGroup></Project>")
    [IO.File]::WriteAllText((Join-Path $directory 'Program.cs'), 'new Microsoft.AspNetCore.Components.Server.CircuitOptions().JsonTypeInfoResolvers.Clear();')
    Push-Location -LiteralPath $directory
    try {
      $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
      $expected = if ($major -eq 10) { 'CS1061.*JsonTypeInfoResolvers' } else { 'ASPNETCORE9004' }
      $output = & $dotnet build -c Release 2>&1 | Out-String
      if ($LASTEXITCODE -eq 0 -or $output -notmatch $expected) { throw "Missing $expected rejection.`n$output" }
      Write-Host "PASS: net$major.0 boundary rejected with $expected"
    } finally { Pop-Location }
  }
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
