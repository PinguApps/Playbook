[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$OutputPath,
  [string]$RepositoryRoot = (Split-Path $PSScriptRoot -Parent),
  [string]$RecipeRoot = (Join-Path $RepositoryRoot 'content/recipes'),
  [string]$VerifyAgainst,
  [switch]$ReleaseReady
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$schema = Join-Path $PSScriptRoot 'recipe.schema.json'
. (Join-Path $PSScriptRoot 'Evidence.ps1')

function Resolve-Source([string]$RelativePath) {
  $path = [IO.Path]::GetFullPath((Join-Path $root $RelativePath))
  if (-not $path.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Source escapes repository: $RelativePath"
  }
  if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing source: $RelativePath" }
  return $path
}

$editions = [Collections.Generic.List[object]]::new()
$keys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($file in (Get-ChildItem $RecipeRoot -Filter recipe.json -Recurse | Sort-Object FullName)) {
  $json = Get-Content -LiteralPath $file.FullName -Raw
  if (-not (Test-Json -Json $json -SchemaFile $schema -ErrorAction SilentlyContinue)) { throw "Invalid recipe metadata: $($file.FullName)" }
  $recipe = ConvertFrom-Json $json -AsHashtable
  if ($ReleaseReady -and $recipe.humanProof.status -ceq 'pending') {
    throw "Pending Human proof blocks release readiness: $($recipe.id)/$($recipe.edition): $($recipe.humanProof.scope)"
  }
  if (-not $recipe.ContainsKey('languageNeutral')) { $recipe.languageNeutral = $false }
  if ($recipe.languageNeutral) {
    if ($recipe.exampleLanguages.Count -ne 0 -or
        @($recipe.snippets.Values | Where-Object { $_.language -cnotin @('xml', 'json', 'jsonc', 'yaml', 'toml', 'ini') }).Count -ne 0) {
      throw 'Language-neutral recipes require configuration snippets and no demonstrated programming language; language-specific directives/settings are not neutral.'
    }
  }
  elseif ($recipe.exampleLanguages.Count -eq 0) { throw 'Programming recipes require demonstrated exampleLanguages.' }
  if ($file.Directory.Name -cne $recipe.edition -or $file.Directory.Parent.Name -cne $recipe.id) {
    throw "Recipe path must match its stable ID and edition: $($file.FullName)"
  }
  if (-not $keys.Add("$($recipe.id)/$($recipe.edition)")) { throw 'Duplicate recipe edition.' }
  if ([version]$recipe.edition -lt [version]$recipe.introducedIn) { throw 'Edition predates introduction.' }
  foreach ($target in $recipe.supportedTargets) {
    if ($target -notin @('10.0', '11.0')) { throw 'Only .NET 10/11 catalogue targets are supported; .NET 9 remains comparison-only.' }
    if ($target -cnotin $recipe.applicability.routes.target) { throw "Missing applicability route for target $target." }
  }
  foreach ($route in $recipe.applicability.routes) {
    $requirements = $route.requirements
    foreach ($requirement in @($requirements.runtime, $requirements.sdk, $requirements.language) + @($requirements.packages) + @($requirements.workloads)) {
      if ($requirement.match -ceq 'release-family' -and $requirement.version -cnotmatch '^\d+\.\d+$') {
        throw 'Release-family requirements use major.minor only; exact test pins belong in evidence.'
      }
    }
    foreach ($field in @('sharedAreas', 'neutralLanguages')) {
      if (-not $route.ContainsKey($field)) { $route[$field] = @() }
    }
    if ($recipe.languageNeutral -ne ($route.neutralLanguages.Count -gt 0)) {
      throw 'Only language-neutral recipes require explicit neutralLanguages on every route.'
    }
    if ($route.target -cnotin $recipe.supportedTargets) { throw 'Route target must be declared in supportedTargets.' }
    if ([version]$route.target -lt [version]$recipe.introducedIn -and -not $route.conditional) {
      throw 'Target predates introduction; an explicit conditional route is required.'
    }
    foreach ($url in $route.sourceUrls) {
      if ($url -cnotin $recipe.sources.url) { throw 'Route source must reference a declared official source.' }
    }
    foreach ($sample in $route.evidenceSamples) {
      $baseline = @($recipe.evidence | Where-Object { $_.sample -ceq $sample -and $_.target -cmatch "^net$([regex]::Escape($route.target))(-|$)" })
      if ($baseline.Count -eq 0) {
        throw 'Route evidence must reference a sample for the exact target.'
      }
      foreach ($field in @('sdk', 'language')) {
        $requirement = $requirements[$field]
        foreach ($evidence in $baseline) {
          $matches = if ($requirement.match -ceq 'exact') { $evidence[$field] -ceq $requirement.version }
            else { $evidence[$field].StartsWith($requirement.version + '.', [StringComparison]::Ordinal) -or $evidence[$field] -ceq $requirement.version }
          if (-not $matches) { throw "Route $field requirement contradicts its verification baseline: $sample." }
        }
      }
    }
    foreach ($field in @('optIns', 'stabilityLimitations')) {
      if (-not $requirements.ContainsKey($field)) { $requirements[$field] = @() }
    }
    if ($recipe.stability -ceq 'experimental' -and ($requirements.optIns.Count -eq 0 -or $requirements.stabilityLimitations.Count -eq 0)) {
      throw 'Experimental routes require explicit optIns and stabilityLimitations (including versioned package requirements where applicable).'
    }
  }
  $sourceUrls = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
  foreach ($source in $recipe.sources) {
    $uri = [uri]$source.url
    if (-not $uri.IsAbsoluteUri -or $uri.Scheme -ne 'https') { throw "Source must be an absolute HTTPS URL: $($source.url)" }
    if (-not $sourceUrls.Add($source.url)) { throw "Duplicate source URL: $($source.url)" }
  }
  $verified = [Collections.Generic.List[object]]::new()
  foreach ($evidence in $recipe.evidence) {
    $projectPath = Resolve-Source $evidence.sample
    if ([IO.Path]::GetExtension($projectPath) -eq '.cs') {
      $fileSource = Get-Content -LiteralPath $projectPath -Raw
      $targetDirective = [regex]::Match($fileSource, '(?m)^#:property TargetFramework=(\S+)\r?$')
      $languageDirective = [regex]::Match($fileSource, '(?m)^#:property LangVersion=(\S+)\r?$')
      if (-not $targetDirective.Success -or -not $languageDirective.Success) {
        throw 'Evidence file applications must declare TargetFramework and LangVersion explicitly.'
      }
      $sampleTarget = $targetDirective.Groups[1].Value
      $sampleLanguage = $languageDirective.Groups[1].Value
    }
    else {
      [xml]$project = Get-Content -LiteralPath $projectPath -Raw
      $sampleTarget = $project.Project.PropertyGroup.TargetFramework
      $sampleLanguage = $project.Project.PropertyGroup.LangVersion
    }
    $pin = Get-Content -LiteralPath (Join-Path (Split-Path $projectPath) 'global.json') -Raw | ConvertFrom-Json
    if ($sampleTarget -cne $evidence.target -or
        $sampleLanguage -cne $evidence.language -or
        $pin.sdk.version -cne $evidence.sdk) { throw "Evidence baseline disagrees with sample: $($evidence.sample)" }
    $verified.Add((Read-VerificationEvidence $root $evidence))
  }
  foreach ($sample in ($recipe.evidence.sample | Select-Object -Unique)) {
    if (-not ($verified | Where-Object { $_.sample -ceq $sample -and $_.kind -in @('compile', 'cross-compile', 'compile-and-run') })) {
      $sampleRecords = @($verified | Where-Object { $_.sample -ceq $sample })
      # Explicitly theoretical editions are reviewable in development builds only.
      # They do not acquire compilation proof; ReleaseReady still rejects pending proof.
      if ($recipe.humanProof.status -cne 'pending' -or
          @($sampleRecords | Where-Object { $_.kind -cne 'official-review' }).Count -gt 0) {
        throw "Missing compilation evidence: $sample. Source review, mocks and execution alone cannot satisfy compilation."
      }
      $warning = 'Theoretical guidance: source review only; samples have not been compiled or executed. Pending proof blocks release-ready packaging.'
      foreach ($route in $recipe.applicability.routes) {
        if ($warning -cnotin $route.selectionNotes) { $route.selectionNotes += $warning }
        if ($warning -cnotin $route.requirements.stabilityLimitations) { $route.requirements.stabilityLimitations += $warning }
      }
    }
  }
  $recipe.evidence = $verified.ToArray()
  foreach ($evidence in $recipe.evidence) {
    $evidence.role = if ($evidence.kind -ceq 'official-review') { 'source-review' } else { 'verification-baseline' }
  }
  $markdown = Get-Content -LiteralPath (Join-Path $file.Directory.FullName 'recipe.md') -Raw
  if ($markdown -match '(?m)^\s*(`{3,}|~{3,})') { throw 'Canonical snippet references are required instead of copied fenced implementations.' }
  foreach ($section in @('Goal', 'Older approach', 'Newer approach', 'Migration', 'Constraints', 'Evidence')) {
    $heading = [regex]::Escape($section)
    $blocks = [regex]::Matches($markdown, "(?ms)^## $heading(?:[ :][^\r\n]*)?\r?\n(.*?)(?=^## |\z)")
    if ($blocks.Count -ne 1 -or [string]::IsNullOrWhiteSpace($blocks[0].Groups[1].Value)) { throw "Expected one nonempty narrative section '$section'." }
    $body = $blocks[0].Groups[1].Value
    $offlineText = [regex]::Replace($body, '(?s)<!--.*?-->|\[[^\]]*\]\([^)]*\)|https?://\S+', '')
    if ($section -eq 'Migration' -and $offlineText -notmatch '(?m)^\d+\.[ \t]+\S') { throw 'Migration requires offline implementation steps, not just a reference link.' }
    if ([string]::IsNullOrWhiteSpace($offlineText)) { throw "Narrative section '$section' must include offline content." }
    if ($section -eq 'Newer approach' -and $body -notmatch '\{\{snippet:[a-z][a-z0-9-]*\}\}') { throw 'Newer approach requires a canonical snippet.' }
  }
  foreach ($key in $recipe.snippets.Keys) {
    $snippet = $recipe.snippets[$key]
    $snippetPath = Resolve-Source $snippet.path
    $sampleDirectories = @($recipe.evidence | ForEach-Object { (Split-Path (Resolve-Source $_.sample)) + [IO.Path]::DirectorySeparatorChar })
    if (-not ($sampleDirectories | Where-Object { $snippetPath.StartsWith($_, [StringComparison]::OrdinalIgnoreCase) })) { throw "Snippet '$key' must belong to a declared evidence sample." }
    $source = Get-Content -LiteralPath $snippetPath -Raw
    $region = [regex]::Escape($snippet.region)
    $matches = [regex]::Matches($source, "(?ms)^[ \t]*(?://|#|/\*|@\*|<!--) snippet:$region(?: -->| \*/| \*@)?\r?\n(.*?)^[ \t]*(?://|#|/\*|@\*|<!--) endsnippet:$region(?: -->| \*/| \*@)?\r?$")
    if ($matches.Count -ne 1 -or [string]::IsNullOrWhiteSpace($matches[0].Groups[1].Value)) {
      throw "Expected one nonempty snippet '$key' in $($snippet.path)."
    }
    $token = "{{snippet:$key}}"
    if (-not $markdown.Contains($token)) { throw "Unused snippet '$key'." }
    $fence = '```'
    $replacement = "$fence$($snippet.language)`n$($matches[0].Groups[1].Value.TrimEnd())`n$fence"
    $markdown = $markdown.Replace($token, $replacement)
  }
  if ($markdown -match '\{\{snippet:') { throw "Unresolved snippet in $($recipe.id)." }
  $recipe.Remove('snippets')
  $recipe.markdown = $markdown.Replace("`r`n", "`n")
  $editions.Add($recipe)
}
if ($editions.Count -eq 0) { throw 'The catalogue must contain at least one edition.' }
$output = [IO.Path]::GetFullPath($OutputPath)
[IO.Directory]::CreateDirectory((Split-Path $output)) | Out-Null
$generated = ConvertTo-Json -InputObject $editions.ToArray() -Depth 30
if ($VerifyAgainst) {
  if (-not (Test-Path -LiteralPath $VerifyAgainst) -or (Get-Content -LiteralPath $VerifyAgainst -Raw) -cne $generated) {
    throw 'Recipe content differs from the compiled catalogue. Rebuild before packing; --no-build cannot package stale guidance.'
  }
}
if (-not (Test-Path -LiteralPath $output) -or (Get-Content -LiteralPath $output -Raw) -cne $generated) {
  [IO.File]::WriteAllText($output, $generated, [Text.UTF8Encoding]::new($false))
}
