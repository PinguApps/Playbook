#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record,
 [string]$Dotnet='C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
 [string]$OlderDotnet='C:\Program Files\dotnet\dotnet.exe')
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/Evidence.ps1"
$paths=@{};foreach($scope in 'Process','User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$old=[Environment]::GetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH')
$client=[Net.Http.HttpClient]::new()
Push-Location $root
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 $scratch=Join-Path $root ('artifacts/sandpiper/run-'+[guid]::NewGuid().ToString('N'));$null=New-Item -ItemType Directory $scratch -Force
 foreach($source in (Get-Content samples/SandpiperFieldwork/sources.json -Raw|ConvertFrom-Json)){
  $bytes=$client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
  if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256){throw "Changed official input: $($source.url)"}
  if($source.url.EndsWith('.nupkg')){
   $file=Join-Path $scratch ([IO.Path]::GetFileName($source.url));[IO.File]::WriteAllBytes($file,$bytes)
   & $Dotnet new install $file --force --debug:custom-hive "$scratch/hive"
  }else{
   $text=[Text.Encoding]::UTF8.GetString($bytes)
   if(-not $text.Contains('EnableCrashReport') -or -not $text.Contains('DOTNET_EnableCrashReport')){throw 'Missing writer contract'}
  }
 }
 foreach($platform in 'android','ios','tvos','macos','maccatalyst'){
  $args=@('new',"${platform}test",'-n',"Sandpiper$platform",'-o',"$scratch/$platform",'--force','--debug:custom-hive',"$scratch/hive")
  if($platform -eq 'android'){$args+='--no-restore'}
  & $Dotnet @args
  $project=Get-Content "$scratch/$platform/Sandpiper$platform.csproj" -Raw
  if(-not $project.Contains('<EnableMSTestRunner>true</EnableMSTestRunner>') -or -not $project.Contains(('Version="{0}"' -f $(if($platform -eq 'android'){'4.3.2'}else{'4.5.1'})))){throw "Unexpected template contract: $platform"}
 }
 foreach($name in 'SandpiperCrashAndroid','SandpiperCrashApple'){
  $sample="samples/net10.0/$name/$name.csproj"
  if($Record){
   Push-Location (Split-Path $sample)
   try { & $OlderDotnet restore "$name.csproj" --force-evaluate } finally {Pop-Location}
   # Restore uses host line endings; evidence must match the LF bytes committed by Git.
   $lock=Join-Path $root "samples/net10.0/$name/packages.lock.json"
   [IO.File]::WriteAllText($lock,[IO.File]::ReadAllText($lock).Replace("`r`n","`n"))
   $assets=Get-Content "samples/net10.0/$name/obj/project.assets.json" -Raw|ConvertFrom-Json -AsHashtable
   $manifest=Get-Content "samples/net10.0/$name/verification.json" -Raw|ConvertFrom-Json
   $manifest.dependencies=@($assets.libraries.Keys|Sort-Object)
   [IO.File]::WriteAllText((Join-Path $root "samples/net10.0/$name/verification.json"),(($manifest|ConvertTo-Json -Depth 10)+"`n").Replace("`r`n","`n"))
   & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $OlderDotnet -Kind compile
  }
  else {Push-Location (Split-Path $sample);try{& $OlderDotnet build "$name.csproj" -c Release -p:RestoreLockedMode=true}finally{Pop-Location}}
 }
 if($Record){
  foreach($name in 'SandpiperCrashAndroid','SandpiperCrashApple','SandpiperPlatformAndroid','SandpiperPlatformIos'){
   $sample="samples/net11.0/$name/$name.csproj";$directory=Split-Path $sample
   [xml]$project=Get-Content $sample -Raw
   $target=$project.Project.PropertyGroup.TargetFramework|Where-Object {$_}
   $receipt=[ordered]@{kind='official-review';sample=$sample;target=$target;sdk='11.0.100';language='15.0';command='eng/Test-SandpiperFieldwork.ps1 -Record';scope='Historical sources and stable template package hashes checked; five real template generations on Windows. Canonical native app compilation/execution not-tested under PIN-699.';recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& 'C:\Program Files\Git\cmd\git.exe' rev-parse HEAD).Trim();sourceDirty=$true;inputs=Get-EvidenceInputs $root $sample;dependencies=@();workloads=@();nativeToolchains=@();host=[Runtime.InteropServices.RuntimeInformation]::OSDescription;sdkInfo='SDK11 GA and stable native package composition, not measured native compilation.';workloadInfo='Native SDK11 workloads unavailable; Apple Xcode and mobile devices unavailable.';output='PASS pinned inputs and five template generations; no native runtime or GA claim.'}
   $receipt|ConvertTo-Json -Depth 30|Set-Content "$directory/verification-evidence.json"
  }
 }
 Write-Host 'PASS sources, actual templates and older configuration compilation; native execution not-tested.'
} finally {
 Pop-Location;$client.Dispose();[Environment]::SetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH',$old)
 foreach($scope in $paths.Keys){if($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "PATH changed at $scope scope"}}
}
