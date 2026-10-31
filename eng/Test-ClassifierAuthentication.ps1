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
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','MSBUILDUSESERVER','REQUIRE_KMAC') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  $env:MSBUILDUSESERVER = '0'
  $env:REQUIRE_KMAC = '1' # Negative support alone cannot satisfy this delivery's KMAC proof.
  if ($VerifySources) {
    $sources = Get-Content (Join-Path $root 'docs/planning/audits/classifier-authentication.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash"
    }
  }
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in 'TideOffsets','HarbourPackets') {
      $sample = "samples/$variant/$name/$name.csproj"
      $project = Join-Path $root $sample
      $auditName = if ($name -eq 'TideOffsets') { 'serialization' } else { 'runtime-security' }
      $assembly = if ($name -eq 'TideOffsets') { 'System.Text.Json.dll' } else { 'System.Security.Cryptography.dll' }
      $audit = Get-Content (Join-Path $root "docs/planning/audits/$auditName.assets.json") -Raw | ConvertFrom-Json
      $baseline = @($audit.baselines | Where-Object target -CEQ $variant)[0]
      Push-Location (Split-Path $project)
      try {
        if ($Record) {
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
        }
        else {
          & $dotnet build $project -c Release -p:RestoreLockedMode=true
          & $dotnet "bin/Release/$variant/$name.dll"
        }
        $pack = (& $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack |
          Where-Object Identity -CEQ 'Microsoft.NETCore.App'
        if ($pack.NuGetPackageVersion -cne $baseline.referencePack) { throw "Reference pack drift: $sample" }
        $reference = Join-Path $pack.Path "ref/$variant/$assembly"
        $expected = @($baseline.framework | Where-Object name -CEQ $assembly)[0].sha256
        if ((Get-FileHash $reference).Hash.ToLowerInvariant() -cne $expected) { throw "Audit reference mismatch: $reference" }
        $sdk = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
        $sdkRoot = Join-Path (Split-Path $dotnet) "sdk/$sdk"
        $runtime = Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$($baseline.referencePack)"
        $paths = @($reference, (Join-Path $sdkRoot 'Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll'),
          (Join-Path $runtime 'System.Private.CoreLib.dll'), (Join-Path $runtime $assembly))
        if ($name -eq 'TideOffsets') { $paths += Join-Path $pack.Path 'analyzers/dotnet/cs/System.Text.Json.SourceGeneration.dll' }
        $lines = foreach ($path in $paths) {
          "ARTIFACT $path version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) sha256=$((Get-FileHash $path).Hash.ToLowerInvariant())"
        }
        $lines | ForEach-Object { Write-Host $_ }
        if ($Record) {
          $recordPath = Join-Path (Split-Path $project) 'verification-evidence.json'
          $evidence = Get-Content $recordPath -Raw | ConvertFrom-Json -AsHashtable
          $evidence.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; compare framework reference SHA256 with $auditName.assets.json; SHA256 compiler/CoreLib/runtime implementation and JSON generator where used; REQUIRE_KMAC=1"
          $evidence.output += "`n" + ($lines -join "`n")
          $evidence.scope = 'Actual Windows x64 paired value-classifier or HMAC/KMAC consumer. Framework references match source audits; no additional NuGet package or older-target package route. Output identifies exact inputs, behavior, compiler/runtime/generator and measured CNG native assets. No union-null, timing, AOT, other-platform or GA claim.'
          [IO.File]::WriteAllText($recordPath, ($evidence | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
      }
      finally { Pop-Location }
    }
  }
  Write-Host 'PASS: four real classifier/authenticator consumers, positive KMAC provider and exact audit references.'
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
