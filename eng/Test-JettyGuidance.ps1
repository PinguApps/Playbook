[CmdletBinding()]
param(
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Output = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin809-810/execution.json')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in @('Process','User','Machine')) { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$records = @()
try {
  foreach ($major in @(10,11)) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($app in @('JettyAccess','MooringClock')) {
      $directory = Join-Path $root "samples/net$major.0/$app"
      Push-Location -LiteralPath $directory
      try {
        $build = (& $dotnet build "$app.csproj" -c Release -p:RestoreLockedMode=true | Out-String)
        $dll = Join-Path $directory "bin/Release/net$major.0/$app.dll"
        $result = (& $dotnet $dll | Out-String)
        if (-not $result.Contains('PASS:')) { throw "Missing behavioral result: $app/net$major.0" }
        $records += @{ sample="samples/net$major.0/$app/$app.csproj"; sdk=(& $dotnet --version | Out-String).Trim(); build=$build; output=$result; assemblySha256=(Get-FileHash $dll).Hash.ToLowerInvariant(); evidenceSha256=(Get-FileHash './verification-evidence.json').Hash.ToLowerInvariant() }
        Write-Host $result
      } finally { Pop-Location }
    }
  }
  New-Item -ItemType Directory -Path (Split-Path $Output) -Force | Out-Null
  @{ kind='real-loopback-and-component-rendering-and-Identity-execution'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); records=$records; scope='Real Kestrel HTTP/WebSocket hub calls and HtmlRenderer output. Test-only authentication and single-user memory persistence; no browser, database, cookies, token expiry, timers, concurrency, external accounts or GA claim.' } | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $Output
  Write-Host 'PASS: four matching-SDK consumers; authorization and actual Identity clock checks.'
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
