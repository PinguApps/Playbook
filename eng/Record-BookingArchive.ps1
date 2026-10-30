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
    foreach ($source in ($manifest.sources | Where-Object id -In @('date-parsing','conversions','previous-runtime-api','current-runtime-api','previous-numeric-api','current-numeric-api'))) {
      $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash $($source.url)"
    }
  }
  $matrix = @(
    @{ variant='net10.0'; name='BookingImport'; dotnet=$Net10Dotnet; runtime=$Net10Dotnet; sdk='10.0.402'; ref='10.0.13'; tfm='net10.0' },
    @{ variant='net11.0'; name='BookingImport'; dotnet=$Net11Dotnet; runtime=$Net11Dotnet; sdk='11.0.100-rc.2.26504.105'; ref='11.0.0-rc.2.26504.105'; tfm='net11.0' },
    @{ variant='net10.0'; name='InstrumentArchive'; dotnet=$Net10Dotnet; runtime=$Net10Dotnet; sdk='10.0.402'; ref='10.0.13'; tfm='net10.0' },
    @{ variant='net11.0'; name='InstrumentArchive'; dotnet=$Net11Dotnet; runtime=$Net11Dotnet; sdk='11.0.100-rc.2.26504.105'; ref='11.0.0-rc.2.26504.105'; tfm='net11.0' },
    @{ variant='net10.0-sdk11'; name='InstrumentArchive'; dotnet=$Net11Dotnet; runtime=$Net10Dotnet; sdk='11.0.100-rc.2.26504.105'; ref='10.0.13'; tfm='net10.0' }
  )
  foreach ($entry in $matrix) {
    $sample = "samples/$($entry.variant)/$($entry.name)/$($entry.name).csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $entry.dotnet -RuntimeDotnet $entry.runtime
    $recordPath = Join-Path $root "samples/$($entry.variant)/$($entry.name)/verification-evidence.json"
    $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
    if (-not $record.output.Contains("runtime=.NET $($entry.ref);")) { throw "Wrong actual runtime for $sample" }
    $project = Join-Path $root $sample
    Push-Location (Split-Path $project)
    try {
      $resolved = & $entry.dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json
    }
    finally { Pop-Location }
    $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
    if ($pack.Count -ne 1 -or $pack[0].NuGetPackageVersion -cne $entry.ref -or $pack[0].TargetFramework -cne $entry.tfm) { throw "Wrong resolved reference pack for $sample" }
    $record.command += "; $($entry.dotnet) msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack"
    $sdkRoot = Split-Path $entry.dotnet
    $artifacts = @(
      (Join-Path $sdkRoot "sdk/$($entry.sdk)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
      (Join-Path $pack[0].Path "ref/$($entry.tfm)/System.Runtime.dll"),
      (Join-Path $pack[0].Path "ref/$($entry.tfm)/System.Runtime.Numerics.dll")
    )
    foreach ($artifact in $artifacts) {
      $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion
      $hash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant()
      $record.output += "`nBUILD ARTIFACT $artifact version=$version sha256=$hash"
    }
    $record.scope = 'Original booking configuration/data and instrument representation checks; actual pinned SDK/ref/runtime executions. SDK11/net10 runs on the supplied net10 runtime. Ref/compiler/runtime hashes identify artifacts; no package, native, provider, browser or GA proof is implied.'
    [IO.File]::WriteAllText($recordPath, ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
  $old = Get-Content -LiteralPath (Join-Path $root 'samples/net10.0/InstrumentArchive/verification-evidence.json') -Raw | ConvertFrom-Json
  $rebuilt = Get-Content -LiteralPath (Join-Path $root 'samples/net10.0-sdk11/InstrumentArchive/verification-evidence.json') -Raw | ConvertFrom-Json
  $oldConstants = [regex]::Match($old.output, '(?m)^EMBEDDED .+$').Value
  $newConstants = [regex]::Match($rebuilt.output, '(?m)^EMBEDDED .+$').Value
  if (-not $oldConstants -or -not $newConstants -or $oldConstants -ceq $newConstants) { throw 'Expected different embedded constants in the two actual net10 consumers.' }
  Write-Host "COMPARE net10 SDK10: $oldConstants"
  Write-Host "COMPARE net10 SDK11: $newConstants"
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
