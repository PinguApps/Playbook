[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$false
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'artifacts/com-diagnostics'
New-Item -ItemType Directory -Force $directory | Out-Null
Copy-Item "$root/samples/net11.0/BeaconContracts/global.json" $directory
$old=[Environment]::GetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH')
$before=@{};foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$results=@()
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach($case in @(@('Mixed','','CS0525'),@('InvalidMarshal','','SYSLIB1090'),@('Safety','updated-memory-safety-rules','CS9389'),@('SafetyFixed','updated-memory-safety-rules','PASS'),@('Safety','','PASS'))) {
  $name,$feature,$expected=$case
  $source=Join-Path $root "samples/BeaconContracts/diagnostics/$name.cs"
  if($name -eq 'SafetyFixed') {
   $source=Join-Path $directory 'SafetyFixed.cs'
   (Get-Content "$root/samples/BeaconContracts/diagnostics/Safety.cs" -Raw).Replace('internal static partial int','internal static safe partial int') | Set-Content $source
  }
  "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><TargetFramework>net11.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><LangVersion>preview</LangVersion><EnableDefaultCompileItems>false</EnableDefaultCompileItems><Features>$feature</Features></PropertyGroup><ItemGroup><Compile Include=`"$source`" /></ItemGroup></Project>" | Set-Content "$directory/Probe.csproj"
  Push-Location $directory
  try {
   $output=(& 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe' build Probe.csproj -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false --nologo 2>&1 | Out-String)
   $exitCode=$LASTEXITCODE
  } finally {Pop-Location}
  if($expected -eq 'PASS') {if($exitCode -ne 0){throw $output}}
  elseif($exitCode -eq 0 -or $output -notmatch "error $expected"){throw "Expected $expected for $name`n$output"}
  $results+=@{name=$name;feature=$feature;expected=$expected;exitCode=$exitCode;output=$output}
  Write-Host "PASS $name feature=$feature expected=$expected"
 }
 $results | ConvertTo-Json -Depth 8 | Set-Content "$directory/results.json"
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$old
 foreach($scope in 'Process','User','Machine'){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]){throw "PATH changed: $scope"}}
}

