[CmdletBinding()]
param([string]$Dotnet=(Get-Command dotnet -CommandType Application|Select-Object -First 1).Source)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$saved=$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
foreach($r in '10','11'){
foreach($n in 'AvocetStore','AvocetSnapshot','AvocetStartup'){
& "$PSScriptRoot/Record-Evidence.ps1" -Sample "samples/net$r.0/$n/$n.csproj" -Dotnet $Dotnet -Kind compile
}
}
foreach($n in 'AvocetStartup','AvocetSnapshot'){
$runner=if($n -eq 'AvocetStartup'){'Test-AvocetTooling.ps1'}else{'Test-AvocetSnapshots.ps1'}
$output=& "$PSScriptRoot/$runner" *>&1 | Out-String
Write-Host $output
foreach($r in '10','11'){
$folder=Join-Path $root "samples/net$r.0/$n"
$receipt=Get-Content "$folder/verification-evidence.json" -Raw|ConvertFrom-Json -AsHashtable
$receipt.kind='execution'
$receipt.command="eng/Record-AvocetTooling.ps1; eng/$runner"
$receipt.scope="EF$r isolated Windows x64 real tooling, schema and persistence controls; RC artifacts, not GA."
$receipt.recordedAt=[DateTimeOffset]::UtcNow.ToString('O')
$receipt.output=$output
[IO.File]::WriteAllText("$folder/behavior.evidence.json",($receipt|ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
}
}
}finally{$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=$saved}