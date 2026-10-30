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
  if ($VerifySources) {
    $manifest = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/numeric-and-parsing.sources.json') -Raw | ConvertFrom-Json
    $sourceIds = @('rounding','small-integers','previous-runtime-api','current-runtime-api','previous-numeric-api','current-numeric-api','round-implementation','round-tensor-implementation','libraries-preview2')
    $selected = foreach ($sourceId in $sourceIds) {
      $matches = @($manifest.sources | Where-Object id -CEQ $sourceId)
      if ($matches.Count -ne 1) { throw "Expected exactly one source: $sourceId" }
      $matches[0]
    }
    foreach ($source in $selected) {
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
    foreach ($name in 'MeasurementExport','RecordOffsets') {
      $sample = "samples/$variant/$name/$name.csproj"
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
      $recordPath = Join-Path $root "samples/$variant/$name/verification-evidence.json"
      $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
      if (-not $record.output.Contains("runtime=.NET $ref;")) { throw "Wrong actual runtime for $sample" }
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try { $resolved = & $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json }
      finally { Pop-Location }
      $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
      if ($pack.Count -ne 1 -or $pack[0].NuGetPackageVersion -cne $ref -or $pack[0].TargetFramework -cne $variant) { throw "Wrong reference pack for $sample" }
      $record.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack"
      foreach ($artifact in @(
        (Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
        (Join-Path $pack[0].Path "ref/$variant/System.Runtime.dll"),
        (Join-Path $pack[0].Path "ref/$variant/System.Runtime.InteropServices.dll")
      )) {
        $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion
        $hash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant()
        $record.output += "`nBUILD ARTIFACT $artifact version=$version sha256=$hash"
      }
      if ($name -eq 'MeasurementExport') {
        $asset = Join-Path $root "samples/$variant/$name/bin/Release/$variant/System.Numerics.Tensors.dll"
        $record.output += "`nPACKAGE ASSET $asset sha256=$((Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash.ToLowerInvariant()); selected lib/$variant; compile/runtime asset recorded in project.assets.json. No selected Round type/method experimental attribute; compilation uses no opt-in."
      }
      $record.scope = 'Actual pinned SDK/reference/runtime and locked tensor package executions for original fractional-rounding or record-offset policies. Runtime-parsed inputs separate execution from compiler folding; exact inputs/results and artifact hashes follow. Windows x64 CoreCLR JIT only; no interpreter, Native AOT, Mono, other-OS/architecture, net10/new-package route or actual-GA proof.'
      [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
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
