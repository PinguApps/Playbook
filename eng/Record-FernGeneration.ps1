#Requires -Version 7.3
[CmdletBinding()]
param([string]$Dotnet=(Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$before=@{}
foreach($scope in 'Process','User','Machine') {$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$integration=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach($release in '10','11') {
  $sample="samples/net$release.0/FernGeneration/FernGeneration.csproj"
  $directory=Join-Path $root "samples/net$release.0/FernGeneration"
  Push-Location $directory
  try {
   & $dotnet restore --force-evaluate
   $lock = Join-Path $directory 'packages.lock.json'
   [IO.File]::WriteAllText($lock, [IO.File]::ReadAllText($lock).Replace("`r`n","`n"))
   $assets=Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
   $manifest=@{dependencies=@($assets.libraries.Keys | Sort-Object -CaseSensitive);workloads=@();nativeToolchains=@();additionalInputs=@('.editorconfig','eng/Evidence.ps1','eng/Record-Evidence.ps1','eng/Record-FernGeneration.ps1');buildArguments=@('-p:EFScaffoldModelStage=none','-p:EFPrecompileQueriesStage=none')}
   [IO.File]::WriteAllText((Join-Path $directory 'verification.json'), ($manifest | ConvertTo-Json -Depth 10).Replace("`r`n","`n") + "`n")
  } finally {Pop-Location}
  & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$integration
 foreach($scope in $before.Keys) {if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change; shared settings preserved."}}
}
