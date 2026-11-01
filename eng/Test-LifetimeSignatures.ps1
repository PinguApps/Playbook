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
    $sources = Get-Content (Join-Path $root 'docs/planning/audits/lifetime-signatures.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Source changed: $($source.id)" }
      Write-Host "SOURCE $($source.id) $hash"
    }
  }
  $audit = Get-Content (Join-Path $root 'docs/planning/audits/runtime-security.assets.json') -Raw | ConvertFrom-Json
  foreach ($variant in 'net10.0','net11.0') {
    $dotnet = if ($variant -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $baseline = @($audit.baselines | Where-Object target -CEQ $variant)[0]
    foreach ($name in 'TypeLedger','PermitManifest') {
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
        if ($output -notmatch "PASS ${name}:") { throw "Missing consumer result: $sample" }
        $runtimeLine = @($output -split '\r?\n' | Where-Object { $_.StartsWith('RUNTIME ',[StringComparison]::Ordinal) })[0]
        $runtime = $runtimeLine.Substring(8) | ConvertFrom-Json
        if ($runtime.framework -cne ".NET $($baseline.referencePack)") { throw "Runtime baseline drift: $sample" }
        if ((Get-FileHash $runtime.coreLibPath).Hash.ToLowerInvariant() -cne $runtime.sha256) { throw 'Loaded CoreLib hash changed.' }
        $pack = (& $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack | ConvertFrom-Json).Items.ResolvedTargetingPack |
          Where-Object Identity -CEQ 'Microsoft.NETCore.App'
        if ($pack.NuGetPackageVersion -cne $baseline.referencePack) { throw "Reference pack drift: $sample" }
        $artifacts = @()
        foreach ($assembly in 'System.Runtime.dll') {
          $path = Join-Path $pack.Path "ref/$variant/$assembly"
          $expected = @($baseline.framework | Where-Object name -CEQ $assembly)[0].sha256
          if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $expected) { throw "Audit asset mismatch: $path" }
          $artifacts += $path
        }
        if ($name -eq 'PermitManifest') {
          $package = @((@($audit.baselines | Where-Object target -CEQ $variant)[0]).packages | Where-Object id -CEQ 'system.security.cryptography.xml')[0]
          $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
          $packageRoot = @($assets.packageFolders.Keys)[0]
          $packageDirectory = Join-Path $packageRoot "system.security.cryptography.xml/$($package.version)"
          $archive = Join-Path $packageDirectory "system.security.cryptography.xml.$($package.version).nupkg"
          if ((Get-FileHash $archive).Hash.ToLowerInvariant() -cne $package.sha256) { throw 'XML package differs from audit.' }
          $relative = "lib/$variant/System.Security.Cryptography.Xml.dll"
          $path = Join-Path $packageDirectory $relative
          $expected = @($package.assets | Where-Object path -CEQ $relative)[0].surface.sha256
          if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $expected) { throw 'XML selected assembly differs from audit.' }
          if ((Get-FileHash "bin/Release/$variant/System.Security.Cryptography.Xml.dll").Hash.ToLowerInvariant() -cne $expected) { throw 'Executed XML assembly differs from audit.' }
          $artifacts += $archive,$path
        }
        $sdk = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
        $artifacts += Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"
        $artifacts += $runtime.coreLibPath
        $artifacts += Join-Path (Split-Path $runtime.coreLibPath) 'coreclr.dll'
        if ($name -eq 'PermitManifest') { $artifacts += 'C:/Windows/System32/bcrypt.dll','C:/Windows/System32/ncrypt.dll' }
        $lines = foreach ($path in $artifacts) {
          "ARTIFACT $path version=$([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) sha256=$((Get-FileHash $path).Hash.ToLowerInvariant())"
        }
        $lines | ForEach-Object { Write-Host $_ }
        if ($Record) {
          $recordPath = Join-Path (Split-Path $project) 'verification-evidence.json'
          $evidence = Get-Content $recordPath -Raw | ConvertFrom-Json -AsHashtable
          $evidence.command += "; $dotnet msbuild $project -t:ResolveFrameworkReferences -p:Configuration=Release -getItem:ResolvedTargetingPack; compare selected references/package/executed XML SHA256 with runtime-security.assets.json; SHA256 compiler/CoreLib/CoreCLR and Windows crypto binaries"
          $evidence.output += "`n" + ($lines -join "`n")
          $evidence.scope = 'Actual Windows x64 aligned native storage and collectible owner release without post-lifetime access, or signed XML default100/strict8 reference parsing and verification in separate processes, malformed/tamper/wrong-key/cached/invalid configuration checks. Exact audit-matching references/XML package and compiler/CoreLib/CoreCLR/Windows crypto binary hashes. No memory-reclamation timing, other-platform, authorization or final-GA proof.'
          [IO.File]::WriteAllText($recordPath, ($evidence | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
      }
      finally { Pop-Location }
    }
  }
  Write-Host 'PASS: paired lifetime/signature consumers and exact audit artifacts; PATH/persistent settings checked.'
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
