[CmdletBinding()]
param(
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/renderer-navigation'),
  [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$owned = @()
$retainHosts = $false
try {
  if (Get-NetTCPConnection -LocalPort 6230,6231,6240,6241 -State Listen -ErrorAction SilentlyContinue) { throw 'A sample port is occupied; no existing process was stopped.' }
  foreach ($major in 10,11) {
    foreach ($name in 'Planetarium','MineralArchive') {
      $directory = Join-Path $root "samples/net$major.0/$name"
      Push-Location -LiteralPath $directory
      try { & $Dotnet build "$name.csproj" -c Release -p:RestoreLockedMode=true }
      finally { Pop-Location }
      $port = $(if ($name -eq 'Planetarium') { 6220 } else { 6230 }) + $major
      $child = Start-Process -FilePath $Dotnet -WorkingDirectory $directory -WindowStyle Hidden -PassThru `
        -ArgumentList "run --no-build --configuration Release --urls http://127.0.0.1:$port" `
        -Environment @{ DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0' } `
        -RedirectStandardOutput (Join-Path $OutputDirectory "$name-$major.stdout.log") `
        -RedirectStandardError (Join-Path $OutputDirectory "$name-$major.stderr.log")
      $owned += @{ pid=$child.Id; name=$name; major=$major; uri="http://127.0.0.1:$port/" }
    }
  }
  foreach ($hostRecord in $owned) {
    $ready = $false
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    do {
      try { $html = (Invoke-WebRequest $hostRecord.uri -TimeoutSec 2).Content; $ready = $true }
      catch { Start-Sleep -Milliseconds 100 }
    } while (-not $ready -and [DateTime]::UtcNow -lt $deadline)
    if (-not $ready -or $html -notmatch 'blazor.webassembly.js') { throw "Missing WASM host: $($hostRecord.uri)" }
  }
  $owned | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'hosts.json')
  $retainHosts = $KeepRunning.IsPresent
  Write-Host 'PASS: four pinned WASM builds and real HTTP boot pages. Run the separate browser checks for behavior.'
}
finally {
  if (-not $retainHosts) {
    foreach ($hostRecord in $owned) {
      # dotnet run owns a DevServer child; stop only this helper's process tree.
      & "$env:SystemRoot/System32/taskkill.exe" /PID $hostRecord.pid /T /F 2>$null | Out-Null
    }
  }
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
