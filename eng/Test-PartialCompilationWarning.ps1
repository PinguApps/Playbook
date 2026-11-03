#Requires -Version 7.0
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root 'artifacts/agent-guidance/partial-compilation.json'
$arguments = @{
  RepositoryRoot = $root
  RecipeRoot = Join-Path $root 'content/recipes/hosting.select-direct-tls'
  OutputPath = $output
}
& (Join-Path $PSScriptRoot 'Build-Content.ps1') @arguments
$recipe = @(Get-Content $output -Raw | ConvertFrom-Json)[0]
$current = 'samples/net11.0/HarbourTls/HarbourTls.csproj'
$old = @($recipe.evidence | Where-Object { $_.target -ceq 'net10.0' })
$new = @($recipe.evidence | Where-Object { $_.sample -ceq $current })
if ($old.Count -ne 1 -or $old[0].kind -cne 'compile' -or $new.Count -ne 1 -or $new[0].kind -cne 'official-review') {
  throw 'Mixed compilation/source-review evidence was misclassified.'
}
foreach ($notes in @($recipe.applicability.routes[0].selectionNotes, $recipe.applicability.routes[0].requirements.stabilityLimitations)) {
  if (-not ($notes -like "*source review only for $current;*not been compiled successfully*")) {
    throw 'Missing sample-specific warning in discovery or detail.'
  }
  if ($notes -ccontains 'Theoretical guidance: source review only; samples have not been compiled or executed. Pending proof blocks release-ready packaging.') {
    throw 'A blanket warning incorrectly denies the recorded older compilation.'
  }
}
$rejected = $false
try { & (Join-Path $PSScriptRoot 'Build-Content.ps1') @arguments -ReleaseReady }
catch {
  if ($_.Exception.Message -notlike '*Pending Human proof blocks release readiness*') { throw }
  $rejected = $true
}
if (-not $rejected) { throw 'Partial compilation cleared the release-ready gate.' }
Write-Host 'PASS: mixed evidence names the uncompiled current sample, retains older compilation and preserves the release-ready gate.'
