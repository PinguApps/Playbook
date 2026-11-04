param([string]$BaseUrl = 'http://127.0.0.1:5187')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$client = New-Object System.Net.Http.HttpClient
$client.BaseAddress = [Uri]$BaseUrl
$client.Timeout = [TimeSpan]::FromSeconds(10)

function Assert($condition, [string]$message) {
    if (-not $condition) { throw $message }
}
function Post([string]$path) {
    $client.PostAsync($path, $null).GetAwaiter().GetResult().EnsureSuccessStatusCode() | Out-Null
}
function Snapshot([string]$code) {
    $client.GetStringAsync("/control/$code").GetAwaiter().GetResult() | ConvertFrom-Json
}
function Wait-For([scriptblock]$condition) {
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    while (-not (& $condition)) {
        if ([DateTime]::UtcNow -gt $deadline) { throw 'Timed out waiting for instrumentation' }
        Start-Sleep -Milliseconds 25
    }
}
function Reserve([string]$json, [Threading.CancellationToken]$token = [Threading.CancellationToken]::None) {
    $body = New-Object System.Net.Http.StringContent($json, [Text.Encoding]::UTF8, 'application/json')
    return $client.PostAsync('/reservations', $body, $token)
}

try {
    $invalid = @(
        @{ Json = '{}'; Keys = @('Code', 'Guests') },
        @{ Json = '{"guests":1}'; Keys = @('Code') },
        @{ Json = '{"code":null,"guests":1}'; Keys = @('Code') },
        @{ Json = '{"code":"","guests":1}'; Keys = @('Code') },
        @{ Json = '{"code":"   ","guests":1}'; Keys = @('Code') },
        @{ Json = '{"code":"invalid-guests","guests":0}'; Keys = @('Guests') },
        @{ Json = '{"code":"invalid-guests","guests":-1}'; Keys = @('Guests') },
        @{ Json = '{"code":"invalid-guests","guests":5}'; Keys = @('Guests') }
    )
    Post '/control/arm/invalid-guests'
    foreach ($case in $invalid) {
        $response = (Reserve $case.Json).GetAwaiter().GetResult()
        Assert ([int]$response.StatusCode -eq 400) "Expected 400 for $($case.Json)"
        $problem = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
        foreach ($key in $case.Keys) {
            Assert ($null -ne $problem.errors.$key) "Missing error key $key"
        }
    }
    $state = Snapshot 'invalid-guests'
    Assert ($state.entered -eq 0 -and $state.accepted -eq 0) 'Invalid fields reached availability or handler'
    Write-Output 'PASS: required code, guest range, keyed 400 errors, early rejection'

    foreach ($guests in @(1, 4)) {
        $code = "open-$guests"
        Post "/control/arm/$code"
        $pending = Reserve "{`"code`":`"$code`",`"guests`":$guests}"
        Wait-For { (Snapshot $code).entered -eq 1 }
        Start-Sleep -Milliseconds 100
        $state = Snapshot $code
        Assert (-not $pending.IsCompleted) 'Request finished while availability was held'
        Assert ($state.completed -eq 0 -and $state.accepted -eq 0) 'Handler ran before availability'
        Post "/control/release/$code"
        $response = $pending.GetAwaiter().GetResult()
        Assert ([int]$response.StatusCode -eq 204) 'Available reservation did not return 204'
        $state = Snapshot $code
        Assert ($state.completed -eq 1 -and $state.accepted -eq 1) 'Valid reservation was not booked once'
    }
    Write-Output 'PASS: pending checks hold request and handler; guest boundaries return 204 after release'

    Post '/control/arm/closed-hide'
    $pending = Reserve '{"code":"closed-hide","guests":2}'
    Wait-For { (Snapshot 'closed-hide').entered -eq 1 }
    Assert (-not $pending.IsCompleted) 'Closed check did not wait'
    Post '/control/release/closed-hide'
    $response = $pending.GetAwaiter().GetResult()
    Assert ([int]$response.StatusCode -eq 400) 'Closed code did not return 400'
    $problem = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
    Assert ($null -ne $problem.errors.Code) 'Closed error is not keyed by Code'
    Assert ((Snapshot 'closed-hide').accepted -eq 0) 'Closed reservation booked'
    Write-Output 'PASS: closed code awaits check, returns keyed 400, never books'

    Post '/control/arm/cancel-hide'
    $source = New-Object Threading.CancellationTokenSource
    $pending = Reserve '{"code":"cancel-hide","guests":2}' $source.Token
    Wait-For { (Snapshot 'cancel-hide').entered -eq 1 }
    $source.Cancel()
    try { $pending.GetAwaiter().GetResult() | Out-Null; throw 'Canceled request unexpectedly completed' }
    catch { Assert $pending.IsCanceled 'Request did not cancel' }
    Wait-For { (Snapshot 'cancel-hide').canceled -eq 1 }
    Post '/control/release/cancel-hide'
    Start-Sleep -Milliseconds 100
    $state = Snapshot 'cancel-hide'
    Assert ($state.canceled -eq 1 -and $state.completed -eq 0 -and $state.accepted -eq 0) 'Canceled check booked or completed'
    $source.Dispose()
    Write-Output 'PASS: cancellation reaches availability and release cannot book canceled request'
}
finally {
    $client.Dispose()
}
