#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record,
 [string]$Net10Dotnet=(Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
 [string]$Net11Dotnet=(Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/Evidence.ps1"
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach ($release in '10','11') {
  $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
  $version = if ($release -eq '10') { '10.0.13' } else { '11.0.0' }
  $sample = "samples/net$release.0/FleetTooling/FleetTooling.csproj"
  if($Record){ & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet }
  $inputs=Get-EvidenceInputs $root $sample
  $observed=[Collections.Generic.List[string]]::new()
  $source = Split-Path (Join-Path $root $sample)
  $run = Join-Path $root ('artifacts/fleet-' + [guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory $run -Force | Out-Null
  Get-ChildItem $source -Exclude bin,obj,'*evidence.json',verification.json | Copy-Item -Destination $run -Recurse
  Push-Location $run
  try {
   & $dotnet build -c Release -p:RestoreLockedMode=true
   & $dotnet tool install dotnet-ef --version $version --tool-path "$run/tools"
   $tool = (Get-ChildItem "$run/tools" -Recurse -Filter dotnet-ef.dll)[0].FullName
   & ./Commands.ps1 -Dotnet $dotnet -EfTool $tool
   & ./Capture.ps1 -Dotnet $dotnet -EfTool $tool
   $captured=& $dotnet "bin/Release/net$release.0/FleetTooling.dll" execute berth-deployment.sql | Out-String
   $observed.Add($captured.Trim())
   Write-Host $captured.Trim()
   function Invoke-Ef([string[]]$Arguments, [string]$Name) {
    & $dotnet $tool @Arguments --no-build --configuration Release 1> "$run/$Name.stdout" 2> "$run/$Name.stderr"
    if ($LASTEXITCODE) { throw "EF failed: $Name" }
   }
   # Both releases retain explicit selection as a valid route.
   foreach ($context in 'Berth','Beacon') {
    Invoke-Ef @('migrations','list','--context',"$($context)Context") "list-$context"
    $listed=Get-Content "$run/list-$context.stdout" -Raw
    if($listed -notmatch "Initial$context"){throw "List missed $context migration."}
    $observed.Add($listed.Trim())
    Invoke-Ef @('migrations','script','--context',"$($context)Context",'--output',"$context.sql") "explicit-$context"
    & $dotnet "bin/Release/net$release.0/FleetTooling.dll" execute "$context.sql"
   }
   if ($release -eq '11') {
    # Known upstream RC2 fall-through: list visits contexts, then tries to resolve the literal '*'.
    $PSNativeCommandUseErrorActionPreference=$false
    & $dotnet $tool migrations list --context '*' --no-build --configuration Release 1> list.stdout 2> list.stderr
    $listExit=$LASTEXITCODE
    $PSNativeCommandUseErrorActionPreference=$true
    if ($listExit -ne 1 -or (Get-Content list.stderr -Raw) -notmatch "No DbContext named '\*' was found") { throw 'Wildcard list behavior changed; refresh the documented limitation.' }
    $observed.Add("Wildcard list exit $listExit`: " + (Get-Content list.stderr -Raw).Trim())
    Invoke-Ef @('migrations','script','--context','*','--output','combined.sql') 'scripts'
    $combined=Get-Content combined.sql -Raw
    if ($combined -notmatch 'CREATE TABLE "Berths"' -or $combined -notmatch 'CREATE TABLE "Beacons"') { throw 'Wildcard script missed a model.' }
    # Combined SQL is not automatically routed to separate databases. Use explicit per-context output for deployment.
    Invoke-Ef @('database','update','--context','*') 'update'
    foreach ($file in 'fleet-berths.db','fleet-beacons.db') { if (-not (Test-Path $file)) { throw "Missing $file" } }
    & $dotnet 'bin/Release/net11.0/FleetTooling.dll' inspect fleet-berths.db Berths
    & $dotnet 'bin/Release/net11.0/FleetTooling.dll' inspect fleet-beacons.db Beacons
    Invoke-Ef @('migrations','script','--context','BerthContext','--verbose') 'clean'
    & $dotnet 'bin/Release/net11.0/FleetTooling.dll' execute "$run/clean.stdout"
    if (-not (Get-Item clean.stderr).Length) { throw 'Expected verbose diagnostics on stderr.' }
    Invoke-Ef @('database','drop','--context','*','--force') 'drop'
    foreach ($file in 'fleet-berths.db','fleet-beacons.db') { if (Test-Path $file) { throw "Drop missed $file" } }
   }
   $result=if($release -eq '10'){'PASS EF10 explicit lists and scripts; canonical output-file capture executes.'}else{'PASS EF11 script/update/drop and clean stdout/stderr; wildcard list RC2 failure reproduced.'}
   Write-Host $result
   $observed.Add($result)
   if($Record){
    if(($inputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)){throw 'Inputs changed during check.'}
    $receipt=Get-Content "$source/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
    $receipt.kind='execution'; $receipt.command='eng/Test-FleetTooling.ps1 -Record'
    $receipt.scope=$result
    $receipt.output=$observed -join "`n"
    $receipt.recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
    $receipt | ConvertTo-Json -Depth 30 | Set-Content "$source/behavior.evidence.json"
   }
  } finally { Pop-Location }
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$previous
 foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change." } }
}



