#Requires -Version 7.3
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$before=@{}
foreach($scope in 'Process','User','Machine'){ $before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach($r in '10','11') {
  $dotnet=if($r -eq '10'){'C:\Program Files\dotnet\dotnet.exe'}else{'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'}
  $source=Join-Path $root "samples/net$r.0/FleetDesign"
  $run=Join-Path $root ('artifacts/fleet-design-'+[guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory $run -Force | Out-Null
  Get-ChildItem $source -Exclude bin,obj,'*evidence.json',verification.json | Copy-Item -Destination $run -Recurse
  Push-Location $run
  try {
   $project=Get-Content FleetDesign.csproj -Raw
   $project -replace '(?m)^.*PackageReference Include="Microsoft.EntityFrameworkCore.Design".*\r?\n','' | Set-Content FleetDesign.csproj
   $output=& $dotnet build -c Release 2>&1 | Out-String
   if($r -eq '11' -and ($LASTEXITCODE -eq 0 -or $output -notmatch 'CS0234|CS1061')){throw 'Missing Design did not reproduce expected compile failure.'}
   if($r -eq '10' -and $LASTEXITCODE){throw "EF10 transitive comparison failed: $output"}
   $project | Set-Content FleetDesign.csproj
   & $dotnet run -c Release
   if($LASTEXITCODE){throw 'Explicit Design did not restore services.'}
   Write-Host "PASS EF$r transitive dependency comparison and direct Design registration."
  } finally {Pop-Location}
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$previous
 foreach($scope in $before.Keys){if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change."}}
}
