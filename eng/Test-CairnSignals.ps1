[CmdletBinding()]
param(
  [string]$Samples = (Join-Path (Split-Path $PSScriptRoot -Parent) 'samples'),
  [string]$Results = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin799/canonical'),
  [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$Samples = [IO.Path]::GetFullPath($Samples)
$Results = [IO.Path]::GetFullPath($Results)
New-Item -ItemType Directory -Path $Results -Force | Out-Null
$owned = @()
$before = @{}
foreach ($scope in @('Process','User','Machine')) { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
try {
  if (Get-NetTCPConnection -LocalPort 5971,5972 -State Listen -ErrorAction SilentlyContinue) { throw 'A CairnSignals port is occupied; no process was stopped.' }
  foreach ($major in @(10,11)) {
    $dotnet = if ($major -eq 10) { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
    $directory = Join-Path $Samples "net$major.0/CairnSignals"
    Push-Location $directory
    try { & $dotnet build CairnSignals.csproj -c Release }
    finally { Pop-Location }
    $port = 5961 + $major
    $child = Start-Process -FilePath $dotnet -WorkingDirectory $directory -WindowStyle Hidden -PassThru `
      -ArgumentList "`"$directory/bin/Release/net$major.0/CairnSignals.dll`" --urls http://127.0.0.1:$port" `
      -Environment @{ DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; ASPNETCORE_ENVIRONMENT='Development'; DOTNET_ENVIRONMENT='Development' } `
      -RedirectStandardOutput (Join-Path $Results "$major.stdout.log") -RedirectStandardError (Join-Path $Results "$major.stderr.log")
    $owned += @{ pid=$child.Id; version=$major; directory=$directory; port=$port; executable=$dotnet }
  }
  $checks = @()
  foreach ($hostRecord in $owned) {
    $uri = "http://127.0.0.1:$($hostRecord.port)/"
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
      try { $html = (Invoke-WebRequest $uri -TimeoutSec 2).Content; $ready = $true } catch { $ready = $false; Start-Sleep -Milliseconds 100 }
    } while (-not $ready -and [DateTime]::UtcNow -lt $deadline)
    if (-not $ready) { throw "Host $uri failed to start." }
    $assetPath = [regex]::Match($html,'<script src="([^"]+)"').Groups[1].Value
    $asset = (Invoke-WebRequest ($uri + $assetPath)).RawContentStream.ToArray()
    $js = [Text.Encoding]::UTF8.GetString($asset)
    $hasGuard = $js.Contains('cannot have the same name as its browserEventName')
    if ($hasGuard -ne ($hostRecord.version -eq 11)) { throw "Unexpected actual browser guard in $assetPath" }
    $dll = Join-Path $hostRecord.directory "bin/Release/net$($hostRecord.version).0/CairnSignals.dll"
    $checks += @{ version=$hostRecord.version; uri=$uri; assetPath=$assetPath; assetBytes=$asset.Length; assetSha256=[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($asset)); sameNameGuard=$hasGuard; dllSha256=(Get-FileHash $dll).Hash.ToLowerInvariant() }
  }
  @{kind='real-http-and-artifact-inspection'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); checks=$checks; verifierSha256=(Get-FileHash $PSCommandPath).Hash.ToLowerInvariant(); scope='Actual built hosts and served JS bytes. Separate browser records establish registration failure and native beforeinput component delivery.'} | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $Results 'http.evidence.json')
  if ($KeepRunning) { $owned | ConvertTo-Json | Set-Content (Join-Path $Results 'processes.json') }
  Write-Host 'PASS: both real hosts serve their matching JS assets with the expected guard difference.'
}
catch { foreach ($child in $owned) { Stop-Process -Id $child.pid -ErrorAction SilentlyContinue }; throw }
finally {
  if (-not $KeepRunning) { foreach ($child in $owned) { Stop-Process -Id $child.pid -ErrorAction SilentlyContinue } }
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings were preserved." } }
}
