[CmdletBinding()]
param(
  [string]$Net10Root = 'C:/Program Files/dotnet',
  [string]$Net11Root = 'C:/Users/pingu/.dotnet-playbook-11',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/web.assets.json'),
  [string]$CompareWith,
  [string]$SourceManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/web.sources.json'),
  [string]$DispositionManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/web.dispositions.json')
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

function Get-WithoutCompilerImplementation([string]$Line) {
  # Only exact counterparts after this normalization qualify for exclusion.
  ($Line -replace '\[System.Runtime.CompilerServices.(?:Async|AsyncIterator|Iterator)StateMachineAttribute\([^\]]*\)\]', '' -replace '\[System.Runtime.CompilerServices.CompilerGeneratedAttribute\(\)\]', '' -replace '\s+', ' ').Trim()
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

$temporary = Join-Path ([IO.Path]::GetTempPath()) "playbook-web-$([guid]::NewGuid().ToString('N'))"
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
    $selected = @($references | Where-Object {
      [IO.Path]::GetFileName($_) -match '^Microsoft\.(AspNetCore\.(?!Components[\.]|Components\.dll|Razor[\.]|Razor\.dll)|Extensions\.Identity\.|Net\.Http\.Headers)'
    })
    $ids = @('microsoft.aspnetcore.openapi','microsoft.aspnetcore.authentication.jwtbearer',
      'microsoft.aspnetcore.authentication.openidconnect','microsoft.aspnetcore.authentication.wsfederation',
      'microsoft.aspnetcore.authentication.negotiate','microsoft.aspnetcore.identity.ui',
      'microsoft.aspnetcore.identity.entityframeworkcore','microsoft.aspnetcore.mvc.razor.runtimecompilation',
      'microsoft.aspnetcore.mvc.newtonsoftjson','microsoft.aspnetcore.signalr.client',
      'microsoft.aspnetcore.signalr.client.core','microsoft.aspnetcore.http.connections.client',
      'microsoft.aspnetcore.signalr.protocols.messagepack','microsoft.aspnetcore.signalr.protocols.newtonsoftjson',
      'microsoft.aspnetcore.grpc.jsontranscoding','microsoft.aspnetcore.mvc.testing','microsoft.aspnetcore.testhost',
      'microsoft.aspnetcore.jsonpatch.systemtextjson','microsoft.aspnetcore.jsonpatch',
      'microsoft.extensions.apidescription.server','grpc.aspnetcore.server','grpc.net.client','grpc.net.clientfactory')
    if ($baseline.target -eq 'net10.0') { $ids += 'microsoft.aspnetcore.grpc.swagger' }
    else { $ids += 'microsoft.aspnetcore.server.kestrel.transport.directtls' }
    $packages = @()
    foreach ($id in $ids) {
      $version = switch ($id) {
        'microsoft.aspnetcore.grpc.swagger' { '0.10.11' }
        'microsoft.aspnetcore.server.kestrel.transport.directtls' { '11.0.0-rc.1.26425.128' }
        { $_.StartsWith('grpc.') } { '2.64.0' }
        default { $baseline.version }
      }
      $packages += Get-Package $id $version $baseline.target
    }
    # Resolve metadata only, choosing declared minimum dependency versions. This
    # is not a NuGet restore or proof that these dependencies form an app graph.
    for ($i=0; $i -lt $packages.Count; $i++) {
      foreach ($dependency in $packages[$i].dependencies) {
        $id,$range = $dependency -split '/',2
        $id=$id.ToLowerInvariant()
        if ($id -in $packages.id -or "$id.dll" -in @($references | ForEach-Object { [IO.Path]::GetFileName($_).ToLowerInvariant() })) { continue }
        $version=[regex]::Match($range,'\d+\.\d+(?:\.\d+)?(?:-[0-9A-Za-z.-]+)?').Value
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
        if ($package.id -in $ids) { $package.assets += [ordered]@{ path=[IO.Path]::GetRelativePath($package.directory,$file.FullName).Replace('\','/'); fullPath=$file.FullName } }
      }
    }
    $surfaces = @($selected | Sort-Object | ForEach-Object {
      $surface=Get-Surface $_ $resolverPaths
      $surface.path=[IO.Path]::GetRelativePath($baseline.root,$_).Replace('\','/')
      $surface
    })
    foreach ($package in $packages) {
      foreach ($asset in $package.assets) { $asset.surface=Get-Surface $asset.fullPath $resolverPaths; $asset.Remove('fullPath') }
      foreach ($file in @(Get-ChildItem $package.directory -Recurse -File | Where-Object { $_.FullName -match '\.(props|targets|js|json)$|[\\/](tools|analyzers)[\\/].*\.dll$' } | Sort-Object FullName)) {
        $package.files += Get-TextAsset $file.FullName $package.directory
      }
      $package.Remove('directory')
      $package.role=if ($package.id -in $ids) { 'reviewed' } else { 'metadata resolution only' }
    }
    $sdk = Join-Path $baseline.root "sdk/$($baseline.sdk)"
    $sdkReferences=@($resolverPaths)
    foreach ($file in Get-ChildItem $sdk -Filter '*.dll') {
      if ($file.Name -notin @($sdkReferences | ForEach-Object { [IO.Path]::GetFileName($_) })) { $sdkReferences += $file.FullName }
    }
    $taskPath=Join-Path $sdk "Sdks/Microsoft.NET.Sdk.Web.ProjectSystem/tools/$($baseline.target)/Microsoft.NET.Sdk.Web.ProjectSystem.Tasks.dll"
    $sdkSurface=Get-Surface $taskPath $sdkReferences
    $sdkSurface.path=[IO.Path]::GetRelativePath($sdk,$taskPath).Replace('\','/')
    $tooling = @(Get-ChildItem (Join-Path $sdk 'Sdks/Microsoft.NET.Sdk.Web.ProjectSystem'),(Join-Path $sdk 'DotnetTools') -Recurse -File | Where-Object {
      ($_.FullName -match 'Microsoft.NET.Sdk.Web.ProjectSystem' -and $_.Extension -in @('.targets','.props','.dll')) -or
      ($_.FullName -match '[\\/]aspnetcoretools[\\/]' -and $_.Extension -in @('.dll','.json','.exe'))
    } | Sort-Object FullName | ForEach-Object { Get-TextAsset $_.FullName $sdk })
    $templatePackage = @(Get-ChildItem (Join-Path $baseline.root "templates/$($baseline.version)") -Filter 'microsoft.dotnet.web.projecttemplates*.nupkg')
    if ($templatePackage.Count -ne 1) { throw 'Missing/ambiguous actual web template package.' }
    $templateRoot=Join-Path $temporary "templates-$($baseline.target)"
    [IO.Compression.ZipFile]::ExtractToDirectory($templatePackage[0].FullName,$templateRoot)
    $mcpPackage=@(Get-ChildItem (Join-Path $baseline.root "templates/$($baseline.version)") -Filter 'microsoft.mcpserver.projecttemplates*.nupkg')
    if ($mcpPackage.Count -ne 1) { throw 'Missing/ambiguous bundled MCP template package.' }
    [IO.Compression.ZipFile]::ExtractToDirectory($mcpPackage[0].FullName,(Join-Path $templateRoot 'mcp'))
    $templates=@(Get-ChildItem $templateRoot -Recurse -File | Where-Object {
      ($_.FullName -match '[\\/](EmptyWeb|WebApi|WebApiAot|StarterWeb|RazorPagesWeb|RazorClassLibrary|GrpcService|Worker)-(CSharp|FSharp)[\\/]' -or $_.FullName -match '[\\/]mcp[\\/]') -and
      $_.FullName -notmatch '[\\/](localize|bootstrap|lib)[\\/]|\.min\.|\.map$' -and $_.Extension -in @('.cs','.csproj','.fs','.fsproj','.cshtml','.json','.js','.css','.html','.props','.targets','.proto')
    } | Sort-Object FullName | ForEach-Object { Get-TextAsset $_.FullName $templateRoot })
    $npmVersion=if ($baseline.target -eq 'net10.0') { '10.0.11' } else { '11.0.0-preview.7.26381.103' }
    $npmUrl="https://registry.npmjs.org/@microsoft/signalr/-/signalr-$npmVersion.tgz"
    $npmArchive=Join-Path $temporary "signalr-$npmVersion.tgz"
    Invoke-WebRequest $npmUrl -OutFile $npmArchive
    $npmRoot=Join-Path $temporary "signalr-$npmVersion"
    New-Item -ItemType Directory -Path $npmRoot | Out-Null
    $stream=[IO.File]::OpenRead($npmArchive)
    $gzip=[IO.Compression.GZipStream]::new($stream,[IO.Compression.CompressionMode]::Decompress)
    try { [System.Formats.Tar.TarFile]::ExtractToDirectory($gzip,$npmRoot,$false) } finally { $gzip.Dispose(); $stream.Dispose() }
    $typeScript=@(Get-ChildItem (Join-Path $npmRoot 'package/dist/esm') -File | Where-Object { $_.Name -match '\.(d\.ts|js)$' } | Sort-Object Name | ForEach-Object {
      $entry=[ordered]@{path=$_.Name;sha256=Get-Hash $_.FullName}
      if ($_.Name.EndsWith('.d.ts')) { $entry.lines=@([IO.File]::ReadAllText($_.FullName) -split '\r?\n') }
      $entry
    })
    $results += [ordered]@{ sdk=$baseline.sdk; referencePack=$baseline.version; target=$baseline.target; metadataReaderSha256=Get-Hash (Join-Path $sdk 'System.Reflection.MetadataLoadContext.dll'); framework=$surfaces; sdkSurface=$sdkSurface; packages=$packages; webTooling=$tooling; signalrNpm=[ordered]@{version=$npmVersion;url=$npmUrl;sha256=Get-Hash $npmArchive;files=$typeScript}; templatePackage=[ordered]@{ file=$templatePackage[0].Name; sha256=Get-Hash $templatePackage[0].FullName; bundledMcp=[ordered]@{file=$mcpPackage[0].Name;sha256=Get-Hash $mcpPackage[0].FullName}; files=$templates } }
  }
  $textDiffs=@()
  foreach ($family in @('webTooling','packageFiles','templates','typeScript')) {
    $pair=@($results | ForEach-Object {
      $files=if ($family -eq 'webTooling') { @($_.webTooling) } elseif ($family -eq 'templates') { @($_.templatePackage.files) } elseif ($family -eq 'typeScript') { @($_.signalrNpm.files) } else { @($_.packages | Where-Object role -EQ 'reviewed' | ForEach-Object { $id=$_.id; $_.files | ForEach-Object { [ordered]@{ path="$id/$($_.path)"; lines=$_.lines } } }) }
      ,$files
    })
    $old,$new=$pair
    foreach ($path in Get-Ordinal @(@($old.path)+@($new.path) | Select-Object -Unique)) {
      $before=@($old | Where-Object path -CEQ $path); $after=@($new | Where-Object path -CEQ $path)
      if (@($before.lines).Count -eq 0 -and @($after.lines).Count -eq 0) { continue }
      $textDiffs += [ordered]@{ asset="$family/$path"; diff=Get-Diff $before.lines $after.lines }
    }
  }
  $diffs=@()
  $diffs += [ordered]@{asset='sdk/Microsoft.NET.Sdk.Web.ProjectSystem.Tasks.dll';diff=Get-Diff $results[0].sdkSurface.declarations $results[1].sdkSurface.declarations}
  foreach ($name in Get-Ordinal @($results.framework.name | Select-Object -Unique)) {
    $old=@($results[0].framework | Where-Object name -CEQ $name); $new=@($results[1].framework | Where-Object name -CEQ $name)
    $diffs += [ordered]@{ asset="framework/$name"; diff=Get-Diff $old.declarations $new.declarations }
  }
  foreach ($id in Get-Ordinal @($results.packages | Where-Object role -EQ 'reviewed' | ForEach-Object id | Select-Object -Unique)) {
    $old=@($results[0].packages | Where-Object id -EQ $id); $new=@($results[1].packages | Where-Object id -EQ $id)
    foreach ($name in Get-Ordinal @(@($old.assets.surface.name)+@($new.assets.surface.name) | Select-Object -Unique)) {
      $before=@($old.assets.surface | Where-Object name -CEQ $name); $after=@($new.assets.surface | Where-Object name -CEQ $name)
      $diffs += [ordered]@{ asset="$id/$name"; diff=Get-Diff $before.declarations $after.declarations }
    }
  }
  foreach ($baseline in $results) {
    foreach ($file in @($baseline.webTooling)+@($baseline.templatePackage.files)+@($baseline.packages.files)+@($baseline.signalrNpm.files)) { $file.Remove('lines') }
    foreach ($surface in @($baseline.framework)+@($baseline.packages.assets.surface)+@($baseline.sdkSurface)) {
      $surface.declarationCount=$surface.declarations.Count
      $surface.declarationsSha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(($surface.declarations -join "`n")+"`n"))).ToLowerInvariant()
      $surface.obsoleteDeclarations=@($surface.declarations | Where-Object { $_.Contains('[System.ObsoleteAttribute(') -or $_.Contains('[System.Diagnostics.CodeAnalysis.ExperimentalAttribute(') })
      $surface.Remove('declarations')
    }
  }
  $rules=Get-Content $DispositionManifest -Raw | ConvertFrom-Json
  $assignments=@(); $unowned=@(); $toolingAssignments=@()
  foreach ($diff in $textDiffs | Where-Object { $_.diff.added.Count -or $_.diff.removed.Count }) {
    $rule=@($rules.toolingGroups | Where-Object { $diff.asset -cmatch $_.pattern } | Select-Object -First 1)
    if ($rule.Count -ne 1) { $unowned += $diff.asset }
    else { $toolingAssignments += [ordered]@{ asset=$diff.asset; group=$rule[0].id } }
  }
  foreach ($diff in $diffs) {
    foreach ($side in @('added','removed')) {
      foreach ($line in $diff.diff[$side]) {
        $otherSide=if ($side -eq 'added') { 'removed' } else { 'added' }
        $normalized=Get-WithoutCompilerImplementation $line
        $counterpart=@($diff.diff[$otherSide] | Where-Object { (Get-WithoutCompilerImplementation $_) -ceq $normalized })
        if ($counterpart.Count) {
          $assignments += [PSCustomObject]@{ asset=$diff.asset; side=$side; group='compiler-implementation-only' }
          continue
        }
        $rule=@($rules.groups | Where-Object { $line -cmatch $_.pattern -and (-not $_.assetPattern -or $diff.asset -cmatch $_.assetPattern) } | Select-Object -First 1)
        if ($rule.Count -ne 1) { $unowned += $line }
        else { $assignments += [PSCustomObject]@{ asset=$diff.asset; side=$side; group=$rule[0].id } }
      }
    }
  }
  $counts=@($assignments | Group-Object asset,side,group | Sort-Object Name | ForEach-Object {
    [ordered]@{ asset=$_.Group[0].asset; side=$_.Group[0].side; group=$_.Group[0].group; count=$_.Count }
  })
  $sources=Get-Content $SourceManifest -Raw | ConvertFrom-Json
  foreach ($source in $sources.sources) {
    if ($source.commit -notmatch '^[0-9a-f]{40}$' -or $source.url -cne "https://raw.githubusercontent.com/$($source.repository)/$($source.commit)/$($source.path)") { throw 'Unpinned source.' }
    $file=Join-Path $temporary 'source'
    Invoke-WebRequest $source.url -OutFile $file
    if ((Get-Hash $file) -cne $source.sha256) { throw "Official source hash mismatch: $($source.path)" }
  }
  $report=[ordered]@{ schemaVersion=1; sourceManifestSha256=Get-Hash $SourceManifest; dispositionManifestSha256=Get-Hash $DispositionManifest; baselines=$results; publicApiDiff=$diffs; publicToolingDiff=$textDiffs; dispositions=$counts; toolingDispositions=$toolingAssignments }
  $json=ConvertTo-Json $report -Depth 40
  New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
  [IO.File]::WriteAllText($output,$json.Replace("`r`n","`n")+"`n",[Text.UTF8Encoding]::new($false))
  if ($unowned.Count) { throw "Unowned API/tooling differences: $($unowned.Count). Inspect $output before adding dispositions." }
  if ($CompareWith) {
    $expected=Get-Content $CompareWith -Raw | ConvertFrom-Json -AsHashtable | ConvertTo-Json -Depth 40
    if ($expected -cne $json) { throw "Web assets/API comparison differs from $CompareWith" }
    Write-Host 'PASS: exact artifact, tooling, template and public API differences reproduce.'
  }
  foreach ($diff in $diffs | Where-Object { $_.diff.added.Count -or $_.diff.removed.Count }) { Write-Host "$($diff.asset): +$($diff.diff.added.Count) -$($diff.diff.removed.Count)" }
  Write-Host "PASS: $($sources.sources.Count) immutable source hashes match. Metadata only; no consumer behavior claim."
} finally {
  $resolved=[IO.Path]::GetFullPath($temporary)
  $tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
  if (-not $resolved.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe temporary path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
