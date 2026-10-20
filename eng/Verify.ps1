[CmdletBinding()]
param(
  [string]$PackageVersion = '1.0.0-preview.1',
  [string]$Net9Dotnet = 'dotnet',
  [string]$Net11Dotnet = 'dotnet',
  [switch]$ReleaseReady,
  [switch]$SkipInstalledTests,
  [switch]$IsolateNetwork
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $root 'artifacts'
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
Push-Location $root
$previous = @{}
foreach ($key in @('PLAYBOOK_TEST_PACKAGE', 'PLAYBOOK_TEST_VERSION', 'PLAYBOOK_TEST_RELEASE_FIXTURE')) {
  $previous[$key] = [Environment]::GetEnvironmentVariable($key)
}
try {
  & (Join-Path $PSScriptRoot 'Test-Content.ps1')
  & (Join-Path $PSScriptRoot 'Test-Evidence.ps1')
  foreach ($sample in @(
    @{ Path = 'net9.0/PropertyAssignment'; Dotnet = $Net9Dotnet },
    @{ Path = 'net10.0/PropertyAssignment'; Dotnet = 'dotnet' },
    @{ Path = 'net10.0/PropertyAssignmentSdk11'; Dotnet = $Net11Dotnet },
    @{ Path = 'net11.0/PropertyAssignment'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/ClosedOutcomes'; Dotnet = 'dotnet' },
    @{ Path = 'net11.0/ClosedOutcomes'; Dotnet = $Net11Dotnet },
    @{ Path = 'net9.0/ExtensionProperties'; Dotnet = $Net9Dotnet },
    @{ Path = 'net10.0/ExtensionProperties'; Dotnet = 'dotnet' },
    @{ Path = 'net11.0/ExtensionProperties'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/CollectionLike'; Dotnet = 'dotnet' },
    @{ Path = 'net11.0/CollectionLike'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/StaticInterfaceHelpers'; Dotnet = 'dotnet' },
    @{ Path = 'net11.0/StaticInterfaceHelpers'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/EnclosingLoops'; Dotnet = 'dotnet' },
    @{ Path = 'net11.0/EnclosingLoops'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpModels'; Dotnet = 'dotnet' },
    @{ Path = 'net11.0/FSharpModels'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpConstructors'; Dotnet = 'dotnet' },
    @{ Path = 'net11.0/FSharpConstructors'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpDiagnostics'; Dotnet = 'dotnet' },
    @{ Path = 'net11.0/FSharpDiagnostics'; Dotnet = $Net11Dotnet }
  )) {
    Push-Location (Join-Path $root "samples/$($sample.Path)")
    try {
      if ($sample.Path -eq 'net10.0/PropertyAssignmentSdk11') {
        & $sample.Dotnet build PropertyAssignment.csproj --configuration Release
        dotnet ./bin/Release/net10.0/PropertyAssignment.dll
      }
      else {
        $name = $sample.Path.Split('/')[-1]
        $project = $name + $(if ($name.StartsWith('FSharp')) { '.fsproj' } else { '.csproj' })
        & $sample.Dotnet run --project $project --configuration Release
      }
    }
    finally { Pop-Location }
  }
  & (Join-Path $PSScriptRoot 'Test-ClosedOutcomes.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-StaticInterfaceHelpers.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-FSharpGuidance.ps1') -Net11Dotnet $Net11Dotnet
  dotnet build PinguApps.Playbook.slnx --configuration Release -p:Version=$PackageVersion
  & (Join-Path $PSScriptRoot 'Test-Content.ps1') -Pack
  & (Join-Path $PSScriptRoot 'Test-Evidence.ps1') -Pack
  dotnet pack src/PinguApps.Playbook --configuration Release --no-build -p:PackageVersion=$PackageVersion "-p:PlaybookReleaseReady=$ReleaseReady" --output $artifacts
  & (Join-Path $PSScriptRoot 'Pack-ReleaseFixture.ps1') -PackageVersion $PackageVersion
  $env:PLAYBOOK_TEST_PACKAGE = Join-Path $artifacts "PinguApps.Playbook.$PackageVersion.nupkg"
  $env:PLAYBOOK_TEST_VERSION = $PackageVersion
  $env:PLAYBOOK_TEST_RELEASE_FIXTURE = Join-Path $artifacts "release-fixture/PinguApps.Playbook.$PackageVersion.nupkg"
  $testArgs = @('test', 'tests/PinguApps.Playbook.Tests', '--configuration', 'Release', '--no-build', '--no-restore', '--logger', 'trx;LogFileName=playbook.trx', '--results-directory', (Join-Path $artifacts 'TestResults'))
  if ($SkipInstalledTests) {
    Write-Host 'Installed tests deferred to the artifact consumers.'
  }
  elseif ($IsolateNetwork) {
    if (-not $IsLinux) { throw '-IsolateNetwork requires Linux network namespaces (sudo and unshare).' }
    # Installation and every installed child process inherit a namespace with no network interfaces/routes.
    sudo --preserve-env=PLAYBOOK_TEST_PACKAGE,PLAYBOOK_TEST_VERSION,PLAYBOOK_TEST_RELEASE_FIXTURE unshare --net -- dotnet @testArgs
  }
  else {
    dotnet @testArgs
  }
  $hash = Get-FileHash -Algorithm SHA256 -LiteralPath $env:PLAYBOOK_TEST_PACKAGE
  "$($hash.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($env:PLAYBOOK_TEST_PACKAGE))" |
    Set-Content -LiteralPath (Join-Path $artifacts 'package.sha256') -Encoding ascii
  $hash
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
  Pop-Location
}
