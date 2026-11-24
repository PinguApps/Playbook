[CmdletBinding()]
param(
  [ValidatePattern('^\d+\.\d+\.\d+(?:-[a-zA-Z0-9]+(?:[.-][a-zA-Z0-9]+)*)?\z')]
  [string]$PackageVersion = '0.0.0-smoke',
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $root 'artifacts'
$packages = Join-Path $artifacts "packages/$PackageVersion"
$results = Join-Path $artifacts 'TestResults'
$previous = @{}
foreach ($key in @('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH', 'PLAYBOOK_TEST_PACKAGE', 'PLAYBOOK_TEST_VERSION', 'PLAYBOOK_TEST_RUNTIME_MAJOR', 'PLAYBOOK_TEST_EVIDENCE_DIRECTORY')) {
  $previous[$key] = [Environment]::GetEnvironmentVariable($key)
}
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) {
  $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope)
}
Push-Location $root
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & (Join-Path $PSScriptRoot 'Test-DependencyPolicy.ps1')
  & $dotnet build PinguApps.Playbook.slnx --configuration Release "-p:Version=$PackageVersion"
  & $dotnet pack src/PinguApps.Playbook --configuration Release "-p:Version=$PackageVersion" -p:PlaybookBundleRuntime=true --output $packages
  & (Join-Path $PSScriptRoot 'Assert-PublishPackage.ps1') -Artifacts $packages -PackageVersion $PackageVersion -Record -BuildVersion
  $env:PLAYBOOK_TEST_PACKAGE = Join-Path $packages "PinguApps.Playbook.$PackageVersion.nupkg"
  $env:PLAYBOOK_TEST_VERSION = $PackageVersion
  $env:PLAYBOOK_TEST_RUNTIME_MAJOR = '11'
  $env:PLAYBOOK_TEST_EVIDENCE_DIRECTORY = Join-Path $results 'runtime'
  New-Item -ItemType Directory -Path $results -Force | Out-Null
  & $dotnet exec tests/PinguApps.Playbook.Tests/bin/Release/net11.0/PinguApps.Playbook.Tests.dll `
    -noLogo -failSkips -trait 'Category=HostSmoke' -result-trx (Join-Path $results 'smoke.trx')
  [xml]$trx = Get-Content -LiteralPath (Join-Path $results 'smoke.trx')
  $counts = $trx.TestRun.ResultSummary.Counters
  if ([int]$counts.total -ne 1 -or [int]$counts.passed -ne 1 -or [int]$counts.failed -ne 0) {
    throw "Expected exactly one passing MCP smoke scenario; received total $($counts.total), passed $($counts.passed), failed $($counts.failed)."
  }
  Write-Host 'PASS: built, packed and installed the actual tool; MCP initialize, tools/list, list_recipes, get_recipe and clean shutdown.'
}
finally {
  Pop-Location
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) {
      throw "Unexpected $scope PATH change during smoke validation; inspect shared settings before changing them."
    }
  }
}
