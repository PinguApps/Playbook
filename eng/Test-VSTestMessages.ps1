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
$scratch = Join-Path $root ('artifacts/vstest-messages/run-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory $scratch
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()
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
  $process = [Diagnostics.Process]::Start($start)
  try {
    $process.StandardInput.Close()
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(180000)) { $process.Kill($true); throw 'Laboratory consumer timed out.' }
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

try {
  $manifest = Get-Content (Join-Path $root 'docs/planning/audits/sdk-package-hosts.assets.json') -Raw | ConvertFrom-Json
  $packages = @($manifest.packages | Where-Object id -EQ 'Microsoft.TestPlatform.TestHost')
  if ($packages.Count -ne 2) { throw 'Expected exactly two audited TestHost packages.' }
  foreach ($package in $packages) {
    $file = Join-Path $scratch "$($package.id).$($package.version).nupkg"
    Invoke-WebRequest $package.url -OutFile $file
    if ((Get-FileHash $file).Hash.ToLowerInvariant() -cne $package.sha256) { throw 'Audited package drift.' }
    $log.Add("audited=$($package.id)/$($package.version);sha256=$($package.sha256);lib=$($package.libFrameworks -join ',')")
    $zip = [IO.Compression.ZipFile]::OpenRead($file)
    try {
      $reader = [IO.StreamReader]::new($zip.GetEntry('README.md').Open())
      try { $readme = $reader.ReadToEnd() } finally { $reader.Dispose() }
      Assert-Text $readme 'advanced hosting scenarios'
      $log.Add("supported-acquisition=$($package.id)/$($package.version);README.md: direct references for advanced hosting scenarios;complete-package-closure-deployed")
    }
    finally { $zip.Dispose() }
  }
  $sources = Get-Content (Join-Path $root 'docs/planning/audits/sdk-package-hosts.sources.json') -Raw | ConvertFrom-Json
  $selectedSources = @($sources.sources | Where-Object id -In 'note-vstest-removes-newtonsoft-json','vstest11-message','vstest11-serializer')
  if ($selectedSources.Count -ne 3) { throw 'Expected all three audited VSTest sources.' }
  foreach ($source in $selectedSources) {
    $file = Join-Path $scratch "$($source.id).txt"
    Invoke-WebRequest $source.url -OutFile $file
    if ((Get-FileHash $file).Hash.ToLowerInvariant() -cne $source.sha256) { throw 'Audited source drift.' }
    $log.Add("source=$($source.url);sha256=$($source.sha256)")
  }
  $cases = @(
    @{Path='LaboratoryMessages';Target='net10.0';Sdk='10.0.402';Language='14.0';Version='18.7.0';Dotnet=$Net10Dotnet;Name='old10'},
    @{Path='net10.0/LaboratoryBridge';Target='net10.0';Sdk='10.0.402';Language='14.0';Version='18.11.0';Dotnet=$Net10Dotnet;Name='current10'},
    @{Path='net11.0/LaboratoryBridge';Target='net11.0';Sdk='11.0.100-rc.2.26504.105';Language='15.0';Version='18.11.0';Dotnet=$Net11Dotnet;Name='current11'}
  )
  $captures = @()
  foreach ($case in $cases) {
    $directory = Join-Path $root "samples/$($case.Path)"
    Assert-Text (Invoke-Consumer $case.Dotnet $directory @('--version')) $case.Sdk
    $info = Invoke-Consumer $case.Dotnet $directory @('--info')
    $compiler = Join-Path (Split-Path $case.Dotnet) "sdk/$($case.Sdk)/Roslyn/bincore/csc.dll"
    $null = Invoke-Consumer $case.Dotnet $directory @($compiler,'-version')
    $restore = @('restore','LaboratoryBridge.csproj')
    if (Test-Path (Join-Path $directory 'packages.lock.json')) { $restore += '--locked-mode' }
    $null = Invoke-Consumer $case.Dotnet $directory $restore
    $lock = Join-Path $directory 'packages.lock.json'
    if ($Record) { [IO.File]::WriteAllText($lock,(Get-Content $lock -Raw).Replace("`r`n","`n")) }
    $assets = Get-Content (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
    $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
    foreach ($dependency in $assets.targets[$case.Target].GetEnumerator()) {
      $log.Add("lane=$($case.Name);resolved=$($dependency.Key);sha512=$($assets.libraries[$dependency.Key].sha512);selected=$($dependency.Value | ConvertTo-Json -Depth 12 -Compress)")
    }
    $package = $packages | Where-Object version -CEQ $case.Version
    $restored = Join-Path $scratch "cache/microsoft.testplatform.testhost/$($case.Version)/microsoft.testplatform.testhost.$($case.Version).nupkg"
    if ((Get-FileHash $restored).Hash.ToLowerInvariant() -cne $package.sha256) { throw 'Restored package differs from audited ZIP.' }
    $manifestPath = Join-Path $directory 'verification.json'
    if ($Record) {
      $verification = Get-Content $manifestPath -Raw | ConvertFrom-Json
      $verification.dependencies = $dependencies
      [IO.File]::WriteAllText($manifestPath,($verification | ConvertTo-Json -Depth 12).Replace("`r`n","`n") + "`n")
    }
    $inputs = Get-EvidenceInputs $root "samples/$($case.Path)/LaboratoryBridge.csproj"
    $null = Invoke-Consumer $case.Dotnet $directory @('build','LaboratoryBridge.csproj','-c','Release','--no-restore')
    $deploy = Join-Path $scratch "deploy-$($case.Name)"
    $null = Invoke-Consumer $case.Dotnet $directory @('publish','LaboratoryBridge.csproj','-c','Release','--no-build','--no-restore','--output',$deploy)
    $entry = $package.entries[0]
    if ((Get-FileHash (Join-Path $deploy 'Microsoft.TestPlatform.CommunicationUtilities.dll')).Hash.ToLowerInvariant() -cne $entry.sha256) { throw 'Published communication bytes differ from audit.' }
    $log.Add("lane=$($case.Name);communication=$($entry.path);sha256=$($entry.sha256);deployed=$deploy")
    $wire = Join-Path $scratch $case.Name
    $output = Invoke-Consumer $case.Dotnet $scratch @((Join-Path $deploy 'LaboratoryBridge.dll'),'--write',$wire)
    Assert-Text $output 'PASS laboratory-bridge'
    foreach ($version in 1,2) { $log.Add("input=$($case.Name)-v$version.json;wire=$([IO.File]::ReadAllText("$wire-v$version.json"))") }
    $captures += @{Case=$case;Info=$info;Dependencies=$dependencies;Inputs=$inputs;Deploy=$deploy;Wire=$wire}
  }
  # Both serialization directions, each real dependency set, both original protocols.
  foreach ($capture in $captures) {
    $foreign = if ($capture.Case.Name -ceq 'old10') { $captures[2] } else { $captures[0] }
    foreach ($version in 1,2) {
      Assert-Text (Invoke-Consumer $capture.Case.Dotnet $scratch @((Join-Path $capture.Deploy 'LaboratoryBridge.dll'),'--read',"$($foreign.Wire)-v$version.json")) 'PASS foreign-wire'
    }
  }
  # Compile every removed symbol independently. A direct Newtonsoft reference is
  # present in BOTH lanes; it cannot bring the removed platform contracts back.
  $probe = Join-Path $scratch 'compile-controls'
  $null = New-Item -ItemType Directory $probe
  foreach ($file in 'Directory.Build.props','Directory.Build.targets') { Copy-Item (Join-Path $root "samples/$file") $probe }
  Copy-Item (Join-Path $root 'NuGet.Config') $probe
  Copy-Item (Join-Path $root 'samples/net10.0/LaboratoryBridge/global.json') $probe
  '<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>' | Set-Content (Join-Path $probe 'Directory.Packages.props')
  $symbols = @('Message.Payload','VersionedMessage','DefaultTestPlatformContractResolver','TestCaseConverter','TestObjectConverter','TestPlatformContractResolver1','TestResultConverter','TestRunStatisticsConverter')
  foreach ($version in '18.7.0','18.11.0') {
    @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Library</OutputType></PropertyGroup><ItemGroup><PackageReference Include="Microsoft.TestPlatform.TestHost" Version="[$version]"/><PackageReference Include="Newtonsoft.Json" Version="[13.0.4]"/></ItemGroup></Project>
"@ | Set-Content (Join-Path $probe 'Probe.csproj')
    foreach ($symbol in $symbols) {
      $member = if ($symbol -ceq 'Message.Payload') { 'public static object? Read(Message message) => message.Payload;' } else { "public static Type Read() => typeof($symbol);" }
      "using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;`nusing Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;`nnamespace CwmLab;`npublic static class Probe { $member }" | Set-Content (Join-Path $probe 'Probe.cs')
      $exit = if ($version -ceq '18.7.0') { 0 } else { 1 }
      $output = Invoke-Consumer $Net10Dotnet $probe @('build','Probe.csproj','-c','Release') $exit
      if ($exit -ne 0 -and $output -notmatch 'error CS(1061|0246|0122)') { throw "Unexpected removed-symbol failure: $output" }
      $log.Add("compile-control=$symbol;TestHost=$version;direct-Newtonsoft=13.0.4;exit=$exit")
    }
  }
  # Retain an actual old executable and its Newtonsoft dependency, replace only
  # the complete matching platform assembly set, then observe ABI failure.
  $mixed = Join-Path $scratch 'old-binary-new-platform'
  Copy-Item $captures[0].Deploy $mixed -Recurse
  Get-ChildItem $captures[1].Deploy -Filter 'Microsoft.*.dll' | Copy-Item -Destination $mixed
  if (-not (Test-Path (Join-Path $mixed 'Newtonsoft.Json.dll'))) { throw 'ABI control must retain Newtonsoft.' }
  $output = Invoke-Consumer $Net10Dotnet $scratch @((Join-Path $mixed 'LaboratoryBridge.dll')) -532462766
  if ($output -notmatch '(TypeLoadException|MissingMethodException)') { throw "Unexpected ABI control: $output" }
  $log.Add('PASS old-binary-new-platform: removed type/member failure despite deployed Newtonsoft; rebuild required, no ABI repair claimed.')
  foreach ($capture in $captures) {
    $case = $capture.Case
    $sample = "samples/$($case.Path)/LaboratoryBridge.csproj"
    if (($capture.Inputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during verification.' }
    if ($Record) {
      $evidenceRecord = [ordered]@{
        kind='compile-and-run';sample=$sample;target=$case.Target;sdk=$case.Sdk;language=$case.Language
        command="eng/Test-VSTestMessages.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Record"
        scope='Actual old/new TestHost deployment and typed laboratory messages; protocols, foreign wire, null/malformed failures, every removed symbol, retained serializer members and ABI control.'
        recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& $git -C $root rev-parse HEAD).Trim()
        sourceDirty=[bool](& $git -C $root status --porcelain -- samples eng/Test-VSTestMessages.ps1)
        inputs=$capture.Inputs;dependencies=$capture.Dependencies;workloads=@();nativeToolchains=@()
        host=[Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
        sdkInfo=$capture.Info;workloadInfo='No workloads, native tooling, VSTest transport, test execution or extension injection invoked; independent framework-dependent advanced-host consumer.'
        output=$log -join "`n"
      }
      [IO.File]::WriteAllText((Join-Path (Split-Path (Join-Path $root $sample)) 'verification-evidence.json'),($evidenceRecord | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
    }
  }
  Write-Host 'PASS: audited TestHost 18.7/18.11 deployment; independent net10/net11 typed consumers; bidirectional protocol1/2 wire; failure, full removed-surface and real ABI controls.'
}
finally {
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  Write-Host "Disposable commands/cache/results: $scratch"
}

