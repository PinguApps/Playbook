[CmdletBinding()]
param([switch]$Pack)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$root = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path ([IO.Path]::GetTempPath()) "playbook-content-$([guid]::NewGuid().ToString('N'))"
$original = Get-Content (Join-Path $root 'content/recipes/csharp.validate-property-assignment/10.0/recipe.json') -Raw
$narrative = Get-Content (Join-Path $root 'content/recipes/csharp.validate-property-assignment/10.0/recipe.md') -Raw

try {
  if ($Pack) {
    $recipes = Join-Path $fixture 'recipes'
    $edition = Join-Path $recipes 'csharp.validate-property-assignment/10.0'
    New-Item -ItemType Directory -Path $edition -Force | Out-Null
    $original | Set-Content (Join-Path $edition 'recipe.json')
    # Prove the real --no-build pack path still validates narrative content.
    [regex]::Replace($narrative, '(?ms)(## Migration\r?\n).*?(?=^## )', '$1' + "`n[Read the implementation](https://example.invalid/steps)`n`n") |
      Set-Content (Join-Path $edition 'recipe.md') -NoNewline
    $packArgs = @('pack', (Join-Path $root 'src/PinguApps.Playbook'), '-c', 'Release', '--no-build', '-p:PackageVersion=0.0.0-content-fixture', "-p:RecipeRoot=$recipes", '--output', (Join-Path $fixture 'packages'))
    $result = & dotnet @packArgs 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or $result -notlike '*Migration requires offline implementation steps*') { throw "Pack accepted incomplete content: $result" }
    Write-Host 'Pack --no-build rejected incomplete content.'
    $narrative | Set-Content (Join-Path $edition 'recipe.md') -NoNewline
    & (Join-Path $PSScriptRoot 'Build-Content.ps1') -RecipeRoot $recipes -OutputPath (Join-Path $fixture 'catalogue.json') `
      -VerifyAgainst (Join-Path $root 'src/PinguApps.Playbook/obj/Release/net10.0/Generated/compiled-catalogue.json')
    $recipe = ConvertFrom-Json $original -AsHashtable
    $recipe.goal += ' Changed since compilation.'
    $recipe | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $edition 'recipe.json')
    $result = & dotnet @packArgs 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or $result -notlike '*differs from the compiled catalogue*') { throw "Pack accepted stale compiled content: $result" }
    if (Get-ChildItem (Join-Path $fixture 'packages') -Filter *.nupkg -ErrorAction SilentlyContinue) { throw 'Rejected content produced a package.' }
    Write-Host 'Pack --no-build rejected valid content changed since compilation; no package produced.'
    return
  }
  $cases = @(
    @{ Name = 'invalid identity'; Change = { $recipe.id = 'csharp..invalid' }; Error = 'Invalid recipe metadata' },
    @{ Name = 'duplicate edition'; Change = { $duplicate = $true }; Error = 'Duplicate recipe edition' },
    @{ Name = 'incompatible edition'; Change = { $recipe.edition = '9.0' }; Error = 'Edition predates introduction' },
    @{ Name = 'missing route'; Change = { $recipe.supportedTargets += '11.0' }; Error = 'Missing applicability route' },
    @{ Name = 'unverified conditional route'; Change = { $recipe.introducedIn = '11.0'; $recipe.edition = '11.0' }; Error = 'explicit conditional route' },
    @{ Name = 'missing source reference'; Change = { $recipe.applicability.routes[0].sourceUrls = @('https://example.invalid/missing') }; Error = 'Route source' },
    @{ Name = 'missing experimental disclosure'; Change = { $recipe.stability = 'experimental' }; Error = 'Experimental routes require' },
    @{ Name = 'missing sample'; Change = { $recipe.evidence[0].sample = 'samples/missing.csproj' }; Error = 'Missing source' },
    @{ Name = 'missing canonical snippet'; Change = { $recipe.snippets.newer.region = 'missing' }; Error = 'Expected one nonempty snippet' },
    @{ Name = 'copied implementation'; Change = { $markdown = $markdown.Replace('{{snippet:newer}}', '```csharp' + "`n// copied code`n" + '```'); $recipe.snippets.Remove('newer') }; Error = 'Canonical snippet references' },
    @{ Name = 'external-only migration'; Change = { $markdown = [regex]::Replace($markdown, '(?ms)(## Migration\r?\n).*?(?=^## )', '$1' + "`n1. [Read the implementation](https://example.invalid/steps)`n`n") }; Error = 'Migration requires offline implementation steps' }
  )
  foreach ($case in $cases) {
    $recipe = ConvertFrom-Json $original -AsHashtable
    $markdown = $narrative
    $duplicate = $false
    . $case.Change
    $recipes = Join-Path $fixture $case.Name
    $edition = Join-Path $recipes "$($recipe.id)/$($recipe.edition)"
    New-Item -ItemType Directory -Path $edition -Force | Out-Null
    $recipe | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $edition 'recipe.json')
    $markdown | Set-Content (Join-Path $edition 'recipe.md') -NoNewline
    if ($duplicate) {
      New-Item -ItemType Directory -Path (Join-Path $recipes 'duplicate') | Out-Null
      Copy-Item -LiteralPath (Split-Path $edition) -Destination (Join-Path $recipes 'duplicate') -Recurse
    }
    $result = & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Build-Content.ps1') -RecipeRoot $recipes -OutputPath (Join-Path $fixture 'catalogue.json') 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or $result -notlike "*$($case.Error)*") { throw "Fixture '$($case.Name)' was not rejected correctly: $result" }
    Write-Host "Rejected $($case.Name)."
  }
}
finally {
  # The generated fixture is confined to this invocation's temporary directory.
  if (-not ([IO.Path]::GetFullPath($fixture).StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase))) { throw 'Unsafe fixture path.' }
  Remove-Item -LiteralPath $fixture -Recurse -Force -ErrorAction SilentlyContinue
}
