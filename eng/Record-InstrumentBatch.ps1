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
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','MSBUILDUSESERVER','PLAYBOOK_TENSOR_NATIVE_CHECKS') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:MSBUILDUSESERVER = '0'
  $env:PLAYBOOK_TENSOR_NATIVE_CHECKS = if ($RunReleaseChecks) { '1' } else { '0' }
  if ($VerifySources) {
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
    $sample = "samples/$variant/InstrumentBatch/InstrumentBatch.csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind $kind
    $recordPath = Join-Path $root "samples/$variant/InstrumentBatch/verification-evidence.json"
    $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
    $project = Join-Path $root $sample
    Push-Location (Split-Path $project)
    try { $resolved = & $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json }
    finally { Pop-Location }
    $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
    if ($pack.Count -ne 1 -or $pack[0].TargetFramework -cne $variant) { throw "Wrong reference pack for $sample" }
    $pin = Get-Content -LiteralPath (Join-Path (Split-Path $project) 'global.json') -Raw | ConvertFrom-Json
    foreach ($artifact in @(
      (Join-Path (Split-Path $dotnet) "sdk/$($pin.sdk.version)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
      (Join-Path $pack[0].Path "ref/$variant/System.Runtime.dll")
    )) {
      $record.output += "`nBUILD ARTIFACT $artifact version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion) sha256=$((Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant())"
    }
    $record.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack"
    $asset = Join-Path $root "samples/$variant/InstrumentBatch/bin/Release/$variant/System.Numerics.Tensors.dll"
    $record.output += "`nPACKAGE ASSET lib/$variant/System.Numerics.Tensors.dll sha256=$((Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash.ToLowerInvariant())"
    if ($variant -eq 'net11.0' -and -not $RunReleaseChecks) {
      $record.scope = 'Actual SDK11 RC2 compilation with locked Tensor11 RC2 net11 assets only. This package lacks the documented release/11.0 shape backport. No successful current shape/empty/Any/view/native-width execution or final SDK/package proof; PIN-962 must refresh after GA.'
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
