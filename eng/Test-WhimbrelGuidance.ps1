#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$oldDotnet='C:/Program Files/dotnet/dotnet.exe'
$newDotnet='C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe'
. "$PSScriptRoot/Evidence.ps1"
$paths=@{};foreach($scope in 'Process','User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$previous=[Environment]::GetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH')
Push-Location $root
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach($source in (Get-Content samples/WhimbrelGuidance/sources.json -Raw|ConvertFrom-Json)){
  $bytes=(Invoke-WebRequest $source.url).RawContentStream.ToArray()
  if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256){throw "Official source drift: $($source.id)"}
 }
 $scratch=Join-Path $root 'artifacts/whimbrel';$null=New-Item -ItemType Directory "$scratch/feed" -Force
 # A deliberately portable owned assembly in two differently labelled packages:
 # these fixtures prove NuGet asset selection, never arbitrary legacy ABI compatibility.
 $refs=Get-ChildItem 'C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/10.0.13/ref/net10.0/*.dll'
 $args=@('/noconfig','/nostdlib+','/target:library','/deterministic+',"/out:$scratch/TrailMarkers.dll")+@($refs|ForEach-Object{'/reference:'+ $_.FullName})+@("$root/samples/WhimbrelPackages/Marker.cs")
 & $oldDotnet 'C:/Program Files/dotnet/sdk/10.0.402/Roslyn/bincore/csc.dll' @args
 foreach($version in '1.0.0','2.0.0'){
  $tfm=if($version -eq '1.0.0'){'monoandroid10.0'}else{'net10.0-android36.0'}
  $package="$scratch/feed/Whimbrel.Trail.Markers.$version.nupkg"
  if(Test-Path -LiteralPath $package){Remove-Item -LiteralPath $package}
  $zip=[IO.Compression.ZipFile]::Open($package,[IO.Compression.ZipArchiveMode]::Create)
  try{
   $entry=$zip.CreateEntry('Whimbrel.Trail.Markers.nuspec');$entry.LastWriteTime=[DateTimeOffset]::new(2026,1,1,0,0,0,[TimeSpan]::Zero);$writer=[IO.StreamWriter]::new($entry.Open())
   try{$writer.Write("<?xml version=`"1.0`"?><package><metadata><id>Whimbrel.Trail.Markers</id><version>$version</version><authors>Playbook</authors><description>Original asset-selection fixture, not a native compatibility promise.</description></metadata></package>")}finally{$writer.Dispose()}
   $dllEntry=$zip.CreateEntry("lib/$tfm/TrailMarkers.dll");$dllEntry.LastWriteTime=[DateTimeOffset]::new(2026,1,1,0,0,0,[TimeSpan]::Zero);$stream=$dllEntry.Open();try{$stream.Write([IO.File]::ReadAllBytes("$scratch/TrailMarkers.dll"))}finally{$stream.Dispose()}
  }finally{$zip.Dispose()}
 }
 $sample='samples/net10.0/WhimbrelTrail/WhimbrelTrail.csproj'
 [IO.File]::WriteAllText("$scratch/NuGet.Config","<configuration><packageSources><clear/><add key=`"fixtures`" value=`"$scratch/feed`"/><add key=`"nuget`" value=`"https://api.nuget.org/v3/index.json`"/></packageSources></configuration>")
 Push-Location (Split-Path $sample)
 try{
  foreach($version in '1.0.0','2.0.0'){
   & $oldDotnet restore WhimbrelTrail.csproj --force-evaluate "-p:MarkerVersion=$version" --configfile "$scratch/NuGet.Config" --packages "$scratch/cache"
   & $oldDotnet build WhimbrelTrail.csproj -c Release --no-restore "-p:MarkerVersion=$version"
   $assets=Get-Content obj/project.assets.json -Raw|ConvertFrom-Json -AsHashtable
   $selection=@($assets.targets.Values)[0]["Whimbrel.Trail.Markers/$version"]
   $expected=if($version -eq '1.0.0'){'monoandroid10.0'}else{'net10.0-android36.0'}
   foreach($kind in 'compile','runtime'){if(-not (@($selection[$kind].Keys)-match [regex]::Escape("lib/$expected/TrailMarkers.dll"))){throw "Wrong $kind selection"}}
  }
 }finally{Pop-Location}
 foreach($major in 10,11){
  $dotnet=if($major -eq 10){$oldDotnet}else{$newDotnet}
  Push-Location "samples/net$major.0/WhimbrelEdit"
  try{ & $dotnet run -c Release }finally{Pop-Location}
 }
 if($Record){
  foreach($sample in 'samples/net10.0/WhimbrelTrail/WhimbrelTrail.csproj','samples/net10.0/WhimbrelTags/WhimbrelTags.csproj','samples/net10.0/WhimbrelEdit/WhimbrelEdit.csproj','samples/net11.0/WhimbrelEdit/WhimbrelEdit.csproj'){
   $directory=Split-Path $sample
   $assets=Get-Content "$directory/obj/project.assets.json" -Raw|ConvertFrom-Json -AsHashtable
   $manifest=Get-Content "$directory/verification.json" -Raw|ConvertFrom-Json
   $manifest.dependencies=@($assets.libraries.Keys|Sort-Object)
   if($sample -like '*WhimbrelTrail*'){$manifest.buildArguments=@('-p:MarkerVersion=2.0.0','-p:RestoreSources=../../../artifacts/whimbrel/feed', '-p:RestorePackagesPath=../../../artifacts/whimbrel/cache')}
   [IO.File]::WriteAllText((Join-Path $root "$directory/verification.json"),(($manifest|ConvertTo-Json -Depth 10).Replace("`r`n","`n")+"`n"))
   $lock="$directory/packages.lock.json";[IO.File]::WriteAllText((Join-Path $root $lock),[IO.File]::ReadAllText((Join-Path $root $lock)).Replace("`r`n","`n"))
   $dotnet=if($sample -like '*net10.0*'){$oldDotnet}else{$newDotnet}
   $kind=if($sample -like '*WhimbrelEdit*'){'compile-and-run'}else{'compile'}
   & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind $kind
  }
  foreach($name in 'WhimbrelTrail','WhimbrelTags'){
   $sample="samples/net11.0/$name/$name.csproj";$directory=Split-Path $sample
   $receipt=[ordered]@{kind='official-review';sample=$sample;target=if($name -eq 'WhimbrelTrail'){'net11.0-android37.2'}else{'net11.0-ios27.0'};sdk='11.0.100';language='15.0';command='eng/Test-WhimbrelGuidance.ps1 -Record';scope='Exact pinned official source review. Current native build attempts fail NETSDK1147. No current restore, compilation or native/device execution claim.';recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' rev-parse HEAD).Trim();sourceDirty=$true;inputs=Get-EvidenceInputs $root $sample;dependencies=@();workloads=@();nativeToolchains=@();host=[Runtime.InteropServices.RuntimeInformation]::OSDescription;sdkInfo='Intended RC3 SDK; current native build fails before compilation.';workloadInfo='Current Android/iOS workloads unavailable; NETSDK1147 observed.';output='PASS pinned official sources; current native restore/build and device behavior not-tested under PIN-699.'}
   [IO.File]::WriteAllText((Join-Path $root "$directory/verification-evidence.json"),(($receipt|ConvertTo-Json -Depth 30).Replace("`r`n","`n")+"`n"))
  }
 }
 Write-Host 'PASS older Android legacy/modern compile-runtime selections and actual old/current MAUI binding engine; native device behavior not-tested.'
}finally{
 Pop-Location;[Environment]::SetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH',$previous)
 foreach($scope in $paths.Keys){if($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected PATH change: $scope"}}
}
