[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
)

$ErrorActionPreference = 'Stop'
$route = Join-Path $PSScriptRoot 'SqlServer.ps1'
$paths = @{}
foreach ($scope in 'Process', 'User', 'Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$temporary = Join-Path ([IO.Path]::GetTempPath()) ('playbook-sql-provider-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $temporary
$peer = 'playbook_peer_' + [guid]::NewGuid().ToString('N')
$peerCreated = $false
try {
  & $route -Action Start -Docker $Docker
  & $route -Action Test -Docker $Docker
  & $route -Action New -Database $peer -Docker $Docker
  $peerCreated = $true
  foreach ($baseline in @(
    @{ Target = 'net10.0'; Sdk = '10.0.402'; Provider = '10.0.13'; Driver = '6.1.7'; Dotnet = $Net10Dotnet },
    @{ Target = 'net11.0'; Sdk = '11.0.100-rc.2.26504.105'; Provider = '11.0.0-rc.2.26504.105'; Driver = '7.1.0'; Dotnet = $Net11Dotnet }
  )) {
    $directory = Join-Path $temporary $baseline.Target
    $null = New-Item -ItemType Directory -Path $directory
    # Outside the checkout: no inherited build, package or SDK policy.
    @{ sdk = @{ version = $baseline.Sdk; rollForward = 'disable'; allowPrerelease = $true } } |
      ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $directory 'global.json')
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>$($baseline.Target)</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><RestorePackagesWithLockFile>true</RestorePackagesWithLockFile></PropertyGroup>
  <ItemGroup><PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="[$($baseline.Provider)]" /><PackageReference Include="Microsoft.Data.SqlClient" Version="[$($baseline.Driver)]" /><PackageReference Include="System.Security.Cryptography.Pkcs" Version="[10.0.13]" /></ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $directory 'Probe.csproj')
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'sql-server/ProviderProbe.cs') -Destination (Join-Path $directory 'Program.cs')
    $name = 'playbook_provider_' + [guid]::NewGuid().ToString('N')
    & $route -Action New -Database $name -Docker $Docker
    try {
      & $route -Action Run -Database $name -Docker $Docker -Executable $baseline.Dotnet -WorkingDirectory $directory `
        -CommandArguments @('run', '--project', (Join-Path $directory 'Probe.csproj'), '--configuration', 'Release', '--no-launch-profile')
      $rejected = $false
      try { & $route -Action New -Database $name -Docker $Docker }
      catch { if ($_.Exception.Message -notlike '*already exists*') { throw }; $rejected = $true }
      if (-not $rejected) { throw 'Duplicate creation was not rejected.' }
      @"
SET NOCOUNT ON;
IF NOT EXISTS (SELECT 1 FROM dbo.ReefReadings WHERE Id = 1 AND Label = N'coral') THROW 51000, 'Duplicate creation damaged existing data', 1;
IF HAS_PERMS_BY_NAME(NULL, 'SERVER', 'CREATE ANY DATABASE') <> 0 THROW 51000, 'Login has server-wide database creation rights', 1;
BEGIN TRY
    EXEC(N'SELECT COUNT(*) FROM [$peer].sys.tables;');
    THROW 51000, 'Peer database was accessible', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() <> 916 THROW;
END CATCH;
SELECT 'PASS: duplicate creation preserves data and peer database access is denied' AS result;
"@ | Set-Content -LiteralPath (Join-Path $directory 'isolation.sql')
      & $route -Action Query -Database $name -Docker $Docker -SqlFile (Join-Path $directory 'isolation.sql')
      if ($baseline.Target -eq 'net10.0') {
        $rejected = $false
        try { & $route -Action New -Database ($name + "`n") -Docker $Docker }
        catch { if ($_.FullyQualifiedErrorId -notlike 'ParameterArgumentValidationError*') { throw }; $rejected = $true }
        if (-not $rejected) { throw 'Trailing newline name was not rejected by parameter validation.' }
        @'
param([int]$Code)
$connection = [Environment]::GetEnvironmentVariable('PLAYBOOK_SQL_CONNECTION_STRING')
$builder = [System.Data.Common.DbConnectionStringBuilder]::new()
$builder.set_ConnectionString($connection)
[Console]::Out.WriteLine($connection)
[Console]::Error.WriteLine($builder.get_Item('Password'))
exit $Code
'@ | Set-Content -LiteralPath (Join-Path $directory 'diagnostics.ps1')
        foreach ($code in 0, 23) {
          $failed = $false
          try {
            $diagnostics = & $route -Action Run -Database $name -Docker $Docker -Executable (Get-Process -Id $PID).Path `
              -CommandArguments @('-NoProfile', '-File', (Join-Path $directory 'diagnostics.ps1'), '-Code', "$code")
          }
          catch { $failed = $true; $diagnostics = $_.Exception.Message }
          if ($failed -ne ($code -ne 0) -or $diagnostics -match 'Pb![A-F0-9]{48}|Server=127\.0\.0\.1' -or
              ([regex]::Matches($diagnostics, '\[redacted\]').Count -ne 2)) {
            throw "Child diagnostic check failed: expected exit $code; threw=$failed; redaction markers=$([regex]::Matches($diagnostics, '\[redacted\]').Count); length=$($diagnostics.Length)."
          }
        }
        Write-Host 'PASS: newline names rejected; successful/failed child diagnostics redact connection string and password.'
      }
    }
    finally { & $route -Action Remove -Database $name -Docker $Docker }
  }
}
finally {
  if ($peerCreated) { & $route -Action Remove -Database $peer -Docker $Docker }
  # This exact GUID directory is owned by this run; never remove a caller-supplied path.
  if ((Split-Path $temporary -Parent) -cne [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetTempPath())) { throw 'Unexpected temporary directory.' }
  Remove-Item -LiteralPath $temporary -Recurse -Force
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings." }
  }
}
