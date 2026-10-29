[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
if (-not $IsWindows -or [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne 'X64') {
  throw 'Capture requires Windows x64 with the native system winsqlite3.dll.'
}
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $sample = "samples/net$major.0/CanalPermits/CanalPermits.csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
    $assets = Get-Content -LiteralPath (Join-Path $root "samples/net$major.0/CanalPermits/obj/project.assets.json") -Raw | ConvertFrom-Json -AsHashtable
    $expected = if ($major -eq 10) { 'lib/net8.0/Microsoft.Data.Sqlite.dll' } else { 'lib/net10.0/Microsoft.Data.Sqlite.dll' }
    $managed = @($assets.targets.Values | ForEach-Object {
      foreach ($entry in $_.GetEnumerator()) { if ($entry.Key -like 'Microsoft.Data.Sqlite.Core/*') { $entry.Value.compile.Keys } }
    } | Sort-Object -Unique)
    if ($managed.Count -ne 1 -or $managed[0] -cne $expected) { throw "Unexpected Core asset: $managed" }
    if (@($assets.libraries.Keys | Where-Object { $_ -like 'SQLite/*' -or $_ -like 'SQLitePCLRaw.lib.*' -or $_ -like '*e_sqlite3*' }).Count) {
      throw 'The system-library route unexpectedly includes a bundled native engine.'
    }
    Write-Host "PASS: $sample selects $expected and the Windows system native provider."
  }

  # Fresh process: Core without a bundle cannot install the provider for the app.
  $directory = Join-Path $root 'samples/net11.0/CanalPermits'
  $dll = Join-Path $directory 'bin/Release/net11.0/CanalPermits.dll'
  $start = [Diagnostics.ProcessStartInfo]::new($Net11Dotnet)
  $start.UseShellExecute = $false
  $start.RedirectStandardOutput = $start.RedirectStandardError = $true
  $start.ArgumentList.Add($dll)
  $start.ArgumentList.Add('--omit-initialization')
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $process = [Diagnostics.Process]::Start($start)
  try {
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    $process.WaitForExit()
    $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    if ($process.ExitCode -eq 0 -or $output -notmatch 'You need to call SQLitePCL.raw.SetProvider') {
      throw "Omitted initialization did not reproduce the expected provider failure: $output"
    }
    . (Join-Path $PSScriptRoot 'Evidence.ps1')
    $record = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json
    $record.kind = 'execution'
    $record.command = "$Net11Dotnet $dll --omit-initialization"
    $record.scope = 'Fresh-process negative control: omitted explicit initialization fails with the real SQLitePCLRaw SetProvider diagnostic; no mock.'
    $record.output = "Exit $($process.ExitCode)`n$output"
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.inputs = Get-EvidenceInputs $root 'samples/net11.0/CanalPermits/CanalPermits.csproj'
    [IO.File]::WriteAllText((Join-Path $directory 'missing-initialization.evidence.json'), ($record | ConvertTo-Json -Depth 30).Replace("`r`n","`n") + "`n")
    Write-Host 'PASS: removing initialization reproduces the actual missing-provider failure.'
  }
  finally { $process.Dispose() }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
}
