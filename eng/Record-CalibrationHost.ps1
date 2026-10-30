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
    foreach ($selection in @(
      @{ File='numeric-and-parsing'; Ids=@('libraries-preview7','clamp-implementation','previous-tensor-api','current-tensor-api') },
      @{ File='hosting-configuration'; Ids=@('host-failure','host-10','host-11','host-extensions-11') }
    )) {
      $manifest = Get-Content -LiteralPath (Join-Path $root "docs/planning/audits/$($selection.File).sources.json") -Raw | ConvertFrom-Json
      foreach ($id in $selection.Ids) {
        $sources = @($manifest.sources | Where-Object id -CEQ $id)
        if ($sources.Count -ne 1) { throw "Expected one source: $id" }
        $source = $sources[0]
        $bytes = (Invoke-WebRequest -Uri $source.url).RawContentStream.ToArray()
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if ($hash -cne $source.sha256) { throw "Source changed: $id" }
        Write-Host "SOURCE $id $hash $($source.url)"
      }
    }
  }
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $sdk = if ($variant -eq 'net10.0') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    $ref = if ($variant -eq 'net10.0') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    foreach ($name in 'CalibrationImport','AquariumImport') {
      $sample = "samples/$variant/$name/$name.csproj"
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
      $recordPath = Join-Path $root "samples/$variant/$name/verification-evidence.json"
      $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json -AsHashtable
      if (-not $record.output.Contains("runtime=.NET $ref;")) { throw "Wrong runtime: $sample" }
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try {
        $resolved = & $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json
      }
      finally { Pop-Location }
      $pack = @($resolved.Items.ResolvedTargetingPack | Where-Object Identity -CEQ 'Microsoft.NETCore.App')
      if ($pack.Count -ne 1 -or $pack[0].NuGetPackageVersion -cne $ref -or $pack[0].TargetFramework -cne $variant) { throw "Wrong reference pack: $sample" }
      $artifacts = @(
        (Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),
        (Join-Path $pack[0].Path "ref/$variant/System.Runtime.dll"),
        (Join-Path (Split-Path $dotnet) "shared/Microsoft.NETCore.App/$ref/System.Private.CoreLib.dll")
      )
      $assets = Get-Content -LiteralPath (Join-Path (Split-Path $project) 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      $target = $assets.targets[$variant]
      $cache = @($assets.packageFolders.Keys)[0]
      foreach ($dependency in $target.Keys | Sort-Object -CaseSensitive) {
        foreach ($kind in 'compile','runtime') {
          foreach ($asset in @($target[$dependency][$kind].Keys | Where-Object { $_.EndsWith('.dll') })) {
            $path = Join-Path $cache ($assets.libraries[$dependency].path + '/' + $asset)
            $artifacts += $path
            $record.output += "`nSELECTED $kind $dependency/$asset"
          }
        }
      }
      foreach ($artifact in $artifacts | Sort-Object -Unique) {
        $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($artifact).ProductVersion
        $hash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant()
        $record.output += "`nARTIFACT $artifact version=$version sha256=$hash"
      }
      $record.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; SHA256 compiler, reference, CoreLib and selected compile/runtime package assets"
      $record.scope = if ($name -eq 'CalibrationImport') {
        'Actual float/decimal scalar and pairwise clamp calls at lengths1/3/33, reversed/equal/ordered bounds, late invalid bounds, capacity/length/NaN controls, untouched failed output and correct clamped values/tail. Windows x64 CoreCLR JIT; exact pinned compiler/reference/runtime/package assets. No newer package on net10, other engine/platform or GA proof.'
      } else {
        '35 lifecycle child processes plus canonical demo per release: RunAsync/Run, explicit StartAsync/StopAsync, timed StopAsync, WaitForShutdownAsync/WaitForShutdown, and owned application boundary; StopHost single/multiple/shutdown failures with original identities, distinct logged/thrown/process outcomes, cancellation, Ignore nonshutdown, startup failure, disposal and child reaping. Windows x64 CoreCLR JIT; no browser/custom-host/other-platform/GA or uncaught OS-exit-code guarantee.'
      }
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
