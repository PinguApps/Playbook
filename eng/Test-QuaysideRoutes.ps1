[CmdletBinding()]
param(
  [string]$Samples = (Join-Path (Split-Path $PSScriptRoot -Parent) 'samples'),
  [string]$Results = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin796/hosts'),
  [switch]$KeepRunning
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$Samples = [IO.Path]::GetFullPath($Samples)
$Results = [IO.Path]::GetFullPath($Results)
New-Item -ItemType Directory -Path $Results -Force | Out-Null
$owned = [Collections.Generic.List[object]]::new()
$before = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'

function Start-Owned([string]$Exe, [string]$Directory, [string]$Arguments, [string]$Label) {
  $child = Start-Process -FilePath $Exe -WorkingDirectory $Directory -ArgumentList $Arguments -WindowStyle Hidden -PassThru `
    -Environment @{ DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'; ASPNETCORE_ENVIRONMENT = 'Development'; DOTNET_ENVIRONMENT = 'Development' } `
    -RedirectStandardOutput (Join-Path $Results "$Label.stdout.log") -RedirectStandardError (Join-Path $Results "$Label.stderr.log")
  $owned.Add(@{ pid = $child.Id; label = $Label; executable = $Exe; directory = $Directory; arguments = $Arguments })
}

try {
  if (@(Get-NetTCPConnection -State Listen -LocalPort 5911,5912,5913,5914 -ErrorAction SilentlyContinue).Count) { throw 'A Quayside port is occupied; no process was stopped.' }
  foreach ($major in @(10,11)) {
    $dotnet = if ($major -eq 10) { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
    $base = Join-Path $Samples "net$major.0/QuaysideRoutes"
    $entry = if ($major -eq 10) { 'Host' } else { 'Client' }
    Push-Location $base
    try {
      & $dotnet build "$entry/QuaysideRoutes.$entry.csproj" -c Release
      & $dotnet build 'Backend/QuaysideRoutes.Backend.csproj' -c Release
    }
    finally { Pop-Location }
    $backendPort = if ($major -eq 10) { 5913 } else { 5914 }
    Start-Owned $dotnet (Join-Path $base 'Backend') "`"$base/Backend/bin/Release/net$major.0/QuaysideRoutes.Backend.dll`" --urls http://127.0.0.1:$backendPort" "backend$major"
    if ($major -eq 10) {
      Start-Owned $dotnet (Join-Path $base 'Host') "`"$base/Host/bin/Release/net10.0/QuaysideRoutes.Host.dll`" --urls http://localhost:5911" 'host10'
    }
    else {
      Push-Location (Join-Path $base 'Client')
      try { $properties = (& $dotnet msbuild QuaysideRoutes.Client.csproj -p:Configuration=Release -getProperty:_BlazorGatewayDll,_GatewayStaticWebAssetsManifest,_GatewayEndpointsManifest | ConvertFrom-Json).Properties }
      finally { Pop-Location }
      Start-Owned $dotnet (Join-Path $base 'Client') "`"$($properties._BlazorGatewayDll)`" --environment Development --staticWebAssets `"$($properties._GatewayStaticWebAssetsManifest)`" --ClientApps:app:EndpointsManifest `"$($properties._GatewayEndpointsManifest)`" --urls http://localhost:5912" 'gateway11'
    }
  }
  $checks = @()
  foreach ($port in @(5911,5912)) {
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
      try { $ready = (Invoke-WebRequest "http://localhost:$port/berths/north" -TimeoutSec 2).StatusCode -eq 200 } catch { $ready = $false }
      if (-not $ready) { Start-Sleep -Milliseconds 100 }
    } while (-not $ready -and [DateTime]::UtcNow -lt $deadline)
    if (-not $ready) { throw "Entry point $port did not start; inspect $Results." }
    foreach ($path in @('/berths/north','/api/quays/north?cargo=granite','/api/not-found','/absent.js')) {
      $response = Invoke-WebRequest "http://localhost:$port$path" -SkipHttpErrorCheck -TimeoutSec 5
      $expected = if ($path -in @('/api/not-found','/absent.js')) { 404 } else { 200 }
      if ($response.StatusCode -ne $expected) { throw "Unexpected $($response.StatusCode) from $port$path" }
      if ($path.StartsWith('/api/quays/') -and ($response.Content -notmatch '^Quay north; cargo=granite; read=\d+$' -or $response.Headers['X-Quayside-Backend'] -ne 'real-loopback')) { throw 'Proxy did not preserve the real backend path/query/header.' }
      $checks += @{ port = $port; path = $path; status = $response.StatusCode; contentType = [string]$response.Headers['Content-Type']; body = if ($path.StartsWith('/api/quays/')) { $response.Content } else { $null } }
    }
  }
  @{ kind = 'real-http'; recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); checks = $checks; processes = @($owned); scope = 'Actual pinned SDK builds, older ASP.NET host/newer bundled Gateway and loopback backends. Browser execution is separate.' } | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $Results 'http.evidence.json')
  Write-Host 'PASS: both actual entries preserve deep links, backend path/query/header and API/file 404 boundaries.'
  if ($KeepRunning) { $owned | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $Results 'processes.json'); Write-Host "Owned hosts retained for browser checks; process inventory: $Results/processes.json" }
}
catch { foreach ($child in $owned) { Stop-Process -Id $child.pid -ErrorAction SilentlyContinue }; throw }
finally {
  if (-not $KeepRunning) { foreach ($child in $owned) { Stop-Process -Id $child.pid -ErrorAction SilentlyContinue } }
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; shared settings were not restored automatically." } }
}
