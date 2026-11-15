[CmdletBinding()]
param(
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
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
    $sources = Get-Content (Join-Path $root 'docs/planning/audits/cbor-servicing.assets.json') -Raw | ConvertFrom-Json
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.version) $($source.url)" }
      Write-Host "SOURCE $($source.version) $($source.url) $hash"
    }
  }
  $cborAudit = Get-Content (Join-Path $root 'docs/planning/audits/cbor-servicing.assets.json') -Raw | ConvertFrom-Json
  $accepted = @{}
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in 'BinaryData','CalibrationGroups') {
      $sample = "samples/$variant/$name/$name.csproj"
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try {
        if ($Record) {
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
          $output = (Get-Content verification-evidence.json -Raw | ConvertFrom-Json).output
        }
        else {
          & $dotnet build $project -c Release -p:RestoreLockedMode=true
          $output = (& $dotnet "bin/Release/$variant/$name.dll" | Out-String).Trim()
          Write-Host $output
        }
        if ($name -eq 'CalibrationGroups') {
          $accepted[$variant] = (($output -split '\r?\n' | Where-Object { $_ -cmatch '^ACCEPT ' }) -join "`n")
          if (-not $accepted[$variant]) { throw 'Missing accepted CBOR values.' }
        }
        $artifacts = @()
        $package = @((@($cborAudit.baselines | Where-Object target -CEQ $variant)[0]).packages | Where-Object id -CEQ 'system.formats.cbor')[0]
        $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
        $packageRoot = @($assets.packageFolders.Keys)[0]
        $packageDirectory = Join-Path $packageRoot "system.formats.cbor/$($package.version)"
        $archive = Join-Path $packageDirectory "system.formats.cbor.$($package.version).nupkg"
        if ((Get-FileHash $archive).Hash.ToLowerInvariant() -cne $package.sha256) { throw 'CBOR package differs from audit.' }
        $relative = "lib/$variant/System.Formats.Cbor.dll"
        $target = $assets.targets[$variant]["System.Formats.Cbor/$($package.version)"]
        if (-not $target.compile.Contains($relative) -or -not $target.runtime.Contains($relative)) { throw 'Unexpected selected CBOR asset.' }
        $path = Join-Path $packageDirectory $relative
        $expected = @($package.assets | Where-Object path -CEQ $relative)[0].surface.sha256
        if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $expected) { throw 'CBOR selected assembly differs from audit.' }
        if ((Get-FileHash "bin/Release/$variant/System.Formats.Cbor.dll").Hash.ToLowerInvariant() -cne $expected) { throw 'Executed CBOR assembly differs from audit.' }
        $artifacts += $archive,$path
        $lines = foreach ($path in $artifacts) {
          "ARTIFACT $path version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) sha256=$((Get-FileHash $path).Hash.ToLowerInvariant())"
        }
        $lines | ForEach-Object { Write-Host $_ }
        if ($Record) {
          $recordPath = Join-Path (Split-Path $project) 'verification-evidence.json'
          $evidence = Get-Content $recordPath -Raw | ConvertFrom-Json -AsHashtable
          $evidence.command += "; compare package/selected/executed CBOR SHA256 with cbor-servicing.assets.json"
          $evidence.output += "`n" + ($lines -join "`n")
          $evidence.scope = 'Actual Windows x64 paired Base64/incremental CBOR or finite CBOR consumer. Output records real inputs, accepted results and separate failure types. Exact archive/selected/executed package asset hashes. No other-platform, performance, unlimited-depth or final stable-candidate claim.'
          [IO.File]::WriteAllText($recordPath, ($evidence | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
      }
      finally { Pop-Location }
    }
  }
  if ($accepted['net10.0'] -cne $accepted['net11.0']) { throw 'Paired CBOR accepted bytes/values differ.' }
  Write-Host 'PASS: paired binary/depth consumers, identical accepted CBOR bytes/values and exact audit artifacts.'
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
