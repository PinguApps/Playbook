[CmdletBinding()]
param(
  [string]$PackageVersion = '1.0.0-preview.1',
  [string]$Net9Dotnet = 'dotnet',
  [switch]$IsolateNetwork
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $root 'artifacts'
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
Push-Location $root
$previous = @{}
foreach ($key in @('PLAYBOOK_TEST_PACKAGE', 'PLAYBOOK_TEST_VERSION')) {
  $previous[$key] = [Environment]::GetEnvironmentVariable($key)
}
try {
  foreach ($sample in @(
    @{ Target = 'net9.0'; Dotnet = $Net9Dotnet },
    @{ Target = 'net10.0'; Dotnet = 'dotnet' }
  )) {
    Push-Location (Join-Path $root "samples/$($sample.Target)/PropertyAssignment")
    try { & $sample.Dotnet run --project PropertyAssignment.csproj --configuration Release }
    finally { Pop-Location }
  }
  dotnet build PinguApps.Playbook.slnx --configuration Release -p:Version=$PackageVersion
  dotnet pack src/PinguApps.Playbook --configuration Release --no-build -p:PackageVersion=$PackageVersion --output $artifacts
  $env:PLAYBOOK_TEST_PACKAGE = Join-Path $artifacts "PinguApps.Playbook.$PackageVersion.nupkg"
  $env:PLAYBOOK_TEST_VERSION = $PackageVersion
  $testArgs = @('test', 'tests/PinguApps.Playbook.Tests', '--configuration', 'Release', '--no-build', '--no-restore', '--logger', 'trx;LogFileName=playbook.trx', '--results-directory', (Join-Path $artifacts 'TestResults'))
  if ($IsolateNetwork) {
    if (-not $IsLinux) { throw '-IsolateNetwork requires Linux network namespaces (sudo and unshare).' }
    # Installation and every installed child process inherit a namespace with no network interfaces/routes.
    sudo --preserve-env=PLAYBOOK_TEST_PACKAGE,PLAYBOOK_TEST_VERSION unshare --net -- dotnet @testArgs
  }
  else {
    dotnet @testArgs
  }
  Get-FileHash -Algorithm SHA256 -LiteralPath $env:PLAYBOOK_TEST_PACKAGE
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
  Pop-Location
}
