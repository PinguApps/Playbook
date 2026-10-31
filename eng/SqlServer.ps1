[CmdletBinding()]
param(
  [Parameter(Mandatory)][ValidateSet('Start', 'Stop', 'New', 'Remove', 'Query', 'Run', 'Test')][string]$Action,
  [ValidatePattern('\Aplaybook_[a-z][a-z0-9_]{0,47}\z')][string]$Database,
  [string]$SqlFile,
  [string]$Executable,
  [string[]]$CommandArguments = @(),
  [string]$WorkingDirectory,
  [string]$Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
)

# Local Windows authoring route only. No arbitrary server/connection-string input.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $IsWindows) { throw 'This route requires Windows PowerShell 7 and the current user DPAPI store.' }
if (-not [IO.Path]::IsPathFullyQualified($Docker) -or -not (Test-Path -LiteralPath $Docker -PathType Leaf)) {
  throw 'Docker must be an existing absolute executable path.'
}
if ($Action -in @('New', 'Remove', 'Query', 'Run') -and -not $Database) { throw 'Supply -Database playbook_<unique_name>.' }
if ($Action -eq 'Query' -and -not (Test-Path -LiteralPath $SqlFile -PathType Leaf)) { throw 'Supply an existing -SqlFile.' }
if ($Action -eq 'Run' -and (-not [IO.Path]::IsPathFullyQualified($Executable) -or -not (Test-Path -LiteralPath $Executable -PathType Leaf))) {
  throw 'Run requires an existing absolute -Executable.'
}
if ($WorkingDirectory -and (-not [IO.Path]::IsPathFullyQualified($WorkingDirectory) -or -not (Test-Path -LiteralPath $WorkingDirectory -PathType Container))) {
  throw 'WorkingDirectory must be an existing absolute directory.'
}
$container = 'playbook-sql-2025'
$volume = 'playbook-sql-2025-data'
$label = 'pin-676-v1'
$image = 'playbook-sql-verification:17.0.5035.5'
$store = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'PinguApps/Playbook/SqlServer'
$adminFile = Join-Path $store 'admin.clixml'

function Invoke-Process([string]$File, [string[]]$Arguments, [string]$InputText = '', [hashtable]$Variables = @{}, [string]$Directory = '', [string[]]$SensitiveValues = @()) {
  $start = [Diagnostics.ProcessStartInfo]::new($File)
  $start.UseShellExecute = $false
  $start.RedirectStandardInput = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  if ($Directory) { $start.WorkingDirectory = $Directory }
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  foreach ($key in $Variables.Keys) { $start.Environment[$key] = $Variables[$key] }
  # Child tools must not integrate global tools or shell profiles on first use.
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $process = [Diagnostics.Process]::Start($start)
  try {
    $output = $process.StandardOutput.ReadToEndAsync()
    $errors = $process.StandardError.ReadToEndAsync()
    $process.StandardInput.Write($InputText)
    $process.StandardInput.Close()
    $process.WaitForExit()
    $text = $output.GetAwaiter().GetResult() + $errors.GetAwaiter().GetResult()
    foreach ($secret in (@($Variables.Values) + $SensitiveValues)) {
      if ($secret) { $text = $text.Replace($secret, '[redacted]') }
    }
    if ($process.ExitCode -ne 0) { throw "Child process failed ($($process.ExitCode)): $text" }
    return $text.Trim()
  }
  finally { $process.Dispose() }
}

function Invoke-Docker([string[]]$Arguments, [string]$InputText = '', [hashtable]$Variables = @{}) {
  Invoke-Process $Docker $Arguments $InputText $Variables
}

function Get-OwnedContainer {
  $info = (Invoke-Docker @('inspect', $container) | ConvertFrom-Json)[0]
  if ($info.Config.Labels.'com.pinguapps.playbook.sql' -cne $label) { throw 'Container ownership label mismatch.' }
  $bindings = @($info.HostConfig.PortBindings.'1433/tcp')
  if ($bindings.Count -ne 1 -or $bindings[0].HostIp -cne '127.0.0.1') { throw 'Expected a single loopback-only SQL port.' }
  $mounts = @($info.Mounts | Where-Object Destination -eq '/var/opt/mssql')
  if ($mounts.Count -ne 1 -or $mounts[0].Name -cne $volume) { throw 'SQL data volume mismatch.' }
  $volumeInfo = (Invoke-Docker @('volume', 'inspect', $volume) | ConvertFrom-Json)[0]
  if ($volumeInfo.Labels.'com.pinguapps.playbook.sql' -cne $label) { throw 'Volume ownership label mismatch.' }
  $expectedImage = Invoke-Docker @('image', 'inspect', $image, '--format', '{{.Id}}')
  if ($info.Image -cne $expectedImage) { throw 'Container does not use the pinned verification image.' }
  return $info
}

function Get-CredentialFile([string]$Name) { Join-Path $store "$Name.clixml" }
function New-Password { 'Pb!' + [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(24)) }
function Read-Credential([string]$Path) {
  if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Missing local encrypted credential file: $Path" }
  $credential = Import-Clixml -LiteralPath $Path
  if ($credential -isnot [pscredential]) { throw 'Invalid credential record.' }
  return $credential
}
function Invoke-Sql([string]$Sql, [string]$Name = 'master', [pscredential]$Credential = (Read-Credential $adminFile)) {
  Invoke-Docker @('exec', '-i', '--env', 'SQLCMDPASSWORD', $container,
    '/opt/mssql-tools18/bin/sqlcmd', '-S', 'localhost', '-U', $Credential.UserName,
    '-d', $Name, '-C', '-I', '-b', '-V', '16', '-r', '1', '-l', '15', '-t', '120', '-x', '-X1') `
    ($Sql + "`nGO`n") @{ SQLCMDPASSWORD = $Credential.GetNetworkCredential().Password }
}
function Assert-OwnedDatabase([string]$Name) {
  $credential = Read-Credential (Get-CredentialFile $Name)
  $expectedLogin = $Name + '_login'
  if ($credential.UserName -cne $expectedLogin) { throw 'Database credential ownership mismatch.' }
  $null = Invoke-Sql "IF ORIGINAL_LOGIN() <> N'$expectedLogin' OR DB_NAME() <> N'$Name' OR IS_SRVROLEMEMBER('sysadmin') <> 0 THROW 51000, 'Unsafe database identity', 1; IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0 AND name = N'PlaybookRoute' AND CONVERT(nvarchar(100), value) = N'$label') THROW 51000, 'Database ownership marker mismatch', 1;" $Name $credential
  return $credential
}
function New-TestDatabase([string]$Name) {
  $file = Get-CredentialFile $Name
  if (Test-Path -LiteralPath $file) { throw 'Database credential already exists; use a fresh database name.' }
  $null = Invoke-Sql "IF DB_ID(N'$Name') IS NOT NULL OR SUSER_ID(N'${Name}_login') IS NOT NULL THROW 51000, 'Database or login already exists', 1;"
  $password = New-Password
  $credential = [pscredential]::new("${Name}_login", (ConvertTo-SecureString $password -AsPlainText -Force))
  # Save the recovery credential before creation; failures never overwrite an existing resource.
  $credential | Export-Clixml -LiteralPath $file
  $createdLogin = $false
  $createdDatabase = $false
  try {
    $null = Invoke-Sql "CREATE LOGIN [${Name}_login] WITH PASSWORD = '$password', CHECK_POLICY = ON;"
    $createdLogin = $true
    $null = Invoke-Sql "CREATE DATABASE [$Name];"
    $createdDatabase = $true
    $null = Invoke-Sql "ALTER AUTHORIZATION ON DATABASE::[$Name] TO [${Name}_login]; ALTER DATABASE [$Name] SET COMPATIBILITY_LEVEL = 170; EXEC sys.sp_addextendedproperty @name=N'PlaybookRoute', @value=N'$label';" $Name
  }
  catch {
    # Only undo resources confirmed created by this invocation, never a pre-existing database.
    if ($createdDatabase) { $null = Invoke-Sql "ALTER DATABASE [$Name] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$Name];" }
    if ($createdLogin) { $null = Invoke-Sql "DROP LOGIN [${Name}_login];" }
    Remove-Item -LiteralPath $file
    throw
  }
  Write-Host "Created $Name with its own database-owner login (no server role)."
}
function Remove-TestDatabase([string]$Name) {
  $null = Assert-OwnedDatabase $Name
  $null = Invoke-Sql "ALTER DATABASE [$Name] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$Name]; DROP LOGIN [${Name}_login];"
  Remove-Item -LiteralPath (Get-CredentialFile $Name)
  Write-Host "Removed owned database and login: $Name"
}

# Serialize lifecycle operations and consumers so Stop cannot interrupt this route's Run/Test.
$mutex = [Threading.Mutex]::new($false, 'Local\PinguApps.Playbook.SqlServer')
$locked = $false
try {
  try { $locked = $mutex.WaitOne([TimeSpan]::FromMinutes(10)) } catch [Threading.AbandonedMutexException] { $locked = $true }
  if (-not $locked) { throw 'Another SQL route operation is still running.' }
  if ($Action -eq 'Start') {
    if (-not (Test-Path -LiteralPath $store)) { $null = New-Item -ItemType Directory -Path $store -Force }
    $acl = [IO.FileSystemAclExtensions]::GetAccessControl([IO.DirectoryInfo]::new($store), [Security.AccessControl.AccessControlSections]::Access)
    $acl.SetAccessRuleProtection($true, $false)
    foreach ($rule in $acl.GetAccessRules($true, $true, [Security.Principal.SecurityIdentifier])) { $acl.RemoveAccessRuleSpecific($rule) }
    $sid = [Security.Principal.WindowsIdentity]::GetCurrent().User
    $acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid, 'FullControl', 'ContainerInherit,ObjectInherit', 'None', 'Allow'))
    [IO.FileSystemAclExtensions]::SetAccessControl([IO.DirectoryInfo]::new($store), $acl)
    $names = @(Invoke-Docker @('ps', '-a', '--filter', "name=^/$container$", '--format', '{{.Names}}'))
    if ($container -notin $names) {
      if (Test-Path -LiteralPath $adminFile) { throw 'Container is missing but credentials remain; ask Pingu before recovering/removing retained data.' }
      $volumes = Invoke-Docker @('volume', 'ls', '--filter', "name=^$volume$", '--format', '{{.Name}}')
      if ($volumes) { throw 'Data volume already exists; ask Pingu before recovery. It will not be overwritten.' }
      $null = Invoke-Docker @('build', '--tag', $image, '--file', (Join-Path $PSScriptRoot 'sql-server/Dockerfile'), (Join-Path $PSScriptRoot 'sql-server'))
      $credential = [pscredential]::new('sa', (ConvertTo-SecureString (New-Password) -AsPlainText -Force))
      $credential | Export-Clixml -LiteralPath $adminFile
      $null = Invoke-Docker @('volume', 'create', '--label', "com.pinguapps.playbook.sql=$label", $volume)
      $null = Invoke-Docker @('run', '-d', '--name', $container, '--label', "com.pinguapps.playbook.sql=$label",
        '--publish', '127.0.0.1::1433', '--volume', "${volume}:/var/opt/mssql", '--memory', '4g',
        '--env', 'ACCEPT_EULA=Y', '--env', 'MSSQL_PID=Developer', '--env', 'MSSQL_SA_PASSWORD', $image) '' `
        @{ MSSQL_SA_PASSWORD = $credential.GetNetworkCredential().Password }
    }
    $info = Get-OwnedContainer
    $null = Read-Credential $adminFile
    if (-not $info.State.Running) { $null = Invoke-Docker @('start', $container) }
    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
      try { $null = Invoke-Sql 'SELECT 1;'; $ready = $true; break } catch { Start-Sleep -Seconds 2 }
    }
    if (-not $ready) { throw 'SQL Server did not become ready; inspect container logs locally without sharing credentials.' }
    $info = Get-OwnedContainer
    Write-Host "Ready: 127.0.0.1,$($info.NetworkSettings.Ports.'1433/tcp'[0].HostPort); $container; Developer edition."
  }
  else {
    $info = Get-OwnedContainer
    if ($Action -eq 'Stop') {
      if ($info.State.Running) { $null = Invoke-Docker @('stop', '--time', '30', $container) }
      Write-Host 'Stopped SQL Server; its container, volume and encrypted credentials are retained.'
    }
    else {
      if (-not $info.State.Running) { throw 'Start the SQL route first.' }
      switch ($Action) {
        'New' { New-TestDatabase $Database }
        'Remove' { Remove-TestDatabase $Database }
        'Query' { Invoke-Sql (Get-Content -LiteralPath $SqlFile -Raw) $Database (Assert-OwnedDatabase $Database) }
        'Run' {
          $credential = Assert-OwnedDatabase $Database
          $port = $info.NetworkSettings.Ports.'1433/tcp'[0].HostPort
          $connection = "Server=127.0.0.1,$port;Database=$Database;User ID=$($credential.UserName);Password=$($credential.GetNetworkCredential().Password);Encrypt=True;TrustServerCertificate=True;Persist Security Info=False"
          Invoke-Process $Executable $CommandArguments '' @{ PLAYBOOK_SQL_CONNECTION_STRING = $connection } $WorkingDirectory @($credential.GetNetworkCredential().Password)
        }
        'Test' {
          $testName = 'playbook_probe_' + [guid]::NewGuid().ToString('N')
          try {
            New-TestDatabase $testName
            Invoke-Sql (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'sql-server/Capabilities.sql') -Raw) $testName (Assert-OwnedDatabase $testName)
          }
          finally {
            if (Test-Path -LiteralPath (Get-CredentialFile $testName)) { Remove-TestDatabase $testName }
          }
        }
      }
    }
  }
}
finally {
  if ($locked) { $mutex.ReleaseMutex() }
  $mutex.Dispose()
}
