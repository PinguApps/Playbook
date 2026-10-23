[CmdletBinding()]
param([string]$ResponsePath = (Join-Path $PSScriptRoot '../artifacts/pin788/returned.json'))
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$recipe = (Get-Content -LiteralPath $ResponsePath -Raw | ConvertFrom-Json).result.structuredContent.recipe
$markdown = $recipe.markdown
$consumer = Join-Path ([IO.Path]::GetTempPath()) ('playbook-tide-consumer-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $consumer | Out-Null
$matches = [regex]::Matches($markdown, '(?ms)^### (net10\.0|net11\.0|shared)/([^\r\n]+)\r?\n```[^\r\n]*\r?\n(.*?)\r?\n```')
if ($matches.Count -ne 14) { throw "Expected all 14 returned setup files, got $($matches.Count)" }
$report = [ordered]@{ kind = 'returned-body-consumer'; markdownSha256 = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($markdown))); consumer = $consumer; results = @() }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($target in @('net10.0', 'net11.0')) {
    $base = Join-Path $consumer $target
    foreach ($match in $matches) {
      if ($match.Groups[1].Value -notin @('shared', $target)) { continue }
      $path = Join-Path $base $match.Groups[2].Value
      New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
      [IO.File]::WriteAllText($path, $match.Groups[3].Value + "`n")
    }
    Copy-Item -LiteralPath (Join-Path $base 'TideDesk/Directory.Packages.props') -Destination (Join-Path $base 'TideDesk.Client/Directory.Packages.props')
    $sdk = if ($target -eq 'net10.0') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    $dotnet = if ($target -eq 'net10.0') { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
    foreach ($folder in @('TideDesk', 'TideDesk.Client')) {
      [IO.File]::WriteAllText((Join-Path $base "$folder/global.json"), ('{"sdk":{"version":"' + $sdk + '","rollForward":"disable"}}'))
    }
    Push-Location (Join-Path $base 'TideDesk')
    try {
      & $dotnet restore TideDesk.csproj | Write-Host
      & $dotnet build TideDesk.csproj -c Release -p:RestoreLockedMode=true | Write-Host
    }
    finally { Pop-Location }
    $report.results += @{ target = $target; sdk = $sdk; compiled = $true; hostSha256 = (Get-FileHash (Join-Path $base "TideDesk/bin/Release/$target/TideDesk.dll")).Hash.ToLowerInvariant(); clientSha256 = (Get-FileHash (Join-Path $base "TideDesk.Client/bin/Release/$target/TideDesk.Client.dll")).Hash.ToLowerInvariant() }
  }
  $base = Join-Path $consumer 'net11.0/TideDesk'
  $start = [Diagnostics.ProcessStartInfo]::new('C:\Users\pingu\.dotnet-playbook-11\dotnet.exe')
  $start.WorkingDirectory = $base
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  foreach ($argument in @((Join-Path $base 'bin/Release/net11.0/TideDesk.dll'), '--urls', 'http://127.0.0.1:5399', '--environment', 'Development', '--Logging:LogLevel:Default', 'Warning')) { $start.ArgumentList.Add($argument) }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $process = [Diagnostics.Process]::Start($start)
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  try {
    for ($i = 0; $i -lt 100; $i++) {
      try { $null = Invoke-WebRequest 'http://127.0.0.1:5399/server'; break }
      catch { if ($process.HasExited -or $i -eq 99) { throw }; Start-Sleep -Milliseconds 100 }
    }
    $http = Join-Path $root 'artifacts/pin788/consumer-http.evidence.json'
    & (Join-Path $PSScriptRoot 'Test-TideDeskHttp.ps1') -BaseUri 'http://127.0.0.1:5399' -OutputPath $http
    $report.http = Get-Content -LiteralPath $http -Raw | ConvertFrom-Json
  }
  finally {
    if (-not $process.HasExited) { $process.Kill($true) }
    $process.WaitForExit()
    $report.stdout = $stdout.GetAwaiter().GetResult()
    $report.stderr = $stderr.GetAwaiter().GetResult()
    $process.Dispose()
  }
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous }
$report | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $root 'docs/verification/PIN-788.consumer.evidence.json')
Write-Host 'PASS: both returned-body host/client pairs compile outside the checkout; actual RC2 HTTP options match.'
