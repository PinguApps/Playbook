$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$base = 'http://127.0.0.1:5187'
$client = New-Object System.Net.Http.HttpClient
$client.Timeout = [TimeSpan]::FromSeconds(10)
function Assert($condition, $message) {
    if (-not $condition) { throw $message }
}
function Post($path) {
    $response = $client.PostAsync("$base$path", $null).GetAwaiter().GetResult()
    Assert $response.IsSuccessStatusCode "Control failed: $path"
    $response.Dispose()
}
function Snapshot($code) {
    $json = $client.GetStringAsync("$base/control/$code").GetAwaiter().GetResult()
    return ($json | ConvertFrom-Json)
}
function Send($json, $token = [Threading.CancellationToken]::None) {
    $content = New-Object System.Net.Http.StringContent($json, [Text.Encoding]::UTF8, 'application/json')
    return $client.PostAsync("$base/reservations", $content, $token)
}
function Wait-Counter($code, $name, $expected) {
    for ($i = 0; $i -lt 100; $i++) {
        $state = Snapshot $code
        if ($state.$name -eq $expected) { return $state }
        Start-Sleep -Milliseconds 30
    }
    throw "Timed out waiting for $code $name=$expected"
}
function Assert-Error($response, $key) {
    Assert ([int]$response.StatusCode -eq 400) "Expected 400 for $key, got $($response.StatusCode)"
    $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
    Assert ($null -ne $body.errors.$key) "Missing error key $key"
    $response.Dispose()
}
$dll = Join-Path $PSScriptRoot 'bin/Release/net11.0/ReservationEvaluation.dll'
$server = Start-Process -FilePath 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -ArgumentList @('exec', ('"' + $dll + '"'), '--urls', $base) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $PSScriptRoot 'checks-server.log') -RedirectStandardError (Join-Path $PSScriptRoot 'checks-server.err')
try {
    $ready = $false
    for ($i = 0; $i -lt 100; $i++) {
        try { $null = Snapshot 'ready'; $ready = $true; break } catch { Start-Sleep -Milliseconds 50 }
    }
    Assert $ready 'Server did not start'

    foreach ($json in @('{"guests":2}', '{"code":null,"guests":2}', '{"code":"","guests":2}', '{"code":"   ","guests":2}')) {
        Assert-Error ((Send $json).GetAwaiter().GetResult()) 'Code'
    }
    Write-Output 'PASS: missing, null, empty and whitespace Code return 400 with Code errors'
    $response = (Send '{}').GetAwaiter().GetResult()
    Assert ([int]$response.StatusCode -eq 400) 'Empty model did not return 400'
    $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
    Assert ($null -ne $body.errors.Code -and $null -ne $body.errors.Guests) 'Empty model must report both fields'
    $response.Dispose()
    Write-Output 'PASS: empty model reports both Code and Guests errors'

    foreach ($guests in @(0, 5, -1)) {
        $code = "size$guests"
        Post "/control/release/$code"
        Assert-Error ((Send ('{"code":"' + $code + '","guests":' + $guests + '}')).GetAwaiter().GetResult()) 'Guests'
        Assert ((Snapshot $code).accepted -eq 0) 'Invalid Guests reached handler'
    }
    Write-Output 'PASS: out-of-range Guests return 400 with Guests errors and never book'

    foreach ($code in @('open-one', 'open-four', 'closed-hide')) {
        Post "/control/arm/$code"
        $guests = if ($code -eq 'open-four') { 4 } else { 1 }
        $pending = Send ('{"code":"' + $code + '","guests":' + $guests + '}')
        $state = Wait-Counter $code 'entered' 1
        Start-Sleep -Milliseconds 150
        Assert (-not $pending.IsCompleted) 'Pending availability completed the request'
        Assert ($state.completed -eq 0 -and $state.accepted -eq 0) 'Handler ran before availability completed'
        Post "/control/release/$code"
        $response = $pending.GetAwaiter().GetResult()
        $state = Snapshot $code
        Assert ($state.completed -eq 1) 'Check did not complete'
        if ($code.StartsWith('closed')) {
            Assert-Error $response 'Code'
            Assert ($state.accepted -eq 0) 'Closed code booked'
        } else {
            Assert ([int]$response.StatusCode -eq 204) 'Valid request did not return 204'
            Assert ($state.accepted -eq 1) 'Valid request did not book exactly once'
            $response.Dispose()
        }
    }
    Write-Output 'PASS: held requests remain unfinished; release yields 204 for sizes 1/4 and 400 for closed Code'

    $code = 'cancel-hide'
    Post "/control/arm/$code"
    $cts = New-Object Threading.CancellationTokenSource
    $pending = Send '{"code":"cancel-hide","guests":2}' $cts.Token
    $null = Wait-Counter $code 'entered' 1
    Assert (-not $pending.IsCompleted) 'Cancellation request did not wait'
    $cts.Cancel()
    try { $null = $pending.GetAwaiter().GetResult(); throw 'Expected cancellation' }
    catch { Assert $pending.IsCanceled 'Request failed without cancellation' }
    $state = Wait-Counter $code 'canceled' 1
    Assert ($state.completed -eq 0 -and $state.accepted -eq 0) 'Canceled check booked'
    Post "/control/release/$code"
    Start-Sleep -Milliseconds 150
    $state = Snapshot $code
    Assert ($state.completed -eq 0 -and $state.accepted -eq 0) 'Release after cancellation booked'
    $cts.Dispose()
    Write-Output 'PASS: request cancellation reaches availability and cannot book, including after release'
} finally {
    $client.Dispose()
    Stop-Process -Id $server.Id -ErrorAction SilentlyContinue
}
