[CmdletBinding()]
param(
 [string]$Image='mcr.microsoft.com/dotnet/sdk@sha256:9c7b6b263d9d4c2e4718e16dae6d75cd8f4b5ccf206f1e7d48a98eb429981b03',
 [string]$Net10Dotnet=(Get-Command dotnet -CommandType Application|Select-Object -First 1).Source,
 [string]$Net11Dotnet=$Net10Dotnet
)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$before=@{}; foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{}; foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
$previous=@{}; foreach($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','MSBUILDUSESERVER','UseSharedCompilation','DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE','DOTNET_CLI_TELEMETRY_OPTOUT'){$previous[$key]=[Environment]::GetEnvironmentVariable($key)}
$docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe'
$runtimeArchive=Join-Path $root 'artifacts/support-paths/runtime11-rc3.tar.gz'
$null=New-Item -ItemType Directory (Split-Path $runtimeArchive) -Force
if(!(Test-Path $runtimeArchive)){Invoke-WebRequest 'https://builds.dotnet.microsoft.com/dotnet/Runtime/11.0.0/dotnet-runtime-11.0.0-linux-x64.tar.gz' -OutFile $runtimeArchive}
Write-Output "RUNTIME_ARCHIVE sha256=$((Get-FileHash $runtimeArchive).Hash)"
$container='playbook-support-'+[guid]::NewGuid().ToString('N')
$network=$container+'-net'
$primaryError=$null
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 $env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'
 $env:MSBUILDUSESERVER='0'; $env:UseSharedCompilation='false'; $env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE='true'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
 & $docker network create --internal $network
 & $docker network create --internal ($network+'-base')
 & $docker run --pull never -d --name $container --network ($network+'-base') --mount "type=bind,source=$root,target=/src,readonly" $Image sleep 300
 & $docker exec $container sh -ec 'mkdir /runtime11; tar -xzf /src/artifacts/support-paths/runtime11-rc3.tar.gz -C /runtime11'
 foreach($major in 10,11){
   $dn=if($major -eq 10){$Net10Dotnet}else{$Net11Dotnet}
   Push-Location "$root/samples/net$major.0/ReedStation"
   try{ & $dn build -c Release }finally{Pop-Location}
   $runtime=if($major -eq 10){'/usr/share/dotnet/dotnet'}else{'/runtime11/dotnet'}
   & $docker exec $container sh -ec 'sha256sum "$1"/shared/Microsoft.NETCore.App/*/libSystem.Native.so "$1"/shared/Microsoft.NETCore.App/*/System.Net.NetworkInformation.dll' sh $(if($major -eq 10){'/usr/share/dotnet'}else{'/runtime11'})
   & $docker exec $container mkdir "/proof$major"
   & $docker exec -d $container sh -c "$runtime /src/samples/net$major.0/ReedStation/bin/Release/net$major.0/ReedStation.dll watch /proof$major > /proof$major/result 2>&1"
   & $docker exec $container sh -ec 'i=0; until test -f "$1/ready"; do i=$((i+1)); test $i -lt 100; sleep .1; done' sh "/proof$major"
   & $docker network connect $network $container
   & $docker exec $container sh -ec 'i=0; until test -f "$1/detached"; do i=$((i+1)); test $i -lt 500; sleep .1; done' sh "/proof$major"
   & $docker network disconnect $network $container
   & $docker exec $container touch "/proof$major/changed"
   & $docker exec $container sh -ec 'i=0; until grep -q "PASS real address" "$1/result"; do i=$((i+1)); test $i -lt 100; sleep .1; done; cat "$1/result"' sh "/proof$major"
 }
}catch{
 $primaryError=$_
 throw
}finally{
 $cleanupErrors=[Collections.Generic.List[string]]::new()
 try {
  try {
   $containers=@(& $docker container ls -a --format '{{.Names}}')
   if($container -cin $containers){& $docker rm -f $container}
  }catch{$cleanupErrors.Add("Container ${container}: $_")}
  foreach($ownedNetwork in @($network,($network+'-base'))){
   try {
    $networks=@(& $docker network ls --format '{{.Name}}')
    if($ownedNetwork -cin $networks){& $docker network rm $ownedNetwork}
   }catch{$cleanupErrors.Add("Network ${ownedNetwork}: $_")}
  }
 }finally{
  foreach($key in $previous.Keys){[Environment]::SetEnvironmentVariable($key,$previous[$key])}
  foreach($scope in $persistent.Keys){if(([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress) -cne $persistent[$scope]){$cleanupErrors.Add("Unexpected persistent $scope environment change; shared settings preserved.")}}
  foreach($scope in $before.Keys){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]){$cleanupErrors.Add("Unexpected $scope PATH change; shared settings preserved.")}}
 }
 if($cleanupErrors.Count){
  $message=$cleanupErrors -join '; '
  if($null -ne $primaryError){Write-Warning $message}else{throw $message}
 }else{Write-Output 'ENVIRONMENT unchanged; owned Docker resources removed'}
}
