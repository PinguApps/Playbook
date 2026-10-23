[CmdletBinding()]
param(
  [string]$Samples = (Join-Path (Split-Path $PSScriptRoot -Parent) 'samples'),
  [string]$Results = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin798/hosts'),
  [bool]$AllowOldInline = $false,
  [switch]$SkipBuild,
  [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$Samples = (Resolve-Path -LiteralPath $Samples).Path
New-Item -ItemType Directory -Path $Results -Force | Out-Null
$Results = (Resolve-Path -LiteralPath $Results).Path
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$owned = [Collections.Generic.List[object]]::new()
$completed = $false
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in @('10', '11')) {
    $dotnet = if ($major -eq '10') { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
    $hostDirectory = Join-Path $Samples "net$major.0/BrackenMenu/Host"
    if (-not $SkipBuild) {
      Push-Location -LiteralPath $hostDirectory
      try {
        & $dotnet --version
        & $dotnet build BrackenMenu.Host.csproj -c Release
      }
      finally { Pop-Location }
    }
    foreach ($client in @($false, $true)) {
      $port = 5951 + [int]($major -eq '11') + 2 * [int]$client
      if (Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue) { throw "Port $port is already in use; no existing listener was stopped." }
      $dll = Join-Path $hostDirectory "bin/Release/net$major.0/BrackenMenu.Host.dll"
      $allow = $major -eq '10' -and $AllowOldInline
      $process = Start-Process -FilePath $dotnet -ArgumentList @("`"$dll`"", '--urls', "http://localhost:$port", '--environment', 'Development', "--GlobalClient=$client", "--AllowInlineMenu=$allow") -WorkingDirectory $hostDirectory -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Results "$port.log") -RedirectStandardError (Join-Path $Results "$port.err")
      $owned.Add(@{ pid = $process.Id; executable = $dotnet; startedAt = $process.StartTime.ToUniversalTime().ToString('O'); port = $port; globalClient = $client; allowInlineMenu = $allow; dll = $dll })
    }
  }
  $owned | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $Results 'processes.json')
  $checks = foreach ($hostInfo in $owned) {
    $route = if ($hostInfo.globalClient) { 'client/fern' } else { 'fern' }
    $url = "http://localhost:$($hostInfo.port)/bracken/$route"
    $response = $null
    $until = [DateTimeOffset]::UtcNow.AddSeconds(30)
    while (-not $response) {
      try { $response = Invoke-WebRequest $url } catch { if ([DateTimeOffset]::UtcNow -gt $until) { throw }; Start-Sleep -Milliseconds 100 }
    }
    if ($response.Content -notmatch 'Bracken fern trail' -or $response.Content -notmatch '<base href="/bracken/"') { throw "Wrong route document at $url" }
    $policy = $response.Headers['Content-Security-Policy'] -join ', '
    if (-not $policy -or $response.Headers.ContainsKey('Content-Security-Policy-Report-Only')) { throw "Missing enforcing CSP at $url" }
    if ($policy.Contains("'unsafe-inline'")) { throw 'Unexpected broad inline exception.' }
    $boot = Invoke-WebRequest "http://localhost:$($hostInfo.port)/bracken/_framework/blazor.web.js"
    if ($boot.StatusCode -ne 200 -or $boot.Content.Length -lt 1000) { throw 'Missing actual browser boot asset.' }
    @{ url = $url; status = [int]$response.StatusCode; policy = $policy; bootStatus = [int]$boot.StatusCode }
  }
  @{ kind = 'execution'; scope = 'Real HTTP documents, enforcing headers and framework assets only; separate browser receipts prove navigation and policy enforcement.'; capturedAt = [DateTimeOffset]::UtcNow.ToString('O'); samples = $Samples; checks = @($checks) } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $Results 'http.json')
  Write-Host "PASS: four real documents, CSP headers and boot assets. Browser checks remain separate. Processes: $(Join-Path $Results 'processes.json')"
  $completed = $true
}
finally {
  if (-not $KeepRunning -or -not $completed) {
    foreach ($child in $owned) {
      $process = Get-Process -Id $child.pid -ErrorAction SilentlyContinue
      if ($process -and $process.Path -eq $child.executable -and $process.StartTime.ToUniversalTime().ToString('O') -eq $child.startedAt) { Stop-Process -Id $process.Id }
    }
  }
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
}
