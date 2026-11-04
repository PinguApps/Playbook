# Recheck unchanged submissions in temporary copies; no model calls or retries.
param([string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe')
$ErrorActionPreference = 'Stop'
$output = Join-Path ([IO.Path]::GetTempPath()) ('reservation-replay-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path "$output/evaluator" | Out-Null
Copy-Item -LiteralPath "$PSScriptRoot/controls/Test-Oracle.ps1.txt" -Destination "$output/evaluator/Test-Oracle.ps1"
Copy-Item -LiteralPath "$PSScriptRoot/controls/Reference.cs.txt" -Destination "$output/evaluator/Reference.cs"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_CLI_HOME = "$output/dotnet-home"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
foreach ($condition in @('with', 'without')) {
    $submission = "$PSScriptRoot/$condition/submission"
    $work = "$output/$condition"
    foreach ($file in (Get-ChildItem -LiteralPath $submission -Recurse -File)) {
        $relative = [IO.Path]::GetRelativePath($submission, $file.FullName)
        if ($relative -match '\.(csproj|props|targets)\.txt$') { $relative = $relative.Substring(0, $relative.Length - 4) }
        $destination = Join-Path $work $relative
        New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination
    }
    Push-Location $work
    try {
        if ((& $Dotnet --version) -ne '11.0.100-rc.3.26522.148' -or $LASTEXITCODE) { throw 'Requires original pinned RC3 SDK' }
        & "$output/evaluator/Test-Oracle.ps1" -Submission $work -Dotnet $Dotnet *> "$output/oracle-$condition.log"
        if (!(Select-String -LiteralPath "$output/oracle-$condition.log" -SimpleMatch 'Oracle: 0 failed' -Quiet)) { throw "Oracle failed: $condition" }
        Get-Content "$output/oracle-$condition.log" | Select-String 'PASS:|Oracle:'
    } finally { Pop-Location }
}
"Disposable evaluation output: $output"
