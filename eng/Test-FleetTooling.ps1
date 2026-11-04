#Requires -Version 7.3
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 foreach ($release in '10','11') {
  $dotnet = if ($release -eq '10') { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
  $version = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
  $sample = "samples/net$release.0/FleetTooling/FleetTooling.csproj"
  $source = Split-Path (Join-Path $root $sample)
  $run = Join-Path $root ('artifacts/fleet-' + [guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory $run -Force | Out-Null
  Get-ChildItem $source -Exclude bin,obj,'*evidence.json',verification.json | Copy-Item -Destination $run -Recurse
  Push-Location $run
  try {
   & $dotnet build -c Release -p:RestoreLockedMode=true
   & $dotnet tool install dotnet-ef --version $version --tool-path "$run/tools"
   $tool = (Get-ChildItem "$run/tools" -Recurse -Filter dotnet-ef.dll)[0].FullName
   function Invoke-Ef([string[]]$Arguments, [string]$Name) {
    & $dotnet $tool @Arguments --no-build --configuration Release 1> "$run/$Name.stdout" 2> "$run/$Name.stderr"
    if ($LASTEXITCODE) { throw "EF failed: $Name" }
   }
   # Both releases retain explicit selection as a valid route.
   foreach ($context in 'Berth','Beacon') {
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
   $result="PASS EF$release explicit scripts execute; EF11 script/update/drop and clean stdout/stderr; wildcard list RC2 failure reproduced."
   Write-Host $result
  } finally { Pop-Location }
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$previous
 foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change." } }
}



