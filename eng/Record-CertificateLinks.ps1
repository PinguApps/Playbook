[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [ValidateSet('LaboratoryInspector','SurveyExport','RehearsalRoom')]
  [string[]]$SampleNames = @('LaboratoryInspector','SurveyExport','RehearsalRoom'),
  [switch]$VerifySources
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$persistent = @{}
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','MSBUILDUSESERVER') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:MSBUILDUSESERVER = '0'
  . (Join-Path $PSScriptRoot 'Evidence.ps1')
  if ($VerifySources) {
    $manifest = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/certificate-links.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $manifest.sources) {
      $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash $($source.url)"
    }
  }
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $sdk = if ($variant -eq 'net10.0') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    $ref = if ($variant -eq 'net10.0') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    foreach ($name in $SampleNames) {
      $sample = "samples/$variant/$name/$name.csproj"
      $kind = if ($name -eq 'RehearsalRoom') { 'compile' } else { 'compile-and-run' }
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind $kind
      $recordPath = Join-Path $root "samples/$variant/$name/verification-evidence.json"
      $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
      if ($name -ne 'RehearsalRoom' -and -not $record.output.Contains("runtime=.NET $ref;")) { throw "Wrong runtime: $sample" }
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try { $resolved = & $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json }
      finally { Pop-Location }
      $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
      if ($pack.Count -ne 1 -or $pack[0].NuGetPackageVersion -cne $ref -or $pack[0].TargetFramework -cne $variant) { throw "Wrong reference pack: $sample" }
      $artifacts = @(
        (Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
        (Join-Path $pack[0].Path "ref/$variant/System.Runtime.dll"),
        (Join-Path $pack[0].Path "ref/$variant/System.Security.Cryptography.dll"),
        (Join-Path $pack[0].Path "ref/$variant/System.Net.Security.dll"),
        (Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$ref/System.Private.CoreLib.dll")
      )
      if ($name -eq 'SurveyExport') { $artifacts += (Join-Path ([Environment]::SystemDirectory) 'kernel32.dll') }
      foreach ($artifact in $artifacts) {
        $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion
        $hash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant()
        $record.output += "`nARTIFACT $artifact version=$version sha256=$hash"
      }
      $record.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; SHA256 compiler, selected references, CoreLib and kernel32 for native hard links"
      $record.scope = switch ($name) {
        'LaboratoryInspector' { 'Windows x64 actual paired consumers: old setter stale-cache, typed constructor/CopyFrom fields/bytes/OID/Critical including reverse mutation, malformed lazy decode boundary and getter-only use. Current warnings-as-errors, isolated old-setter current diagnostic probe below. No network/native fixture or other-platform/GA claim.' }
        'SurveyExport' { 'Windows x64 local NTFS: old native wrapper/current File and FileInfo canonical consumers, bidirectional writes, deleting either name preserves data, destination-exists/absent-target/directory rejection and unchanged failed output; temporary-tree cleanup. No other OS/filesystem, network-share or cross-volume outcome claimed.' }
        'RehearsalRoom' { 'Actual Windows compiler/reference assets for paired Linux TLS runtime consumers. Separate linux.evidence.json records real native handshake/data/failure execution; no Windows TLS or GA behavior claim.' }
      }
      if ($name -eq 'LaboratoryInspector' -and $variant -eq 'net11.0') {
        $probe = Join-Path $root 'artifacts/certificate-links/setter-probe'
        New-Item -ItemType Directory -Path $probe -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path (Split-Path $project) 'global.json') -Destination $probe
        Copy-Item -LiteralPath (Join-Path $root 'samples/net10.0/LaboratoryInspector/ExtensionUpdate.cs') -Destination $probe
        '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>15.0</LangVersion><OutputType>Library</OutputType><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>' | Set-Content -LiteralPath (Join-Path $probe 'SetterProbe.csproj')
        Push-Location $probe
        try {
          $PSNativeCommandUseErrorActionPreference = $false
          $diagnostic = (& $dotnet build SetterProbe.csproj -c Release 2>&1 | Out-String).Trim()
          $exit = $LASTEXITCODE
        }
        finally { $PSNativeCommandUseErrorActionPreference = $true; $global:LASTEXITCODE = 0; Pop-Location }
        if ($exit -eq 0 -or $diagnostic -notmatch 'error SYSLIB0065' -or $diagnostic -match 'error (?!SYSLIB0065)[A-Z]+\d+') { throw "Unexpected setter diagnostic: $diagnostic" }
        $record.command += "; cd $probe; $dotnet build SetterProbe.csproj -c Release (expected SYSLIB0065 failure; unchanged old setter SHA256=$((Get-FileHash -LiteralPath (Join-Path $probe 'ExtensionUpdate.cs')).Hash.ToLowerInvariant()))"
        $record.output += "`nEXPECTED old setter current-target exit=$exit`n$diagnostic"
        Write-Host 'PASS current setter diagnostic SYSLIB0065; no suppression'
      }
      [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
      if ($name -eq 'RehearsalRoom') {
        $image = if ($variant -eq 'net10.0') { 'mcr.microsoft.com/dotnet/runtime@sha256:e454ed02b8c903dff0d7fd40a63451e55f16c186463d168a78e584738244f939' }
          else { 'mcr.microsoft.com/dotnet/runtime@sha256:3b52667dbaff185a1ddd4cefb16d9517e6945f6e61257faa2e57e6040877d95b' }
        $docker = 'C:/Program Files/Docker/Docker/resources/bin/docker.exe'
        $beforeInputs = Get-EvidenceInputs $root $sample
        $record.kind = 'execution'; $record.output = ''; $record.command = ''
        foreach ($mode in 'default','supplied','untrusted','invalid') {
          $container = 'playbook-room-' + [guid]::NewGuid().ToString('N')
          $arguments = @('run','--name',$container,'--rm','--network','none','--read-only','--tmpfs','/tmp','--tmpfs','/root',
            '--env','HOME=/root','--env','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0','--mount',
            "type=bind,source=$root/samples/$variant/RehearsalRoom/bin/Release/$variant,target=/app,readonly",$image,
            '/usr/share/dotnet/dotnet','/app/RehearsalRoom.dll',$mode)
          try {
            $record.command += "$docker $($arguments -join ' ')`n"
            $output = (& $docker @arguments | Out-String).Trim()
            Write-Host $output
            if (-not $output.Contains("runtime=.NET $ref;") -or -not $output.Contains("PASS $mode;") -or -not $output.Contains('NATIVE OpenSSL') -or -not $output.Contains('privateHome=/root; preexistingDotnetCache=False')) { throw "Incomplete TLS evidence: $mode" }
            $record.output += "$output`n"
          }
          finally {
            $remaining = @(& $docker ps -a --filter "name=^/$container`$" --format '{{.ID}}')
            if ($remaining.Count -gt 0) { & $docker rm -f $container | Out-Null; throw "Fixture did not exit/remove itself: $container" }
            $record.output += "CLEANUP removed container=$container; private tmpfs/cache/keys discarded`n"
          }
        }
        if (($beforeInputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during TLS execution' }
        $record.host = 'Docker Linux x64; exact runtime image digests in command, native OpenSSL/OS in output; separately recorded Windows compilation.'
        $record.scope = 'Four fresh network-none read-only containers per runtime with private HOME/tmpfs and random CA/server/client: no-custom-policy old AIA success/new PartialChain without requests; explicit server ExtraStore/downloads-disabled success, negotiated mutual TLS and booking/ACK exchange; untrusted root and expired client rejection without AIA. Strict callback preserves validation. Offline NoCheck does not prove online revocation; no shared trust/cache writes, compatibility switch, Windows TLS or GA claim.'
        [IO.File]::WriteAllText((Join-Path (Split-Path $recordPath) 'linux.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
      }
    }
  }
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved" } }
  foreach ($scope in $persistent.Keys) { if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved" } }
}
