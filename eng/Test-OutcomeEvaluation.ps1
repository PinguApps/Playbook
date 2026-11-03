[CmdletBinding()]
param([string]$Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path $root ('artifacts/outcome-preparation-' + [guid]::NewGuid().ToString('N'))
$pathsBefore = @([Environment]::GetEnvironmentVariable('PATH', 'User'), [Environment]::GetEnvironmentVariable('PATH', 'Machine'))
$oldIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$oldNative = $PSNativeCommandUseErrorActionPreference
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    $PSNativeCommandUseErrorActionPreference = $false
    New-Item -ItemType Directory -Path $scratch -Force | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $root 'samples/OutcomeEvaluation') -File | Copy-Item -Destination $scratch
    Copy-Item (Join-Path $root 'docs/evaluation/csharp-outcomes/Oracle.cs') (Join-Path $scratch 'Program.cs')
    Push-Location $scratch
    try {
        $red = & $Dotnet run -c Release 2>&1 | Out-String
        if ($LASTEXITCODE -eq 0 -or $red -notmatch 'NotImplementedException') { throw "Expected unfinished scaffold failure: $red" }
        Write-Host 'PASS: unfinished scaffold fails the behavior oracle'
        Copy-Item (Join-Path $root 'docs/evaluation/csharp-outcomes/Reference.cs') Workflow.cs
        & $Dotnet build -c Release -t:Rebuild --nologo
        if ($LASTEXITCODE -ne 0) { throw 'Reference solution failed build' }
        & $Dotnet run -c Release
        if ($LASTEXITCODE -ne 0) { throw 'Reference solution failed oracle' }
        # Format-specific ambiguous-input check, private to evaluator preparation.
        $program = Get-Content Program.cs -Raw
        $program = $program.Replace('Console.WriteLine("PASS:', 'try { SyncWorkflow.Deserialize("{\"applied\":\"r17\",\"rejected\":\"checksum\"}"); throw new InvalidOperationException("Ambiguity accepted"); } catch (System.Text.Json.JsonException) { }' + "`n" + 'Console.WriteLine("PASS:')
        $program | Set-Content Program.cs
        & $Dotnet run -c Release
        if ($LASTEXITCODE -ne 0) { throw 'Ambiguous-input check failed' }
    }
    finally { Pop-Location }
}
finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $oldIntegration
    $PSNativeCommandUseErrorActionPreference = $oldNative
    $pathsAfter = @([Environment]::GetEnvironmentVariable('PATH', 'User'), [Environment]::GetEnvironmentVariable('PATH', 'Machine'))
    if (Compare-Object $pathsBefore $pathsAfter) { throw 'Persistent PATH changed' }
}
