[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [ValidateSet('MooringReport','OrchardRoutes','SeedImports')]
  [string[]]$SampleNames = @('MooringReport','OrchardRoutes','SeedImports'),
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
    $sources = Get-Content (Join-Path $root 'docs/planning/audits/text-boundaries.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash"
    }
  }
  $audit = Get-Content (Join-Path $root 'docs/planning/audits/data-io.assets.json') -Raw | ConvertFrom-Json
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $baseline = @($audit.baselines | Where-Object target -CEQ $variant)[0]
    foreach ($name in $SampleNames) {
      $sample = "samples/$variant/$name/$name.csproj"
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try {
        if ($Record) { & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet }
        else {
          & $dotnet build $project -c Release -p:RestoreLockedMode=true
          & $dotnet "bin/Release/$variant/$name.dll"
        }
        $pack = (& $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack |
          Where-Object Identity -CEQ 'Microsoft.NETCore.App'
        if ($pack.NuGetPackageVersion -cne $baseline.referencePack) { throw "Reference pack drift: $sample" }
        $artifacts = @()
        foreach ($assembly in 'System.Runtime.dll','System.Runtime.Extensions.dll','System.Globalization.dll') {
          $path = Join-Path $pack.Path "ref/$variant/$assembly"
          $expected = @($baseline.framework | Where-Object name -CEQ $assembly)[0].sha256
          if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $expected) { throw "Audit asset mismatch: $path" }
          $artifacts += $path
        }
        $sdk = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
        $artifacts += Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
        $artifacts += Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$($baseline.referencePack)/System.Private.CoreLib.dll"
        $lines = foreach ($path in $artifacts) {
          "ARTIFACT $path version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) sha256=$((Get-FileHash $path).Hash.ToLowerInvariant())"
        }
        $lines | ForEach-Object { Write-Host $_ }
        if ($Record) {
          $recordPath = Join-Path (Split-Path $project) 'verification-evidence.json'
          $evidence = Get-Content $recordPath -Raw | ConvertFrom-Json -AsHashtable
          $evidence.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; compare selected reference/package SHA256 to data-io.assets.json; SHA256 compiler/CoreLib"
          $evidence.output += "`n" + ($lines -join "`n")
          $evidence.scope = 'Actual Windows x64 paired text-output, Windows ICU IDN or directory consumer; focused inputs/failures and loaded ICU identity are recorded in output. Exact audit-matching reference/package assets and measured compiler/CoreLib. No other-platform, NLS/invariant IDNA, rollback, arbitrary-writer cancellation, native UI, performance or final-GA claim.'
          [IO.File]::WriteAllText($recordPath, ($evidence | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
      }
      finally { Pop-Location }
    }
  }
  Write-Host 'PASS: selected paired text/IDN/directory consumers and actual audit-matching artifacts.'
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
