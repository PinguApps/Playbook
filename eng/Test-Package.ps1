[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Artifacts,
  [Parameter(Mandatory)][string]$Suite,
  [Parameter(Mandatory)][string]$PackageVersion,
  [ValidateSet(10, 11)][int]$RuntimeMajor = 10,
  [switch]$Smoke,
  [string]$Results = (Join-Path $Artifacts 'TestResults')
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$Artifacts = (Resolve-Path $Artifacts).Path
$Suite = (Resolve-Path $Suite).Path
$package = Join-Path $Artifacts "PinguApps.Playbook.$PackageVersion.nupkg"
$expected = (Get-Content (Join-Path $Artifacts 'package.sha256') -Raw).Trim()
$actual = (Get-FileHash -Algorithm SHA256 $package).Hash.ToLowerInvariant()
if ($expected -cne "$actual  $([IO.Path]::GetFileName($package))") { throw 'Package identity/hash mismatch.' }
$runtimes = @(dotnet --list-runtimes)
$runtimes | Write-Host
$core = @($runtimes | Where-Object { $_ -match '^Microsoft.NETCore.App ' })
if (-not $core -or @($core | Where-Object { $_ -notmatch "^Microsoft.NETCore.App $RuntimeMajor\." }).Count) {
  throw "Expected an isolated .NET $RuntimeMajor-only host."
}
if ($env:DOTNET_ROLL_FORWARD) { throw 'Do not override the packaged roll-forward policy.' }

New-Item -ItemType Directory -Path $Results -Force | Out-Null
$Results = (Resolve-Path $Results).Path
$previous = @{}
foreach ($key in @('PLAYBOOK_TEST_PACKAGE', 'PLAYBOOK_TEST_VERSION', 'PLAYBOOK_TEST_RELEASE_FIXTURE', 'PLAYBOOK_TEST_RUNTIME_MAJOR', 'PLAYBOOK_TEST_EVIDENCE_DIRECTORY')) {
  $previous[$key] = [Environment]::GetEnvironmentVariable($key)
}
try {
  $env:PLAYBOOK_TEST_PACKAGE = $package
  $env:PLAYBOOK_TEST_VERSION = $PackageVersion
  $env:PLAYBOOK_TEST_RELEASE_FIXTURE = Join-Path $Artifacts "release-fixture/PinguApps.Playbook.$PackageVersion.nupkg"
  $env:PLAYBOOK_TEST_RUNTIME_MAJOR = "$RuntimeMajor"
  $env:PLAYBOOK_TEST_EVIDENCE_DIRECTORY = Join-Path $Results 'runtime'
  $arguments = @('exec', '--roll-forward', 'Major', (Join-Path $Suite 'PinguApps.Playbook.Tests.dll'), '-noLogo', '-failSkips', '-result-trx', (Join-Path $Results 'playbook.trx'))
  if ($Smoke) { $arguments += @('-trait', 'Category=HostSmoke') }
  dotnet @arguments
  [xml]$trx = Get-Content (Join-Path $Results 'playbook.trx')
  $counts = $trx.TestRun.ResultSummary.Counters
  if ([int]$counts.total -lt 1 -or [int]$counts.failed -ne 0 -or ($Smoke -and [int]$counts.total -ne 1)) {
    throw 'Expected successful installed tests (exactly one for a desktop smoke).'
  }
  Write-Host "Verified $actual on .NET $RuntimeMajor; $($counts.total) scenarios passed."
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
}
