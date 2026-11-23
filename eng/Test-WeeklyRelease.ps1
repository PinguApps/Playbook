[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$select = Join-Path $PSScriptRoot 'Select-WeeklyRelease.ps1'
function Release($tag, $draft = $false, $preview = $false) {
  @{ id = 123; tag_name = $tag; draft = $draft; prerelease = $preview; published_at = '2026-11-01T00:00:00Z'; body = '' }
}
function Expect($releases, $expected) {
  $result = & $select -Releases $releases -Repository $scratch
  if ($result.tag_name -cne $expected) { throw "Expected '$expected'." }
}
function ExpectRejection($releases, [string]$name, [string]$expectedError) {
  try { $result = & $select -Releases $releases -Repository $scratch } catch {
    if ($_.Exception.Message -notmatch $expectedError) { throw }
    Write-Host "PASS: $name rejected"
    return
  }
  throw "Expected rejection for $name. Selector returned: $(ConvertTo-Json -InputObject $result -Compress)"
}
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-weekly-' + [guid]::NewGuid().ToString('N'))
try {
  & $git init --quiet $scratch
  & $git -C $scratch -c commit.gpgsign=false -c user.name=Fixture -c user.email=fixture@example.invalid commit --quiet --allow-empty -m initial
  & $git -C $scratch -c tag.gpgsign=false tag v1.0.0
  $stable = Release v1.0.0
  $draft = Release v1.0.1 $true
  Expect @() $null
  Expect @((Release v1.0.0-preview.1), $draft) $null
  Expect @((Release v1.0.0 $false $true), $draft) $null
  Expect @((Release v01.0.0), $draft) $null
  Expect @($stable, $draft) $null # No commits since stable.
  # Each change category independently unlocks the same empty-note draft.
  foreach ($path in @('docs/guide.md', '.github/workflows/fixture.yml', 'tests/fixture.txt', 'maintenance.txt')) {
    & $git -C $scratch reset --quiet --hard v1.0.0
    $file = Join-Path $scratch $path
    $null = New-Item -ItemType Directory -Force (Split-Path $file)
    'change' | Set-Content $file
    & $git -C $scratch add -- $path
    & $git -C $scratch -c commit.gpgsign=false -c user.name=Fixture -c user.email=fixture@example.invalid commit --quiet -m change
    Expect @($stable, $draft) v1.0.1
  }
  Expect @($stable) $null
  Expect @($stable, (Release v1.0.1 $true $true)) $null
  foreach ($tag in @('v1.0.1-preview.1', 'v01.0.1', 'v1.0.01', 'v1.0.0', 'v0.9.9', 'v1.0.1+meta', "v1.0.1`n")) {
    $displayTag = $tag.Replace("`n", '\n')
    ExpectRejection @($stable, (Release $tag $true)) "invalid or non-increasing draft tag ($displayTag)" '^Draft must have a newer stable tag\.$'
  }
  ExpectRejection @($stable, $draft, (Release v2.0.0 $true)) 'ambiguous release drafts' '^Multiple release drafts; refusing to choose one\.$'
  Expect @($stable, (Release v1.0.10 $true)) v1.0.10
  & $git -C $scratch -c tag.gpgsign=false tag v2.0.0
  & $git -C $scratch reset --quiet --hard v1.0.0
  ExpectRejection @((Release v2.0.0), (Release v2.0.1 $true)) 'stable tag outside main history (v2.0.0)' '^Latest stable release is not on main \(Git exit 1\)\.\s*$'
  ExpectRejection @((Release v9.0.0), $draft) 'missing stable tag (v9.0.0)' '(?s)Git exit 128.*fatal: Not a valid object name v9\.0\.0'
}
finally {
  if (Test-Path -LiteralPath $scratch) { Remove-Item -LiteralPath $scratch -Recurse -Force }
}
# Expected native failures above must not fail GitHub's PowerShell exit wrapper.
$global:LASTEXITCODE = 0
'PASS: weekly stable/preview flags, strict tags, draft ambiguity, Git ancestry and any-main-commit guards; no remote writes.'
