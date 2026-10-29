[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
$persistent = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','MSBUILDUSESERVER','MSBUILDDISABLENODEREUSE','GANNET_NET10_DOTNET') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:MSBUILDUSESERVER = '0'
  $env:MSBUILDDISABLENODEREUSE = '1'
  $env:GANNET_NET10_DOTNET = $Net10Dotnet
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample "samples/net$major.0/GannetWorkshop/GannetWorkshop.csproj" -Dotnet $dotnet
    $path = Join-Path $root "samples/net$major.0/GannetWorkshop/verification-evidence.json"
    $raw = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json -AsHashtable
    $output = Join-Path $root "artifacts/batch03/capture/net$major"
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    Copy-Item -LiteralPath $path -Destination (Join-Path $output 'raw.evidence.json')
    foreach ($field in 'command','output','sdkInfo','workloadInfo') {
      foreach ($pair in @(@($root,'<repo>'),@((Split-Path $dotnet),'<dotnet-root>'),@($env:USERPROFILE,'<user-profile>'))) {
        foreach ($spelling in @($pair[0],$pair[0].Replace('\','/'),$pair[0].Replace('/','\'))) {
          $raw[$field] = $raw[$field].Replace($spelling,$pair[1],[StringComparison]::OrdinalIgnoreCase)
        }
      }
      $raw[$field] = [regex]::Replace($raw[$field], 'gannet-workshop-[0-9a-f]{32}', 'gannet-workshop-<session>')
    }
    $raw.scope = 'Actual Windows x64 old/current SDK closures; real same-process MSBuild/cache lifecycle and scoped file, original analyzer pack/locked restore/compiler diagnostic controls and mixed targets, obsolete warning and benign/missing OS child launch. Exact raw commands/locks/compiler receipts remain ignored; private paths redacted. No credential exchange, third-party-host automatic reset, metadata enforcement, other-platform or GA claim.'
    [IO.File]::WriteAllText($path, ($raw | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  foreach ($scope in $persistent.Keys) {
    if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved." }
  }
}
