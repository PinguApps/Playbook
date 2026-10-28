[CmdletBinding()]
param(
  [string]$InspectionDirectory = (Join-Path $PSScriptRoot '../artifacts/archive-fragments/inspection'),
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-archive-fragment-consumer-' + [guid]::NewGuid().ToString('N'))
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($pair in @(@('blazor.supply-attribute-selected-state','ReedArchive',14), @('blazor.reuse-contravariant-item-fragments','AsterObservatory',10))) {
    $recipe = (Get-Content -LiteralPath (Join-Path $InspectionDirectory "$($pair[0]).json") -Raw | ConvertFrom-Json).result.structuredContent.recipe
    $blocks = [regex]::Matches($recipe.markdown, '(?ms)^### \.NET (10|11): `([^`]+)`\n\n```[^\n]*\n(.*?)^```$')
    if ($blocks.Count -ne $pair[2]) { throw "Incomplete returned consumer: $($pair[0])" }
    foreach ($block in $blocks) {
      $file = $block.Groups[2].Value
      if ($file -notmatch '^[A-Za-z][A-Za-z0-9.-]*$') { throw 'Unexpected consumer path.' }
      $directory = Join-Path $standalone "net$($block.Groups[1].Value).0/$($pair[1])"
      New-Item -ItemType Directory -Path $directory -Force | Out-Null
      [IO.File]::WriteAllText((Join-Path $directory $file), $block.Groups[3].Value)
    }
    foreach ($major in 10,11) {
      $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
      $directory = Join-Path $standalone "net$major.0/$($pair[1])"
      Push-Location -LiteralPath $directory
      try {
        & $dotnet build "$($pair[1]).csproj" -c Release
        & $dotnet "bin/Release/net$major.0/$($pair[1]).dll"
        $sdk = (& $dotnet --version | Out-String).Trim()
      } finally { Pop-Location }
      $sample = "samples/net$major.0/$($pair[1])/$($pair[1]).csproj"
      $evidence = @($recipe.evidence | Where-Object sample -CEQ $sample)
      if ($evidence.Count -ne 1 -or $sdk -cne $evidence[0].sdk) { throw "Wrong SDK: $sample" }
      $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
      if (($dependencies | ConvertTo-Json -Compress) -cne (@($evidence[0].dependencies) | ConvertTo-Json -Compress)) { throw "Dependency closure differs: $sample" }
    }
  }
  Write-Host "PASS: four complete consumers reconstructed only from installed MCP Markdown, compiled and executed with exact SDK/dependency sets in $standalone."
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
