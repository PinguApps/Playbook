[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$persistent = @{}
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','MSBUILDUSESERVER') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:MSBUILDUSESERVER = '0'
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $sample = "samples/net$major.0/FoundryPublisher/FoundryPublisher.csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
    $recordPath = Join-Path $root "samples/net$major.0/FoundryPublisher/verification-evidence.json"
    $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
    $local = Join-Path $root "artifacts/pin1124/capture/net$major"
    New-Item -ItemType Directory -Path $local -Force | Out-Null
    Copy-Item -LiteralPath $recordPath -Destination (Join-Path $local 'raw.evidence.json')
    # Keep required behaviour/input records, with private machine paths redacted.
    # Exact raw invocations and binlogs remain disposable in ignored artifacts/.
    foreach ($field in 'command','output','sdkInfo','workloadInfo') {
      foreach ($pair in @(@($root,'<repo>'), @((Split-Path $dotnet),'<dotnet-root>'), @($env:USERPROFILE,'<user-profile>'))) {
        if ($pair[0]) {
          foreach ($spelling in @($pair[0],$pair[0].Replace('\','/'),$pair[0].Replace('/','\'))) {
            $record[$field] = $record[$field].Replace($spelling,$pair[1], [StringComparison]::OrdinalIgnoreCase)
          }
        }
      }
    }
    $record.scope = 'Actual SDK-bound NuGet closure and original SDK-packed library/symbols. Real disposable loopback service-index/multipart/failure checks; SDK10 ordinary publication is not staging. Private paths redacted; <dotnet-root> is the supplied complete SDK installation. No hosted interoperability, public upload or GA proof.'
    [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  foreach ($scope in $persistent.Keys) {
    if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved." }
  }
}
