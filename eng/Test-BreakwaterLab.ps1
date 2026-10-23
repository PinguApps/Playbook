[CmdletBinding()]
param(
  [string]$Samples = (Join-Path (Split-Path $PSScriptRoot -Parent) 'samples'),
  [string]$Results = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin801-803/canonical'),
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [int]$StartPort = 5991,
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
  foreach ($major in @(10,11)) {
    $port = $StartPort + $major - 10
    if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) { throw "Port $port is occupied; no process was stopped." }
    $directory = Join-Path $Samples "net$major.0/BreakwaterLab"
    Push-Location $directory
    try { & $Dotnet build BreakwaterLab.csproj -c Release -p:RestoreLockedMode=true }
    finally { Pop-Location }
    $dll = Join-Path $directory "bin/Release/net$major.0/BreakwaterLab.dll"
    $child = Start-Process -FilePath $Dotnet -WorkingDirectory $directory -WindowStyle Hidden -PassThru `
      -ArgumentList "`"$dll`" --urls http://127.0.0.1:$port" `
      -Environment @{ DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; ASPNETCORE_ENVIRONMENT='Development'; DOTNET_ENVIRONMENT='Development' } `
      -RedirectStandardOutput (Join-Path $Results "$major.stdout.log") -RedirectStandardError (Join-Path $Results "$major.stderr.log")
    $owned += @{ pid=$child.Id; version=$major; directory=$directory; port=$port; executable=$Dotnet; dll=$dll }
    $uri = "http://127.0.0.1:$port/"
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
      try { $html = (Invoke-WebRequest $uri -TimeoutSec 2).Content; $ready = $true } catch { $ready = $false; Start-Sleep -Milliseconds 100 }
    } while (-not $ready -and [DateTime]::UtcNow -lt $deadline)
    if (-not $ready) { throw "Host $uri failed to start." }
    $assetPath = [regex]::Match($html,'<script src="([^"]+)"').Groups[1].Value
    $asset = (Invoke-WebRequest ($uri + $assetPath)).RawContentStream.ToArray()
    $owned[-1].assetPath = $assetPath
    $owned[-1].assetSha256 = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($asset))
    $owned[-1].dllSha256 = (Get-FileHash $dll).Hash.ToLowerInvariant()
    $owned[-1].runtimeModules = @((Get-Process -Id $child.Id).Modules | Where-Object ModuleName -In @('coreclr.dll','hostfxr.dll') | Select-Object ModuleName,FileName)
  }
  @{ kind='actual-built-hosts-and-served-assets'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); hosts=$owned; scope='HTTP and artifact inspection only. Separate browser receipts establish injection, interop and MathML behavior.' } | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $Results 'hosts.evidence.json')
  if ($KeepRunning) { $owned | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $Results 'processes.json') }
  Write-Host 'PASS: both matching real hosts and framework assets. Browser checks remain separate.'
}
catch { foreach ($child in $owned) { Stop-Process -Id $child.pid -ErrorAction SilentlyContinue }; throw }
finally {
  if (-not $KeepRunning) { foreach ($child in $owned) { Stop-Process -Id $child.pid -ErrorAction SilentlyContinue } }
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings were preserved." } }
}
