[CmdletBinding()]
param(
  [string]$Net10Root = 'C:/Program Files/dotnet',
  [string]$Net11Root = 'C:/Users/pingu/.dotnet-playbook-11',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/blazor.assets.json'),
  [string]$CompareWith,
  [string]$SourceManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/blazor.sources.json'),
  [string]$DispositionManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/blazor.dispositions.json')
)

# Metadata and archive inspection only; inspected code is never executed.
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputPath)
if ($CompareWith -and $output -eq [IO.Path]::GetFullPath($CompareWith)) { throw 'Do not overwrite the comparison baseline.' }
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
  if ([IO.Path]::GetExtension($File) -in @('.cs','.csproj','.razor','.json','.js','.css','.html','.props','.targets')) { $entry.lines=@([IO.File]::ReadAllText($File) -split '\r?\n') }
  $entry
}
$temporary = Join-Path ([IO.Path]::GetTempPath()) "playbook-blazor-$([guid]::NewGuid().ToString('N'))"
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
    $selected = @($references | Where-Object { [IO.Path]::GetFileName($_) -match '^Microsoft\.(AspNetCore\.(Components|Razor)(\.|\.dll)|JSInterop(\.|\.dll))' })
    $packages = @()
    $packageRoots = @()
    $ids = @('microsoft.aspnetcore.components.webassembly','microsoft.aspnetcore.components.webassembly.authentication','microsoft.aspnetcore.components.quickgrid','microsoft.aspnetcore.components.quickgrid.entityframeworkadapter','microsoft.aspnetcore.components.webassembly.server','microsoft.aspnetcore.components.webassembly.devserver','microsoft.aspnetcore.components.analyzers','microsoft.aspnetcore.mvc.razor.runtimecompilation','microsoft.aspnetcore.razor.language','microsoft.jsinterop.webassembly')
    if ($baseline.target -eq 'net11.0') { $ids += @('microsoft.aspnetcore.components.ai','microsoft.aspnetcore.components.server.autopause','microsoft.aspnetcore.components.gateway','microsoft.aspnetcore.components.gateway.cli','microsoft.extensions.ai.abstractions','microsoft.extensions.dependencyinjection.abstractions') }
    foreach ($id in $ids) {
      $version = switch ($id) {
        'microsoft.aspnetcore.components.ai' { '11.0.0-preview.7.26425.128' }
        'microsoft.extensions.ai.abstractions' { '10.6.0' }
        'microsoft.extensions.dependencyinjection.abstractions' { '10.0.0' }
        'microsoft.aspnetcore.razor.language' { '6.0.0' }
        default { $baseline.version }
      }
      $url = "https://api.nuget.org/v3-flatcontainer/$id/$version/$id.$version.nupkg"
      $zip = Join-Path $temporary "$id.$version.nupkg"
      Invoke-WebRequest $url -OutFile $zip
      $directory = "$zip.$($baseline.target).contents"
      [IO.Compression.ZipFile]::ExtractToDirectory($zip,$directory)
      [xml]$nuspec = Get-Content (Get-ChildItem $directory -Filter '*.nuspec').FullName -Raw
      $packages += [ordered]@{ id=$id; version=$version; url=$url; sha256=Get-Hash $zip; repository=$nuspec.package.metadata.repository.url; commit=$nuspec.package.metadata.repository.commit; dependencies=@($nuspec.package.metadata.dependencies.group | ForEach-Object { [ordered]@{ target=$_.targetFramework; dependencies=@($_.dependency | Where-Object { $null -ne $_ } | ForEach-Object { "$($_.id)/$($_.version)" } | Sort-Object) } }); assets=@(); files=@() }
      $packageRoots += $directory
    }
    # Package references provide resolution only. First framework identity wins;
    # independently versioned dependencies are not claimed as release-family APIs.
    $resolverPaths = @($references)
    foreach ($directory in $packageRoots) {
      foreach ($file in @(Get-ChildItem $directory -Recurse -Filter '*.dll' | Where-Object FullName -Match '[\\/]lib[\\/]net(11\.0|10\.0|standard2\.0)[\\/]')) {
        if ([IO.Path]::GetFileName($file.FullName) -notin @($resolverPaths | ForEach-Object { [IO.Path]::GetFileName($_) })) { $resolverPaths += $file.FullName }
      }
    }
    $surfaces = @($selected | Sort-Object | ForEach-Object { $surface=Get-Surface $_ $resolverPaths; $surface.path=[IO.Path]::GetRelativePath($baseline.root,$_).Replace('\','/'); $surface })
    for ($i=0; $i -lt $packages.Count; $i++) {
      $package=$packages[$i]; $directory=$packageRoots[$i]
      foreach ($file in @(Get-ChildItem $directory -Recurse -File | Sort-Object FullName)) {
        $relative=[IO.Path]::GetRelativePath($directory,$file.FullName).Replace('\','/')
        if ($relative -match '^lib/net(10|11)\.0/.*\.dll$' -and $package.id -notmatch '^microsoft\.extensions\.') {
          $package.assets += [ordered]@{ path=$relative; surface=Get-Surface $file.FullName $resolverPaths }
        }
        # Preserve exact public build/script contracts, including tool-only packages.
        if ($relative -match '\.(targets|props|js|json)$' -or $relative -match '(analyzers|tools)/.*\.dll$') {
          $entry=[ordered]@{ path=$relative; sha256=Get-Hash $file.FullName }
          if ($relative -match '\.(targets|props)$') { $entry.lines=@([IO.File]::ReadAllText($file.FullName) -split '\r?\n') }
          $package.files += $entry
        }
      }
    }
    $sdk = Join-Path $baseline.root "sdk/$($baseline.sdk)"
    $sdkReferences=@($resolverPaths)
    foreach ($file in @(Get-ChildItem $sdk,(Join-Path $sdk 'Roslyn/bincore') -Filter '*.dll')+@(Get-ChildItem (Join-Path $sdk 'Sdks/Microsoft.NET.Sdk.Razor') -Recurse -Filter '*.dll' | Where-Object FullName -NotMatch '[\\/]net472[\\/]')) {
      if ($file.Name -notin @($sdkReferences | ForEach-Object { [IO.Path]::GetFileName($_) })) { $sdkReferences += $file.FullName }
    }
    $sdkSurfaces=@()
    foreach ($relative in @('Sdks/Microsoft.NET.Sdk.Razor/source-generators/Microsoft.CodeAnalysis.Razor.Compiler.dll',"Sdks/Microsoft.NET.Sdk.Razor/tasks/$($baseline.target)/Microsoft.NET.Sdk.Razor.Tasks.dll")) {
      $surface=Get-Surface (Join-Path $sdk $relative) $sdkReferences
      $surface.path=$relative
      $sdkSurfaces += $surface
    }
    $tooling = @(Get-ChildItem (Join-Path $sdk 'Sdks/Microsoft.NET.Sdk.Razor') -Recurse -File | Where-Object { $_.Extension -in @('.targets','.props','.dll') } | Sort-Object FullName | ForEach-Object {
      $entry=[ordered]@{ path=[IO.Path]::GetRelativePath($sdk,$_.FullName).Replace('\','/'); sha256=Get-Hash $_.FullName }
      if ($_.Extension -in @('.targets','.props')) { $entry.lines=@([IO.File]::ReadAllText($_.FullName) -split '\r?\n') }
      $entry
    })
    $templatePackage = @(Get-ChildItem (Join-Path $baseline.root "templates/$($baseline.version)") -Filter 'microsoft.dotnet.web.projecttemplates*.nupkg')
    if ($templatePackage.Count -ne 1) { throw 'Missing/ambiguous actual web template package.' }
    $templateRoot=Join-Path $temporary "templates-$($baseline.target)"
    [IO.Compression.ZipFile]::ExtractToDirectory($templatePackage[0].FullName,$templateRoot)
    $templates=@(Get-ChildItem $templateRoot -Recurse -File | Where-Object { $_.FullName -match '[\\/](BlazorWeb|BlazorWasm|BlazorWebWorker|BlazorWasmServiceDefaults)-CSharp[\\/]' -and $_.FullName -notmatch '[\\/](localize|bootstrap|lib)[\\/]|\.min\.|\.map$' -and $_.Extension -notin @('.png','.ico','.woff','.woff2') } | Sort-Object FullName | ForEach-Object { Get-TextAsset $_.FullName $templateRoot })
    $results += [ordered]@{ sdk=$baseline.sdk; referencePack=$baseline.version; target=$baseline.target; metadataReaderSha256=Get-Hash (Join-Path $sdk 'System.Reflection.MetadataLoadContext.dll'); framework=$surfaces; packages=$packages; sdkSurfaces=$sdkSurfaces; razorTooling=$tooling; templatePackage=[ordered]@{ file=$templatePackage[0].Name; sha256=Get-Hash $templatePackage[0].FullName; files=$templates } }
  }
  $textDiffs=@()
  foreach ($family in @('razorTooling','packageFiles','templates')) {
    $old=if ($family -eq 'razorTooling') { @($results[0].razorTooling) } elseif ($family -eq 'templates') { @($results[0].templatePackage.files) } else { @($results[0].packages | ForEach-Object { $id=$_.id; $_.files | ForEach-Object { [ordered]@{ path="$id/$($_.path)"; lines=$_.lines } } }) }
    $new=if ($family -eq 'razorTooling') { @($results[1].razorTooling) } elseif ($family -eq 'templates') { @($results[1].templatePackage.files) } else { @($results[1].packages | ForEach-Object { $id=$_.id; $_.files | ForEach-Object { [ordered]@{ path="$id/$($_.path)"; lines=$_.lines } } }) }
    foreach ($path in Get-Ordinal @(@($old.path)+@($new.path) | Select-Object -Unique)) {
      $before=@($old | Where-Object path -CEQ $path); $after=@($new | Where-Object path -CEQ $path)
      if (@($before.lines).Count -eq 0 -and @($after.lines).Count -eq 0) { continue }
      $textDiffs += [ordered]@{ asset="$family/$path"; diff=Get-Diff $before.lines $after.lines }
    }
  }
  $diffs=@()
  foreach ($name in Get-Ordinal @($results.sdkSurfaces.name | Select-Object -Unique)) {
    $old=@($results[0].sdkSurfaces | Where-Object name -CEQ $name); $new=@($results[1].sdkSurfaces | Where-Object name -CEQ $name)
    $diffs += [ordered]@{ asset="sdk/$name"; diff=Get-Diff $old.declarations $new.declarations }
  }
  foreach ($name in Get-Ordinal @($results.framework.name | Select-Object -Unique)) {
    $old=@($results[0].framework | Where-Object name -CEQ $name); $new=@($results[1].framework | Where-Object name -CEQ $name)
    $diffs += [ordered]@{ asset="framework/$name"; diff=Get-Diff $old.declarations $new.declarations }
  }
  foreach ($id in Get-Ordinal @($results.packages.id | Where-Object { $_ -notmatch '^microsoft\.extensions\.' } | Select-Object -Unique)) {
    $old=@($results[0].packages | Where-Object id -EQ $id); $new=@($results[1].packages | Where-Object id -EQ $id)
    foreach ($name in Get-Ordinal @(@($old.assets.surface.name)+@($new.assets.surface.name) | Select-Object -Unique)) {
      $before=@($old.assets.surface | Where-Object name -CEQ $name); $after=@($new.assets.surface | Where-Object name -CEQ $name)
      $diffs += [ordered]@{ asset="$id/$name"; diff=Get-Diff $before.declarations $after.declarations }
    }
  }
  foreach ($baseline in $results) {
    foreach ($file in @($baseline.razorTooling)+@($baseline.templatePackage.files)+@($baseline.packages.files)) { $file.Remove('lines') }
    foreach ($surface in @($baseline.framework)+@($baseline.packages.assets.surface)+@($baseline.sdkSurfaces)) {
      $surface.declarationCount=$surface.declarations.Count
      $surface.declarationsSha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(($surface.declarations -join "`n")+"`n"))).ToLowerInvariant()
      $surface.obsoleteDeclarations=@($surface.declarations | Where-Object { $_.Contains('[System.ObsoleteAttribute(') -or $_.Contains('[System.Diagnostics.CodeAnalysis.ExperimentalAttribute(') })
      $surface.Remove('declarations')
    }
  }
  $rules=Get-Content $dispositionManifest -Raw | ConvertFrom-Json
  $assignments=@()
  $unowned=@()
  $toolingAssignments=@()
  foreach ($diff in $textDiffs | Where-Object { $_.diff.added.Count -or $_.diff.removed.Count }) {
    $rule=@($rules.toolingGroups | Where-Object { $diff.asset -cmatch $_.pattern } | Select-Object -First 1)
    if ($rule.Count -ne 1) { $unowned += [ordered]@{ asset=$diff.asset; side='tooling'; declaration='changed text contract' } }
    else { $toolingAssignments += [ordered]@{ asset=$diff.asset; group=$rule[0].id } }
  }
  foreach ($diff in $diffs) {
    foreach ($side in @('added','removed')) {
      foreach ($line in $diff.diff[$side]) {
        $rule=@($rules.groups | Where-Object { $line -cmatch $_.pattern -and (-not $_.assetPattern -or $diff.asset -cmatch $_.assetPattern) } | Select-Object -First 1)
        if ($rule.Count -ne 1) { $unowned += [ordered]@{ asset=$diff.asset; side=$side; declaration=$line } }
        else { $assignments += [PSCustomObject]@{ asset=$diff.asset; side=$side; group=$rule[0].id } }
      }
    }
  }
  $counts=@($assignments | Group-Object asset,side,group | Sort-Object Name | ForEach-Object {
    [ordered]@{ asset=$_.Group[0].asset; side=$_.Group[0].side; group=$_.Group[0].group; count=$_.Count }
  })
  $sources=Get-Content $sourceManifest -Raw | ConvertFrom-Json
  $sourceTexts=@{}
  foreach ($source in $sources.sources) {
    if ($source.commit -notmatch '^[0-9a-f]{40}$' -or $source.url -cne "https://raw.githubusercontent.com/$($source.repository)/$($source.commit)/$($source.path)") { throw 'Unpinned source.' }
    $file=Join-Path $temporary 'source'
    Invoke-WebRequest $source.url -OutFile $file
    if ((Get-Hash $file) -cne $source.sha256) { throw "Official source hash mismatch: $($source.path)" }
    $sourceTexts["$($source.commit)/$($source.path)"]=[IO.File]::ReadAllText($file)
  }
  $diagnostics=@()
  foreach ($path in @('src/Components/Analyzers/src/DiagnosticDescriptors.cs','src/Framework/AspNetCoreAnalyzers/src/Analyzers/DiagnosticDescriptors.cs')) {
    $oldText=$sourceTexts["0ef4bbfa3291b306a21e5001cb0491277bdd35bc/$path"]
    $newText=$sourceTexts["4a70870818bf5662f21b0c28f9a8c267dccb77f0/$path"]
    if (-not $oldText -or -not $newText) { throw 'Missing paired descriptor source.' }
    $oldIds=@([regex]::Matches($oldText,'"((?:BL|ASP)\d+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
    $newIds=@([regex]::Matches($newText,'"((?:BL|ASP)\d+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
    $diagnostics += [ordered]@{ path=$path; oldIds=$oldIds; newIds=$newIds; idDiff=Get-Diff $oldIds $newIds; sourceDiff=Get-Diff ($oldText -split '\r?\n') ($newText -split '\r?\n') }
  }
  $report=[ordered]@{ schemaVersion=1; sourceManifestSha256=Get-Hash $sourceManifest; dispositionManifestSha256=Get-Hash $dispositionManifest; baselines=$results; publicApiDiff=$diffs; publicToolingDiff=$textDiffs; analyzerDiff=$diagnostics; dispositions=$counts; toolingDispositions=$toolingAssignments }
  $json=ConvertTo-Json $report -Depth 40
  New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
  [IO.File]::WriteAllText($output,$json.Replace("`r`n","`n")+"`n",[Text.UTF8Encoding]::new($false))
  if ($unowned.Count) { throw "Unowned API differences: $($unowned.Count). Inspect publicApiDiff in $output before adding dispositions." }
  if ($CompareWith) {
    $expected=Get-Content $CompareWith -Raw | ConvertFrom-Json -AsHashtable | ConvertTo-Json -Depth 40
    if ($expected -cne $json) { throw "Blazor assets/API comparison differs from $CompareWith" }
    Write-Host 'PASS: exact artifact, tooling, template and public API differences reproduce.'
  }
  foreach ($diff in $diffs) { Write-Host "$($diff.asset): +$($diff.diff.added.Count) -$($diff.diff.removed.Count)" }
  Write-Host "PASS: $($sources.sources.Count) immutable source hashes match. Metadata only; no consumer behavior claim."
} finally {
  $resolved=[IO.Path]::GetFullPath($temporary)
  $tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
  if (-not $resolved.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe temporary path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
