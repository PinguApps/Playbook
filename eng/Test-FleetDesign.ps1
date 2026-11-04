#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record,
 [string]$Net10Dotnet='C:\Program Files\dotnet\dotnet.exe',
 [string]$Net11Dotnet='C:\Users\pingu\.dotnet-playbook-11\dotnet.exe')
$ErrorActionPreference='Stop'
$nativePreference=$PSNativeCommandUseErrorActionPreference
$PSNativeCommandUseErrorActionPreference=$false # The missing-Design build must fail.
$root=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/Evidence.ps1"
$before=@{}
foreach($scope in 'Process','User','Machine'){ $before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach($r in '10','11') {
  $dotnet=if($r -eq '10'){$Net10Dotnet}else{$Net11Dotnet}
  $sample="samples/net$r.0/FleetDesign/FleetDesign.csproj"
  if($Record){ & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet }
  $inputs=Get-EvidenceInputs $root $sample
  $source=Join-Path $root "samples/net$r.0/FleetDesign"
  $run=Join-Path $root ('artifacts/fleet-design-'+[guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory $run -Force | Out-Null
  Get-ChildItem $source -Exclude bin,obj,'*evidence.json',verification.json | Copy-Item -Destination $run -Recurse
  Push-Location $run
  try {
   $project=Get-Content FleetDesign.csproj -Raw
   $project -replace '(?m)^.*PackageReference Include="Microsoft.EntityFrameworkCore.Design".*\r?\n','' | Set-Content FleetDesign.csproj
   $output=& $dotnet build -c Release 2>&1 | Out-String
   $buildExit=$LASTEXITCODE
   if($r -eq '11' -and ($LASTEXITCODE -eq 0 -or $output -notmatch 'CS0234|CS1061')){throw 'Missing Design did not reproduce expected compile failure.'}
   if($r -eq '10' -and $LASTEXITCODE){throw "EF10 transitive comparison failed: $output"}
   $project | Set-Content FleetDesign.csproj
   $registration=& $dotnet run -c Release | Out-String
   if($LASTEXITCODE){throw 'Explicit Design did not restore services.'}
   Write-Host $registration.Trim()
   Write-Host "PASS EF$r transitive dependency comparison and direct Design registration."
   if($Record){
    if(($inputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)){throw 'Inputs changed during check.'}
    $receipt=Get-Content "$source/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
    $receipt.kind='execution'; $receipt.command='eng/Test-FleetDesign.ps1 -Record'
    $receipt.scope="EF$r actual Tools dependency consumer: missing direct Design build exit $buildExit; explicit Design restores real service registration. No Tasks/PMC/GA execution."
    $receipt.output="Missing direct Design build exit: $buildExit`n$output`n$registration"
    $receipt.recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
    $receipt | ConvertTo-Json -Depth 30 | Set-Content "$source/behavior.evidence.json"
   }
  } finally {Pop-Location}
 }
} finally {
 $PSNativeCommandUseErrorActionPreference=$nativePreference
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$previous
 foreach($scope in $before.Keys){if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change."}}
}
