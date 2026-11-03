[CmdletBinding()]
param([string]$SourceSdk='C:/Users/pingu/.dotnet-playbook-11')
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$taskRoot=Join-Path $root 'artifacts/native-dotnet'
$before=@{};foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$old=@{};foreach($key in 'DOTNET_CLI_HOME','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE'){$old[$key]=[Environment]::GetEnvironmentVariable($key)}
try {
 New-Item -ItemType Directory -Force $taskRoot | Out-Null
 if(Test-Path "$taskRoot/dotnet.exe") {
  if((Get-FileHash "$SourceSdk/dotnet.exe").Hash -cne (Get-FileHash "$taskRoot/dotnet.exe").Hash){throw 'Existing isolated SDK host differs; preserve it and inspect its ownership.'}
 } else {Copy-Item "$SourceSdk/dotnet.exe" $taskRoot}
 foreach($name in 'host','sdk','shared','templates') {
  if(-not(Test-Path "$taskRoot/$name")){New-Item -ItemType Junction -Path "$taskRoot/$name" -Target "$SourceSdk/$name" | Out-Null}
 }
 if(-not(Test-Path "$taskRoot/sdk-manifests")){Copy-Item "$SourceSdk/sdk-manifests" $taskRoot -Recurse}
 New-Item -ItemType Directory -Force "$taskRoot/packs" | Out-Null
 foreach($pack in Get-ChildItem "$SourceSdk/packs" -Directory){
  if(-not(Test-Path "$taskRoot/packs/$($pack.Name)")){New-Item -ItemType Junction -Path "$taskRoot/packs/$($pack.Name)" -Target $pack.FullName | Out-Null}
 }
 $env:DOTNET_CLI_HOME=Join-Path $root 'artifacts/native-cli-home'
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0';$env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'
 Push-Location "$root/samples/net11.0/LanternHostMono"
 try {& "$taskRoot/dotnet.exe" workload install wasm-tools --skip-manifest-update --disable-parallel --no-cache} finally {Pop-Location}
} finally {
 foreach($key in $old.Keys){[Environment]::SetEnvironmentVariable($key,$old[$key])}
 foreach($scope in 'Process','User','Machine'){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]){throw "PATH changed: $scope"}}
}
