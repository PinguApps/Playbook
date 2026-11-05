[CmdletBinding()]
param([switch]$Record,
 [string]$Docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'EstuaryProof.ps1')
$scratch=Join-Path $root 'artifacts/tide-mono'
$null=New-Item -ItemType Directory $scratch -Force
$log=[Collections.Generic.List[string]]::new()
$container='playbook-tide-mono-'+[guid]::NewGuid().ToString('N')
$paths=@{}
foreach($scope in 'Process','User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
try {
 $image=(Invoke-Estuary $Docker $root @('image','inspect','mcr.microsoft.com/dotnet/sdk:10.0.403','--format','{{.Id}}')).Trim()
 $null=Invoke-Estuary $Docker $root @('run','--detach','--name',$container,'--env','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0','--env','DOTNET_CLI_TELEMETRY_OPTOUT=1','--mount',"type=bind,source=$root,target=/source,readonly",$image,'sleep','3600')
 $setup=@'
set -eu
apt-get update -qq
DEBIAN_FRONTEND=noninteractive apt-get install -y -qq mono-devel clang zlib1g-dev curl >/tmp/setup.log 2>&1
curl -fsSL https://builds.dotnet.microsoft.com/dotnet/Sdk/11.0.100-rc.3.26522.148/dotnet-sdk-11.0.100-rc.3.26522.148-linux-x64.tar.gz -o /tmp/sdk11.tar.gz
sha256sum /tmp/sdk11.tar.gz
mkdir /sdk11 /work
tar -xzf /tmp/sdk11.tar.gz -C /sdk11
mkdir -p /work/samples
cp /source/samples/Directory.* /work/samples/
cp /source/NuGet.Config /work/
for major in 10 11; do
 for name in TideWindow ObservatoryImporter; do
  destination=/work/samples/net$major.0/$name
  mkdir -p "$destination"
  find /source/samples/net$major.0/$name -maxdepth 1 -type f -exec cp {} "$destination/" \;
 done
done
cat /etc/os-release
mono --version
clang --version
ld --version
dpkg-query -W mono-devel mono-runtime clang zlib1g-dev
sha256sum /usr/lib/mono/4.7.2-api/mscorlib.dll
'@
 $info=Invoke-Estuary $Docker $root @('exec',$container,'sh','-ec',$setup.Replace("`r`n","`n"))
 $native=@(('Mono/'+[regex]::Match($info,'Mono JIT compiler version ([^ ]+)').Groups[1].Value),
           ('Clang/'+[regex]::Match($info,'clang version ([^ ]+)').Groups[1].Value))
 foreach($major in '10','11'){
  $sdk=if($major -eq '10'){'10.0.403'}else{'11.0.100-rc.3.26522.148'}
  $dotnet=if($major -eq '10'){'/usr/share/dotnet/dotnet'}else{'/sdk11/dotnet'}
  foreach($name in 'ObservatoryImporter','TideWindow'){
   $sample="samples/net$major.0/$name/$name.csproj"
   $directory="/work/samples/net$major.0/$name"
   $local=Split-Path (Join-Path $root $sample)
   $before=Get-EvidenceInputs $root $sample
   if($name -eq 'ObservatoryImporter'){
    $check=@'
set -eu
cd @@directory@@
@@dotnet@@ --info
@@dotnet@@ build -c Release
@@dotnet@@ msbuild -p:Configuration=Release -getProperty:RunCommand,RunArguments,RunWorkingDirectory
mkdir -p 'working directory'
touch 'working directory/observatory.input' observatory.input
status=0; @@dotnet@@ run -c Release > /tmp/default-@@major@@.log 2>&1 || status=$?
cat /tmp/default-@@major@@.log
test "$status" -eq @@defaultExit@@
rm observatory.input
status=0; @@dotnet@@ run -c Release -p:ExplicitMono=true > /tmp/explicit-@@major@@.log 2>&1 || status=$?
cat /tmp/explicit-@@major@@.log
test "$status" -eq 7
grep -Fx 'Working directory: @@directory@@/working directory' /tmp/explicit-@@major@@.log
# A broken working-directory setting must fail even though the quoted launch works.
status=0; /usr/bin/mono 'bin/importer output/net472/ObservatoryImporter.exe' 'night survey' 7 || status=$?
test "$status" -eq 9
rm 'working directory/observatory.input'
test ! -e observatory.input
test ! -e 'working directory/observatory.input'
echo PASS_MONO_@@major@@
'@
    $check=$check.Replace('@@defaultExit@@',$(if($major -eq '10'){'7'}else{'1'}))
    $lang='7.3';$target='net472'
   }else{
    $check=@'
set -eu
cd @@directory@@
@@dotnet@@ --info
@@dotnet@@ publish -c Release -r linux-x64 -p:RestorePackagesWithLockFile=true -o /tmp/tide-@@major@@
clang caller.c -ldl -o /tmp/caller-@@major@@
expected=@@expected@@
test -f "/tmp/tide-@@major@@/$expected"
nm -D "/tmp/tide-@@major@@/$expected" | grep tide_window_minutes
selected=$(sh package.sh /tmp/tide-@@major@@ /tmp/package-@@major@@ so)
test "$(find /tmp/package-@@major@@ -type f | wc -l)" -eq 1
/tmp/caller-@@major@@ "$selected"
if [ '@@major@@' = 11 ]; then
 test ! -e /tmp/tide-11/TideWindow.so
 if cp /tmp/tide-11/TideWindow.so /tmp/old-name-control.so; then exit 1; fi
 @@dotnet@@ publish -c Release -r linux-x64 -p:UseNativeLibPrefix=false -o /tmp/tide-optout
 test -f /tmp/tide-optout/TideWindow.so
 selected=$(sh package.sh /tmp/tide-optout /tmp/package-optout so)
 /tmp/caller-11 "$selected"
fi
echo PASS_TIDE_@@major@@
'@
    $check=$check.Replace('@@expected@@',$(if($major -eq '10'){'TideWindow.so'}else{'libTideWindow.so'}))
    $lang=if($major -eq '10'){'14.0'}else{'15.0'};$target="net$major.0"
   }
   $check=$check.Replace('@@directory@@',$directory).Replace('@@dotnet@@',$dotnet).Replace('@@major@@',$major).Replace("`r`n","`n")
   $output=Invoke-Estuary $Docker $root @('exec',$container,'sh','-ec',$check)
   Assert-Estuary $output "PASS_$(if($name -eq 'TideWindow'){'TIDE'}else{'MONO'})_$major"
   $null=Invoke-Estuary $Docker $root @('cp',"${container}:$directory/obj",$local)
   if($name -eq 'TideWindow'){$null=Invoke-Estuary $Docker $root @('cp',"${container}:$directory/packages.lock.json",(Join-Path $local 'packages.lock.json'))}
   if($Record){
    $manifest=Get-Content (Join-Path $local 'verification.json') -Raw|ConvertFrom-Json -AsHashtable
    $manifest.dependencies=@((Get-Content (Join-Path $local 'obj/project.assets.json') -Raw|ConvertFrom-Json -AsHashtable).libraries.Keys|Sort-Object -CaseSensitive)
    $manifest.nativeToolchains=$native
    Write-EstuaryFile (Join-Path $local 'verification.json') (($manifest|ConvertTo-Json -Depth 10)+"`n")
   }
   # Dependency inventory is generated after restore; executable source bytes must stay identical.
   $after=Get-EvidenceInputs $root $sample
   foreach($key in $before.Keys){if($key -notlike '*/verification.json' -and $before[$key] -cne $after[$key]){throw "Input changed: $key"}}
   Save-EstuaryEvidence $sample $sdk $lang $target 'Real Linux x64 container compilation and native/Mono execution; see exact commands and measured toolchain inventory. macOS not tested.' $info 'eng/Test-TideAndMono.ps1 -Record' $native -ExpectedInputs $after
  }
 }
 Write-Host 'PASS Linux shared-library naming, native calls, packaging controls and explicit Mono launch'
} finally {
 $null=& $Docker rm --force $container
 foreach($scope in $paths.Keys){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $paths[$scope]){throw "Unexpected $scope PATH change; shared settings preserved"}}
}
