[CmdletBinding()]
param(
  [string]$InspectionDirectory = (Join-Path $PSScriptRoot '../artifacts/renderer-navigation/inspection'),
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-blazor-consumer-' + [guid]::NewGuid().ToString('N'))
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($pair in @(@('blazor.allocate-web-renderer-identity','Planetarium',27), @('blazor.preserve-wasm-navigation-state','MineralArchive',22))) {
    $recipe = (Get-Content -LiteralPath (Join-Path $InspectionDirectory "$($pair[0]).json") -Raw | ConvertFrom-Json).result.structuredContent.recipe
    $blocks = [regex]::Matches($recipe.markdown, '(?ms)^### \.NET (10|11): `([^`]+)`\n\n```[^\n]*\n(.*?)^```$')
    if ($blocks.Count -ne $pair[2]) { throw "Incomplete returned consumer: $($pair[0])" }
    foreach ($block in $blocks) {
      $file = $block.Groups[2].Value
      if ($file -notmatch '^(?:wwwroot/)?[A-Za-z][A-Za-z0-9.-]*$') { throw 'Unexpected consumer path.' }
      $directory = Join-Path $standalone "net$($block.Groups[1].Value).0/$($pair[1])"
      New-Item -ItemType Directory -Path (Split-Path (Join-Path $directory $file) -Parent) -Force | Out-Null
      [IO.File]::WriteAllText((Join-Path $directory $file),$block.Groups[3].Value)
    }
    foreach ($major in 10,11) {
      $directory = Join-Path $standalone "net$major.0/$($pair[1])"
      Push-Location -LiteralPath $directory
      try {
        & $Dotnet build "$($pair[1]).csproj" -c Release
        $sdk = (& $Dotnet --version | Out-String).Trim()
      }
      finally { Pop-Location }
      $sample = "samples/net$major.0/$($pair[1])/$($pair[1]).csproj"
      $evidence = @($recipe.evidence | Where-Object sample -CEQ $sample)
      if ($evidence.Count -ne 1 -or $sdk -cne $evidence[0].sdk) { throw "Wrong consumer SDK: $sample" }
      $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      $dependencies = @($assets.libraries.Keys | Sort-Object -CaseSensitive)
      if (($dependencies | ConvertTo-Json -Compress) -cne (@($evidence[0].dependencies) | ConvertTo-Json -Compress)) { throw "Dependency closure differs: $sample" }
    }
  }
  Write-Host "PASS: all four consumers reconstructed only from installed MCP Markdown and compiled with matching SDK/dependencies in $standalone. Browser behavior remains separate."
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
