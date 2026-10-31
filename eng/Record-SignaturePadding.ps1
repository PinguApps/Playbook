#Requires -Version 7.0
[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$VerifySources
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (-not $IsWindows) { throw 'Real Windows CAPI checks are required by this recorder.' }
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$persistent = @{}
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','MSBUILDUSESERVER') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('playbook-signature-padding-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:MSBUILDUSESERVER = '0'
  . (Join-Path $PSScriptRoot 'Evidence.ps1')
  if ($VerifySources) {
    $manifest = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/signature-padding.sources.json') -Raw | ConvertFrom-Json
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
    foreach ($name in 'CoastalReports','InstrumentGateway') {
      $sample = "samples/$variant/$name/$name.csproj"
      $kind = if ($name -eq 'InstrumentGateway') { 'compile' } else { 'compile-and-run' }
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind $kind
      $recordPath = Join-Path $root "samples/$variant/$name/verification-evidence.json"
      $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try { $resolved = & $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json }
      finally { Pop-Location }
      $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
      if ($pack.Count -ne 1 -or $pack[0].NuGetPackageVersion -cne $ref -or $pack[0].TargetFramework -cne $variant) { throw "Wrong reference pack: $sample" }
      $artifacts = @(
        (Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
        (Join-Path $pack[0].Path "ref/$variant/System.Security.Cryptography.dll"),
        (Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$ref/System.Security.Cryptography.dll"),
        (Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$ref/System.Private.CoreLib.dll"),
        (Join-Path ([Environment]::SystemDirectory) 'advapi32.dll'),
        (Join-Path ([Environment]::SystemDirectory) 'rsaenh.dll'),
        (Join-Path ([Environment]::SystemDirectory) 'ncrypt.dll')
      )
      foreach ($artifact in $artifacts) {
        $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion
        $hash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant()
        $record.output += "`nARTIFACT $artifact version=$version sha256=$hash"
      }
      $record.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; SHA256 compiler, crypto reference/implementation, CoreLib and Windows native libraries"
      if ($name -eq 'CoastalReports') {
        if (-not $record.output.Contains("runtime=.NET $ref;")) { throw "Wrong runtime: $sample" }
        $record.scope = 'Windows x64 paired compilation/execution: imported public-only DSA1024/SHA1/P1363 archive fixture, independent ECDSA P256/SHA256/DER certificate private/public and SPKI peer verification, tampering/encoding/key/algorithm rejection and explicit unsupported-legacy contract outcome. macOS10 legacy verification and macOS11 native DSA/certificate access failures plus ECDSA operations are not-tested under PIN-699; Windows results do not establish them.'
      }
      else {
        $record.scope = 'Windows x64 paired compilation with no NuGet/workload. Cross-runtime real CAPI execution, disposable key and native identities are captured below; no production/named key container or stronger-provider redesign.'
      }
      if ($name -eq 'InstrumentGateway' -and $variant -eq 'net11.0') {
        $probe = Join-Path $fixture 'diagnostic'
        New-Item -ItemType Directory -Path $probe | Out-Null
        Copy-Item -LiteralPath (Join-Path (Split-Path $project) 'global.json') -Destination $probe
        Copy-Item -LiteralPath (Join-Path $root 'samples/net10.0/InstrumentGateway/GatewayCipher.cs') -Destination $probe
        '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net11.0</TargetFramework><LangVersion>15.0</LangVersion><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>' | Set-Content -LiteralPath (Join-Path $probe 'LegacyProbe.csproj')
        Push-Location $probe
        try {
          $PSNativeCommandUseErrorActionPreference = $false
          $diagnostic = (& $dotnet build LegacyProbe.csproj -c Release 2>&1 | Out-String).Trim()
          $exit = $LASTEXITCODE
        }
        finally { $PSNativeCommandUseErrorActionPreference = $true; $global:LASTEXITCODE = 0; Pop-Location }
        if ($exit -eq 0 -or $diagnostic -notmatch 'error SYSLIB0064' -or $diagnostic -match 'error (?!SYSLIB0064)[A-Z]+\d+') { throw "Unexpected Boolean diagnostic: $diagnostic" }
        $record.command += "; cd $probe; $dotnet build LegacyProbe.csproj -c Release (expected SYSLIB0064 failure; unchanged old source SHA256=$((Get-FileHash -LiteralPath (Join-Path $probe 'GatewayCipher.cs')).Hash.ToLowerInvariant()))"
        $record.output += "`nEXPECTED legacy Boolean overloads current-target exit=$exit`n$diagnostic"
        Write-Host 'PASS current legacy diagnostic SYSLIB0064; no suppression'
      }
      [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
    }
  }
  $execution = ''
  $commands = ''
  $beforeInputs = @{}
  foreach ($variant in 'net10.0','net11.0') { $beforeInputs[$variant] = Get-EvidenceInputs $root "samples/$variant/InstrumentGateway/InstrumentGateway.csproj" }
  foreach ($mode in 'old-send','new-exchange','old-receive') {
    $variant = if ($mode -eq 'new-exchange') { 'net11.0' } else { 'net10.0' }
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $runtime = if ($variant -eq 'net10.0') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $dll = Join-Path $root "samples/$variant/InstrumentGateway/bin/Release/$variant/InstrumentGateway.dll"
    $commands += "$dotnet $dll $mode $fixture`n"
    $output = (& $dotnet $dll $mode $fixture | Out-String).Trim()
    Write-Host $output
    if (-not $output.Contains("runtime=.NET $runtime;") -or -not $output.Contains("PASS $mode;")) { throw "Incomplete real CAPI execution: $mode" }
    $execution += "$output`n"
  }
  $resolvedFixture = [IO.Path]::GetFullPath($fixture)
  $tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\','/')
  if ((Split-Path $resolvedFixture -Parent) -cne $tempParent -or (Split-Path $resolvedFixture -Leaf) -notlike 'playbook-signature-padding-*') { throw 'Unsafe fixture cleanup path' }
  Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
  if (Test-Path -LiteralPath $resolvedFixture) { throw 'Private fixture cleanup failed' }
  foreach ($variant in 'net10.0','net11.0') {
    $sample = "samples/$variant/InstrumentGateway/InstrumentGateway.csproj"
    if (($beforeInputs[$variant] | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during CAPI execution' }
    $recordPath = Join-Path $root "samples/$variant/InstrumentGateway/verification-evidence.json"
    $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
    $record.kind = 'compile-and-run'
    $record.command += "; $commands"
    $record.output += "`n$execution"
    $record.scope = 'Actual Windows CAPI provider24 ephemeral RSA2048 key exported only to private temporary fixture: old Boolean/current explicit cross-decryption in both directions for false/Pkcs1 and true/OaepSHA1, canonical/explicit equivalence on each runtime, wrong-padding/zeroed-ciphertext rejection and same-provider OAEP-SHA256 encrypt/decrypt failure. Current unchanged old-source SYSLIB0064 probe fails without suppression; supported consumer warnings-as-errors. No production key/container, stronger-provider redesign, other-platform or GA claim.'
    $record.output += "`nCLEANUP private key/ciphertext/diagnostic fixture removed and absence checked; no persistent/named container created"
    [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
}
finally {
  $resolvedFixture = [IO.Path]::GetFullPath($fixture)
  $tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\','/')
  if ((Split-Path $resolvedFixture -Parent) -cne $tempParent -or (Split-Path $resolvedFixture -Leaf) -notlike 'playbook-signature-padding-*') { throw 'Unsafe fixture cleanup path' }
  if (Test-Path -LiteralPath $resolvedFixture) { Remove-Item -LiteralPath $resolvedFixture -Recurse -Force }
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved" } }
  foreach ($scope in $persistent.Keys) { if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved" } }
}
