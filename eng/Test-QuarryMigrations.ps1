#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record,
 [string]$Net10Dotnet='C:\Program Files\dotnet\dotnet.exe',
 [string]$Net11Dotnet='C:\Users\pingu\.dotnet-playbook-11\dotnet.exe')
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$true
$root=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/Evidence.ps1"
$paths=@{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
 & "$PSScriptRoot/SqlServer.ps1" -Action Start
 foreach ($release in '10','11') {
  $dotnet=if ($release -eq '10') {$Net10Dotnet} else {$Net11Dotnet}
  $version=if ($release -eq '10') {'10.0.13'} else {'11.0.0-rc.2.26504.105'}
  $source=Join-Path $root "samples/net$release.0/QuarryMigrations"
  $sample="samples/net$release.0/QuarryMigrations/QuarryMigrations.csproj"
  if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
  $before=Get-EvidenceInputs $root $sample
  $run=Join-Path $root ('artifacts/quarry-migrations-'+[guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory $run -Force | Out-Null
  foreach ($file in 'Program.cs','QuarryMigrations.csproj','Directory.Build.props','Directory.Build.targets','Directory.Packages.props','global.json','packages.lock.json') { Copy-Item (Join-Path $source $file) $run }
  Push-Location $run
  $database='playbook_quarry_'+[guid]::NewGuid().ToString('N')
  $created=$false
  try {
   & $dotnet restore --locked-mode
   & $dotnet tool install dotnet-ef --version $version --tool-path (Join-Path $run 'tools')
   $tool=@(Get-ChildItem (Join-Path $run 'tools') -Recurse -Filter dotnet-ef.dll)[0].FullName
   function Invoke-Ef([string[]]$Arguments) {
    & $dotnet $tool @Arguments --project (Join-Path $run 'QuarryMigrations.csproj')
    if ($LASTEXITCODE -ne 0) { throw 'EF command failed.' }
   }
   Invoke-Ef @('migrations','add','Unpublished')
   if ($release -eq '11') {
    # Explicit connected selection must attempt the selected unavailable server.
    $PSNativeCommandUseErrorActionPreference=$false
    $connected=& $dotnet $tool migrations remove --connection 'Server=127.0.0.1,1;Database=offline;User ID=offline;Password=not-a-secret;Encrypt=False;Connect Timeout=1;ConnectRetryCount=0' --project (Join-Path $run 'QuarryMigrations.csproj') 2>&1 | Out-String
    $connectedExit=$LASTEXITCODE
    $PSNativeCommandUseErrorActionPreference=$true
    if ($connectedExit -eq 0 -or $connected -notmatch 'network|server|connection') { throw 'Expected selected unreachable connection failure.' }
    if (!(Get-ChildItem Migrations -Filter '*_Unpublished.cs')) { throw 'Failed connected removal changed files.' }
    # An unreachable configured server must not be consulted by offline removal.
    Invoke-Ef @('migrations','remove','--offline')
    if (@(Get-ChildItem Migrations -File -ErrorAction SilentlyContinue).Count) { throw 'Offline removal left generated files.' }
    Invoke-Ef @('migrations','add','ForceControl')
    $PSNativeCommandUseErrorActionPreference=$false
    $rejection=& $dotnet $tool migrations remove --offline --force --project (Join-Path $run 'QuarryMigrations.csproj') 2>&1 | Out-String
    $exit=$LASTEXITCODE
    $PSNativeCommandUseErrorActionPreference=$true
    if ($exit -eq 0 -or $rejection -notmatch '(?is)(offline.*force|force.*offline)') { throw 'Expected incompatible offline/force rejection.' }
    if (!(Get-ChildItem Migrations -Filter '*_ForceControl.cs')) { throw 'Rejected command changed migration.' }
    Invoke-Ef @('migrations','remove','--offline')
   } else {
    # Older EF has no offline switch: remove only the known-unpublished generated
    # files from this disposable first-migration project, including its snapshot.
    Get-ChildItem Migrations -File | Remove-Item
   }
   & "$PSScriptRoot/SqlServer.ps1" -Action New -Database $database
   $created=$true
   if ($release -eq '10') {
    Invoke-Ef @('migrations','add','CreateQuarry')
    $migration=(Get-ChildItem Migrations -Filter '*_CreateQuarry.cs').FullName
    $text=Get-Content $migration -Raw
    if ($text -notmatch 'table.ForeignKey') { throw 'Older comparison missing generated FK.' }
    [regex]::Replace($text,'\s*table\.ForeignKey\([\s\S]*?\);','') | Set-Content $migration
    & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $dotnet -CommandArguments @($tool,'database','update','--project',"$run/QuarryMigrations.csproj") -WorkingDirectory $run
   } else {
    & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $dotnet -CommandArguments @($tool,'database','update','CreateQuarry','--add','--project',"$run/QuarryMigrations.csproj") -WorkingDirectory $run
   }
   $migration=Get-ChildItem Migrations -Filter '*_CreateQuarry.cs'
   if (!$migration -or !(Get-ChildItem Migrations -Filter '*_CreateQuarry.Designer.cs') -or !(Test-Path Migrations/QuarryContextModelSnapshot.cs)) { throw 'Generated files not retained.' }
   if ((Get-Content $migration.FullName -Raw) -match 'table.ForeignKey') { throw 'Excluded constraint generated.' }
   & $dotnet build -c Release
   & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $dotnet -CommandArguments @("$run/bin/Release/net$release.0/QuarryMigrations.dll",'--inspect') -WorkingDirectory $run
   $result="PASS EF$release generated files retained; schema/history/index and relationship checked; offline file workflow checked."
   Write-Host $result
   if ($Record) {
    $after=Get-EvidenceInputs $root $sample
    if (($before|ConvertTo-Json -Compress) -cne ($after|ConvertTo-Json -Compress)) { throw 'Canonical inputs changed during execution.' }
    $receipt=Get-Content (Join-Path $source 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
    $receipt.kind='execution'
    $receipt.command="eng/Test-QuarryMigrations.ps1 -Record; actual dotnet-ef $version CLI, isolated SQL2025 database and compiled inspection consumer"
    $receipt.scope=if ($release -eq '10') { 'EF10 actual two-step scaffold/apply, manual removal of known-unpublished first migration files, applied SQL history/schema, retained model/index, excluded database FK and accepted orphan insert. No EF11, PMC or GA execution in this record.' } else { 'EF11 actual combined add/apply with retained generated files, applied SQL history/schema, retained model/index, excluded database FK and accepted orphan insert. Unreachable offline removal, rejected offline/force and selected unreachable --connection failure with files preserved. No successful connected removal, PMC or GA execution.' }
    $receipt.output=$result
    $receipt.inputs=$after
    $receipt.recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
    $receipt|ConvertTo-Json -Depth 30|Set-Content (Join-Path $source 'migration-checks.evidence.json') -Encoding utf8
   }
  } finally {
   if ($created) { & "$PSScriptRoot/SqlServer.ps1" -Action Remove -Database $database }
   Pop-Location
  }
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$previous
 foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
