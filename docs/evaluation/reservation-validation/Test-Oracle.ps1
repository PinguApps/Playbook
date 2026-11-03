param(
    [Parameter(Mandatory)][string] $Submission,
    [Parameter(Mandatory)][string] $Dotnet,
    [switch] $Reference
)
$ErrorActionPreference = 'Stop'
$workspace = Join-Path ([IO.Path]::GetTempPath()) ('reservation-oracle-' + [guid]::NewGuid())
$process = $null
$client = $null
$failures = 0
$savedPaths = @{}
foreach ($scope in 'User', 'Machine') { $savedPaths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
function Assert($claim, $condition) {
    if ($condition) { Write-Host "PASS: $claim" }
    else { Write-Host "FAIL: $claim"; $script:failures++ }
}
try {
    New-Item -ItemType Directory $workspace | Out-Null
    $submissionRoot = (Resolve-Path -LiteralPath $Submission).Path
    Get-ChildItem -LiteralPath $submissionRoot -File -Recurse -Force | ForEach-Object {
        $relative = [IO.Path]::GetRelativePath($submissionRoot, $_.FullName)
        if ($relative -match '(^|[\\/])(bin|obj|\.git)([\\/]|$)') { return }
        $destination = Join-Path $workspace $relative
        New-Item -ItemType Directory -Force (Split-Path $destination) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination
    }
    if ($Reference) {
        Copy-Item (Join-Path $PSScriptRoot 'Reference.cs') (Join-Path $workspace 'Reservation.cs')
        $program = Join-Path $workspace 'Program.cs'
        [IO.File]::WriteAllText($program, [IO.File]::ReadAllText($program).Replace(
            'builder.Services.AddSingleton<Availability>();',
            "builder.Services.AddSingleton<Availability>();`nbuilder.Services.AddValidation();"))
    }
    $start = [Diagnostics.ProcessStartInfo]::new($Dotnet)
    $start.WorkingDirectory = $workspace
    $start.UseShellExecute = $false
    $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
    $start.Environment['Logging__LogLevel__Default'] = 'Warning'
    $start.ArgumentList.Add('build')
    $build = [Diagnostics.Process]::Start($start)
    if (!$build.WaitForExit(120000)) { $build.Kill($true); $build.WaitForExit(); throw 'Build exceeded two-minute setup deadline' }
    if ($build.ExitCode -ne 0) { throw 'Submission build failed' }
    # Reserve an ephemeral port; a concurrent claimant causes a visible setup failure.
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $listener.Start(); $port = $listener.LocalEndpoint.Port; $listener.Stop()
    $start.ArgumentList.Clear()
    foreach ($argument in @('run', '--no-build', '--urls', "http://127.0.0.1:$port")) { $start.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::Start($start)
    $client = [Net.Http.HttpClient]::new()
    $client.BaseAddress = "http://127.0.0.1:$port"
    $client.Timeout = [TimeSpan]::FromSeconds(30)
    function Snapshot($code) {
        $response = $client.GetStringAsync("/control/$code").GetAwaiter().GetResult()
        $response | ConvertFrom-Json
    }
    function Control($action, $code) {
        $response = $client.PostAsync("/control/$action/$code", $null).GetAwaiter().GetResult()
        try { $response.EnsureSuccessStatusCode() | Out-Null } finally { $response.Dispose() }
    }
    function Send($code, $guests, $token = [Threading.CancellationToken]::None) {
        $content = [Net.Http.StringContent]::new((@{ code = $code; guests = $guests } | ConvertTo-Json),
            [Text.Encoding]::UTF8, 'application/json')
        $client.PostAsync('/reservations', $content, $token)
    }
    $ready = $false
    for ($i = 0; $i -lt 100; $i++) {
        try { $null = Snapshot 'startup'; $ready = $true; break } catch { Start-Sleep -Milliseconds 50 }
    }
    if (!$ready) { throw 'Server did not start' }
    foreach ($case in @(@('', 2, 'Code'), @('open-size', 0, 'Guests'), @('open-large', 5, 'Guests'))) {
        $code, $guests, $key = $case
        if ($code) { Control arm $code; Control release $code }
        $response = (Send $code $guests).GetAwaiter().GetResult()
        try {
            $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
            Assert "invalid $key returns field error" ($response.StatusCode -eq 400 -and $null -ne $body.errors.$key)
            if ($code) { Assert 'invalid party never enters handler' ((Snapshot $code).accepted -eq 0) }
        } finally { $response.Dispose() }
    }
    foreach ($code in 'open-held', 'closed-held', 'open-cancel') {
        Control arm $code
        $cancellation = [Threading.CancellationTokenSource]::new()
        $pending = Send $code 2 $cancellation.Token
        for ($i = 0; $i -lt 500; $i++) {
            $state = Snapshot $code
            if ($state.entered -gt 0 -or $pending.IsCompleted) { break }
            Start-Sleep -Milliseconds 20
        }
        Assert "$code entered rule" ($state.entered -gt 0)
        Assert "$code awaits before handler" (!$pending.IsCompleted -and $state.completed -eq 0 -and $state.accepted -eq 0)
        if ($code -eq 'open-cancel') {
            $cancellation.Cancel()
            try { $response = $pending.GetAwaiter().GetResult(); $response.Dispose() } catch [OperationCanceledException] { }
            for ($i = 0; $i -lt 500; $i++) {
                $state = Snapshot $code
                if ($state.canceled -gt 0) { break }
                Start-Sleep -Milliseconds 20
            }
            Assert 'request token reaches rule' ($state.canceled -gt 0 -and $state.completed -eq 0 -and $state.accepted -eq 0)
            Control release $code
            Start-Sleep -Milliseconds 100
            Assert 'canceled request never books after release' ((Snapshot $code).accepted -eq 0)
        } else {
            Control release $code
            $response = $pending.GetAwaiter().GetResult()
            try {
                $state = Snapshot $code
                if ($code.StartsWith('closed')) {
                    $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
                    Assert 'unavailable rejected before handler' ($response.StatusCode -eq 400 -and $null -ne $body.errors.Code -and $state.accepted -eq 0 -and $state.completed -gt 0)
                } else {
                    Assert 'valid accepted exactly once after completion' ($response.StatusCode -eq 204 -and $state.completed -gt 0 -and $state.accepted -eq 1)
                }
            } finally { $response.Dispose() }
        }
        Write-Host "OBSERVATION: $code counters $((Snapshot $code) | ConvertTo-Json -Compress)"
        $cancellation.Dispose()
    }
    Write-Host "Oracle: $failures failed"
    if ($failures) { throw "Oracle failed $failures assertions" }
} finally {
    if ($client) { $client.Dispose() }
    if ($process -and !$process.HasExited) { $process.Kill($true); $process.WaitForExit() }
    $resolvedWorkspace = [IO.Path]::GetFullPath($workspace)
    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if (!$resolvedWorkspace.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Cleanup target outside temporary directory' }
    # Child processes can briefly retain assembly handles after the launcher exits.
    for ($attempt = 0; (Test-Path -LiteralPath $resolvedWorkspace) -and $attempt -lt 30; $attempt++) {
        try { Remove-Item -LiteralPath $resolvedWorkspace -Recurse -Force }
        catch {
            if ($attempt -eq 29) { throw }
            Start-Sleep -Milliseconds 100
        }
    }
    foreach ($scope in 'User', 'Machine') {
        if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $savedPaths[$scope]) { Write-Warning "$scope PATH changed unexpectedly; shared setting preserved" }
    }
}
