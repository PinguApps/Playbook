[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Sample,
  [string]$Dotnet = 'dotnet',
  [string]$RuntimeDotnet = $Dotnet,
  [ValidateSet('compile', 'cross-compile', 'compile-and-run')][string]$Kind = 'compile-and-run',
  [string[]]$NativeToolchains = @()
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$project = Resolve-EvidencePath $root $Sample
$directory = Split-Path $project
$manifest = Get-Content (Join-Path $directory 'verification.json') -Raw | ConvertFrom-Json -AsHashtable
$buildArguments = @($manifest.buildArguments)
if ((ConvertTo-Json -InputObject @($NativeToolchains) -Compress) -cne (ConvertTo-Json -InputObject @($manifest.nativeToolchains) -Compress)) {
  throw 'Supply measured native toolchain versions matching verification.json; never infer native versions from a successful .NET build.'
}
$before = Get-EvidenceInputs $root $Sample
Push-Location $directory
try {
  $sdk = (& $Dotnet --version | Out-String).Trim()
  $pin = Get-Content global.json -Raw | ConvertFrom-Json
  if ($sdk -cne $pin.sdk.version) { throw "Actual SDK $sdk does not match pin $($pin.sdk.version)." }
  $info = (& $Dotnet --info | Out-String).Trim()
  $workloadInfo = (& $Dotnet workload list | Out-String).Trim()
  foreach ($workload in $manifest.workloads) {
    $parts = $workload.Split('/', 2)
    if ($parts.Count -ne 2 -or $workloadInfo -notmatch "(?m)^\s*$([regex]::Escape($parts[0]))\s+$([regex]::Escape($parts[1]))\s") {
      throw "Required workload baseline not installed: $workload"
    }
  }
  $buildArgs = @('build', [IO.Path]::GetFileName($project), '--configuration', 'Release', '-p:RestoreLockedMode=true') + $buildArguments
  $buildOutput = (& $Dotnet @buildArgs | Out-String).Trim()
  Write-Host $buildOutput
  $propertyArgs = if ([IO.Path]::GetExtension($project) -eq '.cs') {
    @('build', $project, '-p:Configuration=Release')
  } else { @('msbuild', $project, '-p:Configuration=Release') }
  $properties = & $Dotnet @propertyArgs @buildArguments -getProperty:TargetFramework,LangVersion,TargetPath,ProjectAssetsFile | ConvertFrom-Json
  $properties = $properties.Properties
  $assets = Get-Content -LiteralPath $properties.ProjectAssetsFile -Raw | ConvertFrom-Json -AsHashtable
  $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
  if ((ConvertTo-Json -InputObject $dependencies -Compress) -cne (ConvertTo-Json -InputObject @($manifest.dependencies) -Compress)) {
    throw 'Resolved package/project dependencies do not match verification.json. Pin the actual versions and rerun.'
  }
  $commands = @("$Dotnet $($buildArgs -join ' ')")
  $output = $buildOutput
  if ($Kind -eq 'compile-and-run') {
    $commands += "$RuntimeDotnet $($properties.TargetPath)"
    $runOutput = (& $RuntimeDotnet $properties.TargetPath | Out-String).Trim()
    Write-Host $runOutput
    $output += "`n$runOutput"
  }
  $after = Get-EvidenceInputs $root $Sample
  if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Inputs changed during verification; no evidence saved.' }
  $record = [ordered]@{
    kind = $Kind; sample = $Sample; target = $properties.TargetFramework; sdk = $sdk; language = $properties.LangVersion
    command = $commands -join '; '; scope = 'See recipe scope; output records only the commands actually performed.'
    recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); sourceCommit = (& git -C $root rev-parse HEAD).Trim()
    sourceDirty = [bool](& git -C $root status --porcelain -- samples eng/Evidence.ps1 eng/Record-Evidence.ps1)
    inputs = $after; dependencies = $dependencies; workloads = @($manifest.workloads); nativeToolchains = @($NativeToolchains)
    host = [Runtime.InteropServices.RuntimeInformation]::OSDescription + ' / ' + [Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    sdkInfo = $info; workloadInfo = $workloadInfo; output = $output
  }
  [IO.File]::WriteAllText((Join-Path $directory 'verification-evidence.json'), (ConvertTo-Json $record -Depth 30).Replace("`r`n", "`n") + "`n")
}
finally { Pop-Location }
