[CmdletBinding()]
param([switch]$Pack)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$root = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path ([IO.Path]::GetTempPath()) "playbook-evidence-$([guid]::NewGuid().ToString('N'))"
try {
  New-Item -ItemType Directory -Path $fixture | Out-Null
  Copy-Item (Join-Path $root 'samples') $fixture -Recurse
  Copy-Item (Join-Path $root 'content') $fixture -Recurse
  Copy-Item (Join-Path $root 'NuGet.Config') $fixture
  New-Item -ItemType Directory -Path (Join-Path $fixture 'eng') | Out-Null
  Copy-Item (Join-Path $PSScriptRoot 'Evidence.ps1'), (Join-Path $PSScriptRoot 'Record-Evidence.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-StaticInterfaceHelpers.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-FSharpGuidance.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-UnsafeBoundaries.ps1') (Join-Path $fixture 'eng')
  $output = Join-Path $fixture 'catalogue.json'
  $buildArgs = @('-NoProfile', '-File', (Join-Path $PSScriptRoot 'Build-Content.ps1'), '-RepositoryRoot', $fixture, '-OutputPath', $output)
  & pwsh @buildArgs
  if ($LASTEXITCODE -ne 0) { throw 'Unchanged evidence must remain valid.' }
  $first = Get-Content $output -Raw
  'An unrelated documentation edit must not invalidate sample proof.' | Set-Content (Join-Path $fixture 'README.md')
  & pwsh @buildArgs
  if ($LASTEXITCODE -ne 0 -or (Get-Content $output -Raw) -cne $first) { throw 'Evidence reuse must be deterministic.' }
  Write-Host 'PASS: unchanged evidence is reusable without invoking sample SDKs.'

  $source = Join-Path $fixture 'samples/PropertyAssignmentChecks.cs'
  $original = Get-Content $source -Raw
  Add-Content $source '// A changed verification input needs fresh compilation.'
  if ($Pack) {
    $result = & dotnet pack (Join-Path $root 'src/PinguApps.Playbook') -c Release --no-build `
      "-p:EvidenceRepositoryRoot=$fixture" "-p:RecipeRoot=$fixture/content/recipes" `
      -p:PackageVersion=0.0.0-evidence-fixture --output (Join-Path $fixture 'packages') 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or $result -notlike '*Stale compilation evidence*') { throw "Pack accepted stale compilation: $result" }
    if (Get-ChildItem (Join-Path $fixture 'packages') -Filter *.nupkg -ErrorAction SilentlyContinue) { throw 'Stale evidence produced a package.' }
    Write-Host 'PASS: real pack --no-build rejects changed sample inputs; no package produced.'
    [IO.File]::WriteAllText($source, $original)
    $recipePath = Join-Path $fixture 'content/recipes/csharp.validate-property-assignment/10.0/recipe.json'
    $recipe = Get-Content $recipePath -Raw | ConvertFrom-Json -AsHashtable
    $recipe.humanProof = @{ status = 'pending'; scope = 'Contract fixture: release proof has not been supplied.' }
    $recipe | ConvertTo-Json -Depth 30 | Set-Content $recipePath
    $result = & dotnet pack (Join-Path $root 'src/PinguApps.Playbook') -c Release --no-build `
      "-p:EvidenceRepositoryRoot=$fixture" "-p:RecipeRoot=$fixture/content/recipes" `
      -p:PlaybookReleaseReady=true -p:PackageVersion=0.0.0-evidence-fixture --output (Join-Path $fixture 'packages') 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0 -or $result -notlike '*Pending Human proof blocks release readiness*') { throw "Release pack accepted pending Human proof: $result" }
    if (Get-ChildItem (Join-Path $fixture 'packages') -Filter *.nupkg -ErrorAction SilentlyContinue) { throw 'Pending Human proof produced a release package.' }
    Write-Host 'PASS: real release pack rejects pending Human proof; no package produced.'
    return
  }
  $result = & pwsh @buildArgs 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0 -or $result -notlike '*Stale compilation evidence*') { throw "Changed source was accepted: $result" }
  [IO.File]::WriteAllText($source, $original)
  Write-Host 'PASS: changed shared source requires refreshed compilation.'

  foreach ($case in @(
    @{ Path = 'samples/net10.0/PropertyAssignment/global.json'; From = '10.0.402'; To = '10.0.403'; Error = 'Evidence baseline disagrees' },
    @{ Path = 'samples/net10.0/PropertyAssignment/PropertyAssignment.csproj'; From = 'net10.0'; To = 'net11.0'; Error = 'Evidence baseline disagrees' },
    @{ Path = 'samples/Directory.Packages.props'; From = '</Project>'; To = '<ItemGroup><PackageVersion Include="Fixture.Dependency" Version="2.0.0" /></ItemGroup></Project>'; Error = 'Stale compilation evidence' },
    @{ Path = 'samples/net10.0/PropertyAssignment/verification.json'; From = '"nativeToolchains": []'; To = '"nativeToolchains": ["Fixture.Native/2.0"]'; Error = 'Stale compilation evidence' }
  )) {
    $fullPath = Join-Path $fixture $case.Path
    $before = Get-Content $fullPath -Raw
    [IO.File]::WriteAllText($fullPath, $before.Replace($case.From, $case.To))
    $result = & pwsh @buildArgs 2>&1 | Out-String
    [IO.File]::WriteAllText($fullPath, $before)
    if ($LASTEXITCODE -eq 0 -or $result -notlike "*$($case.Error)*") { throw "Changed input was accepted ($($case.Path)): $result" }
  }
  Write-Host 'PASS: SDK, dependency and toolchain declarations require refreshed compilation.'
  $recordPath = Join-Path $fixture 'samples/net10.0/PropertyAssignment/verification-evidence.json'
  $recordText = Get-Content $recordPath -Raw
  Remove-Item -LiteralPath $recordPath
  $result = & pwsh @buildArgs 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0 -or $result -notlike '*Missing compilation evidence*') { throw "Missing compilation was accepted: $result" }
  $record = $recordText | ConvertFrom-Json -AsHashtable
  $record.kind = 'mock'
  $record | ConvertTo-Json -Depth 30 | Set-Content $recordPath
  $result = & pwsh @buildArgs 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0 -or $result -notlike '*Missing compilation evidence*') { throw "Mock was accepted as compilation: $result" }
  [IO.File]::WriteAllText($recordPath, $recordText)
  Write-Host 'PASS: missing records and mocks cannot satisfy compilation.'

  $recipePath = Join-Path $fixture 'content/recipes/csharp.validate-property-assignment/10.0/recipe.json'
  $recipe = Get-Content $recipePath -Raw | ConvertFrom-Json -AsHashtable
  # Synthetic classification only; this fixture does not claim an official review happened.
  $review = $recordText | ConvertFrom-Json -AsHashtable
  $review.kind = 'official-review'
  $review.command = 'Fixture classification: review an official reference.'
  $review.output = 'Contract fixture only; not real source review or native evidence.'
  $review | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $fixture 'content/review.evidence.json')
  $reference = $recipe.evidence[1].Clone()
  $reference.kind = 'official-review'
  $reference.record = 'content/review.evidence.json'
  $recipe.evidence += $reference
  $recipe | ConvertTo-Json -Depth 30 | Set-Content $recipePath
  & pwsh @buildArgs
  if ($LASTEXITCODE -ne 0) { throw 'Official review must be distinct from accompanying compilation.' }
  $served = Get-Content $output -Raw | ConvertFrom-Json
  $propertyRecipe = @($served | Where-Object id -eq 'csharp.validate-property-assignment')
  if ($propertyRecipe.Count -ne 1 -or $propertyRecipe[0].evidence[-1].kind -cne 'official-review') { throw 'Official review classification was lost.' }
  $recipe.evidence = @($recipe.evidence | Where-Object { $_.sample -cne $reference.sample }) + @($reference)
  $recipe | ConvertTo-Json -Depth 30 | Set-Content $recipePath
  $result = & pwsh @buildArgs 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0 -or $result -notlike '*Missing compilation evidence*') { throw "Source review was accepted as compilation: $result" }
  $recipe = Get-Content (Join-Path $root 'content/recipes/csharp.validate-property-assignment/10.0/recipe.json') -Raw | ConvertFrom-Json -AsHashtable
  Write-Host 'PASS: official review is preserved separately and cannot replace compilation.'
  $recipe.humanProof = @{ status = 'pending'; scope = 'Targeted authoring-machine proof remains pending.' }
  $recipe | ConvertTo-Json -Depth 30 | Set-Content $recipePath
  & pwsh @buildArgs
  if ($LASTEXITCODE -ne 0) { throw 'Pending Human proof may be served in incremental builds.' }
  $result = & pwsh @buildArgs -ReleaseReady 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0 -or $result -notlike '*Pending Human proof blocks release readiness*') { throw "Release accepted pending Human proof: $result" }
  Write-Host 'PASS: pending Human proof is visible in incremental builds and blocks release readiness.'
}
finally {
  $resolved = [IO.Path]::GetFullPath($fixture)
  if (-not $resolved.StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase) -or
      (Split-Path $resolved -Leaf) -notlike 'playbook-evidence-*') { throw 'Unsafe fixture cleanup target.' }
  Remove-Item -LiteralPath $resolved -Recurse -Force -ErrorAction SilentlyContinue
}
