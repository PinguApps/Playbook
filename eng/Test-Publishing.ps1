[CmdletBinding()]
param([string]$Package)

$ErrorActionPreference = 'Stop'
function Reject([scriptblock]$Action) {
  $rejected = $false
  try { & $Action | Out-Null } catch { $rejected = $true }
  if (-not $rejected) { throw 'Expected publication guard to reject input.' }
}
$resolve = Join-Path $PSScriptRoot 'Resolve-PublishVersion.ps1'
$check = Join-Path $PSScriptRoot 'Assert-PublishPackage.ps1'
foreach ($tag in @('v1.0.0', '2.3.4', 'v1.0.0-preview.1')) {
  if ((& $resolve -Tag $tag -EventName workflow_dispatch) -cne ($tag -creplace '^v', '')) { throw 'Incorrect version.' }
}
foreach ($tag in @('v01.0.0', '1.0.0-preview.0', '1.0.0-preview.01', '1.0', '1.0.0-rc.1', '1.0.0+metadata', "1.0.0`n")) {
  Reject { & $resolve -Tag $tag -EventName workflow_dispatch }
}
Reject { & $resolve -Tag v1.0.0-preview.1 -EventName release -Prerelease }
Reject { & $resolve -Tag v1.0.0-preview.1 -EventName schedule }
Reject { & $resolve -Tag v1.0.0 -EventName release -Prerelease }
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-publish-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
try {
  $version = '1.0.0-preview.1'
  if ($Package) {
    $archive = [IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $Package))
    try {
      $entry = @($archive.Entries | Where-Object { $_.FullName -cmatch '^[^/]+\.nuspec$' })[0]
      $reader = [IO.StreamReader]::new($entry.Open())
      try { [xml]$spec = $reader.ReadToEnd(); $version = $spec.package.metadata.version } finally { $reader.Dispose() }
    } finally { $archive.Dispose() }
    Copy-Item -LiteralPath $Package -Destination (Join-Path $scratch "PinguApps.Playbook.$version.nupkg")
  }
  else {
    $source = Join-Path $scratch 'source'
    $null = New-Item -ItemType Directory -Path $source
    '<package><metadata><id>PinguApps.Playbook</id><version>1.0.0-preview.1</version></metadata></package>' | Set-Content (Join-Path $source 'PinguApps.Playbook.nuspec')
    [IO.Compression.ZipFile]::CreateFromDirectory($source, (Join-Path $scratch "PinguApps.Playbook.$version.nupkg"))
  }
  $file = Join-Path $scratch "PinguApps.Playbook.$version.nupkg"
  $hash = (Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant()
  $manifest = Join-Path $scratch 'package.sha256'
  "$hash  PinguApps.Playbook.$version.nupkg" | Set-Content $manifest
  & $check -Artifacts $scratch -PackageVersion $version
  Reject { & $check -Artifacts $scratch -PackageVersion '99.0.0' }
  Copy-Item $file (Join-Path $scratch 'unexpected.nupkg')
  Reject { & $check -Artifacts $scratch -PackageVersion $version }
  Remove-Item -LiteralPath (Join-Path $scratch 'unexpected.nupkg')
  ('0' * 64 + "  PinguApps.Playbook.$version.nupkg") | Set-Content $manifest
  Reject { & $check -Artifacts $scratch -PackageVersion $version }
  "$hash  other.nupkg" | Set-Content $manifest
  Reject { & $check -Artifacts $scratch -PackageVersion $version }
  # A matching filename/hash cannot disguise a different embedded version.
  $wrongVersion = '99.0.0'
  Move-Item $file (Join-Path $scratch "PinguApps.Playbook.$wrongVersion.nupkg")
  "$hash  PinguApps.Playbook.$wrongVersion.nupkg" | Set-Content $manifest
  Reject { & $check -Artifacts $scratch -PackageVersion $wrongVersion }
}
finally { Remove-Item -LiteralPath $scratch -Recurse -Force }
'PASS: publication versions, manual preview channel, package selection, embedded identity and hash guards (no login or push).'

& (Join-Path $PSScriptRoot 'Test-WeeklyRelease.ps1')
