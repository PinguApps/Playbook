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
$scratch = Join-Path $root ('artifacts/json-dependencies/run-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source

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
  $start.Environment['VSTEST_DOTNET_HOST_PATH'] = $Dotnet
  $process = [Diagnostics.Process]::Start($start)
  try {
    $process.StandardInput.Close()
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(180000)) { $process.Kill($true); throw 'Weather consumer timed out.' }
    $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    $log.Add("cwd=$Directory;cache=$($start.Environment['NUGET_PACKAGES'])`n$Dotnet $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
    [IO.File]::WriteAllText((Join-Path $scratch 'commands.log'),($log -join "`n"))
    if ($process.ExitCode -ne $Exit) { throw "Expected exit $Exit, got $($process.ExitCode): $output" }
    return $output
  }
  finally { $process.Dispose() }
}

function Assert-Text([string]$Actual,[string]$Expected) {
  if (-not $Actual.Contains($Expected,[StringComparison]::Ordinal)) { throw "Missing '$Expected': $Actual" }
}

function Read-Closure([string]$Directory,[string]$Target) {
  $assets = Get-Content (Join-Path $Directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
  foreach ($dependency in $assets.targets[$Target].GetEnumerator()) {
    $value = $dependency.Value
    $compile = if ($value.ContainsKey('compile')) { $value.compile.Keys -join ',' } else { 'none' }
    $runtime = if ($value.ContainsKey('runtime')) { $value.runtime.Keys -join ',' } else { 'none' }
    $native = if ($value.ContainsKey('runtimeTargets')) { $value.runtimeTargets | ConvertTo-Json -Depth 5 -Compress } else { 'none' }
    $log.Add("resolved=$($dependency.Key);compile=$compile;runtime=$runtime;runtimeTargets=$native;sha512=$($assets.libraries[$dependency.Key].sha512)")
  }
  return $assets
}

function Copy-Probe([string]$Source,[string]$Name) {
  $destination = Join-Path $scratch $Name
  $null = New-Item -ItemType Directory $destination
  foreach ($file in Get-ChildItem $Source -File) {
    if ($file.Name -notin 'verification-evidence.json','verification.json','packages.lock.json') { Copy-Item $file.FullName $destination }
  }
  Copy-Item (Join-Path $root 'samples/Directory.Build.props') $destination
  Copy-Item (Join-Path $root 'samples/Directory.Build.targets') $destination
  Copy-Item (Join-Path $root 'NuGet.Config') $destination
  foreach ($project in Get-ChildItem $destination -Filter '*.csproj') {
    $text = (Get-Content $project.FullName -Raw).Replace('../../WeatherJson/', ($root.Replace('\','/') + '/samples/WeatherJson/'))
    [IO.File]::WriteAllText($project.FullName,$text)
  }
  return $destination
}

function Write-CollectorSettings([string]$Extension,[string]$Name) {
  $file = Join-Path $scratch "$Name.runsettings"
  $codebase = [Security.SecurityElement]::Escape((Join-Path $Extension 'WeatherStation.Collector.dll'))
  (Get-Content (Join-Path $root 'samples/WeatherJson/Collector.xml') -Raw).Replace('COLLECTOR_DLL',$codebase) | Set-Content $file
  return $file
}

function Save-Record($Case,[string]$Info,[string[]]$Dependencies,[int]$First) {
  if (-not $Record) { return }
  $sample = "samples/$($Case.Path)/$($Case.Name).csproj"
  $directory = Split-Path (Join-Path $root $sample)
  $record = [ordered]@{
    kind='compile-and-run'; sample=$sample; target=$Case.Target; sdk=$Case.Sdk; language=$Case.Language
    command="eng/Test-JsonDependencies.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Record"
    scope='Real pinned VSTest runner/test-host execution, original weather JSON tests and managed adapter/collector loading; dependency, compile and missing-runtime controls as recorded.'
    recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& $git -C $root rev-parse HEAD).Trim()
    sourceDirty=[bool](& $git -C $root status --porcelain -- samples eng/Test-JsonDependencies.ps1)
    inputs=Get-EvidenceInputs $root $sample; dependencies=$Dependencies; workloads=@(); nativeToolchains=@()
    host=[Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    sdkInfo=$Info; workloadInfo='No workloads or native toolchains invoked. Microsoft.CodeCoverage is a Test.Sdk dependency; native coverage collection is not enabled.'
    output=($log | Select-Object -Skip $First) -join "`n"
  }
  [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'),($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
}

try {
  $manifest = Get-Content (Join-Path $root 'docs/planning/audits/sdk-package-hosts.assets.json') -Raw | ConvertFrom-Json
  $packages = @($manifest.packages | Where-Object id -In 'Microsoft.NET.Test.Sdk','Microsoft.TestPlatform.TestHost')
  if ($packages.Count -ne 4) { throw 'Expected exactly four audited test packages.' }
  foreach ($package in $packages) {
    $file = Join-Path $scratch "$($package.id).$($package.version).nupkg"
    Invoke-WebRequest $package.url -OutFile $file
    if ((Get-FileHash $file).Hash.ToLowerInvariant() -cne $package.sha256) { throw 'Audited test package drift.' }
    $log.Add("audited=$($package.id)/$($package.version);sha256=$($package.sha256);lib=$($package.libFrameworks -join ',')")
  }
  $sources = Get-Content (Join-Path $root 'docs/planning/audits/sdk-package-hosts.sources.json') -Raw | ConvertFrom-Json
  foreach ($source in ($sources.sources | Where-Object url -Like '*vstest-removes-newtonsoft-json.md')) {
    $file = Join-Path $scratch 'vstest-note.md'
    Invoke-WebRequest $source.url -OutFile $file
    if ((Get-FileHash $file).Hash.ToLowerInvariant() -cne $source.sha256) { throw 'Audited VSTest note drift.' }
    $log.Add("source=$($source.url);sha256=$($source.sha256)")
  }
  $runners = @{}
  foreach ($version in '18.7.0','18.11.0') {
    $archive = Join-Path $scratch "runner-$version.nupkg"
    Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/microsoft.testplatform.cli/$version/microsoft.testplatform.cli.$version.nupkg" -OutFile $archive
    $directory = Join-Path $scratch "runner-$version"
    [IO.Compression.ZipFile]::ExtractToDirectory($archive,$directory)
    $runner = @(Get-ChildItem $directory -Recurse -Filter 'vstest.console.dll')
    if ($runner.Count -ne 1) { throw 'Expected one actual managed runner.' }
    $runners[$version] = $runner[0].FullName
    $log.Add("runner=Microsoft.TestPlatform.CLI/$version;sha256=$((Get-FileHash $archive).Hash.ToLowerInvariant());entry=$([IO.Path]::GetRelativePath($directory,$runner[0].FullName));newtonsoft=$([bool](Test-Path (Join-Path $runner[0].DirectoryName 'Newtonsoft.Json.dll')))")
  }
  $sdkRoot = Join-Path (Split-Path $Net11Dotnet) 'sdk/11.0.100-rc.2.26504.105'
  foreach ($file in $manifest.inspectedSdk.files) {
    $actual = Join-Path $sdkRoot $file.file
    if ((Get-FileHash $actual).Hash.ToLowerInvariant() -cne $file.sha256) { throw "Audited SDK file drift: $($file.file)" }
    $log.Add("sdk-root=$($file.file);sha256=$($file.sha256)")
  }
  foreach ($name in 'dotnet','vstest.console','testhost') {
    $deps = Get-Content (Join-Path $sdkRoot "$name.deps.json") -Raw | ConvertFrom-Json -AsHashtable
    $json = @($deps.libraries.Keys | Where-Object { $_ -like 'Newtonsoft.Json/*' })
    if (($name -eq 'dotnet' -and ($json -join ',') -cne 'Newtonsoft.Json/13.0.4') -or ($name -ne 'dotnet' -and $json.Count -ne 0)) { throw 'Unexpected SDK-root JSON dependency.' }
    $log.Add("sdk-root=$name.deps.json;newtonsoft=$($json -join ',');not-a-project-reference=true")
  }
  $prefix = $log.ToArray()
  $cases = @(
    @{Path='net10.0/WeatherExtension';Name='WeatherExtension';Target='net10.0';Sdk='10.0.402';Language='14.0';Version='18.7.0';Extension=$true},
    @{Path='net10.0/WeatherCollector';Name='WeatherCollector';Target='net10.0';Sdk='10.0.402';Language='14.0';Version='18.7.0';Extension=$true},
    @{Path='net11.0/WeatherCollector';Name='WeatherCollector';Target='net11.0';Sdk='11.0.100-rc.2.26504.105';Language='15.0';Version='18.11.0';Extension=$true},
    @{Path='net11.0/WeatherExtension';Name='WeatherExtension';Target='net11.0';Sdk='11.0.100-rc.2.26504.105';Language='15.0';Version='18.11.0';Extension=$true},
    @{Path='net10.0/WeatherTestsLegacy';Name='WeatherTestsLegacy';Target='net10.0';Sdk='10.0.402';Language='14.0';Version='18.7.0';Extension=$false},
    @{Path='net10.0/WeatherTests';Name='WeatherTests';Target='net10.0';Sdk='10.0.402';Language='14.0';Version='18.11.0';Extension=$false},
    @{Path='net11.0/WeatherTests';Name='WeatherTests';Target='net11.0';Sdk='11.0.100-rc.2.26504.105';Language='15.0';Version='18.11.0';Extension=$false}
  )
  $captures = @()
  foreach ($case in $cases) {
    $first = $log.Count
    foreach ($line in $prefix) { $log.Add($line) }
    $dotnet = if ($case.Target -eq 'net11.0') { $Net11Dotnet } else { $Net10Dotnet }
    $directory = Join-Path $root "samples/$($case.Path)"
    Assert-Text (Invoke-Consumer $dotnet $directory @('--version')) $case.Sdk
    $info = Invoke-Consumer $dotnet $directory @('--info')
    $restore = @('restore',"$($case.Name).csproj")
    if (Test-Path (Join-Path $directory 'packages.lock.json')) { $restore += '--locked-mode' }
    $null = Invoke-Consumer $dotnet $directory $restore
    $assets = Read-Closure $directory $case.Target
    $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
    foreach ($package in ($packages | Where-Object { $_.version -ceq $case.Version })) {
      if ($assets.libraries.ContainsKey("$($package.id)/$($package.version)")) {
        $id = $package.id.ToLowerInvariant()
        $archive = Join-Path $scratch "cache/$id/$($package.version)/$id.$($package.version).nupkg"
        if ((Get-FileHash $archive).Hash.ToLowerInvariant() -cne $package.sha256) { throw 'Restored artifact differs from audited ZIP.' }
      }
    }
    $verificationFile = Join-Path $directory 'verification.json'
    $verification = Get-Content $verificationFile -Raw | ConvertFrom-Json -AsHashtable
    if ($Record) {
      $verification.dependencies = $dependencies
      [IO.File]::WriteAllText($verificationFile,($verification | ConvertTo-Json -Depth 10).Replace("`r`n","`n") + "`n")
    }
    if (($dependencies -join ',') -cne ($verification.dependencies -join ',')) { throw 'Resolved dependencies changed.' }
    $inputs = Get-EvidenceInputs $root "samples/$($case.Path)/$($case.Name).csproj"
    $null = Invoke-Consumer $dotnet $directory @('build',"$($case.Name).csproj",'-c','Release','--no-restore')
    $captures += @{Case=$case;Info=$info;Dependencies=$dependencies;First=$first;Inputs=$inputs}
  }
  foreach ($capture in $captures) {
    $case = $capture.Case
    if ($case.Extension) { continue }
    $dotnet = if ($case.Target -eq 'net11.0') { $Net11Dotnet } else { $Net10Dotnet }
    $directory = Join-Path $root "samples/$($case.Path)"
    $assembly = Join-Path $directory "bin/Release/$($case.Target)/WeatherTests.dll"
    $extension = Join-Path $root "samples/$($case.Target)/WeatherExtension/bin/Release/$($case.Target)"
    # Deploy the adapter beside its test source. The collector has an explicit codebase.
    Copy-Item (Join-Path $extension 'WeatherStation.TestAdapter.dll') (Split-Path $assembly)
    $results = Join-Path $scratch ($case.Path.Replace('/','-'))
    $collector = Join-Path $root "samples/$($case.Target)/WeatherCollector/bin/Release/$($case.Target)"
    $settings = Write-CollectorSettings $collector $case.Path.Replace('/','-')
    $discovery = Invoke-Consumer $dotnet $scratch @($runners[$case.Version],$assembly,'/ListTests','/Logger:console;verbosity=detailed')
    Assert-Text $discovery 'weather-adapter=loaded;schema=2'
    Assert-Text $discovery 'WeatherStation.WeatherChecks.RainfallTotal'
    $arguments = @($runners[$case.Version],$assembly,"/TestAdapterPath:$collector","/Settings:$settings",'/Collect:WeatherStation',"/ResultsDirectory:$results",'/Logger:trx;LogFileName=weather.trx','/Logger:console;verbosity=detailed')
    $output = Invoke-Consumer $dotnet $scratch $arguments
    Assert-Text $output 'weather-collector=loaded;sensor=rain-gauge'
    Assert-Text $output 'weather-test=Gull Point;rainfall=5.0;runtime='
    [xml]$trx = Get-Content (Join-Path $results 'weather.trx')
    if ($trx.TestRun.ResultSummary.Counters.total -ne '1' -or $trx.TestRun.ResultSummary.Counters.passed -ne '1') { throw 'Expected exactly one actual passing weather test.' }
    Write-Host "PASS: $($case.Path), actual VSTest $($case.Version) test plus adapter/collector."
  }
  # The current package also supports an older target: the preceding net10 case ran it.
  # Missing direct dependency: update only Test.Sdk/TestHost, keep the original JSON source.
  $probe = Copy-Probe (Join-Path $root 'samples/net10.0/WeatherTestsLegacy') 'no-direct-json'
  $central = Join-Path $probe 'Directory.Packages.props'
  (Get-Content $central -Raw).Replace('[18.7.0]','[18.11.0]') | Set-Content $central
  $output = Invoke-Consumer $Net10Dotnet $probe @('build','WeatherTestsLegacy.csproj','-c','Release') 1
  Assert-Text $output 'CS0246'
  Assert-Text $output 'Newtonsoft'
  $assets = Read-Closure $probe 'net10.0'
  if (@($assets.libraries.Keys | Where-Object { $_ -like 'Newtonsoft.Json/*' }).Count -ne 0) { throw 'Unexpected transitive JSON dependency.' }
  # Compile-only reference, with no stale output/cache file to conceal missing runtime assets.
  $probe = Copy-Probe (Join-Path $root 'samples/net11.0/WeatherTests') 'no-test-runtime'
  $project = Join-Path $probe 'WeatherTests.csproj'
  (Get-Content $project -Raw).Replace('Include="Newtonsoft.Json" />','Include="Newtonsoft.Json" ExcludeAssets="runtime" />') | Set-Content $project
  $null = Invoke-Consumer $Net11Dotnet $probe @('build','WeatherTests.csproj','-c','Release')
  $assembly = Join-Path $probe 'bin/Release/net11.0/WeatherTests.dll'
  if (Test-Path (Join-Path (Split-Path $assembly) 'Newtonsoft.Json.dll')) { throw 'Runtime exclusion did not remove JSON.' }
  # Adapter discovery uses its own dependency; the test host must still own the test dependency.
  $extension = Join-Path $root 'samples/net11.0/WeatherExtension/bin/Release/net11.0'
  Copy-Item (Join-Path $extension 'WeatherStation.TestAdapter.dll') (Split-Path $assembly)
  $output = Invoke-Consumer $Net11Dotnet $scratch @($runners['18.11.0'],$assembly,'/Logger:console;verbosity=detailed') 1
  Assert-Text $output 'FileNotFoundException'
  Assert-Text $output 'Newtonsoft.Json'
  # Separate discovery/collector failure: no test execution can mask the extension load defect.
  $probe = Copy-Probe (Join-Path $root 'samples/net11.0/WeatherExtension') 'no-extension-runtime'
  $project = Join-Path $probe 'WeatherExtension.csproj'
  (Get-Content $project -Raw).Replace('Include="Newtonsoft.Json" />','Include="Newtonsoft.Json" ExcludeAssets="runtime" />') | Set-Content $project
  $null = Invoke-Consumer $Net11Dotnet $probe @('build','WeatherExtension.csproj','-c','Release')
  $extension = Join-Path $probe 'bin/Release/net11.0'
  if (Test-Path (Join-Path $extension 'Newtonsoft.Json.dll')) { throw 'Extension runtime exclusion did not remove JSON.' }
  Copy-Item (Join-Path $extension 'WeatherStation.TestAdapter.dll') (Split-Path $assembly)
  $output = Invoke-Consumer $Net11Dotnet $scratch @($runners['18.11.0'],$assembly,'/ListTests','/Logger:console;verbosity=detailed')
  Assert-Text $output 'Newtonsoft.Json'
  if ($output.Contains('WeatherStation.WeatherChecks.RainfallTotal')) { throw 'Broken adapter unexpectedly discovered the weather test.' }
  $collectorProbe = Copy-Probe (Join-Path $root 'samples/net11.0/WeatherCollector') 'no-collector-runtime'
  $project = Join-Path $collectorProbe 'WeatherCollector.csproj'
  (Get-Content $project -Raw).Replace('Include="Newtonsoft.Json" />','Include="Newtonsoft.Json" ExcludeAssets="runtime" />') | Set-Content $project
  $null = Invoke-Consumer $Net11Dotnet $collectorProbe @('build','WeatherCollector.csproj','-c','Release')
  $collector = Join-Path $collectorProbe 'bin/Release/net11.0'
  if (Test-Path (Join-Path $collector 'Newtonsoft.Json.dll')) { throw 'Collector runtime exclusion did not remove JSON.' }
  $settings = Write-CollectorSettings $collector 'no-collector-runtime'
  $output = Invoke-Consumer $Net11Dotnet $scratch @($runners['18.11.0'],$assembly,"/TestAdapterPath:$collector","/Settings:$settings",'/Collect:WeatherStation','/Logger:console;verbosity=detailed') 1
  Assert-Text $output "Data collector 'WeatherStation'"
  Assert-Text $output 'Newtonsoft.Json'
  $log.Add('Missing extension runtime assets: real discovery/collector diagnostics asserted even though VSTest returns zero with no tests; zero exit is not a passing test.')
  foreach ($capture in $captures) {
    $sample = "samples/$($capture.Case.Path)/$($capture.Case.Name).csproj"
    if (($capture.Inputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during verification.' }
    Save-Record $capture.Case $capture.Info $capture.Dependencies $capture.First
  }
  Write-Host 'PASS: four audited ZIPs; actual 18.7/18.11 consumers; explicit JSON net10/net11 execution; adapter/collector loading; missing-direct compile and missing-test/extension-runtime controls.'
}
finally {
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  Write-Host "Disposable commands/cache/results: $scratch"
}
