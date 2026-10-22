[CmdletBinding()]
param(
  [string]$Net10Root = 'C:/Program Files/dotnet',
  [string]$Net11Root = 'C:/Users/pingu/.dotnet-playbook-11',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/sdk-build.assets.json'),
  [string]$CompareWith,
  [switch]$SkipSources
)

# Read actual installed tooling metadata/XML only; never execute inspected code.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
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
  "$(if ($null -eq $Argument.ArgumentType) { '<untyped>' } else { $Argument.ArgumentType.ToString() }):$value"
}
function Get-Generic($Parameters) {
  (@($Parameters | Where-Object IsGenericParameter | ForEach-Object {
    "$($_.ToString()) : $($_.GenericParameterAttributes) [$((Get-Ordinal @($_.GetGenericParameterConstraints() | ForEach-Object ToString)) -join ',')] $(Get-Attributes $_)"
  })) -join ';'
}
function Get-Parameter($Parameter) {
  if ($null -eq $Parameter) { throw "Null parameter in $($method.DeclaringType.FullName) $($method.Name) inspecting $Path" }
  $default = if ($Parameter.Position -lt 0) { '<return>' } elseif ($Parameter.HasDefaultValue) {
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
        $destination = if ($export.Implementation.Kind -eq [Reflection.Metadata.HandleKind]::AssemblyReference) {
          $reference = $metadata.GetAssemblyReference([Reflection.Metadata.AssemblyReferenceHandle]::op_Explicit($export.Implementation))
          $metadata.GetString($reference.Name)
        } else { $export.Implementation.Kind.ToString() }
        $lines.Add("forward $($metadata.GetString($export.Namespace)).$($metadata.GetString($export.Name)) -> $destination [$($export.Attributes)]")
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

function Get-XmlSurface([string]$Path) {
  # DTDs/resolvers are prohibited; these are SDK inputs, not evaluated projects.
  $settings = [Xml.XmlReaderSettings]::new()
  $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
  $settings.XmlResolver = $null
  $reader = [Xml.XmlReader]::Create($Path, $settings)
  try { $document = [Xml.Linq.XDocument]::Load($reader) } finally { $reader.Dispose() }
  @(Get-Ordinal @($document.Descendants() | ForEach-Object {
    $node = $_
    $ancestors = @($node.Ancestors())
    [Array]::Reverse($ancestors)
    $parents = @($ancestors | ForEach-Object {
      $key = $_.Attribute('Name')
      $_.Name.LocalName + $(if ($key) { "[$($key.Value)]" })
    })
    $attributes = @(Get-Ordinal @($node.Attributes() | ForEach-Object { "$($_.Name)=$($_.Value)" }))
    ($parents + $node.Name.LocalName) -join '/' | ForEach-Object {
      "$_ [$($attributes -join ';')] $(if (-not $node.HasElements) { $node.Value.Trim() })"
    }
  }))
}

Add-Type -Path (Join-Path $Net10Root 'sdk/10.0.402/System.Reflection.MetadataLoadContext.dll')
$baselines = @(
  @{ root = $Net10Root; sdk = '10.0.402'; runtime = '10.0.13'; target = 'net10.0' },
  @{ root = $Net11Root; sdk = '11.0.100-rc.2.26504.105'; runtime = '11.0.0-rc.2.26504.105'; target = 'net11.0' }
)
$results = @()
foreach ($baseline in $baselines) {
  $sdk = Join-Path $baseline.root "sdk/$($baseline.sdk)"
  $referenceRoot = Join-Path $baseline.root "packs/Microsoft.NETCore.App.Ref/$($baseline.runtime)/ref/$($baseline.target)"
  if (-not (Test-Path -LiteralPath $referenceRoot)) { throw 'Missing pinned .NET reference pack.' }
  $allDlls = @(Get-ChildItem -LiteralPath $sdk -Recurse -File -Filter '*.dll' | Where-Object FullName -NotMatch '[\\/]resources[.]dll$')
  $selected = @($allDlls | Where-Object {
    $relative = [IO.Path]::GetRelativePath($sdk, $_.FullName).Replace('\', '/')
    ($relative -notmatch '/' -and $_.Name -match '^(Microsoft\.(Build|TemplateEngine|TestPlatform|VisualStudio\.TestPlatform)|NuGet\.).*\.dll$') -or
    $relative -match '^Sdks/[^/]+/tools/net1[01][.]0/[^/]*(Tasks?|StaticWebAssets)[^/]*\.dll$' -or
    $relative -match '^Sdks/Microsoft.NET.Sdk/tools/net1[01][.]0/Microsoft[.](DotNet[.](ApiCompatibility|ApiSymbolExtensions|PackageValidation)|Deployment[.]DotNet[.]Releases)[.]dll$' -or
    $relative -match '^Containers/tasks/net1[01][.]0/Microsoft\.NET\.Build\.Containers\.dll$'
  } | Sort-Object FullName)
  $surfaces = @()
  foreach ($file in $selected) {
    $references = [Collections.Generic.Dictionary[string,string]]::new([StringComparer]::OrdinalIgnoreCase)
    # Prefer the inspected directory, then SDK root, then reference assemblies.
    foreach ($candidate in @($file) + @(Get-ChildItem -LiteralPath $file.DirectoryName -File -Filter '*.dll') + @(Get-ChildItem -LiteralPath $sdk -File -Filter '*.dll') + @(Get-ChildItem -LiteralPath $referenceRoot -File -Filter '*.dll') + $allDlls) {
      if (-not $references.ContainsKey($candidate.Name)) { $references.Add($candidate.Name, $candidate.FullName) }
    }
    Write-Host "Inspect $($baseline.sdk)/$([IO.Path]::GetRelativePath($sdk, $file.FullName))"
    $surface = Get-Surface $file.FullName @($references.Values)
    # net10/net11 task directory is a host location, not an API identity.
    $surface.path = [IO.Path]::GetRelativePath($sdk, $file.FullName).Replace('\', '/')
    $surface.key = $surface.path -replace '/net1[01][.]0/', '/net-current/'
    $surface.assemblyVersion = [Reflection.AssemblyName]::GetAssemblyName($file.FullName).Version.ToString()
    $surface.productVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($file.FullName).ProductVersion
    $surfaces += $surface
  }
  if (@($surfaces.key | Sort-Object -Unique).Count -ne $surfaces.Count) { throw 'Duplicate tooling identity.' }
  $xml = @(Get-ChildItem -LiteralPath $sdk -Recurse -File | Where-Object {
    $relative = [IO.Path]::GetRelativePath($sdk, $_.FullName).Replace('\', '/')
    $_.Extension -in '.props', '.targets' -and ($relative -notmatch '/' -or $relative -match '^(Sdks/|Current/|Containers/build/)') -and $relative -notmatch '/(FSharp|Roslyn)/'
  } | Sort-Object FullName | ForEach-Object {
    [ordered]@{ path = [IO.Path]::GetRelativePath($sdk, $_.FullName).Replace('\', '/'); sha256 = Get-Hash $_.FullName; declarations = @(Get-XmlSurface $_.FullName) }
  })
  $results += [ordered]@{ sdk = $baseline.sdk; sdkIdentity = @([IO.File]::ReadAllLines((Join-Path $sdk '.version'))); referencePack = $baseline.runtime; target = $baseline.target; assemblies = $surfaces; buildFiles = $xml }
}
$diffs = @()
foreach ($kind in @('assemblies', 'buildFiles')) {
  $identity = if ($kind -eq 'assemblies') { 'key' } else { 'path' }
  foreach ($key in Get-Ordinal @(($results[0][$kind].$identity + $results[1][$kind].$identity) | Select-Object -Unique)) {
    $old = @($results[0][$kind] | Where-Object { $_[$identity] -ceq $key })
    $new = @($results[1][$kind] | Where-Object { $_[$identity] -ceq $key })
    $diffs += [ordered]@{ kind = $kind; asset = $key; diff = Get-Diff $old.declarations $new.declarations }
  }
}
foreach ($baseline in $results) {
  foreach ($surface in @($baseline.assemblies) + @($baseline.buildFiles)) {
    $bytes = [Text.Encoding]::UTF8.GetBytes(($surface.declarations -join "`n") + "`n")
    $surface.declarationCount = $surface.declarations.Count
    $surface.declarationsSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    if ($surface.Contains('key')) { $surface.obsoleteDeclarations = @($surface.declarations | Where-Object { $_.Contains('[System.ObsoleteAttribute(') }) }
    $surface.Remove('declarations')
  }
}
$rulesPath = Join-Path $root 'docs/planning/audits/sdk-build.dispositions.json'
$rules = Get-Content -LiteralPath $rulesPath -Raw | ConvertFrom-Json
$assignments = @()
function Get-WithoutAttributes([string]$Line) {
  # Only identifies paired metadata churn. Original declarations stay in the report.
  (($Line -replace '\[[\w.]+Attribute\(.*?\)\]', '' -replace ',? ?BeforeFieldInit', '') -replace '\s+', ' ').Trim()
}
foreach ($diff in $diffs) {
  foreach ($side in @('added', 'removed')) {
    $otherSide = if ($side -eq 'added') { 'removed' } else { 'added' }
    $other = @($diff.diff[$otherSide] | ForEach-Object { Get-WithoutAttributes $_ })
    foreach ($line in $diff.diff[$side]) {
      $pairedMetadata = $diff.kind -eq 'assemblies' -and
        (Get-WithoutAttributes $line) -cin $other -and
        $line -notmatch 'Requires(Unreferenced|Dynamic)Code|DynamicallyAccessedMembers|System.ObsoleteAttribute'
      if ($pairedMetadata) { $group = 'paired-metadata'; $owner = 'PIN-707: nullable/compiler/initialization metadata; retained, no standalone recipe' }
      else {
        $rule = @($rules.groups | Where-Object { $line -cmatch $_.pattern -and (-not $_.assetPattern -or $diff.asset -cmatch $_.assetPattern) } | Select-Object -First 1)
        if ($rule.Count -ne 1) { throw "Unowned $($diff.asset): $line" }
        $group = $rule[0].id; $owner = $rule[0].owner
      }
      $assignments += [PSCustomObject]@{ asset = $diff.asset; side = $side; group = $group; owner = $owner }
    }
  }
}
$counts = @($assignments | Group-Object asset, side, group | Sort-Object Name | ForEach-Object {
  [ordered]@{ asset = $_.Group[0].asset; side = $_.Group[0].side; group = $_.Group[0].group; owner = $_.Group[0].owner; count = $_.Count }
})
$sourcePath = Join-Path $root 'docs/planning/audits/sdk-build.sources.json'
$report = [ordered]@{ schemaVersion = 1; sourceManifestSha256 = Get-Hash $sourcePath; dispositionManifestSha256 = Get-Hash $rulesPath; baselines = $results; differences = $diffs; dispositions = $counts }
New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
$json = ConvertTo-Json -InputObject $report -Depth 40
[IO.File]::WriteAllText($output, $json.Replace("`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
if ($CompareWith) {
  $expected = Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json -AsHashtable | ConvertTo-Json -Depth 40
  if ($expected -cne $json) { throw "SDK API/build comparison differs from $CompareWith" }
  Write-Host 'PASS: exact installed tooling hashes, public API and authored XML differences reproduce.'
}
if (-not $SkipSources) {
  $sources = Get-Content -LiteralPath $sourcePath -Raw | ConvertFrom-Json
  if ($sources.sources.Count -ne 59 -or @($sources.sources.id | Sort-Object -Unique).Count -ne 59) { throw 'Expected 59 unique pinned sources.' }
  $canonical = @($sources.sources | Where-Object { $_.repository -eq 'dotnet/core' })
  foreach ($component in @(@{ name = 'sdk'; count = 9 }, @{ name = 'containers'; count = 9 }, @{ name = 'msbuild'; count = 7 }, @{ name = 'nuget'; count = 6 }, @{ name = 'README'; count = 9 })) {
    if (@($canonical | Where-Object path -Match "/$($component.name)[.]md$").Count -ne $component.count) { throw "Incomplete canonical $($component.name) boundary." }
  }
  $client = [Net.Http.HttpClient]::new()
  try {
    foreach ($source in $sources.sources) {
      if ($source.commit -notmatch '^[0-9a-f]{40}$' -or $source.url -cne "https://raw.githubusercontent.com/$($source.repository)/$($source.commit)/$($source.path)") { throw "Unpinned source: $($source.id)" }
      $bytes = $client.GetByteArrayAsync($source.url).GetAwaiter().GetResult()
      $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
      if ($hash -cne $source.sha256) { throw "Official source hash mismatch: $($source.id)" }
    }
  } finally { $client.Dispose() }
  Write-Host "PASS: $($sources.sources.Count) immutable official sources reproduce."
}
foreach ($diff in $diffs | Where-Object { $_.diff.added.Count -or $_.diff.removed.Count }) { Write-Host "$($diff.asset): +$($diff.diff.added.Count) -$($diff.diff.removed.Count)" }
Write-Host "PASS: $($results[0].assemblies.Count)/$($results[1].assemblies.Count) tooling assemblies, $($results[0].buildFiles.Count)/$($results[1].buildFiles.Count) build files; $($assignments.Count) API/XML differences have owners."
