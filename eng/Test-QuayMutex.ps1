[CmdletBinding()]
param([switch]$Record,
 [string]$Docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe',
 [string]$Net10Dotnet='C:/Program Files/dotnet/dotnet.exe',
 [string]$Net11Dotnet='C:/Users/pingu/.dotnet-playbook-11/dotnet.exe')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'EstuaryProof.ps1')
$scratch=Join-Path $root 'artifacts/quay-mutex'
$null=New-Item -ItemType Directory $scratch -Force
$log=[Collections.Generic.List[string]]::new()
$container='playbook-quay-'+[guid]::NewGuid().ToString('N')
$created=$false
try {
 foreach($version in '11.0.0-rc.2.26504.105','10.0.12'){
  $file=Join-Path $scratch "$version.tar.gz"
  if(-not(Test-Path $file)){Invoke-WebRequest "https://builds.dotnet.microsoft.com/dotnet/Runtime/$version/dotnet-runtime-$version-linux-x64.tar.gz" -OutFile $file}
  $log.Add("RUNTIME_ARCHIVE=$version;sha256=$((Get-FileHash $file).Hash)")
 }
 try {
  $image=(Invoke-Estuary $Docker $root @('image','inspect','mcr.microsoft.com/dotnet/sdk:10.0.403','--format','{{.Id}}')).Trim()
 } catch {
  throw "Required local Docker image mcr.microsoft.com/dotnet/sdk:10.0.403 is unavailable. Fetch it first with '$Docker' pull mcr.microsoft.com/dotnet/sdk:10.0.403. $($_.Exception.Message)"
 }
 $log.Add("DOCKER_IMAGE=$image")
 $null=Invoke-Estuary $Docker $root @('run','--detach','--name',$container,'--network','none','--mount',"type=bind,source=$root,target=/src,readonly",'--mount',"type=bind,source=$scratch,target=/proof,readonly",$image,'sleep','600')
 $created=$true
 $setup='mkdir /runtime11 /runtimeold; tar -xzf /proof/11.0.0-rc.2.26504.105.tar.gz -C /runtime11; tar -xzf /proof/10.0.12.tar.gz -C /runtimeold; useradd -m -u 24101 quay_a; useradd -m -u 24102 quay_b; mkdir /run/quay_a /run/quay_b /run/quay_unknown; chown 24101:24101 /run/quay_a; chown 24102:24102 /run/quay_b; chown 24103:24103 /run/quay_unknown; chmod 700 /run/quay_*'
 $null=Invoke-Estuary $Docker $root @('exec',$container,'sh','-ec',$setup)
 foreach($major in '10','11'){
  $dotnet=if($major -eq '10'){$Net10Dotnet}else{$Net11Dotnet}
  $sdk=if($major -eq '10'){'10.0.403'}else{'11.0.100-rc.3.26522.148'}
  $lang=if($major -eq '10'){'14.0'}else{'15.0'}
  $sample="samples/net$major.0/QuayMutex/QuayMutex.csproj"
  $directory=Split-Path (Join-Path $root $sample)
  Assert-Estuary (Invoke-Estuary $dotnet $directory @('--version')) $sdk
  $null=Invoke-Estuary $dotnet $directory @('build','-c','Release')
  $inputs=Initialize-EstuaryInputs $sample
  $info=Invoke-Estuary $dotnet $directory @('--info')
  $runtime=if($major -eq '10'){'/usr/share/dotnet/dotnet'}else{'/runtime11/dotnet'}
  $dll="/src/samples/net$major.0/QuayMutex/bin/Release/net$major.0/QuayMutex.dll"
  $modes=if($major -eq '10'){@('default','global','historical')}else{@('default','global')}
  foreach($mode in $modes){
   $caseRuntime=if($mode -eq 'historical'){'/runtimeold/dotnet'}else{$runtime}
   $apiMode=if($mode -eq 'historical'){'default'}else{$mode}
   $name="quay-$major-$mode-$([guid]::NewGuid().ToString('N'))"
   # All children share one Unix session. Separate docker exec calls create
   # separate sessions and would invalidate the default CurrentSessionOnly check.
   $test=@'
setpriv --reuid=24101 --regid=24101 --clear-groups env XDG_RUNTIME_DIR=/run/quay_a @@runtime@@ @@dll@@ @@mode@@ @@name@@ hold >/tmp/quay-holder.log 2>&1 & holder=$!
trap 'kill "$holder" 2>/dev/null || true; wait "$holder" 2>/dev/null || true; rm -f /tmp/quay-holder.log' EXIT
i=0; until grep -q READY /tmp/quay-holder.log; do i=$((i+1)); test "$i" -lt 50 || { cat /tmp/quay-holder.log; exit 1; }; sleep 0.1; done
cat /tmp/quay-holder.log
status=0; setpriv --reuid=24101 --regid=24101 --clear-groups env XDG_RUNTIME_DIR=/run/quay_a @@runtime@@ @@dll@@ @@mode@@ @@name@@ || status=$?
test "$status" -eq 20
echo SAME_USER_EXCLUDED
status=0; setpriv --reuid=24102 --regid=24102 --clear-groups env XDG_RUNTIME_DIR=/run/quay_b @@runtime@@ @@dll@@ @@mode@@ @@name@@ || status=$?
test "$status" -eq @@cross@@
echo CROSS_USER_VERIFIED
'@
   $test=$test.Replace('@@runtime@@',$caseRuntime).Replace('@@dll@@',$dll).Replace('@@mode@@',$apiMode).Replace('@@name@@',$name).Replace('@@cross@@',$(if($mode -eq 'default'){'0'}else{'20'}))
   $actual=Invoke-Estuary $Docker $root @('exec',$container,'sh','-ec',$test.Replace("`r`n","`n"))
   Assert-Estuary $actual 'SAME_USER_EXCLUDED'
   Assert-Estuary $actual 'CROSS_USER_VERIFIED'
   $expectedVersion=if($mode -eq 'historical'){'.NET 10.0.12'}elseif($major -eq '10'){'.NET 10.0.13'}else{'.NET 11.0.0-rc.2.26504.105'}
   Assert-Estuary $actual $expectedVersion
  }
  $unknown=Invoke-Estuary $Docker $root @('exec','--user','24103',$container,'env','-u','XDG_RUNTIME_DIR','HOME=/home/quay_a',$runtime,$dll,'default',"unknown-$major") -Exit 21
  Assert-Estuary $unknown 'EXPECTED_STORAGE_FAILURE='
  $remedy=Invoke-Estuary $Docker $root @('exec','--user','24103',$container,'env','XDG_RUNTIME_DIR=/run/quay_unknown',$runtime,$dll,'default',"remedy-$major")
  Assert-Estuary $remedy 'ENTERED=True'
  # Historical default control uses the SAME net10 consumer on actual 10.0.12.
  if($major -eq '10'){
   $old=Invoke-Estuary $Docker $root @('exec','--user','24103',$container,'env','-u','XDG_RUNTIME_DIR','/runtimeold/dotnet',$dll,'default','quay-historical')
   Assert-Estuary $old 'ENTERED=True'
   Assert-Estuary $old '.NET 10.0.12'
  }
  Save-EstuaryEvidence $sample $sdk $lang "net$major.0" 'Real isolated Linux processes: same-user exclusion, cross-user default isolation and explicit global sharing; unknown UID fails despite HOME, owned XDG_RUNTIME_DIR remedies failure. Actual 10.0.12 historical control, 10.0.13 and RC2.' $info 'eng/Test-QuayMutex.ps1 -Record' -ExpectedInputs $inputs
 }
 Write-Host 'PASS real Unix mutex processes and storage controls'
} finally {if($created){$null=Invoke-Estuary $Docker $root @('rm','--force',$container)}}
