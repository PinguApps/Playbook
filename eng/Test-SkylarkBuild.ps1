[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$VisualCppRoot = 'C:/Program Files (x86)/Microsoft Visual Studio/18/BuildTools/VC/Tools/MSVC/14.51.36231',
  [string]$WindowsSdkLibRoot = 'C:/Program Files (x86)/Windows Kits/10/Lib/10.0.26100.0',
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-skylark-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory $scratch
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()
$captures = [Collections.Generic.List[object]]::new()
$environmentOverrides = @{}
$linker = (Resolve-Path -LiteralPath (Join-Path $VisualCppRoot 'bin/Hostx64/x64/link.exe')).Path
$nativeVersions = @("MSVC-link/$([Diagnostics.FileVersionInfo]::GetVersionInfo($linker).FileVersion)","Windows-SDK/$(Split-Path $WindowsSdkLibRoot -Leaf)")
$libraryPaths = @((Join-Path $VisualCppRoot 'lib/x64'),(Join-Path $WindowsSdkLibRoot 'um/x64'),(Join-Path $WindowsSdkLibRoot 'ucrt/x64'))
foreach ($library in $libraryPaths) { $null = Resolve-Path -LiteralPath $library }
$nativeArguments = @("-p:CppLinker=$linker","-p:SkylarkMsvcLibraries=$($libraryPaths[0])","-p:SkylarkUmLibraries=$($libraryPaths[1])","-p:SkylarkUcrtLibraries=$($libraryPaths[2])")

function Write-Lf([string]$Path,[string]$Value) { [IO.File]::WriteAllText($Path,$Value.Replace("`r`n","`n")) }
function Assert-Text([string]$Value,[string]$Expected) {
  if (-not $Value.Contains($Expected,[StringComparison]::Ordinal)) { throw "Missing '$Expected': $Value" }
}
function Invoke-Skylark([string]$Executable,[string]$Directory,[string[]]$Arguments,[int]$Exit = 0) {
  $start = [Diagnostics.ProcessStartInfo]::new($Executable)
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false; $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true; $start.RedirectStandardInput = $true
  $settings = @{
    DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; DOTNET_GENERATE_ASPNET_CERTIFICATE='false'; DOTNET_NOLOGO='1'
    DOTNET_CLI_HOME=(Join-Path $scratch 'home'); NUGET_PACKAGES=(Join-Path $scratch 'cache'); NUGET_HTTP_CACHE_PATH=(Join-Path $scratch 'http')
    DOTNET_CLI_USE_MSBUILD_SERVER='false'; MSBUILDUSESERVER='0'; DOTNET_CLI_TELEMETRY_OPTOUT='1'; DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE='true'
    DOTNET_ROOT=(Split-Path $currentDotnet); DOTNET_ROOT_X64=(Split-Path $currentDotnet); DOTNET_HOST_PATH=$currentDotnet
  }
  foreach ($key in $settings.Keys) { $start.Environment[$key] = $settings[$key] }
  foreach ($key in $environmentOverrides.Keys) { $start.Environment[$key] = $environmentOverrides[$key] }
  $process = [Diagnostics.Process]::Start($start)
  try {
    $process.StandardInput.Close()
    $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(240000)) { $process.Kill($true); throw 'Skylark consumer timed out.' }
    $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    $log.Add("cwd=$Directory`n$Executable $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$output")
    Write-Lf (Join-Path $scratch 'commands.log') ($log -join "`n")
    if ($process.ExitCode -ne $Exit) { throw "Expected exit $Exit, got $($process.ExitCode): $output" }
    return $output
  }
  finally { $process.Dispose() }
}
function Read-Pe([string]$File) {
  $stream = [IO.File]::OpenRead($File)
  $reader = [Reflection.PortableExecutable.PEReader]::new($stream)
  try {
    $cor = $reader.PEHeaders.CorHeader
    $flags = 0; $sections = @(); $runtimeFunctionsBytes = 0
    if ($null -ne $cor -and $cor.ManagedNativeHeaderDirectory.Size -gt 0) {
      [byte[]]$bytes = $reader.GetSectionData($cor.ManagedNativeHeaderDirectory.RelativeVirtualAddress).GetContent()
      if ([BitConverter]::ToUInt32($bytes,0) -ne 0x00525452) { throw 'Unexpected R2R signature.' }
      $flags = [BitConverter]::ToUInt32($bytes,8)
      $count = [BitConverter]::ToUInt16($bytes,12)
      for ($index = 0; $index -lt $count; $index++) {
        $section = [BitConverter]::ToUInt32($bytes,16 + $index * 12)
        $sections += $section
        if ($section -eq 102) { $runtimeFunctionsBytes = [BitConverter]::ToUInt32($bytes,24 + $index * 12) }
      }
    }
    $result = [ordered]@{ file=$File;sha256=(Get-FileHash $File).Hash.ToLowerInvariant();machine=$reader.PEHeaders.CoffHeader.Machine.ToString();managed=($null -ne $cor);nativeHeader=($null -ne $cor -and $cor.ManagedNativeHeaderDirectory.Size -gt 0);flags=$flags;sections=$sections;runtimeFunctionsBytes=$runtimeFunctionsBytes }
    $log.Add('PE ' + ($result | ConvertTo-Json -Compress))
    return $result
  }
  finally { $reader.Dispose(); $stream.Dispose() }
}

try {
  foreach ($major in '10','11') {
    $currentDotnet = if ($major -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
    $sdk = if ($major -eq '10') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    $language = if ($major -eq '10') { '14.0' } else { '15.0' }
    $runtimeVersion = if ($major -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    Write-Lf (Join-Path $scratch 'global.json') ('{"sdk":{"version":"' + $sdk + '","rollForward":"disable"}}')
    $lane = Join-Path $root "samples/net$major.0/SkylarkServicing"
    Assert-Text (Invoke-Skylark $currentDotnet $lane @('--version')) $sdk
    $info = Invoke-Skylark $currentDotnet $lane @('--info')
    $sdkRoot = Join-Path (Split-Path $currentDotnet) "sdk/$sdk"
    $compiler = Join-Path $sdkRoot 'Roslyn/bincore/csc.dll'
    $null = Invoke-Skylark $currentDotnet $lane @($compiler,'-version')
    $pins = Get-Content (Join-Path $root 'samples/SkylarkBuild/artifacts.json') -Raw | ConvertFrom-Json
    foreach ($pin in $pins.artifacts | Where-Object sdk -CEQ $sdk) {
      $asset = Join-Path (Split-Path $currentDotnet) $pin.path
      if ((Get-FileHash $asset).Hash.ToLowerInvariant() -cne $pin.sha256) { throw "Audited SDK/compiler/framework artifact drift: $asset" }
      $log.Add("artifact=$asset;sha256=$($pin.sha256);product=$($pin.product)")
    }
    foreach ($name in 'SkylarkServicing','SkylarkReadyToRun','SkylarkNative') {
      Write-Host "Qualify net$major.0/$name"
      $directory = Join-Path $root "samples/net$major.0/$name"
      $project = "$name.csproj"
      $sample = "samples/net$major.0/$name/$project"
      $startLog = $log.Count
      $restore = @('restore',$project)
      if (Test-Path (Join-Path $directory 'packages.lock.json')) { $restore += '--locked-mode' }
      $null = Invoke-Skylark $currentDotnet $directory $restore
      # Finalize real locks/dependencies before the immutable evidence snapshot.
      if ($Record) {
        foreach ($lock in Get-ChildItem $directory -Recurse -Filter packages.lock.json) { Write-Lf $lock.FullName (Get-Content $lock.FullName -Raw) }
      }
      $properties = (Invoke-Skylark $currentDotnet $directory @('msbuild',$project,'-p:Configuration=Release','-getProperty:TargetPath,ProjectAssetsFile')).Trim() | ConvertFrom-Json
      $assets = Get-Content $properties.Properties.ProjectAssetsFile -Raw | ConvertFrom-Json -AsHashtable
      $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
      $native = @(); if ($name -eq 'SkylarkNative') { $native = $nativeVersions }
      $manifestPath = Join-Path $directory 'verification.json'
      if ($Record) {
        $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
        $manifest.dependencies = $dependencies; $manifest.nativeToolchains = @($native)
        Write-Lf $manifestPath (($manifest | ConvertTo-Json -Depth 12) + "`n")
      }
      $inputs = Get-EvidenceInputs $root $sample
      $null = Invoke-Skylark $currentDotnet $directory @('build',$project,'-c','Release','--no-restore')
      $null = Invoke-Skylark $currentDotnet $scratch @($properties.Properties.TargetPath)

      if ($name -eq 'SkylarkServicing') {
        $advisoryHome = Join-Path $scratch "servicing-$major"
        $cache = Join-Path $advisoryHome ".dotnet/sdk-vulnerability-cache/sdk-status-$sdk.json"
        $null = New-Item -ItemType Directory (Split-Path $cache) -Force
        $environmentOverrides['DOTNET_CLI_HOME'] = $advisoryHome
        foreach ($proxy in 'HTTP_PROXY','HTTPS_PROXY','ALL_PROXY') { $environmentOverrides[$proxy] = 'http://127.0.0.1:1' }
        foreach ($scenario in 'missing','clear','vulnerable','eol','stale','corrupt') {
          if (Test-Path $cache) { Remove-Item -LiteralPath $cache }
          $summary = @{IsEol=$false;Cves=@();LatestSdkVersion='99.0.999';FeatureBandDiscontinued=$false}
          if ($scenario -in 'vulnerable','stale') { $summary.Cves = @(@{Id='CVE-2099-00001';Url='https://example.invalid/skylark-controlled'}) }
          if ($scenario -eq 'eol') { $summary.IsEol=$true; $summary.EolDate='2000-01-01T00:00:00Z' }
          if ($scenario -ne 'missing') { Write-Lf $cache ($summary | ConvertTo-Json -Depth 5) }
          if ($scenario -eq 'stale') { (Get-Item $cache).LastWriteTimeUtc = [datetime]'2000-01-01' }
          if ($scenario -eq 'corrupt') { Write-Lf $cache '{broken' }
          $cacheBefore = if (Test-Path $cache) { (Get-FileHash $cache).Hash } else { 'missing' }
          # msbuild -restore exercises real restore without the CLI's background advisory updater.
          $output = Invoke-Skylark $currentDotnet $directory @('msbuild',$project,'-restore','-t:Build','-p:Configuration=Release','-p:CheckSdkVulnerabilities=true','-p:TreatWarningsAsErrors=false','-p:NuGetAudit=false','-p:RestoreIgnoreFailedSources=true')
          $diagnostic = if ($scenario -in 'vulnerable','stale') { 'NETSDK1238' } elseif ($scenario -eq 'eol') { 'NETSDK1239' } else { '' }
          if ($major -eq '11' -and $diagnostic) { Assert-Text $output $diagnostic }
          elseif ($output -match 'NETSDK123[89]') { throw "Unexpected SDK warning for $major/$scenario." }
          if ($cacheBefore -cne $(if (Test-Path $cache) { (Get-FileHash $cache).Hash } else { 'missing' })) { throw 'Build modified advisory cache.' }
          $log.Add("PASS servicing=$scenario;sdk=$sdk;cache=$cacheBefore;expected=$(if($major -eq '11'){$diagnostic}else{'no SDK task'})")
        }
        $environmentOverrides.Clear()
      }

      if ($name -eq 'SkylarkReadyToRun') {
        foreach ($mode in 'full','selected') {
          $publish = Join-Path $scratch "r2r-$major-$mode"
          $null = Invoke-Skylark $currentDotnet $directory @('clean',$project,'-c','Release')
          $null = Invoke-Skylark $currentDotnet $directory @('publish',$project,'-c','Release',"-p:SkylarkMode=$mode",'-o',$publish)
          $main = Read-Pe (Join-Path $publish 'SkylarkReadyToRun.dll')
          $hot = Read-Pe (Join-Path $publish 'Skylark.HotRules.dll')
          $cold = Read-Pe (Join-Path $publish 'Skylark.ColdRules.dll')
          if (-not $main.nativeHeader) { throw 'Main assembly lacks real native R2R header.' }
          if ($mode -eq 'full') {
            if (-not $hot.nativeHeader -or -not $cold.nativeHeader -or $hot.runtimeFunctionsBytes -eq 0) { throw 'Full control lacks native R2R method output.' }
          }
          elseif ($cold.nativeHeader -or ($major -eq '10' -and $hot.nativeHeader)) { throw 'Excluded assembly was precompiled.' }
          elseif ($major -eq '11' -and (-not $hot.nativeHeader -or ($hot.flags -band 4) -eq 0)) { throw 'Selected assembly lacks partial R2R flag.' }
          if ($mode -eq 'selected' -and $major -eq '11' -and $hot.runtimeFunctionsBytes -ne 0) { throw 'Unprofiled partial control unexpectedly contains native method bodies.' }
          $output = Invoke-Skylark $currentDotnet $scratch @((Join-Path $publish 'SkylarkReadyToRun.dll'))
          Assert-Text $output 'PASS Skylark dispatch;direct=21;late=35;cold=SK-7'
          $environmentOverrides['DOTNET_ReadyToRun'] = '0'
          Assert-Text (Invoke-Skylark $currentDotnet $scratch @((Join-Path $publish 'SkylarkReadyToRun.dll'))) 'PASS Skylark dispatch'
          $environmentOverrides.Clear()
          $log.Add("PASS R2R=$major/$mode;normal-and-forced-JIT")
        }
        if ($major -eq '11') {
          $null = Invoke-Skylark $currentDotnet $directory @('clean',$project,'-c','Release')
          $failure = Invoke-Skylark $currentDotnet $directory @('publish',$project,'-c','Release','-p:PublishReadyToRunContainerFormat=skylark-invalid','-o',(Join-Path $scratch 'invalid-r2r')) 1
          if ($failure -notmatch '(MSB4018|NETSDK1096|NETSDK1095|invalid|Unknown)') { throw 'Unexpected invalid container failure.' }
        }
        $crossgen = Join-Path $scratch "cache/microsoft.netcore.app.crossgen2.win-x64/$runtimeVersion/tools/crossgen2.exe"
        $log.Add("crossgen=$crossgen;sha256=$((Get-FileHash $crossgen).Hash.ToLowerInvariant());product=$([Diagnostics.FileVersionInfo]::GetVersionInfo($crossgen).ProductVersion)")
        $runtime = Join-Path $scratch "cache/microsoft.netcore.app.runtime.win-x64/$runtimeVersion/runtimes/win-x64/native/coreclr.dll"
        $log.Add("runtime-pack=$runtime;sha256=$((Get-FileHash $runtime).Hash.ToLowerInvariant());product=$([Diagnostics.FileVersionInfo]::GetVersionInfo($runtime).ProductVersion)")
      }

      if ($name -eq 'SkylarkNative') {
        $feed = Join-Path $scratch "feed-$major"
        $packArguments = @('pack',$project,'-c','Release','-o',$feed) + $nativeArguments
        if ($major -eq '10') { $packArguments += @('-r','win-x64') }
        $output = Invoke-Skylark $currentDotnet $directory $packArguments
        if ([regex]::Matches($output,'(?m)^\s*SKYLARK_PACK:rid=win-x64\s*$').Count -ne 1) { throw 'Expected exactly one RID Pack inner build.' }
        if ($major -eq '10') { $null = Invoke-Skylark $currentDotnet $directory (@('pack',$project,'-c','Release','-o',$feed) + $nativeArguments) }
        $packages = @(Get-ChildItem $feed -Filter '*.nupkg')
        $expectedCount = 2
        if ($packages.Count -ne $expectedCount) { throw "Expected $expectedCount real tool packages: $output" }
        foreach ($package in $packages) {
          $zip = [IO.Compression.ZipFile]::OpenRead($package.FullName)
          try {
            $log.Add("package=$($package.Name);sha256=$((Get-FileHash $package.FullName).Hash.ToLowerInvariant());entries=$($zip.Entries.FullName -join ',')")
            $nativeEntry = @($zip.Entries | Where-Object FullName -Like 'tools/*/*/*.exe')
            if ($package.Name -match 'win-x64' -and $nativeEntry.Count -ne 1) { throw 'RID package lacks one actual native executable.' }
            foreach ($entry in $zip.Entries | Where-Object { $_.FullName -match '\.(json|nuspec|xml)$' }) {
              $reader = [IO.StreamReader]::new($entry.Open())
              try {
                $body = $reader.ReadToEnd()
                $log.Add("$($entry.FullName)=$body")
                if ($entry.FullName.EndsWith('DotnetToolSettings.xml') -and $package.BaseName -eq 'Skylark.Native.Tool.1.0.0') {
                  Assert-Text $body 'win-x64'
                  if ($body.Contains('win-arm64')) { throw 'Outer RID graph advertises an unsupported payload.' }
                }
              } finally { $reader.Dispose() }
            }
          }
          finally { $zip.Dispose() }
        }
        $config = Join-Path $scratch "tool-$major.config"
        Write-Lf $config ('<configuration><packageSources><clear/><add key="local" value="' + [Security.SecurityElement]::Escape($feed) + '"/></packageSources></configuration>')
        $tool = Join-Path $scratch "tool-$major"
        $null = Invoke-Skylark $currentDotnet $scratch @('tool','install','Skylark.Native.Tool','--version','1.0.0','--tool-path',$tool,'--configfile',$config,'--no-cache')
        $output = Invoke-Skylark (Join-Path $env:SystemRoot 'System32/cmd.exe') $scratch @('/d','/c',(Join-Path $tool 'skylark-native.cmd'))
        Assert-Text $output 'SKYLARK-NATIVE;crates=21;architecture=X64;dynamic=False'
        $nativeExe = @(Get-ChildItem (Join-Path $tool '.store') -Recurse -Filter SkylarkNative.exe -Force)
        if ($nativeExe.Count -ne 1 -or (Read-Pe $nativeExe[0].FullName).managed) { throw 'Installed payload is not a native executable.' }
        $failure = Invoke-Skylark $currentDotnet $directory (@('pack',$project,'--no-restore','-r','win-arm64','-o',(Join-Path $scratch 'unsupported')) + $nativeArguments) 1
        Assert-Text $failure 'SKYLARK001'
        $failure = Invoke-Skylark $currentDotnet $directory @('pack',$project,'--no-restore','-r','win-x64','-p:CppLinker=C:/skylark-missing/link.exe','-o',(Join-Path $scratch 'missing-linker')) 1
        Assert-Text $failure 'SKYLARK002'
        if ($major -eq '11') {
          $defaults = Invoke-Skylark $currentDotnet $directory @('msbuild',$project,'-t:ComputeToolPackageRuntimeIdentifiersToPack,_ComputeDefaultToolPackageRuntimeIdentifiersToPack','-p:SkylarkUseDefaults=true','-p:SkylarkInspectCandidateDefaults=true','-getItem:ToolPackageRuntimeIdentifiersToPack')
          Assert-Text $defaults 'win-x64'; Assert-Text $defaults 'win-arm64'
        }
        $log.Add("PASS native=$major;packages=$expectedCount;installed-real-native-x64;unsupported-and-missing-toolchain-rejected;native=$($native -join ',')")
      }
      $captures.Add(@{Sample=$sample;Inputs=$inputs;Dependencies=$dependencies;Native=@($native);Info=$info;Sdk=$sdk;Language=$language;Target="net$major.0";Start=$startLog;End=$log.Count})
      Write-Host "PASS net$major.0/$name"
    }
  }
  foreach ($capture in $captures) {
    if (($capture.Inputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $capture.Sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during verification.' }
    if ($Record) {
      $recordValue = [ordered]@{
        kind='compile-and-run';sample=$capture.Sample;target=$capture.Target;sdk=$capture.Sdk;language=$capture.Language
        command="eng/Test-SkylarkBuild.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -VisualCppRoot '$VisualCppRoot' -WindowsSdkLibRoot '$WindowsSdkLibRoot' -Record"
        scope='Real paired build/run and bounded SDK cache, R2R PE/partial/exclusion/JIT, or native package/install/failure checks. Windows x64; controlled advisory fixtures, no live security verdict, performance or GA claim.'
        recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim();sourceDirty=$true
        inputs=$capture.Inputs;dependencies=$capture.Dependencies;workloads=@();nativeToolchains=$capture.Native
        host=[Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
        sdkInfo=$capture.Info;workloadInfo='No optional workload; native versions are measured FileVersion and installed Windows SDK library directory.'
        output=($log | Select-Object -Skip $capture.Start -First ($capture.End - $capture.Start)) -join "`n"
      }
      Write-Lf (Join-Path (Split-Path (Join-Path $root $capture.Sample)) 'verification-evidence.json') (($recordValue | ConvertTo-Json -Depth 40) + "`n")
    }
  }
  Write-Host 'PASS: paired SDK servicing, partial ReadyToRun and native tool RID consumers.'
}
finally {
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  Write-Host "Disposable commands/cache/results: $scratch"
}
