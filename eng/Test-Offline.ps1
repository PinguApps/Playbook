[CmdletBinding()]
param(
  [string]$PackageVersion = '1.0.0-preview.1',
  [string]$Image = 'mcr.microsoft.com/dotnet/sdk@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $root 'artifacts'
$suite = Join-Path $root 'tests/PinguApps.Playbook.Tests/bin/Release/net10.0'
if (-not (Test-Path (Join-Path $artifacts "PinguApps.Playbook.$PackageVersion.nupkg")) -or
    -not (Test-Path (Join-Path $suite 'PinguApps.Playbook.Tests.dll'))) { throw 'Run eng/Verify.ps1 first.' }

# No repository mount and no network: the consumer receives only the actual package and test binaries.
# Canonical snippet text is embedded in the test assembly, independently of the installed tool.
docker run --rm --network none `
  --mount "type=bind,source=$artifacts,target=/artifacts,readonly" `
  --mount "type=bind,source=$suite,target=/suite,readonly" `
  -e "PLAYBOOK_TEST_PACKAGE=/artifacts/PinguApps.Playbook.$PackageVersion.nupkg" `
  -e "PLAYBOOK_TEST_VERSION=$PackageVersion" `
  -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 `
  $Image dotnet /suite/PinguApps.Playbook.Tests.dll
