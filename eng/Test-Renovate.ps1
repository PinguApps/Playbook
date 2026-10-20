[CmdletBinding()]
param([string]$RenovateRoot, [switch]$IsolatedNode)

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
  if ($IsolatedNode -or -not $node -or (& $node --version) -notmatch '^v24\.(1[1-9]|[2-9][0-9])\.') {
    # Portable archives need no installer, PATH change or shell integration.
    $nodeVersion = '24.21.0'
    $platform = if ($IsWindows) { 'win' } elseif ($IsLinux) { 'linux' } elseif ($IsMacOS) { 'darwin' } else { throw 'Unsupported Node.js platform.' }
    $architecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
    if ($architecture -notin @('x64', 'arm64')) { throw "Unsupported Node.js architecture: $architecture" }
    $nodeHome = Join-Path ([IO.Path]::GetTempPath()) ('playbook-node-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $nodeHome | Out-Null
    $distribution = "node-v$nodeVersion-$platform-$architecture"
    $extension = if ($IsWindows) { 'zip' } else { 'tar.gz' }
    $archiveName = "$distribution.$extension"
    $downloadRoot = "https://nodejs.org/dist/v$nodeVersion"
    $checksums = (Invoke-WebRequest "$downloadRoot/SHASUMS256.txt").Content
    $checksumLine = @($checksums -split '\r?\n' | Where-Object { $_ -match ('^[a-f0-9]{64}\s+' + [regex]::Escape($archiveName) + '$') })
    if ($checksumLine.Count -ne 1) { throw "Missing unique Node.js checksum for $archiveName" }
    $expectedHash = ($checksumLine[0] -split '\s+')[0]
    $archive = Join-Path $nodeHome $archiveName
    Invoke-WebRequest "$downloadRoot/$archiveName" -OutFile $archive
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ine $expectedHash) { throw 'Node.js archive checksum mismatch.' }
    if ($IsWindows) {
      Expand-Archive -LiteralPath $archive -DestinationPath $nodeHome
      $node = Join-Path $nodeHome "$distribution/node.exe"
    }
    else {
      $tar = (Get-Command tar -CommandType Application).Source
      & $tar -xzf $archive -C $nodeHome
      $node = Join-Path $nodeHome "$distribution/bin/node"
    }
    if ((& $node --version) -ne "v$nodeVersion") { throw 'Unexpected portable Node.js version.' }
  }
  if (-not $RenovateRoot) {
    $tools = Join-Path ([IO.Path]::GetTempPath()) ('playbook-renovate-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $tools | Out-Null
    $npmCli = Join-Path (Split-Path $node) 'node_modules/npm/bin/npm-cli.js'
    if (-not (Test-Path -LiteralPath $npmCli)) {
      $npmCli = Join-Path (Split-Path (Split-Path $node)) 'lib/node_modules/npm/bin/npm-cli.js'
    }
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
