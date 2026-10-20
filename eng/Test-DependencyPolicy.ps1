[CmdletBinding()]
param([string]$RepositoryRoot = (Split-Path $PSScriptRoot -Parent))

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$git = (Get-Command git -CommandType Application).Source
$files = @(& $git -C $root ls-files --cached --others --exclude-standard)
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect the dependency-policy inventory.' }

# These are the reviewed consumers of automated updates to the root package file.
$rootConsumers = @(
  'src/PinguApps.Playbook/PinguApps.Playbook.csproj',
  'tests/PinguApps.Playbook.Tests/PinguApps.Playbook.Tests.csproj'
)
[xml]$build = Get-Content -LiteralPath (Join-Path $root 'Directory.Build.props') -Raw
if (@($build.SelectNodes('//TargetFramework')).Count -ne 1 -or $build.Project.PropertyGroup.TargetFramework -ne 'net10.0') {
  throw 'Review the Renovate release constraints and consumer inventory before changing the host framework.'
}

foreach ($consumer in $rootConsumers) {
  if ($consumer -notin $files) { throw "Review the Renovate consumer inventory after removing or moving $consumer." }
}
foreach ($file in $files) {
  if ($file -match '^(src|tests)/.*packages\.lock\.json$') {
    throw "Review Renovate's artifact-update exemption before adding $file."
  }
  if ($file -notmatch '\.(cs|fs|vb)proj$') { continue }
  if ($file -notin $rootConsumers -and -not $file.StartsWith('samples/', [StringComparison]::Ordinal)) {
    throw "Classify $file and review its inherited dependency policy before adding or moving this project."
  }
  $directory = Split-Path (Join-Path $root $file)
  while (-not (Test-Path -LiteralPath (Join-Path $directory 'Directory.Packages.props'))) {
    if ($directory -eq $root) { throw "No central-package boundary found for $file." }
    $directory = Split-Path $directory
  }
  if ($file.StartsWith('samples/', [StringComparison]::Ordinal)) {
    if ($directory -eq $root) { throw "Sample $file inherits the automated root package versions; give it an isolated package boundary." }
  }
  else {
    if ($directory -ne $root) { throw "Review the changed package inheritance for $file." }
    [xml]$project = Get-Content -LiteralPath (Join-Path $root $file) -Raw
    foreach ($target in $project.SelectNodes('//TargetFramework | //TargetFrameworks')) {
      if ($target.InnerText -ne 'net10.0') { throw "Review Renovate's release constraints before retargeting $file." }
    }
  }
}
Write-Host 'PASS: reviewed root consumers, .NET 10 target, isolated samples and artifact-update exemption.'
