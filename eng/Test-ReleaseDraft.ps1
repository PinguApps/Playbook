[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in @('Process', 'User', 'Machine')) {
  $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope)
}
try {
  $node = (Get-Command node -CommandType Application | Select-Object -First 1).Source
  if ((& $node --version) -notmatch '^v24\.') { throw 'Use Node.js 24 to execute the pinned upstream TypeScript.' }
  $workflow = Get-Content (Join-Path $root '.github/workflows/release-drafter.yml') -Raw
  if ($workflow -notmatch 'release-drafter/release-drafter@([a-f0-9]{40})') { throw 'Missing Release Drafter SHA pin.' }
  $pin = $Matches[1]
  $fixture = Join-Path $root "artifacts/release-drafter/$pin"
  $source = Join-Path $fixture "release-drafter-$pin"
  New-Item -ItemType Directory -Path $fixture -Force | Out-Null
  if (-not (Test-Path "$source/packages/core/package.json")) {
    $archive = Join-Path $fixture 'source.zip'
    Invoke-WebRequest "https://codeload.github.com/release-drafter/release-drafter/zip/$pin" -OutFile $archive
    Expand-Archive -LiteralPath $archive -DestinationPath $fixture -Force
  }
  # Fixture dependencies stay outside production CPM and Renovate permissions.
  $package = Get-Content "$source/packages/core/package.json" -Raw | ConvertFrom-Json
  @{ private = $true; type = 'module'; dependencies = $package.dependencies } |
    ConvertTo-Json -Depth 8 | Set-Content (Join-Path $fixture 'package.json')
  $npm = Join-Path (Split-Path $node) 'node_modules/npm/bin/npm-cli.js'
  if (-not (Test-Path $npm)) { throw 'Node.js must include npm beside its executable.' }
  & $node $npm install --prefix $fixture --ignore-scripts --no-audit --no-fund
  & $node --experimental-transform-types (Join-Path $PSScriptRoot 'Test-ReleaseDraft.mjs') $source
}
finally {
  foreach ($scope in @('Process', 'User', 'Machine')) {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $before[$scope]) {
      throw "PATH changed at $scope scope; inspect shared settings before proceeding."
    }
  }
}
