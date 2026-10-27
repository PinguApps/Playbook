[CmdletBinding()]
param([string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$work = Join-Path ([IO.Path]::GetTempPath()) ('playbook-web-tooling-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
$report = @()
function New-Child([string]$Directory, [string[]]$Arguments) {
  $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_NOLOGO'] = '1'
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $work 'cli-home'
  $start.Environment['APPDATA'] = Join-Path $work 'appdata'
  $start.Environment['ASPNETCORE_ENVIRONMENT'] = 'Development'
  $process = [Diagnostics.Process]::new()
  $process.StartInfo = $start
  $null = $process.Start()
  [pscustomobject]@{ Process = $process; Out = $process.StandardOutput.ReadToEndAsync(); Err = $process.StandardError.ReadToEndAsync() }
}
function Invoke-Child([string]$Directory, [string[]]$Arguments) {
  $child = New-Child $Directory $Arguments
  try {
    if (-not $child.Process.WaitForExit(120000)) { $child.Process.Kill($true); throw 'SDK command timed out.' }
    $out = $child.Out.GetAwaiter().GetResult()
    $err = $child.Err.GetAwaiter().GetResult()
    if ($child.Process.ExitCode -ne 0) { throw "SDK command failed: $out $err" }
    return $out + $err
  } finally { $child.Process.Dispose() }
}
function Get-Port {
  $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
  $listener.Start()
  try { return $listener.LocalEndpoint.Port } finally { $listener.Stop() }
}
function Wait-Http($Child, [string]$Url) {
  for ($attempt = 0; $attempt -lt 100; $attempt++) {
    if ($Child.Process.HasExited) { throw "Server exited: $($Child.Out.Result) $($Child.Err.Result)" }
    try { $null = Invoke-WebRequest -Uri $Url -SkipHttpErrorCheck; return } catch { Start-Sleep -Milliseconds 200 }
  }
  throw 'Server did not become ready.'
}
function Invoke-Mcp([string]$Url, [string]$Method, $Parameters, [int]$Id) {
  $body = @{ jsonrpc = '2.0'; id = $Id; method = $Method; params = $Parameters } | ConvertTo-Json -Depth 30 -Compress
  $response = Invoke-WebRequest -Uri $Url -Method Post -ContentType 'application/json' -Headers @{ Accept = 'application/json, text/event-stream' } -Body $body
  $json = [string]$response.Content
  if ($response.Headers['Content-Type'] -match 'text/event-stream') {
    $json = @($json -split "\r?\n" | Where-Object { $_.StartsWith('data: ') } | ForEach-Object { $_.Substring(6) })[-1]
  }
  $rpc = $json | ConvertFrom-Json
  if ($rpc.error -or $rpc.result.isError) { throw "MCP failed: $json" }
  return $rpc.result
}
try {
  foreach ($version in @(10, 11)) {
    $lane = Join-Path $work "net$version"
    New-Item -ItemType Directory -Path $lane | Out-Null
    $source = Join-Path $root "samples/net$version.0/QuayTokens"
    Copy-Item -LiteralPath (Join-Path $source 'global.json') -Destination $lane
    foreach ($file in @('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props')) { Copy-Item -LiteralPath (Join-Path $source $file) -Destination $lane }
    $app = Join-Path $lane 'tokens'
    New-Item -ItemType Directory -Path $app | Out-Null
    Get-ChildItem -LiteralPath $source -File | Where-Object { $_.Extension -in @('.cs', '.csproj') } | Copy-Item -Destination $app
    $selector = if ($version -eq 11) { @('--file', 'quay.cs') } else { @('--project', 'QuayTokens.csproj') }
    $port = Get-Port
    $url = "http://127.0.0.1:$port"
    '{}' | Set-Content -LiteralPath (Join-Path $app 'appsettings.Development.json')
    $tokenOutput = Invoke-Child $app (@('user-jwts', 'create') + $selector + @('--name', 'HarbourKeeper', '--audience', $url, '--valid-for', '5m', '--appsettings-file', 'appsettings.Development.json', '--output', 'token'))
    $token = @($tokenOutput -split "\r?\n" | Where-Object { $_ -match '^[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+$' })[-1]
    if ($token -notmatch '^[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+$') { throw 'No development JWT received. Check the development appsettings file and user-secrets setup.' }
    if (-not (Get-ChildItem -LiteralPath (Join-Path $work 'appdata') -Filter secrets.json -Recurse -ErrorAction SilentlyContinue)) { throw 'Signing key was not saved in the isolated home.' }
    $target = if ($version -eq 11) { 'quay.cs' } else { 'QuayTokens.csproj' }
    $null = Invoke-Child $app @('build', $target, '-c', 'Release')
    $child = New-Child $app (@('run', '--no-build', '-c', 'Release') + $selector + @('--', '--urls', $url))
    try {
      Wait-Http $child "$url/permit"
      $denied = Invoke-WebRequest "$url/permit" -SkipHttpErrorCheck
      $allowed = Invoke-WebRequest "$url/permit" -Headers @{ Authorization = "Bearer $token" } -SkipHttpErrorCheck
      $invalidToken = $token.Substring(0, $token.LastIndexOf('.')) + '.bad'
      $invalid = Invoke-WebRequest "$url/permit" -Headers @{ Authorization = "Bearer $invalidToken" } -SkipHttpErrorCheck
      if ($denied.StatusCode -ne 401 -or $invalid.StatusCode -ne 401 -or $allowed.StatusCode -ne 200 -or $allowed.Content -ne 'HarbourKeeper may inspect W3.') { throw "net$version JWT request assertions failed: absent=$($denied.StatusCode), invalid=$($invalid.StatusCode), signed=$($allowed.StatusCode)." }
      $report += [ordered]@{ target = "net$version.0"; jwtSelector = $selector -join ' '; absent = 401; invalid = 401; signed = 200 }
    } finally {
      if (-not $child.Process.HasExited) { $child.Process.Kill($true); $child.Process.WaitForExit() }
      $child.Process.Dispose()
    }

    $template = Join-Path $lane 'template'
    $null = Invoke-Child $lane @('new', 'mcpserver', '--transport', 'remote', '--self-contained', 'false', '--no-restore', '--no-update-check', '--name', 'BerthTools', '--output', $template)
    $generatedProject = Join-Path $template 'BerthTools.csproj'
    [xml]$xml = Get-Content -LiteralPath $generatedProject -Raw
    if ($xml.Project.PropertyGroup.TargetFramework -ne "net$version.0" -or $xml.Project.ItemGroup.PackageReference.Version -ne '2.1.0') { throw 'Bundled template dependency baseline changed.' }
    # Keep the generated project and transport; replace the demonstration with the original tool.
    $canonical = Join-Path $root "samples/net$version.0/BerthTools"
    Copy-Item -LiteralPath (Join-Path $canonical 'Program.cs') -Destination $template
    Copy-Item -LiteralPath (Join-Path $canonical 'BerthLookup.cs') -Destination $template
    # The generated explicit package pin is deliberately tested outside repository CPM.
    '<Project />' | Set-Content -LiteralPath (Join-Path $template 'Directory.Packages.props')
    $null = Invoke-Child $template @('build', '-c', 'Release')
    $url = "http://127.0.0.1:$(Get-Port)"
    $child = New-Child $template @('run', '--no-build', '-c', 'Release', '--no-launch-profile', '--', '--urls', $url)
    try {
      Wait-Http $child $url
      $init = Invoke-Mcp $url 'initialize' @{ protocolVersion = '2025-11-25'; capabilities = @{}; clientInfo = @{ name = 'Playbook template consumer'; version = '1.0' } } 1
      $null = Invoke-WebRequest -Uri $url -Method Post -ContentType 'application/json' -Headers @{ Accept = 'application/json, text/event-stream'; 'MCP-Protocol-Version' = $init.protocolVersion } -Body '{"jsonrpc":"2.0","method":"notifications/initialized"}'
      $tools = Invoke-Mcp $url 'tools/list' @{} 2
      $tool = @($tools.tools | Where-Object { $_.name -eq 'load_limit' })
      if ($tool.Count -ne 1) { throw 'Original tool not discovered.' }
      $result = Invoke-Mcp $url 'tools/call' @{ name = $tool[0].name; arguments = @{ berth = 'W3' } } 3
      if ($result.content[0].text -ne 'W3 permits 18 tonnes.') { throw 'Incorrect generated-server tool result.' }
      $unknown = Invoke-Mcp $url 'tools/call' @{ name = $tool[0].name; arguments = @{ berth = 'X9' } } 4
      if ($unknown.content[0].text -ne 'No load limit is registered for that berth.') { throw 'Incorrect unknown-berth result.' }
      $report += [ordered]@{ target = "net$version.0"; bundledTemplate = 'mcpserver'; package = 'ModelContextProtocol.AspNetCore/2.1.0'; transport = 'Streamable HTTP/stateless'; result = $result.content[0].text }
    } finally {
      if (-not $child.Process.HasExited) { $child.Process.Kill($true); $child.Process.WaitForExit() }
      $child.Process.Dispose()
    }
  }
  $output = Join-Path $root 'artifacts/web-tooling.json'
  New-Item -ItemType Directory -Force -Path (Split-Path $output) | Out-Null
  $report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $output
  Write-Host 'PASS actual .NET 10/11 JWT requests and SDK-bundled remote MCP generation/initialize/list/call.'
} finally {
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings." }
  }
  # The unique temporary home holds development signing keys. No shared user secrets were modified.
  $resolved = [IO.Path]::GetFullPath($work)
  if (-not $resolved.StartsWith([IO.Path]::GetTempPath(), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe cleanup path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
