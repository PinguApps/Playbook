#Requires -Version 7.0
[CmdletBinding()]
param(
 [string]$Net10Dotnet='C:/Program Files/dotnet/dotnet.exe',
 [string]$Net11Dotnet='C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$work=Join-Path $root 'artifacts/ieee-decimal'
New-Item -ItemType Directory -Force $work | Out-Null
$previous=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$paths=@{}
foreach($scope in 'Process','User','Machine') {$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 $releases=Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json'
 $file=($releases.releases | Where-Object release-version -EQ '11.0.0-rc.1').runtime.files |
   Where-Object { $_.rid -eq 'win-x64' -and $_.name -eq 'dotnet-runtime-win-x64.zip' }
 if(@($file).Count -ne 1) {throw 'Expected one exact RC1 runtime'}
 $zip=Join-Path $work 'rc1.zip'
 if(-not(Test-Path $zip)) {Invoke-WebRequest $file.url -OutFile $zip}
 if((Get-FileHash $zip -Algorithm SHA512).Hash -ine $file.hash) {throw 'RC1 archive hash mismatch'}
 $rc1=Join-Path $work ('rc1-'+[guid]::NewGuid().ToString('N'))
 Expand-Archive -LiteralPath $zip -DestinationPath $rc1 -Force
 foreach($manifestName in 'decimal-results','cryptography-platforms') {
  $manifest=Get-Content (Join-Path $root "docs/planning/audits/$manifestName.sources.json") -Raw | ConvertFrom-Json
  foreach($source in $manifest.sources) {
   if($manifestName -eq 'cryptography-platforms' -and $source.id -notlike 'composite-*') {continue}
   $bytes=(Invoke-WebRequest $source.url).RawContentStream.ToArray()
   if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -ine $source.sha256) {throw "Source hash mismatch: $($source.id)"}
  }
 }
 foreach($v in 10,11) {
  $exe=if($v -eq 10){$Net10Dotnet}else{$Net11Dotnet}
  $sdk=if($v -eq 10){'10.0.403'}else{'11.0.100-rc.3.26522.148'}
  $runtime=if($v -eq 10){'10.0.13'}else{'11.0.0-rc.2.26504.105'}
  foreach($name in 'ObservatoryResults','ObservatoryBulletins') {
   $sample="samples/net$v.0/$name/$name.csproj"
   & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $exe
   $recordPath=Join-Path $root "samples/net$v.0/$name/verification-evidence.json"
   $record=Get-Content $recordPath -Raw | ConvertFrom-Json -AsHashtable
   foreach($asset in @(
     (Join-Path (Split-Path $exe) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
     (Join-Path (Split-Path $exe) "packs/Microsoft.NETCore.App.Ref/$runtime/ref/net$v.0/System.Runtime.dll"),
     (Join-Path (Split-Path $exe) "shared/Microsoft.NETCore.App/$runtime/System.Private.CoreLib.dll"),
     (Join-Path (Split-Path $exe) "shared/Microsoft.NETCore.App/$runtime/System.Security.Cryptography.dll")
   )) {
    $record.output += "`nARTIFACT $asset version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($asset).ProductVersion) sha256=$((Get-FileHash $asset -Algorithm SHA256).Hash)"
   }
   if($v -eq 11 -and $name -eq 'ObservatoryResults') {
    $app=Join-Path $root "samples/net11.0/ObservatoryResults/bin/Release/net11.0/ObservatoryResults.dll"
    $rc1Exe=Join-Path $rc1 'dotnet.exe'
    $observed=(& $rc1Exe exec --fx-version 11.0.0-rc.1.26425.128 $app --observe | Out-String).Trim()
    if($observed -notmatch '1.414250203437893161687265742266675E-17') {throw 'Expected actual RC1 cancellation-sensitive result'}
    $record.output += "`nRC1 archive=$($file.url) sha512=$($file.hash)`n$observed"
    $core=Join-Path $rc1 'shared/Microsoft.NETCore.App/11.0.0-rc.1.26425.128/System.Private.CoreLib.dll'
    $record.output += "`nRC1 CoreLib version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($core).ProductVersion) sha256=$((Get-FileHash $core -Algorithm SHA256).Hash)"
    $record.command += "; $rc1Exe exec --fx-version 11.0.0-rc.1.26425.128 $app --observe"
   }
   $record.command += '; SHA256 compiler/reference/runtime assets'
   $record | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath $recordPath
  }
 }
} finally {
 if($rc1 -and (Test-Path -LiteralPath $rc1)) {
  $target=[IO.Path]::GetFullPath($rc1)
  if((Split-Path -Parent $target) -cne [IO.Path]::GetFullPath($work) -or (Split-Path -Leaf $target) -notlike 'rc1-*') {throw 'Unsafe RC1 cleanup target'}
  Remove-Item -LiteralPath $target -Recurse -Force
 }
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$previous
 foreach($scope in $paths.Keys) {if($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) {throw "Unexpected $scope PATH change"}}
}
