[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Tag,
  [Parameter(Mandatory)][string]$EventName,
  [switch]$Prerelease
)

$ErrorActionPreference = 'Stop'
if ($Tag -cnotmatch '^v?(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-preview\.[1-9][0-9]*)?\z') {
  throw 'Expected a stable SemVer tag or manual preview.N tag.'
}
$version = $Tag -creplace '^v', ''
$preview = $version.Contains('-')
if ($preview -and $EventName -ne 'workflow_dispatch') { throw 'Preview publication must be manually dispatched.' }
if ($EventName -eq 'release' -and $Prerelease) { throw 'Stable release tags must not be marked prerelease.' }
$version
