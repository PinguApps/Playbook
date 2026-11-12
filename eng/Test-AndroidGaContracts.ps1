#Requires -Version 7.4
[CmdletBinding()]
param([string]$Dotnet='C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',[switch]$Record)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$scratch=Join-Path $root ("artifacts/android-ga-"+[guid]::NewGuid().ToString("N"))
$null=New-Item -ItemType Directory $scratch -Force
$previous=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$paths=@{}; foreach($scope in 'User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach($package in @(@{id='microsoft.android.ref.37.2';name='reference';hash='42DBD909D63350E75498A27E80DF764A3ED0F25D9621CB74301BD0A465646000'},@{id='microsoft.android.sdk.windows';name='sdk';hash='D25B8EBA0CA8B92500ACB9B075A354D1EC1445C7351DE433A76558B63ADADD25'})){
  $url="https://api.nuget.org/v3-flatcontainer/$($package.id)/37.2.31/$($package.id).37.2.31.nupkg"
  Invoke-WebRequest $url -OutFile "$scratch/$($package.name).zip"
  if((Get-FileHash "$scratch/$($package.name).zip").Hash -cne $package.hash){throw "Changed actual GA package: $url"}
  Expand-Archive "$scratch/$($package.name).zip" "$scratch/$($package.name)" -Force
  Write-Host "$url SHA256=$((Get-FileHash "$scratch/$($package.name).zip").Hash)"
 }
 $dotnetRoot=Split-Path $Dotnet -Parent
 $compiler="$dotnetRoot/sdk/11.0.100/Roslyn/bincore/csc.dll"
 $refs=@(Get-ChildItem "$dotnetRoot/packs/Microsoft.NETCore.App.Ref/11.0.0/ref/net11.0/*.dll")+@(Get-ChildItem "$scratch/reference/ref/net11.0/*.dll")
 $args=@('/noconfig','/nostdlib+','/target:library','/nullable:enable','/langversion:15.0')+@($refs|ForEach-Object {'/r:'+ $_.FullName})
 Copy-Item -LiteralPath "$root/samples/net11.0/CoastalProbe/Contracts/RejectedCallback.cs.txt" -Destination "$scratch/Rejected.cs"
 $output=& $Dotnet $compiler @args "/out:$scratch/Rejected.dll" "$scratch/Rejected.cs" 2>&1 | Out-String
 if($LASTEXITCODE -eq 0 -or $output -notmatch 'error CS0619:'){throw "Missing CS0619: $output"}
 Write-Host $output
 [IO.File]::WriteAllText("$scratch/GlobalUsings.cs",'global using System; global using System.IO;')
 & $Dotnet $compiler @args "/out:$scratch/ProbePeer.dll" "$root/samples/net11.0/CoastalProbe/ProbePeer.cs" "$scratch/GlobalUsings.cs"
 if($LASTEXITCODE -ne 0){throw 'Canonical Export peer reference compilation failed'}
 Copy-Item -LiteralPath "$root/samples/net11.0/CoastalProbe/Contracts/api.xml" -Destination "$scratch/api.xml"
 & $Dotnet "$scratch/sdk/tools/generator.dll" --codegen-target=XAJavaInterop1 "--csdir=$scratch/generated" "-L:$dotnetRoot/packs/Microsoft.NETCore.App.Ref/11.0.0/ref/net11.0" "-r:$dotnetRoot/packs/Microsoft.NETCore.App.Ref/11.0.0/ref/net11.0/System.Runtime.dll" "-L:$scratch/reference/ref/net11.0" "-r:$scratch/reference/ref/net11.0/Mono.Android.dll" "-r:$scratch/reference/ref/net11.0/Java.Interop.dll" "$scratch/api.xml"
 if($LASTEXITCODE -ne 0){throw 'GA binding generation failed'}
 $generated=Get-Content "$scratch/generated/Org.Coastal.Probe.ProbePeer.cs" -Raw
 if($generated.Contains('JNINativeWrapper.CreateDelegate') -or -not $generated.Contains('JniMarshal.SafeInvokeFunc')){throw 'Changed current generated JNI callback contract'}
 & $Dotnet $compiler @args /unsafe "/out:$scratch/Generated.dll" @((Get-ChildItem "$scratch/generated/*.cs").FullName)
 if($LASTEXITCODE -ne 0){throw 'Generated current callbacks failed reference compilation'}
 # Exercise the real property defaults and diagnostic task without Android SDK/device discovery.
 [xml]$common=Get-Content "$scratch/sdk/tools/Xamarin.Android.Common.targets" -Raw
 $diagnostic=$common.SelectNodes("//*[@Code='XA1044' and @ResourceName='XA1044_RunAOTCompilationDisabled']")
 if($diagnostic.Count -ne 1){throw 'Changed diagnostic target'}
 $project='<Project><PropertyGroup><_AndroidRuntime>CoreCLR</_AndroidRuntime><OutputType>Exe</OutputType></PropertyGroup><Import Project="'+"$scratch/sdk/targets/Microsoft.Android.Sdk.DefaultProperties.targets"+'"/><UsingTask TaskName="Xamarin.Android.Tasks.AndroidError" AssemblyFile="'+"$scratch/sdk/tools/Xamarin.Android.Build.Tasks.dll"+'"/><Target Name="Check">'+$diagnostic[0].OuterXml+'</Target></Project>'
 [IO.File]::WriteAllText("$scratch/control.proj",$project)
 $output=& $Dotnet msbuild "$scratch/control.proj" /t:Check /p:RunAOTCompilation=false /v:minimal 2>&1 | Out-String
 if($LASTEXITCODE -eq 0 -or $output -notmatch 'error XA1044:'){throw "Missing XA1044: $output"}
 Write-Host $output
 & $Dotnet msbuild "$scratch/control.proj" /t:Check /p:PublishReadyToRun=false /v:minimal
 if($LASTEXITCODE -ne 0){throw 'Corrected CoreCLR property control failed'}
 & $Dotnet msbuild "$scratch/control.proj" /t:Check /p:_AndroidRuntime=MonoVM /p:RunAOTCompilation=false /v:minimal
 if($LASTEXITCODE -ne 0){throw 'Mono property control failed'}
 Write-Host 'PASS actual GA reference compilation and isolated Android property/diagnostic tasks; no Android workload build or device execution.'
 if($Record){
  . (Join-Path $PSScriptRoot 'Evidence.ps1')
  $sample='samples/net11.0/CoastalProbe/CoastalProbe.csproj'
  $receipt=Get-Content (Join-Path $root 'samples/net11.0/CoastalProbe/verification-evidence.json') -Raw|ConvertFrom-Json
  $receipt.command='eng/Test-AndroidGaContracts.ps1 -Record'
  $receipt.scope='GA Android37.2.31 reference compilation, actual binding generator and isolated MSBuild property/diagnostic task controls. CS0619/XA1044 negative controls; corrected generator/CoreCLR/Mono controls pass. No full native build or device execution.'
  $receipt.recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
  $receipt.sourceCommit=(& 'C:\Program Files\Git\cmd\git.exe' -C $root rev-parse HEAD).Trim()
  $receipt.sourceDirty=$true
  $receipt.inputs=Get-EvidenceInputs $root $sample
  $receipt.sdkInfo='Actual SDK11.0.100/compiler5.12.0-1.26529.118 and net11 reference11.0.0; Android SDK/reference37.2.31 packages pinned by SHA256 in the runner. Java.Interop declares System.Runtime10: reference compilation emits CS1701; no runtime policy claim.'
  $receipt.workloadInfo='Isolated SDK task/generator execution on Windows; no installed Android workload, JDK/NDK, APK, device or JNI runtime proof.'
  $receipt.output='PASS expected CS0619/XA1044, corrected Export peer and generated JNI callback compilation; corrected ReadyToRun and Mono property controls pass. Generated callbacks retain nullable warnings. RC receipts remain historical at cb318c7.'
  [IO.File]::WriteAllText((Join-Path $root 'samples/net11.0/CoastalProbe/verification-evidence.json'),($receipt|ConvertTo-Json -Depth 40)+"`n")
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$previous
 foreach($scope in $paths.Keys){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $paths[$scope]){throw "Unexpected persistent PATH change: $scope"}}
}
