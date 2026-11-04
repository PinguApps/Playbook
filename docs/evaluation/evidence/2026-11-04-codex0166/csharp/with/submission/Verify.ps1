$ErrorActionPreference = 'Stop'
$sdk = 'V:/_worktrees/Playbook/t3-0f031b59/artifacts/protocol/sdk'
$dotnet = "$sdk/dotnet.exe"
$msbuild = "$sdk/sdk/11.0.100-rc.3.26522.148/MSBuild.dll"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
Push-Location $PSScriptRoot
try {
    # Invoke managed MSBuild directly to avoid the CLI launcher's network attempt.
    & $dotnet $msbuild Task.csproj /restore /t:Build /p:Configuration=Release /p:UseSharedCompilation=false /nologo /v:minimal
    if ($LASTEXITCODE -ne 0) { throw 'Application build failed.' }
    & $dotnet "$PSScriptRoot/bin/Release/net11.0/Task.dll"
    if ($LASTEXITCODE -ne 0) { throw 'Runtime checks failed.' }

    $probeDirectory = Join-Path $PSScriptRoot 'obj/CompilerChecks'
    New-Item -ItemType Directory -Force -Path $probeDirectory | Out-Null
    @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <LangVersion>15.0</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup><Reference Include="Task"><HintPath>../../bin/Release/net11.0/Task.dll</HintPath></Reference></ItemGroup>
</Project>
'@ | Set-Content -LiteralPath "$probeDirectory/Probe.csproj"
    $probes = @(
        @{ Name = 'omitted Rejected'; Code = 'using TideSync; public static class Probe { public static string Describe(SyncOutcome x) => x switch { Applied a => a.Revision, Unchanged u => u.Revision, null => "absent" }; }'; Diagnostic = 'CS8509' },
        @{ Name = 'omitted absence'; Code = 'using TideSync; public static class Probe { public static string Describe(SyncOutcome x) => x switch { Applied a => a.Revision, Unchanged u => u.Revision, Rejected r => r.Reason }; }'; Diagnostic = $null },
        @{ Name = 'unrelated value'; Code = 'using TideSync; public static class Probe { public static SyncOutcome Value() => "not an outcome"; }'; Diagnostic = 'CS0029' }
    )
    foreach ($probe in $probes) {
        $probe.Code | Set-Content -LiteralPath "$probeDirectory/Probe.cs"
        $output = & $dotnet $msbuild "$probeDirectory/Probe.csproj" /restore /t:Rebuild /p:UseSharedCompilation=false /nologo /v:minimal 2>&1
        $exitCode = $LASTEXITCODE
        $output | Set-Content -LiteralPath "$probeDirectory/$($probe.Name.Replace(' ', '-')).log"
        if ($null -eq $probe.Diagnostic) {
            if ($exitCode -ne 0) { $output | Write-Output; throw 'Absence probe failed unexpectedly.' }
            Write-Output 'CONFIRMED compiler limitation: omitted absence compiles without a diagnostic.'
            continue
        }
        if ($exitCode -eq 0 -or ($output -join "`n") -notmatch $probe.Diagnostic) {
            $output | Write-Output
            throw "Compiler probe did not reject $($probe.Name) as expected."
        }
        Write-Output "PASS compiler rejection: $($probe.Name)"
    }
}
finally { Pop-Location }
