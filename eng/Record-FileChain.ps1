[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
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
    foreach ($selection in @(
      @{ File='hosting-configuration'; Ids=@('ignored-load','io-callback','file-provider-10','file-provider-11','watcher-paths','watcher-10','watcher-11','fileproviders.physical-project') },
      @{ File='cryptography-platforms'; Ids=@('aia-limit','aia-chain-10','aia-chain-11') }
    )) {
      $manifest = Get-Content -LiteralPath (Join-Path $root "docs/planning/audits/$($selection.File).sources.json") -Raw | ConvertFrom-Json
      foreach ($id in $selection.Ids) {
        $sources = @($manifest.sources | Where-Object id -CEQ $id)
        if ($sources.Count -ne 1) { throw "Expected one source: $id" }
        $source = $sources[0]
        $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if ($hash -cne $source.sha256) { throw "Source changed: $id" }
        Write-Host "SOURCE $id $hash $($source.url)"
      }
    }
  }
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $sdk = if ($variant -eq 'net10.0') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    $ref = if ($variant -eq 'net10.0') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    foreach ($name in 'PlanetariumSchedule','GardenObservations','ArchiveManifest') {
      $sample = "samples/$variant/$name/$name.csproj"
      $kind = if ($name -eq 'ArchiveManifest') { 'compile' } else { 'compile-and-run' }
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind $kind
      $recordPath = Join-Path $root "samples/$variant/$name/verification-evidence.json"
      $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
      if ($name -ne 'ArchiveManifest' -and -not $record.output.Contains("runtime=.NET $ref;")) { throw "Wrong runtime: $sample" }
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try {
        $resolved = & $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json
      }
      finally { Pop-Location }
      $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
      if ($pack.Count -ne 1 -or $pack[0].NuGetPackageVersion -cne $ref -or $pack[0].TargetFramework -cne $variant) { throw "Wrong reference pack: $sample" }
      $artifacts = @(
        (Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
        (Join-Path $pack[0].Path "ref/$variant/System.Runtime.dll"),
        (Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$ref/System.Private.CoreLib.dll")
      )
      $assets = Get-Content -LiteralPath (Join-Path (Split-Path $project) 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      $target = $assets.targets[$variant]
      $cache = @($assets.packageFolders.Keys)[0]
      foreach ($dependency in $target.Keys | Sort-Object -CaseSensitive) {
        foreach ($kind in 'compile','runtime') {
          foreach ($asset in @($target[$dependency][$kind].Keys | Where-Object { $_.EndsWith('.dll') })) {
            $path = Join-Path $cache ($assets.libraries[$dependency].path + '/' + $asset)
            $artifacts += $path
            $record.output += "`nSELECTED $kind $dependency/$asset"
          }
        }
      }
      foreach ($artifact in $artifacts | Sort-Object -Unique) {
        $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion
        $hash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant()
        $record.output += "`nARTIFACT $artifact version=$version sha256=$hash"
      }
      $record.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; SHA256 compiler, reference, CoreLib and selected compile/runtime package assets"
      $record.scope = switch ($name) {
        'PlanetariumSchedule' { 'Real Windows NTFS JSON initial/explicit/reload parse, missing/deletion/recreation, exclusive-lock open failure/recovery; callback exception/Ignore/data/token matrix. Synthetic CreateReadStream and GetFileInfo faults separately labelled. Canonical policy, bounded waits and cleanup. Unignored real automatic reload is isolated in a reaped child on each release; no universal OS notification count or GA proof.' }
        'GardenObservations' { 'Both actual constructors: null/NUL/unrelated sibling and shared-prefix validation; real equal/descendant tokens on both versions; current empty/relative/deferred root creation tokens, ancestor recursion and canonical factories. Older empty/deferred token failure and relative/ancestor construction only. Windows NTFS x64, bounded waits/disposal/cleanup; no polling/browser/symlink/other-filesystem/GA claim.' }
        'ArchiveManifest' { 'Actual Windows compiler/reference assets for paired Linux runtime consumers; see linux.evidence.json for separate real OpenSSL execution. No Windows chain behavior or GA claim.' }
      }
      [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
      if ($name -eq 'ArchiveManifest') {
        $image = if ($variant -eq 'net10.0') {
          'mcr.microsoft.com/dotnet/runtime@sha256:e454ed02b8c903dff0d7fd40a63451e55f16c186463d168a78e584738244f939'
        } else { 'mcr.microsoft.com/dotnet/runtime@sha256:3b52667dbaff185a1ddd4cefb16d9517e6945f6e61257faa2e57e6040877d95b' }
        $docker = 'C:/Program Files/Docker/Docker/resources/bin/docker.exe'
        $beforeInputs = Get-EvidenceInputs $root $sample
        $record.kind = 'execution'; $record.output = ''; $record.command = ''
        foreach ($mode in 'aia','supplied','untrusted','invalid') {
          $container = 'playbook-chain-' + [guid]::NewGuid().ToString('N')
          $arguments = @('run','--name',$container,'--rm','--network','none','--read-only','--tmpfs','/tmp','--tmpfs','/root',
            '--env','HOME=/root','--env','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0','--mount',
            "type=bind,source=$root/samples/$variant/ArchiveManifest/bin/Release/$variant,target=/app,readonly",
            $image,'/usr/share/dotnet/dotnet','/app/ArchiveManifest.dll',$mode)
          try {
            $record.command += "$docker $($arguments -join ' ')`n"
            $output = (& $docker @arguments | Out-String).Trim()
            Write-Host $output
            if (-not $output.Contains("runtime=.NET $ref;") -or -not $output.Contains("PASS $mode;") -or -not $output.Contains('NATIVE OpenSSL') -or -not $output.Contains('privateHome=/root; preexistingDotnetCache=False')) { throw "Incomplete Linux evidence: $mode" }
            $record.output += "$output`n"
          }
          finally {
            # --rm removes each container including its ephemeral HOME/cache/private material.
            $remaining = @(& $docker ps -a --filter "name=^/$container`$" --format '{{.ID}}')
            if ($remaining.Count -gt 0) { & $docker rm -f $container | Out-Null; throw "Fixture did not exit/remove itself: $container" }
            $record.output += "CLEANUP removed container=$container; private tmpfs and cache discarded`n"
          }
        }
        $afterInputs = Get-EvidenceInputs $root $sample
        if (($beforeInputs | ConvertTo-Json -Compress) -cne ($afterInputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during Linux execution.' }
        $record.host = 'Docker Linux x64; exact runtime image digest in command; native OpenSSL and OS in output. Compilation uses the separately recorded Windows SDK.'
        $record.scope = 'Four fresh network-none read-only containers per runtime, private HOME/tmpfs and random ephemeral CA: three-AIA old success/current two-request PartialChain; supplied intermediates/downloads disabled success with zero requests; untrusted root and expired chain rejection, real signed manifest/tamper controls and canonical verifier. No shared trust writes; container/listener/private-cache cleanup. Offline NoCheck fixture is not online revocation or TLS proof.'
        [IO.File]::WriteAllText((Join-Path (Split-Path $recordPath) 'linux.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
      }

    }
  }
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  foreach ($scope in $persistent.Keys) {
    if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved." }
  }
}
