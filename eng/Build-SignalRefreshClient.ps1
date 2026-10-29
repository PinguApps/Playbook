[CmdletBinding()]
param(
  [string]$Node = (Get-Command node -CommandType Application).Source,
  [string]$SampleRoot = (Split-Path $PSScriptRoot -Parent),
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/identity-tooling/ts')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$canonical = Join-Path $SampleRoot 'samples/net11.0/CopperRidgeHub/TypeScript'
foreach ($file in Get-ChildItem -LiteralPath $canonical -Filter '*.txt' -File) {
  $source = Get-Content -LiteralPath $file.FullName -Raw
  $match = [regex]::Match($source, '(?ms)^// snippet:complete\r?\n(.*?)^// endsnippet:complete\r?$')
  if (-not $match.Success) { throw "Missing canonical body: $($file.Name)" }
  [IO.File]::WriteAllText((Join-Path $OutputDirectory $file.BaseName), $match.Groups[1].Value)
}
$npm = Join-Path (Split-Path $Node) 'node_modules/npm/bin/npm-cli.js'
& $Node $npm ci --prefix $OutputDirectory --ignore-scripts --no-audit --no-fund
& (Join-Path $OutputDirectory 'Build-Client.ps1') -Node $Node
& $Node (Join-Path $OutputDirectory 'node_modules/typescript/bin/tsc') --project (Join-Path $OutputDirectory 'tsconfig.json')
Write-Host "PASS: canonical TypeScript and pinned first-party client built in $OutputDirectory"
