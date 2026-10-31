[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
)

$ErrorActionPreference = 'Stop'
$route = Join-Path $PSScriptRoot 'Cosmos.ps1'
$paths = @{}
foreach ($scope in 'Process', 'User', 'Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$temporary = Join-Path ([IO.Path]::GetTempPath()) ('playbook-cosmos-provider-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $temporary
try {
  & $route -Action Start -Docker $Docker
  & $route -Action Test -Docker $Docker
  foreach ($baseline in @(
    @{ Target = 'net10.0'; Sdk = '10.0.402'; Provider = '10.0.13'; Dotnet = $Net10Dotnet },
    @{ Target = 'net11.0'; Sdk = '11.0.100-rc.2.26504.105'; Provider = '11.0.0-rc.2.26504.105'; Dotnet = $Net11Dotnet }
  )) {
    $directory = Join-Path $temporary $baseline.Target
    $null = New-Item -ItemType Directory -Path $directory
    @{ sdk = @{ version = $baseline.Sdk; rollForward = 'disable'; allowPrerelease = $true } } |
      ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $directory 'global.json')
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>$($baseline.Target)</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><RestorePackagesWithLockFile>true</RestorePackagesWithLockFile></PropertyGroup>
  <ItemGroup><PackageReference Include="Microsoft.EntityFrameworkCore.Cosmos" Version="[$($baseline.Provider)]" /><PackageReference Include="Microsoft.Azure.Cosmos" Version="[3.63.1]" /><PackageReference Include="Newtonsoft.Json" Version="[13.0.4]" /></ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $directory 'Probe.csproj')
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'cosmos/ProviderProbe.cs') -Destination (Join-Path $directory 'Program.cs')
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'cosmos/EmulatorTransport.cs') -Destination $directory
    $name = 'playbook_provider_' + [guid]::NewGuid().ToString('N')
    $created = $false
    try {
      & $route -Action New -Database $name -Docker $Docker
      $created = $true
      $refused = $false
      try { & $route -Action Stop -Docker $Docker } catch { if ($_.Exception.Message -notlike '*Databases remain*') { throw }; $refused = $true }
      if (-not $refused) { throw 'Stop did not protect the active test database.' }
      Write-Host 'PASS: Stop refuses while a database remains.'
      & $route -Action Run -Database $name -Docker $Docker -Executable $baseline.Dotnet -WorkingDirectory $directory `
        -CommandArguments @('run', '--project', (Join-Path $directory 'Probe.csproj'), '--configuration', 'Release', '--no-launch-profile')
    }
    finally { if ($created) { & $route -Action Remove -Database $name -Docker $Docker } }
  }
}
finally {
  if ((Split-Path $temporary -Parent) -cne [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetTempPath())) { throw 'Unexpected temporary directory.' }
  Remove-Item -LiteralPath $temporary -Recurse -Force
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings." }
  }
}
