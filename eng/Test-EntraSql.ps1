[CmdletBinding()]
param(
    [string] $Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe',
    [string] $Dotnet = 'C:\Program Files\dotnet\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# This check uses only the separately provisioned PIN-679 container and client.
$secretRoot = Join-Path $env:LOCALAPPDATA 'PinguApps\Playbook\EntraSql'
$certificate = Join-Path $secretRoot 'client.pfx'
if (!(Test-Path -LiteralPath $certificate)) { throw 'Pingu must provision the PIN-679 client certificate; see docs/entra-sql-verification.md.' }
if (![IO.Path]::IsPathFullyQualified($Docker) -or ![IO.Path]::IsPathFullyQualified($Dotnet)) { throw 'Executables must use absolute paths.' }
$container = & $Docker inspect playbook-sql-entra-pin679 --format '{{json .}}' | ConvertFrom-Json
if ($LASTEXITCODE) { throw 'PIN-679 container is unavailable.' }
if ($container.Config.Labels.'com.pinguapps.playbook.owner' -ne 'PIN-679' -or !$container.State.Running) { throw 'Expected running PIN-679 container.' }
$bindings = @($container.NetworkSettings.Ports.'1433/tcp')
if ($bindings.Count -ne 1 -or $bindings[0].HostIp -ne '127.0.0.1') { throw 'Expected exactly one loopback SQL binding.' }
$port = [int]$bindings[0].HostPort
if ($port -lt 1 -or $port -gt 65535) { throw 'Invalid SQL port.' }

$work = Join-Path ([IO.Path]::GetTempPath()) ('playbook-entra-' + [Guid]::NewGuid().ToString('N'))
$beforeUser = [Environment]::GetEnvironmentVariable('PATH', 'User')
$beforeMachine = [Environment]::GetEnvironmentVariable('PATH', 'Machine')
$beforeProcess = [Environment]::GetEnvironmentVariable('PATH', 'Process')
try {
    New-Item -ItemType Directory -Path $work | Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'entra-sql/IdentityProbe.cs') -Destination (Join-Path $work 'Program.cs')
    '{"sdk":{"version":"10.0.402","rollForward":"disable"}}' | Set-Content -LiteralPath (Join-Path $work 'global.json')
    @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><PackageReference Include="Microsoft.Data.SqlClient" Version="[7.1.0]" /><PackageReference Include="Azure.Identity" Version="[1.21.0]" /><PackageReference Include="System.Security.Cryptography.Pkcs" Version="[10.0.13]" /></ItemGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $work 'Probe.csproj')
    $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
    $start.UseShellExecute = $false
    $start.WorkingDirectory = $work
    $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
    foreach ($argument in @('run', '--no-launch-profile', '--project', (Join-Path $work 'Probe.csproj'), '-c', 'Release', '--', $certificate, [string]$port)) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::Start($start)
    $process.WaitForExit()
    if ($process.ExitCode) { throw 'Entra SQL access probe failed.' }
}
finally {
    # Only remove this invocation's unique temporary consumer, never credentials/data.
    $resolvedWork = [IO.Path]::GetFullPath($work)
    $resolvedTemp = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
    if ([IO.Path]::GetDirectoryName($resolvedWork) -ne $resolvedTemp -or [IO.Path]::GetFileName($resolvedWork) -notmatch '^playbook-entra-[a-f0-9]{32}$') { throw 'Unexpected cleanup target.' }
    if (Test-Path -LiteralPath $resolvedWork) { Remove-Item -LiteralPath $resolvedWork -Recurse -Force }
    if ($beforeProcess -cne [Environment]::GetEnvironmentVariable('PATH', 'Process') -or $beforeUser -cne [Environment]::GetEnvironmentVariable('PATH', 'User') -or $beforeMachine -cne [Environment]::GetEnvironmentVariable('PATH', 'Machine')) { throw 'PATH changed during the probe; investigate without overwriting shared settings.' }
}
