[CmdletBinding()]
param([switch]$Record,[switch]$VerifySources)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/HostContractEvidence.ps1"
$before=@{}; foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$old=[Environment]::GetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH')
Push-Location $root
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 if($VerifySources){Test-HostPublisherSources}
 & "$PSScriptRoot/Test-BeaconDiagnostics.ps1"
 $compiler='C:/Program Files/Microsoft Visual Studio/18/Community/VC/Tools/MSVC/14.51.36231/bin/Hostx64/x64/cl.exe'
 $linker='C:/Program Files/Microsoft Visual Studio/18/Community/VC/Tools/MSVC/14.51.36231/bin/Hostx64/x64/link.exe'
 $native=Join-Path $root 'artifacts/beacon';New-Item -ItemType Directory -Force $native | Out-Null
 & $compiler /nologo /c /GS- /TC "/Fo$native/beacon.obj" samples/net11.0/BeaconContracts/beacon.c
 & $linker /NOLOGO /DLL /NOENTRY /NODEFAULTLIB /MACHINE:X64 "/OUT:$native/beacon.dll" "$native/beacon.obj"
 $toolchains=@("MSVC/$((Get-Item $compiler).VersionInfo.FileVersion)","MSVC-link/$((Get-Item $linker).VersionInfo.FileVersion)")
 foreach($target in 'net10.0','net11.0') {
  $dotnet=if($target -eq 'net10.0'){(Get-Command dotnet -CommandType Application | Select-Object -First 1).Source}else{(Get-Command dotnet -CommandType Application | Select-Object -First 1).Source}
  $sample="samples/$target/BeaconContracts/BeaconContracts.csproj"
  Push-Location "samples/$target/BeaconContracts"
  try { & $dotnet build -c Release -p:RestoreLockedMode=true } finally {Pop-Location}
  Copy-Item "$native/beacon.dll" "samples/$target/BeaconContracts/bin/Release/$target/beacon.dll"
  $identities=Get-HostArtifactProof $dotnet (Join-Path $root $sample) $target 'Microsoft.Interop.ComInterfaceGenerator.dll'
  $identities+=@($compiler,$linker,"$native/beacon.dll") | ForEach-Object { "NATIVE $_ sha256=$((Get-FileHash $_).Hash.ToLowerInvariant())" }
  if($Record){
   & ./eng/Record-Evidence.ps1 -Sample $sample -Dotnet $dotnet -NativeToolchains $toolchains
   $receiptPath=Join-Path (Split-Path $sample) 'verification-evidence.json'
   $receipt=Get-Content $receiptPath -Raw | ConvertFrom-Json -AsHashtable
   $receipt.command="$compiler /nologo /c /GS- /TC /Fo$native/beacon.obj $root/samples/net11.0/BeaconContracts/beacon.c; $linker /NOLOGO /DLL /NOENTRY /NODEFAULTLIB /MACHINE:X64 /OUT:$native/beacon.dll $native/beacon.obj; Copy-Item beacon.dll beside managed output; " + $receipt.command + "; $root/eng/Test-BeaconDiagnostics.ps1; Get-HostArtifactProof"
   $receipt.scope='Real Windows x64 native C consumer invokes generated COM HRESULT/UTF16 accessors; original audit reference/generator hashes agree; actual mixed-accessor/count-only-marshalling and separate preview-safety compiler checks.'
   $receipt.output+="`n"+($identities -join "`n")+"`n"+[IO.File]::ReadAllText("$root/artifacts/com-diagnostics/results.json")
   [IO.File]::WriteAllText((Join-Path $root $receiptPath),($receipt | ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
  }
  else {& $dotnet "samples/$target/BeaconContracts/bin/Release/$target/BeaconContracts.dll"}
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$old;Pop-Location
 foreach($scope in 'Process','User','Machine'){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]){throw "PATH changed: $scope"}}
}
