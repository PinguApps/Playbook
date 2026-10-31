[CmdletBinding()]
param(
  [Parameter(Mandatory)][ValidateSet('Start', 'Stop', 'New', 'Remove', 'Run', 'Test')][string]$Action,
  [ValidatePattern('\Aplaybook_[a-z][a-z0-9_]{0,47}\z')][string]$Database,
  [string]$Executable,
  [string[]]$CommandArguments = @(),
  [string]$WorkingDirectory,
  [string]$Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not [IO.Path]::IsPathFullyQualified($Docker) -or -not (Test-Path -LiteralPath $Docker -PathType Leaf)) { throw 'Docker must be an existing absolute executable path.' }
if ($Action -in @('New', 'Remove', 'Run') -and -not $Database) { throw 'Supply -Database playbook_<unique_name>.' }
if ($Action -eq 'Run' -and (-not $Executable -or -not [IO.Path]::IsPathFullyQualified($Executable) -or -not (Test-Path -LiteralPath $Executable -PathType Leaf))) { throw 'Run requires an existing absolute -Executable.' }
if ($WorkingDirectory -and (-not [IO.Path]::IsPathFullyQualified($WorkingDirectory) -or -not (Test-Path -LiteralPath $WorkingDirectory -PathType Container))) { throw 'WorkingDirectory must be an existing absolute directory.' }
$container = 'playbook-cosmos-legacy'
$label = 'pin-677-v1'
$image = 'mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator@sha256:9a4f69121f20c1d6502db84625c71ebb441ab61b01880a7b2d3d8031c6dac429'

function Invoke-Process([string]$File, [string[]]$Arguments, [hashtable]$Variables = @{}, [string]$Directory = '') {
  $start = [Diagnostics.ProcessStartInfo]::new($File)
  $start.UseShellExecute = $false
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  if ($Directory) { $start.WorkingDirectory = $Directory }
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  foreach ($name in $Variables.Keys) { $start.Environment[$name] = $Variables[$name] }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $process = [Diagnostics.Process]::Start($start)
  try {
    $output = $process.StandardOutput.ReadToEndAsync()
    $errors = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(600000)) {
      $process.Kill($true)
      $process.WaitForExit()
      throw 'Child process exceeded the ten-minute operation limit and was stopped.'
    }
    $text = $output.GetAwaiter().GetResult() + $errors.GetAwaiter().GetResult()
    $text = $text.Replace([PlaybookCosmos.EmulatorTransport]::Key, '[emulator key redacted]')
    if ($process.ExitCode -ne 0) { throw "Child process failed ($($process.ExitCode)): $text" }
    return $text.Trim()
  }
  finally { $process.Dispose() }
}
function Invoke-Docker([string[]]$Arguments) { Invoke-Process $Docker $Arguments }
function Get-OwnedContainer {
  $info = (Invoke-Docker @('inspect', $container) | ConvertFrom-Json)[0]
  if ($info.Config.Labels.'com.pinguapps.playbook.cosmos' -cne $label) { throw 'Container ownership label mismatch.' }
  $bindings = @($info.HostConfig.PortBindings.'8081/tcp')
  if ($bindings.Count -ne 1 -or $bindings[0].HostIp -cne '127.0.0.1' -or @($info.HostConfig.PortBindings.PSObject.Properties).Count -ne 1) { throw 'Expected only one loopback Cosmos port.' }
  if (@($info.Mounts).Count -ne 0 -or $info.HostConfig.Privileged -or $info.HostConfig.NetworkMode -cne 'bridge') { throw 'Unexpected mounts, privileges or network mode.' }
  $expected = Invoke-Docker @('image', 'inspect', $image, '--format', '{{.Id}}')
  if ($info.Image -cne $expected) { throw 'Container does not use the pinned legacy image.' }
  return $info
}
function Connect-Emulator($Info) {
  $endpoint = "https://127.0.0.1:$($Info.NetworkSettings.Ports.'8081/tcp'[0].HostPort)/"
  $pem = Invoke-Docker @('exec', $container, '/bin/cat', '/tmp/cosmos/appdata/Packages/DataExplorer/emulator.pem')
  $hash = [PlaybookCosmos.EmulatorTransport]::CertificateHash($pem)
  return @{ Client = [PlaybookCosmos.EmulatorTransport]::new($endpoint, $hash); Endpoint = $endpoint; Hash = $hash }
}
function Assert-OwnedDatabase($Client, [string]$Name) {
  $marker = $Client.Request('GET', "/dbs/$Name/colls/playbook_route/docs/route", 'docs', "dbs/$Name/colls/playbook_route/docs/route") | ConvertFrom-Json
  if ($marker.route -cne $label -or $marker.database -cne $Name) { throw 'Database ownership marker mismatch.' }
}
function New-TestDatabase($Client, [string]$Name) {
  # POST must conflict rather than adopt/reset an existing database.
  $null = $Client.Request('POST', '/dbs', 'dbs', '', (@{ id = $Name } | ConvertTo-Json -Compress))
  try {
    $null = $Client.Request('POST', "/dbs/$Name/colls", 'colls', "dbs/$Name", '{"id":"playbook_route","partitionKey":{"paths":["/id"],"kind":"Hash"}}')
    $null = $Client.Request('POST', "/dbs/$Name/colls/playbook_route/docs", 'docs', "dbs/$Name/colls/playbook_route", (@{ id = 'route'; route = $label; database = $Name } | ConvertTo-Json -Compress))
  }
  catch {
    # Only this invocation's successfully created database may be undone.
    $null = $Client.Request('DELETE', "/dbs/$Name", 'dbs', "dbs/$Name")
    throw
  }
  Write-Host "Created owned emulator database: $Name"
}

if (-not ('PlaybookCosmos.EmulatorTransport' -as [type])) { Add-Type -Path (Join-Path $PSScriptRoot 'cosmos/EmulatorTransport.cs') }
$mutex = [Threading.Mutex]::new($false, 'Local\PinguApps.Playbook.Cosmos')
$locked = $false
$connection = $null
try {
  try { $locked = $mutex.WaitOne([TimeSpan]::FromMinutes(10)) } catch [Threading.AbandonedMutexException] { $locked = $true }
  if (-not $locked) { throw 'Another Cosmos route operation is still running.' }
  if ($Action -eq 'Start') {
    $names = Invoke-Docker @('ps', '-a', '--filter', "name=^/$container$", '--format', '{{.Names}}')
    if (-not $names) {
      $null = Invoke-Docker @('pull', $image)
      $null = Invoke-Docker @('run', '-d', '--name', $container, '--label', "com.pinguapps.playbook.cosmos=$label", '--network', 'bridge',
        '--publish', '127.0.0.1::8081', '--memory', '4g', '--env', 'AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3',
        '--env', 'AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true', $image)
    }
    $info = Get-OwnedContainer
    if (-not $info.State.Running) { $null = Invoke-Docker @('start', $container) }
    for ($attempt = 0; $attempt -lt 90; $attempt++) {
      try {
        $info = Get-OwnedContainer
        $connection = Connect-Emulator $info
        $null = $connection.Client.Request('GET', '/dbs', 'dbs', '')
        break
      }
      catch {
        if ($connection) { $connection.Client.Dispose(); $connection = $null }
        if ($attempt -eq 89) { throw 'Legacy emulator did not become ready within the bounded startup check. Inspect its local Docker logs.' }
        Start-Sleep -Seconds 2
      }
    }
    Write-Host "Ready: $($connection.Endpoint); $container; certificate pinned without changing host trust."
  }
  else {
    $info = Get-OwnedContainer
    if ($Action -eq 'Stop') {
      if ($info.State.Running) { $null = Invoke-Docker @('stop', '--time', '30', $container) }
      Write-Host 'Stopped Cosmos; container and its persisted data are retained.'
    }
    else {
      if (-not $info.State.Running) { throw 'Start the Cosmos route first.' }
      $connection = Connect-Emulator $info
      switch ($Action) {
        'New' { New-TestDatabase $connection.Client $Database }
        'Remove' {
          Assert-OwnedDatabase $connection.Client $Database
          $null = $connection.Client.Request('DELETE', "/dbs/$Database", 'dbs', "dbs/$Database")
          Write-Host "Removed owned emulator database: $Database"
        }
        'Run' {
          Assert-OwnedDatabase $connection.Client $Database
          Invoke-Process $Executable $CommandArguments @{
            PLAYBOOK_COSMOS_ENDPOINT = $connection.Endpoint
            PLAYBOOK_COSMOS_KEY = [PlaybookCosmos.EmulatorTransport]::Key
            PLAYBOOK_COSMOS_DATABASE = $Database
            PLAYBOOK_COSMOS_CERT_SHA256 = $connection.Hash
          } $WorkingDirectory
        }
        'Test' {
          $name = 'playbook_probe_' + [guid]::NewGuid().ToString('N')
          $created = $false
          try {
            New-TestDatabase $connection.Client $name
            $created = $true
            Assert-OwnedDatabase $connection.Client $name
            $conflict = $false
            try { New-TestDatabase $connection.Client $name } catch { if ($_.Exception.Message -notlike '*Cosmos HTTP 409:*') { throw }; $conflict = $true }
            if (-not $conflict) { throw 'Duplicate database creation was not refused.' }
            Assert-OwnedDatabase $connection.Client $name
            Write-Host 'PASS: real NoSQL create/read; duplicate creation refuses existing database and preserves its marker.'
            $wrongCertificate = [PlaybookCosmos.EmulatorTransport]::new($connection.Endpoint, ('00' * 32))
            $rejected = $false
            try { $null = $wrongCertificate.Request('GET', '/dbs', 'dbs', '') } catch { $rejected = $true } finally { $wrongCertificate.Dispose() }
            if (-not $rejected) { throw 'A mismatched TLS certificate was accepted.' }
            $null = $connection.Client.Request('PUT', "/dbs/$name/colls/playbook_route/docs/route", 'docs', "dbs/$name/colls/playbook_route/docs/route", '{"id":"route","route":"not-owned"}')
            $rejected = $false
            try { Assert-OwnedDatabase $connection.Client $name } catch { if ($_.Exception.Message -notlike '*Database ownership marker mismatch*') { throw }; $rejected = $true }
            if (-not $rejected) { throw 'A mismatched database ownership marker was accepted.' }
            $null = $connection.Client.Request('GET', "/dbs/$name", 'dbs', "dbs/$name")
            Write-Host 'PASS: mismatched TLS certificate and database marker refused; database remains intact.'
          }
          finally { if ($created) { $null = $connection.Client.Request('DELETE', "/dbs/$name", 'dbs', "dbs/$name") } }
        }
      }
    }
  }
}
finally {
  if ($connection) { $connection.Client.Dispose() }
  if ($locked) { $mutex.ReleaseMutex() }
  $mutex.Dispose()
}
