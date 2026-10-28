[CmdletBinding()]
param(
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/blazor-migrations'),
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
try {
  if (Get-NetTCPConnection -LocalPort 6210,6211,6220,6221 -State Listen -ErrorAction SilentlyContinue) { throw 'A sample port is occupied; no existing process was stopped.' }
  foreach ($major in 10,11) {
    foreach ($name in 'EstuaryLogs','GreenhouseValves') {
      $directory = Join-Path $root "samples/net$major.0/$name"
      Push-Location -LiteralPath $directory
      try { & $Dotnet build "$name.csproj" -c Release -p:RestoreLockedMode=true }
      finally { Pop-Location }
      $port = $(if ($name -eq 'EstuaryLogs') { 6200 } else { 6210 }) + $major
      $dll = Join-Path $directory "bin/Release/net$major.0/$name.dll"
      $child = Start-Process -FilePath $Dotnet -WorkingDirectory $directory -WindowStyle Hidden -PassThru `
        -ArgumentList "`"$dll`" --urls http://127.0.0.1:$port" `
        -Environment @{ DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; ASPNETCORE_ENVIRONMENT='Development'; DOTNET_ENVIRONMENT='Development' } `
        -RedirectStandardOutput (Join-Path $OutputDirectory "$name-$major.stdout.log") `
        -RedirectStandardError (Join-Path $OutputDirectory "$name-$major.stderr.log")
      $owned += @{ pid=$child.Id; name=$name; major=$major; uri="http://127.0.0.1:$port/"; dllSha256=(Get-FileHash -LiteralPath $dll).Hash.ToLowerInvariant() }
    }
  }
  foreach ($hostRecord in $owned) {
    $ready = $false
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
      try { $html = (Invoke-WebRequest $hostRecord.uri -TimeoutSec 2).Content; $ready = $true }
      catch { Start-Sleep -Milliseconds 100 }
    } while (-not $ready -and [DateTime]::UtcNow -lt $deadline)
    if (-not $ready -or $html -notmatch 'Interactive: False') { throw "Missing prerendered sample: $($hostRecord.uri)" }
  }
  [IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'small.log'), [Text.Encoding]::UTF8.GetBytes("station=reed-bank; salinity=14.2`n"))
  [IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'large.log'), [byte[]]::new(1048577))
  [IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'cancel.log'), [byte[]]::new(1048576))
  $owned | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'hosts.json')
  Write-Host 'PASS: four pinned builds and real HTTP prerendering. Browser transfer/callback checks remain separate.'
}
catch { foreach ($hostRecord in $owned) { Stop-Process -Id $hostRecord.pid -ErrorAction SilentlyContinue }; throw }
finally {
  if (-not $KeepRunning) { foreach ($hostRecord in $owned) { Stop-Process -Id $hostRecord.pid -ErrorAction SilentlyContinue } }
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
