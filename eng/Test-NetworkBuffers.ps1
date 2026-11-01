[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [ValidateSet('RelayDial','EndpointLedger')][string[]]$SampleNames = @('RelayDial','EndpointLedger'),
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
  if ($VerifySources) {
    $sources = Get-Content (Join-Path $root 'docs/planning/audits/network-buffers.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash"
    }
  }
  $audit = Get-Content (Join-Path $root 'docs/planning/audits/runtime-security.assets.json') -Raw | ConvertFrom-Json
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $baseline = @($audit.baselines | Where-Object target -CEQ $variant)[0]
    foreach ($name in $SampleNames) {
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
        if ($output -notmatch "PASS ${name}:") { throw "Missing consumer result: $sample" }
        $runtimeLines = @($output -split '\r?\n' | Where-Object { $_.StartsWith('RUNTIME ', [StringComparison]::Ordinal) })
        if ($runtimeLines.Count -ne 1) { throw "Missing unique consumer runtime identity: $sample" }
        $runtime = $runtimeLines[0].Substring(8) | ConvertFrom-Json
        if ($runtime.framework -cne ".NET $($baseline.referencePack)") { throw "Runtime baseline drift: $sample" }
        if ((Get-FileHash -LiteralPath $runtime.coreLibPath).Hash.ToLowerInvariant() -cne $runtime.sha256) { throw 'Loaded CoreLib hash changed.' }
        $pack = (& $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack |
          Where-Object Identity -CEQ 'Microsoft.NETCore.App'
        if ($pack.NuGetPackageVersion -cne $baseline.referencePack) { throw "Reference pack drift: $sample" }
        $assemblies = if ($name -eq 'RelayDial') { @('System.Runtime.dll','System.Net.Sockets.dll','System.Net.NameResolution.dll') } else { @('System.Runtime.dll','System.Net.Primitives.dll') }
        $artifacts = @()
        foreach ($assembly in $assemblies) {
          $path = Join-Path $pack.Path "ref/$variant/$assembly"
          $expected = @($baseline.framework | Where-Object name -CEQ $assembly)[0].sha256
          if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $expected) { throw "Audit asset mismatch: $path" }
          $artifacts += $path
        }
        $sdk = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
        $artifacts += Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
        $artifacts += $runtime.coreLibPath
        if ($name -eq 'RelayDial' -and $IsWindows) {
          $artifacts += Join-Path ([Environment]::SystemDirectory) 'ws2_32.dll'
          $artifacts += Join-Path ([Environment]::SystemDirectory) 'dnsapi.dll'
        }
        $lines = foreach ($path in $artifacts) {
          "ARTIFACT $path version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) sha256=$((Get-FileHash $path).Hash.ToLowerInvariant())"
        }
        $lines | ForEach-Object { Write-Host $_ }
        if ($Record) {
          $evidence.output += "`n" + ($lines -join "`n")
          $evidence.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; compare reference SHA256 with runtime-security.assets.json; verify consumer-emitted runtime/hash; SHA256 compiler/CoreLib and Windows socket/DNS native assets where applicable"
          $evidence.scope = 'Real Windows x64 paired loopback TCP dual-stack/racing/event/cancellation or UTF8/char endpoint consumers; exact audit-matching references and measured compiler/runtime/native identities. localhost answers checked for both loopback families; no system DNS/routes edited. No fixed latency, WAN/DNS outage, other-platform, allocation benchmark or GA proof.'
          [IO.File]::WriteAllText((Join-Path (Split-Path $project) 'verification-evidence.json'), ($evidence | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
      }
      finally { Pop-Location }
    }
  }
  Write-Host 'PASS: selected paired networking consumers, audit assets and runtime identities; PATH/persistent settings checked.'
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
