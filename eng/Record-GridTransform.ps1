[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$RunReleaseChecks,
  [switch]$VerifySources
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','MSBUILDUSESERVER') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:MSBUILDUSESERVER = '0'
  if ($VerifySources) {
    $audit = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/numeric-and-parsing.sources.json') -Raw | ConvertFrom-Json
    foreach ($sourceId in @('tensor-storage','tensor-guide','previous-tensor-api','current-tensor-api','previous-tensor-targets','current-tensor-targets')) {
      $sources = @($audit.sources | Where-Object id -CEQ $sourceId)
      if ($sources.Count -ne 1) { throw "Expected exactly one source: $sourceId" }
      $source = $sources[0]
      $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash $($source.url)"
    }
    $manifest = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/tensor-release.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $manifest.sources) {
      $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash $($source.url)"
    }
  }
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $kind = if ($variant -eq 'net10.0' -or $RunReleaseChecks) { 'compile-and-run' } else { 'compile' }
    $sample = "samples/$variant/GridTransform/GridTransform.csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind $kind
    $recordPath = Join-Path $root "samples/$variant/GridTransform/verification-evidence.json"
    $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
    $project = Join-Path $root $sample
    Push-Location (Split-Path $project)
    try { $resolved = & $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json }
    finally { Pop-Location }
    $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
    $expectedRef = if ($variant -eq 'net10.0') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $validRef = $pack.Count -eq 1 -and $pack[0].TargetFramework -ceq $variant
    if ($validRef) {
      $validRef = if ($variant -eq 'net11.0' -and $RunReleaseChecks) {
        $pack[0].NuGetPackageVersion -cmatch '^11\.0\.\d+$'
      } else { $pack[0].NuGetPackageVersion -ceq $expectedRef }
    }
    $validRuntime = $kind -ceq 'compile' -or $(if ($variant -eq 'net10.0') {
      $record.output.Contains('runtime=.NET 10.0.13;')
    } else { $record.output -cmatch '(?m)^runtime=\.NET 11\.0\.\d+;' })
    if (-not $validRef -or -not $validRuntime) {
      # Record-Evidence wrote this attempt; do not retain a rejected qualification.
      Remove-Item -LiteralPath $recordPath
      throw "Wrong reference pack/runtime for $sample; rejected receipt removed."
    }
    $pin = Get-Content -LiteralPath (Join-Path (Split-Path $project) 'global.json') -Raw | ConvertFrom-Json
    foreach ($artifact in @(
      (Join-Path (Split-Path $dotnet) "sdk/$($pin.sdk.version)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
      (Join-Path $pack[0].Path "ref/$variant/System.Runtime.dll")
    )) {
      $record.output += "`nBUILD ARTIFACT $artifact version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion) sha256=$((Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant())"
    }
    $record.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack"
    $asset = Join-Path $root "samples/$variant/GridTransform/bin/Release/$variant/System.Numerics.Tensors.dll"
    $record.output += "`nPACKAGE ASSET lib/$variant/System.Numerics.Tensors.dll sha256=$((Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash.ToLowerInvariant())"
    if ($variant -eq 'net11.0' -and -not $RunReleaseChecks) {
      $record.scope = 'Actual SDK11 RC2 compilation with locked Tensor11 RC2 net11 assets only. The package lacks the documented release/11.0 storage backport. Current rejection/allowed-overlap/constructor controls and final SDK/package execution are pending under PIN-1051; compilation does not satisfy its delivery gate.'
    }
    [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
