# Re-evaluate frozen submissions in disposable copies; never rerun the agents.
param(
    [string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe',
    [string]$OutputDirectory = (Join-Path ([IO.Path]::GetTempPath()) ('playbook-outcome-replay-' + [guid]::NewGuid().ToString('N')))
)
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath $OutputDirectory) { throw 'Use a fresh output directory' }
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_CLI_HOME = Join-Path $OutputDirectory 'dotnet-home'
$env:NUGET_PACKAGES = Join-Path $OutputDirectory 'packages'
$env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'
$env:MSBUILDDISABLENODEREUSE = '1'
$env:MSBuildEnableWorkloadResolver = 'false'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
foreach ($condition in @('without', 'with')) {
    $work = Join-Path $OutputDirectory $condition
    New-Item -ItemType Directory -Path $work | Out-Null
    $submission = Join-Path $PSScriptRoot "csharp/$condition/submission"
    foreach ($file in Get-ChildItem -LiteralPath $submission -File -Recurse) {
        $relative = [IO.Path]::GetRelativePath($submission, $file.FullName)
        if ($relative -match '\.(csproj|props|targets)\.txt$') { $relative = $relative.Substring(0, $relative.Length - 4) }
        $destination = Join-Path $work $relative
        New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination
    }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'controls/Oracle.cs.txt') -Destination "$work/Program.cs" -Force
    Push-Location $work
    try {
        $version = & $Dotnet --version
        if ($LASTEXITCODE -ne 0 -or $version -ne '11.0.100-rc.3.26522.148') {
            throw 'Replay requires the original pinned SDK 11.0.100-rc.3.26522.148'
        }
        $ambiguous = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'controls/ambiguous-input.json') -Raw
        & $Dotnet run --project Task.csproj --no-launch-profile -- $ambiguous *> "$work/oracle.log"
        if ($LASTEXITCODE -ne 0) { Get-Content "$work/oracle.log"; throw "Oracle failed: $condition" }
        $output = Get-Content "$work/oracle.log" -Raw
        if ($output -notmatch 'PASS: ambiguous JSON rejected' -or $output -notmatch 'Oracle: 0 failed') { throw 'Missing oracle observations' }
        foreach ($probe in @('unrelated', 'fourth')) {
            $code = if ($probe -eq 'unrelated') {
                'using TideSync; SyncWorkflow.Describe("unrelated");'
            } else {
                'using TideSync; public sealed class Fourth : SyncOutcome { }'
            }
            $code | Set-Content -LiteralPath "$work/Program.cs"
            & $Dotnet build Task.csproj --no-restore --nologo *> "$work/$probe.log"
            $expected = if ($probe -eq 'unrelated') { 'CS1503' } else { 'CS0509' }
            if ($LASTEXITCODE -eq 0 -or (Get-Content "$work/$probe.log" -Raw) -notmatch $expected) {
                throw "Expected compiler rejection missing: $condition/$probe"
            }
        }
        "PASS $condition`: 13 behavior assertions; CS1503 unrelated value; CS0509 fourth case."
    } finally { Pop-Location }
}
"Replay output: $OutputDirectory"
