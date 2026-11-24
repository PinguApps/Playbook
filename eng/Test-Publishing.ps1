[CmdletBinding()]
param([string]$Artifacts, [string]$PackageVersion = '1.0.0-preview.1')

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
$rids = @('win-x64', 'win-arm64', 'osx-x64', 'osx-arm64', 'linux-x64', 'linux-arm64', 'any')
$version = '1.0.0-preview.1'
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-publish-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
function Write-FixturePackage([string]$Id, [hashtable]$Entries, [string]$EmbeddedVersion = $version) {
  $archive = [IO.Compression.ZipFile]::Open((Join-Path $scratch "$Id.$version.nupkg"), [IO.Compression.ZipArchiveMode]::Create)
  try {
    $Entries["$Id.nuspec"] = "<package><metadata><id>$Id</id><version>$EmbeddedVersion</version></metadata></package>"
    foreach ($entry in $Entries.GetEnumerator()) {
      $writer = [IO.StreamWriter]::new($archive.CreateEntry($entry.Key).Open())
      try { $writer.Write($entry.Value) } finally { $writer.Dispose() }
    }
  }
  finally { $archive.Dispose() }
}
try {
  $references = foreach ($rid in $rids) {
    "<RuntimeIdentifierPackage RuntimeIdentifier='$rid' Id='PinguApps.Playbook.$rid' />"
    $entries = @{}
    if ($rid -ceq 'any') {
      $entries['DotnetToolSettings.xml'] = "<DotNetCliTool><Commands><Command Name='playbook' /></Commands></DotNetCliTool>"
      $entries['PinguApps.Playbook.runtimeconfig.json'] = '{"runtimeOptions":{"tfm":"net11.0","rollForward":"Major","framework":{"name":"Microsoft.NETCore.App","version":"11.0.0"}}}'
    }
    else {
      $entries['DotnetToolSettings.xml'] = "<DotNetCliTool><Commands><Command Name='playbook' EntryPoint='app' Runner='executable' /></Commands></DotNetCliTool>"
      $entries['PinguApps.Playbook.runtimeconfig.json'] = '{"runtimeOptions":{"tfm":"net11.0","includedFrameworks":[{"name":"Microsoft.NETCore.App","version":"11.0.0"}]}}'
      $entries['PinguApps.Playbook.deps.json'] = (@{
        runtimeTarget = @{ name = ".NETCoreApp,Version=v11.0/$rid" }
        libraries = @{ "runtimepack.Microsoft.NETCore.App.Runtime.$rid/11.0.0" = @{} }
      } | ConvertTo-Json -Depth 4 -Compress)
      $entries['app'] = ''
      $coreClr = if ($rid.StartsWith('win-')) { 'coreclr.dll' } elseif ($rid.StartsWith('osx-')) { 'libcoreclr.dylib' } else { 'libcoreclr.so' }
      $entries[$coreClr] = ''
    }
    Write-FixturePackage "PinguApps.Playbook.$rid" $entries
  }
  $references += "<RuntimeIdentifierPackage RuntimeIdentifier='linux-musl' Id='PinguApps.Playbook.any' />"
  $pointer = @{ 'DotnetToolSettings.xml' = "<DotNetCliTool><Commands><Command Name='playbook' /></Commands><RuntimeIdentifierPackages>$references</RuntimeIdentifierPackages></DotNetCliTool>" }
  Write-FixturePackage 'PinguApps.Playbook' $pointer
  $file = Join-Path $scratch "PinguApps.Playbook.$version.nupkg"
  $manifest = Join-Path $scratch 'package.sha256'
  & $check -Artifacts $scratch -PackageVersion $version -Record | Out-Null
  $plan = & $check -Artifacts $scratch -PackageVersion $version -PassThru
  $expected = @($rids | ForEach-Object { "PinguApps.Playbook.$_.$version.nupkg" }) + "PinguApps.Playbook.$version.nupkg"
  if (($plan.Name -join ',') -cne ($expected -join ',')) { throw 'Expected to publish every payload before the pointer.' }
  Reject { & $check -Artifacts $scratch -PackageVersion '99.0.0' }
  $payload = Join-Path $scratch "PinguApps.Playbook.any.$version.nupkg"
  Move-Item -LiteralPath $payload -Destination "$payload.hidden"
  Reject { & $check -Artifacts $scratch -PackageVersion $version }
  Move-Item -LiteralPath "$payload.hidden" -Destination $payload
  Copy-Item $file (Join-Path $scratch 'unexpected.nupkg')
  Reject { & $check -Artifacts $scratch -PackageVersion $version }
  Remove-Item -LiteralPath (Join-Path $scratch 'unexpected.nupkg')
  $lines = Get-Content -LiteralPath $manifest
  $altered = $lines.Clone()
  $altered[0] = ('0' * 64) + $lines[0].Substring(64)
  $altered | Set-Content -LiteralPath $manifest
  Reject { & $check -Artifacts $scratch -PackageVersion $version }
  # A matching filename/hash cannot disguise a different embedded version.
  Remove-Item -LiteralPath $file
  Write-FixturePackage 'PinguApps.Playbook' $pointer '99.0.0'
  $lines[-1] = "$((Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant())  $(Split-Path $file -Leaf)"
  $lines | Set-Content -LiteralPath $manifest
  Reject { & $check -Artifacts $scratch -PackageVersion $version }
}
finally {
  Get-ChildItem -LiteralPath $scratch -File | Remove-Item
  Remove-Item -LiteralPath $scratch
}
if ($Artifacts) { & $check -Artifacts $Artifacts -PackageVersion $PackageVersion }
'PASS: publication channels, payload-first push plan, complete package set, embedded identities and hashes.'

& (Join-Path $PSScriptRoot 'Test-WeeklyRelease.ps1')
& (Join-Path $PSScriptRoot 'Test-PreviewReadiness.ps1') -GuardsOnly
