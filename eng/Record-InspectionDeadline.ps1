[CmdletBinding()]
param(
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [switch]$VerifySources
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if (-not $IsWindows) { throw 'This bounded ordinary wait check requires Windows.' }
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$persistent = @{}
foreach ($scope in 'User','Machine') { $persistent[$scope] = [Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  if ($VerifySources) {
    $manifest = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/archive-path-handle-clock.sources.json') -Raw | ConvertFrom-Json
    $sourceIds = @('windows-clock','environment-windows','system-runtime-10','system-runtime-11')
    $selected = @($manifest.sources | Where-Object id -In $sourceIds)
    if ($selected.Count -ne 4 -or @($selected.id | Sort-Object -Unique).Count -ne 4) { throw 'Expected exactly four distinct deadline sources.' }
    foreach ($source in $selected) {
      $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash $($source.url)"
    }
  }
  foreach ($baseline in @(
    @{ target='net10.0'; dotnet=$Net10Dotnet; sdk='10.0.403'; runtime='10.0.14' },
    @{ target='net11.0'; dotnet=$Net11Dotnet; sdk='11.0.100-rc.3.26522.148'; runtime='11.0.0-rc.3.26522.148' }
  )) {
    foreach ($name in 'InspectionDeadline') {
      $sample = "samples/$($baseline.target)/$name/$name.csproj"
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $baseline.dotnet
      $directory = Split-Path (Join-Path $root $sample)
      $recordPath = Join-Path $directory 'verification-evidence.json'
      $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
      if (-not $record.output.Contains(".NET $($baseline.runtime)")) { throw "Wrong runtime: $sample" }
      Push-Location $directory
      try { $resolved = & $baseline.dotnet msbuild "$name.csproj" -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json }
      finally { Pop-Location }
      $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
      if ($pack.Count -ne 1 -or $pack[0].NuGetPackageVersion -cne $baseline.runtime) { throw "Wrong reference pack: $sample" }
      $record.command += "; $($baseline.dotnet) msbuild $sample -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack"
      $artifacts = @(
        (Join-Path (Split-Path $baseline.dotnet) "sdk/$($baseline.sdk)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
        (Join-Path $pack[0].Path "ref/$($baseline.target)/System.Runtime.dll"),
        (Join-Path (Split-Path $baseline.dotnet) "shared/Microsoft.NETCore.App/$($baseline.runtime)/System.Private.CoreLib.dll"),
        (Join-Path (Split-Path $baseline.dotnet) "shared/Microsoft.NETCore.App/$($baseline.runtime)/coreclr.dll")
      )
      foreach ($artifact in $artifacts) {
        $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion
        $record.output += "`nBUILD ARTIFACT $artifact version=$version sha256=$((Get-FileHash -LiteralPath $artifact).Hash.ToLowerInvariant())"
      }
      [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
    }
  }
  Write-Host 'PASS: real paired consumers, ordinary OS waits and deterministic clock-jump policy. Physical suspend and hibernation NOT TESTED.'
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  foreach ($scope in $persistent.Keys) {
    if ($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress)) { throw "Unexpected persistent $scope environment change; shared settings preserved." }
  }
}
