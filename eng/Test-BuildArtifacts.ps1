[CmdletBinding()]
param([string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe', [string]$Net10Dotnet = (Join-Path $PSScriptRoot '../artifacts/sdk10/dotnet/dotnet.exe'), [switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$paths = @{}
foreach ($scope in 'User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
foreach ($release in '10','11') {
  $sample = "samples/net$release.0/BuildArtifacts/BuildArtifacts.csproj"
  $directory = Join-Path $root (Split-Path $sample)
  $selectedDotnet = if ($release -eq '10') { $Net10Dotnet } else { $Dotnet }
  Push-Location -LiteralPath $directory
  try {
    $sdk = (& $selectedDotnet --version | Out-String).Trim()
    $expected = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
    if ($sdk -cne $expected) { throw "Wrong SDK: $sdk" }
    $output = & $selectedDotnet build -c Release -t:ArchiveDispatch "-p:BuildDotnet=$selectedDotnet" | Out-String
    $original = (Get-FileHash payload/departures.txt).Hash
    if ((Get-FileHash obj/unpacked/payload/departures.txt).Hash -ne $original) { throw 'Payload differs' }
    $entries = & "$env:SystemRoot\System32\tar.exe" -tf obj/dispatch.tar | Out-String
    if ($entries -notmatch 'payload/departures.txt' -or $entries -match 'draft-late|private/crew') { throw "Wrong entries: $entries" }
    # Existing tar replacement and repeated extraction must both succeed.
    $output += & $selectedDotnet build -c Release -t:ArchiveDispatch "-p:BuildDotnet=$selectedDotnet" | Out-String
    $dll = "bin/Release/net$release.0/BuildArtifacts.dll"
    Set-Content obj/unpacked/retained.txt 'Extraction does not mirror'
    & $selectedDotnet $dll extract obj/dispatch.tar obj/unpacked
    $payload = "bin/Release/net$release.0/payload/departures.txt"
    $bytes = [IO.File]::ReadAllBytes((Join-Path $directory $payload))
    try {
      Set-Content $payload 'Changed departure sheet'
      & $selectedDotnet $dll archive "bin/Release/net$release.0" obj/dispatch.tar
      & $selectedDotnet $dll extract obj/dispatch.tar obj/unpacked
      if ((Get-FileHash $payload).Hash -ne (Get-FileHash obj/unpacked/payload/departures.txt).Hash) { throw 'Changed payload differs' }
      if (-not (Test-Path obj/unpacked/retained.txt)) { throw 'Extraction removed unrelated file' }
    } finally { [IO.File]::WriteAllBytes((Join-Path $directory $payload),$bytes) }
    $globs = & $selectedDotnet build -t:ExplainDispatchInputs | Out-String
    foreach ($pattern in 'payload/**/*.txt','payload/private/**','payload/**/draft-*.txt') {
      if (-not $globs.Contains($pattern)) { throw "Missing pattern $pattern" }
    }
    if ($globs -notmatch 'Expanded files=payload[/\\]departures.txt\s*\r?\n') { throw "Wrong expanded files: $globs" }
    if ($release -eq '11') {
      $unset = & $selectedDotnet build -t:ExplainDispatchInputs '-p:MSBuildProvideItemGlobs=' | Out-String
      if ($unset.Contains('include=payload/')) { throw 'Unset property still reports globs' }
    }
    if ($Record) {
      $receipt = Get-Content verification-evidence.json -Raw | ConvertFrom-Json -AsHashtable
      $receipt.kind = 'compile-and-run'
      $receipt.inputs = Get-EvidenceInputs $root $sample
      $receipt.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $receipt.sourceCommit = (& 'C:\Program Files\Git\cmd\git.exe' -C $root rev-parse HEAD).Trim()
      $receipt.sourceDirty = $true
      $receipt.command = "eng/Test-BuildArtifacts.ps1 -Dotnet '$Dotnet' -Record"
      $receipt.scope = 'Windows canonical build and TarFile helper: tar entries, payload bytes, existing archive replacement, repeated extraction, changed file, retained unrelated destination file; authored patterns and excluded/draft controls. No inbox task success, imported graph, Framework MSBuild, reproducibility or hostile archive proof.'
      $receipt.output = $output + $globs + "`nPASS: entries, bytes, replacement, repeated/changed extraction, retained file and glob controls"
      $receipt.sdkInfo = (& $selectedDotnet --info | Out-String).Trim()
      $receipt | ConvertTo-Json -Depth 40 | Set-Content verification-evidence.json
    }
  } finally { Pop-Location }
}
foreach ($scope in 'User','Machine') { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "$scope PATH changed" } }
Write-Host 'PASS: older/current archive and glob behavior; saved PATH unchanged'
