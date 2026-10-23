[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$results = [Collections.Generic.List[object]]::new()
function Assert-True([bool]$Value, [string]$Message) { if (-not $Value) { throw $Message } }
function New-Client([uri]$BaseUri) {
  $handler = [Net.Http.HttpClientHandler]::new()
  $handler.AllowAutoRedirect = $false
  $client = [Net.Http.HttpClient]::new($handler)
  $client.BaseAddress = $BaseUri
  $client.Timeout = [TimeSpan]::FromSeconds(15)
  return $client
}
function Get-Page($Client, [string]$Path) {
  $response = $Client.GetAsync($Path).GetAwaiter().GetResult()
  try {
    Assert-True ($response.IsSuccessStatusCode) "GET $Path returned $($response.StatusCode)"
    return [Net.WebUtility]::HtmlDecode($response.Content.ReadAsStringAsync().GetAwaiter().GetResult())
  } finally { $response.Dispose() }
}
function Post-Form($Client, [string]$Path, [string]$Handler, [string]$Berth) {
  $html = Get-Page $Client $Path
  $token = [regex]::Match($html, 'name="__RequestVerificationToken" value="([^"]+)"').Groups[1].Value
  Assert-True ($token.Length -gt 0) 'Missing antiforgery token'
  $fields = [Collections.Generic.Dictionary[string,string]]::new()
  $fields.Add('_handler', $Handler)
  $fields.Add('__RequestVerificationToken', $token)
  if ($Berth) { $fields.Add('Input.Berth', $Berth) }
  $body = [Net.Http.FormUrlEncodedContent]::new($fields)
  try {
    $response = $Client.PostAsync($Path, $body).GetAwaiter().GetResult()
    try {
      Assert-True ([int]$response.StatusCode -eq 302) "POST $Path must redirect: $($response.StatusCode) $($response.Content.ReadAsStringAsync().GetAwaiter().GetResult())"
      return $response.Headers.Location.ToString()
    } finally { $response.Dispose() }
  } finally { $body.Dispose() }
}
function Post-Control($Client, [string]$Path) {
  $response = $Client.PostAsync($Path, $null).GetAwaiter().GetResult()
  try { $null = $response.EnsureSuccessStatusCode() } finally { $response.Dispose() }
}

foreach ($case in @(
  @{ Name = 'previous'; Target = 'net10.0'; Dotnet = $Net10Dotnet; Port = 5390; SessionTempData = $false; SkipSession = $false },
  @{ Name = 'cookie'; Target = 'net11.0'; Dotnet = $Net11Dotnet; Port = 5391; SessionTempData = $false; SkipSession = $false },
  @{ Name = 'session'; Target = 'net11.0'; Dotnet = $Net11Dotnet; Port = 5392; SessionTempData = $true; SkipSession = $false },
  @{ Name = 'missing-middleware'; Target = 'net11.0'; Dotnet = $Net11Dotnet; Port = 5393; SessionTempData = $false; SkipSession = $true }
)) {
  $sample = Join-Path $root "samples/$($case.Target)/HarbourPermit"
  $artifact = Join-Path $sample "bin/Release/$($case.Target)/HarbourPermit.dll"
  $start = [Diagnostics.ProcessStartInfo]::new($case.Dotnet)
  $start.WorkingDirectory = $sample
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  foreach ($argument in @($artifact, '--urls', "http://127.0.0.1:$($case.Port)", '--environment', 'Development', '--SessionTempData', $case.SessionTempData.ToString(), '--SkipSession', $case.SkipSession.ToString())) { $start.ArgumentList.Add($argument) }
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $process = [Diagnostics.Process]::Start($start)
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  $client = New-Client "http://127.0.0.1:$($case.Port)/"
  $other = New-Client $client.BaseAddress
  try {
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
      try { $runtime = Get-Page $client '/control/runtime'; break }
      catch { if ($process.HasExited -or [DateTime]::UtcNow -gt $deadline) { throw }; Start-Sleep -Milliseconds 100 }
    } while ($true)
    if ($case.SkipSession) {
      $response = $client.GetAsync('/progress').GetAwaiter().GetResult()
      try {
        $errorBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        Assert-True ([int]$response.StatusCode -eq 500 -and $errorBody.Contains('session', [StringComparison]::OrdinalIgnoreCase)) 'Missing middleware must fail rather than silently supply a value'
        $results.Add(@{ case = $case.Name; runtime = $runtime; status = 500; missingSessionFails = $true })
      } finally { $response.Dispose() }
      continue
    }
    $location = Post-Form $client '/apply' 'permit' ''
    Assert-True ($location.EndsWith('/outcome')) 'Wrong outcome redirect'
    $outcomeLocation = $location
    $first = Get-Page $client $location
    $second = Get-Page $client '/outcome'
    Assert-True ($first.Contains('Harbour permit request recorded') -and $second.Contains('No new outcome')) "$($case.Name) TempData must survive the redirect and disappear on second read: FIRST=$first SECOND=$second"
    $location = Post-Form $client '/progress' 'progress' 'North-12'
    $read1 = Get-Page $client $location
    $read2 = Get-Page $client '/review'
    Assert-True ($read1.Contains('Step 1; berth North-12') -and $read2.Contains('Step 1; berth North-12')) 'Session values must survive repeated reads'
    $location = Post-Form $client '/progress' 'progress' 'East-4'
    Assert-True ((Get-Page $client $location).Contains('Step 2; berth East-4')) 'Second step must resume and update persisted progress'
    Assert-True ((Get-Page $other '/review').Contains('Step 0; berth unselected')) 'Independent cookie jar must not inherit progress'
    $streamClient = New-Client $client.BaseAddress
    try {
      Post-Control $client '/control/arm'
      $response = $streamClient.GetAsync('/stream', [Net.Http.HttpCompletionOption]::ResponseHeadersRead).GetAwaiter().GetResult()
      try {
        Assert-True ($response.IsSuccessStatusCode) 'Streaming request failed'
        $cookies = $response.Headers.GetValues('Set-Cookie') -join ';'
        Assert-True ($cookies.Contains('.HarbourPermit.Session')) 'Fresh streaming request must issue the session cookie before its asynchronous work completes'
        $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
        $buffer = [byte[]]::new(16384)
        $count = $stream.ReadAsync($buffer, 0, $buffer.Length).WaitAsync([TimeSpan]::FromSeconds(10)).GetAwaiter().GetResult()
        $initial = [Text.Encoding]::UTF8.GetString($buffer, 0, $count)
        Assert-True ($initial.Contains('Inspection pending') -and -not $initial.Contains('Inspection saved')) 'Expected real initial streamed content before release'
        Post-Control $client '/control/release'
        $reader = [IO.StreamReader]::new($stream)
        $tail = $reader.ReadToEndAsync().GetAwaiter().GetResult()
        Assert-True ($tail.Contains('Inspection saved') -and $tail.Contains('Headers started: True')) 'Late work must run after headers were flushed'
      } finally { $response.Dispose() }
      Assert-True ((Get-Page $streamClient '/review').Contains('Step 7; berth unselected')) 'Late session write must persist with early session establishment'
      $streamOutcome = Get-Page $streamClient '/outcome'
      $expectedOutcome = if ($case.SessionTempData) { 'Dock inspection complete' } else { 'No new outcome' }
      Assert-True ($streamOutcome.Contains($expectedOutcome)) 'Streaming TempData provider behavior differs from documented timing'
      Assert-True ((Get-Page $streamClient '/outcome').Contains('No new outcome')) 'Session-backed TempData must also be one-use'
      if ($case.Target -eq 'net11.0') {
        $cold = New-Client $client.BaseAddress
        try { Assert-True ((Get-Page $cold '/late-session').Contains('new-session-rejected-after-headers')) 'Late new-session establishment should be rejected' }
        finally { $cold.Dispose() }
      }
      $results.Add(@{ case = $case.Name; runtime = $runtime; outcomeRedirect = $outcomeLocation; progressRedirect = $location; tempDataFirstRead = 'Harbour permit request recorded'; tempDataSecondRead = 'No new outcome'; sessionReads = @('Step 1; berth North-12', 'Step 1; berth North-12', 'Step 2; berth East-4'); isolatedSession = $true; streamingInitial = 'Inspection pending'; earlySessionCookie = $true; lateHeadersStarted = $true; streamedSessionRead = 'Step 7; berth unselected'; streamedTempData = $expectedOutcome })
    } finally { $streamClient.Dispose() }
  } finally {
    $client.Dispose(); $other.Dispose()
    if (-not $process.HasExited) { $process.Kill($true) }
    $process.WaitForExit()
    $logs = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
    $logs | Set-Content -LiteralPath (Join-Path ([IO.Path]::GetTempPath()) ("harbour-" + $case.Name + ".log"))
    $process.Dispose()
  }
}
$inputs = foreach ($target in @('net10.0', 'net11.0')) {
  $sample = "samples/$target/HarbourPermit"
  foreach ($file in Get-ChildItem (Join-Path $root $sample) -File -Recurse | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Name -ne 'verification-evidence.json' }) {
    @{ path = [IO.Path]::GetRelativePath($root, $file.FullName).Replace('\', '/'); sha256 = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
  }
  foreach ($extension in @('dll', 'deps.json', 'runtimeconfig.json')) {
    $file = Join-Path $root "$sample/bin/Release/$target/HarbourPermit.$extension"
    @{ path = [IO.Path]::GetRelativePath($root, $file).Replace('\', '/'); sha256 = (Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() }
  }
}
$report = @{ kind = 'real-loopback-http'; recordedAt = [DateTimeOffset]::UtcNow.ToString('O'); checks = $results; inputs = @($inputs); testSha256 = (Get-FileHash $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant() }
if ($OutputPath) { $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath }
Write-Host 'PASS: real redirects/read-once TempData, persistent multi-step session, cookie-jar isolation, held streaming/early cookies, provider timing and missing middleware.'
