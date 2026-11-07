[CmdletBinding()]
param(
    [switch]$Record,
    [switch]$CheckEvidence,
    [string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
    [string]$Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
# Pingu approved this isolated table and temporary table-only grants for PIN-958.
$container = 'playbook-sql-entra-pin679'
$database = 'playbook_entra_pin679'
$root = Split-Path $PSScriptRoot -Parent
$evidencePath = Join-Path $root 'docs/verification/entra-sql.json'
$inputs = [ordered]@{}
foreach ($release in 10,11) {
    foreach ($name in 'Program.cs','MarshAccess.csproj','Directory.Build.props','Directory.Build.targets','Directory.Packages.props','global.json','packages.lock.json') {
        $path = "samples/net$release.0/MarshAccess/$name"
        $inputs[$path] = (Get-FileHash -LiteralPath (Join-Path $root $path) -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}
$inputs['eng/Test-MarshEntra.ps1'] = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant()
if ($CheckEvidence) {
    if ($Record) { throw 'Choose either Record or CheckEvidence.' }
    $saved = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json -AsHashtable
    if ((ConvertTo-Json $inputs -Compress) -cne (ConvertTo-Json $saved.inputs -Compress)) { throw 'Entra evidence inputs changed; rerun the authorized real check with -Record.' }
    Write-Host 'PASS saved real-authentication evidence matches both canonical samples and runner.'
    return
}
$private = Join-Path $env:LOCALAPPDATA 'PinguApps\Playbook\EntraSql'
$certificate = Join-Path $private 'client.pfx'
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
foreach ($executable in $Dotnet,$Docker) {
    if (![IO.Path]::IsPathFullyQualified($executable) -or !(Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'Supply absolute existing executable paths.' }
}
if (!(Test-Path -LiteralPath $certificate)) { throw 'PIN-679 client certificate is unavailable.' }
function Invoke-Child([string]$Executable, [string[]]$Arguments, [string]$Directory, [hashtable]$Environment = @{}) {
    $start = [Diagnostics.ProcessStartInfo]::new($Executable)
    $start.UseShellExecute = $false
    $start.WorkingDirectory = $Directory
    $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
    foreach ($entry in $Environment.GetEnumerator()) { $start.Environment[$entry.Key] = $entry.Value }
    foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::Start($start)
    try { $process.WaitForExit(); if ($process.ExitCode) { throw "Child failed (exit $($process.ExitCode))." } }
    finally { $process.Dispose() }
}
$state = & $Docker inspect $container --format '{{json .}}' | ConvertFrom-Json
if ($LASTEXITCODE -or $state.Config.Labels.'com.pinguapps.playbook.owner' -ne 'PIN-679') { throw 'Expected owned PIN-679 container.' }
$wasRunning = $state.State.Running
$created = $false
function Invoke-Sql([string]$Sql) {
    # Password is passed in the child environment, never arguments or output.
    $credentials = Import-Clixml -LiteralPath (Join-Path $private 'credentials.xml')
    $password = [Net.NetworkCredential]::new('', $credentials.Password).Password
    Invoke-Child $Docker @('exec','-e','SQLCMDPASSWORD',$container,'/opt/mssql-tools18/bin/sqlcmd','-S','localhost','-U','sa','-C','-b','-d',$database,'-Q',$Sql) $root @{ SQLCMDPASSWORD = $password }
}
try {
    if (!$wasRunning) {
        Invoke-Child $Docker @('start',$container) $root
        Start-Sleep -Seconds 15
    }
    $bindings = @((& $Docker inspect $container --format '{{json .NetworkSettings.Ports}}' | ConvertFrom-Json).'1433/tcp')
    if ($LASTEXITCODE -or $bindings.Count -ne 1 -or $bindings[0].HostIp -ne '127.0.0.1') { throw 'Expected one loopback SQL binding.' }
    # Refuse any pre-existing table; never replace someone else's data or grants.
    Invoke-Sql @'
IF OBJECT_ID(N'dbo.ObservationCache') IS NOT NULL THROW 51000, 'ObservationCache already exists; refusing setup.', 1;
CREATE TABLE dbo.ObservationCache (
    Id nvarchar(449) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL PRIMARY KEY,
    Value varbinary(max) NOT NULL,
    ExpiresAtTime datetimeoffset NOT NULL,
    SlidingExpirationInSeconds bigint NULL,
    AbsoluteExpiration datetimeoffset NULL
);
'@
    $created = $true
    Invoke-Sql @'
CREATE NONCLUSTERED INDEX IX_ObservationCache_ExpiresAtTime ON dbo.ObservationCache (ExpiresAtTime);
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.ObservationCache TO [Playbook PIN-679 SQL client];
'@
    $connection = "Server=127.0.0.1,$($bindings[0].HostPort);Database=$database;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=True;Connect Timeout=30"
    foreach ($release in 10,11) {
        $directory = Join-Path $root "samples/net$release.0/MarshAccess"
        # Clear ambient secret/federated credentials so Default uses this certificate.
        $environment = @{
            MARSH_SQL_CONNECTION = $connection
            AZURE_TENANT_ID = 'e098693d-4698-42b9-81bb-83881e28a826'
            AZURE_CLIENT_ID = '836d8bc3-2e01-4e20-908b-4fbfeb1569a2'
            AZURE_CLIENT_CERTIFICATE_PATH = $certificate
            AZURE_CLIENT_SECRET = ''; AZURE_CLIENT_CERTIFICATE_PASSWORD = ''
            AZURE_FEDERATED_TOKEN_FILE = ''; AZURE_CLIENT_SEND_CERTIFICATE_CHAIN = 'false'
        }
        Write-Host "Testing original net$release.0 MarshAccess --connect"
        Invoke-Child $Dotnet @('run','--configuration','Release','-p:RestoreLockedMode=true','--','--connect') $directory $environment
    }
    Invoke-Sql "IF EXISTS (SELECT 1 FROM dbo.ObservationCache) THROW 51000, 'Cache rows remained after samples.', 1; SELECT @@VERSION AS Engine;"
    Write-Host 'PASS both original EF/cache samples authenticated using Default with the PIN-679 certificate; no cache rows remain.'
}
finally {
    try {
        if ($created) {
            Invoke-Sql @'
REVOKE SELECT, INSERT, UPDATE, DELETE ON dbo.ObservationCache FROM [Playbook PIN-679 SQL client];
DROP TABLE dbo.ObservationCache;
IF OBJECT_ID(N'dbo.ObservationCache') IS NOT NULL THROW 51000, 'Cleanup failed.', 1;
PRINT 'PASS temporary table and table grants removed.';
'@
        }
    }
    finally {
        if (!$wasRunning) { Invoke-Child $Docker @('stop',$container) $root }
        foreach ($scope in 'Process','User','Machine') {
            if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "PATH changed at $scope scope; investigate without overwriting shared settings." }
        }
    }
}
if ($Record) {
    $graphs = [ordered]@{}
    foreach ($release in 10,11) {
        $assets = Get-Content -LiteralPath (Join-Path $root "samples/net$release.0/MarshAccess/obj/project.assets.json") -Raw | ConvertFrom-Json -AsHashtable
        $graphs["net$release.0"] = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
    }
    $evidence = [ordered]@{
        recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
        scope = 'PIN-958; original net10/net11 MarshAccess --connect; actual EF login and SQL distributed-cache write/read/remove; table empty, temporary grants/table removed, original container running state restored; process/user/machine PATH unchanged.'
        route = 'Driver-managed Active Directory Default -> EnvironmentCredential -> PIN-679 certificate application identity; no injected token, callback, custom provider, browser session or mock.'
        provider = 'SQL Server 2025 RTM-CU10 17.0.5035.5 Enterprise Developer on Linux; isolated playbook_entra_pin679, loopback TLS with local self-signed certificate trusted.'
        limitations = 'Windows x64 only. EF/cache11 RC2 packages on SDK11 RC3/runtime11 RC3; no GA, managed/workload identity, interactive/MFA, migration or production TLS-trust proof. Certificate expires 5 February 2027; rerun after identity, server or relevant artifact changes.'
        inputs = $inputs
        dependencies = $graphs
    }
    $evidence | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath $evidencePath -Encoding utf8NoBOM
    Write-Host 'PASS recorded real-authentication evidence without credentials.'
}
