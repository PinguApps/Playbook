[CmdletBinding()]
param(
  [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Releases,
  [Parameter(Mandatory)][string]$Repository
)

$ErrorActionPreference = 'Stop'
$git = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$stable = '^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\z'
$latest = $Releases | Where-Object { $_.draft -eq $false -and $_.prerelease -eq $false -and $_.tag_name -cmatch $stable } |
  Sort-Object published_at -Descending | Select-Object -First 1
if (-not $latest) { Write-Host 'No published stable release; first stable publication is manual'; return }
& $git -C $Repository merge-base --is-ancestor $latest.tag_name HEAD
if ($LASTEXITCODE -ne 0) { throw 'Latest stable release is not on main.' }
$count = & $git -C $Repository rev-list --count "$($latest.tag_name)..HEAD"
if ($LASTEXITCODE -ne 0) { throw 'Cannot count main commits.' }
if ($count -eq '0') { Write-Host 'No new main commits'; return }
$drafts = @($Releases | Where-Object { $_.draft -eq $true })
if ($drafts.Count -gt 1) { throw 'Multiple release drafts; refusing to choose one.' }
$draft = $drafts | Where-Object { $_.prerelease -eq $false }
if (-not $draft) { Write-Host 'No stable release draft'; return }
if ($draft.tag_name -cnotmatch $stable) { throw 'Draft must have a newer stable tag.' }
# Compare arbitrary-size SemVer components rather than lexical or fixed-width versions.
$old = $latest.tag_name.Substring(1).Split('.')
$new = $draft.tag_name.Substring(1).Split('.')
$comparison = 0
for ($i = 0; $i -lt 3 -and $comparison -eq 0; $i++) {
  $comparison = ([bigint]::Parse($new[$i])).CompareTo([bigint]::Parse($old[$i]))
}
if ($comparison -le 0) { throw 'Draft must have a newer stable tag.' }
$draft
