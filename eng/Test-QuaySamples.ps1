[CmdletBinding()]
param(
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Results = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/quay'),
  [int]$StartPort = 6010,
  [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$Results = [IO.Path]::GetFullPath($Results)
New-Item -ItemType Directory -Path $Results -Force | Out-Null
$paths = @{}
foreach ($scope in @('Process','User','Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$owned = @()
$observations = [Collections.Generic.List[object]]::new()
function Check-Post($hostInfo, $path, $origin, $includeToken, $expected, $expectedText = '') {
  $uri = "http://127.0.0.1:$($hostInfo.port)$path"
  $session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
  $page = Invoke-WebRequest $uri -WebSession $session
  $token = [regex]::Match($page.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
  $shouldRenderToken = $hostInfo.name -ne 'automatic11' -or $path -eq '/page'
  if ([bool]$token -ne $shouldRenderToken) { throw "Unexpected token generation: $($hostInfo.name)$path" }
  $body = @{ cargo='lanterns' }
  if ($path -eq '/') { $body = @{ _handler='cargo-permit'; 'Permit.Cargo'='lanterns' } }
  if ($includeToken -and $token) { $body['__RequestVerificationToken'] = [Net.WebUtility]::HtmlDecode($token) }
  $headers = @{}
  if ($origin) { $headers.Origin = $origin }
  $response = Invoke-WebRequest $uri -Method Post -Body $body -Headers $headers -WebSession $session -SkipHttpErrorCheck
  $record = @{ host=$hostInfo.name; path=$path; origin=$origin; sentToken=[bool]($includeToken -and $token); renderedToken=[bool]$token; status=[int]$response.StatusCode; accepted=$response.Content.Contains('accepted: lanterns') }
  $observations.Add($record)
  if ([int]$response.StatusCode -ne $expected -or ($expectedText -and -not $response.Content.Contains($expectedText))) { throw "Unexpected form result: $($record | ConvertTo-Json -Compress) Body=$($response.Content)" }
}
try {
  $definitions = @(
    @{ name='chat10'; major=10; app='QuayChat'; arguments='' },
    @{ name='chat11'; major=11; app='QuayChat'; arguments='' },
    @{ name='tokens10'; major=10; app='QuayPermits'; arguments='' },
    @{ name='automatic11'; major=11; app='QuayPermits'; arguments='' },
    @{ name='tokens11'; major=11; app='QuayPermits'; arguments='--keep-tokens true' },
    @{ name='additional11'; major=11; app='QuayPermits'; arguments='--keep-tokens true --reject-additional true' },
    @{ name='dispatch-source'; major=11; app='QuayPermits'; arguments=''; port=6019 }
  )
  foreach ($definition in $definitions) {
    $port = if($definition.ContainsKey('port')){$definition.port}else{$StartPort + $owned.Count}
    if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) { throw "Port $port occupied; nothing stopped." }
    $directory = Join-Path $root "samples/net$($definition.major).0/$($definition.app)"
    $dll = Join-Path $directory "bin/Release/net$($definition.major).0/$($definition.app).dll"
    $child = Start-Process -FilePath $Dotnet -WorkingDirectory $directory -WindowStyle Hidden -PassThru -ArgumentList "`"$dll`" --urls http://127.0.0.1:$port $($definition.arguments)" -Environment @{ DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'; ASPNETCORE_ENVIRONMENT='Development' } -RedirectStandardOutput (Join-Path $Results "$($definition.name).stdout.log") -RedirectStandardError (Join-Path $Results "$($definition.name).stderr.log")
    $hostInfo = @{ name=$definition.name; pid=$child.Id; port=$port; dll=$dll; sha256=(Get-FileHash $dll).Hash.ToLowerInvariant(); arguments=$definition.arguments }
    $owned += $hostInfo
    $ready = $false
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
      try { $null = Invoke-WebRequest "http://127.0.0.1:$port/" -TimeoutSec 2; $ready=$true } catch { Start-Sleep -Milliseconds 100 }
    } while (-not $ready -and [DateTime]::UtcNow -lt $deadline)
    if (-not $ready) { throw "Host $($definition.name) failed. Inspect $Results." }
    $hostInfo.runtimeModules = @((Get-Process -Id $child.Id).Modules | Where-Object ModuleName -In @('coreclr.dll','hostfxr.dll') | Select-Object ModuleName,FileName)
  }
  foreach ($hostInfo in @($owned | Where-Object name -In @('tokens10','automatic11','tokens11','additional11'))) {
    foreach ($path in @('/','/minimal','/mvc','/page')) {
      $same = "http://127.0.0.1:$($hostInfo.port)"
      $automatic = $hostInfo.name -eq 'automatic11' -and $path -ne '/page'
      $reject = $hostInfo.name -eq 'additional11'
      Check-Post $hostInfo $path $same $false $(if($automatic){200}else{400}) $(if($automatic){'accepted: lanterns'}else{''})
      Check-Post $hostInfo $path 'http://attacker.invalid' $false 400
      if (-not $automatic) {
        Check-Post $hostInfo $path $same $true $(if($reject){400}else{200}) $(if($reject){''}else{'accepted: lanterns'})
        Check-Post $hostInfo $path 'http://attacker.invalid' $true $(if($reject){400}else{200})
      } else {
        Check-Post $hostInfo $path '' $false 200 'accepted: lanterns'
      }
    }
    $json = Invoke-WebRequest "http://127.0.0.1:$($hostInfo.port)/json" -Method Post -ContentType application/json -Body '{"cargo":"lanterns"}' -Headers @{ Origin='http://attacker.invalid' } -SkipHttpErrorCheck
    if ($json.StatusCode -ne 200 -or $json.Content -ne 'JSON accepted: lanterns') { throw 'JSON behavior changed.' }
    $observations.Add(@{ host=$hostInfo.name; path='/json'; origin='http://attacker.invalid'; status=[int]$json.StatusCode })
  }
  $automaticHost = $owned | Where-Object name -EQ 'automatic11'
  foreach ($case in @(
    @{ site='cross-site'; origin="http://127.0.0.1:$($automaticHost.port)"; status=400 },
    @{ site='same-site'; origin="http://127.0.0.1:$($automaticHost.port)"; status=400 },
    @{ site='same-origin'; origin='http://attacker.invalid'; status=200 }
  )) {
    $response = Invoke-WebRequest "http://127.0.0.1:$($automaticHost.port)/minimal" -Method Post -Body @{ cargo='lanterns' } -Headers @{ Origin=$case.origin; 'Sec-Fetch-Site'=$case.site } -SkipHttpErrorCheck
    $observations.Add(@{ host='automatic11'; path='/minimal'; origin=$case.origin; fetchSite=$case.site; status=[int]$response.StatusCode })
    if ($response.StatusCode -ne $case.status) { throw 'Fetch Metadata precedence changed.' }
  }
  foreach ($origin in @('http://localhost:6019','http://attacker.invalid')) {
    $response = Invoke-WebRequest "http://127.0.0.1:$($automaticHost.port)/trusted" -Method Post -Body @{ cargo='lanterns' } -Headers @{ Origin=$origin } -SkipHttpErrorCheck
    $expected = if($origin -eq 'http://localhost:6019'){200}else{400}
    $observations.Add(@{ host='automatic11'; path='/trusted'; origin=$origin; status=[int]$response.StatusCode })
    if ($response.StatusCode -ne $expected) { throw "CORS policy did not select expected origin: $origin" }
  }
  @{ kind='actual-loopback-http'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O'); hosts=$owned; observations=$observations; scope='Real Kestrel form/JSON requests. Origin headers here are synthetic; separate browser receipts establish browser-origin behavior and rendered streaming.' } | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $Results 'http.evidence.json')
  if ($KeepRunning) { $owned | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $Results 'processes.json') }
  Write-Host "PASS: $($observations.Count) real HTTP checks. Browser checks remain separate."
}
catch { foreach ($item in $owned) { Stop-Process -Id $item.pid -ErrorAction SilentlyContinue }; throw }
finally {
  if (-not $KeepRunning) { foreach ($item in $owned) { Stop-Process -Id $item.pid -ErrorAction SilentlyContinue } }
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
