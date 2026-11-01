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
    $sources = Get-Content (Join-Path $root 'docs/planning/audits/extension-metadata.sources.json') -Raw | ConvertFrom-Json
    foreach ($source in $sources.sources) {
      $bytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
      if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne $source.sha256) { throw "Source mismatch: $($source.id)" }
      Write-Host "SOURCE $($source.id) $($source.sha256)"
    }
  }
  # Copy isolated probe projects, never overwrite a canonical sample's obj/bin/lock.
  $probes = @()
  $session = Join-Path $root ('artifacts/extension-metadata/probes/' + [guid]::NewGuid().ToString('N'))
  foreach ($case in @(
    @('old-contract','', 'CS0246', 'net11.0'),
    @('inaccessible-marker','INACCESSIBLE_MARKER','ASP0033','net11.0'),
    @('inaccessible-property','INACCESSIBLE_PROPERTY','ASP0035','net11.0'),
    @('missing-registration','MISSING_REGISTRATION','ASP0038','net11.0'),
    @('unsupported-net10','','NU1202','net10.0')
  )) {
    $directory = Join-Path $session $case[0]
    New-Item -ItemType Directory -Force $directory | Out-Null
    Copy-Item (Join-Path $root 'samples/Directory.Build.props'),(Join-Path $root 'samples/Directory.Build.targets'),
      (Join-Path $root 'samples/net11.0/ReefMaintenance/Directory.Packages.props'),
      (Join-Path $root 'samples/net11.0/ReefMaintenance/global.json') $directory
    $resolverTarget = if ($case[0] -eq 'old-contract') { 'net10.0' } else { 'net11.0' }
    Copy-Item (Join-Path $root "samples/$resolverTarget/ReefMaintenance/ReefResolver.cs") $directory
    Copy-Item (Join-Path $root 'samples/ExtensionMetadata/ValidationChecks.cs'),
      (Join-Path $root 'samples/ExtensionMetadata/Diagnostics.cs'),
      (Join-Path $root 'samples/net11.0/ReefMaintenance/MaintenanceLedger.cs'),
      (Join-Path $root 'samples/NetworkBuffers/RuntimeIdentity.cs') $directory
    if ($case[0] -ne 'missing-registration') { Copy-Item (Join-Path $root 'samples/net11.0/ReefMaintenance/ReefExample.cs') $directory }
    $xml = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>{0}</TargetFramework><LangVersion>15.0</LangVersion><InterceptorsNamespaces>Microsoft.AspNetCore.Http.Validation.Generated;Microsoft.Extensions.Validation.Generated</InterceptorsNamespaces><DefineConstants>{1}</DefineConstants></PropertyGroup><ItemGroup><PackageReference Include="Microsoft.Extensions.Validation" /><PackageReference Include="Microsoft.Extensions.DependencyInjection" /></ItemGroup></Project>' -f $case[3],$case[1]
    Set-Content (Join-Path $directory 'Probe.csproj') $xml
    Push-Location $directory
    try {
      $PSNativeCommandUseErrorActionPreference = $false
      $output = (& $Net11Dotnet build Probe.csproj -c Release 2>&1 | Out-String).Trim()
      $exit = $LASTEXITCODE
      $PSNativeCommandUseErrorActionPreference = $true
    } finally { Pop-Location }
    if ($exit -eq 0 -or $output -notmatch $case[2]) { throw "Expected $($case[2]) for $($case[0]):`n$output" }
    if ($case[0] -eq 'old-contract') {
      foreach ($name in 'ValidatableTypeInfo','ValidatablePropertyInfo','ValidatableParameterInfo') {
        if ($output -notmatch "'$name'.*could not be found") { throw "Missing expected removed base: $name" }
      }
    }
    $global:LASTEXITCODE = 0
    $probes += "COMMAND cwd=$directory $Net11Dotnet build Probe.csproj -c Release`nexit=$exit`n$output"
    Write-Host "EXPECTED $($case[0]): $($case[2])"
  }
  $audit = Get-Content (Join-Path $root 'docs/planning/audits/extensions.assets.json') -Raw | ConvertFrom-Json
  foreach ($target in 'net10.0','net11.0') {
    $dotnet = if ($target -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $baseline = @($audit.baselines | Where-Object target -CEQ $target)[0]
    foreach ($name in 'ArchiveStages','ReefMaintenance') {
      $sample = "samples/$target/$name/$name.csproj"
      $project = Join-Path $root $sample
      Push-Location (Split-Path $project)
      try {
        if ($Record) {
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
          $recorded = Get-Content verification-evidence.json -Raw | ConvertFrom-Json -AsHashtable
          $output = $recorded.output
        } else {
          & $dotnet build $project -c Release -p:RestoreLockedMode=true
          $output = (& $dotnet "bin/Release/$target/$name.dll" | Out-String).Trim()
          Write-Host $output
        }
        if ($output -notmatch "PASS ${name}:") { throw "Missing actual behaviour: $sample" }
        $runtime = @($output -split '\r?\n' | Where-Object { $_.StartsWith('RUNTIME ',[StringComparison]::Ordinal) })
        if ($runtime.Count -ne 1) { throw 'Missing runtime identity' }
        $runtime = $runtime[0].Substring(8) | ConvertFrom-Json
        if ($runtime.framework -cne ".NET $($baseline.referencePack)" -or (Get-FileHash $runtime.coreLibPath).Hash.ToLowerInvariant() -cne $runtime.sha256) { throw 'Runtime drift' }
        $sdk = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
        $paths = @((Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),$runtime.coreLibPath)
        $reference = @($baseline.framework | Where-Object name -CEQ 'System.Diagnostics.DiagnosticSource.dll')[0]
        $referencePath = Join-Path (Split-Path $dotnet) $reference.path
        if ((Get-FileHash $referencePath).Hash.ToLowerInvariant() -cne $reference.sha256) { throw 'Audited reference drift' }
        $paths += $referencePath
        $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
        $lines = @("SELECTED $($assets.targets[$target] | ConvertTo-Json -Depth 12 -Compress)")
        foreach ($library in $assets.libraries.Keys) {
          $entry = $assets.libraries[$library]
          $folder = @($assets.packageFolders.Keys | Where-Object { Test-Path (Join-Path $_ $entry.path) })[0]
          $archive = Join-Path $folder ($entry.path + '/' + ($library.Replace('/','.').ToLowerInvariant()) + '.nupkg')
          $paths += $archive
          if ($library.StartsWith('Microsoft.Extensions.Validation/',[StringComparison]::Ordinal)) {
            $reviewed = @($baseline.packages | Where-Object id -CEQ 'microsoft.extensions.validation')[0]
            if ((Get-FileHash $archive).Hash.ToLowerInvariant() -cne $reviewed.sha256) { throw 'Audited validation package drift' }
            foreach ($asset in $reviewed.assets) {
              $path = Join-Path $folder ($entry.path + '/' + $asset.path)
              if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $asset.surface.sha256) { throw 'Validation asset drift' }
              $paths += $path
              $loaded = Join-Path (Split-Path $project) "bin/Release/$target/Microsoft.Extensions.Validation.dll"
              if ((Get-FileHash $loaded).Hash -cne (Get-FileHash $path).Hash) { throw 'Executed validation differs from audited package' }
            }
            foreach ($tool in $reviewed.tooling | Where-Object path -Like '*ValidationsGenerator.dll') {
              $path = Join-Path $folder ($entry.path + '/' + $tool.path)
              if ((Get-FileHash $path).Hash.ToLowerInvariant() -cne $tool.sha256) { throw 'Generator drift' }
              $paths += $path
            }
          }
        }
        $lines += @($paths | ForEach-Object { "ARTIFACT $_ sha256=$((Get-FileHash $_).Hash.ToLowerInvariant())" })
        if ($Record) {
          $recorded.output += "`n" + ($lines -join "`n")
          if ($name -eq 'ReefMaintenance' -and $target -eq 'net11.0') { $recorded.output += "`n" + ($probes -join "`n") }
          $recorded.command += '; Test-ExtensionMetadata.ps1: audit reference/package/generator hashes and selected assets; isolated failing/fixed compilation probes'
          $recorded.scope = 'Windows x64 paired managed execution. Trace flags, parsing/formatting, sampling, default/custom generators; or non-web validation metadata, nesting, service/completion/failure/cancellation and current generated/localized integration. Actual RC2; no final-GA, endpoint, collector, AOT or other-platform claim.'
          [IO.File]::WriteAllText((Join-Path (Split-Path $project) 'verification-evidence.json'), ($recorded | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
        }
      } finally { Pop-Location }
    }
  }
  Write-Host 'PASS: paired metadata/trace consumers, expected contract/analyzer failures, audit identities; unchanged PATH/persistent settings.'
} finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $before[$scope]) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  foreach ($scope in $persistent.Keys) { if (([Environment]::GetEnvironmentVariables($scope) | ConvertTo-Json -Compress) -cne $persistent[$scope]) { throw "Unexpected persistent $scope environment change; shared settings preserved." } }
}
