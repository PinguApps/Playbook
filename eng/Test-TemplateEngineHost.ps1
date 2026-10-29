[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-template-host-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source

function Invoke-Sdk([string]$Dotnet,[string]$Directory,[string[]]$Arguments,[string]$Lane,[int]$Exit = 0) {
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
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $scratch "home-$Lane"
  $start.Environment['NUGET_PACKAGES'] = Join-Path $scratch "cache-$Lane"
  $start.Environment['NUGET_HTTP_CACHE_PATH'] = Join-Path $scratch "http-$Lane"
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
    if (-not $process.WaitForExit(180000)) { $process.Kill($true); throw 'Template host command timed out.' }
    $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    $observed = ($output -split '\r?\n' | Where-Object { $_ -match 'NU[0-9]{4}|error |host=|engine=|archive=|Build succeeded|^[0-9]+\.[0-9]+\.' } | Select-Object -Unique) -join "`n"
    $log.Add("cwd=$Directory;cache=$($start.Environment['NUGET_PACKAGES'])`n$Dotnet $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$observed")
    [IO.File]::WriteAllText((Join-Path $scratch 'commands.log'),($log -join "`n"))
    if ($process.ExitCode -ne $Exit) { throw "Expected exit $Exit, got $($process.ExitCode): $output" }
    return $output
  }
  finally { $process.Dispose() }
}

function Assert-Text([string]$Actual,[string]$Expected) {
  if (-not $Actual.Contains($Expected,[StringComparison]::Ordinal)) { throw "Missing '$Expected': $Actual" }
}

function Read-Assets([string]$Directory,[string]$Target,[string]$Version,[string]$Asset,[string[]]$Ids) {
  $assets = Get-Content -LiteralPath (Join-Path $Directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
  foreach ($id in $Ids) {
    $selected = $assets.targets[$Target]["$id/$Version"]
    foreach ($kind in 'compile','runtime') {
      $entry = "lib/$Asset/$id.dll"
      if (@($selected[$kind].Keys) -cnotcontains $entry) { throw "Wrong $kind selection for $id on $Target" }
    }
    $log.Add("selected=$id/$Version;target=$Target;compile/runtime=lib/$Asset/$id.dll")
  }
  return $assets
}

function Save-Evidence([string]$Sample,[string]$Target,[string]$Sdk,[string]$Language,[string]$Kind,[string]$Info,[int]$First) {
  if (-not $Record) { return }
  $directory = Split-Path (Join-Path $root $Sample)
  $manifest = Get-Content -LiteralPath (Join-Path $directory 'verification.json') -Raw | ConvertFrom-Json -AsHashtable
  $record = [ordered]@{
    kind = $Kind; sample = $Sample; target = $Target; sdk = $Sdk; language = $Language
    command = "eng/Test-TemplateEngineHost.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Record"
    scope = if ($Kind -eq 'compile') { 'Historical netstandard host compilation, sixteen audited artifacts, eight old direct builds/eight strict current NU1202 restores and default NU1701 fallback control.' } else { 'Real embedded template creation and generated net10 application execution with independently selected compile/runtime dependency closure.' }
    recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); sourceCommit = (& $git -C $root rev-parse HEAD).Trim()
    sourceDirty = [bool](& $git -C $root status --porcelain -- samples eng/Test-TemplateEngineHost.ps1)
    inputs = Get-EvidenceInputs $root $Sample; dependencies = @($manifest.dependencies); workloads = @(); nativeToolchains = @()
    host = [Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    sdkInfo = $Info; workloadInfo = 'No workloads required or invoked.'
    output = ($log | Select-Object -Skip $First) -join "`n"
  }
  [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'),($record | ConvertTo-Json -Depth 30).Replace("`r`n","`n") + "`n")
}

try {
  # Reconcile the bounded original sources and every distributable, not just folder names.
  $sourceManifest = Get-Content (Join-Path $root 'docs/planning/audits/sdk-package-hosts.sources.json') -Raw | ConvertFrom-Json
  foreach ($source in ($sourceManifest.sources | Where-Object id -In 'note-template-engine-netstandard','template-edge-project')) {
    $file = Join-Path $scratch $source.id
    Invoke-WebRequest $source.url -OutFile $file
    if ((Get-FileHash $file).Hash.ToLowerInvariant() -cne $source.sha256) { throw "Source drift: $($source.id)" }
    $log.Add("source=$($source.url);sha256=$($source.sha256)")
  }
  $manifest = Get-Content (Join-Path $root 'docs/planning/audits/sdk-package-hosts.assets.json') -Raw | ConvertFrom-Json
  $packages = @($manifest.packages | Where-Object id -Like 'Microsoft.TemplateEngine*')
  if ($packages.Count -ne 16) { throw 'Expected sixteen bounded template ZIPs.' }
  foreach ($package in $packages) {
    $file = Join-Path $scratch "$($package.id).$($package.version).nupkg"
    Invoke-WebRequest $package.url -OutFile $file
    if ((Get-FileHash $file).Hash.ToLowerInvariant() -cne $package.sha256) { throw "Package drift: $($package.id)" }
    $zip = [IO.Compression.ZipFile]::OpenRead($file)
    try {
      $frameworks = @($zip.Entries.FullName | Where-Object { $_ -match '^lib/([^/]+)/[^/]+\.dll$' } | ForEach-Object { $_.Split('/')[1] } | Sort-Object -Unique)
      if (($frameworks -join ',') -cne ($package.libFrameworks -join ',')) { throw "Framework drift: $($package.id)" }
      $log.Add("artifact=$($package.id)/$($package.version);sha256=$($package.sha256);lib=$($frameworks -join ',')")
    }
    finally { $zip.Dispose() }
  }
  $ids = @($packages.id | Sort-Object -Unique)
  # Independent direct consumers: all eight historical routes compile; all eight new routes fail NU1202.
  foreach ($version in '10.0.402','11.0.100-rc.2.26504.105') {
    $lane = if ($version -eq '10.0.402') { 'old' } else { 'current10' }
    foreach ($id in $ids) {
      $directory = Join-Path $scratch "$lane-$id"
      $null = New-Item -ItemType Directory $directory
      Copy-Item (Join-Path $root 'samples/net10.0/ArchiveTemplateHost/global.json') $directory
      $configSample = if ($lane -eq 'old') { 'ArchiveTemplate' } else { 'net10.0/ArchiveTemplateHost' }
      Copy-Item (Join-Path $root "samples/$configSample/NuGet.Config") $directory
      $probe = (Get-Content (Join-Path $root 'samples/ArchiveTemplate/Probe.xml') -Raw).Replace('PACKAGE_ID',$id).Replace('PACKAGE_VERSION',$version)
      [IO.File]::WriteAllText((Join-Path $directory 'Probe.csproj'),$probe)
      'public class ArchivePluginMarker { }' | Set-Content (Join-Path $directory 'Marker.cs')
      if ($lane -eq 'old') {
        $null = Invoke-Sdk $Net10Dotnet $directory @('build','Probe.csproj','-c','Release','-p:NuGetAudit=false') $lane
        $null = Read-Assets $directory 'netstandard2.0' $version 'netstandard2.0' @($id)
      }
      else {
        if ($id -eq 'Microsoft.TemplateEngine.Abstractions') {
          $fallback = Invoke-Sdk $Net10Dotnet $directory @('restore','Probe.csproj','-p:NuGetAudit=false','-p:DisableImplicitAssetTargetFallback=false') $lane
          Assert-Text $fallback 'NU1701'
          $null = Read-Assets $directory 'netstandard2.0' $version 'net472' @($id)
          $log.Add('Default implicit Framework fallback emits NU1701; restore success is not netstandard compatibility. Strict probes disable that fallback.')
        }
        $output = Invoke-Sdk $Net10Dotnet $directory @('restore','Probe.csproj','-p:NuGetAudit=false') $lane 1
        Assert-Text $output "NU1202: Package $id $version is not compatible with netstandard2.0"
      }
    }
  }
  $proofPrefix = $log.ToArray()
  foreach ($case in @(
    @{ Path='ArchiveTemplate'; Target='netstandard2.0'; Version='10.0.402'; Asset='netstandard2.0'; Lane='old'; Sdk='10.0.402'; Language='14.0'; Kind='compile' },
    @{ Path='net10.0/ArchiveTemplateRunner'; Target='net10.0'; Version='10.0.402'; Asset='net10.0'; Lane='old'; Sdk='10.0.402'; Language='14.0'; Kind='compile-and-run' },
    @{ Path='net10.0/ArchiveTemplateHost'; Target='net10.0'; Version='11.0.100-rc.2.26504.105'; Asset='net10.0'; Lane='current10'; Sdk='10.0.402'; Language='14.0'; Kind='compile-and-run' },
    @{ Path='net11.0/ArchiveTemplateHost'; Target='net11.0'; Version='11.0.100-rc.2.26504.105'; Asset='net11.0'; Lane='current11'; Sdk='11.0.100-rc.2.26504.105'; Language='15.0'; Kind='compile-and-run' }
  )) {
    $first = $log.Count
    if ($case.Kind -eq 'compile') { foreach ($line in $proofPrefix) { $log.Add($line) } }
    $dotnet = if ($case.Lane -eq 'current11') { $Net11Dotnet } else { $Net10Dotnet }
    $directory = Join-Path $root "samples/$($case.Path)"
    $name = if ($case.Kind -eq 'compile') { 'ArchiveTemplateLegacy' } else { Split-Path $directory -Leaf }
    $sample = "samples/$($case.Path)/$name.csproj"
    Assert-Text (Invoke-Sdk $dotnet $directory @('--version') $case.Lane) $case.Sdk
    $info = Invoke-Sdk $dotnet $directory @('--info') $case.Lane
    $restore = @('restore',"$name.csproj",'-p:NuGetAudit=false')
    if (Test-Path (Join-Path $directory 'packages.lock.json')) { $restore += '--locked-mode' }
    $null = Invoke-Sdk $dotnet $directory $restore $case.Lane
    $assets = Read-Assets $directory $case.Target $case.Version $case.Asset @('Microsoft.TemplateEngine.Edge','Microsoft.TemplateEngine.Orchestrator.RunnableProjects')
    $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
    $verificationPath = Join-Path $directory 'verification.json'
    $verification = Get-Content $verificationPath -Raw | ConvertFrom-Json -AsHashtable
    if ($Record) {
      $verification.dependencies = $dependencies
      [IO.File]::WriteAllText($verificationPath,($verification | ConvertTo-Json -Depth 10).Replace("`r`n","`n") + "`n")
    }
    if (($dependencies -join ',') -cne ($verification.dependencies -join ',')) { throw "Dependency drift: $sample" }
    $before = Get-EvidenceInputs $root $sample
    $null = Invoke-Sdk $dotnet $directory @('build',"$name.csproj",'-c','Release','--no-restore') $case.Lane
    foreach ($dependency in $assets.targets[$case.Target].GetEnumerator()) {
      $selected = $dependency.Value
      $compile = if ($selected.ContainsKey('compile')) { $selected.compile.Keys -join ',' } else { '' }
      $runtime = if ($selected.ContainsKey('runtime')) { $selected.runtime.Keys -join ',' } else { '' }
      $hash = if ($assets.libraries[$dependency.Key].ContainsKey('sha512')) { $assets.libraries[$dependency.Key].sha512 } else { 'project-reference' }
      $runtimeTargets = if ($selected.ContainsKey('runtimeTargets')) { $selected.runtimeTargets | ConvertTo-Json -Compress -Depth 5 } else { 'none' }
      $log.Add("closure=$($dependency.Key);compile=$compile;runtime=$runtime;runtimeTargets=$runtimeTargets;sha512=$hash")
      if ($dependency.Key -like 'Microsoft.TemplateEngine.*') {
        $id,$version = $dependency.Key.Split('/')
        $expected = $packages | Where-Object { $_.id -ceq $id -and $_.version -ceq $version }
        $archive = Join-Path $scratch "cache-$($case.Lane)/$($id.ToLowerInvariant())/$version/$($id.ToLowerInvariant()).$version.nupkg"
        if ((Get-FileHash $archive).Hash.ToLowerInvariant() -cne $expected.sha256) { throw "Restored artifact differs from audit: $id/$version" }
      }
    }
    if ($case.Kind -ne 'compile') {
      $workspace = Join-Path $scratch "$($case.Lane)-creation"
      $output = Invoke-Sdk $dotnet $directory @("bin/Release/$($case.Target)/$name.dll",$workspace) $case.Lane
      Assert-Text $output 'generated=net10.0;collection=MAPS-1932'
      $generated = Join-Path $workspace 'generated'
      Copy-Item (Join-Path $root 'samples/net10.0/ArchiveTemplateHost/global.json') $generated
      # Build/run output with SDK/runtime 10 even after the net11 host created it.
      $null = Invoke-Sdk $Net10Dotnet $generated @('build','CwmArchive.csproj','-c','Release') 'generated'
      Assert-Text (Invoke-Sdk $Net10Dotnet $generated @('bin/Release/net10.0/CwmArchive.dll') 'generated') 'archive=CwmArchive;collection=MAPS-1932'
      if (Test-Path (Join-Path $generated '.template.config')) { throw 'Template configuration leaked into output.' }
    }
    if (($before | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
    Save-Evidence $sample $case.Target $case.Sdk $case.Language $case.Kind $info $first
    Write-Host "PASS: $sample; $($case.Kind), selected dependency closure and original template."
  }
  Write-Host 'PASS: sixteen audited ZIPs, eight old netstandard builds/eight new NU1202 restores, legacy netstandard execution and independent current net10/net11 creation; generated net10 applications built/executed with SDK/runtime 10.'
}
finally {
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  Write-Host "Disposable commands/caches: $scratch"
}
