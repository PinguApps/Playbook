[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Recipe,
  [Parameter(Mandatory)][string]$OutputDirectory,
  [Parameter(Mandatory)][string]$DotNet10,
  [Parameter(Mandatory)][string]$DotNet11
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
foreach ($executable in @($DotNet10, $DotNet11)) {
  if (-not [IO.Path]::IsPathFullyQualified($executable) -or -not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'Supply absolute paths to the installed .NET 10 and .NET 11 executables.' }
}
$response = Get-Content -LiteralPath $Recipe -Raw | ConvertFrom-Json
$body = $response.markdown
$blocks = [regex]::Matches($body, '(?ms)^```[^\n]*\n(.*?)^```$')
if ($blocks.Count -ne 12) { throw 'Expected all twelve complete canonical code/configuration blocks.' }
$pins = [regex]::Matches($body, 'global.json containing exactly `([^`]+)`')
if ($pins.Count -ne 2) { throw 'Expected both actual standalone SDK pins.' }
$imports = [regex]::Matches($body, '`(using [^`;]+;)`')
if ($imports.Count -ne 3) { throw 'Expected all three Program.cs imports from the returned body.' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'Use a fresh unrelated reconstruction directory.' }
$before = @{}
foreach ($scope in @('Process', 'User', 'Machine')) { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = @{}
foreach ($key in @('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH', 'DOTNET_GENERATE_ASPNET_CERTIFICATE', 'DOTNET_SKIP_FIRST_TIME_EXPERIENCE')) { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
$receipt = [ordered]@{ recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); recipeSha256 = (Get-FileHash -LiteralPath $Recipe -Algorithm SHA256).Hash.ToLowerInvariant(); verifierSha256 = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant(); scope = 'Actual standalone builds from returned expanded stdio Markdown only. No canonical checkout application/configuration source copied. Browser execution recorded separately.'; projects = @() }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
  foreach ($major in @(10, 11)) {
    $directory = Join-Path $output "net$major.0"
    New-Item -ItemType Directory -Path (Join-Path $directory 'wwwroot') -Force | Out-Null
    $offset = if ($major -eq 10) { 0 } else { 2 }
    [IO.File]::WriteAllText((Join-Path $directory 'global.json'), $pins[$major - 10].Groups[1].Value + "`n")
    [IO.File]::WriteAllText((Join-Path $directory 'TernLedger.csproj'), $blocks[$offset].Groups[1].Value)
    $program = ($imports | ForEach-Object { $_.Groups[1].Value }) -join "`n"
    [IO.File]::WriteAllText((Join-Path $directory 'Program.cs'), $program + "`n" + $blocks[$offset + 1].Groups[1].Value)
    $files = @('_Imports.razor', 'App.razor', 'Routes.razor', 'Home.razor', 'Ledger.razor', 'wwwroot/tern.js', 'wwwroot/ledger-module.js', 'wwwroot/ledger-data.json')
    for ($index = 0; $index -lt $files.Count; $index++) { [IO.File]::WriteAllText((Join-Path $directory $files[$index]), $blocks[4 + $index].Groups[1].Value) }
    $dotnet = if ($major -eq 10) { $DotNet10 } else { $DotNet11 }
    Push-Location $directory
    try {
      $sdk = (& $dotnet --version | Out-String).Trim()
      $build = (& $dotnet build TernLedger.csproj -c Release | Out-String).Trim()
      Write-Host $build
      $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
      $declaration = @($response.evidence | Where-Object target -CEQ "net$major.0")
      $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
      if ($declaration.Count -ne 1 -or $sdk -cne $declaration[0].sdk -or ($dependencies | ConvertTo-Json -Compress) -cne (@($declaration[0].dependencies) | ConvertTo-Json -Compress)) { throw 'Reconstructed SDK/dependency closure differs from the served evidence.' }
    } finally { Pop-Location }
    $inputs = [ordered]@{}
    $artifacts = [ordered]@{}
    foreach ($file in Get-ChildItem -LiteralPath $directory -Recurse -File | Sort-Object FullName) {
      $relative = [IO.Path]::GetRelativePath($directory, $file.FullName).Replace('\', '/')
      $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
      if ($relative.StartsWith('bin/', [StringComparison]::Ordinal)) { $artifacts[$relative] = $hash }
      elseif (-not $relative.StartsWith('obj/', [StringComparison]::Ordinal)) { $inputs[$relative] = $hash }
    }
    $receipt.projects += @{ directory = $directory; sdk = $sdk; target = "net$major.0"; dependencies = $dependencies; build = $build; inputs = $inputs; artifacts = $artifacts }
  }
  $receipt | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath (Join-Path $output 'consumer.evidence.json')
} finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings." } }
}
Write-Host "PASS: both complete standalone consumers reconstructed and compiled in $output."
