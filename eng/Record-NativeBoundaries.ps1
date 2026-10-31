[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$Docker = 'C:/Program Files/Docker/Docker/resources/bin/docker.exe',
  [switch]$VerifySources
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
function Assert-LfInputs([string]$Sample) {
  # These samples have text-only inputs. Git stores/checks them out as LF.
  foreach ($relative in (Get-EvidenceInputs $root $Sample).Keys) {
    if ([IO.File]::ReadAllText((Join-Path $root $relative)).Contains("`r")) {
      throw "Use LF input bytes before capturing evidence: $relative. Git normalizes CRLF and would invalidate the saved hash on another checkout."
    }
  }
}
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$persistent = @{}
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','MSBUILDUSESERVER') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:MSBUILDUSESERVER = '0'
  $manifestPath = 'docs/planning/audits/native-boundaries.sources.json'
  $sources = Get-Content -LiteralPath (Join-Path $root $manifestPath) -Raw | ConvertFrom-Json
  if ($VerifySources) {
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash"
    }
  }
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $runtime = if ($variant -eq 'net10.0') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $names = @('TelemetryKeys','BuoyPipes','MobileJournal')
    if ($variant -eq 'net10.0') { $names += 'ShoreCalibration' }
    foreach ($name in $names) {
      $sample = "samples/$variant/$name/$name.csproj"
      Assert-LfInputs $sample
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
      $directory = Join-Path $root "samples/$variant/$name"
      $recordPath = Join-Path $directory 'verification-evidence.json'
      $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
      if ($name -ne 'ShoreCalibration' -and -not $record.output.Contains("runtime=.NET $runtime;")) { throw "Wrong runtime: $sample" }
      Push-Location $directory
      try { $pack = (& $dotnet msbuild "$name.csproj" -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App' }
      finally { Pop-Location }
      if ($pack.NuGetPackageVersion -cne $runtime -or $pack.TargetFramework -cne $variant) { throw "Reference pack drift: $sample" }
      $artifacts = @(
        (Join-Path (Split-Path $dotnet) "sdk/$($record.sdk)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
        (Join-Path $pack.Path "ref/$variant/System.Runtime.dll"),
        (Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$runtime/System.Private.CoreLib.dll")
      )
      if ($name -eq 'TelemetryKeys') { $artifacts += (Join-Path $pack.Path "ref/$variant/System.Security.Cryptography.dll"), (Join-Path ([Environment]::SystemDirectory) 'bcrypt.dll') }
      if ($name -eq 'BuoyPipes') { $artifacts += (Join-Path $pack.Path "ref/$variant/System.IO.Pipes.dll") }
      foreach ($artifact in $artifacts) {
        $record.output += "`nARTIFACT $artifact version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion) sha256=$((Get-FileHash -LiteralPath $artifact).Hash.ToLowerInvariant())"
      }
      $record.command += '; ResolveFrameworkReferences; SHA256 selected compiler/reference/runtime/native artifacts'
      $record.scope = switch ($name) {
        'TelemetryKeys' { 'Windows x64 SHA256 only: actual CNG availability/current selection; raw DeriveKey IKM and Expand PRK at 2048/2049, independent HMAC expected output, RFC5869 case1, unchanged bytes, canonical rejection with null output; separate managed Extract. Other Windows builds/hashes/GA not established.' }
        'BuoyPipes' { 'Windows x64 actual old/new canonical round trips; connected handle ownership transfer; current four blocking/async pipe-end combinations and FileStream wrapping, EOF/disposal; regular-file async read and Windows SendPacketsElement guard. Separate linux.evidence.json executes Unix conditions.' }
        'MobileJournal' { 'Windows x64 portable helper execution: complete journal data and pre-cancellation; current concurrent calls use one singleton gate. No iOS/tvOS workload compilation, advisory-lock, suspension or extension/process coordination execution; these are explicitly not-tested.' }
        'ShoreCalibration' { 'Actual .NET10 managed arithmetic comparison only. It does not reuse a C library or establish a CoreCLR WebAssembly native relink.' }
      }
      if ($name -eq 'BuoyPipes' -and $variant -eq 'net11.0') {
        $probe = Join-Path $root 'artifacts/native-boundaries/obsolete-probe'
        New-Item -ItemType Directory -Path $probe -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $directory 'global.json') -Destination $probe
        foreach ($boundary in 'Directory.Build.props','Directory.Build.targets','Directory.Packages.props') {
          Copy-Item -LiteralPath (Join-Path $root "samples/$boundary") -Destination $probe
        }
        Copy-Item -LiteralPath (Join-Path $root 'samples/net10.0/BuoyPipes/PipeTransfer.cs') -Destination $probe
        '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>15.0</LangVersion><OutputType>Library</OutputType><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>' | Set-Content -LiteralPath (Join-Path $probe 'Obsolete.csproj')
        Push-Location $probe
        try {
          $PSNativeCommandUseErrorActionPreference = $false
          $output = (& $dotnet build Obsolete.csproj -c Release 2>&1 | Out-String).Trim()
          $exit = $LASTEXITCODE
        }
        finally { $PSNativeCommandUseErrorActionPreference = $true; $global:LASTEXITCODE = 0; Pop-Location }
        if ($exit -eq 0 -or $output -notmatch 'error SYSLIB0063' -or $output -match 'error (?!SYSLIB0063)[A-Z]+\d+') { throw "Unexpected old-constructor diagnostic: $output" }
        $record.output += "`nEXPECTED old constructor current-target exit=$exit`n$output"
        $record.command += "; cd $probe; $dotnet build Obsolete.csproj -c Release (expected SYSLIB0063)"
      }
      [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
      if ($name -eq 'BuoyPipes') {
        $image = if ($variant -eq 'net10.0') { 'mcr.microsoft.com/dotnet/runtime@sha256:e454ed02b8c903dff0d7fd40a63451e55f16c186463d168a78e584738244f939' }
          else { 'mcr.microsoft.com/dotnet/runtime@sha256:3b52667dbaff185a1ddd4cefb16d9517e6945f6e61257faa2e57e6040877d95b' }
        $before = Get-EvidenceInputs $root $sample
        $container = 'playbook-buoy-' + [guid]::NewGuid().ToString('N')
        # Copy to private tmpfs to avoid executing mapped binaries while the host builds.
        $args = @('run','--name',$container,'--rm','--network','none','--read-only','--tmpfs','/tmp',
          '--env','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0','--mount',"type=bind,source=$directory/bin/Release/$variant,target=/app,readonly",$image,
          '/bin/sh','-c','cp /app/BuoyPipes.* /tmp; cd /tmp; /usr/share/dotnet/dotnet BuoyPipes.dll')
        try {
          $output = (& $Docker @args | Out-String).Trim()
          if (-not $output.Contains("runtime=.NET $runtime;") -or -not $output.Contains('CurrentUserOnly bind mode=') -or -not $output.Contains('PASS bounded pipe contracts')) { throw "Incomplete Unix proof: $output" }
          Write-Host $output
        }
        finally {
          $remaining = @(& $Docker ps -a --filter "name=^/$container`$" --format '{{.ID}}')
          if ($remaining.Count -gt 0) { & $Docker rm -f $container | Out-Null; throw 'Pipe fixture did not exit/remove itself' }
        }
        if (($before | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Pipe inputs changed during execution' }
        $record.kind = 'execution'; $record.command = "$Docker $($args -join ' ')"
        $record.output = "$output`nCLEANUP container/private tmpfs/socket paths discarded"
        $record.host = 'Docker Linux x64; image digest in command; OS/runtime in output. Separate Windows compiler/reference evidence.'
        $record.scope = 'Actual paired Linux runtime execution: canonical pipes, transferred connected handle ownership, regular-file IsAsync and working async reads, SendPacketsElement constructor acceptance, current blocking/nonblocking flags on both pipe ends and wrappers, bind/shared-path 0600 ratchet and cleanup. Same-user only; no cross-user SendPacketsAsync, later external fcntl mutation or GA claim.'
        [IO.File]::WriteAllText((Join-Path $directory 'linux.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
      }
    }
  }
  $sample = 'samples/net11.0/ShoreCalibration/ShoreCalibration.csproj'
  Assert-LfInputs $sample
  $directory = Join-Path $root 'samples/net11.0/ShoreCalibration'
  Push-Location $directory
  try { $evaluation = (& $Net11Dotnet msbuild ShoreCalibration.csproj -getProperty:TargetFramework,UseMonoRuntime,RuntimeIdentifier,_WebAssemblyTargetsFile,WasmBuildNative | Out-String).Trim() }
  finally { Pop-Location }
  $record = [ordered]@{
    kind='official-review'; sample=$sample; target='net11.0'; sdk='11.0.100-rc.2.26504.105'; language='15.0'
    command='Review pinned native-boundaries.sources.json CoreCLR relink target, SDK selection, Emscripten6.0.2 requirements and exact canonical C/C#/project/JS inputs; evaluate project properties only'
    scope='Intended SDK/C# pins; official source review plus property evaluation only, no build/native compilation/relink/browser execution. Matching CoreCLR browser runtime/WebAssembly/Emscripten6.0.2 packs are unavailable locally and in the existing Docker authoring image; Mono workload labels and older3.1.56 assets do not qualify. PIN-699 sample-specific not-tested exclusion.'
    recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); sourceCommit=(& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim(); sourceDirty=$true
    inputs=(Get-EvidenceInputs $root $sample); dependencies=@(); workloads=@(); nativeToolchains=@()
    host=[Runtime.InteropServices.RuntimeInformation]::OSDescription; sdkInfo='Intended .NET11RC2 SDK; no matched native/browser toolchain measured'
    workloadInfo='Existing wasm-tools label is not a matching CoreCLR relink installation; intended Emscripten6.0.2 from pinned source, not measured'
    output="$evaluation`nSOURCE REVIEW: $($sources.sources.Count) immutable official source entries; no native result claimed. Expected only on a matching toolchain: shore_convert(60)=42."
  }
  [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved" } }
  foreach ($scope in $persistent.Keys) { if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved" } }
}
