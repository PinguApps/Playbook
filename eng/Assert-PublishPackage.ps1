[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Artifacts,
  [Parameter(Mandatory)][string]$PackageVersion,
  [switch]$BuildVersion,
  [switch]$Record,
  [switch]$PassThru
)

$ErrorActionPreference = 'Stop'
if ($BuildVersion) {
  if ($PackageVersion -cnotmatch '^\d+\.\d+\.\d+(?:-[a-zA-Z0-9]+(?:[.-][a-zA-Z0-9]+)*)?\z') { throw 'Invalid build version.' }
}
elseif ($PackageVersion -cnotmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-preview\.[1-9][0-9]*)?\z') { throw 'Invalid publication version.' }

# The pointer exposes the version to installers, so publish it only after every payload.
$rids = @('win-x64', 'win-arm64', 'osx-x64', 'osx-arm64', 'linux-x64', 'linux-arm64', 'any')
$ids = @($rids | ForEach-Object { "PinguApps.Playbook.$_" }) + 'PinguApps.Playbook'
function Read-PackageEntry($Archive, [string]$FileName) {
  $entries = @($Archive.Entries | Where-Object Name -CEQ $FileName)
  if ($entries.Count -ne 1) { throw "Expected one $FileName in $name." }
  $reader = [IO.StreamReader]::new($entries[0].Open())
  try { $reader.ReadToEnd() } finally { $reader.Dispose() }
}
$packages = @(Get-ChildItem -LiteralPath $Artifacts -Filter *.nupkg)
if ($packages.Count -ne $ids.Count) { throw 'Expected exactly the selected package set.' }
$plan = foreach ($id in $ids) {
  $name = "$id.$PackageVersion.nupkg"
  $package = $packages | Where-Object Name -CEQ $name
  if (-not $package) { throw "Missing package $name." }
  $archive = [IO.Compression.ZipFile]::OpenRead($package.FullName)
  try {
    $specs = @($archive.Entries | Where-Object { $_.FullName -cmatch '^[^/]+\.nuspec$' })
    if ($specs.Count -ne 1) { throw "Expected one root nuspec in $name." }
    $reader = [IO.StreamReader]::new($specs[0].Open())
    try { [xml]$spec = $reader.ReadToEnd() } finally { $reader.Dispose() }
    $metadata = $spec.package.metadata
    if ($metadata.id -cne $id -or $metadata.version -cne $PackageVersion) { throw "Embedded package identity mismatch in $name." }
    [xml]$settings = Read-PackageEntry $archive 'DotnetToolSettings.xml'
    if ($settings.DotNetCliTool.Commands.Command.Name -cne 'playbook') { throw "Unexpected command in $name." }
    if ($id -ceq 'PinguApps.Playbook') {
      $references = @($settings.DotNetCliTool.RuntimeIdentifierPackages.RuntimeIdentifierPackage)
      if ($references.Count -ne ($rids.Count + 1)) { throw 'Pointer must reference exactly the expected payloads and musl fallback.' }
      foreach ($rid in ($rids + 'linux-musl')) {
        $referencesForRid = @($references | Where-Object RuntimeIdentifier -CEQ $rid)
        $expectedId = if ($rid -ceq 'linux-musl') { 'PinguApps.Playbook.any' } else { "PinguApps.Playbook.$rid" }
        if ($referencesForRid.Count -ne 1 -or $referencesForRid[0].Id -cne $expectedId) {
          throw "Unexpected pointer reference for $rid."
        }
      }
      if ($archive.Entries.Name -ccontains 'PinguApps.Playbook.dll') { throw 'Pointer includes a tool payload.' }
    }
    else {
      $rid = $id.Substring('PinguApps.Playbook.'.Length)
      $config = (Read-PackageEntry $archive 'PinguApps.Playbook.runtimeconfig.json' | ConvertFrom-Json).runtimeOptions
      if ($config.tfm -cne 'net11.0') { throw "Unexpected target framework in $name." }
      if ($rid -ceq 'any') {
        if ($config.framework.name -cne 'Microsoft.NETCore.App' -or $config.framework.version -notmatch '^11\.' -or
            $config.rollForward -cne 'Major' -or $config.includedFrameworks) { throw 'Expected a framework-dependent fallback.' }
      }
      else {
        $deps = Read-PackageEntry $archive 'PinguApps.Playbook.deps.json' | ConvertFrom-Json -AsHashtable
        $framework = @($config.includedFrameworks | Where-Object Name -CEQ 'Microsoft.NETCore.App')
        $coreClr = if ($rid.StartsWith('win-')) { 'coreclr.dll' } elseif ($rid.StartsWith('osx-')) { 'libcoreclr.dylib' } else { 'libcoreclr.so' }
        $entryPoint = $settings.DotNetCliTool.Commands.Command.EntryPoint
        if ($config.framework -or $framework.Count -ne 1 -or $framework[0].version -notmatch '^11\.' -or
            $deps.runtimeTarget.name -cne ".NETCoreApp,Version=v11.0/$rid" -or
            -not $deps.libraries.Contains("runtimepack.Microsoft.NETCore.App.Runtime.$rid/$($framework[0].version)") -or
            $settings.DotNetCliTool.Commands.Command.Runner -cne 'executable' -or
            $archive.Entries.Name -cnotcontains $entryPoint -or $archive.Entries.Name -cnotcontains $coreClr) {
          throw "Expected a self-contained .NET 11 payload for $rid."
        }
      }
    }
    [pscustomobject]@{
      Name = $name
      Path = $package.FullName
      Hash = (Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
  }
  finally { $archive.Dispose() }
}
$manifestPath = Join-Path $Artifacts 'package.sha256'
$expected = @($plan | ForEach-Object { "$($_.Hash)  $($_.Name)" })
if ($Record) {
  $expected | Set-Content -LiteralPath $manifestPath -Encoding ascii
}
elseif (((Get-Content -LiteralPath $manifestPath) -join "`n") -cne ($expected -join "`n")) {
  throw 'Package hash or manifest identity mismatch.'
}
if ($PassThru) { $plan }
else { $plan | ForEach-Object { "Verified $($_.Name) SHA-256 $($_.Hash)" } }
