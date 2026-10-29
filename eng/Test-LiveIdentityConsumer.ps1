[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Playwright,
  [Parameter(Mandatory)][string]$Chromium,
  [string]$InspectionDirectory = (Join-Path $PSScriptRoot '../artifacts/identity-tooling/inspection'),
  [string]$Dotnet = (Get-Command dotnet -CommandType Application).Source,
  [string]$Node = (Get-Command node -CommandType Application).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$standalone = Join-Path ([IO.Path]::GetTempPath()) ('playbook-live-identity-consumer-' + [guid]::NewGuid().ToString('N'))
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($pair in @(@('signalr.refresh-live-credentials','CopperRidgeHub'),@('blazor.react-to-refreshed-identity','ArchiveAccess'))) {
    $recipe = (Get-Content -LiteralPath (Join-Path $InspectionDirectory "$($pair[0]).json") -Raw | ConvertFrom-Json).result.structuredContent.recipe
    $blocks = [regex]::Matches($recipe.markdown, '(?ms)^### \.NET (10|11): `([^`]+)`\n\n```[^\n]*\n(.*?)^```$')
    if ($blocks.Count -ne $(if ($pair[1] -eq 'ArchiveAccess') { 18 } else { 12 })) { throw "Incomplete returned consumer: $($pair[0])" }
    foreach ($block in $blocks) {
      $file = $block.Groups[2].Value
      if ($file -notmatch '^[A-Za-z][A-Za-z0-9.-]*$' -or $file.Contains('..')) { throw 'Unexpected returned file path.' }
      $directory = Join-Path $standalone "samples/net$($block.Groups[1].Value).0/$($pair[1])"
      New-Item -ItemType Directory -Path $directory -Force | Out-Null
      [IO.File]::WriteAllText((Join-Path $directory $file), $block.Groups[3].Value)
    }
    foreach ($major in 10,11) {
      $directory = Join-Path $standalone "samples/net$major.0/$($pair[1])"
      Push-Location -LiteralPath $directory
      try {
        & $Dotnet build "$($pair[1]).csproj" -c Release
        $sdk = (& $Dotnet --version | Out-String).Trim()
      } finally { Pop-Location }
      $sample = "samples/net$major.0/$($pair[1])/$($pair[1]).csproj"
      $evidence = @($recipe.evidence | Where-Object { $_.sample -ceq $sample -and $_.kind -in 'compile','compile-and-run' })
      if ($evidence.Count -ne 1 -or $sdk -cne $evidence[0].sdk) { throw "Wrong SDK: $sample" }
      $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
      if ((@($assets.libraries.Keys | Sort-Object -CaseSensitive) | ConvertTo-Json -Compress) -cne (@($evidence[0].dependencies) | ConvertTo-Json -Compress)) { throw "Dependency closure differs: $sample" }
      if ($pair[1] -eq 'CopperRidgeHub') { & $Dotnet (Join-Path $directory "bin/Release/net$major.0/CopperRidgeHub.dll") }
    }
    if ($pair[1] -eq 'CopperRidgeHub') {
      $typescript = [regex]::Matches($recipe.markdown, '(?ms)^### TypeScript: `([^`]+)`\n\n```[^\n]*\n(.*?)^```$')
      if ($typescript.Count -ne 5) { throw 'Incomplete TypeScript preparation.' }
      $directory = Join-Path $standalone 'samples/net11.0/CopperRidgeHub/TypeScript'
      New-Item -ItemType Directory -Path $directory -Force | Out-Null
      foreach ($block in $typescript) {
        $file = $block.Groups[1].Value
        if ($file -notmatch '^[A-Za-z][A-Za-z0-9.-]*$' -or $file.Contains('..')) { throw 'Unexpected TypeScript file path.' }
        [IO.File]::WriteAllText((Join-Path $directory "$file.txt"), "// snippet:complete`n" + $block.Groups[2].Value + "// endsnippet:complete`n")
      }
    }
  }
  & (Join-Path $PSScriptRoot 'Build-SignalRefreshClient.ps1') -Node $Node -SampleRoot $standalone -OutputDirectory (Join-Path $standalone 'artifacts/identity-tooling/ts')
  & $Node (Join-Path $PSScriptRoot 'Test-LiveIdentity.mjs') $Playwright $Chromium $Dotnet (Join-Path $root 'artifacts/identity-tooling/consumer.json') $standalone
  Write-Host "PASS: four actual consumers and TypeScript reconstructed solely from installed MCP Markdown; exact SDK/dependency checks, real .NET/TypeScript hubs and browser circuits. Files: $standalone"
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
