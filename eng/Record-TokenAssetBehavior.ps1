[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Playwright,
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Node = 'C:\Program Files\nodejs\node.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$records = @()
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($version in '10.0','11.0') {
    foreach ($name in 'HydrophoneArchive','MaritimeGallery') {
      $sample = "samples/net$version/$name/$name.csproj"
      $record = Get-Content -LiteralPath (Join-Path $root "samples/net$version/$name/verification-evidence.json") -Raw | ConvertFrom-Json -AsHashtable
      $null = Read-VerificationEvidence $root @{ sample=$sample; target="net$version"; sdk=$record.sdk; language=$record.language; kind='compile'; scope='Matching target prerequisite.' }
      $records += $record
    }
  }
  # Preserve the actual RC2 source-generator limitation as a negative compiler control.
  $gallery11 = Join-Path $root 'samples/net11.0/MaritimeGallery'
  Push-Location -LiteralPath $gallery11
  try {
    $negativeCommand = "$Net11Dotnet build -c Release --no-restore -p:UseRazorSourceGenerator=true"
    $negativeOutput = & $Net11Dotnet build -c Release --no-restore -p:UseRazorSourceGenerator=true | Out-String
    $generated = Get-ChildItem -LiteralPath (Join-Path $gallery11 'obj/Release/net11.0/generated') -Recurse -Filter 'App_razor.g.cs' | Select-Object -First 1
    $code = Get-Content -LiteralPath $generated.FullName -Raw
    if ($code -notmatch 'Assets\[@"images/sounding.svg"\]' -or
        $code -notmatch 'museum-image[^\r\n]*source=(?:\\"|"")~/images/sounding.svg' -or
        $code -notmatch '<img[^\r\n]*src=(?:\\"|"")~/images/sounding.svg') {
      throw 'RC2 source-generator control changed: reconcile the route before recapturing evidence.'
    }
    $negativeOutput += "`nPASS: component parameter expands; custom and built-in HTML remain literal ~/ paths with source generation."
  }
  finally { Pop-Location }
  $publishOutput = @{}
  foreach ($version in '10.0','11.0') {
    $dotnet = if ($version -eq '10.0') { $Net10Dotnet } else { $Net11Dotnet }
    Push-Location -LiteralPath (Join-Path $root "samples/net$version/MaritimeGallery")
    try {
      $destination = Join-Path $root "artifacts/token-assets/gallery-$version"
      $publishOutput[$version] = & $dotnet publish -c Release --no-restore -o $destination | Out-String
    }
    finally { Pop-Location }
  }
  $reportPath = Join-Path $root 'artifacts/token-assets/behavior.json'
  & $Node (Join-Path $PSScriptRoot 'Test-TokenAssetConsumers.mjs') $Playwright $Net10Dotnet $Net11Dotnet $reportPath
  $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
  foreach ($record in $records) {
    $directory = Split-Path (Join-Path $root $record.sample)
    $name = Split-Path $directory -Leaf
    $version = $record.target.Substring(3)
    $execution = @($report.results | Where-Object { $_.version -eq $version -and $_.sample -eq $name })
    if ($execution.Count -ne 1) { throw "Missing actual execution: $($record.sample)" }
    $after = Get-EvidenceInputs $root $record.sample
    if (($after | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during actual execution.' }
    $dotnet = if ($version -eq '10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $sdkDirectory = Join-Path (Split-Path $dotnet -Parent) "sdk/$($record.sdk)"
    $compilerFiles = @('Sdks/Microsoft.NET.Sdk.Razor/source-generators/Microsoft.CodeAnalysis.Razor.Compiler.dll')
    if ($name -eq 'MaritimeGallery' -and $version -eq '11.0') { $compilerFiles += 'Sdks/Microsoft.NET.Sdk.Razor/tools/Microsoft.CodeAnalysis.Razor.Compiler.dll' }
    $tooling = foreach ($relative in $compilerFiles) {
      $file = Join-Path $sdkDirectory $relative
      if (-not (Test-Path -LiteralPath $file)) { throw "Missing measured compiler: $file" }
      "$file | $([Diagnostics.FileVersionInfo]::GetVersionInfo($file).ProductVersion) | SHA256 $((Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant())"
    }
    $record.kind = 'execution'
    $record.command = "& '$PSScriptRoot/Record-TokenAssetBehavior.ps1' -Playwright '$Playwright' -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Node '$Node'"
    $record.scope = if ($name -eq 'HydrophoneArchive') { 'Real WASM token-result consumers and browser history state; controlled IAccessTokenProvider boundary only. No provider issuance or authorized API/download claim.' } else { 'Actual generated C#, restored dependencies, published endpoint manifest, non-root fingerprinted asset HTTP retrieval and literal/unknown controls. RC2 task route; source-generator limitation retained.' }
    $record.output = "PASS: Chromium $($report.browser)`n" + ($execution[0] | ConvertTo-Json -Depth 20) + "`n" + ($tooling -join "`n")
    if ($name -eq 'MaritimeGallery') {
      $record.output += "`n$($publishOutput[$version])"
      if ($version -eq '11.0') { $record.output += "`n$negativeCommand`n$negativeOutput" }
    }
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.sourceCommit = (& (Get-Command git -CommandType Application).Source -C $root rev-parse HEAD).Trim()
    $record.sourceDirty = [bool](& (Get-Command git -CommandType Application).Source -C $root status --porcelain)
    [IO.File]::WriteAllText((Join-Path $directory 'behavior.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
  Write-Host 'PASS: four fresh real execution records; compiler pins and source-generator limitation retained.'
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
