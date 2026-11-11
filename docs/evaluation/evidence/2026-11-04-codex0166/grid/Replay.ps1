# Re-evaluate unchanged source in disposable copies. No model calls.
param([string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe')
$ErrorActionPreference = 'Stop'
$environmentNames = @('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH', 'DOTNET_GENERATE_ASPNET_CERTIFICATE', 'DOTNET_CLI_HOME', 'DOTNET_CLI_TELEMETRY_OPTOUT')
$savedEnvironment = @{}
foreach ($name in $environmentNames) { $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
$output = Join-Path ([IO.Path]::GetTempPath()) ('grid-replay-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path "$output/evaluator" | Out-Null
Copy-Item "$PSScriptRoot/controls/Test-Oracle.ps1.txt" "$output/evaluator/Test-Oracle.ps1"
Copy-Item "$PSScriptRoot/controls/Reference.razor" "$output/evaluator/Reference.razor"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_CLI_HOME = "$output/dotnet-home"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
foreach ($condition in @('without', 'with')) {
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
        try { & "$output/evaluator/Test-Oracle.ps1" -Submission $work -Dotnet $Dotnet *> "$output/oracle-$condition.log" }
        catch { if ($_.Exception.Message -ne 'Behavior oracle failed') { throw } }
        $expected = if ($condition -eq 'without') { 1 } else { 2 }
        if (!(Select-String -LiteralPath "$output/oracle-$condition.log" -SimpleMatch -Pattern "Failed assertions: $expected" -Quiet)) { throw 'Frozen oracle observations differ; inspect actual output' }
        # The frozen oracle selects ambiguous sort links and rejects omitted page=1.
        # Preserve its failures; the browser check exercises semantically exact links.
        $retained = (Select-String -LiteralPath "$output/oracle-$condition.log" -Pattern 'Disposable source retained at (.*)').Matches[0].Groups[1].Value.Trim()
        $assembly = Join-Path $retained 'bin/Debug/net11.0/GridLinksEvaluation.dll'
        $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
        $listener.Start(); $port = $listener.LocalEndpoint.Port; $listener.Stop()
        & 'C:\Program Files\nodejs\node.exe' "$PSScriptRoot/controls/RunBrowser.cjs" $assembly $port "$output/browser-$condition.log"
        if ($LASTEXITCODE -or !(Select-String -LiteralPath "$output/browser-$condition.log" -SimpleMatch -Pattern 'BROWSER_PASS' -Quiet)) { throw "Browser checks failed: $condition" }
        Get-Content "$output/oracle-$condition.log" | Select-String 'FAIL:|Failed assertions:'
        Get-Content "$output/browser-$condition.log" | Select-String 'browser|PASS:|BROWSER_PASS'
    } finally { Pop-Location }
}
"Disposable evaluation output: $output"

} finally {
    foreach ($name in $environmentNames) {
        if ($null -eq $savedEnvironment[$name]) { Remove-Item -LiteralPath "Env:$name" -ErrorAction SilentlyContinue }
        else { [Environment]::SetEnvironmentVariable($name, $savedEnvironment[$name], 'Process') }
    }
}
