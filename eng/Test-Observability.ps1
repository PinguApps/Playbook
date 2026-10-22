[CmdletBinding()]
param([string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe')

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$sample = Join-Path $PSScriptRoot '../samples/net11.0/TracingLogging'
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
Push-Location $sample
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($probe in @(
        @{ Symbol = 'UNSUPPORTED_REF_STRUCT'; Diagnostic = 'SYSLIB1011' },
        @{ Symbol = 'EXPENSIVE_LOG_ARGUMENT'; Diagnostic = 'CA1873' }
    )) {
        $output = & $Net11Dotnet build -c Release -p:RestoreLockedMode=true "-p:DefineConstants=$($probe.Symbol)" 2>&1 | Out-String
        if ($LASTEXITCODE -eq 0 -or $output -notmatch $probe.Diagnostic) {
            throw "Expected actual $($probe.Diagnostic) rejection: $output"
        }
        Write-Host "PASS: actual pinned toolchain rejected $($probe.Symbol) with $($probe.Diagnostic)."
    }
}
finally {
    # Restore ordinary generated output even after an unexpected probe result.
    & $Net11Dotnet build -c Release -p:RestoreLockedMode=true
    $restored = $LASTEXITCODE
    Pop-Location
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    if ($restored) { throw 'Ordinary generated logging build was not restored.' }
}
