[CmdletBinding()]
param(
  [ValidatePattern('^\d+\.\d+\.\d+(?:-[a-zA-Z0-9]+(?:[.-][a-zA-Z0-9]+)*)?\z')]
  [string]$PackageVersion = '1.0.0-preview.1',
  [ValidateSet(11)][int]$RuntimeMajor = 11,
  [ValidateSet('Suite', 'Bundled', 'Fallback')][string]$Mode = 'Suite',
  [string]$Image
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $root 'artifacts'
$packages = Join-Path $artifacts "packages/$PackageVersion"
$fixtures = Join-Path $artifacts 'release-fixture'
$suite = Join-Path $root 'tests/PinguApps.Playbook.Tests/bin/Release/net11.0'
if (-not $Image) {
  $Image = switch ($Mode) {
    'Bundled' { 'mcr.microsoft.com/dotnet/sdk@sha256:fe0fd4e93bd2fbb47c883b7a6369e5c8d3db8bf01cdc5981f6ee35fbdf3ff4fb' }
    'Fallback' { 'mcr.microsoft.com/dotnet/sdk@sha256:52556c0fb9d0f609ae9aeab9efeeba96f4acc2f7038f7696f2235ccbdbb8e290' }
    'Suite' { 'mcr.microsoft.com/dotnet/sdk@sha256:65f3c7c6a25a239ab3e6719f2896147e7b68242ff2d8818dd70498bde6ef8bea' }
  }
}
$results = Join-Path $artifacts "TestResults/linux-$($Mode.ToLowerInvariant())"
New-Item -ItemType Directory -Path $results -Force | Out-Null
if (-not (Test-Path (Join-Path $packages "PinguApps.Playbook.$PackageVersion.nupkg"))) { throw 'Run eng/Verify.ps1 first.' }

# No checkout or network: only packed artifacts, test binaries and the consumer script are readable.
# Canonical snippet text is embedded in the test assembly, independently of the installed tool.
$docker = (Get-Command docker -CommandType Application | Select-Object -First 1).Source
$common = @('run', '--rm', '--network', 'none',
  '--mount', "type=bind,source=$packages,target=/artifacts,readonly",
  '--mount', "type=bind,source=$PSScriptRoot/Assert-PublishPackage.ps1,target=/test/Assert-PublishPackage.ps1,readonly",
  '--mount', "type=bind,source=$results,target=/results",
  '-e', 'DOTNET_CLI_TELEMETRY_OPTOUT=1', '-e', 'DOTNET_NOLOGO=1', '-e', 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0')
$arguments = @('-Artifacts', '/artifacts', '-PackageVersion', $PackageVersion, '-Results', '/results')
if ($Mode -ceq 'Bundled') {
  & $docker @common --mount "type=bind,source=$PSScriptRoot/Test-BundledTool.ps1,target=/test/Test-BundledTool.ps1,readonly" `
    $Image pwsh -NoProfile -File /test/Test-BundledTool.ps1 @arguments
}
else {
  if (-not (Test-Path (Join-Path $suite 'PinguApps.Playbook.Tests.dll'))) { throw 'Run eng/Verify.ps1 first.' }
  $mounts = @('--mount', "type=bind,source=$suite,target=/suite,readonly",
    '--mount', "type=bind,source=$PSScriptRoot/Test-Package.ps1,target=/test/Test-Package.ps1,readonly")
  if ($Mode -ceq 'Suite') { $mounts += @('--mount', "type=bind,source=$fixtures,target=/fixtures,readonly") }
  else { $arguments += '-Fallback' }
  & $docker @common @mounts $Image pwsh -NoProfile -File /test/Test-Package.ps1 @arguments `
    -Suite /suite -RuntimeMajor $RuntimeMajor -FixtureDirectory /fixtures
}
