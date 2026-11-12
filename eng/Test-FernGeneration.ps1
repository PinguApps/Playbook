#Requires -Version 7.3
[CmdletBinding()]
param([string]$Dotnet='C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe', [switch]$Record)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
$saved=@{}
foreach($scope in 'Process','User','Machine') {$saved[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$integration=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 $sample=Join-Path $root 'samples/net11.0/FernGeneration'
 $tool=Join-Path $root 'artifacts/ef11'
 if (!(Test-Path "$tool/dotnet-ef.exe")) { & $Dotnet tool install dotnet-ef --version 11.0.0 --tool-path $tool }
 Push-Location $sample
 try {
  & $Dotnet build -c Release -p:EFPrecompileQueriesStage=none
  & "$tool/dotnet-ef.exe" dbcontext optimize --configuration Release --no-build --output-dir CompiledModel --namespace FernGeneration.CompiledModel
  foreach($file in Get-ChildItem CompiledModel -Filter '*.cs') {
   [IO.File]::WriteAllText($file.FullName,[IO.File]::ReadAllText($file.FullName).Replace("`r`n","`n"))
  }
  if (@(Get-ChildItem CompiledModel -Filter '*.cs').Count -lt 4) {throw 'Compiled model files missing'}
  & $Dotnet publish -c Release
  $manifest=Get-Content obj/Release/net11.0/FernGeneration.EFGeneratedSources.Publish.txt
  if (!($manifest -match 'EFInterceptors')) {throw 'Query interceptors missing'}
  foreach($file in $manifest) {if (!(Test-Path -LiteralPath $file)) {throw "Generated source missing: $file"}}
  & $Dotnet bin/Release/net11.0/win-x64/publish/FernGeneration.dll --require-model
  if ($Record) {
   . "$PSScriptRoot/Evidence.ps1"
   $hashes=@{}
   foreach($file in @((Get-ChildItem CompiledModel -Filter '*.cs').FullName) + @($manifest)) {
    $hashes[[IO.Path]::GetRelativePath($sample,$file).Replace('\','/')]=(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
   }
   $receipt=[ordered]@{recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sdk=(& $Dotnet --version);cli=(& "$tool/dotnet-ef.exe" --version);inputs=(Get-EvidenceInputs $root 'samples/net11.0/FernGeneration/FernGeneration.csproj');generatedSources=$hashes;scope='CLI compiled model and Tasks publish interceptors compiled; published SQLite app executed with --require-model. NativeAOT not-tested. Automatic clean model-build failure is not relabelled as success.'}
   [IO.File]::WriteAllText((Join-Path $sample 'generation.evidence.json'),($receipt|ConvertTo-Json -Depth 12)+"`n")
  }
  Write-Output 'PASS: CLI compiled model, Tasks publish interceptors, published SQLite execution. NativeAOT not tested.'
 } finally {Pop-Location}
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$integration
 foreach($scope in $saved.Keys) {if($saved[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change; shared settings preserved."}}
}
