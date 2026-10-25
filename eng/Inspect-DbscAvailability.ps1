[CmdletBinding()]
param(
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/dbsc.replayed.json'),
  [string]$CompareWith
)

# Archive/metadata inspection only. Never load or execute the inspected assembly.
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputPath)
if ($CompareWith -and $output -eq [IO.Path]::GetFullPath($CompareWith)) { throw 'Do not overwrite the comparison baseline.' }
$scratch = [IO.Directory]::CreateTempSubdirectory('playbook-dbsc-').FullName
function Get-Artifact([string]$Url, [int]$ExpectedStatus = 200) {
  $path = Join-Path $scratch ([guid]::NewGuid().ToString('N'))
  $response = Invoke-WebRequest -Uri $Url -OutFile $path -PassThru -SkipHttpErrorCheck
  if ([int]$response.StatusCode -ne $ExpectedStatus) { throw "Expected HTTP $ExpectedStatus from $Url; received $($response.StatusCode)." }
  [pscustomobject]@{ url = $Url; status = [int]$response.StatusCode; path = $path; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() }
}
function Read-Entry($Entry) {
  $reader = [IO.StreamReader]::new($Entry.Open())
  try { $reader.ReadToEnd() } finally { $reader.Dispose() }
}
function Get-DbscSymbols($Entry) {
  $stream = [IO.MemoryStream]::new()
  $inputStream = $Entry.Open()
  try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
  $stream.Position = 0
  $pe = [Reflection.PortableExecutable.PEReader]::new($stream)
  try {
    $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
    foreach ($handle in $metadata.TypeDefinitions) {
      $type = $metadata.GetTypeDefinition($handle)
      $name = "$($metadata.GetString($type.Namespace)).$($metadata.GetString($type.Name))"
      if ($name -match 'DeviceBoundSession') { "type $name" }
    }
    foreach ($handle in $metadata.MethodDefinitions) {
      $method = $metadata.GetMethodDefinition($handle)
      $name = $metadata.GetString($method.Name)
      if ($name -match 'DeviceBoundSession') { "method $name" }
    }
  } finally { $pe.Dispose(); $stream.Dispose() }
}
try {
  $sources = @(
    @{ id = 'cumulative'; url = 'https://raw.githubusercontent.com/dotnet/AspNetCore.Docs/ecf7aca5136aea60aa8db7330f66ac799d3edcf7/aspnetcore/release-notes/aspnetcore-11/includes/device-bound-sessions.md'; pattern = 'AddDeviceBoundSession' },
    @{ id = 'rc1'; url = 'https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc1/aspnetcore.md'; pattern = 'AddDeviceBoundSession' },
    @{ id = 'rc2'; url = 'https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/aspnetcore.md'; pattern = 'removed from this build' },
    @{ id = 'rc1-public-api'; url = 'https://raw.githubusercontent.com/dotnet/aspnetcore/c3325eeb6b47bc6383c127d4f4827dc9642a2b6e/src/Security/Authentication/DeviceBoundSessions/src/PublicAPI.Unshipped.txt'; pattern = 'AddDeviceBoundSession' }
  ) | ForEach-Object {
    $artifact = Get-Artifact $_.url
    if ((Get-Content -LiteralPath $artifact.path -Raw) -notmatch $_.pattern) { throw "Missing expected source evidence: $($_.id)." }
    [ordered]@{ id = $_.id; url = $_.url; sha256 = $artifact.sha256 }
  }
  $removedUrl = 'https://raw.githubusercontent.com/dotnet/aspnetcore/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Security/Authentication/DeviceBoundSessions/src/PublicAPI.Unshipped.txt'
  $removed = Get-Artifact $removedUrl 404
  $packageId = 'microsoft.aspnetcore.authentication.deviceboundsessions'
  $index = Get-Artifact "https://api.nuget.org/v3-flatcontainer/$packageId/index.json"
  $versions = @((Get-Content -LiteralPath $index.path -Raw | ConvertFrom-Json).versions)
  $rc1Version = '0.11.0-rc.1.26425.128'
  if ($versions.Count -ne 1 -or $versions[0] -ne $rc1Version) { throw 'Published DBSC versions changed; reassess availability instead of replaying the old decision.' }
  $missingVersion = '0.11.0-rc.2.26504.105'
  $missing = Get-Artifact "https://api.nuget.org/v3-flatcontainer/$packageId/$missingVersion/$packageId.$missingVersion.nupkg" 404
  $rc1 = Get-Artifact "https://api.nuget.org/v3-flatcontainer/$packageId/$rc1Version/$packageId.$rc1Version.nupkg"
  $zip = [IO.Compression.ZipFile]::OpenRead($rc1.path)
  try {
    [xml]$nuspec = Read-Entry ($zip.Entries | Where-Object FullName -like '*.nuspec')
    $dll = $zip.GetEntry('lib/net11.0/Microsoft.AspNetCore.Authentication.DeviceBoundSessions.dll')
    $symbols = @(Get-DbscSymbols $dll | Sort-Object -Unique)
    if ('method AddDeviceBoundSession' -notin $symbols) { throw 'RC1 positive API control missing.' }
    $rc1Evidence = [ordered]@{
      url = $rc1.url; sha256 = $rc1.sha256; version = [string]$nuspec.package.metadata.version
      repository = [string]$nuspec.package.metadata.repository.url; commit = [string]$nuspec.package.metadata.repository.commit
      dependencies = @($nuspec.package.metadata.dependencies.group.dependency | ForEach-Object { "$($_.id) $($_.version)" })
      asset = $dll.FullName; symbols = $symbols
    }
  } finally { $zip.Dispose() }
  $referenceVersion = '11.0.0-rc.2.26504.105'
  $reference = Get-Artifact "https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.app.ref/$referenceVersion/microsoft.aspnetcore.app.ref.$referenceVersion.nupkg"
  $zip = [IO.Compression.ZipFile]::OpenRead($reference.path)
  try {
    $assemblies = @($zip.Entries | Where-Object FullName -like 'ref/net11.0/*.dll' | Sort-Object FullName)
    if ($assemblies.Count -eq 0) { throw 'No RC2 reference assemblies found.' }
    $matches = @($assemblies | ForEach-Object { Get-DbscSymbols $_ })
    if ($matches.Count -ne 0) { throw "DBSC symbols found in RC2: $($matches -join ', ')" }
    $referenceEvidence = [ordered]@{ url = $reference.url; sha256 = $reference.sha256; version = $referenceVersion; assemblies = @($assemblies.FullName); dbscSymbols = $matches }
  } finally { $zip.Dispose() }
  $report = [ordered]@{
    scope = 'PIN-692 / WEB-071 / identity.bind-session-to-device'; disposition = 'unavailable on pinned RC2; GA not verified'
    sources = @($sources); removedApi = [ordered]@{ url = $removed.url; status = $removed.status }
    packageIndex = [ordered]@{ url = $index.url; versions = $versions }
    missingRc2Package = [ordered]@{ url = $missing.url; status = $missing.status }
    rc1Package = $rc1Evidence; rc2References = $referenceEvidence
    gaRecheck = 'PIN-964 / GA_WEB: actual final artifacts, browser/protocol prerequisites and separately sized original recipe if restored'
  }
  $json = $report | ConvertTo-Json -Depth 12
  if ($CompareWith) {
    $expected = Get-Content -LiteralPath $CompareWith -Raw | ConvertFrom-Json | ConvertTo-Json -Depth 12
    if ($expected -cne $json) { throw 'DBSC evidence changed; inspect the artifacts and disposition.' }
  }
  [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($output)) | Out-Null
  $json | Set-Content -LiteralPath $output -Encoding utf8
  Write-Host "PASS: RC1 positive API control; no RC2 package or DBSC symbols in $($assemblies.Count) reference assemblies; immutable source evidence reconciled."
} finally {
  # Scratch is flat and created by this invocation; never recursively delete a computed path.
  Get-ChildItem -LiteralPath $scratch -File | Remove-Item -Force
  Remove-Item -LiteralPath $scratch
}
