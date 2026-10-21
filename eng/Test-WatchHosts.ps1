[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'dotnet',
  [string]$Net11Dotnet = 'dotnet',
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$Net10Dotnet = (Get-Command $Net10Dotnet -CommandType Application | Select-Object -First 1).Source
$Net11Dotnet = (Get-Command $Net11Dotnet -CommandType Application | Select-Object -First 1).Source
foreach ($release in @('10', '11')) {
  $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
  $directory = Join-Path $root "samples/net$release.0/WatchAppHost"
  $sample = "samples/net$release.0/WatchAppHost/WatchAppHost.csproj"
  $before = Get-EvidenceInputs $root $sample
  $start = [Diagnostics.ProcessStartInfo]::new($dotnet)
  $start.WorkingDirectory = $directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  foreach ($argument in @('watch', '--project', 'WatchAppHost.csproj', '--non-interactive', '--no-hot-reload', 'run', '-c', 'Debug', '--no-launch-profile')) { $start.ArgumentList.Add($argument) }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DEPOT_DOTNET'] = $dotnet
  $start.Environment['DOTNET_HOST_PATH'] = $dotnet
  $start.Environment['Configuration'] = 'Release'
  $start.Environment['ASPIRE_ALLOW_UNSECURED_TRANSPORT'] = 'true'
  $start.Environment['ASPNETCORE_URLS'] = 'http://127.0.0.1:0'
  $start.Environment['DOTNET_DASHBOARD_OTLP_ENDPOINT_URL'] = 'http://127.0.0.1:0'
  $start.Environment['DOTNET_RESOURCE_SERVICE_ENDPOINT_URL'] = 'http://127.0.0.1:0'
  $process = [Diagnostics.Process]::Start($start)
  $stderr = $process.StandardError.ReadToEndAsync()
  $lines = [Collections.Generic.List[string]]::new()
  $started = $false
  $debug = $false
  $deadline = [DateTimeOffset]::UtcNow.AddSeconds(45)
  try {
    while ([DateTimeOffset]::UtcNow -lt $deadline) {
      $next = $process.StandardOutput.ReadLineAsync()
      if (-not $next.Wait(10000)) { break }
      $line = $next.GetAwaiter().GetResult()
      if ($null -eq $line) { break }
      $lines.Add($line)
      if ($line.Contains('watch-configuration=Debug')) { $debug = $true }
      if ($line.Contains('Distributed application started.') -or $line.Contains('Press CTRL+C to stop the AppHost and exit.')) { $started = $true; break }
    }
  }
  finally {
    if (-not $process.HasExited) { $process.Kill($true) }
    $process.WaitForExit()
    $lines.Add($stderr.GetAwaiter().GetResult())
    $log = [regex]::Replace(($lines -join "`n"), '(?<=/login\?t=)[^\s]+', '[redacted]')
    $log | Write-Host
    [IO.File]::WriteAllText((Join-Path $root "artifacts/watch-$release.log"), $log)
  }
  if (-not $started -or -not $debug) { throw "SDK $release watch host did not start in Debug; see artifacts/watch-$release.log." }
  if ($Record) {
    $after = Get-EvidenceInputs $root $sample
    if ((ConvertTo-Json $before -Compress) -cne (ConvertTo-Json $after -Compress)) { throw 'Host inputs changed during watch execution.' }
    $evidence = Get-Content -LiteralPath (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
    $evidence.kind = 'execution'
    $evidence.inputs = $after
    $evidence.command = "$dotnet watch --project WatchAppHost.csproj --non-interactive --no-hot-reload run -c Debug --no-launch-profile"
    $evidence.scope = 'Real Windows watch startup with pinned Aspire/DCP assets; not mobile, UI, resource hot reload or GA proof.'
    $evidence.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $evidence.output = $log
    [IO.File]::WriteAllText((Join-Path $directory 'watch.evidence.json'), (ConvertTo-Json $evidence -Depth 30).Replace("`r`n", "`n") + "`n")
  }
  Write-Host "PASS: SDK $release watch starts the real pinned Aspire host in Debug; no device or hot-reload claim."
}
