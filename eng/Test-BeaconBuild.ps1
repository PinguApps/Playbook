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
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-beacon-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$paths = @{}; $persistent = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$log = [Collections.Generic.List[string]]::new()
$cache = Join-Path $scratch 'setup-cache'

function Write-Lf([string]$Path,[string]$Text) { [IO.File]::WriteAllText($Path,$Text.Replace("`r`n","`n")) }
function Assert-Text([string]$Text,[string]$Expected) {
  if (-not $Text.Contains($Expected,[StringComparison]::Ordinal)) { throw "Missing '$Expected': $Text" }
}
function Invoke-Beacon([string]$Directory,[string[]]$Arguments,[int]$Exit = 0) {
  $start = [Diagnostics.ProcessStartInfo]::new($dotnet)
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.WorkingDirectory = $Directory; $start.UseShellExecute = $false; $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true; $start.RedirectStandardInput = $true
  $settings = @{
    DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; DOTNET_GENERATE_ASPNET_CERTIFICATE='false'; DOTNET_NOLOGO='1'
    DOTNET_CLI_HOME=(Join-Path $scratch 'home'); NUGET_PACKAGES=$cache; NUGET_HTTP_CACHE_PATH=(Join-Path $scratch 'http')
    DOTNET_CLI_USE_MSBUILD_SERVER='false'; MSBUILDUSESERVER='0'; MSBUILDDISABLENODEREUSE='1'
    DOTNET_CLI_TELEMETRY_OPTOUT='1'; DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE='true'
    DOTNET_ROOT=(Split-Path $dotnet); DOTNET_ROOT_X64=(Split-Path $dotnet); DOTNET_HOST_PATH=$dotnet
    HTTP_PROXY='http://127.0.0.1:1'; HTTPS_PROXY='http://127.0.0.1:1'; ALL_PROXY='http://127.0.0.1:1'
  }
  foreach ($key in $settings.Keys) { $start.Environment[$key] = $settings[$key] }
  $process = [Diagnostics.Process]::Start($start)
  try {
    $process.StandardInput.Close()
    $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(120000)) { $process.Kill($true); throw 'Beacon consumer timed out.' }
    $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    $transcript = "cwd=$Directory`n$dotnet $($Arguments -join ' ')`ncache=$cache;exit=$($process.ExitCode)`n$output"
    [IO.File]::AppendAllText((Join-Path $scratch 'raw.log'),$transcript + "`n")
    $captured = $output
    if ('-getItem:MSBuildImportedProject' -in $Arguments -or '-t:ShowImports' -in $Arguments) {
      $digest = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($output)))
      $captured = "Evaluated imports checked below; raw response SHA256=$digest. Full output in disposable raw.log."
    }
    $log.Add("cwd=$Directory`n$dotnet $($Arguments -join ' ')`ncache=$cache;exit=$($process.ExitCode)`n$captured")
    Write-Lf (Join-Path $scratch 'commands.log') ($log -join "`n")
    if ($process.ExitCode -ne $Exit) { throw "Expected exit $Exit, got $($process.ExitCode): $output" }
    return $output
  }
  finally { $process.Dispose() }
}
function Copy-Fixture([string]$Source,[string]$Destination) {
  $null = New-Item -ItemType Directory -Path $Destination
  foreach ($file in Get-ChildItem -LiteralPath $Source -File -Recurse) {
    $relative = [IO.Path]::GetRelativePath($Source,$file.FullName)
    if ($relative -match '(^|[\\/])(bin|obj)[\\/]' -or $file.Name -like '*evidence.json') { continue }
    $target = Join-Path $Destination $relative
    $null = New-Item -ItemType Directory -Path (Split-Path $target) -Force
    Copy-Item -LiteralPath $file.FullName -Destination $target
  }
  # Recreate the effective sample boundary outside the repository.
  Write-Lf (Join-Path $Destination 'Directory.Build.props') '<Project><PropertyGroup><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors><Deterministic>true</Deterministic></PropertyGroup></Project>'
  Write-Lf (Join-Path $Destination 'Directory.Build.targets') '<Project />'
  Write-Lf (Join-Path $Destination 'Directory.Packages.props') '<Project />'
}
function Read-Imports([string]$Directory,[string[]]$Properties) {
  $output = Invoke-Beacon $Directory (@('msbuild','Imports/Beacon.proj','-nologo','-getItem:MSBuildImportedProject') + $Properties)
  if ('-p:BeaconDuplicate=true' -in $Properties) { Assert-Text $output 'MSB4011' }
  # Diagnostics can precede or follow JSON (stderr is captured separately).
  $first = $output.IndexOf('{'); $last = $output.LastIndexOf('}')
  if ($first -lt 0 -or $last -lt $first) { throw 'No evaluated-item JSON returned.' }
  $json = $output.Substring($first,$last - $first + 1) | ConvertFrom-Json
  # Incidental file AccessedTime changes when MSBuild reads imports. Compare the
  # actual tree contract, not filesystem timestamps from built-in item metadata.
  $items = @($json.Items.MSBuildImportedProject | ForEach-Object {
    $sdkMetadata = $_.PSObject.Properties['Sdk']
    [pscustomobject]@{ Identity=$_.Identity; ImportingProjectPath=$_.ImportingProjectPath; Sdk=$(if ($null -eq $sdkMetadata) { '' } else { $sdkMetadata.Value }) }
  })
  return ,$items
}

try {
  foreach ($relative in 'Program.cs','Rules/BerthCharge.cs','Rates/Tariff.cs','Imports/Shared.props','Imports/Leaf.props','Imports/Night.props') {
    $older = Join-Path $root "samples/net10.0/BeaconBuild/$relative"
    $newer = Join-Path $root "samples/net11.0/BeaconBuild/$relative"
    if ((Get-FileHash -LiteralPath $older).Hash -cne (Get-FileHash -LiteralPath $newer).Hash) { throw "Shared canonical source drift: $relative" }
  }
  $pins = Get-Content -LiteralPath (Join-Path $root 'samples/net11.0/BeaconBuild/artifact-pins.json') -Raw | ConvertFrom-Json -AsHashtable
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    if (-not [IO.Path]::IsPathFullyQualified($dotnet)) { throw 'Supply absolute SDK executable paths.' }
    $sample = "samples/net$major.0/BeaconBuild/BeaconBuild.csproj"
    $directory = Split-Path (Join-Path $root $sample)
    $pin = Get-Content -LiteralPath (Join-Path $directory 'global.json') -Raw | ConvertFrom-Json
    $sdk = (Invoke-Beacon $directory @('--version')).Trim()
    if ($sdk -cne $pin.sdk.version) { throw "Unexpected SDK $sdk" }
    $info = Invoke-Beacon $directory @('--info')
    $startLog = $log.Count
    foreach ($asset in $pins[$sdk]) {
      $path = [IO.Path]::GetFullPath((Join-Path (Split-Path $dotnet) "sdk/$sdk/$($asset.path)"))
      if ((Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant() -cne $asset.sha256) { throw "Artifact drift: $path" }
      $log.Add("artifact=$path;sha256=$($asset.sha256);product=$($asset.product)")
    }
    $feed = Join-Path $scratch "feed-$major"
    $null = New-Item -ItemType Directory -Path $feed
    $emptyConfig = Join-Path $scratch 'empty.config'
    Write-Lf $emptyConfig '<configuration><packageSources><clear/></packageSources></configuration>'
    $cache = Join-Path $scratch "setup-cache-$major"
    $null = Invoke-Beacon $directory @('pack','Rates/Beacon.Rates.csproj','-c','Release','-o',$feed,'--configfile',$emptyConfig,'-p:NuGetAudit=false','-p:UseSharedCompilation=false')
    $package = Join-Path $feed 'Beacon.Rates.1.0.0.nupkg'
    $zip = [IO.Compression.ZipFile]::OpenRead($package)
    try {
      if (@($zip.Entries | Where-Object FullName -CEQ "lib/net$major.0/Beacon.Rates.dll").Count -ne 1) { throw 'Original package lacks real compiled asset.' }
      $log.Add("package=$package;sha256=$((Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant());entries=$($zip.Entries.FullName -join ',')")
    } finally { $zip.Dispose() }
    $config = Join-Path $scratch "local-$major.config"
    Write-Lf $config ('<configuration><packageSources><clear/><add key="local" value="' + [Security.SecurityElement]::Escape($feed) + '"/></packageSources></configuration>')
    # Create/update and compile the actual canonical consumers, then freeze the input set.
    $lockMode = if ($Record) { '--force-evaluate' } else { '--locked-mode' }
    $null = Invoke-Beacon $directory @('restore','BeaconBuild.csproj',$lockMode,'--configfile',$config,'-p:NuGetAudit=false')
    if ($Record) {
      foreach ($lock in Get-ChildItem -LiteralPath $directory -Filter packages.lock.json -Recurse) { Write-Lf $lock.FullName (Get-Content -LiteralPath $lock.FullName -Raw) }
    }
    $null = Invoke-Beacon $directory @('build','BeaconBuild.csproj','-c','Release','--no-restore','-p:UseSharedCompilation=false')
    Assert-Text (Invoke-Beacon $directory @((Join-Path $directory "bin/Release/net$major.0/BeaconBuild.dll"))) 'BEACON: crates=3;credits=21'
    $inputs = Get-EvidenceInputs $root $sample
    $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
    $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
    $manifest = Get-Content -LiteralPath (Join-Path $directory 'verification.json') -Raw | ConvertFrom-Json
    if (($dependencies | ConvertTo-Json -Compress) -cne ($manifest.dependencies | ConvertTo-Json -Compress)) { throw 'Dependency closure differs from manifest.' }

    $signatures = @{}
    foreach ($mode in 'true','false','default') {
      $fixture = Join-Path $scratch "restore-$major-$mode"
      Copy-Fixture $directory $fixture
      $cache = Join-Path $scratch "restore-cache-$major-$mode"
      $arguments = @('restore','BeaconBuild.csproj','--locked-mode','--configfile',$config,'-p:NuGetAudit=false')
      if ($mode -ne 'default') { $arguments += "-p:RestoreDoNotWriteDependencyGraphSpec=$mode" }
      $null = Invoke-Beacon $fixture $arguments
      $graph = Test-Path -LiteralPath (Join-Path $fixture 'obj/BeaconBuild.csproj.nuget.dgspec.json')
      if ($graph -ne ($mode -ne 'true')) { throw "Unexpected graph presence $major/$mode=$graph" }
      $restored = Get-Content -LiteralPath (Join-Path $fixture 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      $signatures[$mode] = $restored.targets | ConvertTo-Json -Depth 30 -Compress
      if (@($restored.libraries.Keys).Count -ne 2 -or -not $restored.libraries.ContainsKey('Beacon.Rates/1.0.0')) { throw 'Missing exact restored closure.' }
      $packageHash = Join-Path $cache 'beacon.rates/1.0.0/beacon.rates.1.0.0.nupkg.sha512'
      if ((Get-Content -LiteralPath $packageHash -Raw).Trim() -cne $restored.libraries['Beacon.Rates/1.0.0'].sha512) { throw 'Restored package hash mismatch.' }
      $null = Invoke-Beacon $fixture @('build','BeaconBuild.csproj','-c','Release','--no-restore','-p:UseSharedCompilation=false')
      Assert-Text (Invoke-Beacon $fixture @((Join-Path $fixture "bin/Release/net$major.0/BeaconBuild.dll"))) 'BEACON: crates=3;credits=21'
      if ($mode -eq 'default') {
        $before = (Get-FileHash -LiteralPath (Join-Path $fixture 'obj/BeaconBuild.csproj.nuget.dgspec.json')).Hash
        $null = Invoke-Beacon $fixture $arguments
        if ($before -cne (Get-FileHash -LiteralPath (Join-Path $fixture 'obj/BeaconBuild.csproj.nuget.dgspec.json')).Hash) { throw 'Incremental local graph bytes changed.' }
      }
      $log.Add("PASS restore=$major/$mode;dgspec=$graph;exact-package-hash;two-library-closure;build-and-run")
    }
    if ($signatures.true -cne $signatures.false -or $signatures.true -cne $signatures.default) { throw 'Selected compile/runtime assets differ across restore settings.' }
    $missing = Join-Path $scratch "missing-$major"
    Copy-Fixture $directory $missing
    $cache = Join-Path $scratch "missing-cache-$major"
    $missingOutput = Invoke-Beacon $missing @('restore','BeaconBuild.csproj','--locked-mode','--configfile',$emptyConfig,'-p:NuGetAudit=false') 1
    if ($missingOutput -notmatch 'NU110[01]') { throw 'Missing package did not fail dependency resolution.' }
    $log.Add("PASS missing-local-package=$major;fresh-cache;NU1100-or-NU1101")

    $fixture = Join-Path $scratch "symbols-$major"
    Copy-Fixture $directory $fixture
    $cache = Join-Path $scratch "symbols-cache-$major"
    $null = Invoke-Beacon $fixture @('restore','BeaconBuild.csproj','--locked-mode','--configfile',$config,'-p:NuGetAudit=false')
    foreach ($mode in 'true','false','default','singular') {
      $publish = Join-Path $scratch "publish-$major-$mode"
      $arguments = @('publish','BeaconBuild.csproj','-c','Release','--no-restore','-p:UseSharedCompilation=false','-o',$publish)
      if ($mode -in 'true','false') { $arguments += "-p:PublishReferencesSymbols=$mode" }
      elseif ($mode -eq 'singular') { $arguments += '-p:PublishReferenceSymbols=false' }
      $null = Invoke-Beacon $fixture $arguments
      $files = @(Get-ChildItem -LiteralPath $publish -File | Select-Object -ExpandProperty Name | Sort-Object)
      Write-Host "Symbols $major/$mode : $($files -join ',')"
      foreach ($required in 'BeaconBuild.dll','BeaconBuild.pdb','Beacon.Rules.dll','Beacon.Rates.dll') {
        if ($required -notin $files) { throw "Missing publish artifact $required" }
      }
      $expectedReference = $major -eq 10 -or $mode -ne 'false'
      if (('Beacon.Rules.pdb' -in $files) -ne $expectedReference) { throw "Unexpected referenced PDB for $major/$mode" }
      # An snupkg is not restored as the ordinary dependency and supplies no publish PDB.
      if ('Beacon.Rates.pdb' -in $files) { throw 'Unexpected package symbol.' }
      foreach ($name in 'BeaconBuild','Beacon.Rules') {
        $buildDirectory = if ($name -eq 'BeaconBuild') { $fixture } else { Join-Path $fixture 'Rules' }
        foreach ($extension in 'dll','pdb') {
          $published = Join-Path $publish "$name.$extension"
          if (-not (Test-Path -LiteralPath $published)) { continue }
          $built = Join-Path $buildDirectory "bin/Release/net$major.0/$name.$extension"
          if ((Get-FileHash -LiteralPath $published).Hash -cne (Get-FileHash -LiteralPath $built).Hash) { throw 'Publish bytes differ from the built artifact.' }
          if ($extension -eq 'pdb' -and [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($published),0,4) -cne 'BSJB') { throw 'Expected portable PDB.' }
        }
      }
      Assert-Text (Invoke-Beacon $fixture @((Join-Path $publish 'BeaconBuild.dll'))) 'BEACON: crates=3;credits=21'
      $log.Add("PASS symbols=$major/$mode;files=$($files -join ',');executed")
    }
    $ownControl = Join-Path $scratch "publish-$major-no-own-symbols"
    $null = Invoke-Beacon $fixture @('publish','BeaconBuild.csproj','-c','Release','--no-restore','-p:UseSharedCompilation=false','-p:PublishReferencesSymbols=true','-p:CopyOutputSymbolsToPublishDirectory=false','-o',$ownControl)
    if (Test-Path -LiteralPath (Join-Path $ownControl 'BeaconBuild.pdb')) { throw 'Own-symbol control retained application PDB.' }
    if ($major -eq 11 -and -not (Test-Path -LiteralPath (Join-Path $ownControl 'Beacon.Rules.pdb'))) { throw 'Reference-symbol opt-in did not preserve reference PDB independently.' }
    Assert-Text (Invoke-Beacon $fixture @((Join-Path $ownControl 'BeaconBuild.dll'))) 'BEACON: crates=3;credits=21'
    $log.Add("PASS own-symbol-control=$major;app-pdb-absent;reference-pdb=$(Test-Path -LiteralPath (Join-Path $ownControl 'Beacon.Rules.pdb'));executed")

    foreach ($enabled in $false,$true) {
      $properties = @()
      if ($enabled) { $properties += '-p:MSBuildProvideImportedProjects=true' }
      $imports = Read-Imports $fixture $properties
      Write-Host "Imports $major/enabled=$enabled : $($imports.Count)"
      if (-not $enabled -and $imports.Count -ne 0) { throw 'Unset import opt-in returned items.' }
      if ($enabled) {
        if ($imports.Count -eq 0) { throw 'Opt-in returned no evaluated imports.' }
        $baselineImports = $imports
        $shared = @($imports | Where-Object Identity -CEQ (Join-Path $fixture 'Imports/Shared.props'))
        $leaf = @($imports | Where-Object Identity -CEQ (Join-Path $fixture 'Imports/Leaf.props'))
        $sdkProps = @($imports | Where-Object { $_.Identity.EndsWith('Microsoft.NET.Sdk\Sdk\Sdk.props',[StringComparison]::OrdinalIgnoreCase) })
        if ($shared.Count -ne 1 -or $leaf.Count -ne 1 -or $sdkProps.Count -ne 1) { throw 'Missing nested/SDK import.' }
        if ($shared[0].ImportingProjectPath -cne (Join-Path $fixture 'Imports/Beacon.proj') -or $leaf[0].ImportingProjectPath -cne $shared[0].Identity) { throw 'Wrong nested import parent.' }
        if ($sdkProps[0].Sdk -cne 'Microsoft.NET.Sdk') { throw 'Wrong SDK metadata.' }
        foreach ($custom in @($shared[0],$leaf[0])) {
          $sdkMetadata = $custom.PSObject.Properties['Sdk']
          if ($null -ne $sdkMetadata -and $sdkMetadata.Value -cne '') { throw 'Custom import incorrectly labelled as an SDK.' }
        }
        if ([array]::IndexOf($imports,$shared[0]) -ge [array]::IndexOf($imports,$leaf[0])) { throw 'Import order is not parent before child.' }
        if (@($imports | Where-Object { $_.Identity.EndsWith('Night.props') }).Count -ne 0) { throw 'False condition imported Night.props.' }
      }
      $relevant = @($imports | Where-Object { $_.Identity.StartsWith((Join-Path $fixture 'Imports')) -or $_.Sdk -ne '' })
      $log.Add("imports=$major/enabled=$enabled;count=$($imports.Count);" + ($relevant | ConvertTo-Json -Depth 10 -Compress))
    }
    $disabled = Read-Imports $fixture @('-p:MSBuildProvideImportedProjects=false')
    if ($disabled.Count -ne 0) { throw 'Explicit false import control returned items.' }
    $repeated = Read-Imports $fixture @('-p:MSBuildProvideImportedProjects=true')
    if (($baselineImports | ConvertTo-Json -Depth 10 -Compress) -cne ($repeated | ConvertTo-Json -Depth 10 -Compress)) { throw 'Repeated import query changed order/metadata.' }
    $nightImports = Read-Imports $fixture @('-p:MSBuildProvideImportedProjects=true','-p:BeaconNight=true')
    $night = @($nightImports | Where-Object Identity -CEQ (Join-Path $fixture 'Imports/Night.props'))
    if ($night.Count -ne 1 -or $night[0].ImportingProjectPath -cne (Join-Path $fixture 'Imports/Beacon.proj') -or $nightImports.Count -ne $baselineImports.Count + 1) { throw 'Wrong conditional import closure.' }
    $duplicateImports = Read-Imports $fixture @('-p:MSBuildProvideImportedProjects=true','-p:BeaconDuplicate=true')
    if (($baselineImports | ConvertTo-Json -Depth 10 -Compress) -cne ($duplicateImports | ConvertTo-Json -Depth 10 -Compress)) { throw 'Duplicate import changed unique closure or first parent.' }
    $shown = Invoke-Beacon $fixture @('msbuild','Imports/Beacon.proj','-nologo','-t:ShowImports','-p:MSBuildProvideImportedProjects=true','-p:BeaconNight=true')
    Assert-Text $shown 'Leaf.props|'; Assert-Text $shown '|Microsoft.NET.Sdk'
    $log.Add("PASS imports=$major;unset-and-false-empty;SDK-and-nested-parent-metadata;parent-before-child;stable-order;conditional-plus-one;duplicate-ignored-with-MSB4011;target-output")
    if (($inputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during verification.' }
    if ($Record) {
      $output = ($log | Select-Object -Skip $startLog) -join "`n"
      foreach ($pair in @(@($root,'<repo>'),@($scratch,'<session>'),@((Split-Path $dotnet),'<dotnet-root>'))) {
        $output = $output.Replace($pair[0],$pair[1],[StringComparison]::OrdinalIgnoreCase).Replace($pair[0].Replace('\','/'),$pair[1],[StringComparison]::OrdinalIgnoreCase)
      }
      $recordValue = [ordered]@{
        kind='compile-and-run';sample=$sample;target="net$major.0";sdk=$sdk;language=$(if($major -eq 10){'14.0'}else{'15.0'})
        command="eng/Test-BeaconBuild.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Record"
        scope='Real paired local package pack/locked restore, dgspec on/off/default/incremental, publish symbols and evaluated import controls; Windows x64, no performance or GA claim.'
        recordedAt=[DateTimeOffset]::UtcNow.ToString('O');sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim();sourceDirty=$true
        inputs=$inputs;dependencies=$dependencies;workloads=@();nativeToolchains=@()
        host=[Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
        sdkInfo=$info;workloadInfo='No optional workload or native compiler required.';output=$output
      }
      Write-Lf (Join-Path $directory 'verification-evidence.json') (($recordValue | ConvertTo-Json -Depth 40) + "`n")
    }
    Write-Host "PASS net$major.0 Beacon build consumers."
  }
}
finally {
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
  Write-Host "Disposable commands, artifacts and caches: $scratch"
}
