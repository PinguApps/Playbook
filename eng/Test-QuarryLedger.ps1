[CmdletBinding()]
param(
  [string]$Dotnet10 = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Dotnet11 = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/quarry-ledger')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$null = New-Item -ItemType Directory -Force -Path $OutputDirectory
$OutputDirectory = (Resolve-Path $OutputDirectory).Path
$checks = [Collections.Generic.List[object]]::new()
function Assert-Check([bool]$Condition, [string]$Name) {
  if (-not $Condition) { throw "FAIL: $Name" }
  $checks.Add($Name)
}
function New-Client([uri]$BaseUri) {
  $handler = [Net.Http.HttpClientHandler]::new()
  $handler.CookieContainer = [Net.CookieContainer]::new()
  $client = [Net.Http.HttpClient]::new($handler)
  $client.BaseAddress = $BaseUri
  return $client
}
function Read-Page($Client, [string]$Path = '/ledger?shelf=north') {
  $response = $Client.GetAsync($Path).GetAwaiter().GetResult()
  try {
    $html = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    if (-not $response.IsSuccessStatusCode) { throw "HTTP $($response.StatusCode): $html" }
    $stamp = [regex]::Match($html, 'data-stamp="([a-f0-9]+)"').Groups[1].Value
    $survey = [regex]::Match($html, '<section id="survey"[\s\S]*?</section>').Value
    $request = [regex]::Match($html, '<p id="request">(.*?)</p>').Groups[1].Value
    if (-not $stamp -or -not $survey) { throw 'Missing rendered survey' }
    return @{ Stamp = $stamp; Survey = $survey; Request = $request; Html = $html }
  }
  finally { $response.Dispose() }
}
function Post-Control($Client, [string]$Path) {
  $response = $Client.PostAsync($Path, [Net.Http.StringContent]::new('')).GetAwaiter().GetResult()
  try { $null = $response.EnsureSuccessStatusCode() } finally { $response.Dispose() }
}

foreach ($lane in @('older', 'memory', 'hybrid', 'disabled')) {
  $major = if ($lane -eq 'older') { 10 } else { 11 }
  $dotnet = if ($major -eq 10) { $Dotnet10 } else { $Dotnet11 }
  $sample = "samples/net$major.0/QuarryLedger/QuarryLedger.csproj"
  $directory = Join-Path $root "samples/net$major.0/QuarryLedger"
  $dll = Join-Path $directory "bin/Release/net$major.0/QuarryLedger.dll"
  $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
  $listener.Start(); $port = $listener.LocalEndpoint.Port; $listener.Stop()
  $base = [uri]"http://127.0.0.1:$port"
  $info = [Diagnostics.ProcessStartInfo]::new($dotnet)
  $info.WorkingDirectory = $directory
  $info.UseShellExecute = $false
  $info.CreateNoWindow = $true
  $info.RedirectStandardOutput = $true
  $info.RedirectStandardError = $true
  foreach ($arg in @($dll, '--urls', $base.AbsoluteUri, '--environment', 'Development', '--CacheSeconds', '30', '--CacheProvider', $lane, '--CacheEnabled', "$($lane -ne 'disabled')")) { $info.ArgumentList.Add($arg) }
  $info.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $process = [Diagnostics.Process]::Start($info)
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  $mara = New-Client $base; $ivo = New-Client $base; $anonymous = New-Client $base
  $checks.Clear()
  $slidingObservations = [Collections.Generic.List[object]]::new()
  try {
    $ready = $false
    for ($i = 0; $i -lt 80; $i++) {
      if ($process.HasExited) { throw "Server exited: $($stderr.GetAwaiter().GetResult())" }
      try { $null = $anonymous.GetStringAsync('/control/state').GetAwaiter().GetResult(); $ready = $true; break } catch { Start-Sleep -Milliseconds 100 }
    }
    if (-not $ready) { throw 'Server readiness timed out' }
    Post-Control $mara '/control/signin/mara/reader'
    Post-Control $ivo '/control/signin/ivo/reader'
    $a = Read-Page $mara; $b = Read-Page $mara; $c = Read-Page $ivo
    Assert-Check ($a.Survey.Contains('Prepared for mara with reader access.') -and $c.Survey.Contains('Prepared for ivo with reader access.') -and -not $c.Survey.Contains('mara')) "$lane cross-user rendered isolation"
    Assert-Check ($a.Request -ne $b.Request) "$lane uncached outer markup changes"
    $state = $anonymous.GetStringAsync('/control/state').GetAwaiter().GetResult() | ConvertFrom-Json
    if ($lane -in @('memory', 'hybrid')) {
      Assert-Check ($a.Survey -ceq $b.Survey -and $state.Lifecycles -eq 2 -and $state.Builds -eq 2) "$lane hit replays complete markup and skips child lifecycle"
    }
    else {
      Assert-Check ($a.Stamp -ne $b.Stamp -and $state.Lifecycles -eq 3) "$lane child lifecycle runs on every request"
      if ($lane -eq 'older') { Assert-Check ($state.Builds -eq 1) 'older reuses public data while rendering each user freshly' }
    }
    $south = Read-Page $mara '/ledger?shelf=south'
    Assert-Check ($south.Stamp -ne $a.Stamp -and $south.Survey.Contains('south:survey-')) "$lane query variation"
    Post-Control $mara '/control/signin/mara/editor'
    $editor = Read-Page $mara
    Assert-Check ($editor.Stamp -ne $a.Stamp -and $editor.Survey.Contains('editor access.')) "$lane same identity changed permission"
    $guest = Read-Page $anonymous
    Assert-Check ($guest.Survey.Contains('anonymous with none access.') -and -not $guest.Survey.Contains('mara')) "$lane anonymous isolation"
    Post-Control $anonymous '/control/revise'
    $revised = Read-Page $mara
    Assert-Check ($revised.Stamp -ne $editor.Stamp -and $revised.Survey.Contains('data-revision="2"')) "$lane server revision invalidates prior content"
    $ivoRevised = Read-Page $ivo
    Assert-Check ($ivoRevised.Stamp -ne $c.Stamp -and $ivoRevised.Survey.Contains('data-revision="2"')) "$lane revision invalidates other user key"
    if ($lane -in @('memory', 'hybrid')) {
      Post-Control $anonymous '/control/revise'
      $before = $anonymous.GetStringAsync('/control/state').GetAwaiter().GetResult() | ConvertFrom-Json
      $pending = @(1..8 | ForEach-Object { $mara.GetStringAsync('/ledger?shelf=north') })
      $pages = @($pending | ForEach-Object { $_.GetAwaiter().GetResult() })
      $stamps = @($pages | ForEach-Object { [regex]::Match($_, 'data-stamp="([a-f0-9]+)"').Groups[1].Value } | Select-Object -Unique)
      $after = $anonymous.GetStringAsync('/control/state').GetAwaiter().GetResult() | ConvertFrom-Json
      Assert-Check ($stamps.Count -eq 1 -and $after.Builds -eq $before.Builds + 1 -and $after.Lifecycles -eq $before.Lifecycles + 1) "$lane concurrent miss renders exactly once"
      Post-Control $anonymous '/control/revise'
      $mixed = @($mara.GetStringAsync('/ledger'), $ivo.GetStringAsync('/ledger'))
      $mixedMara = $mixed[0].GetAwaiter().GetResult()
      $mixedIvo = $mixed[1].GetAwaiter().GetResult()
      Assert-Check ($mixedMara.Contains('Prepared for mara') -and -not $mixedMara.Contains('Prepared for ivo') -and $mixedIvo.Contains('Prepared for ivo') -and -not $mixedIvo.Contains('Prepared for mara')) "$lane concurrent user keys remain isolated"
      $expiring = Read-Page $anonymous '/expiry'
      $expiryHit = Read-Page $anonymous '/expiry'
      Assert-Check ($expiring.Stamp -eq $expiryHit.Stamp) "$lane absolute expiry immediate hit"
      Start-Sleep -Milliseconds 1300
      $expiryMiss = Read-Page $anonymous '/expiry'
      Assert-Check ($expiring.Stamp -ne $expiryMiss.Stamp) "$lane absolute expiry renders fresh content"
      $unsafeMara = Read-Page $mara '/unsafe'
      $unsafeIvo = Read-Page $ivo '/unsafe'
      Assert-Check ($unsafeMara.Survey -ceq $unsafeIvo.Survey -and $unsafeIvo.Survey.Contains('mara')) "$lane negative fixture detects actual omitted-user leak"
      if ($lane -eq 'hybrid') {
        $response = $anonymous.GetAsync('/sliding').GetAwaiter().GetResult()
        try {
          $errorBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
          Assert-Check ([int]$response.StatusCode -eq 500 -and $errorBody.Contains('ExpiresSliding is not supported')) 'hybrid rejects sliding expiration at real render'
        } finally { $response.Dispose() }
      }
      else {
        $first = Read-Page $anonymous '/sliding'
        $hit = Read-Page $anonymous '/sliding'
        Assert-Check ($first.Stamp -eq $hit.Stamp) 'memory sliding immediate hit'
        Start-Sleep -Milliseconds 1300
        $expired = Read-Page $anonymous '/sliding'
        Assert-Check ($first.Stamp -ne $expired.Stamp) 'memory expires after idle window'
        $clock = [Diagnostics.Stopwatch]::StartNew()
        $last = $expired
        $previousAccess = 0.0
        while ($clock.Elapsed.TotalSeconds -lt 4.6) {
          Start-Sleep -Milliseconds 400
          $last = Read-Page $anonymous '/sliding'
          $elapsed = $clock.Elapsed.TotalSeconds
          if ($elapsed - $previousAccess -ge 1) { throw 'Cannot establish sliding bound: an access gap reached the idle lifetime.' }
          if ($elapsed -lt 3 -and $expired.Stamp -ne $last.Stamp) { throw 'Sliding entry expired despite repeated accesses before its absolute lifetime.' }
          $slidingObservations.Add(@{ seconds = $elapsed; stamp = $last.Stamp })
          $previousAccess = $elapsed
        }
        Assert-Check ($expired.Stamp -ne $last.Stamp) 'memory absolute lifetime bounds repeated sliding hits'
      }
    }
    $artifacts = [ordered]@{}
    foreach ($file in Get-ChildItem (Split-Path $dll) -File | Where-Object { $_.Extension -in @('.dll', '.json') }) {
      $path = [IO.Path]::GetRelativePath($root, $file.FullName).Replace('\', '/')
      $artifacts[$path] = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    [ordered]@{
      kind = 'real loopback HTTP static SSR execution'; lane = $lane; target = "net$major.0"
      recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); arguments = @($info.ArgumentList)
      checks = @($checks); inputs = Get-EvidenceInputs $root $sample; artifacts = $artifacts
      rendered = @{ first = $a; repeated = $b; otherUser = $c; revised = $revised }
      slidingObservations = @($slidingObservations)
      limits = 'Local memory providers on Windows x64; real cookie authentication, no external store, multi-node, interactive rendering or GA proof. Negative fixture intentionally leaks only fictional local test identities.'
    } | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $OutputDirectory "$lane.evidence.json")
    Write-Host "PASS: $lane ($($checks.Count) rendered-content checks)"
  }
  finally {
    $mara.Dispose(); $ivo.Dispose(); $anonymous.Dispose()
    if (-not $process.HasExited) { $process.Kill($true) }
    $process.WaitForExit()
    $stdout.GetAwaiter().GetResult() | Set-Content (Join-Path $OutputDirectory "$lane.stdout.log")
    $stderr.GetAwaiter().GetResult() | Set-Content (Join-Path $OutputDirectory "$lane.stderr.log")
    $process.Dispose()
  }
}
