# Shared by authoring-time capture and build-time validation. No SDK is invoked here.
function Resolve-EvidencePath([string]$Root, [string]$RelativePath) {
  $path = [IO.Path]::GetFullPath((Join-Path $Root $RelativePath))
  if (-not $path.StartsWith($Root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Evidence input escapes repository: $RelativePath"
  }
  if (-not (Test-Path -LiteralPath $path)) { throw "Missing evidence input: $RelativePath" }
  return $path
}

function Get-EvidenceInputs([string]$Root, [string]$Sample) {
  $projectPath = Resolve-EvidencePath $Root $Sample
  $directory = Split-Path $projectPath
  $manifestJson = Get-Content -LiteralPath (Join-Path $directory 'verification.json') -Raw
  if (-not (Test-Json -Json $manifestJson -SchemaFile (Join-Path $PSScriptRoot 'verification.schema.json') -ErrorAction SilentlyContinue)) {
    throw "Invalid verification inputs: $Sample"
  }
  $manifest = $manifestJson | ConvertFrom-Json -AsHashtable
  if ($manifest.dependencies.Count -gt 0 -and -not (Test-Path -LiteralPath (Join-Path $directory 'packages.lock.json'))) {
    throw "Dependency samples require a committed packages.lock.json: $Sample"
  }
  $paths = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
  $sources = @($directory) + @($manifest.additionalInputs | ForEach-Object { Resolve-EvidencePath $Root $_ })
  foreach ($source in $sources) {
    foreach ($file in (Get-ChildItem -LiteralPath $source -File -Recurse -Force)) {
      $relative = [IO.Path]::GetRelativePath($Root, $file.FullName).Replace('\', '/')
      if ($relative -match '(^|/)(bin|obj)/' -or $file.Name -eq 'verification-evidence.json' -or $file.Name -like '*.evidence.json') { continue }
      $null = $paths.Add($relative)
    }
  }
  # Track the nearest effective conventional config, including newly added overrides.
  foreach ($name in @('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'NuGet.Config', 'global.json')) {
    $ancestor = $directory
    while ($ancestor.StartsWith($Root, [StringComparison]::OrdinalIgnoreCase)) {
      $candidate = Join-Path $ancestor $name
      if (Test-Path -LiteralPath $candidate -PathType Leaf) {
        $null = $paths.Add([IO.Path]::GetRelativePath($Root, $candidate).Replace('\', '/'))
        break
      }
      if ($ancestor -eq $Root) { break }
      $ancestor = Split-Path $ancestor
    }
  }
  # Explicit linked sources/imports/project references cannot silently escape the manifest.
  # File-based applications have no XML project. Their includes/references must be
  # covered by the directory inventory or explicit additionalInputs, like imports.
  $items = @()
  if ([IO.Path]::GetExtension($projectPath) -ne '.cs') {
    [xml]$project = Get-Content -LiteralPath $projectPath -Raw
    $items = @($project.SelectNodes('//*[@Include or @Project]'))
  }
  foreach ($item in $items) {
    $include = if ($item.HasAttribute('Include')) { $item.GetAttribute('Include') } else { $item.GetAttribute('Project') }
    if ($item.Name -notin @('Compile', 'Content', 'None', 'EmbeddedResource', 'ProjectReference', 'Import')) { continue }
    if ($include -match '[*$;]') { throw "Declare evaluated inputs explicitly; unsupported dynamic include in $Sample : $include" }
    $included = [IO.Path]::GetFullPath((Join-Path $directory $include))
    $relative = [IO.Path]::GetRelativePath($Root, $included).Replace('\', '/')
    if (-not $paths.Contains($relative)) { throw "Declare linked input in verification.json additionalInputs: $relative" }
  }
  $inputs = [ordered]@{}
  foreach ($path in ($paths | Sort-Object -CaseSensitive)) {
    $inputs[$path] = (Get-FileHash -LiteralPath (Resolve-EvidencePath $Root $path) -Algorithm SHA256).Hash.ToLowerInvariant()
  }
  return $inputs
}

function Read-VerificationEvidence([string]$Root, $Expected) {
  $samplePath = Resolve-EvidencePath $Root $Expected.sample
  $recordPath = Join-Path (Split-Path $samplePath) 'verification-evidence.json'
  if ($Expected.ContainsKey('record')) { $recordPath = Resolve-EvidencePath $Root $Expected.record }
  if (-not (Test-Path -LiteralPath $recordPath -PathType Leaf)) { throw "Missing compilation evidence: $($Expected.sample). Run eng/Record-Evidence.ps1 on a suitable authoring machine." }
  $json = Get-Content -LiteralPath $recordPath -Raw
  if (-not (Test-Json -Json $json -SchemaFile (Join-Path $PSScriptRoot 'evidence.schema.json') -ErrorAction SilentlyContinue)) {
    throw "Invalid verification evidence: $recordPath"
  }
  $record = $json | ConvertFrom-Json -AsHashtable
  if ($Expected.kind -in @('compile', 'cross-compile', 'compile-and-run') -and $record.kind -notin @('compile', 'cross-compile', 'compile-and-run')) {
    throw "Missing compilation evidence: $($Expected.sample). $($record.kind) is not compilation."
  }
  foreach ($field in @('sample', 'target', 'sdk', 'language', 'kind')) {
    if ($record[$field] -cne $Expected[$field]) { throw "Stale compilation evidence: $($Expected.sample) changed $field." }
  }
  $inputs = Get-EvidenceInputs $Root $Expected.sample
  if ($inputs.Count -ne $record.inputs.Count) { throw "Stale compilation evidence: $($Expected.sample) input set changed." }
  foreach ($path in $inputs.Keys) {
    if ($inputs[$path] -cne $record.inputs[$path]) { throw "Stale compilation evidence: $($Expected.sample) changed $path. Refresh on a suitable authoring machine." }
  }
  $manifest = Get-Content (Join-Path (Split-Path $samplePath) 'verification.json') -Raw | ConvertFrom-Json -AsHashtable
  foreach ($field in @('dependencies', 'workloads', 'nativeToolchains')) {
    if ((ConvertTo-Json -InputObject @($record[$field]) -Compress) -cne (ConvertTo-Json -InputObject @($manifest[$field]) -Compress)) {
      throw "Stale compilation evidence: $($Expected.sample) changed $field."
    }
  }
  $record.scope = $Expected.scope
  $record.freshness = 'current'
  return $record
}
