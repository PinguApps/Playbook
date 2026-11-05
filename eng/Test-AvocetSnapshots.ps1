[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$dotnet='C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
$saved=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$oldChange=$env:PLAYBOOK_AVOCET_CHANGE
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
$run=Join-Path $root ('artifacts/avocet-snapshot-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $run|Out-Null
function Call-Ef([string[]]$Arguments,[bool]$Failure=$false) {
$text=& $dotnet $tool @Arguments 2>&1 | Out-String
if(($LASTEXITCODE -ne 0) -ne $Failure){throw $text}
$text | Write-Host
return $text
}
try {
foreach($r in '10','11'){
$lane=Join-Path $run "net$r"
New-Item -ItemType Directory $lane|Out-Null
$source=Join-Path $root "samples/net$r.0/AvocetSnapshot"
Get-ChildItem $source -File|Where-Object{$_.Extension -notin '.db' -and $_.Name -notlike '*evidence.json'}|Copy-Item -Destination $lane
Copy-Item "$source/Migrations" "$lane/Migrations" -Recurse
$tool=(Get-ChildItem "$root/artifacts/ef$r" -Filter dotnet-ef.dll -Recurse)[0].FullName
Push-Location $lane
try {
& $dotnet restore | Out-Host
Call-Ef @('migrations','has-pending-model-changes')|Out-Null
Call-Ef @('database','update')|Out-Null
& $dotnet run -- --check | Out-Host
if($LASTEXITCODE){throw 'Schema/persistence control failed'}
if($r -eq '11'){
Copy-Item "$root/samples/net10.0/AvocetSnapshot/Migrations/AvocetContextModelSnapshot.cs" Migrations/AvocetContextModelSnapshot.cs -Force
(Get-Item Migrations/AvocetContextModelSnapshot.cs).LastWriteTimeUtc=[DateTime]::UtcNow
Call-Ef @('migrations','has-pending-model-changes')|Out-Null
$env:PLAYBOOK_AVOCET_CHANGE='1'
$text=Call-Ef @('database','update')
if($text -notmatch 'snapshot was created with EF Core version'){throw 'Old-version diagnostic absent'}
$env:PLAYBOOK_AVOCET_CHANGE=$null
Call-Ef @('migrations','add','Regenerate')|Out-Null
$generated=Get-Content (Get-ChildItem Migrations/*_Regenerate.cs)[0].FullName -Raw
if($generated -match 'migrationBuilder\.'){throw 'Unchanged model unexpectedly changed schema'}
$env:PLAYBOOK_AVOCET_CHANGE='1'
Call-Ef @('database','update') $true|Out-Null
Call-Ef @('migrations','add','AddCount')|Out-Null
$changed=Get-Content (Get-ChildItem Migrations/*_AddCount.cs)[0].FullName -Raw
if($changed -notmatch 'AddColumn<int>') {throw 'Genuine change did not generate Count column'}
Call-Ef @('database','update')|Out-Null
Call-Ef @('migrations','has-pending-model-changes')|Out-Null
$env:PLAYBOOK_AVOCET_CHANGE=$null
}
Write-Host "PASS EF$r actual snapshot validation, regenerated artifacts and schema controls"
}finally{Pop-Location}
}
}finally{
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$saved
$env:PLAYBOOK_AVOCET_CHANGE=$oldChange
}