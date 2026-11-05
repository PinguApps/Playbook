[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$dotnet='C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
$saved=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$savedDatabase=$env:PLAYBOOK_AVOCET_DATABASE
$paths=@{};foreach($scope in 'Process','User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
function Invoke-Ef([string[]]$Arguments) {
$output=& $dotnet $tool @Arguments 2>&1 | Out-String
$output | Write-Host
if($LASTEXITCODE) {throw $output}
return $output
}
try {
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
foreach($r in '10','11') {
$tool=(Get-ChildItem "$root/artifacts/ef$r" -Filter dotnet-ef.dll -Recurse)[0].FullName
Push-Location "$root/samples/net$r.0/AvocetStartup"
try {
$env:PLAYBOOK_AVOCET_DATABASE="Data Source=$root/artifacts/avocet-startup-"+[guid]::NewGuid().ToString("N")+".db"
& $dotnet restore
$common=@('--project','../AvocetStore','--startup-project','.')
Invoke-Ef (@('migrations','list')+$common) | Out-Null
Invoke-Ef (@('database','update','--verbose')+$common) | Out-Null
if($r -eq '11') {
$out='bin/Debug/net11.0'
& $dotnet exec --runtimeconfig "$out/avocet-launch.runtimeconfig.json" --depsfile "$out/avocet-launch.deps.json" "$out/AvocetStartup.dll" --check
} else { & $dotnet run -- --check }
if($LASTEXITCODE) {throw 'Startup persistence failed'}
} finally {Pop-Location}
}
} finally {
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$saved
$env:PLAYBOOK_AVOCET_DATABASE=$savedDatabase
foreach($scope in $paths.Keys){if($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change"}}
}