[CmdletBinding()]
param(
  [string]$Net10Root = 'C:/Program Files/dotnet',
  [string]$Net11Root = 'C:/Users/pingu/.dotnet-playbook-11',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/runtime-security.assets.json'),
  [string]$CompareWith,
  [string]$SourceManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/runtime-security.sources.json'),
  [string]$DispositionManifest = (Join-Path $PSScriptRoot '../docs/planning/audits/runtime-security.dispositions.json')
)

# Metadata only: no inspected code, restore, SDK first-run setup or PATH changes.
$ErrorActionPreference = 'Stop'
# Resolve relative output against PowerShell's location. Set-Location does not
# necessarily change the process working directory used by IO.Path.GetFullPath.
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
if ($CompareWith -and $output -eq $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($CompareWith)) {
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


function Test-InScope([string]$Path, [string]$Name) {
  if ([IO.Path]::GetFileName($Path) -notin @('System.Runtime.dll', 'System.Runtime.Extensions.dll', 'System.Runtime.Handles.dll')) { return $true }
  return $Name -match '^(System\.(Runtime\.|Reflection\.|Threading\.|GC($|\+)|GCCollectionMode$|GCKind$|GCMemoryInfo$|GCNotificationStatus$|GCSettings$|GCLatencyMode$|GCLargeObjectHeapCompactionMode$|Environment($|\+)|AppContext$|AppDomain($|\+)|OperatingSystem$)|Microsoft\.Win32\.SafeHandles\.)' -and $Name -notmatch '(SafeFileHandle|SafePipeHandle|SafeProcessHandle|UnixProcessStartArguments|WindowsProcessStartArguments|SequenceMarshal)'
}
$temporary = Join-Path ([IO.Path]::GetTempPath()) "playbook-runtime-security-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $temporary | Out-Null
try {
  Add-Type -Path (Join-Path $Net10Root 'sdk/10.0.402/System.Reflection.MetadataLoadContext.dll')
  $baselines = @(
    @{ root = $Net10Root; sdk = '10.0.402'; version = '10.0.13'; target = 'net10.0' },
    @{ root = $Net11Root; sdk = '11.0.100-rc.2.26504.105'; version = '11.0.0-rc.2.26504.105'; target = 'net11.0' }
  )
  $results = @()
  foreach ($baseline in $baselines) {
    $refDirectory = Join-Path $baseline.root "packs/Microsoft.NETCore.App.Ref/$($baseline.version)/ref/$($baseline.target)"
    $references = @(Get-ChildItem -LiteralPath $refDirectory -Filter '*.dll' | ForEach-Object FullName)
    if ($references.Count -eq 0) { throw "Missing reference pack: $refDirectory" }
    $selected = @($references | Where-Object { [IO.Path]::GetFileName($_) -match '^System\.(Net\..*|Security.*|Threading.*|Reflection.*|Runtime|Runtime\.Extensions|Runtime\.Handles|Runtime\.InteropServices.*|Runtime\.CompilerServices.*|Runtime\.Loader)\.dll$' })
    Write-Host "Inspecting $($baseline.target): $($selected.Count) reference assemblies and selected security/transport packages."
    $framework = @($selected | Sort-Object | ForEach-Object { Get-Surface $_ $references })
    $tooling = @(Get-ChildItem (Join-Path $baseline.root "packs/Microsoft.NETCore.App.Ref/$($baseline.version)/analyzers") -Recurse -Filter 'Microsoft.Interop.*.dll' | Sort-Object FullName | ForEach-Object {
      [ordered]@{ path = [IO.Path]::GetRelativePath($baseline.root, $_.FullName).Replace('\', '/'); sha256 = Get-Hash $_.FullName }
    })
    if ($tooling.Count -eq 0) { throw 'Missing framework analyzer/tooling artifacts.' }
    $tooling += [ordered]@{ path = "sdk/$($baseline.sdk)/System.Reflection.MetadataLoadContext.dll"; sha256 = Get-Hash (Join-Path $baseline.root "sdk/$($baseline.sdk)/System.Reflection.MetadataLoadContext.dll") }
    $packages = @()
    foreach ($id in @('microsoft.bcl.cryptography', 'system.security.cryptography.pkcs', 'system.security.cryptography.xml', 'system.security.cryptography.protecteddata', 'system.net.http.winhttphandler', 'system.threading.accesscontrol')) {
      $url = "https://api.nuget.org/v3-flatcontainer/$id/$($baseline.version)/$id.$($baseline.version).nupkg"
      $zip = Join-Path $temporary "$id.$($baseline.version).nupkg"
      Invoke-WebRequest -Uri $url -OutFile $zip
      $directory = "$zip.contents"
      [IO.Compression.ZipFile]::ExtractToDirectory($zip, $directory)
      [xml]$nuspec = Get-Content -LiteralPath (Get-ChildItem $directory -Filter '*.nuspec').FullName -Raw
      $assetReferences = $references
      $assets = @()
      $otherAssets = @()
      foreach ($asset in Get-ChildItem (Join-Path $directory 'lib') -Recurse -Filter '*.dll' | Sort-Object FullName) {
        $relative = [IO.Path]::GetRelativePath($directory, $asset.FullName).Replace('\', '/')
        if ($relative -notmatch '^lib/net(8|9|10|11)\.0/') {
          $otherAssets += [ordered]@{ path = $relative; sha256 = Get-Hash $asset.FullName }
          continue
        }
        $dependencyReferences = @($packages | ForEach-Object {
          $dependency = $_
          @($dependency.assets | Where-Object { [int]([regex]::Match($_.path, 'net(\d+)').Groups[1].Value) -le [int]([regex]::Match($relative, 'net(\d+)').Groups[1].Value) } | Sort-Object { [int]([regex]::Match($_.path, 'net(\d+)').Groups[1].Value) } -Descending)[0] | ForEach-Object { Join-Path "$temporary/$($dependency.id).$($dependency.version).nupkg.contents" $_.path }
        })
        $surface = Get-Surface $asset.FullName @($dependencyReferences + $assetReferences)
        $assets += [ordered]@{ path = $relative; surface = $surface }
      }
      if ($assets.Count -eq 0) { throw "Missing reviewed modern target assets: $id" }
      $packages += [ordered]@{ id = $id; version = $baseline.version; url = $url; sha256 = Get-Hash $zip; repository = $nuspec.package.metadata.repository.url; commit = $nuspec.package.metadata.repository.commit; assets = $assets; otherAssets = $otherAssets }
    }
    $browser = @()
    $browserPackages = @('microsoft.netcore.app.runtime.mono.browser-wasm')
    if ($baseline.target -eq 'net11.0') { $browserPackages += 'microsoft.netcore.app.runtime.browser-wasm' }
    foreach ($id in $browserPackages) {
      $url = "https://api.nuget.org/v3-flatcontainer/$id/$($baseline.version)/$id.$($baseline.version).nupkg"
      $zip = Join-Path $temporary "$id.$($baseline.version).nupkg"
      Invoke-WebRequest -Uri $url -OutFile $zip
      $directory = "$zip.contents"
      [IO.Compression.ZipFile]::ExtractToDirectory($zip, $directory)
      $declarations = @(Get-ChildItem -LiteralPath $directory -Recurse -Filter 'dotnet.d.ts')
      if ($declarations.Count -ne 1) { throw "Missing/ambiguous browser public tooling: $id" }
      $browser += [ordered]@{
        id = $id; version = $baseline.version; url = $url; sha256 = Get-Hash $zip
        path = [IO.Path]::GetRelativePath($directory, $declarations[0].FullName).Replace('\', '/')
        declarationsSha256 = Get-Hash $declarations[0].FullName
        declarations = @(Get-Content -LiteralPath $declarations[0].FullName)
      }
    }
    $results += [ordered]@{ sdk = $baseline.sdk; referencePack = $baseline.version; target = $baseline.target; tooling = $tooling; browser = $browser; framework = $framework; packages = $packages }
  }
  $diffs = @()
  foreach ($name in Get-Ordinal @($results.framework.name | Select-Object -Unique)) {
    $old = @($results[0].framework | Where-Object name -CEQ $name)
    $new = @($results[1].framework | Where-Object name -CEQ $name)
    $diffs += [ordered]@{ asset = "framework/$name"; diff = Get-Diff $old.declarations $new.declarations }
  }
  foreach ($package in $results[0].packages) {
    $newPackage = $results[1].packages | Where-Object id -EQ $package.id
    # Compare the nearest previous modern asset to every current modern TFM.
    $old = @($package.assets | Sort-Object { [int]([regex]::Match($_.path, 'net(\d+)').Groups[1].Value) } -Descending)[0]
    foreach ($new in $newPackage.assets) {
      $diffs += [ordered]@{ asset = "$($package.id)/$($old.path)->$($new.path)"; diff = Get-Diff $old.surface.declarations $new.surface.declarations }
    }
  }
  $publicToolingDiff = @()
  foreach ($browser in $results[1].browser) {
    # CoreCLR is new in this release; compare both current public contracts with
    # the actual prior Mono contract rather than inventing a .NET 10 CoreCLR pack.
    $publicToolingDiff += [ordered]@{
      asset = $browser.id
      diff = Get-Diff $results[0].browser[0].declarations $browser.declarations
      owner = 'R14'
    }
  }
  foreach ($baseline in $results) { foreach ($browser in $baseline.browser) { $browser.Remove('declarations') } }
  # Save whole-surface fingerprints and complete deltas, without repeating unchanged APIs.
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
        $rule = @($rules.groups | Where-Object { $line -cmatch $_.pattern } | Select-Object -First 1)
        if ($rule.Count -ne 1) { throw "Unowned API difference in $($diff.asset): $line" }
        $assignments += [PSCustomObject]@{ asset = $diff.asset; side = $side; group = $rule[0].id }
      }
    }
  }
  $counts = @($assignments | Group-Object asset, side, group | Sort-Object Name | ForEach-Object {
    [ordered]@{ asset = $_.Group[0].asset; side = $_.Group[0].side; group = $_.Group[0].group; count = $_.Count }
  })
  foreach ($diff in $diffs) {
    foreach ($side in @('added', 'removed')) {
      $assigned = ($counts | Where-Object { $_.asset -ceq $diff.asset -and $_.side -ceq $side } | ForEach-Object { $_.count } | Measure-Object -Sum).Sum
      if ([int]$assigned -ne $diff.diff[$side].Count) { throw "Incomplete disposition counts: $($diff.asset) $side" }
    }
  }
  $report = [ordered]@{ schemaVersion = 1; sourceManifestSha256 = Get-Hash $SourceManifest; dispositionManifestSha256 = Get-Hash $DispositionManifest; baselines = $results; publicApiDiff = $diffs; publicToolingDiff = $publicToolingDiff; dispositions = $counts }
  New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
  $json = ConvertTo-Json -InputObject $report -Depth 40
  [IO.File]::WriteAllText($output, $json.Replace("`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
  if ($CompareWith) {
    $expected = Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json -AsHashtable | ConvertTo-Json -Depth 40
    if ($expected -cne $json) { throw "Runtime/security assets/API comparison differs from $CompareWith" }
    Write-Host 'PASS: exact package, tooling and reference hashes plus all public declarations/diffs match.'
  }
  $sources = Get-Content -LiteralPath $SourceManifest -Raw | ConvertFrom-Json
  if ($sources.sources.Count -eq 0) { throw 'Empty source manifest.' }
  Write-Host "Verifying $($sources.sources.Count) immutable source hashes."
  foreach ($source in $sources.sources) {
    $download = Join-Path $temporary 'source'
    Invoke-WebRequest -Uri $source.url -OutFile $download
    if ((Get-Hash $download) -cne $source.sha256) { throw "Official source hash mismatch: $($source.url)" }
  }
  Write-Host "PASS: $($sources.sources.Count) immutable official source hashes match."
  foreach ($diff in $diffs) { Write-Host "$($diff.asset): +$($diff.diff.added.Count) -$($diff.diff.removed.Count)" }
} finally {
  # The generated absolute path is always a unique child of the system temp directory.
  $resolved = [IO.Path]::GetFullPath($temporary)
  $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
  if (-not $resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe temporary path.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
