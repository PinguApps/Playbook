[CmdletBinding()]
param(
  [string]$Net10Dotnet='C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet='C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$Docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe',
  [switch]$Record
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
. (Join-Path $PSScriptRoot 'QuayProof.ps1')
$scratch=Join-Path $root ('artifacts/quay-worker/'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory $scratch
$log=[Collections.Generic.List[string]]::new()
$command='eng/Test-QuayWorker.ps1 -Record'
$before=@{};foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{};foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
$images=[Collections.Generic.List[string]]::new()
try {
  $dockerInfo=Invoke-Quay $Docker @('version')
  foreach($major in 10,11){
    $startLog=$log.Count
    $dotnet=if($major -eq 10){$Net10Dotnet}else{$Net11Dotnet}
    $directory=Join-Path $root "samples/net$major.0/QuayWorker"
    $sdk=(Get-Content (Join-Path $directory 'global.json') -Raw|ConvertFrom-Json).sdk.version
    if((Invoke-Quay $dotnet @('--version') $directory).Trim() -cne $sdk){throw 'Wrong SDK'}
    $info=Invoke-Quay $dotnet @('--info') $directory
    $pins=Get-Content (Join-Path $root 'samples/QuayContainerProof/sdk-pins.json') -Raw|ConvertFrom-Json -AsHashtable
    foreach($pin in $pins[$sdk]){
      $file=[IO.Path]::GetFullPath((Join-Path (Split-Path $dotnet) "sdk/$sdk/$($pin.path)"))
      if((Get-FileHash $file).Hash.ToLowerInvariant() -cne $pin.sha256){throw "SDK artifact drift: $file"}
      $log.Add("SDK_ASSET $($pin.path) sha256=$($pin.sha256);product=$($pin.product)")
    }
    $context=Join-Path $scratch "context-$major"
    $null=New-Item -ItemType Directory $context
    $empty=Join-Path $scratch 'empty.config'
    Write-QuayFile $empty '<configuration><packageSources><clear/></packageSources></configuration>'
    $null=Invoke-Quay $dotnet @('publish','QuayWorker.csproj','-c','Release','-o',(Join-Path $context 'publish'),'--configfile',$empty,'-p:UseSharedCompilation=false','-p:NuGetAudit=false') $directory
    Copy-Item (Join-Path $directory 'Dockerfile') $context
    $imagePins=(Get-Content (Join-Path $root 'samples/QuayContainerProof/artifacts.json') -Raw|ConvertFrom-Json -AsHashtable).images["$major"]
    $dockerfile=Get-Content (Join-Path $context 'Dockerfile') -Raw
    Assert-QuayText $dockerfile ("FROM mcr.microsoft.com/dotnet/runtime:"+$imagePins.tag+'@'+$imagePins.amd64)
    $image='playbook-quay-'+[guid]::NewGuid().ToString('N')
    $images.Add($image)
    $null=Invoke-Quay $Docker @('build','--platform','linux/amd64','--network','none','-t',$image,$context)
    $result=Invoke-Quay $Docker @('run','--rm','--network','none','--read-only',$image)
    Assert-QuayText $result '"os":"azurelinux"'
    Assert-QuayText $result ('"version":"'+$(if($major -eq 10){'3.0'}else{'4.0'})+'"')
    Assert-QuayText $result '"fee":"1234,50"'
    Assert-QuayText $result '"architecture":"X64"'
    Assert-QuayText $result ('"runtime":".NET '+$(if($major -eq 10){'10.0.13'}else{'11.0.0-rc.2.26504.105'})+'"')
    foreach($library in 'libz.so','libicu','libc.so','libcoreclr.so'){Assert-QuayText $result $library}
    $missing=Invoke-Quay $Docker @('run','--rm','--network','none','--read-only','-e','QUAY_ZLIB=libquay_missing.so',$image) -Exit 23
    Assert-QuayText $missing 'QUAY_MISSING_NATIVE:libquay_missing.so'
    $log.Add("PASS net$major.0: actual Azure Linux, runtime, zlib export/call, ICU culture and missing-native exit23; network none/read-only/non-root image.")
    if($Record){Save-QuayEvidence "samples/net$major.0/QuayWorker/QuayWorker.csproj" $sdk $(if($major -eq 10){'14.0'}else{'15.0'}) ($info+$dockerInfo) @() (($log|Select-Object -Skip $startLog)-join "`n") 'Managed worker compiled with exact Windows SDK then executed on pinned Linux x64 runtime image; OS/native/globalization and missing dependency failure. No AOT, performance, other architecture or GA claim.'}
    Write-Host "PASS Quay worker net$major.0."
  }
}finally{
  foreach($image in $images){$null=Invoke-Quay $Docker @('image','rm',$image)}
  foreach($scope in $before.Keys){if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change; preserved shared settings."}}
  foreach($scope in $persistent.Keys){if($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress)){throw "Unexpected persistent $scope environment change."}}
  Write-Host "Disposable output: $scratch"
}
