[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$Record,
  [switch]$VerifySources
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$persistent = @{}
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','MSBUILDUSESERVER') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  $env:MSBUILDUSESERVER = '0'
  . (Join-Path $PSScriptRoot 'Evidence.ps1')
  if ($VerifySources) {
    $sources = Get-Content (Join-Path $root 'docs/planning/audits/identity-records.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash"
    }
  }
  $audit = Get-Content (Join-Path $root 'docs/planning/audits/runtime-security.assets.json') -Raw | ConvertFrom-Json
  $der = @{}
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $baseline = @($audit.baselines | Where-Object target -CEQ $variant)[0]
    $sdk = if ($variant -eq 'net10.0') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    foreach ($name in 'RegisteredIdentifiers','TypedDns') {
      $sample = "samples/$variant/$name/$name.csproj"
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try {
        if ($Record) {
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
          $evidence = Get-Content verification-evidence.json -Raw | ConvertFrom-Json -AsHashtable
          $output = $evidence.output
        }
        else {
          & $dotnet build $project -c Release -p:RestoreLockedMode=true
          $output = (& $dotnet "bin/Release/$variant/$name.dll" | Out-String).Trim()
          Write-Host $output
        }
        if ($output -notmatch 'PASS ') { throw "Missing consumer result: $sample" }
        if ($name -eq 'RegisteredIdentifiers') { $der[$variant] = [regex]::Match($output,'DER=([A-F0-9]+)').Groups[1].Value }
        $pack = (& $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack |
          Where-Object Identity -CEQ 'Microsoft.NETCore.App'
        if ($pack.NuGetPackageVersion -cne $baseline.referencePack) { throw "Reference pack drift: $sample" }
        $assemblies = if ($name -eq 'TypedDns') { @('System.Runtime.dll','System.Net.NameResolution.dll') } else { @('System.Runtime.dll','System.Security.Cryptography.dll') }
        $artifacts = @()
        foreach ($assembly in $assemblies) {
          $path = Join-Path $pack.Path "ref/$variant/$assembly"
          $expected = @($baseline.framework | Where-Object name -CEQ $assembly)[0].sha256
          if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $expected) { throw "Audit asset mismatch: $path" }
          $artifacts += $path
        }
        if ($name -eq 'RegisteredIdentifiers') { $artifacts += Join-Path $pack.Path "ref/$variant/System.Formats.Asn1.dll" }
        $artifacts += Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
        $artifacts += Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$($baseline.referencePack)/System.Private.CoreLib.dll"
        if ($name -eq 'TypedDns' -and $IsWindows) { $artifacts += Join-Path ([Environment]::SystemDirectory) 'dnsapi.dll' }
        if ($name -eq 'TypedDns' -and $variant -eq 'net10.0') {
          $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
          $packageDirectory = Join-Path @($assets.packageFolders.Keys)[0] 'dnsclient/1.8.0'
          $packageAssembly = Join-Path $packageDirectory 'lib/net8.0/DnsClient.dll'
          if ((Get-FileHash "bin/Release/$variant/DnsClient.dll").Hash -cne (Get-FileHash $packageAssembly).Hash) { throw 'Wrong executed package assembly.' }
          $artifacts += $packageAssembly, (Join-Path $packageDirectory 'dnsclient.1.8.0.nupkg')
        }
        $lines = foreach ($path in $artifacts) {
          "ARTIFACT $path version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) sha256=$((Get-FileHash $path).Hash.ToLowerInvariant())"
        }
        $lines | ForEach-Object { Write-Host $_ }
        if ($Record) {
          $evidence.output += "`n" + ($lines -join "`n")
          $evidence.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; compare selected reference SHA256 with runtime-security.assets.json; SHA256 compiler/CoreLib, DNSAPI or resolved/executed DnsClient net8.0 asset/archive as applicable"
          $evidence.scope = 'Real Windows x64 certificate SAN DER/invalid OID or authoritative loopback typed DNS records, TTL/status, two-server override and pre/in-flight cancellation. Exact paired audit references/compiler/runtime/package/native identities. Separate Linux record measures managed DNS negative TTL/custom port. No trust/connection, public zone, DNSSEC or GA proof.'
          [IO.File]::WriteAllText((Join-Path (Split-Path $project) 'verification-evidence.json'), ($evidence | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
        if ($name -eq 'TypedDns') {
          $image = if ($variant -eq 'net10.0') { 'mcr.microsoft.com/dotnet/runtime@sha256:e454ed02b8c903dff0d7fd40a63451e55f16c186463d168a78e584738244f939' }
            else { 'mcr.microsoft.com/dotnet/runtime@sha256:3b52667dbaff185a1ddd4cefb16d9517e6945f6e61257faa2e57e6040877d95b' }
          $docker = 'C:/Program Files/Docker/Docker/resources/bin/docker.exe'
          $inputs = Get-EvidenceInputs $root $sample
          $container = 'playbook-typed-dns-' + [guid]::NewGuid().ToString('N')
          $arguments = @('run','--name',$container,'--rm','--network','none','--read-only','--tmpfs','/tmp','--tmpfs','/root',
            '--env','HOME=/root','--env','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0','--mount',
            "type=bind,source=$root/samples/$variant/$name/bin/Release/$variant,target=/app,readonly",$image,
            '/usr/share/dotnet/dotnet',"/app/$name.dll")
          try {
            $linuxOutput = (& $docker @arguments | Out-String).Trim()
            Write-Host $linuxOutput
            if ($linuxOutput -notmatch 'PASS ' -or $linuxOutput -notmatch 'NEGATIVE absent NxDomain ttl=45') { throw 'Incomplete Linux DNS proof.' }
          }
          finally {
            $remaining = @(& $docker ps -a --filter "name=^/$container`$" --format '{{.ID}}')
            if ($remaining.Count -gt 0) { & $docker rm -f $container | Out-Null; throw "Fixture did not exit/remove itself: $container" }
          }
          if (($inputs | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during Linux execution.' }
          if ($Record) {
            $evidence.kind = 'execution'; $evidence.command = "$docker $($arguments -join ' '); docker ps -a --filter name=^/$container`$ --format {{.ID}}"
            $evidence.output = "$linuxOutput`nCLEANUP container=$container absent; private tmpfs discarded."
            $evidence.host = 'Docker Linux x64; immutable runtime image in command; separately captured Windows compilation.'
            $evidence.scope = 'Actual network-none Linux x64 loopback UDP DNS with two explicit ephemeral-port servers; all seven record kinds/fields/TTLs, NODATA versus NXDOMAIN, min(SOA TTL120,MINIMUM45)=45, absent SOA zero, server override and pre/in-flight cancellation. No system DNS configuration edits or public/network identity claim.'
            [IO.File]::WriteAllText((Join-Path (Split-Path $project) 'linux.evidence.json'), ($evidence | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
          }
        }
      }
      finally { Pop-Location }
    }
  }
  if (-not $der['net10.0'] -or $der['net10.0'] -cne $der['net11.0']) { throw 'Paired SAN DER differs.' }
  Write-Host 'PASS: paired identical certificate SAN DER and real Windows/Linux typed DNS behavior; PATH/persistent settings checked.'
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
