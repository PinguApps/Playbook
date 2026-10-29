[CmdletBinding()]
param(
  [string]$Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$Gh = 'C:/Program Files/GitHub CLI/gh.exe',
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/web-migrations'),
  [switch]$RecordEvidence
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $output | Out-Null
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
# The source audit selected primary packages, not these complete application/tool
# closures. Qualify only these additional exact identities, with their reason;
# The map is batch-wide; each sample's independent lock/manifest graph is checked
# before compilation or evidence capture in both operating modes.
$additionalPackages = @{}
foreach ($version in '10.0.13','11.0.0-rc.2.26504.105') {
  foreach ($id in @('Microsoft.Data.Sqlite.Core','Microsoft.EntityFrameworkCore.Abstractions',
      'Microsoft.EntityFrameworkCore.Analyzers','Microsoft.EntityFrameworkCore.Relational',
      'Microsoft.EntityFrameworkCore.Sqlite.Core','Microsoft.EntityFrameworkCore.Sqlite',
      'Microsoft.EntityFrameworkCore','Microsoft.Extensions.DependencyModel',
      'Microsoft.Extensions.FileProviders.Embedded')) {
    $additionalPackages["$id/$version"] = 'Matching locked Identity/EF SQLite supporting closure; actual provider, selected assets and published account checks qualify it separately from the primary web audit.'
  }
}
foreach ($id in @('Grpc.AspNetCore.Server.ClientFactory','Grpc.AspNetCore.Server','Grpc.AspNetCore',
    'Grpc.Core.Api','Grpc.Net.Client','Grpc.Net.ClientFactory','Grpc.Net.Common','Grpc.Tools')) {
  $additionalPackages["$id/2.64.0"] = 'Pinned gRPC template/client/tool closure; real paired gRPC/JSON calls and Windows x64 protoc/plugin execution qualify these additional assets.'
}
foreach ($entry in @('Google.Api.CommonProtos/2.15.0','Google.Protobuf/3.27.0')) {
  $additionalPackages[$entry] = 'Exact JsonTranscoding dependency minimum selected by the lock; actual annotated protobuf generation, runtime descriptors and JSON wire/schema checks qualify it.'
}
foreach ($entry in @('Microsoft.Extensions.ApiDescription.Server/6.0.5','Microsoft.OpenApi/1.6.14',
    'Swashbuckle.AspNetCore.Swagger/6.6.2','Swashbuckle.AspNetCore.SwaggerGen/6.6.2',
    'Swashbuckle.AspNetCore.SwaggerUI/6.6.2','Swashbuckle.AspNetCore/6.6.2')) {
  $additionalPackages[$entry] = 'Batch-wide exact historical Swagger0.10.11/Swashbuckle6.6.2 identity, distinct from newer OpenAPI audit selections; consumer-checked by the actual net10 service/document/comments/failures in this batch.'
}
foreach ($entry in @('SQLitePCLRaw.bundle_e_sqlite3/2.1.13','SQLitePCLRaw.core/2.1.13',
    'SQLitePCLRaw.lib.e_sqlite3/2.1.13','SQLitePCLRaw.provider.e_sqlite3/2.1.13',
    'SQLite/3.53.4','SQLitePCLRaw.bundle_e_sqlite3/3.0.5','SQLitePCLRaw.config.e_sqlite3/3.0.5',
    'SQLitePCLRaw.core/3.0.5','SQLitePCLRaw.provider.e_sqlite3/3.0.5')) {
  $additionalPackages[$entry] = 'Exact locked native SQLite provider closure; loaded native-library hashes/version and real published store/session checks qualify Windows x64 separately from managed primary-package audit metadata.'
}
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $generator = Join-Path $output 'protoc-gen-openapiv2-v2.31.0-windows-x86_64.exe'
  $checksums = Join-Path $output 'grpc-gateway_2.31.0_checksums.txt'
  if (-not (Test-Path $generator) -or -not (Test-Path $checksums)) {
    & $Gh release download v2.31.0 --repo grpc-ecosystem/grpc-gateway --pattern 'protoc-gen-openapiv2-v2.31.0-windows-x86_64.exe' --pattern 'grpc-gateway_2.31.0_checksums.txt' --dir $output --clobber
  }
  if ((Get-FileHash $generator).Hash.ToLowerInvariant() -cne '214ecb46f982f3c8f1ac07ded35ea01ef87ca4a53e018ea9832f9e67b999ceac' -or
      (Get-FileHash $checksums).Hash.ToLowerInvariant() -cne 'c69d16ba8e39d420c42f4684a4521b1453bb13f70f25d99490318e80a498a242' -or
      (Get-Content $checksums -Raw) -notmatch '214ecb46f982f3c8f1ac07ded35ea01ef87ca4a53e018ea9832f9e67b999ceac\s+protoc-gen-openapiv2-v2.31.0-windows-x86_64.exe') { throw 'Generator release/checksum mismatch.' }
  $audit = Get-Content (Join-Path $root 'docs/planning/audits/web.assets.json') -Raw | ConvertFrom-Json
  foreach ($release in '10','11') {
    $dotnetHost = if ($release -eq '10') { $Dotnet } else { $Net11Dotnet }
    foreach ($name in 'BirdRoutes','TideSurvey','VolunteerAccess') {
      $sample = "samples/net$release.0/$name/$name.csproj"
      $directory = Join-Path $root (Split-Path $sample)
      Push-Location $directory
      try {
        & $dotnetHost restore --locked-mode
        $properties = (& $dotnetHost msbuild -getProperty:NuGetPackageRoot,TargetFramework | ConvertFrom-Json).Properties
        # Preflight the actual restored graph and ZIPs before any sample executes
        # or Record-Evidence can write a new compilation record.
        $assets = Get-Content 'obj/project.assets.json' -Raw | ConvertFrom-Json -AsHashtable
        $manifest = Get-Content 'verification.json' -Raw | ConvertFrom-Json -AsHashtable
        $resolved = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
        if ((ConvertTo-Json -InputObject $resolved -Compress) -cne (ConvertTo-Json -InputObject @($manifest.dependencies) -Compress)) {
          throw "Restored graph differs from the per-sample verification manifest: $sample"
        }
        $lockedPackages = @{}
        if ($resolved.Count -gt 0) {
          $lock = Get-Content 'packages.lock.json' -Raw | ConvertFrom-Json -AsHashtable
          $lockedPackages = $lock.dependencies[$properties.TargetFramework]
        }
        $baseline = $audit.baselines | Where-Object target -EQ "net$release.0"
        $observations = [Collections.Generic.List[string]]::new()
        $observations.Add('ACTUAL-RESOLVED-ASSETS ' + (ConvertTo-Json $assets.targets -Depth 50 -Compress))
        foreach ($library in $resolved) {
          $identity = $library.Split('/',2)
          $locked = $lockedPackages[$identity[0]]
          if ($null -eq $locked -or $locked.resolved -cne $identity[1] -or [string]::IsNullOrWhiteSpace($locked.contentHash)) {
            throw "Missing exact package-lock identity/contentHash: $library"
          }
          $package = Join-Path $properties.NuGetPackageRoot ($library.ToLowerInvariant() + '/' + $library.Replace('/','.').ToLowerInvariant() + '.nupkg')
          $hash = (Get-FileHash $package).Hash.ToLowerInvariant()
          $audited = @($baseline.packages | Where-Object { "$($_.id)/$($_.version)" -ieq $library })
          if ($audited.Count -eq 1) {
            if ($hash -cne $audited[0].sha256) { throw "Audited ZIP mismatch: $library" }
            $qualification = 'Exact ZIP matches the primary source audit.'
          } elseif ($audited.Count -gt 1) {
            throw "Ambiguous source-audit identity: $library"
          } elseif ($additionalPackages.ContainsKey($library)) {
            $qualification = $additionalPackages[$library]
          } else {
            throw "Unqualified additional package: $library. Review its exact identity and consumer before extending the qualification list."
          }
          [xml]$nuspec = Get-Content (Join-Path (Split-Path $package) ($identity[0].ToLowerInvariant() + '.nuspec')) -Raw
          # NuGet's signed-package contentHash is not the raw ZIP file hash.
          # Keep the independent lock value and observed ZIP SHA256 distinct.
          $observations.Add("PACKAGE $library | SHA256=$hash | nugetLockContentHash=$($locked.contentHash) | auditMatch=$($audited.Count -eq 1) | qualification=$qualification | repository=$($nuspec.package.metadata.repository | ConvertTo-Json -Compress)")
        }
        if ($name -eq 'TideSurvey' -and $release -eq '11') {
          & (Join-Path $directory 'Generate-Contract.ps1') -PackageRoot $properties.NuGetPackageRoot -Generator $generator
        }
        $kind = if ($name -eq 'VolunteerAccess') { 'compile' } else { 'compile-and-run' }
        if ($RecordEvidence) {
          $native = @()
          if ($name -eq 'TideSurvey') {
            $protoc = Join-Path $properties.NuGetPackageRoot 'grpc.tools/2.64.0/tools/windows_x64/protoc.exe'
            if ((& $protoc --version).Trim() -cne 'libprotoc 26.1') { throw 'Unexpected actual protoc.' }
            $native = @('protoc/26.1','grpc_csharp_plugin/2.64.0')
            if ($release -eq '11') { $native += 'protoc-gen-openapiv2/2.31.0' }
          }
          & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnetHost -RuntimeDotnet $dotnetHost -Kind $kind -NativeToolchains $native
        } else {
          & $dotnetHost build -c Release --no-restore
          if ($kind -eq 'compile-and-run') { & $dotnetHost "bin/Release/net$release.0/$name.dll" }
        }
        if ($name -eq 'VolunteerAccess') {
          $publish = Join-Path $output "volunteer$release"
          & $dotnetHost publish -c Release --no-restore -o $publish
          $observations.Add((& $dotnetHost (Join-Path $publish 'VolunteerAccess.dll') | Out-String).Trim())
          if ($release -eq '11') {
            $warning = (& $dotnetHost build -c Release --no-restore -p:IdentityUIFrameworkVersion=Bootstrap4 -p:TreatWarningsAsErrors=false | Out-String)
            if ($warning -notmatch 'warning : Bootstrap 4 support in Microsoft.AspNetCore.Identity.UI is obsolete') { throw 'Actual package warning not reproduced.' }
            $observations.Add($warning.Trim())
            # An EMPTY global property cannot be defaulted by package targets. Remove the
            # property in an isolated complete source copy to test actual default selection.
            $copy = Join-Path $output ('default-project-' + [guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $copy | Out-Null
            Get-ChildItem $directory | Where-Object Name -NotIn @('bin','obj') | Copy-Item -Destination $copy -Recurse
            $project = Join-Path $copy 'VolunteerAccess.csproj'
            [IO.File]::WriteAllText($project, [IO.File]::ReadAllText($project).Replace("    <IdentityUIFrameworkVersion>Bootstrap5</IdentityUIFrameworkVersion>`n", ''))
            if ([IO.File]::ReadAllText($project).Contains('IdentityUIFrameworkVersion')) { throw 'Property was not removed.' }
            $defaultPublish = Join-Path $output 'volunteer11-default'
            Push-Location $copy
            try {
              & $dotnetHost publish -c Release -p:RestoreLockedMode=true -o $defaultPublish
              $selection = & $dotnetHost msbuild -getProperty:IdentityUIFrameworkVersion
              if ($selection.Trim() -cne 'Bootstrap5') { throw 'Removed property did not select Bootstrap5.' }
              $observations.Add('DEFAULT-PROJECT-SHA256 ' + (Get-FileHash $project).Hash.ToLowerInvariant())
              $observations.Add('DEFAULT-SELECTION ' + $selection.Trim())
              $observations.Add((& $dotnetHost (Join-Path $defaultPublish 'VolunteerAccess.dll') | Out-String).Trim())
            } finally { Pop-Location }
            # Leave ordinary build output consistent with the authored Bootstrap5 project.
            & $dotnetHost build -c Release --no-restore
          }
        }
        $framework = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
        $sdk = if ($release -eq '10') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
        $installation = Split-Path $dotnetHost -Parent
        foreach ($assembly in 'Microsoft.AspNetCore.Server.Kestrel.Core.dll','Microsoft.AspNetCore.Http.Abstractions.dll') {
          $file = Join-Path $installation "packs/Microsoft.AspNetCore.App.Ref/$framework/ref/net$release.0/$assembly"
          $hash = (Get-FileHash $file).Hash.ToLowerInvariant()
          if ($hash -cne ($baseline.framework | Where-Object name -CEQ $assembly).sha256) { throw "Reference mismatch: $assembly" }
          $observations.Add("REFERENCE $assembly | $hash | auditMatch=true")
        }
        foreach ($file in @("sdk/$sdk/Roslyn/bincore/csc.dll", "shared/Microsoft.NETCore.App/$framework/coreclr.dll", "host/fxr/$framework/hostfxr.dll")) {
          $path = Join-Path $installation $file
          $observations.Add("ACTUAL $file | $([Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion) | $((Get-FileHash $path).Hash.ToLowerInvariant())")
        }
        if ($name -eq 'TideSurvey') {
          $protoc = Join-Path $properties.NuGetPackageRoot 'grpc.tools/2.64.0/tools/windows_x64/protoc.exe'
          $plugin = Join-Path $properties.NuGetPackageRoot 'grpc.tools/2.64.0/tools/windows_x64/grpc_csharp_plugin.exe'
          $observations.Add("PROTOC $(& $protoc --version) | $((Get-FileHash $protoc).Hash.ToLowerInvariant())")
          $observations.Add("CSHARP-PLUGIN $((Get-FileHash $plugin).Hash.ToLowerInvariant())")
          if ($release -eq '11') {
            $observations.Add("OPENAPIV2 v2.31.0 | $((Get-FileHash $generator).Hash.ToLowerInvariant()) | release checksum manifest $((Get-FileHash $checksums).Hash.ToLowerInvariant())")
            $observations.Add('GENERATED-DOCUMENT-SHA256 ' + (Get-FileHash 'obj/tides.swagger.json').Hash.ToLowerInvariant())
          }
        }
        if ($RecordEvidence) {
          $record = Get-Content 'verification-evidence.json' -Raw | ConvertFrom-Json -AsHashtable
          $record.kind = 'execution'
          $record.command = "eng/Test-WebMigrationBatch.ps1 -Dotnet '$Dotnet' -Net11Dotnet '$Net11Dotnet' -OutputDirectory '$output' -RecordEvidence"
          $record.scope = 'Actual source-audit ZIP/reference reconciliation, selected compile/runtime/native assets and SDK/compiler/host identities; TideSurvey includes actual pinned protoc/generator qualification; VolunteerAccess includes actual published page/assets/antiforgery/confirmation/local-return checks and current warning/default selection. Browser proof is separate, never inferred here.'
          $record.output = $observations -join "`n"
          [IO.File]::WriteAllText((Join-Path $directory 'artifacts.evidence.json'), (ConvertTo-Json $record -Depth 60).Replace("`r`n","`n") + "`n")
        }
      } finally { Pop-Location }
    }
  }
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
Write-Host 'PASS all paired web-migration consumers and PATH scopes. Browser observations remain separate.'
