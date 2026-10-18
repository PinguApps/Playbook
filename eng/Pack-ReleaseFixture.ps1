[CmdletBinding()]
param([string]$PackageVersion = '1.0.0-preview.1')

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path ([IO.Path]::GetTempPath()) ("playbook-release-fixture-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
try {
  Copy-Item (Join-Path $root 'content/recipes/*') $fixture -Recurse
  $retainedPath = Join-Path $fixture 'csharp.validate-property-assignment/10.0/recipe.json'
  $retained = Get-Content $retainedPath -Raw | ConvertFrom-Json -AsHashtable
  $newer = Get-Content (Join-Path $fixture 'csharp.validate-property-assignment/11.0/recipe.json') -Raw | ConvertFrom-Json -AsHashtable
  $retained.supportedTargets += '11.0'
  $retained.applicability.routes += $newer.applicability.routes
  $retained.evidence += @($newer.evidence | Where-Object { $_.target -eq 'net11.0' })
  $retained | ConvertTo-Json -Depth 30 | Set-Content $retainedPath -Encoding utf8
  # Contract-only metadata: no fictional .NET 11 API or publication evidence is claimed.
  # Reuse actual compiled property samples to test selection independently of catalogue growth.
  foreach ($case in @(
    @{ Id = 'fixture.only-eleven'; Edition = '11.0'; SourceEdition = '11.0'; Introduced = '10.0' },
    @{ Id = 'fixture.conditional-route'; Edition = '11.0'; SourceEdition = '10.0'; Introduced = '11.0' }
  )) {
    $source = Join-Path $root "content/recipes/csharp.validate-property-assignment/$($case.SourceEdition)"
    $destination = Join-Path $fixture "$($case.Id)/$($case.Edition)"
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    $recipe = Get-Content (Join-Path $source 'recipe.json') -Raw | ConvertFrom-Json -AsHashtable
    $recipe.id = $case.Id
    $recipe.edition = $case.Edition
    $recipe.introducedIn = $case.Introduced
    $recipe.title = "Contract fixture: $($case.Id)"
    $recipe.goal = 'Test release routing with actual sample evidence; not a shipped recipe or an API introduction claim.'
    if ($case.Id -eq 'fixture.conditional-route') {
      $recipe.applicability.routes = @($recipe.applicability.routes | Where-Object { $_.conditional })
    }
    $recipe | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $destination 'recipe.json') -Encoding utf8
    Copy-Item (Join-Path $source 'recipe.md') $destination
  }
  dotnet pack (Join-Path $root 'src/PinguApps.Playbook') -c Release -p:Version=$PackageVersion -p:PackageVersion=$PackageVersion `
    "-p:RecipeRoot=$fixture" --output (Join-Path $root 'artifacts/release-fixture')
  # The positive fixture above must build; removing its explicit conditional flag must fail.
  $conditionalPath = Join-Path $fixture 'fixture.conditional-route/11.0/recipe.json'
  $invalid = Get-Content $conditionalPath -Raw | ConvertFrom-Json -AsHashtable
  $invalid.applicability.routes[0].conditional = $false
  $invalid | ConvertTo-Json -Depth 30 | Set-Content $conditionalPath -Encoding utf8
  $PSNativeCommandUseErrorActionPreference = $false
  try {
    $validation = & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Build-Content.ps1') `
      -RecipeRoot $fixture -OutputPath (Join-Path $fixture 'invalid.json') 2>&1 | Out-String
    $validationExitCode = $LASTEXITCODE
  }
  finally { $PSNativeCommandUseErrorActionPreference = $true }
  if ($validationExitCode -eq 0 -or $validation -notlike '*explicit conditional route is required*') {
    throw "Expected the unsupported pre-introduction route to fail: $validation"
  }
  Write-Output 'PASS: pre-introduction target requires a documented conditional route.'
}
finally {
  # Restore ordinary build output so a later --no-build pack cannot pick up fixture recipes.
  dotnet build (Join-Path $root 'src/PinguApps.Playbook') -c Release -p:Version=$PackageVersion
  # The resolved cleanup target must stay inside the uniquely named temporary fixture.
  $resolved = [IO.Path]::GetFullPath($fixture)
  $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
  if (-not $resolved.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase) -or
      (Split-Path $resolved -Leaf) -notlike 'playbook-release-fixture-*') { throw 'Unsafe fixture cleanup target.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
