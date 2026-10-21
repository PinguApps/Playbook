[CmdletBinding()]
param(
  [string]$Net9Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-async-linq-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$previousNativePreference = $PSNativeCommandUseErrorActionPreference
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($case in @('older', 'ambiguous', 'excluded')) {
    $directory = Join-Path $scratch $case
    New-Item -ItemType Directory -Path $directory | Out-Null
    $version = if ($case -eq 'older') { '9' } else { '10' }
    $dotnet = if ($case -eq 'older') { $Net9Dotnet } else { $Net10Dotnet }
    Copy-Item -LiteralPath (Join-Path $root "samples/net$version.0/AsyncStream/global.json") -Destination $directory
    Copy-Item -LiteralPath (Join-Path $root 'NuGet.Config') -Destination $directory
    @'
<Project><PropertyGroup><ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally><CentralPackageVersionOverrideEnabled>false</CentralPackageVersionOverrideEnabled></PropertyGroup><ItemGroup><PackageVersion Include="System.Linq.Async" Version="6.0.1" /></ItemGroup></Project>
'@ | Set-Content -LiteralPath (Join-Path $directory 'Directory.Packages.props')
    $exclude = if ($case -eq 'excluded') { ' ExcludeAssets="compile"' } else { '' }
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net$version.0</TargetFramework><RestorePackagesWithLockFile>true</RestorePackagesWithLockFile><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>
  <ItemGroup><PackageReference Include="System.Linq.Async"$exclude /></ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $directory 'Probe.csproj')
    $select = if ($case -eq 'excluded') { 'Select' } else { 'SelectAwaitWithCancellation' }
    @"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
class Program
{
    static async Task Main()
    {
        int total = 0;
        await foreach (int value in Read().Where(x => x > 0).$select((int x, CancellationToken token) => ValueTask.FromResult(x * 3))) { total += value; }
        if (total != 36) { throw new InvalidOperationException("Unexpected legacy migration results."); }
        Console.WriteLine("PASS legacy migration ${case}: 36; " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
    }
    static async IAsyncEnumerable<int> Read() { await Task.Yield(); yield return 4; yield return 8; }
}
"@ | Set-Content -LiteralPath (Join-Path $directory 'Program.cs')
    Push-Location $directory
    try {
      $PSNativeCommandUseErrorActionPreference = $true
      & $dotnet restore Probe.csproj --packages (Join-Path $scratch 'packages') | Out-Host
      $PSNativeCommandUseErrorActionPreference = $false
      $output = (& $dotnet build Probe.csproj --configuration Release -p:RestoreLockedMode=true 2>&1 | Out-String)
      $exitCode = $LASTEXITCODE
      if ($case -eq 'ambiguous') {
        if ($exitCode -eq 0 -or $output -notmatch 'CS0121' -or $output -notmatch 'Where') { throw "Expected real old/framework Where ambiguity: $output" }
        Write-Host 'PASS actual net10.0 + System.Linq.Async 6.0.1 rejects ambiguous Where with CS0121.'
      }
      else {
        if ($exitCode -ne 0) { throw $output }
        $PSNativeCommandUseErrorActionPreference = $true
        & $dotnet "./bin/Release/net$version.0/Probe.dll"
      }
      $assets = Get-Content -LiteralPath 'obj/project.assets.json' -Raw | ConvertFrom-Json -AsHashtable
      Write-Host "Resolved $case dependencies: $(@($assets.libraries.Keys | Sort-Object) -join ', ')"
      Write-Host "Lock SHA256: $((Get-FileHash -LiteralPath 'packages.lock.json' -Algorithm SHA256).Hash)"
    }
    finally { Pop-Location }
  }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  $PSNativeCommandUseErrorActionPreference = $previousNativePreference
  $resolved = [IO.Path]::GetFullPath($scratch)
  $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
  if (-not $resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or
      -not ([IO.Path]::GetFileName($resolved)).StartsWith('playbook-async-linq-', [StringComparison]::Ordinal)) {
    throw 'Refusing cleanup outside the dedicated temporary probe directory.'
  }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
