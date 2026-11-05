[CmdletBinding()]
param(
  [string]$PackageVersion = '1.0.0-preview.1',
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net9Dotnet = $Dotnet,
  [string]$Net11Dotnet = $Dotnet,
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
  & (Join-Path $PSScriptRoot 'Test-DependencyPolicy.ps1')
  & (Join-Path $PSScriptRoot 'Test-Content.ps1') -Dotnet $Dotnet
  & (Join-Path $PSScriptRoot 'Test-Evidence.ps1') -Dotnet $Dotnet
  foreach ($sample in @(
    @{ Path = 'net9.0/MossRequests'; Dotnet = $Net9Dotnet },
    @{ Path = 'net10.0/MossRequests'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/MossRequests'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/MossViews'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/MossViews'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/ArchiveContracts'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/ArchiveContracts'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/SkyLedger'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/SkyLedger'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/BeaconKeys'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/BeaconKeys'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/HttpCapacity'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/HttpCapacity'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/DnsServices'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/DnsServices'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/ConfigurationLifecycle'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/ConfigurationLifecycle'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/OptionsStartup'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/OptionsStartup'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/WeatherImport'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/WeatherImport'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/QuarryMaintenance'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/QuarryMaintenance'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/CacheEntries'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/CacheEntries'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/MetadataInspection'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/MetadataInspection'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/ContentDescriptions'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/ContentDescriptions'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/DeploymentInspection'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/DeploymentInspection'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/BitCollections'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/BitCollections'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/VectorLanes'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/VectorLanes'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/SupportedInstructions'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/SupportedInstructions'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/NumericGuidance'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/NumericGuidance'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FenContracts'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FenContracts'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/Archives'; Dotnet = $Dotnet },
    @{ Path = 'net10.0/Compression'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/Archives'; Dotnet = $Net11Dotnet },
    @{ Path = 'net11.0/Compression'; Dotnet = $Net11Dotnet },
    @{ Path = 'net9.0/Reconciliation'; Dotnet = $Net9Dotnet },
    @{ Path = 'net10.0/Reconciliation'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/Reconciliation'; Dotnet = $Net11Dotnet },
    @{ Path = 'net9.0/AsyncStream'; Dotnet = $Net9Dotnet },
    @{ Path = 'net10.0/AsyncStream'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/AsyncStream'; Dotnet = $Net11Dotnet },
    @{ Path = 'net9.0/JsonImport'; Dotnet = $Net9Dotnet },
    @{ Path = 'net10.0/JsonImport'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/JsonImport'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/JsonStreaming'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/JsonStreaming'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/JsonResolution'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/JsonResolution'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/JsonModels'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/JsonModels'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/JsonContracts'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/JsonContracts'; Dotnet = $Net11Dotnet },
    @{ Path = 'net9.0/PropertyAssignment'; Dotnet = $Net9Dotnet },
    @{ Path = 'net10.0/PropertyAssignment'; Dotnet = $Dotnet },
    @{ Path = 'net10.0/PropertyAssignmentSdk11'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/ValidationRules'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/ValidationRules'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/TracingLogging'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/TracingLogging'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/HttpObservations'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/HttpObservations'; Dotnet = $Net11Dotnet },
    @{ Path = 'net11.0/PropertyAssignment'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/ClosedOutcomes'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/ClosedOutcomes'; Dotnet = $Net11Dotnet },
    @{ Path = 'net9.0/ExtensionProperties'; Dotnet = $Net9Dotnet },
    @{ Path = 'net10.0/ExtensionProperties'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/ExtensionProperties'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/CollectionLike'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/CollectionLike'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/StaticInterfaceHelpers'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/StaticInterfaceHelpers'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/EnclosingLoops'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/EnclosingLoops'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpModels'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpModels'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpConstructors'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpConstructors'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpDiagnostics'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpDiagnostics'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/UnsafeBoundaries'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/UnsafeBoundaries'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpApi'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpApi'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpApiPreview'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpApiPreview'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpAsync'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpAsync'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpRuntime'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpRuntime'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpArrayAot'; Dotnet = $Dotnet },
    @{ Path = 'net10.0/FSharpFsiCleanup'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpFsiCleanup'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpSequence'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpSequence'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/RuntimeAsync'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/RuntimeAsync'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpCompilerHost'; Dotnet = $Dotnet },
    @{ Path = 'net10.0/FSharpCompilerHostCurrent'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpCompilerHost'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpRethrow'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpRethrow'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/FSharpFlags'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/FSharpFlags'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/BinaryData'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/BinaryData'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/BufferAdapters'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/BufferAdapters'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/ProcessExecution'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/ProcessExecution'; Dotnet = $Net11Dotnet },
    @{ Path = 'net10.0/UnicodeText'; Dotnet = $Dotnet },
    @{ Path = 'net11.0/UnicodeText'; Dotnet = $Net11Dotnet }
  )) {
    Push-Location (Join-Path $root "samples/$($sample.Path)")
    try {
      if ($sample.Path -eq 'net10.0/PropertyAssignmentSdk11') {
        & $sample.Dotnet build PropertyAssignment.csproj --configuration Release
        & $Dotnet ./bin/Release/net10.0/PropertyAssignment.dll
      }
      else {
        $name = $sample.Path.Split('/')[-1]
        $project = $name + $(if ($name.StartsWith('FSharp')) { '.fsproj' } else { '.csproj' })
        & $sample.Dotnet run --project $project --configuration Release
      }
    }
    finally { Pop-Location }
  }
  & (Join-Path $PSScriptRoot 'Test-PasskeyTemplate.ps1') -Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-SkyLedger.ps1') -Net11Dotnet $Net11Dotnet -Dotnet $Dotnet
  & (Join-Path $PSScriptRoot 'Test-OptionsStartup.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-NuGetGuidance.ps1') -Net10Dotnet $Dotnet -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-Observability.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-AsyncLinqMigration.ps1') -Net9Dotnet $Net9Dotnet -Net10Dotnet $Dotnet
  & (Join-Path $PSScriptRoot 'Test-SdkWorkflows.ps1') -Net9Dotnet $Net9Dotnet -Net11Dotnet $Net11Dotnet -Net10Dotnet $Dotnet
  & (Join-Path $PSScriptRoot 'Test-SimdGuidance.ps1') -Net10Dotnet $Dotnet -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-JsonContracts.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-ClosedOutcomes.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-StaticInterfaceHelpers.ps1') -Net11Dotnet $Net11Dotnet -Dotnet $Dotnet
  & (Join-Path $PSScriptRoot 'Test-FSharpGuidance.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-FSharpBoundaries.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-RuntimeAsync.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-UnsafeBoundaries.ps1') -Net11Dotnet $Net11Dotnet
  & (Join-Path $PSScriptRoot 'Test-ArchiveFragments.ps1') -Net10Dotnet $Dotnet -Net11Dotnet $Net11Dotnet
  & $Dotnet build PinguApps.Playbook.slnx --configuration Release -p:Version=$PackageVersion
  & (Join-Path $PSScriptRoot 'Test-Content.ps1') -Pack -Dotnet $Dotnet
  & (Join-Path $PSScriptRoot 'Test-Evidence.ps1') -Pack -Dotnet $Dotnet
  & $Dotnet pack src/PinguApps.Playbook --configuration Release --no-build -p:PackageVersion=$PackageVersion "-p:PlaybookReleaseReady=$ReleaseReady" --output $artifacts
  & (Join-Path $PSScriptRoot 'Pack-ReleaseFixture.ps1') -PackageVersion $PackageVersion -Dotnet $Dotnet
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
    sudo --preserve-env=PLAYBOOK_TEST_PACKAGE,PLAYBOOK_TEST_VERSION,PLAYBOOK_TEST_RELEASE_FIXTURE unshare --net -- $Dotnet @testArgs
  }
  else {
    & $Dotnet @testArgs
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
