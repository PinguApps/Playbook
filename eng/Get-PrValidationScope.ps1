[CmdletBinding()]
param(
  [string]$BaseSha,
  [string]$HeadSha,
  [string]$MergeSha
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
try {
  foreach ($sha in @($BaseSha, $HeadSha, $MergeSha)) {
    if ($sha -cnotmatch '\A[0-9a-f]{40}\z') { throw 'Missing event commit identity.' }
  }

  $root = Split-Path $PSScriptRoot -Parent
  $git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
  $parents = & $git -C $root rev-list --parents -n 1 HEAD
  if ($parents -cne "$MergeSha $BaseSha $HeadSha") {
    throw 'Checkout is not the expected two-parent PR merge.'
  }

  # Compare the tested merge with its base. Disabling rename detection checks both paths.
  $paths = @(& $git -C $root -c core.quotePath=true diff --no-ext-diff --no-renames --ignore-submodules=none --name-only $BaseSha $MergeSha --)
  if (-not $paths.Count -or @($paths | Where-Object { $_.StartsWith('"') }).Count) {
    throw 'The changed paths are empty or need Git path decoding.'
  }

  # These exact paths are unserved and do not feed the build or package.
  # README.md is packed, and docs/ also contains build and evidence inputs.
  $unserved = @('.github/pull_request_template.md', 'GLOSSARY.md', 'docs/automation.md', 'docs/contributing.md', 'docs/inspect-guidance.md')
  $policyInputs = '(^\.github/workflows/|^eng/|^renovate\.json$|^docs/dependency-updates\.md$|(^|/)(Directory\.[^/]+|global\.json|NuGet\.config|packages\.lock\.json|dotnet-tools\.json)$|\.(csproj|fsproj|vbproj|props|targets|slnx?)$)'
  @{
    DocsOnly = @($paths | Where-Object { $_ -cnotin $unserved }).Count -eq 0
    PolicyRequired = @($paths | Where-Object { $_ -match $policyInputs }).Count -gt 0
  }
}
catch {
  Write-Warning "PR scope unavailable. Running smoke and dependency policy checks. $($_.Exception.Message)"
  @{ DocsOnly = $false; PolicyRequired = $true }
}
