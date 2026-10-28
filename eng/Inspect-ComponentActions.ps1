[CmdletBinding()]
param(
  [string]$Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Node = 'C:\Program Files\nodejs\node.exe',
  [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
Push-Location -LiteralPath $root
try {
  if (-not $NoBuild) { & (Join-Path $PSScriptRoot 'Test-Smoke.ps1') }
  $package = Join-Path $root 'artifacts/PinguApps.Playbook.0.0.0-smoke.nupkg'
  $sandbox = Join-Path ([IO.Path]::GetTempPath()) ('playbook-component-actions-' + [guid]::NewGuid().ToString('N'))
  $feed = Join-Path $sandbox 'feed'
  New-Item -ItemType Directory -Path $feed -Force | Out-Null
  Copy-Item -LiteralPath $package -Destination $feed
  $config = Join-Path $sandbox 'NuGet.Config'
  '<configuration><packageSources><clear/><add key="local" value="{0}"/></packageSources></configuration>' -f [Security.SecurityElement]::Escape($feed) | Set-Content -LiteralPath $config
  $install = [Diagnostics.ProcessStartInfo]::new($Dotnet)
  $install.WorkingDirectory = $sandbox
  $install.UseShellExecute = $false
  $install.CreateNoWindow = $true
  foreach ($arg in @('tool','install','PinguApps.Playbook','--version','0.0.0-smoke','--tool-path',(Join-Path $sandbox 'tool'),'--configfile',$config,'--no-cache')) { $install.ArgumentList.Add($arg) }
  $install.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $install.Environment['DOTNET_CLI_HOME'] = Join-Path $sandbox 'home'
  $install.Environment['NUGET_PACKAGES'] = Join-Path $sandbox 'cache'
  $process = [Diagnostics.Process]::Start($install)
  $process.WaitForExit()
  if ($process.ExitCode -ne 0) { throw 'Isolated local-only installation failed.' }
  $tool = Join-Path $sandbox $(if ($IsWindows) { 'tool/playbook.exe' } else { 'tool/playbook' })
  # The client launches the installed artifact from this unrelated directory.
  Push-Location -LiteralPath $sandbox
  try { & $Node (Join-Path $PSScriptRoot 'Inspect-ComponentActions.mjs') $tool $root $Dotnet $Net11Dotnet }
  finally { Pop-Location }
  $hash = (Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant()
  Write-Host "PASS: exact package SHA256 $hash; local-only feed/cache, unrelated-directory stdio and returned-body consumers."
}
finally {
  Pop-Location
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
