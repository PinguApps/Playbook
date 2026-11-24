[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Artifacts,
  [Parameter(Mandatory)][string]$PackageVersion,
  [Parameter(Mandatory)][string]$Results,
  [string]$ExpectedRid,
  [string]$InstallerFeed,
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$plan = & (Join-Path $PSScriptRoot 'Assert-PublishPackage.ps1') -Artifacts $Artifacts -PackageVersion $PackageVersion -BuildVersion -PassThru
$rid = [Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
if ($ExpectedRid -and $rid -cne $ExpectedRid) { throw "Expected a native $ExpectedRid smoke host, got $rid." }
$package = $plan | Where-Object Name -CEQ "PinguApps.Playbook.$rid.$PackageVersion.nupkg"
if (-not $package) { throw "No bundled package for smoke host $rid." }
$runtimes = @(& $Dotnet --list-runtimes)
if (-not $runtimes -or @($runtimes | Where-Object { $_ -match '^Microsoft.NETCore.App 11\.' }).Count) {
  throw "Expected an installer with a shared runtime inventory excluding .NET 11: $runtimes"
}

$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) {
  $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope)
}
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-bundled-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$feed = [Security.SecurityElement]::Escape((Resolve-Path -LiteralPath $Artifacts).Path)
$sources = "<add key='playbook' value='$feed'/>"
if ($InstallerFeed) {
  $installerSource = [Security.SecurityElement]::Escape((Resolve-Path -LiteralPath $InstallerFeed).Path)
  $sources += "<add key='installer' value='$installerSource'/>"
}
"<configuration><packageSources><clear/>$sources</packageSources></configuration>" |
  Set-Content -LiteralPath (Join-Path $scratch 'NuGet.Config')

function Start-Child([string]$File, [string[]]$Arguments, [switch]$Mcp) {
  $start = [Diagnostics.ProcessStartInfo]::new($File)
  $start.WorkingDirectory = $scratch
  $start.UseShellExecute = $false
  $start.RedirectStandardInput = $Mcp
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  if ($IsWindows -and $File.EndsWith('.cmd', [StringComparison]::OrdinalIgnoreCase)) {
    $start.FileName = Join-Path ([Environment]::SystemDirectory) 'cmd.exe'
    foreach ($argument in @('/d', '/c', '.\tool\playbook.cmd')) { $start.ArgumentList.Add($argument) }
  }
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  foreach ($key in @($start.Environment.Keys)) {
    if ($key -like 'DOTNET_ROOT*' -or $key -eq 'DOTNET_ROLL_FORWARD') { $null = $start.Environment.Remove($key) }
  }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_GENERATE_ASPNET_CERTIFICATE'] = 'false'
  $start.Environment['DOTNET_NOLOGO'] = '1'
  $start.Environment['DOTNET_CLI_HOME'] = $scratch
  $start.Environment['NUGET_PACKAGES'] = Join-Path $scratch 'cache'
  if ($Mcp) {
    $start.Environment['DOTNET_HOST_TRACE'] = '1'
    $start.Environment['DOTNET_HOST_TRACEFILE'] = Join-Path $scratch 'host.log'
  }
  [Diagnostics.Process]::Start($start)
}

function Invoke-Child([string]$File, [string[]]$Arguments) {
  $child = Start-Child $File $Arguments
  try {
    $stdout = $child.StandardOutput.ReadToEndAsync()
    $stderr = $child.StandardError.ReadToEndAsync()
    if (-not $child.WaitForExit(60000)) { throw "Timed out: $File $Arguments" }
    if ($child.ExitCode -ne 0) { throw "$File exited $($child.ExitCode): $($stdout.Result) $($stderr.Result)" }
    $stdout.Result.Trim()
  }
  finally {
    if (-not $child.HasExited) { $child.Kill($true) }
    $child.Dispose()
  }
}

function Invoke-Mcp([string]$Method, [hashtable]$Params, [int]$Id) {
  $request = @{ jsonrpc = '2.0'; method = $Method; params = $Params }
  if ($Id) { $request.id = $Id }
  $server.StandardInput.WriteLine(($request | ConvertTo-Json -Depth 10 -Compress))
  $server.StandardInput.Flush()
  if (-not $Id) { return }
  $reply = $server.StandardOutput.ReadLineAsync().WaitAsync([TimeSpan]::FromSeconds(20)).GetAwaiter().GetResult() | ConvertFrom-Json
  if ($reply.id -ne $Id -or $reply.error -or $reply.result.isError) { throw "MCP request failed: $Method" }
  $reply.result
}

$server = $null
try {
  $tool = Join-Path $scratch 'tool'
  Invoke-Child $Dotnet @('tool', 'install', 'PinguApps.Playbook', '--version', $PackageVersion,
    '--tool-path', $tool, '--configfile', (Join-Path $scratch 'NuGet.Config'), '--no-http-cache') | Write-Host
  $command = Join-Path $tool $(if ($IsWindows) { 'playbook.exe' } else { 'playbook' })
  if ($IsWindows -and -not (Test-Path -LiteralPath $command)) { $command = Join-Path $tool 'playbook.cmd' }
  $version = Invoke-Child $command @('--version')
  if ($version.Split('+')[0] -cne $PackageVersion) { throw "Unexpected tool version: $version" }

  $server = Start-Child $command @('mcp') -Mcp
  $diagnostics = $server.StandardError.ReadToEndAsync()
  $initialized = Invoke-Mcp 'initialize' @{
    protocolVersion = '2025-11-25'; capabilities = @{}; clientInfo = @{ name = 'Playbook bundled smoke'; version = '1.0' }
  } 1
  if ($null -eq $initialized.capabilities.tools) { throw 'MCP initialization did not advertise tools.' }
  Invoke-Mcp 'notifications/initialized' @{}
  $tools = (Invoke-Mcp 'tools/list' @{} 2).tools
  if ((($tools.name | Sort-Object) -join ',') -cne 'find_guidance,get_recipe,list_recipes') { throw 'Unexpected MCP tool list.' }
  $list = (Invoke-Mcp 'tools/call' @{
    name = 'list_recipes'; arguments = @{ targetVersion = '10.0'; query = 'field'; limit = 1 }
  } 3).structuredContent
  $detail = (Invoke-Mcp 'tools/call' @{
    name = 'get_recipe'; arguments = @{ targetVersion = '10.0'; id = 'csharp.validate-property-assignment' }
  } 4).structuredContent.recipe
  if ($list.recipes[0].id -ne $detail.id -or -not $list.recipes[0].goal -or -not $detail.markdown -or -not $detail.applicability) {
    throw 'Expected recipe discovery, full Markdown and applicability.'
  }
  $server.StandardInput.Close()
  if (-not $server.WaitForExit(15000) -or $server.ExitCode -ne 0) { throw 'MCP server did not shut down cleanly.' }
  $coreClr = Get-Content -LiteralPath (Join-Path $scratch 'host.log') | Where-Object { $_ -like 'CoreCLR path =*' }
  if (@($coreClr).Count -ne 1 -or $coreClr -notlike "CoreCLR path = '*pinguapps.playbook.$rid*") {
    throw "Expected CoreCLR from the $rid payload: $coreClr"
  }
  $null = New-Item -ItemType Directory -Path $Results -Force
  @{
    package = $package.Name; sha256 = $package.Hash; rid = $rid; version = $version
    installerRuntimes = $runtimes; coreClr = $coreClr; shutdownExitCode = $server.ExitCode
  } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Results 'bundled-runtime.json')
  Write-Host "PASS: installed $version for $rid and completed MCP on the bundled runtime."
}
finally {
  if ($server) {
    if (-not $server.HasExited) { $server.Kill($true); $server.WaitForExit() }
    $server.Dispose()
  }
  $expectedPath = [IO.Path]::GetFullPath($scratch)
  if ((Resolve-Path -LiteralPath $scratch).Path -cne $expectedPath -or
      (Split-Path $expectedPath -Leaf) -notlike 'playbook-bundled-*') { throw 'Unexpected smoke cleanup path.' }
  Remove-Item -LiteralPath $expectedPath -Recurse -Force
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) {
      throw "Unexpected $scope PATH change during smoke validation."
    }
  }
}
