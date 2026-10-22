[CmdletBinding()]
param([switch]$Pack)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$root = Split-Path $PSScriptRoot -Parent
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$inputs = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($record in (Get-ChildItem (Join-Path $root 'samples') -Recurse -Filter verification-evidence.json)) {
  $saved = Get-Content -LiteralPath $record.FullName -Raw | ConvertFrom-Json -AsHashtable
  foreach ($inputPath in $saved.inputs.Keys) { $null = $inputs.Add($inputPath) }
}
$inputPaths = @($inputs | Sort-Object -CaseSensitive)
$raw = @($inputPaths | & $git -C $root hash-object --no-filters --stdin-paths)
if ($LASTEXITCODE -ne 0) { throw 'Could not hash the raw evidence inputs.' }
$stored = @($inputPaths | & $git -C $root hash-object --stdin-paths)
if ($LASTEXITCODE -ne 0 -or $raw.Count -ne $inputPaths.Count -or $stored.Count -ne $inputPaths.Count) { throw 'Could not inspect Git normalization for every evidence input.' }
for ($index = 0; $index -lt $inputPaths.Count; $index++) {
  if ($raw[$index] -cne $stored[$index]) {
    throw "Git normalization changes evidence input $($inputPaths[$index]). Normalize its bytes (normally LF), then recapture affected proof before committing."
  }
}
Write-Host 'PASS: evidence input bytes survive Git normalization in a fresh checkout.'
$fixture = Join-Path ([IO.Path]::GetTempPath()) "playbook-evidence-$([guid]::NewGuid().ToString('N'))"
try {
  New-Item -ItemType Directory -Path $fixture | Out-Null
  # Evidence fixtures need authored inputs, not generated build outputs. Besides
  # their size, outputs can contain configuration names Windows copy normalizes.
  foreach ($file in (Get-ChildItem (Join-Path $root 'samples') -Recurse -File -Force)) {
    $relative = [IO.Path]::GetRelativePath($root, $file.FullName).Replace('\', '/')
    if ($relative -match '(^|/)(bin|obj)/') { continue }
    $destination = Join-Path $fixture $relative
    New-Item -ItemType Directory -Path (Split-Path $destination) -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $destination
  }
  Copy-Item (Join-Path $root 'content') $fixture -Recurse
  Copy-Item (Join-Path $root 'NuGet.Config') $fixture
  Copy-Item (Join-Path $root '.editorconfig') $fixture
  New-Item -ItemType Directory -Path (Join-Path $fixture 'eng') | Out-Null
  Copy-Item (Join-Path $PSScriptRoot 'Evidence.ps1'), (Join-Path $PSScriptRoot 'Record-Evidence.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-StaticInterfaceHelpers.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-FSharpGuidance.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-FSharpBoundaries.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-RuntimeAsync.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-FSharpLifetimes.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-UnsafeBoundaries.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-ProcessPlatforms.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-AssemblyDeployments.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-OptionsStartup.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-Observability.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-CryptoPlatforms.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-CrashEvidence.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-SdkWorkflows.ps1'), (Join-Path $PSScriptRoot 'Test-WatchHosts.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-SdkDelivery.ps1') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-SurveyForms.ps1'), (Join-Path $PSScriptRoot 'SurveyForms.browser.js') (Join-Path $fixture 'eng')
  Copy-Item (Join-Path $PSScriptRoot 'Test-VisitBookings.ps1'), (Join-Path $PSScriptRoot 'VisitBookings.browser.js') (Join-Path $fixture 'eng')
  $output = Join-Path $fixture 'catalogue.json'
  $buildArgs = @('-NoProfile', '-File', (Join-Path $PSScriptRoot 'Build-Content.ps1'), '-RepositoryRoot', $fixture, '-OutputPath', $output)
  & pwsh @buildArgs
  if ($LASTEXITCODE -ne 0) { throw 'Unchanged evidence must remain valid.' }
  $first = Get-Content $output -Raw
  'An unrelated documentation edit must not invalidate sample proof.' | Set-Content (Join-Path $fixture 'README.md')
  & pwsh @buildArgs
  if ($LASTEXITCODE -ne 0 -or (Get-Content $output -Raw) -cne $first) { throw 'Evidence reuse must be deterministic.' }
  Write-Host 'PASS: unchanged evidence is reusable without invoking sample SDKs.'

  $template = Join-Path $fixture 'samples/net10.0/SdkWorkflows/Template/.template.config/template.json'
  if (-not (Test-Path -LiteralPath $template)) { throw 'Evidence fixtures must retain hidden template inputs.' }
  $templateText = Get-Content -LiteralPath $template -Raw
  Add-Content -LiteralPath $template ' '
  $result = & pwsh @buildArgs 2>&1 | Out-String
  [IO.File]::WriteAllText($template, $templateText)
  if ($LASTEXITCODE -eq 0 -or $result -notlike '*Stale compilation evidence*') { throw "Changed hidden template input was accepted: $result" }
  Write-Host 'PASS: hidden template inputs are copied and invalidate stale evidence.'

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
    @{ Path = 'samples/net10.0/RepositoryUtility/slots.cs'; From = 'TargetFramework=net10.0'; To = 'TargetFramework=net11.0'; Error = 'Stale compilation evidence' },
    @{ Path = 'samples/net10.0/RepositoryUtility/slots.cs'; From = 'slots={total}'; To = 'racks={total}'; Error = 'Stale compilation evidence' },
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
  $recipe.humanProof = @{ status = 'pending'; scope = 'Theoretical edition; compilation and behavior have deliberately not been checked.' }
  $recipe | ConvertTo-Json -Depth 30 | Set-Content $recipePath
  & pwsh @buildArgs
  if ($LASTEXITCODE -ne 0) { throw 'An explicitly pending source-only edition may be served for review.' }
  $served = Get-Content $output -Raw | ConvertFrom-Json
  $theoretical = @($served | Where-Object id -eq 'csharp.validate-property-assignment')[0]
  if ($theoretical.evidence[-1].role -cne 'source-review' -or
      -not ($theoretical.applicability.routes[0].selectionNotes -like '*Theoretical guidance:*') -or
      -not ($theoretical.applicability.routes[0].requirements.stabilityLimitations -like '*not been compiled*')) {
    throw 'Theoretical guidance lost its discovery/detail warning or evidence classification.'
  }
  $result = & pwsh @buildArgs -ReleaseReady 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0 -or $result -notlike '*Pending Human proof blocks release readiness*') { throw "Release accepted theoretical guidance: $result" }
  $review.kind = 'mock'
  $review | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $fixture 'content/review.evidence.json')
  $recipe.evidence[-1].kind = 'mock'
  $recipe | ConvertTo-Json -Depth 30 | Set-Content $recipePath
  $result = & pwsh @buildArgs 2>&1 | Out-String
  if ($LASTEXITCODE -eq 0 -or $result -notlike '*Missing compilation evidence*') { throw "Pending status accepted a mock as compilation: $result" }
  Write-Host 'PASS: theoretical source-only guidance is disclosed; release builds and mocks remain gated.'
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
