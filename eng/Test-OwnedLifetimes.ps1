[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
# Controls deliberately fail compilation; inspect their exact exit codes/diagnostics.
$PSNativeCommandUseErrorActionPreference = $false
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$report = @()
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $source = Join-Path $root "samples/net$major.0/ReadingRoom"
    $control = Join-Path ([IO.Path]::GetTempPath()) ('playbook-lifetime-controls-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $control | Out-Null
    Get-ChildItem -LiteralPath $source -File | Where-Object Name -NotMatch 'evidence\.json$|verification\.json$' | Copy-Item -Destination $control
    Copy-Item -LiteralPath (Join-Path $source 'Before/Unguarded.cs.txt') -Destination (Join-Path $control 'Unguarded.cs')
    Push-Location -LiteralPath $control
    try {
      $before = & $dotnet build ReadingRoom.csproj -c Release -p:RestoreLockedMode=true 2>&1 | Out-String
      $beforeExit = $LASTEXITCODE
      if ($major -eq 11) {
        if ($beforeExit -eq 0 -or $before -notmatch 'error BL0016:') { throw "Missing actual RC2 BL0016: $before" }
      } elseif ($beforeExit -ne 0 -or $before -match 'BL0016') { throw "Unexpected older analyzer outcome: $before" }
      # Any enclosing catch silences this analyzer; it does not prove correct recovery.
      $guarded = (Get-Content -LiteralPath Unguarded.cs -Raw).Replace('=> module.InvokeAsync<string>("readLabel", false);', '{ try { return module.InvokeAsync<string>("readLabel", false); } catch (FormatException) { throw; } }')
      [IO.File]::WriteAllText((Join-Path $control 'Unguarded.cs'), $guarded)
      $irrelevantCatch = & $dotnet build ReadingRoom.csproj -c Release --no-restore 2>&1 | Out-String
      if ($LASTEXITCODE -ne 0 -or $irrelevantCatch -match 'BL0016') { throw "Enclosing-catch control failed: $irrelevantCatch" }
      $hook = @'
using Microsoft.AspNetCore.Components.Forms;
public sealed class HookControl : InputFile
{
    protected override void Dispose(bool disposing) => base.Dispose(disposing);
}
'@
      [IO.File]::WriteAllText((Join-Path $control 'HookControl.cs'), $hook)
      $hookOutput = & $dotnet build ReadingRoom.csproj -c Release --no-restore 2>&1 | Out-String
      $hookExit = $LASTEXITCODE
      if ($major -eq 10) {
        if ($hookExit -eq 0 -or $hookOutput -notmatch 'CS0115') { throw "Older hook availability control failed: $hookOutput" }
      } elseif ($hookExit -ne 0) { throw "Actual RC2 hook unavailable: $hookOutput" }
      $report += @{ target="net$major.0"; before=$before.Trim(); irrelevantCatch=$irrelevantCatch.Trim(); hook=$hookOutput.Trim() }
    } finally { Pop-Location }
    foreach ($name in 'ReadingRoom','RestorationBench') {
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample "samples/net$major.0/$name/$name.csproj" -Dotnet $dotnet -Kind compile
    }
  }
  $output = Join-Path $root 'artifacts/owned-lifetimes/analyzers.json'
  [IO.Directory]::CreateDirectory((Split-Path $output)) | Out-Null
  $report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $output
  Write-Host 'PASS: actual old/current analyzers, enclosing-catch limitation, hook absence/presence and four clean compiled consumers.'
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
