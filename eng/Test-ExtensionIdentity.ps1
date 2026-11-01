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
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','MSBUILDUSESERVER') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  $env:MSBUILDUSESERVER = '0'
  if ($VerifySources) {
    $sources = Get-Content (Join-Path $root 'docs/planning/audits/extension-identity.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Source mismatch: $($source.id)" }
      Write-Host "SOURCE $($source.id) $($source.sha256)"
    }
  }
  $checks = @()
  function Invoke-Variant([string]$Relative, [string]$Name, [string[]]$Properties, [bool]$ExpectFailure = $false) {
    $project = Join-Path $root $Relative
    $builder = if ($Relative.StartsWith('samples/net10.0/',[StringComparison]::Ordinal)) { $Net10Dotnet } else { $Net11Dotnet }
    $directory = Join-Path $root "artifacts/extension-identity/checks/$Name"
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    $arguments = @('build',$project,'-c','Release','-p:RestoreLockedMode=false',
      "-p:MSBuildProjectExtensionsPath=$directory/obj/", "-p:IntermediateOutputPath=$directory/obj/Release/",
      "-p:OutputPath=$directory/bin/", "-p:NuGetLockFilePath=$directory/packages.lock.json") + $Properties
    Push-Location (Split-Path $project)
    try {
      $PSNativeCommandUseErrorActionPreference = $false
      $output = (& $builder @arguments 2>&1 | Out-String).Trim()
      $exit = $LASTEXITCODE
    } finally { Pop-Location }
    if ($ExpectFailure) {
      if ($exit -eq 0 -or $output -notmatch 'CS0234|CS0246') { throw "Expected missing-contract compile failure: $Name`n$output" }
    } elseif ($exit -ne 0) { throw "Variant failed: $Name`n$output" }
    $global:LASTEXITCODE = 0
    Write-Host $output
    return @{ output = "COMMAND $builder $($arguments -join ' ')`nexit=$exit`n$output"; directory = $directory }
  }
  # Separate obj/bin/locks preserve each canonical target and its saved dependency closure.
  $missing = Invoke-Variant 'samples/net10.0/CelestialSchedule/CelestialSchedule.csproj' 'missing-net10' @('-p:OmitReferences=true') $true
  $checks += $missing.output
  $missingImplementation = Invoke-Variant 'samples/ExtensionIdentity/Implementation/Implementation.csproj' 'missing-implementation' @('-p:OmitReferences=true') $true
  $checks += $missingImplementation.output
  $retained = Invoke-Variant 'samples/net11.0/CelestialSchedule/CelestialSchedule.csproj' 'retained-net11' @('-p:RetainReferences=true','-p:TreatWarningsAsErrors=false')
  $retainedAssets = Get-Content (Join-Path $retained.directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
  if (@($retainedAssets['logs'] | Where-Object code -CEQ 'NU1510').Count -ne 0 -or $retainedAssets.libraries.Count -ne 9) { throw 'Exact-range direct references must remain without NU1510 on this SDK' }
  $minimum = Invoke-Variant 'samples/net11.0/CelestialSchedule/CelestialSchedule.csproj' 'minimum-net11' @('-p:RetainReferences=true','-p:ReferenceVersion=10.0.13','-p:TreatWarningsAsErrors=false')
  $minimumAssets = Get-Content (Join-Path $minimum.directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
  if (@($minimumAssets['logs'] | Where-Object code -CEQ 'NU1510').Count -ne 9 -or $minimumAssets.libraries.Count -ne 9) { throw 'Expected nine NU1510 direct-reference warnings with retained packages' }
  foreach ($assets in @($minimumAssets)) {
    foreach ($library in $assets.targets['net11.0'].Keys) {
      $selected = $assets.targets['net11.0'][$library]
      if (@($selected.compile.Keys | Where-Object { $_ -notlike '*/_._' }).Count -gt 0 -or
          @($selected.runtime.Keys | Where-Object { $_ -notlike '*/_._' }).Count -gt 0) { throw "Redundant package supplied executable assets: $library" }
    }
    $checks += "RETAINED_SELECTION $($assets.targets['net11.0'] | ConvertTo-Json -Depth 15 -Compress)"
  }
  $checks += "EXACT_RANGE_SELECTION $($retainedAssets.targets['net11.0'] | ConvertTo-Json -Depth 15 -Compress)"
  $checks += $minimum.output
  $checks += $retained.output
  $retainedRun = (& $Net11Dotnet (Join-Path $retained.directory 'bin/CelestialSchedule.dll') | Out-String).Trim()
  if ($retainedRun -notmatch 'PASS PlanetariumContracts:') { throw 'Retained-reference consumer failed' }
  $checks += $retainedRun
  foreach ($name in 'Consumer','Implementation','Override') {
    $project = Join-Path $root "samples/ExtensionIdentity/$name/$name.csproj"
    Push-Location (Split-Path $project)
    try {
      $build = (& $Net11Dotnet build $project -c Release -p:RestoreLockedMode=true | Out-String).Trim()
      $checks += "COMMAND $Net11Dotnet build $project -c Release -p:RestoreLockedMode=true`n$build"
      foreach ($target in $(if ($name -eq 'Consumer') { @('net10.0','net11.0') } else { @('net11.0') })) {
        $runner = if ($target -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
        $path = "bin/Release/$target/$name.dll"
        $result = (& $runner $path | Out-String).Trim()
        if ($result -notmatch $(if ($name -eq 'Override') { 'OVERRIDE .*11.0.0-rc.3.26522.148' } else { "PASS $name " })) { throw "Supporting consumer failed: $name/$target" }
        $checks += "COMMAND $runner $(Join-Path (Split-Path $project) $path)`n$result"
        Write-Host $result
      }
      $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
      $checks += "SUPPORT $name libraries=$($assets.libraries.Keys -join ',') targets=$($assets.targets | ConvertTo-Json -Depth 20 -Compress)"
      if ($name -eq 'Override') {
        if (-not $assets.libraries.ContainsKey('Microsoft.Extensions.Logging.Abstractions/11.0.0-rc.3.26522.148')) { throw 'Newer override pruned unexpectedly' }
      }
      foreach ($library in $assets.libraries.Keys) {
        if ($assets.libraries[$library].type -eq 'package') {
          $entry = $assets.libraries[$library]
          $base = @($assets.packageFolders.Keys | Where-Object { Test-Path (Join-Path $_ $entry.path) })[0]
          $archive = Join-Path $base ($entry.path + '/' + ($library.Replace('/','.').ToLowerInvariant()) + '.nupkg')
          $checks += "SUPPORT_PACKAGE $library sha256=$((Get-FileHash $archive).Hash.ToLowerInvariant())"
        }
      }
    } finally { Pop-Location }
  }
  $audit = Get-Content (Join-Path $root 'docs/planning/audits/extensions.assets.json') -Raw | ConvertFrom-Json
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $baseline = @($audit.baselines | Where-Object target -CEQ $variant)[0]
    foreach ($name in 'CelestialSchedule','BorealSignals') {
      $sample = "samples/$variant/$name/$name.csproj"
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try {
        if ($Record) {
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
          $evidenceRecord = Get-Content verification-evidence.json -Raw | ConvertFrom-Json -AsHashtable
          $output = $evidenceRecord.output
        }
        else {
          & $dotnet build $project -c Release -p:RestoreLockedMode=true
          $output = (& $dotnet "bin/Release/$variant/$name.dll" | Out-String).Trim()
          Write-Host $output
        }
        if ($output -notmatch "PASS $(if ($name -eq 'CelestialSchedule') { 'PlanetariumContracts' } else { $name }):") { throw 'Missing behaviour proof' }
        $runtimeLine = @($output -split '\r?\n' | Where-Object { $_.StartsWith('RUNTIME ',[StringComparison]::Ordinal) })
        if ($runtimeLine.Count -ne 1) { throw 'Missing unique runtime identity' }
        $runtime = $runtimeLine[0].Substring(8) | ConvertFrom-Json
        if ($runtime.framework -cne ".NET $($baseline.referencePack)" -or
            (Get-FileHash $runtime.coreLibPath).Hash.ToLowerInvariant() -cne $runtime.sha256) { throw 'Runtime drift' }
        $assemblies = if ($name -eq 'CelestialSchedule') {
          @($output -split '\r?\n' | Where-Object { $_.StartsWith('ASSEMBLY ',[StringComparison]::Ordinal) } | ForEach-Object { $_.Substring(9) | ConvertFrom-Json })
        } else { @() }
        if ($name -eq 'CelestialSchedule' -and $assemblies.Count -ne 9) { throw 'Expected nine moved assemblies' }
        $pack = (& $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack |
          Where-Object Identity -CEQ 'Microsoft.NETCore.App'
        if ($pack.NuGetPackageVersion -cne $baseline.referencePack) { throw 'Reference pack drift' }
        $artifacts = @()
        $refNames = if ($name -eq 'CelestialSchedule') { @($assemblies.name | ForEach-Object { "${_}.dll" }) } else { @('System.Diagnostics.Tracing.dll') }
        foreach ($refName in $refNames) {
          $asset = @($baseline.framework | Where-Object name -CEQ $refName)[0]
          if (-not $asset) { throw "Missing audited reference: $refName" }
          $path = Join-Path (Split-Path $dotnet) $asset.path
          if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $asset.sha256) { throw "Audit mismatch: $path" }
          $artifacts += $path
        }
        $sdk = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
        $artifacts += Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
        $artifacts += $runtime.coreLibPath
        $lines = @($artifacts | ForEach-Object { "ARTIFACT $_ version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($_).ProductVersion) sha256=$((Get-FileHash $_).Hash.ToLowerInvariant())" })
        if ($name -eq 'CelestialSchedule') {
          $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
          foreach ($library in $assets.libraries.Keys) {
            $entry = $assets.libraries[$library]
            $base = @($assets.packageFolders.Keys | Where-Object { Test-Path (Join-Path $_ $entry.path) })[0]
            $archive = Join-Path $base ($entry.path + '/' + ($library.Replace('/','.').ToLowerInvariant()) + '.nupkg')
            $lines += "PACKAGE $library archive=$archive sha256=$((Get-FileHash $archive).Hash.ToLowerInvariant())"
          }
          $target = $assets.targets[$variant]
          foreach ($library in $target.Keys) { $lines += "SELECTED $library $($target[$library] | ConvertTo-Json -Compress -Depth 12)" }
          foreach ($assembly in $assemblies) {
            if ((Get-FileHash $assembly.path).Hash.ToLowerInvariant() -cne $assembly.sha256) { throw 'Executed assembly changed' }
            $fromFramework = $assembly.path.StartsWith((Join-Path (Split-Path $dotnet) 'shared'),[StringComparison]::OrdinalIgnoreCase)
            if ($fromFramework -ne ($variant -eq 'net11.0')) { throw "Unexpected load source: $($assembly.path)" }
            if ($assembly.version -notlike "$($baseline.referencePack)*") { throw 'Executed Extensions baseline drift' }
            if ($variant -eq 'net10.0') {
              $library = "$($assembly.name)/10.0.13"
              $entry = $assets.libraries[$library]
              $folder = @($assets.packageFolders.Keys | Where-Object { Test-Path (Join-Path $_ $entry.path) })[0]
              foreach ($kind in 'compile','runtime') {
                $selected = @($assets.targets[$variant][$library][$kind].Keys | Where-Object { $_ -like '*.dll' })
                if ($selected.Count -ne 1) { throw 'Expected one actual package asset' }
                $path = Join-Path $folder ($entry.path + '/' + $selected[0])
                if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $assembly.sha256) { throw 'Executed assembly differs from selected package asset' }
                $lines += "MATCHED $kind $path sha256=$($assembly.sha256)"
              }
            }
          }
          $publish = Join-Path $root "artifacts/extension-identity/publish/$variant"
          $publishOutput = (& $dotnet publish $project -c Release --no-restore --self-contained false -o $publish | Out-String).Trim()
          $publishedOutput = (& $dotnet (Join-Path $publish "$name.dll") | Out-String).Trim()
          if ($publishedOutput -notmatch 'PASS PlanetariumContracts:') { throw 'Published consumer failed' }
          $deps = Get-Content (Join-Path $publish "$name.deps.json") -Raw | ConvertFrom-Json -AsHashtable
          $loose = @(Get-ChildItem $publish 'Microsoft.Extensions.*.dll' | Select-Object -ExpandProperty Name)
          if (($variant -eq 'net10.0' -and $loose.Count -ne 9) -or ($variant -eq 'net11.0' -and $loose.Count -ne 0)) { throw 'Unexpected framework-dependent publish layout' }
          if (($variant -eq 'net10.0' -and $deps.libraries.Count -ne 10) -or ($variant -eq 'net11.0' -and $deps.libraries.Count -ne 1)) { throw 'Unexpected publish deps' }
          $lines += @("COMMAND $dotnet publish $project -c Release --no-restore --self-contained false -o $publish",$publishOutput,"COMMAND $dotnet $publish/$name.dll",$publishedOutput,
            "PUBLISH loose=$($loose -join ',') deps=$($deps.libraries.Keys -join ',') runtimeconfig=$(Get-Content (Join-Path $publish "$name.runtimeconfig.json") -Raw)")
        }
        $lines | ForEach-Object { Write-Host $_ }
        if ($Record) {
          $evidenceRecord.output += "`n" + ($lines -join "`n")
          if ($name -eq 'CelestialSchedule') { $evidenceRecord.output += "`n" + ($checks -join "`n") }
          $evidenceRecord.command += '; Test-ExtensionIdentity.ps1: resolve/audit reference hashes; SHA256 compiler/CoreLib/package archives; actual publish/run/deps/load inspection where applicable'
          $evidenceRecord.scope = 'Windows x64 actual paired managed consumers. Nine moved Extensions contracts with selected assets, framework-dependent publish/run/deps/load identities; or managed EventListener identity, payload, traits/settings, disposal and observed initialization/write errors. RC2, not GA; no ETW/EventPipe, native/AOT, self-contained or other-platform deployment claim.'
          [IO.File]::WriteAllText((Join-Path (Split-Path $project) 'verification-evidence.json'), ($evidenceRecord | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
      } finally { Pop-Location }
    }
  }
  Write-Host 'PASS: paired reference and EventSource consumers, audit assets, publish/load identities; PATH/persistent settings checked.'
} finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
