$ErrorActionPreference = 'Stop'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = 'true'
$env:DOTNET_NOLOGO = '1'
$env:MSBuildEnableWorkloadResolver = 'false'
$env:DOTNET_CLI_HOME = 'C:\Users\pingu\AppData\Local\Temp\playbook-comparison-20261104\without\dotnet-home'
$env:NUGET_PACKAGES = 'C:\Users\pingu\AppData\Local\Temp\playbook-comparison-20261104\without\cache'
$env:TEMP = 'C:\Users\pingu\AppData\Local\Temp\playbook-comparison-20261104\without\temp'
$env:TMP = $env:TEMP
& 'V:/_worktrees/Playbook/t3-0f031b59/artifacts/protocol/sdk/dotnet.exe' 'V:/_worktrees/Playbook/t3-0f031b59/artifacts/protocol/sdk/sdk/11.0.100-rc.3.26522.148/MSBuild.dll' "$PSScriptRoot/Task.csproj" -t:Build -p:NuGetAudit=false -p:UseSharedCompilation=false -nologo -verbosity:minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& 'V:/_worktrees/Playbook/t3-0f031b59/artifacts/protocol/sdk/dotnet.exe' "$PSScriptRoot/bin/Debug/net11.0/Task.dll"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# These intentionally invalid calls must fail compilation with specific diagnostics.
$compilerArgs = @('/noconfig', '/nostdlib+', '/target:library', '/langversion:15.0', '/nullable:enable', '/nologo', "/out:$PSScriptRoot/obj/CompileFailures.dll", "/reference:$PSScriptRoot/bin/Debug/net11.0/Task.dll")
$compilerArgs += Get-ChildItem 'V:/_worktrees/Playbook/t3-0f031b59/artifacts/protocol/sdk/packs/Microsoft.NETCore.App.Ref/11.0.0-rc.3.26522.148/ref/net11.0/*.dll' | ForEach-Object { '/reference:' + $_.FullName }
$compilerArgs += "$PSScriptRoot/CompileFailures.cs.txt"
$diagnostics = & 'V:/_worktrees/Playbook/t3-0f031b59/artifacts/protocol/sdk/dotnet.exe' 'V:/_worktrees/Playbook/t3-0f031b59/artifacts/protocol/sdk/sdk/11.0.100-rc.3.26522.148/Roslyn/bincore/csc.dll' @compilerArgs
if ($LASTEXITCODE -eq 0) { throw 'Invalid outcome calls unexpectedly compiled.' }
$diagnosticText = $diagnostics -join "`n"
if ($diagnosticText -notmatch 'CS0029' -or $diagnosticText -notmatch "CS7036.*onRejected" -or $diagnosticText -notmatch "CS7036.*onAbsent") {
    $diagnostics | Write-Output
    throw 'Expected unrelated-value and missing-handler diagnostics were not all present.'
}
$diagnostics | Write-Output
Write-Output 'Passed compiler rejection checks for unrelated values and missing handlers.'
exit 0
