[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path $root ('artifacts/tern-testing/run-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory $scratch
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()
$captures = [Collections.Generic.List[object]]::new()
$git = 'C:/Program Files/Git/cmd/git.exe'

function Invoke-Consumer([string]$Dotnet,[string]$Directory,[string[]]$Arguments,[int]$Exit = 0) {
  $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  $start.RedirectStandardInput = $true
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_GENERATE_ASPNET_CERTIFICATE'] = 'false'
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $scratch 'home'
  $start.Environment['NUGET_PACKAGES'] = Join-Path $scratch 'cache'
  $start.Environment['NUGET_HTTP_CACHE_PATH'] = Join-Path $scratch 'http'
  $start.Environment['DOTNET_NOLOGO'] = '1'
  $start.Environment['DOTNET_CLI_USE_MSBUILD_SERVER'] = 'false'
  $start.Environment['MSBUILDUSESERVER'] = '0'
  $start.Environment['DOTNET_ROOT'] = Split-Path $Dotnet
  $start.Environment['DOTNET_ROOT_X64'] = Split-Path $Dotnet
  $start.Environment['DOTNET_HOST_PATH'] = $Dotnet
  $process = [Diagnostics.Process]::Start($start)
  try {
    $process.StandardInput.Close()
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(180000)) { $process.Kill($true); throw 'Tern consumer timed out.' }
    $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    $log.Add("cwd=$Directory`n$Dotnet $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
    [IO.File]::WriteAllText((Join-Path $scratch 'commands.log'),($log -join "`n"))
    if ($process.ExitCode -ne $Exit) { throw "Expected exit $Exit, got $($process.ExitCode): $output" }
    return $output
  }
  finally { $process.Dispose() }
}
function Assert-Text([string]$Actual,[string]$Expected) {
  if (-not $Actual.Contains($Expected,[StringComparison]::Ordinal)) { throw "Missing '$Expected': $Actual" }
}
function Write-Lf([string]$Path,[string]$Content) { [IO.File]::WriteAllText($Path,$Content.Replace("`r`n","`n")) }

try {
  $cases = @(
    @{Path='TernTesting';Target='net10.0';Sdk='10.0.402';Language='14.0';Version='18.7.0';Dotnet=$Net10Dotnet},
    @{Path='net10.0/TernHost';Target='net10.0';Sdk='10.0.402';Language='14.0';Version='18.11.0';Dotnet=$Net10Dotnet},
    @{Path='net11.0/TernHost';Target='net11.0';Sdk='11.0.100-rc.2.26504.105';Language='15.0';Version='18.11.0';Dotnet=$Net11Dotnet}
  )
  foreach ($case in $cases) {
    $directory = Join-Path $root "samples/$($case.Path)"
    Write-Host "Qualify $($case.Path): $($case.Version)"
    Assert-Text (Invoke-Consumer $case.Dotnet $directory @('--version')) $case.Sdk
    $info = Invoke-Consumer $case.Dotnet $directory @('--info')
    $compiler = Join-Path (Split-Path $case.Dotnet) "sdk/$($case.Sdk)/Roslyn/bincore/csc.dll"
    $null = Invoke-Consumer $case.Dotnet $directory @($compiler,'-version')
    foreach ($relative in 'TernHost.csproj','Tests/TernTests.csproj','Resolver/TernResolver.csproj') {
      $project = Join-Path $directory $relative
      $restore = @('restore',$project)
      if (Test-Path (Join-Path (Split-Path $project) 'packages.lock.json')) { $restore += '--locked-mode' }
      $null = Invoke-Consumer $case.Dotnet $directory $restore
      $lock = Join-Path (Split-Path $project) 'packages.lock.json'
      if ($Record) { Write-Lf $lock (Get-Content $lock -Raw) }
      $assets = Get-Content (Join-Path (Split-Path $project) 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
      foreach ($entry in $assets.targets[$case.Target].GetEnumerator()) {
        $log.Add("lane=$($case.Path);project=$relative;resolved=$($entry.Key);sha512=$($assets.libraries[$entry.Key].sha512);assets=$($entry.Value | ConvertTo-Json -Depth 15 -Compress)")
      }
      $verificationPath = Join-Path (Split-Path $project) 'verification.json'
      if ($Record) {
        $verification = Get-Content $verificationPath -Raw | ConvertFrom-Json
        $verification.dependencies = $dependencies
        Write-Lf $verificationPath (($verification | ConvertTo-Json -Depth 12) + "`n")
      }
      $sample = [IO.Path]::GetRelativePath($root,$project).Replace('\','/')
      $null = Invoke-Consumer $case.Dotnet $directory @('build',$project,'-c','Release','--no-restore')
      $captures.Add(@{Case=$case;Sample=$sample;Inputs=$null;Dependencies=$dependencies;Info=$info})
    }
    # Parent sample inputs include the nested test/resolver projects. Finalize all locks first.
    foreach ($capture in $captures | Where-Object { $_.Case.Path -eq $case.Path }) {
      $capture.Inputs = Get-EvidenceInputs $root $capture.Sample
    }
    $cache = Join-Path $scratch 'cache'
    $cliPackage = Join-Path $cache "microsoft.testplatform.cli/$($case.Version)"
    $console = @(Get-ChildItem (Join-Path $cliPackage 'contentFiles/any') -Recurse -Filter 'vstest.console.dll')
    if ($console.Count -ne 1) { throw 'Expected one cross-platform console DLL.' }
    $console = $console[0].FullName
    foreach ($id in 'microsoft.testplatform.cli','microsoft.testplatform.translationlayer','microsoft.testplatform.build','microsoft.testplatform.testhost','microsoft.testplatform.objectmodel') {
      $package = Join-Path $cache "$id/$($case.Version)/$id.$($case.Version).nupkg"
      $log.Add("package=$id/$($case.Version);sha256=$((Get-FileHash $package).Hash.ToLowerInvariant())")
    }
    $pins = Get-Content (Join-Path $root 'samples/TernTesting/artifacts.json') -Raw | ConvertFrom-Json
    foreach ($pin in $pins.packages | Where-Object version -EQ $case.Version) {
      $package = Join-Path $cache "$($pin.id)/$($pin.version)/$($pin.id).$($pin.version).nupkg"
      if ((Get-FileHash $package).Hash.ToLowerInvariant() -cne $pin.sha256) { throw "Package ZIP drift: $($pin.id)" }
    }
    $translationReadme = Join-Path $cache "microsoft.testplatform.translationlayer/$($case.Version)/README.md"
    if ($case.Version -eq '18.11.0') {
      Assert-Text (Get-Content $translationReadme -Raw) 'programmatically discover and execute tests'
      Assert-Text (Get-Content $translationReadme -Raw) 'StartSession'
    }
    foreach ($dll in @($console,(Join-Path $cache "microsoft.testplatform.build/$($case.Version)/lib/netstandard2.0/Microsoft.TestPlatform.Build.dll"))) {
      $log.Add("artifact=$dll;sha256=$((Get-FileHash $dll).Hash.ToLowerInvariant());product=$([Diagnostics.FileVersionInfo]::GetVersionInfo($dll).ProductVersion)")
    }

    # Build a controlled real extension graph outside all application probing paths.
    $plugin = Join-Path $scratch ("plugin-" + $case.Path.Replace('/','-'))
    $null = New-Item -ItemType Directory -Force "$plugin/rules-source","$plugin/plugin-source","$plugin/deploy/rules"
    Copy-Item (Join-Path $directory 'global.json') $plugin
    '<Project />' | Set-Content "$plugin/Directory.Build.props"
    '<Project />' | Set-Content "$plugin/Directory.Build.targets"
    '<Project />' | Set-Content "$plugin/Directory.Packages.props"
    Copy-Item (Join-Path $root 'NuGet.Config') $plugin
    Copy-Item (Join-Path $root 'samples/TernTesting/Rules.cs') "$plugin/rules-source/Rules.cs"
    Copy-Item (Join-Path $root 'samples/TernTesting/Plugin.cs') "$plugin/plugin-source/Plugin.cs"
    Copy-Item (Join-Path $root 'samples/TernTesting/Rules.csproj.txt') "$plugin/rules-source/Rules.csproj"
    Copy-Item (Join-Path $root 'samples/TernTesting/Plugin.csproj.txt') "$plugin/plugin-source/Plugin.csproj"
    $null = Invoke-Consumer $case.Dotnet $plugin @('build','plugin-source/Plugin.csproj','-c','Release',"-p:TernTargetFramework=$($case.Target)")
    Copy-Item "$plugin/plugin-source/bin/Release/$($case.Target)/Tern.Plugin.dll" "$plugin/deploy"
    Copy-Item "$plugin/rules-source/bin/Release/$($case.Target)/Tern.Rules.dll" "$plugin/deploy/rules"
    $testDll = Join-Path $directory "Tests/bin/Release/$($case.Target)/Tern.TestAdapter.dll"
    $hostDll = Join-Path $directory "bin/Release/$($case.Target)/TernHost.dll"
    $output = Invoke-Consumer $case.Dotnet $scratch @($hostDll,$console,$testDll)
    foreach ($text in 'PASS discovery;total=4','PASS repeat=0','PASS repeat=1','PASS cancellation;total=0','PASS after-cancel','PASS tern-host;connection-closed') { Assert-Text $output $text }
    Write-Host $output
    $resolverDll = Join-Path $directory "Resolver/bin/Release/$($case.Target)/TernResolver.dll"
    $resolverOutput = Invoke-Consumer $case.Dotnet $scratch @($resolverDll,"$plugin/deploy")
    Assert-Text $resolverOutput 'PASS real-plugin'
    Assert-Text $resolverOutput 'PASS platform-resolver'
    Write-Host $resolverOutput

    # Real custom MSBuild task invocation. Parse its real TRX; no runner-selection changes.
    $task = Join-Path $cache "microsoft.testplatform.build/$($case.Version)/lib/netstandard2.0/Microsoft.TestPlatform.Build.dll"
    $acquisition = Join-Path $scratch ("acquire-" + $case.Path.Replace('/','-'))
    $null = New-Item -ItemType Directory $acquisition
    Copy-Item (Join-Path $root 'samples/TernTesting/Acquire.csproj.txt') "$acquisition/Acquire.csproj"
    foreach ($boundary in 'Directory.Build.props','Directory.Build.targets','Directory.Packages.props') {
      '<Project />' | Set-Content (Join-Path $acquisition $boundary)
    }
    Copy-Item (Join-Path $root 'samples/net10.0/TernHost/global.json') $acquisition
    $null = Invoke-Consumer $Net10Dotnet $acquisition @('restore','Acquire.csproj',"-p:TernPlatformVersion=$($case.Version)")
    $results = Join-Path $scratch ("settings-" + $case.Path.Replace('/','-'))
    $arguments = @('msbuild',(Join-Path $root 'samples/TernTesting/RunTests.proj'),'-t:Check',"-p:TernTaskAssembly=$task","-p:TernTests=$testDll","-p:TernConsole=$console","-p:TernAdapterDirectory=$(Split-Path $testDll)","-p:TernResults=$results")
    $output = Invoke-Consumer $case.Dotnet $directory $arguments
    [xml]$trx = Get-Content (Join-Path $results 'settings.trx') -Raw
    if ($trx.TestRun.ResultSummary.Counters.total -ne '1' -or $trx.TestRun.ResultSummary.Counters.passed -ne '1') { throw 'Real settings test did not pass exactly once.' }
    foreach ($value in 'Harbour=North Quay','Pattern=^Tern\.Bay/West=13$','Literal=--listTests=not a switch') { Assert-Text $trx.OuterXml "TERN_EFFECTIVE $value" }
    $log.Add("effective-settings-trx=$($trx.OuterXml)")
    Write-Host "PASS real-task-settings $($case.Path): spaces/backslash/slash/equals/switch-looking value;one test passed"

    # A real source-based settings test must fail when the XML/CLI configuration disappears.
    $emptySettings = Join-Path $scratch 'missing-tern.runsettings'
    Write-Lf $emptySettings '<RunSettings />'
    $missingArguments = $arguments.Clone()
    $missingArguments[1] = Join-Path $root 'samples/TernTesting/XmlTests.proj'
    $missingArguments[-1] = "-p:TernResults=$results-missing"
    $missingArguments += "-p:TernSettingsFile=$emptySettings"
    $null = Invoke-Consumer $case.Dotnet $directory $missingArguments 1
    [xml]$missingTrx = Get-Content "$results-missing/settings.trx" -Raw
    if ($missingTrx.TestRun.ResultSummary.Counters.total -ne '1' -or $missingTrx.TestRun.ResultSummary.Counters.failed -ne '1') { throw 'Missing settings did not fail exactly one real test.' }
    Assert-Text $missingTrx.OuterXml 'Tern settings are required for source-based execution.'
    Write-Host "PASS missing-settings-rejected $($case.Path)"

    # Compile the old pooled API, setter and task assignment against both real package sets.
    $probe = Join-Path $scratch ("probe-" + $case.Path.Replace('/','-'))
    $null = New-Item -ItemType Directory $probe
    Copy-Item (Join-Path $directory 'global.json') $probe
    Copy-Item (Join-Path $root 'NuGet.Config') $probe
    '<Project />' | Set-Content "$probe/Directory.Build.props"
    '<Project />' | Set-Content "$probe/Directory.Build.targets"
    '<Project />' | Set-Content "$probe/Directory.Packages.props"
    @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>$($case.Target)</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings></PropertyGroup><ItemGroup>
<PackageReference Include="Microsoft.TestPlatform.TranslationLayer" Version="[$($case.Version)]" />
<PackageReference Include="Microsoft.TestPlatform.TestHost" Version="[$($case.Version)]" />
<Reference Include="Microsoft.TestPlatform.Build"><HintPath>$task</HintPath></Reference>
<Reference Include="Microsoft.Build.Utilities.Core"><HintPath>`$(MSBuildBinPath)/Microsoft.Build.Utilities.Core.dll</HintPath></Reference>
<Reference Include="Microsoft.Build.Framework"><HintPath>`$(MSBuildBinPath)/Microsoft.Build.Framework.dll</HintPath></Reference>
</ItemGroup></Project>
"@ | Set-Content "$probe/Probe.csproj"
    $old = $case.Version -eq '18.7.0'
    $controls = @{
      session = (Get-Content (Join-Path $root 'samples/TernTesting/LegacySession.cs') -Raw)
      task = 'namespace TernTesting; public static class Probe { public static object Make() => new Microsoft.TestPlatform.Build.Tasks.VSTestTask { VSTestCLIRunSettings = new[] { "Tern.Harbour=North Quay" } }; }'
    }
    foreach ($control in $controls.GetEnumerator()) {
      Write-Lf "$probe/Probe.cs" $control.Value
      $expected = if ($old) { 0 } else { 1 }
      $output = Invoke-Consumer $case.Dotnet $probe @('build','Probe.csproj','-c','Release') $expected
      if (-not $old -and $output -notmatch 'error CS(0246|1061|0200|0029)') { throw 'Unexpected old-contract compiler failure.' }
      $log.Add("PASS old-contract=$($control.Key);package=$($case.Version);exit=$expected")
    }
    $contract = Get-Content (Join-Path $root 'samples/TernTesting/TaskContract.cs') -Raw
    Write-Lf "$probe/Probe.cs" $contract
    $null = Invoke-Consumer $case.Dotnet $probe @('build','Probe.csproj','-c','Release',$(if ($old) { '-p:DefineConstants=LEGACY_PLATFORM' } else { '-p:DefineConstants=TERN_CURRENT' }))
    # The old resolver setter exists in the audited SDK binary, not public package18.7.
    $probeProject = Get-Content "$probe/Probe.csproj" -Raw
    if ($old) {
      $sdkResolver = Join-Path (Split-Path $case.Dotnet) "sdk/$($case.Sdk)/Microsoft.TestPlatform.PlatformAbstractions.dll"
      $probeProject = [regex]::Replace($probeProject,'<PackageReference[^>]*/>','')
      $probeProject = $probeProject.Replace('</ItemGroup>',"<Reference Include=`"Microsoft.TestPlatform.PlatformAbstractions`"><HintPath>$sdkResolver</HintPath></Reference></ItemGroup>")
      Write-Lf "$probe/Probe.csproj" $probeProject
      if ((Get-FileHash $sdkResolver).Hash.ToLowerInvariant() -cne 'e52ae9db24b333b65ccb15173d2db1fabd944f7f861e1b9209e613710bf624b5') { throw 'Audited old SDK resolver drift.' }
      $log.Add("resolver-old-sdk=$sdkResolver;sha256=$((Get-FileHash $sdkResolver).Hash.ToLowerInvariant());public-package18.7-lacks-RequestingAssembly")
    }
    Write-Lf "$probe/Probe.cs" 'using R = Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces.AssemblyResolveEventArgs; namespace TernTesting; public static class Probe { public static R Make() => new R("Tern") { RequestingAssembly = typeof(Probe).Assembly }; }'
    $output = Invoke-Consumer $case.Dotnet $probe @('build','Probe.csproj','-c','Release') $(if ($old) { 0 } else { 1 })
    if (-not $old) { Assert-Text $output 'error CS0200' }

    # Literal semicolons are a changed contract. The new scalar loses MSBuild's old escaped-entry boundary.
    $boundaryProject = Join-Path $scratch ("boundary-" + $case.Path.Replace('/','-') + '.proj')
    $taskProject = Join-Path $root 'samples/TernTesting/RunTests.proj'
    Write-Lf $boundaryProject "<Project><Import Project=`"$taskProject`" /><PropertyGroup><TernCliSettings>`$(TernCliSettings);Tern.RejectSemicolon=east%3Bwest</TernCliSettings></PropertyGroup></Project>"
    $boundaryArguments = $arguments.Clone()
    $boundaryArguments[1] = $boundaryProject
    $boundaryArguments[-1] = "-p:TernResults=$results-boundary"
    $boundaryOutput = Invoke-Consumer $case.Dotnet $directory $boundaryArguments $(if ($old) { 0 } else { 1 })
    if ($old) {
      Assert-Text $boundaryOutput 'Passed'
      [xml]$boundaryTrx = Get-Content "$results-boundary/settings.trx" -Raw
      Assert-Text $boundaryTrx.OuterXml 'TERN_EFFECTIVE RejectSemicolon=east;west'
    }
    else {
      if ($boundaryOutput -notmatch '(invalid|Invalid|semicolon|Test Run Failed|runsettings)') { throw 'Expected visible semicolon boundary failure.' }
      $xmlProject = Join-Path $root 'samples/TernTesting/XmlTests.proj'
      $xmlArguments = $arguments.Clone()
      $xmlArguments[1] = $xmlProject
      $xmlArguments[-1] = "-p:TernResults=$results-xml"
      $null = Invoke-Consumer $case.Dotnet $directory $xmlArguments
      [xml]$xmlTrx = Get-Content "$results-xml/settings.trx" -Raw
      if ($xmlTrx.TestRun.ResultSummary.Counters.total -ne '1' -or $xmlTrx.TestRun.ResultSummary.Counters.passed -ne '1') { throw 'XML semicolon alternative did not pass exactly once.' }
      foreach ($value in 'Harbour=North Quay','Pattern=^Tern\.Bay/West=13$','Literal=--listTests=not a switch','RejectSemicolon=east;west') { Assert-Text $xmlTrx.OuterXml "TERN_EFFECTIVE $value" }
    }
    Write-Host "PASS semicolon boundary $($case.Path): old escaped item retained; new scalar rejected; new XML alternative verified where applicable"
  }
  foreach ($capture in $captures) {
    if (($capture.Inputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $capture.Sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during verification.' }
    if ($Record) {
      $case = $capture.Case
      $recordValue = [ordered]@{
        kind='compile-and-run';sample=$capture.Sample;target=$case.Target;sdk=$case.Sdk;language=$case.Language
        command="eng/Test-TernTesting.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Record"
        scope='Real VSTest translation-layer discovery, pass/fail/repeat/cancel/cleanup; real extension dependency resolution; real custom task effective settings; actual old-contract compiler controls.'
        recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& $git -C $root rev-parse HEAD).Trim()
        sourceDirty=[bool](& $git -C $root status --porcelain -- samples eng/Test-TernTesting.ps1)
        inputs=$capture.Inputs;dependencies=$capture.Dependencies;workloads=@();nativeToolchains=@()
        host=[Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
        sdkInfo=$capture.Info;workloadInfo='No workload, native compiler, provider, rendering or device dependency. Windows x64 real local execution; not GA proof.'
        output=$log -join "`n"
      }
      Write-Lf (Join-Path (Split-Path (Join-Path $root $capture.Sample)) 'verification-evidence.json') (($recordValue | ConvertTo-Json -Depth 40) + "`n")
    }
  }
  Write-Host 'PASS: real paired VSTest host, extension loading, effective task settings and removed-contract compilation.'
}
finally {
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  Write-Host "Disposable commands/cache/results: $scratch"
}
