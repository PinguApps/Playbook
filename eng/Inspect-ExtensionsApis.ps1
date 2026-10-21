[CmdletBinding()]
param(
  [string]$Net10Root = 'C:/Program Files/dotnet',
  [string]$Net11Root = 'C:/Users/pingu/.dotnet-playbook-11',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/extensions.assets.json'),
  [string]$CompareWith,
  [string]$SourceManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/extensions.sources.json'),
  [string]$DispositionManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/extensions.dispositions.json')
)

# Metadata only: no inspected code, restore, SDK first-run setup or PATH changes.
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputPath)
if ($CompareWith -and $output -eq [IO.Path]::GetFullPath($CompareWith)) {
  throw 'Comparison output must not overwrite the committed baseline.'
}
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
        if (-not (Test-InScope $Path $exportName)) { continue }
        $destination = if ($export.Implementation.Kind -eq [Reflection.Metadata.HandleKind]::AssemblyReference) {
          $reference = $metadata.GetAssemblyReference([Reflection.Metadata.AssemblyReferenceHandle]::op_Explicit($export.Implementation))
          $metadata.GetString($reference.Name)
        } else { $export.Implementation.Kind.ToString() }
        $lines.Add("forward $($metadata.GetString($export.Namespace)).$($metadata.GetString($export.Name)) -> $destination [$($export.Attributes)]")
      }
    } finally { $pe.Dispose(); $stream.Dispose() }
    foreach ($type in $assembly.GetTypes() | Where-Object IsVisible) {
      $name = $type.FullName
      if (-not (Test-InScope $Path $name)) { continue }
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

function Test-InScope([string]$Path, [string]$Name) { return $true }
$temporary = Join-Path ([IO.Path]::GetTempPath()) "playbook-extensions-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $temporary | Out-Null
try {
  Add-Type -Path (Join-Path $Net10Root 'sdk/10.0.402/System.Reflection.MetadataLoadContext.dll')
  $baselines = @(
    @{ root = $Net10Root; sdk = '10.0.402'; version = '10.0.13'; target = 'net10.0' },
    @{ root = $Net11Root; sdk = '11.0.100-rc.2.26504.105'; version = '11.0.0-rc.2.26504.105'; target = 'net11.0' }
  )
  $results = @()
  foreach ($baseline in $baselines) {
    $core = Join-Path $baseline.root "packs/Microsoft.NETCore.App.Ref/$($baseline.version)/ref/$($baseline.target)"
    $asp = Join-Path $baseline.root "packs/Microsoft.AspNetCore.App.Ref/$($baseline.version)/ref/$($baseline.target)"
    if (-not (Test-Path -LiteralPath $core) -or -not (Test-Path -LiteralPath $asp)) { throw 'Missing pinned reference packs.' }
    $references = @(Get-ChildItem -LiteralPath $core,$asp -Filter '*.dll' | ForEach-Object FullName)
    $selected = @($references | Where-Object {
      $name = [IO.Path]::GetFileName($_)
      ($name -match '^Microsoft\.Extensions\.' -and $name -notmatch '^Microsoft\.Extensions\.(Features|Identity\.|Localization|WebEncoders)') -or
      $name -match '^System\.(ComponentModel\.(Annotations|DataAnnotations)|Diagnostics\.(Contracts|Debug|DiagnosticSource|StackTrace|TextWriterTraceListener|Tools|TraceSource|Tracing))\.dll$'
    } | Sort-Object)
    Write-Host "Inspecting $($baseline.target): $($selected.Count) framework references."
    $framework = @($selected | ForEach-Object {
      $surface = Get-Surface $_ $references
      $surface.path = [IO.Path]::GetRelativePath($baseline.root, $_).Replace('\', '/')
      $surface
    })
    $packages = @()
    $packageRoots = @()
    foreach ($id in @('microsoft.extensions.caching.hybrid', 'microsoft.extensions.configuration.binder', 'microsoft.extensions.options', 'microsoft.extensions.logging.abstractions', 'microsoft.extensions.validation', 'system.diagnostics.eventlog', 'system.diagnostics.performancecounter')) {
      $version = if ($id -eq 'microsoft.extensions.caching.hybrid') { if ($baseline.target -eq 'net10.0') { '10.0.0' } else { '10.11.0' } } else { $baseline.version }
      $url = "https://api.nuget.org/v3-flatcontainer/$id/$version/$id.$version.nupkg"
      $zip = Join-Path $temporary "$id.$version.nupkg"
      Invoke-WebRequest -Uri $url -OutFile $zip
      $directory = "$zip.contents"
      [IO.Compression.ZipFile]::ExtractToDirectory($zip, $directory)
      [xml]$nuspec = Get-Content -LiteralPath (Get-ChildItem $directory -Filter '*.nuspec').FullName -Raw
      $packageRoots += $directory
      $dependencies = @($nuspec.package.metadata.dependencies.group | ForEach-Object {
        [ordered]@{ target = $_.targetFramework; dependencies = @($_.dependency | Where-Object { $null -ne $_ } | ForEach-Object { "$($_.id)/$($_.version)" } | Sort-Object) }
      })
      $packages += [ordered]@{ id = $id; version = $version; url = $url; sha256 = Get-Hash $zip; repository = $nuspec.package.metadata.repository.url; commit = $nuspec.package.metadata.repository.commit; dependencies = $dependencies; assets = @(); otherAssets = @(); tooling = @() }
    }
    for ($i = 0; $i -lt $packages.Count; $i++) {
      $package = $packages[$i]; $directory = $packageRoots[$i]
      foreach ($asset in Get-ChildItem (Join-Path $directory 'lib') -Recurse -Filter '*.dll' | Sort-Object FullName) {
        $relative = [IO.Path]::GetRelativePath($directory, $asset.FullName).Replace('\', '/')
        if ($relative -notmatch '^lib/net1[01]\.0/') {
          $package.otherAssets += [ordered]@{ path = $relative; sha256 = Get-Hash $asset.FullName }
          continue
        }
        $package.assets += [ordered]@{ path = $relative; surface = Get-Surface $asset.FullName $references }
      }
      $expectedTarget = if ($package.id -eq 'microsoft.extensions.caching.hybrid') { 'net10.0' } else { $baseline.target }
      if (@($package.assets | Where-Object path -Match "^lib/$expectedTarget/").Count -ne 1) { throw "Missing/ambiguous $($package.id) target asset." }
      foreach ($tool in Get-ChildItem $directory -Recurse -Filter '*.dll' | Where-Object FullName -NotMatch '[\\/]lib[\\/]' | Sort-Object FullName) {
        $package.tooling += [ordered]@{ path = [IO.Path]::GetRelativePath($directory, $tool.FullName).Replace('\', '/'); sha256 = Get-Hash $tool.FullName }
      }
    }
    $results += [ordered]@{ sdk = $baseline.sdk; referencePack = $baseline.version; target = $baseline.target; metadataReaderSha256 = Get-Hash (Join-Path $baseline.root "sdk/$($baseline.sdk)/System.Reflection.MetadataLoadContext.dll"); framework = $framework; packages = $packages }
  }
  $diffs = @()
  foreach ($name in Get-Ordinal @($results.framework.name | Select-Object -Unique)) {
    $old = @($results[0].framework | Where-Object name -CEQ $name)
    $new = @($results[1].framework | Where-Object name -CEQ $name)
    $diffs += [ordered]@{ asset = "framework/$name"; diff = Get-Diff $old.declarations $new.declarations }
  }
  foreach ($package in $results[0].packages) {
    $newPackage = $results[1].packages | Where-Object id -EQ $package.id
    $old = $package.assets | Where-Object path -Match '^lib/net10.0/'
    foreach ($new in $newPackage.assets) {
      $diffs += [ordered]@{ asset = "$($package.id)/net10.0->$($new.path)"; diff = Get-Diff $old.surface.declarations $new.surface.declarations }
    }
    $new10 = $newPackage.assets | Where-Object path -Match '^lib/net10.0/'
    $new11 = $newPackage.assets | Where-Object path -Match '^lib/net11.0/'
    if (@($old).Count -ne 1 -or $newPackage.assets.Count -eq 0) { throw "Missing package comparison asset: $($package.id)" }
    if (@($new11).Count -eq 1 -and @($new10).Count -eq 1) {
      $diffs += [ordered]@{ asset = "$($package.id)/RC2-net10.0->net11.0"; diff = Get-Diff $new10.surface.declarations $new11.surface.declarations }
    }
  }
  foreach ($baseline in $results) {
    foreach ($surface in @($baseline.framework) + @($baseline.packages.assets.surface)) {
      $bytes = [Text.Encoding]::UTF8.GetBytes(($surface.declarations -join "`n") + "`n")
      $surface.declarationCount = $surface.declarations.Count
      $surface.declarationsSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      $surface.obsoleteDeclarations = @($surface.declarations | Where-Object { $_.Contains('[System.ObsoleteAttribute(') })
      $surface.Remove('declarations')
    }
  }
  $rules = Get-Content -LiteralPath $DispositionManifest -Raw | ConvertFrom-Json
  $assignments = @()
  foreach ($diff in $diffs) {
    foreach ($side in @('added', 'removed')) {
      foreach ($line in $diff.diff[$side]) {
        $rule = @($rules.groups | Where-Object { $line -cmatch $_.pattern -and (-not $_.assetPattern -or $diff.asset -cmatch $_.assetPattern) } | Select-Object -First 1)
        if ($rule.Count -ne 1) { throw "Unowned API difference in $($diff.asset): $line" }
        $assignments += [PSCustomObject]@{ asset = $diff.asset; side = $side; group = $rule[0].id }
      }
    }
  }
  $counts = @($assignments | Group-Object asset, side, group | Sort-Object Name | ForEach-Object {
    [ordered]@{ asset = $_.Group[0].asset; side = $_.Group[0].side; group = $_.Group[0].group; count = $_.Count }
  })
  $report = [ordered]@{ schemaVersion = 1; sourceManifestSha256 = Get-Hash $SourceManifest; dispositionManifestSha256 = Get-Hash $DispositionManifest; baselines = $results; publicApiDiff = $diffs; dispositions = $counts }
  New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
  $json = ConvertTo-Json -InputObject $report -Depth 40
  [IO.File]::WriteAllText($output, $json.Replace("`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
  if ($CompareWith) {
    $expected = Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json -AsHashtable | ConvertTo-Json -Depth 40
    if ($expected -cne $json) { throw "Extensions assets/API comparison differs from $CompareWith" }
    Write-Host 'PASS: exact package, tooling and reference hashes plus all public declarations/diffs match.'
  }
  $sources = Get-Content -LiteralPath $SourceManifest -Raw | ConvertFrom-Json
  if ($sources.sources.Count -eq 0) { throw 'Empty source manifest.' }
  $sourceTexts = @{}
  foreach ($source in $sources.sources) {
    if ($source.commit -notmatch '^[0-9a-f]{40}$' -or $source.url -cne "https://raw.githubusercontent.com/$($source.repository)/$($source.commit)/$($source.path)") { throw "Unpinned source: $($source.url)" }
    $download = Join-Path $temporary 'source'
    Invoke-WebRequest -Uri $source.url -OutFile $download
    if ((Get-Hash $download) -cne $source.sha256) { throw "Official source hash mismatch: $($source.url)" }
    $sourceTexts["$($source.commit)/$($source.path)"] = [IO.File]::ReadAllText($download)
  }
  Write-Host "PASS: $($sources.sources.Count) immutable official source hashes match."
  foreach ($comparison in $sources.generatorComparisons) {
    foreach ($side in @(@{ commit = '4099f83977f4d29604a9aeb971fb0a7bc1d5f699'; ids = $comparison.oldIds }, @{ commit = '820230c386db5b0106c5430b3e5aee92a7826bf5'; ids = $comparison.newIds })) {
      $text = $sourceTexts["$($side.commit)/$($comparison.path)"]
      if ($null -eq $text) { throw "Missing generator source: $($comparison.path)" }
      $ids = @([regex]::Matches($text, '"(SYSLIB\d+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
      if (($ids -join ',') -cne ($side.ids -join ',')) { throw "Generator ID inventory differs: $($comparison.path)" }
    }
  }
  Write-Host "PASS: $($sources.generatorComparisons.Count) paired generator ID inventories reproduce."
  foreach ($diff in $diffs) { Write-Host "$($diff.asset): +$($diff.diff.added.Count) -$($diff.diff.removed.Count)" }
} finally {
  $resolved = [IO.Path]::GetFullPath($temporary)
  $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
  if (-not $resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe temporary path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
