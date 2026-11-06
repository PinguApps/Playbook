[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Artifacts,
  [Parameter(Mandatory)][string]$PackageVersion
)

$ErrorActionPreference = 'Stop'
$name = "PinguApps.Playbook.$PackageVersion.nupkg"
if ($PackageVersion -cnotmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-preview\.[1-9][0-9]*)?\z') { throw 'Invalid publication version.' }
$packages = @(Get-ChildItem -LiteralPath $Artifacts -Filter *.nupkg)
if ($packages.Count -ne 1 -or $packages[0].Name -cne $name) { throw 'Expected exactly the selected package.' }
$manifest = (Get-Content -LiteralPath (Join-Path $Artifacts 'package.sha256') -Raw).Trim()
$hash = (Get-FileHash -LiteralPath $packages[0].FullName -Algorithm SHA256).Hash.ToLowerInvariant()
if ($manifest -cnotmatch ('^[a-f0-9]{64}  ' + [regex]::Escape($name) + '$') -or $manifest.Substring(0, 64) -cne $hash) { throw 'Package hash or manifest identity mismatch.' }
$archive = [IO.Compression.ZipFile]::OpenRead($packages[0].FullName)
try {
  $specs = @($archive.Entries | Where-Object { $_.FullName -cmatch '^[^/]+\.nuspec$' })
  if ($specs.Count -ne 1) { throw 'Expected one root nuspec.' }
  $reader = [IO.StreamReader]::new($specs[0].Open())
  try { [xml]$spec = $reader.ReadToEnd() } finally { $reader.Dispose() }
  if ($spec.package.metadata.id -cne 'PinguApps.Playbook' -or $spec.package.metadata.version -cne $PackageVersion) { throw 'Embedded package identity mismatch.' }
}
finally { $archive.Dispose() }
"Verified $name SHA-256 $hash"
