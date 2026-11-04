# Re-evaluate preserved submissions in disposable copies. Never launch model calls.
param([string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe')
$ErrorActionPreference = 'Stop'
$names = @('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','DOTNET_CLI_HOME','DOTNET_CLI_TELEMETRY_OPTOUT')
$saved = @{}
foreach ($name in $names) { $saved[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
    $output = Join-Path ([IO.Path]::GetTempPath()) ('manifest-replay-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path "$output/evaluator" | Out-Null
    foreach ($name in @('Test-Oracle.ps1','Oracle.cs','Reference.cs')) { Copy-Item "$PSScriptRoot/controls/$name.txt" "$output/evaluator/$name" }
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
    $env:DOTNET_CLI_HOME = "$output/dotnet-home"
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    function Invoke-Oracle([string]$Name, [string]$Work, [int]$Failures, [switch]$Reference) {
        # The oracle launches a native child whose inherited stdout bypasses
        # PowerShell redirection. Capture at the outer process boundary.
        $start = [Diagnostics.ProcessStartInfo]::new((Get-Process -Id $PID).Path)
        $start.UseShellExecute = $false
        $start.RedirectStandardOutput = $true
        $start.RedirectStandardError = $true
        foreach ($arg in @('-NoProfile','-File',"$output/evaluator/Test-Oracle.ps1",'-Submission',$Work,'-Dotnet',$Dotnet)) { $start.ArgumentList.Add($arg) }
        if ($Reference) { $start.ArgumentList.Add('-Reference') }
        $process = [Diagnostics.Process]::Start($start)
        try {
            $stdout = $process.StandardOutput.ReadToEndAsync()
            $stderr = $process.StandardError.ReadToEndAsync()
            if (!$process.WaitForExit(180000)) { $process.Kill($true); throw 'Evaluator capture timeout' }
            $text = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
            [IO.File]::WriteAllText("$output/oracle-$Name.log", $text)
            if ([regex]::Matches($text, '(?m)^(PASS|FAIL):').Count -ne 10 -or $text -notmatch "Failed assertions: $Failures(?:\r?\n|$)") { throw "Unexpected assertion output: $Name" }
            if (($Failures -eq 0) -ne ($process.ExitCode -eq 0)) { throw "Unexpected evaluator exit: $Name" }
            $text
        } finally { $process.Dispose() }
    }
    foreach ($condition in @('with','without')) {
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
            if ((& $Dotnet --version) -ne '11.0.100-rc.3.26522.148' -or $LASTEXITCODE) { throw 'Requires pinned RC3 SDK' }
            Invoke-Oracle $condition $work 0
        } finally { Pop-Location }
    }
    # Reconstruct the frozen unfinished scaffold from the unchanged submission
    # inputs and the retained original implementation, then calibrate both ways.
    $baseline = "$output/baseline"
    Copy-Item -LiteralPath "$output/without" -Destination $baseline -Recurse
    Copy-Item "$PSScriptRoot/controls/Baseline.cs.txt" "$baseline/Reconciliation.cs"
    Invoke-Oracle 'baseline' $baseline 9
    Invoke-Oracle 'reference' $baseline 0 -Reference
    "Disposable evaluation output: $output"
} finally {
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $saved[$name], 'Process') }
}
