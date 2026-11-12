#Requires -Version 7.4
[CmdletBinding()]
param([switch]$Record,
 [string]$OutputDirectory=(Join-Path $PSScriptRoot '../artifacts/apple-authoring'),
 [string]$Python='C:\Python315\python.exe',
 [string]$Dotnet='C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
 [string]$Git='C:\Program Files\Git\cmd\git.exe')
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/Evidence.ps1"
$paths=@{}; foreach($scope in 'Process','User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$previous=[Environment]::GetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH')
$client=[Net.Http.HttpClient]::new()
Push-Location $root
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 $beforeInputs=@{}
 foreach($name in 'OystercatcherManifest','OystercatcherFramework','OystercatcherBinding'){ $sample="samples/net11.0/$name/$name.csproj"; $beforeInputs[$sample]=Get-EvidenceInputs $root $sample }
 $scratch=$ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
 $null=New-Item -ItemType Directory $scratch -Force
 foreach($source in (Get-Content "$root/samples/AppleAuthoring/sources.json" -Raw | ConvertFrom-Json)) {
  $bytes=$client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
  if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256){throw "Changed source/artifact: $($source.url)"}
  if($source.archive){
   [IO.File]::WriteAllBytes("$scratch/$($source.archive).zip",$bytes)
   Expand-Archive "$scratch/$($source.archive).zip" "$scratch/$($source.archive)" -Force
  }
  foreach($check in $source.checks){if(-not [Text.Encoding]::UTF8.GetString($bytes).Contains($check)){throw "Missing source contract: $check"}}
 }
 $dotnetRoot=Split-Path $Dotnet -Parent
 $sdk='11.0.100'
 $compiler="$dotnetRoot/sdk/$sdk/Roslyn/bincore/csc.dll"
 $referenceFiles=@(Get-ChildItem "$dotnetRoot/packs/Microsoft.NETCore.App.Ref/11.0.0/ref/net11.0/*.dll")
 $ios="$scratch/reference/ref/net11.0/Microsoft.iOS.dll"
 $attributes="$scratch/sdk/tools/lib/Xamarin.Apple.BindingAttributes.dll"
 $references=@($referenceFiles | ForEach-Object {'/r:'+ $_.FullName})+@("/r:$ios","/r:$attributes")
 & $Dotnet $compiler /noconfig /nostdlib /target:library /langversion:15.0 /nullable:enable "/out:$scratch/Api.dll" @references "$root/samples/net11.0/OystercatcherBinding/ApiDefinition.cs"
 & $Dotnet "$scratch/sdk/tools/lib/bgen/bgen.dll" "--compiled-api-definition-assembly=$scratch/Api.dll" '--target-framework=.NETCoreApp,Version=v11.0,Profile=ios' "--baselib=$ios" "--attributelib=$attributes" "--sourceonly=$scratch/generated.txt" "--outdir=$scratch/generated" @($referenceFiles | ForEach-Object {'-r:'+ $_.FullName})
 $generated=@(Get-Content "$scratch/generated.txt")
 $permit=Get-Content "$scratch/generated/Oystercatcher/SurveyPermit.g.cs" -Raw
 foreach($contract in 'internal SurveyPermit (string stationCode)','public static SurveyPermit? TryCreate (string stationCode)','rv.Handle == global::ObjCRuntime.NativeHandle.Zero','rv.Dispose ();','return null;'){
  if(-not $permit.Contains($contract)){throw "Missing generated contract: $contract"}
 }
 & $Dotnet $compiler /noconfig /nostdlib /target:library /unsafe /langversion:15.0 /nullable:enable "/out:$scratch/Binding.dll" @($references | Where-Object {$_ -ne "/r:$attributes"}) @generated "$root/samples/net11.0/OystercatcherBinding/Consumer.cs"
 # Run actual Apple tasks without SDK/Xcode discovery or signing. This is task output, not a published app.
 [xml]$framework=Get-Content "$root/samples/net11.0/OystercatcherFramework/OystercatcherFramework.csproj" -Raw
 $preserve=$framework.Project.PropertyGroup.StripFrameworkHeaders
 if($preserve -cne 'false'){throw 'Canonical preservation opt-out missing'}
 $project=@"
<Project>
<UsingTask TaskName="Xamarin.MacDev.Tasks.CompileAppManifest" AssemblyFile="$scratch/sdk/tools/msbuild/net11.0/Xamarin.MacDev.Tasks.dll" />
<UsingTask TaskName="Xamarin.MacDev.Tasks.StripFrameworkHeaders" AssemblyFile="$scratch/sdk/tools/msbuild/net11.0/Xamarin.MacDev.Tasks.dll" />
<ItemGroup>
<AppManifestEntry Include="CFBundleDisplayName" Type="String" Value="Oystercatcher Surveys" />
<AppManifestEntry Include="OystercatcherOffline" Type="Boolean" Value="true" />
<AppManifestEntry Include="OystercatcherRegions" Type="StringArray" Value="Marsh|Estuary" ArraySeparator="|" />
<AppManifestEntry Include="OystercatcherLegacy" Type="Remove" />
</ItemGroup>
<Target Name="Check">
<CompileAppManifest AppBundleName="Oystercatcher" BundleExecutable="Oystercatcher" AppManifest="$root/samples/net11.0/OystercatcherManifest/Info.plist" AppManifestEntries="@(AppManifestEntry)" CompiledAppManifest="$scratch/output.plist" IsAppExtension="false" MinSupportedOSPlatformVersion="17.0" ProjectDir="$root" ResourcePrefix="Resources" SdkIsSimulator="false" TargetFrameworkMoniker=".NETCoreApp,Version=v11.0,Profile=ios" SdkVersion="27.0" SupportedOSPlatformVersion="17.0" TargetArchitectures="ARM64" />
<StripFrameworkHeaders AppBundleDir="$scratch/strip.app" TargetFrameworkMoniker=".NETCoreApp,Version=v11.0,Profile=ios" />
<StripFrameworkHeaders AppBundleDir="$scratch/preserve.app" TargetFrameworkMoniker=".NETCoreApp,Version=v11.0,Profile=ios" Condition="'$preserve' != 'false'" />
</Target></Project>
"@
 # Pull the tested item declarations directly from the canonical project.
 [xml]$canonical=Get-Content "$root/samples/net11.0/OystercatcherManifest/OystercatcherManifest.csproj" -Raw
 $project=$project -replace '(?s)<ItemGroup>.*?</ItemGroup>', $canonical.Project.ItemGroup.OuterXml
 [IO.File]::WriteAllText("$scratch/tasks.proj",$project)
 foreach($bundle in 'strip.app','preserve.app') {
  foreach($dir in 'Headers','PrivateHeaders','Modules'){
   $target="$scratch/$bundle/Frameworks/SurveyKit.framework/$dir"
   $null=New-Item -ItemType Directory $target -Force
   [IO.File]::WriteAllText("$target/owned-fixture.txt",'Oystercatcher task fixture; not a native framework binary.')
  }
  [IO.File]::WriteAllText("$scratch/$bundle/Frameworks/SurveyKit.framework/Info.plist",'<plist version="1.0"><dict/></plist>')
 }
 & $Dotnet "$dotnetRoot/sdk/$sdk/MSBuild.dll" "$scratch/tasks.proj" /t:Check /v:minimal
 # Read the real binary plist using Python's standard plist parser.
 & $Python -c 'import plistlib,sys; p=plistlib.load(open(sys.argv[1],"rb")); assert p["CFBundleDisplayName"]=="Oystercatcher Surveys"; assert p["OystercatcherOffline"] is True; assert p["OystercatcherRegions"]==["Marsh","Estuary"]; assert "OystercatcherLegacy" not in p; print("PASS actual typed plist add/override/remove")' "$scratch/output.plist"
 foreach($dir in 'Headers','PrivateHeaders','Modules'){
  if(Test-Path "$scratch/strip.app/Frameworks/SurveyKit.framework/$dir"){throw "Not stripped: $dir"}
  if(-not(Test-Path "$scratch/preserve.app/Frameworks/SurveyKit.framework/$dir/owned-fixture.txt")){throw "Not preserved: $dir"}
 }
 if(-not(Test-Path "$scratch/strip.app/Frameworks/SurveyKit.framework/Info.plist")){throw 'Unrelated framework metadata removed'}
 Write-Host 'PASS actual strip task removes Headers/PrivateHeaders/Modules; canonical opt-out task control retains them. No signing/native binary.'
 foreach($sample in $beforeInputs.Keys){
  if((ConvertTo-Json $beforeInputs[$sample] -Compress) -cne (ConvertTo-Json (Get-EvidenceInputs $root $sample) -Compress)){throw "Inputs changed during checks: $sample"}
 }
 if($Record){
  foreach($name in 'OystercatcherManifest','OystercatcherFramework','OystercatcherBinding'){
   foreach($version in '11'){
    $sample="samples/net$version.0/$name/$name.csproj"
    $receipt=[ordered]@{kind='official-review';sample=$sample;target="net$version.0-ios$(if($version -eq '10'){'26'}else{'27'}).0";sdk=if($version -eq '10'){'10.0.402'}else{$sdk};language=if($version -eq '10'){'14.0'}else{'15.0'};command='eng/Test-AppleAuthoring.ps1 -Record';scope='Pinned official source/artifact review. Actual GA task output and generated net11 binding/consumer reference compilation on Windows; not a native workload build, package or runtime.';recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& $Git rev-parse HEAD).Trim();sourceDirty=[bool](& $Git status --porcelain -- "samples/net$version.0/$name" samples/AppleAuthoring eng/Test-AppleAuthoring.ps1);inputs=(Get-EvidenceInputs $root $sample);dependencies=@();workloads=@();nativeToolchains=@();host=[Runtime.InteropServices.RuntimeInformation]::OSDescription;sdkInfo='GA compiler and MSBuild; GA Apple tools/references. Intended native pins are not measured native compositions.';workloadInfo='Xcode unavailable; native compilation/execution not-tested under PIN-699.';output='PASS exact sources/artifacts; real plist and stripping tasks; generated nullable factory and consumer compiled. No native execution or GA claim.'}
    [IO.File]::WriteAllText("$root/samples/net$version.0/$name/verification-evidence.json",(ConvertTo-Json $receipt -Depth 30)+"`n")
   }
  }
 }
} finally {
 Pop-Location
 $client.Dispose()
 [Environment]::SetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH',$previous)
 foreach($scope in $paths.Keys){if($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "PATH changed at $scope scope"}}
}
