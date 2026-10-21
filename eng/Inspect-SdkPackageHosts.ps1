[CmdletBinding()]
param([string]$Net11SdkDirectory)

# Read ZIP/XML/PE metadata only. Never load or execute a reviewed assembly.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$manifest = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/sdk-package-hosts.sources.json') -Raw | ConvertFrom-Json
$assets = Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/sdk-package-hosts.assets.json') -Raw | ConvertFrom-Json
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('pin688-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $scratch | Out-Null

function Get-CommunicationSurface($zip) {
  $memory = [IO.MemoryStream]::new()
  $stream = $zip.GetEntry('lib/net8.0/Microsoft.TestPlatform.CommunicationUtilities.dll').Open()
  try { $stream.CopyTo($memory) } finally { $stream.Dispose() }
  $memory.Position = 0
  $pe = [Reflection.PortableExecutable.PEReader]::new($memory)
  try {
    $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
    foreach ($handle in $metadata.TypeDefinitions) {
      $type = $metadata.GetTypeDefinition($handle)
      $name = $metadata.GetString($type.Name)
      if (($type.Attributes -band [Reflection.TypeAttributes]::VisibilityMask) -ne [Reflection.TypeAttributes]::Public) { continue }
      if ($name -notmatch '^(Message$|VersionedMessage$|DefaultTestPlatformContractResolver$|TestCaseConverter$|TestObjectConverter$|TestPlatformContractResolver|TestResultConverter$|TestRunStatisticsConverter$)') { continue }
      $properties = @($type.GetProperties() | ForEach-Object { $metadata.GetString($metadata.GetPropertyDefinition($_).Name) } | Sort-Object)
      $metadata.GetString($type.Namespace) + '.' + $name + ': ' + ($properties -join ',')
    }
  }
  finally { $pe.Dispose(); $memory.Dispose() }
}

try {
  if ($manifest.sources.Count -ne 32 -or @($manifest.sources | Where-Object { $_.id -like 'note-*' }).Count -ne 7) { throw 'Expected 32 sources including seven full notes' }
  if (@($manifest.sources.id | Sort-Object -Unique).Count -ne $manifest.sources.Count) { throw 'Duplicate source ID' }
  if ($assets.packages.Count -ne 23 -or @($assets.packages | ForEach-Object { $_.id + '/' + $_.version } | Sort-Object -Unique).Count -ne 23) { throw 'Expected 23 unique packages' }
  if ($Net11SdkDirectory) {
    foreach ($proof in $assets.inspectedSdk.files) {
      $file = Join-Path $Net11SdkDirectory $proof.file
      if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -cne $proof.sha256) { throw "SDK file mismatch: $($proof.file)" }
    }
    Write-Host "PASS: local SDK $($assets.inspectedSdk.version) recorded file hashes."
  }
  foreach ($source in $manifest.sources) {
    $expectedUrl = "https://raw.githubusercontent.com/$($source.repository)/$($source.commit)/$($source.path)"
    if ($source.commit -notmatch '^[0-9a-f]{40}$' -or $source.url -cne $expectedUrl) { throw "Unpinned source: $($source.id)" }
    $file = Join-Path $scratch ($source.id + '.txt')
    Invoke-WebRequest -Uri $source.url -OutFile $file
    if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $source.sha256) {
      throw "Source hash mismatch: $($source.id)"
    }
    if ($source.id -like 'note-*') {
      $body = Get-Content -LiteralPath $file -Raw
      foreach ($section in @('Version introduced', 'Previous behavior', 'Reason for change', 'Recommended action', 'Affected APIs')) {
        if ($body -notmatch ('(?m)^## ' + [regex]::Escape($section) + '\r?$')) { throw "Missing section: $($source.id) / $section" }
      }
    }
  }
  foreach ($package in $assets.packages) {
    $file = Join-Path $scratch ($package.id + '.' + $package.version + '.nupkg')
    Invoke-WebRequest -Uri $package.url -OutFile $file
    if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $package.sha256) {
      throw "Package hash mismatch: $($package.id) $($package.version)"
    }
    $zip = [IO.Compression.ZipFile]::OpenRead($file)
    try {
      $entry = @($zip.Entries | Where-Object { $_.Name -like '*.nuspec' })
      if ($entry.Count -ne 1) { throw 'Expected one nuspec' }
      $reader = [IO.StreamReader]::new($entry[0].Open())
      try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
      if ($nuspec.package.metadata.id -ine $package.id -or $nuspec.package.metadata.version -cne $package.version) { throw 'Package identity mismatch' }
      $frameworks = @($zip.Entries.FullName | Where-Object { $_ -match '^lib/[^/]+/[^/]+\.dll$' } |
        ForEach-Object { $_.Split('/')[1] } | Sort-Object -Unique)
      if (($frameworks -join '|') -cne ($package.libFrameworks -join '|')) { throw "Framework mismatch: $($package.id)" }
      $dependencies = @($nuspec.SelectNodes('//*[local-name()="dependency"]') |
        ForEach-Object { $_.id + '/' + $_.version } | Sort-Object -Unique)
      if (($dependencies -join '|') -cne ($package.dependencies -join '|')) { throw "Dependency mismatch: $($package.id)" }
      if ($package.id -eq 'Microsoft.TestPlatform.TestHost') {
        $surface = @(Get-CommunicationSurface $zip | Sort-Object)
        if (($surface -join '|') -cne ($package.communicationSurface -join '|')) { throw 'Communication public API mismatch' }
        $surface | ForEach-Object { Write-Host $_ }
      }
      foreach ($proof in $package.entries) {
        $entry = $zip.GetEntry($proof.path)
        if (-not $entry) { throw "Missing entry: $($proof.path)" }
        $stream = $entry.Open()
        try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() }
        finally { $stream.Dispose() }
        if ($hash -cne $proof.sha256) { throw "Entry hash mismatch: $($proof.path)" }
      }
      Write-Host "$($package.id) $($package.version): lib=$($frameworks -join ',')"
    }
    finally { $zip.Dispose() }
  }
  $proposals = @(Get-Content -LiteralPath (Join-Path $root 'docs/planning/audits/sdk-package-hosts.md') |
    Select-String -Pattern '^### [A-I]\. ')
  if ($proposals.Count -ne 9) { throw 'Expected nine approval-only proposals' }
  $proposals | ForEach-Object { Write-Host $_.Line }
  Write-Host "PASS: $($manifest.sources.Count) immutable source hashes, seven full notes, $($assets.packages.Count) package ZIPs and nine approval-only proposals."
}
finally {
  $resolved = [IO.Path]::GetFullPath($scratch)
  if (-not $resolved.StartsWith([IO.Path]::GetTempPath(), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected cleanup path' }
  Remove-Item -LiteralPath $resolved -Recurse -Force
}
