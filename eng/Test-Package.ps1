[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Artifacts,
  [Parameter(Mandatory)][string]$Suite,
  [Parameter(Mandatory)][string]$PackageVersion,
  [ValidateSet(11)][int]$RuntimeMajor = 11,
  [string]$FixtureDirectory = (Join-Path $Artifacts 'release-fixture'),
  [switch]$Fallback,
  [switch]$Smoke,
  [string]$Results = (Join-Path $Artifacts 'TestResults')
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$Artifacts = (Resolve-Path $Artifacts).Path
$Suite = (Resolve-Path $Suite).Path
$package = Join-Path $Artifacts "PinguApps.Playbook.$PackageVersion.nupkg"
$plan = & (Join-Path $PSScriptRoot 'Assert-PublishPackage.ps1') -Artifacts $Artifacts -PackageVersion $PackageVersion -BuildVersion -PassThru
$dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
$runtimes = @(& $dotnet --list-runtimes)
$runtimes | Write-Host
$core = @($runtimes | Where-Object { $_ -match '^Microsoft.NETCore.App ' })
if (-not $core -or @($core | Where-Object { $_ -notmatch "^Microsoft.NETCore.App $RuntimeMajor\." }).Count) {
  throw "Expected an isolated .NET $RuntimeMajor-only host."
}
if ($env:DOTNET_ROLL_FORWARD) { throw 'Do not override the packaged roll-forward policy.' }

New-Item -ItemType Directory -Path $Results -Force | Out-Null
$Results = (Resolve-Path $Results).Path
$previous = @{}
foreach ($key in @('PLAYBOOK_TEST_PACKAGE', 'PLAYBOOK_TEST_VERSION', 'PLAYBOOK_TEST_RELEASE_FIXTURE', 'PLAYBOOK_TEST_RUNTIME_MAJOR', 'PLAYBOOK_TEST_EVIDENCE_DIRECTORY', 'PLAYBOOK_TEST_REQUIRE_FALLBACK')) {
  $previous[$key] = [Environment]::GetEnvironmentVariable($key)
}
try {
  $env:PLAYBOOK_TEST_PACKAGE = $package
  $env:PLAYBOOK_TEST_VERSION = $PackageVersion
  $env:PLAYBOOK_TEST_RELEASE_FIXTURE = Join-Path $FixtureDirectory "PinguApps.Playbook.$PackageVersion.nupkg"
  $env:PLAYBOOK_TEST_RUNTIME_MAJOR = "$RuntimeMajor"
  $env:PLAYBOOK_TEST_REQUIRE_FALLBACK = if ($Fallback) { '1' } else { $null }
  $env:PLAYBOOK_TEST_EVIDENCE_DIRECTORY = Join-Path $Results 'runtime'
  $arguments = @('exec', '--roll-forward', 'Major', (Join-Path $Suite 'PinguApps.Playbook.Tests.dll'), '-noLogo', '-failSkips', '-result-trx', (Join-Path $Results 'playbook.trx'))
  if ($Smoke -or $Fallback) { $arguments += @('-trait', 'Category=HostSmoke') }
  & $dotnet @arguments
  [xml]$trx = Get-Content (Join-Path $Results 'playbook.trx')
  $counts = $trx.TestRun.ResultSummary.Counters
  $expectedCount = if ($Smoke -or $Fallback) { 1 } else { 51 }
  if ([int]$counts.total -ne $expectedCount -or [int]$counts.passed -ne $expectedCount -or [int]$counts.failed -ne 0) {
    throw "Expected $expectedCount passing installed scenarios; received total $($counts.total), passed $($counts.passed), failed $($counts.failed)."
  }
  Write-Host "Verified the $($plan.Count)-package set; $($counts.total) scenarios passed."
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
}
