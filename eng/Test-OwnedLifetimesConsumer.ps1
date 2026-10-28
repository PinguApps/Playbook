[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Playwright,
  [Parameter(Mandatory)][string]$Chromium,
  [string]$InspectionDirectory = (Join-Path $PSScriptRoot '../artifacts/owned-lifetimes/inspection'),
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Node = 'C:\Program Files\nodejs\node.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-owned-lifetimes-' + [guid]::NewGuid().ToString('N'))
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($pair in @(@('blazor.handle-js-invocation-failures','ReadingRoom',24), @('blazor.dispose-input-file-subclass-resources','RestorationBench',20))) {
    $recipe = (Get-Content -LiteralPath (Join-Path $InspectionDirectory "$($pair[0]).json") -Raw | ConvertFrom-Json).result.structuredContent.recipe
    $blocks = [regex]::Matches($recipe.markdown, '(?ms)^### \.NET (10|11): `([^`]+)`\n\n```[^\n]*\n(.*?)^```$')
    if ($blocks.Count -ne $pair[2]) { throw "Incomplete returned consumer: $($pair[0])" }
    foreach ($block in $blocks) {
      $file = $block.Groups[2].Value
      if ($file -notmatch '^[A-Za-z][A-Za-z0-9./-]*$' -or '..' -in $file.Split('/')) { throw 'Unexpected returned consumer path.' }
      $directory = Join-Path $standalone "samples/net$($block.Groups[1].Value).0/$($pair[1])"
      $destination = Join-Path $directory $file
      New-Item -ItemType Directory -Path (Split-Path $destination) -Force | Out-Null
      [IO.File]::WriteAllText($destination, $block.Groups[3].Value)
    }
    foreach ($major in 10,11) {
      $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
      $directory = Join-Path $standalone "samples/net$major.0/$($pair[1])"
      Push-Location -LiteralPath $directory
      try {
        & $dotnet build "$($pair[1]).csproj" -c Release
        $sdk = (& $dotnet --version | Out-String).Trim()
      } finally { Pop-Location }
      $sample = "samples/net$major.0/$($pair[1])/$($pair[1]).csproj"
      $evidence = @($recipe.evidence | Where-Object { $_.sample -ceq $sample -and $_.kind -eq 'compile' })
      if ($evidence.Count -ne 1 -or $sdk -cne $evidence[0].sdk) { throw "Wrong SDK: $sample" }
      $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      if ((@($assets.libraries.Keys | Sort-Object -CaseSensitive) | ConvertTo-Json -Compress) -cne (@($evidence[0].dependencies) | ConvertTo-Json -Compress)) { throw "Dependency closure differs: $sample" }
    }
  }
  & $Node (Join-Path $PSScriptRoot 'Test-OwnedLifetimes.mjs') $Playwright $Chromium $Net10Dotnet $Net11Dotnet (Join-Path $root 'artifacts/owned-lifetimes/consumer-browser.json') $standalone
  Write-Host "PASS: four consumers reconstructed only from installed MCP Markdown; exact SDK/dependency compilation and actual browser/renderer execution. Files: $standalone"
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
