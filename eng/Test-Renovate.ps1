[CmdletBinding()]
param([string]$RenovateRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in @('Process', 'User', 'Machine')) {
  $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope)
}
$previousGit = $env:PLAYBOOK_GIT_PATH
try {
  $nodeCommand = Get-Command node -CommandType Application -ErrorAction SilentlyContinue
  $node = $nodeCommand.Source
  if (-not $node -or (& $node --version) -notmatch '^v24\.(1[1-9]|[2-9][0-9])\.') {
    throw 'The Renovate policy check requires Node.js 24.11 or later in the Node.js 24 release line.'
  }
  if (-not $RenovateRoot) {
    $tools = Join-Path ([IO.Path]::GetTempPath()) ('playbook-renovate-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $tools | Out-Null
    $npmCli = Join-Path (Split-Path $node) 'node_modules/npm/bin/npm-cli.js'
    if (-not (Test-Path -LiteralPath $npmCli)) {
      # Unix npm launchers resolve to npm-cli.js; Node runs it directly.
      $npmCli = (Get-Command npm -CommandType Application).Source
    }
    & $node $npmCli install --prefix $tools --ignore-scripts --no-audit --no-fund --package-lock=false renovate@44.172.1
    # Download the native artifact directly: installer helpers may fall back to npm rebuild.
    $nativeSetup = @'
import fs from 'node:fs/promises';
import path from 'node:path';
import { createRequire } from 'node:module';
import { brotliDecompressSync } from 'node:zlib';
const re2Root = path.join(process.argv[1], 'node_modules', 're2');
const { version } = JSON.parse(await fs.readFile(path.join(re2Root, 'package.json'), 'utf8'));
if (!/^\d+\.\d+\.\d+$/.test(version)) throw new Error('Unexpected RE2 version.');
const url = `https://github.com/uhop/node-re2/releases/download/${version}/${process.platform}-${process.arch}-${process.versions.modules}.br`;
const response = await fetch(url);
if (!response.ok) throw new Error(`RE2 artifact download failed: ${response.status}`);
const binary = path.join(re2Root, 'build', 'Release', 're2.node');
await fs.mkdir(path.dirname(binary), { recursive: true });
await fs.writeFile(binary, brotliDecompressSync(Buffer.from(await response.arrayBuffer())));
const RE2 = createRequire(path.join(re2Root, 'package.json'))(re2Root);
if (!new RE2('^ready$').test('ready')) throw new Error('RE2 binary verification failed.');
'@
    & $node --input-type=module --eval $nativeSetup $tools
    $RenovateRoot = Join-Path $tools 'node_modules/renovate'
  }
  $RenovateRoot = [IO.Path]::GetFullPath($RenovateRoot)
  $manifest = Get-Content -LiteralPath (Join-Path $RenovateRoot 'package.json') -Raw | ConvertFrom-Json
  if ($manifest.version -ne '44.172.1') { throw 'Use the tested Renovate 44.172.1 installation for these policy checks.' }
  $env:PLAYBOOK_GIT_PATH = (Get-Command git -CommandType Application).Source
  & $node (Join-Path $RenovateRoot 'dist/config-validator.js') --strict --no-global (Join-Path $root 'renovate.json')
  & $node (Join-Path $PSScriptRoot 'Test-Renovate.mjs') $RenovateRoot
}
finally {
  $env:PLAYBOOK_GIT_PATH = $previousGit
  foreach ($scope in @('Process', 'User', 'Machine')) {
    if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) {
      throw "Unexpected $scope PATH change during Renovate validation; inspect shared settings before changing them."
    }
  }
}
