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
            & "$output/evaluator/Test-Oracle.ps1" -Submission $work -Dotnet $Dotnet *> "$output/oracle-$condition.log"
            Get-Content "$output/oracle-$condition.log"
        } finally { Pop-Location }
    }
    "Disposable evaluation output: $output"
} finally {
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $saved[$name], 'Process') }
}
