[CmdletBinding()]
param(
  [string]$Net10Root = 'C:/Program Files/dotnet',
  [string]$Net11Root = 'C:/Users/pingu/.dotnet-playbook-11',
  [string]$NetFrameworkRoot = 'C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/ef.assets.json'),
  [string]$CompareWith,
  [string]$SourceManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/ef.sources.json'),
  [string]$DispositionManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/ef.dispositions.json')
)

# Metadata/text only. No inspected code, restore, SDK setup or PATH changes.
$ErrorActionPreference = 'Stop'
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
if ($CompareWith -and $output -eq $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($CompareWith)) { throw 'Comparison output must not overwrite the baseline.' }
function Get-Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Get-Ordinal($Values) {
  [string[]]$items = @($Values)
  [Array]::Sort($items, [StringComparer]::Ordinal)
  $items
}
function Get-Attributes($Item) {
  (Get-Ordinal @($Item.GetCustomAttributesData() | ForEach-Object {
    $arguments = @($_.ConstructorArguments | ForEach-Object { Get-AttributeValue $_ })
    $named = @(Get-Ordinal @($_.NamedArguments | ForEach-Object { "$($_.MemberName)=$(Get-AttributeValue $_.TypedValue)" }))
    "[$($_.AttributeType.FullName)($((@($arguments) + @($named)) -join ','))]"
  })) -join ' '
}
function Get-AttributeValue($Argument) {
  $value = if ($null -eq $Argument.Value) { 'null' }
  elseif ($Argument.Value -is [Collections.Generic.IList[Reflection.CustomAttributeTypedArgument]]) {
    '{' + ((@($Argument.Value | ForEach-Object { Get-AttributeValue $_ })) -join ',') + '}'
  } elseif ($Argument.Value -is [Type]) { $Argument.Value.ToString() }
  else { ConvertTo-Json -InputObject $Argument.Value -Compress }
  "$($Argument.ArgumentType.ToString()):$value"
}
function Get-Generic($Parameters) {
  (@($Parameters | Where-Object IsGenericParameter | ForEach-Object {
    "$($_.ToString()) : $($_.GenericParameterAttributes) [$((Get-Ordinal @($_.GetGenericParameterConstraints() | ForEach-Object ToString)) -join ',')] $(Get-Attributes $_)"
  })) -join ';'
}
function Get-Parameter($Parameter) {
  if ($null -eq $Parameter) { throw "Null parameter in $($method.DeclaringType.FullName) $($method.Name) inspecting $Path" }
  $default = if ($Parameter.HasDefaultValue) {
    if ($null -eq $Parameter.RawDefaultValue) { 'null' } else { [string]$Parameter.RawDefaultValue }
  } else { '<none>' }
  "$($Parameter.ParameterType.ToString()) $($Parameter.Name) [$($Parameter.Attributes)] default=$default modreq=$((@($Parameter.GetRequiredCustomModifiers() | ForEach-Object ToString)) -join ',') modopt=$((@($Parameter.GetOptionalCustomModifiers() | ForEach-Object ToString)) -join ',') $(Get-Attributes $Parameter)"
}
function Get-Surface([string]$Path, [string[]]$References) {
  # Prefer the assembly being inspected over a same-named framework reference.
  $referencesWithoutSelf = @($References | Where-Object { [IO.Path]::GetFileName($_) -ne [IO.Path]::GetFileName($Path) })
  $resolver = [Reflection.PathAssemblyResolver]::new([string[]](@($Path) + $referencesWithoutSelf))
  $context = [Reflection.MetadataLoadContext]::new($resolver, 'System.Runtime')
  try {
    $assembly = $context.LoadFromAssemblyPath($Path)
    $lines = [Collections.Generic.List[string]]::new()
    # Read forwarders without resolving optional assemblies (for example Schema).
    $stream = [IO.File]::OpenRead($Path)
    $pe = [Reflection.PortableExecutable.PEReader]::new($stream)
    try {
      $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
      foreach ($handle in $metadata.ExportedTypes) {
        $export = $metadata.GetExportedType($handle)
        $exportName = "$($metadata.GetString($export.Namespace)).$($metadata.GetString($export.Name))"
        $destination = if ($export.Implementation.Kind -eq [Reflection.Metadata.HandleKind]::AssemblyReference) {
          $reference = $metadata.GetAssemblyReference([Reflection.Metadata.AssemblyReferenceHandle]::op_Explicit($export.Implementation))
          $metadata.GetString($reference.Name)
        } else { $export.Implementation.Kind.ToString() }
        $lines.Add("forward $exportName -> $destination [$($export.Attributes)]")
      }
    } finally { $pe.Dispose(); $stream.Dispose() }
    foreach ($type in $assembly.GetTypes() | Where-Object IsVisible) {
      $name = $type.FullName
      $lines.Add("type $name [$($type.Attributes)] base=$(if ($type.BaseType) { $type.BaseType.ToString() }) interfaces=$((Get-Ordinal @($type.GetInterfaces() | ForEach-Object ToString)) -join ',') generic=$(Get-Generic $type.GetGenericArguments()) $(Get-Attributes $type)")
      $flags = [Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
      foreach ($method in @($type.GetConstructors($flags)) + @($type.GetMethods($flags))) {
        if (-not ($method.IsPublic -or $method.IsFamily -or $method.IsFamilyOrAssembly)) { continue }
        $return = if ($method -is [Reflection.MethodInfo]) { Get-Parameter $method.ReturnParameter } else { 'void' }
        $generic = if ($method.IsGenericMethod) { Get-Generic $method.GetGenericArguments() } else { '' }
        $lines.Add("method ${name}::$($method.Name) [$($method.Attributes)] ($((@($method.GetParameters() | ForEach-Object { Get-Parameter $_ })) -join ', ')) -> $return generic=$generic $(Get-Attributes $method)")
      }
      foreach ($property in $type.GetProperties($flags)) {
        if (-not @($property.GetAccessors($true) | Where-Object { $_.IsPublic -or $_.IsFamily -or $_.IsFamilyOrAssembly }).Count) { continue }
        $lines.Add("property ${name}::$($property.Name) $($property.PropertyType.ToString()) ($((@($property.GetIndexParameters() | ForEach-Object { Get-Parameter $_ })) -join ', ')) $(Get-Attributes $property)")
      }
      foreach ($field in $type.GetFields($flags)) {
        if (-not ($field.IsPublic -or $field.IsFamily -or $field.IsFamilyOrAssembly)) { continue }
        $value = if ($field.IsLiteral) { [string]$field.GetRawConstantValue() } else { '<none>' }
        $lines.Add("field ${name}::$($field.Name) $($field.FieldType.ToString()) [$($field.Attributes)] value=$value $(Get-Attributes $field)")
      }
      foreach ($event in $type.GetEvents($flags)) {
        if (-not @($event.GetAddMethod($true), $event.GetRemoveMethod($true) | Where-Object { $null -ne $_ -and ($_.IsPublic -or $_.IsFamily -or $_.IsFamilyOrAssembly) }).Count) { continue }
        $lines.Add("event ${name}::$($event.Name) $($event.EventHandlerType.ToString()) $(Get-Attributes $event)")
      }
    }
    [ordered]@{ name = [IO.Path]::GetFileName($Path); sha256 = Get-Hash $Path; declarations = @(Get-Ordinal $lines) }
  } finally { $context.Dispose() }
}
function Get-Diff($Old, $New) {
  $Old = @($Old | Where-Object { $null -ne $_ })
  $New = @($New | Where-Object { $null -ne $_ })
  $oldSet = [Collections.Generic.HashSet[string]]::new([string[]]$Old, [StringComparer]::Ordinal)
  $newSet = [Collections.Generic.HashSet[string]]::new([string[]]$New, [StringComparer]::Ordinal)
  [ordered]@{
    added = @(Get-Ordinal @($New | Where-Object { -not $oldSet.Contains($_) }))
    removed = @(Get-Ordinal @($Old | Where-Object { -not $newSet.Contains($_) }))
  }
}

function Get-TextAsset($File, $Root) {
  $entry=[ordered]@{ path=[IO.Path]::GetRelativePath($Root,$File).Replace('\','/'); sha256=Get-Hash $File }
  if ([IO.Path]::GetExtension($File) -in @('.cs','.csproj','.fs','.fsproj','.cshtml','.razor','.json','.js','.css','.html','.props','.targets','.proto')) { $entry.lines=@([IO.File]::ReadAllText($File) -split '\r?\n') }
  $entry
}

function Get-Package([string]$Id, [string]$Version, [string]$Target) {
  $url = "https://api.nuget.org/v3-flatcontainer/$Id/$Version/$Id.$Version.nupkg"
  $zip = Join-Path $temporary "$Id.$Version.nupkg"
  if (-not (Test-Path -LiteralPath $zip)) {
    try { Invoke-WebRequest $url -OutFile $zip } catch { throw "Download failed: $url — $_" }
  }
  $directory = "$zip.$Target.contents"
  [IO.Compression.ZipFile]::ExtractToDirectory($zip, $directory)
  [xml]$nuspec = Get-Content (Get-ChildItem $directory -Filter '*.nuspec').FullName -Raw
  $groups = @($nuspec.package.metadata.dependencies.group | Where-Object { $null -ne $_ })
  $preferred = @($Target, ".NETCoreApp$($Target.Substring(3))", 'netstandard2.0', '.NETStandard2.0')
  $group = @()
  foreach ($tfm in $preferred) {
    $group = @($groups | Where-Object targetFramework -EQ $tfm)
    if ($group.Count) { break }
  }
  if (-not $group.Count) {
    $major=[int]$Target.Substring(3).Split('.')[0]
    $group=@($groups | Where-Object {
      $_.targetFramework -match '^(?:net|\.NETCoreApp)(\d+)\.\d+$' -and [int]$Matches[1] -le $major
    } | Sort-Object { [version]($_.targetFramework -replace '^(?:net|\.NETCoreApp)','') } -Descending | Select-Object -First 1)
  }
  if (-not $group.Count) { $group = @($groups | Where-Object targetFramework -Match '^(netstandard|\.NETStandard)2\.[01]$' | Sort-Object targetFramework -Descending | Select-Object -First 1) }
  if (-not $group.Count -and $groups.Count) { throw "No supported metadata dependency group: $Id/$Version for $Target" }
  $dependencies = @($group.dependency | Where-Object { $null -ne $_ })
  [ordered]@{
    id=$Id; version=$Version; url=$url; sha256=Get-Hash $zip
    repository=$nuspec.package.metadata.repository.url; commit=$nuspec.package.metadata.repository.commit
    selectedDependencyTarget=$group.targetFramework
    dependencies=@($dependencies | ForEach-Object { "$($_.id)/$($_.version)" })
    directory=$directory; assets=@(); files=@()
  }
}

$temporary = Join-Path ([IO.Path]::GetTempPath()) "playbook-ef-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $temporary | Out-Null
try {
  Add-Type -Path (Join-Path $Net10Root 'sdk/10.0.402/System.Reflection.MetadataLoadContext.dll')
  $baselines = @(
    @{ root=$Net10Root; sdk='10.0.402'; version='10.0.13'; target='net10.0' },
    @{ root=$Net11Root; sdk='11.0.100-rc.2.26504.105'; version='11.0.0-rc.2.26504.105'; target='net11.0' }
  )
  $results = @()
  foreach ($baseline in $baselines) {
    $core = Join-Path $baseline.root "packs/Microsoft.NETCore.App.Ref/$($baseline.version)/ref/$($baseline.target)"
    $asp = Join-Path $baseline.root "packs/Microsoft.AspNetCore.App.Ref/$($baseline.version)/ref/$($baseline.target)"
    $references = @(Get-ChildItem -LiteralPath $core,$asp -Filter '*.dll' | ForEach-Object FullName)
    if (-not $references.Count) { throw 'Missing pinned reference packs.' }
    $ids = @('microsoft.entityframeworkcore','microsoft.entityframeworkcore.abstractions',
      'microsoft.entityframeworkcore.relational','microsoft.entityframeworkcore.sqlserver',
      'microsoft.entityframeworkcore.sqlserver.abstractions','microsoft.entityframeworkcore.sqlserver.hierarchyid',
      'microsoft.entityframeworkcore.sqlserver.nettopologysuite','microsoft.entityframeworkcore.sqlite',
      'microsoft.entityframeworkcore.sqlite.core','microsoft.entityframeworkcore.sqlite.nettopologysuite',
      'microsoft.entityframeworkcore.cosmos','microsoft.entityframeworkcore.design',
      'microsoft.entityframeworkcore.tools','microsoft.entityframeworkcore.tasks',
      'microsoft.entityframeworkcore.analyzers','microsoft.entityframeworkcore.proxies',
      'microsoft.entityframeworkcore.inmemory','microsoft.data.sqlite','microsoft.data.sqlite.core','dotnet-ef')
    $packages = @()
    foreach ($id in $ids) { $packages += Get-Package $id $baseline.version $baseline.target }
    # Resolve only metadata dependencies, using declared minimum versions. This
    # is not an application restore or proof that this is a valid consumer graph.
    for ($i=0; $i -lt $packages.Count; $i++) {
      foreach ($dependency in $packages[$i].dependencies) {
        $id,$range = $dependency -split '/',2
        $id=$id.ToLowerInvariant()
        if ($id -in $packages.id -or "$id.dll" -in @($references | ForEach-Object { [IO.Path]::GetFileName($_).ToLowerInvariant() })) { continue }
        $version=[regex]::Match($range,'\d+\.\d+(?:\.\d+){0,2}(?:-[0-9A-Za-z.-]+)?').Value
        if (-not $version) { throw "Cannot resolve metadata dependency: $dependency" }
        $packages += Get-Package $id $version $baseline.target
      }
    }
    $resolverPaths = @($references)
    foreach ($package in $packages) {
      $libs = @(Get-ChildItem (Join-Path $package.directory 'lib') -Directory -ErrorAction SilentlyContinue)
      $lib = @($libs | Where-Object Name -EQ $baseline.target)
      if (-not $lib.Count) {
        $major=[int]$baseline.target.Substring(3).Split('.')[0]
        $lib=@($libs | Where-Object { $_.Name -match '^net(\d+)\.\d+$' -and [int]$Matches[1] -le $major } | Sort-Object { [version]$_.Name.Substring(3) } -Descending | Select-Object -First 1)
      }
      if (-not $lib.Count) { $lib=@($libs | Where-Object Name -Match '^netstandard2\.[01]$' | Sort-Object Name -Descending | Select-Object -First 1) }
      foreach ($file in @($lib | Get-ChildItem -Filter '*.dll')) {
        if ($file.Name -notin @($resolverPaths | ForEach-Object { [IO.Path]::GetFileName($_) })) { $resolverPaths += $file.FullName }
      }
      # Inspect each SQLite 11 net10/net11 asset that actually exists, rather
      # than inferring its lower-target route from EF's net11-only assemblies.
      $reviewed = if ($package.id -eq 'microsoft.data.sqlite.core' -and $baseline.target -eq 'net11.0') { @($libs | Where-Object Name -Match '^net1[01]\.0$') } else { $lib }
      if ($package.id -in $ids) {
        foreach ($file in @($reviewed | Get-ChildItem -Filter '*.dll')) {
          $package.assets += [ordered]@{ path=[IO.Path]::GetRelativePath($package.directory,$file.FullName).Replace('\','/'); fullPath=$file.FullName }
        }
        if ($package.id -eq 'microsoft.entityframeworkcore.tasks') {
          foreach ($file in Get-ChildItem (Join-Path $package.directory 'tasks') -Recurse -Filter 'Microsoft.EntityFrameworkCore.Tasks.dll' | Sort-Object FullName) {
            $package.assets += [ordered]@{path=[IO.Path]::GetRelativePath($package.directory,$file.FullName).Replace('\','/');fullPath=$file.FullName}
          }
        }
      }
    }
    foreach ($file in Get-ChildItem (Join-Path $baseline.root "sdk/$($baseline.sdk)") -Filter '*.dll') {
      if ($file.Name -match '^Microsoft\.Build\.') { $resolverPaths=@($resolverPaths | Where-Object { [IO.Path]::GetFileName($_) -ne $file.Name }) }
      if ($file.Name -notin @($resolverPaths | ForEach-Object { [IO.Path]::GetFileName($_) })) { $resolverPaths += $file.FullName }
    }
    foreach ($package in $packages) {
      if ($package.id -in $ids -and $package.id -notin @('dotnet-ef','microsoft.entityframeworkcore.tools','microsoft.entityframeworkcore.analyzers','microsoft.entityframeworkcore.sqlite','microsoft.data.sqlite') -and -not $package.assets.Count) { throw "Missing selected API asset: $($package.id)" }
      foreach ($asset in $package.assets) {
        $assetReferences=@($resolverPaths)
        if ($asset.path -match '^tasks/(net472|netframework)/') {
          # Full-framework MSBuild exposes additional interfaces. Resolve that
          # route from its own task dependencies, not the net10 MSBuild facade.
          $assetReferences=@(Get-ChildItem -LiteralPath $NetFrameworkRoot,(Join-Path $NetFrameworkRoot 'Facades') -Filter '*.dll' | ForEach-Object FullName)
          if (-not $assetReferences.Count) { throw 'Missing .NET Framework 4.7.2 reference assemblies for the MSBuild task route.' }
          $frameworkReferences=@($packages | ForEach-Object { Get-ChildItem (Join-Path $_.directory 'lib/net472') -Filter '*.dll' -ErrorAction SilentlyContinue })
          $frameworkReferences+=@(Get-ChildItem (Split-Path $asset.fullPath -Parent) -Filter '*.dll')
          foreach ($file in $frameworkReferences | Where-Object Name -NotMatch '^Microsoft\.Build\.') {
            $assetReferences=@($assetReferences | Where-Object { [IO.Path]::GetFileName($_) -ne $file.Name })+@($file.FullName)
          }
        }
        $asset.surface=Get-Surface $asset.fullPath $assetReferences
        $asset.Remove('fullPath')
      }
      if ($package.id -in $ids) {
        foreach ($file in @(Get-ChildItem $package.directory -Recurse -File | Where-Object {
          $_.Extension -in @('.props','.targets','.ps1','.psm1','.psd1') -or
          ($_.Extension -in @('.dll','.json','.exe') -and $_.FullName -match '[\\/](tasks|tools|analyzers)[\\/]')
        } | Sort-Object FullName)) {
          $entry=Get-TextAsset $file.FullName $package.directory
          if ($file.Extension -in @('.ps1','.psm1','.psd1')) { $entry.lines=@([IO.File]::ReadAllText($file.FullName) -split '\r?\n') }
          $package.files += $entry
        }
        $package.otherAssemblies=@(Get-ChildItem $package.directory -Recurse -Filter '*.dll' | Where-Object {
          [IO.Path]::GetRelativePath($package.directory,$_.FullName).Replace('\','/') -notin $package.assets.path -and
          [IO.Path]::GetRelativePath($package.directory,$_.FullName).Replace('\','/') -notin $package.files.path
        } | Sort-Object FullName | ForEach-Object { [ordered]@{path=[IO.Path]::GetRelativePath($package.directory,$_.FullName).Replace('\','/');sha256=Get-Hash $_.FullName} })
      }
      $package.role=if ($package.id -in $ids) { 'reviewed' } else { 'metadata resolution only' }
    }
    foreach ($package in $packages) { $package.Remove('directory') }
    Write-Host "Read $($baseline.target): $($packages.Count) packages, $(@($packages.assets).Count) selected assemblies."
    $sdkMetadataReferences=@($resolverPaths | Where-Object { [IO.Path]::GetFileName($_) -match '^Microsoft\.Build\.' } | Sort-Object | ForEach-Object { [ordered]@{name=[IO.Path]::GetFileName($_);sha256=Get-Hash $_} })
    $frameworkMetadataReferences=@(Get-ChildItem -LiteralPath $NetFrameworkRoot,(Join-Path $NetFrameworkRoot 'Facades') -Filter '*.dll' | Sort-Object FullName | ForEach-Object { [ordered]@{path=[IO.Path]::GetRelativePath($NetFrameworkRoot,$_.FullName).Replace('\','/');sha256=Get-Hash $_.FullName} })
    $results += [ordered]@{ sdk=$baseline.sdk; referencePack=$baseline.version; target=$baseline.target; metadataReaderSha256=Get-Hash (Join-Path $Net10Root 'sdk/10.0.402/System.Reflection.MetadataLoadContext.dll'); sdkMetadataReferences=$sdkMetadataReferences; frameworkMetadataReferences=$frameworkMetadataReferences; packages=$packages }
  }
  $diffs=@()
  foreach ($id in $ids) {
    $old=@($results[0].packages | Where-Object id -EQ $id); $new=@($results[1].packages | Where-Object id -EQ $id)
    foreach ($asset in $new.assets) {
      $before=@($old.assets)
      if ($id -eq 'microsoft.entityframeworkcore.tasks') {
        $framework=$asset.path -match '/netframework/'
        $before=@($old.assets | Where-Object { ($_.path -match '/net472/') -eq $framework })
      }
      if ($before.Count -ne 1 -and $id -ne 'microsoft.entityframeworkcore.tools') { throw "Missing/ambiguous previous asset for $id/$($asset.path)" }
      $diffs += [ordered]@{ asset="$id/$($before.path)->$($asset.path)"; diff=Get-Diff $before.surface.declarations $asset.surface.declarations }
    }
  }
  $textDiffs=@()
  $pair=@($results | ForEach-Object { ,@($_.packages | Where-Object role -EQ 'reviewed' | ForEach-Object { $id=$_.id; $_.files | ForEach-Object { [ordered]@{ path="$id/$($_.path)"; lines=$_.lines } } }) })
  $old,$new=$pair
  foreach ($path in Get-Ordinal @(@($old.path)+@($new.path) | Select-Object -Unique)) {
    $before=@($old | Where-Object path -CEQ $path); $after=@($new | Where-Object path -CEQ $path)
    if (@($before.lines).Count -eq 0 -and @($after.lines).Count -eq 0) { continue }
    $textDiffs += [ordered]@{ asset=$path; diff=Get-Diff $before.lines $after.lines }
  }
  foreach ($baseline in $results) {
    foreach ($file in $baseline.packages.files) { $file.Remove('lines') }
    foreach ($surface in $baseline.packages.assets.surface) {
      $surface.declarationCount=$surface.declarations.Count
      $surface.declarationsSha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(($surface.declarations -join "`n")+"`n"))).ToLowerInvariant()
      $surface.obsoleteDeclarations=@($surface.declarations | Where-Object { $_.Contains('[System.ObsoleteAttribute(') -or $_.Contains('[System.Diagnostics.CodeAnalysis.ExperimentalAttribute(') })
      $surface.Remove('declarations')
    }
  }
  $rules=Get-Content -LiteralPath $DispositionManifest -Raw | ConvertFrom-Json
  $assignments=@()
  foreach ($diff in $diffs) {
    foreach ($side in @('added','removed')) {
      foreach ($line in $diff.diff[$side]) {
        $rule=@($rules.groups | Where-Object { $line -cmatch $_.pattern -and (-not $_.assetPattern -or $diff.asset -cmatch $_.assetPattern) } | Select-Object -First 1)
        if ($rule.Count -ne 1) { throw "Unowned API difference in $($diff.asset): $line" }
        $assignments += [PSCustomObject]@{ asset=$diff.asset; side=$side; group=$rule[0].id }
      }
    }
  }
  $counts=@($assignments | Group-Object asset,side,group | Sort-Object Name | ForEach-Object { [ordered]@{asset=$_.Group[0].asset;side=$_.Group[0].side;group=$_.Group[0].group;count=$_.Count} })
  $obsoleteOwnership=@()
  foreach ($baseline in $results) {
    foreach ($package in $baseline.packages) {
      foreach ($asset in $package.assets) {
        foreach ($line in $asset.surface.obsoleteDeclarations) {
          $name="$($package.id)/$($asset.path)"
          $rule=@($rules.groups | Where-Object { $line -cmatch $_.pattern -and (-not $_.assetPattern -or $name -cmatch $_.assetPattern) } | Select-Object -First 1)
          if ($rule.Count -ne 1) { throw "Unowned obsolete/experimental declaration in ${name}: $line" }
          $obsoleteOwnership += [ordered]@{target=$baseline.target;asset=$name;declaration=$line;group=$rule[0].id}
        }
      }
    }
  }
  $sources=Get-Content -LiteralPath $SourceManifest -Raw | ConvertFrom-Json
  if (-not $sources.sources.Count) { throw 'Empty source manifest.' }
  $sourceTexts=@{}
  foreach ($source in $sources.sources) {
    if ($source.commit -notmatch '^[0-9a-f]{40}$' -or $source.url -cne "https://raw.githubusercontent.com/$($source.repository)/$($source.commit)/$($source.path)") { throw "Unpinned source: $($source.url)" }
    $download=Join-Path $temporary 'source'
    Invoke-WebRequest -Uri $source.url -OutFile $download
    if ((Get-Hash $download) -cne $source.sha256) { throw "Official source hash mismatch: $($source.url)" }
    $sourceTexts["$($source.commit)/$($source.path)"]=@([IO.File]::ReadAllText($download) -split '\r?\n')
  }
  $sourceDiffs=@()
  foreach ($path in Get-Ordinal @($sources.sources | Where-Object repository -EQ 'dotnet/dotnet' | ForEach-Object path | Select-Object -Unique)) {
    $before=$sourceTexts["4099f83977f4d29604a9aeb971fb0a7bc1d5f699/$path"]
    $after=$sourceTexts["820230c386db5b0106c5430b3e5aee92a7826bf5/$path"]
    $sourceDiffs += [ordered]@{asset=$path;diff=Get-Diff $before $after}
  }
  Write-Host "PASS: $($sources.sources.Count) immutable official source hashes match."
  $report=[ordered]@{ schemaVersion=1; sourceManifestSha256=Get-Hash $SourceManifest; dispositionManifestSha256=Get-Hash $DispositionManifest; baselines=$results; publicApiDiff=$diffs; toolingTextDiff=$textDiffs; packageMatchedSourceDiff=$sourceDiffs; dispositions=$counts; obsoleteOwnership=$obsoleteOwnership }
  New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
  $json=ConvertTo-Json -InputObject $report -Depth 40
  [IO.File]::WriteAllText($output,$json.Replace("`r`n","`n")+"`n",[Text.UTF8Encoding]::new($false))
  if ($CompareWith) {
    $expected=Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json -AsHashtable | ConvertTo-Json -Depth 40
    if ($expected -cne $json) { throw "EF assets/API comparison differs from $CompareWith" }
    Write-Host 'PASS: exact package/tooling hashes, public declaration deltas and ownership reproduce.'
  }
  foreach ($diff in $diffs) { Write-Host "$($diff.asset): +$($diff.diff.added.Count) -$($diff.diff.removed.Count)" }
} finally {
  $resolved=[IO.Path]::GetFullPath($temporary)
  $tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
  if (-not $resolved.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe temporary path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
