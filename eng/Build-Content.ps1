[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$OutputPath,
  [string]$RepositoryRoot = (Split-Path $PSScriptRoot -Parent),
  [string]$RecipeRoot = (Join-Path $RepositoryRoot 'content/recipes')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$schema = Join-Path $PSScriptRoot 'recipe.schema.json'

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
  if (-not (Test-Json -Json $json -SchemaFile $schema)) { throw "Invalid recipe metadata: $($file.FullName)" }
  $recipe = ConvertFrom-Json $json -AsHashtable
  if ($file.Directory.Name -cne $recipe.edition -or $file.Directory.Parent.Name -cne $recipe.id) {
    throw "Recipe path must match its stable ID and edition: $($file.FullName)"
  }
  if (-not $keys.Add("$($recipe.id)/$($recipe.edition)")) { throw 'Duplicate recipe edition.' }
  foreach ($target in $recipe.supportedTargets) {
    if ($target -notin @('10.0', '11.0')) { throw 'Only .NET 10/11 catalogue targets are supported; .NET 9 remains comparison-only.' }
    if ($target -notin $recipe.applicability.routes.target) { throw "Missing applicability route for target $target." }
  }
  foreach ($route in $recipe.applicability.routes) {
    if ($route.target -notin $recipe.supportedTargets) { throw 'Route target must be declared in supportedTargets.' }
    if ([version]$route.target -lt [version]$recipe.introducedIn -and -not $route.conditional) {
      throw 'Target predates introduction; an explicit conditional route is required.'
    }
    foreach ($url in $route.sourceUrls) {
      if ($url -notin $recipe.sources.url) { throw 'Route source must reference a declared official source.' }
    }
    foreach ($sample in $route.evidenceSamples) {
      if (-not ($recipe.evidence | Where-Object { $_.sample -ceq $sample -and $_.target -ceq "net$($route.target)" })) {
        throw 'Route evidence must reference a sample for the exact target.'
      }
    }
  }
  foreach ($source in $recipe.sources) {
    $uri = [uri]$source.url
    if (-not $uri.IsAbsoluteUri -or $uri.Scheme -ne 'https') { throw "Source must be an absolute HTTPS URL: $($source.url)" }
  }
  foreach ($evidence in $recipe.evidence) {
    $projectPath = Resolve-Source $evidence.sample
    [xml]$project = Get-Content -LiteralPath $projectPath -Raw
    $pin = Get-Content -LiteralPath (Join-Path (Split-Path $projectPath) 'global.json') -Raw | ConvertFrom-Json
    if ($project.Project.PropertyGroup.TargetFramework -cne $evidence.target -or
        $project.Project.PropertyGroup.LangVersion -cne $evidence.language -or
        $pin.sdk.version -cne $evidence.sdk) { throw "Evidence baseline disagrees with sample: $($evidence.sample)" }
  }
  $markdown = Get-Content -LiteralPath (Join-Path $file.Directory.FullName 'recipe.md') -Raw
  foreach ($key in $recipe.snippets.Keys) {
    $snippet = $recipe.snippets[$key]
    $source = Get-Content -LiteralPath (Resolve-Source $snippet.path) -Raw
    $region = [regex]::Escape($snippet.region)
    $matches = [regex]::Matches($source, "(?ms)^// snippet:$region\r?\n(.*?)^// endsnippet:$region\r?$")
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
  foreach ($section in @('Goal', 'Older approach', 'Newer approach', 'Migration', 'Constraints', 'Evidence')) {
    if ($markdown -notmatch "(?m)^## $section") { throw "Missing narrative section '$section'." }
  }
  $recipe.Remove('snippets')
  $recipe.markdown = $markdown.Replace("`r`n", "`n")
  $editions.Add($recipe)
}
if ($editions.Count -eq 0) { throw 'The catalogue must contain at least one edition.' }
$output = [IO.Path]::GetFullPath($OutputPath)
[IO.Directory]::CreateDirectory((Split-Path $output)) | Out-Null
$generated = ConvertTo-Json -InputObject $editions.ToArray() -Depth 30
if (-not (Test-Path -LiteralPath $output) -or (Get-Content -LiteralPath $output -Raw) -cne $generated) {
  [IO.File]::WriteAllText($output, $generated, [Text.UTF8Encoding]::new($false))
}
