[CmdletBinding()]
param(
  [string]$PackageVersion = '1.0.0-preview.1',
  [ValidateSet(10, 11)][int]$RuntimeMajor = 10,
  [string]$Image
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $root 'artifacts'
$suite = Join-Path $root 'tests/PinguApps.Playbook.Tests/bin/Release/net10.0'
if (-not $Image) {
  $Image = if ($RuntimeMajor -eq 10) {
    'mcr.microsoft.com/dotnet/sdk@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317'
  }
  else {
    'mcr.microsoft.com/dotnet/sdk@sha256:bfabf51e74c5a7cf2fc4eac852145db86f4867d475257d809518d6a2b0fe71e1'
  }
}
$results = Join-Path $artifacts "TestResults/linux-$RuntimeMajor"
New-Item -ItemType Directory -Path $results -Force | Out-Null
if (-not (Test-Path (Join-Path $artifacts "PinguApps.Playbook.$PackageVersion.nupkg")) -or
    -not (Test-Path (Join-Path $suite 'PinguApps.Playbook.Tests.dll'))) { throw 'Run eng/Verify.ps1 first.' }

# No checkout or network: only packed artifacts, test binaries and the consumer script are readable.
# Canonical snippet text is embedded in the test assembly, independently of the installed tool.
docker run --rm --network none `
  --mount "type=bind,source=$artifacts,target=/artifacts,readonly" `
  --mount "type=bind,source=$suite,target=/suite,readonly" `
  --mount "type=bind,source=$PSScriptRoot/Test-Package.ps1,target=/test/Test-Package.ps1,readonly" `
  --mount "type=bind,source=$results,target=/results" `
  -e DOTNET_ROLL_FORWARD_TO_PRERELEASE=1 `
  -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 `
  $Image pwsh -NoProfile -File /test/Test-Package.ps1 -Artifacts /artifacts -Suite /suite `
    -PackageVersion $PackageVersion -RuntimeMajor $RuntimeMajor -Results /results
