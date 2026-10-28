[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Chromium,
  [string]$InspectionDirectory = (Join-Path $PSScriptRoot '../artifacts/renderer-payload/inspection'),
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Node = (Get-Command node -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-renderer-payload-' + [guid]::NewGuid().ToString('N'))
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($pair in @(@('blazor.update-staged-render-attribute','MossGallery',10), @('blazor.configure-circuit-json-metadata','CedarPlanetarium',15))) {
    $recipe = (Get-Content -LiteralPath (Join-Path $InspectionDirectory "$($pair[0]).json") -Raw | ConvertFrom-Json).result.structuredContent.recipe
    $blocks = [regex]::Matches($recipe.markdown, '(?ms)^### \.NET (10|11): `([^`]+)`\n\n```[^\n]*\n(.*?)^```$')
    if ($blocks.Count -ne $pair[2]) { throw "Incomplete returned consumer: $($pair[0]) ($($blocks.Count) files)" }
    foreach ($block in $blocks) {
      $file = $block.Groups[2].Value
      if ($file -notmatch '^[A-Za-z][A-Za-z0-9./-]*$' -or '..' -in $file.Split('/')) { throw 'Unexpected consumer path.' }
      $directory = Join-Path $standalone "net$($block.Groups[1].Value).0/$($pair[1])"
      $destination = Join-Path $directory $file
      New-Item -ItemType Directory -Path (Split-Path $destination) -Force | Out-Null
      [IO.File]::WriteAllText($destination, $block.Groups[3].Value)
    }
    foreach ($major in 10,11) {
      $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
      $directory = Join-Path $standalone "net$major.0/$($pair[1])"
      Push-Location -LiteralPath $directory
      try {
        & $dotnet build "$($pair[1]).csproj" -c Release
        if ($pair[1] -eq 'MossGallery') { & $dotnet "bin/Release/net$major.0/MossGallery.dll" }
        $sdk = (& $dotnet --version | Out-String).Trim()
      } finally { Pop-Location }
      $sample = "samples/net$major.0/$($pair[1])/$($pair[1]).csproj"
      $evidence = @($recipe.evidence | Where-Object { $_.sample -ceq $sample -and $_.kind -in 'compile','compile-and-run' })
      if ($evidence.Count -ne 1 -or $sdk -cne $evidence[0].sdk) { throw "Wrong SDK: $sample" }
      $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      if ((@($assets.libraries.Keys | Sort-Object -CaseSensitive) | ConvertTo-Json -Compress) -cne (@($evidence[0].dependencies) | ConvertTo-Json -Compress)) { throw "Dependency closure differs: $sample" }
    }
  }
  $entry = Join-Path $root 'artifacts/cedar-payload/browser-tools/node_modules/playwright-core/index.mjs'
  & $Node (Join-Path $PSScriptRoot 'Test-CedarPayload.mjs') $entry $Chromium $Net10Dotnet $Net11Dotnet (Join-Path $root 'artifacts/renderer-payload/consumer-browser.json') $standalone
  Write-Host "PASS: four standalone consumers reconstructed only from installed MCP Markdown, exact SDK/dependency compilation and actual renderer/browser execution. Files: $standalone"
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
