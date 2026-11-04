#Requires -Version 7.3
[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$before=@{}
foreach($scope in 'Process','User','Machine') {$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$integration=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach($release in '10','11') {
  $dotnet=if($release -eq '10'){'C:\Program Files\dotnet\dotnet.exe'}else{'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'}
  $sample="samples/net$release.0/FernGeneration/FernGeneration.csproj"
  $directory=Join-Path $root "samples/net$release.0/FernGeneration"
  Push-Location $directory
  try {
   & $dotnet restore --force-evaluate
   $assets=Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
   $manifest=@{dependencies=@($assets.libraries.Keys | Sort-Object -CaseSensitive);workloads=@();nativeToolchains=@();additionalInputs=@('.editorconfig','eng/Evidence.ps1','eng/Record-Evidence.ps1','eng/Record-FernGeneration.ps1');buildArguments=@('-p:EFScaffoldModelStage=none','-p:EFPrecompileQueriesStage=none')}
   $manifest | ConvertTo-Json -Depth 10 | Set-Content verification.json
  } finally {Pop-Location}
  & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$integration
 foreach($scope in $before.Keys) {if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change; shared settings preserved."}}
}
