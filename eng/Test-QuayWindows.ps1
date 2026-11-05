#Requires -Version 7.4
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sample = Join-Path $root 'samples/net10.0/QuayCSharp'
$receipt = Join-Path ([IO.Path]::GetTempPath()) ('quay-' + [guid]::NewGuid().ToString('N') + '.txt')
$exe = Join-Path $sample 'bin/Release/net10.0-windows10.0.19041.0/win-x64/QuayCSharp.exe'
$process = Start-Process -FilePath $exe -PassThru -WindowStyle Hidden -Environment @{
    PLAYBOOK_QUAY_PROBE=$receipt; DOTNET_ROOT=(Split-Path (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source -Parent); DOTNET_ROOT_X64=(Split-Path (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source -Parent); DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'
}
if (-not $process.WaitForExit(30000)) { Stop-Process -Id $process.Id; throw 'Quay window timed out.' }
if ($process.ExitCode -ne 0) { throw "Quay exited with $($process.ExitCode)." }
if (-not (Test-Path -LiteralPath $receipt)) { throw 'No loaded-window receipt.' }
$result = [IO.File]::ReadAllText($receipt)
if (-not $result.StartsWith('PASS: Label.Loaded and managed button event; Crates checked: 1;')) { throw $result }
Write-Host $result
Remove-Item -LiteralPath $receipt
