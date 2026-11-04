# Check the retained observations. Never launch or replace a scored attempt.
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$receipts = Get-Content -LiteralPath (Join-Path $root 'sha256.json') -Raw | ConvertFrom-Json
foreach ($receipt in $receipts) {
    $path = [IO.Path]::GetFullPath((Join-Path $root $receipt.path))
    if (!$path.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Receipt path escapes the evidence bundle'
    }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -cne $receipt.sha256) {
        throw "Changed evidence: $($receipt.path)"
    }
}
$threads = @()
$elapsed = 0.0
foreach ($condition in @('without', 'with')) {
    $directory = Join-Path $root "csharp/$condition"
    $timing = Get-Content -LiteralPath "$directory/scored-$condition.timing.json" -Raw | ConvertFrom-Json
    if ($timing.deadlineSeconds -ne 600 -or $timing.elapsedSeconds -gt 600 -or $timing.timeout -ne $false) {
        throw "Execution budget exceeded: $condition"
    }
    $elapsed += $timing.elapsedSeconds
    $events = @(Get-Content -LiteralPath "$directory/scored-$condition.jsonl" | ForEach-Object { $_ | ConvertFrom-Json })
    $started = @($events | Where-Object type -eq 'thread.started')
    if ($started.Count -ne 1 -or @($events | Where-Object type -eq 'turn.started').Count -ne 1) {
        throw "Expected one fresh attempt: $condition"
    }
    $threads += $started[0].thread_id
    $preflight = @(Get-Content -LiteralPath "$directory/preflight-$condition.jsonl" | ForEach-Object { $_ | ConvertFrom-Json })
    $passed = @($preflight | Where-Object {
        $_.type -eq 'item.completed' -and $_.item.type -eq 'command_execution' -and
        $_.item.exit_code -eq 0 -and $_.item.aggregated_output -match 'PREFLIGHT_PASS'
    })
    if ($passed.Count -ne 1) { throw "Missing successful preflight: $condition" }
    $mcp = @($events | Where-Object { $_.type -eq 'item.completed' -and $_.item.type -eq 'mcp_tool_call' -and $_.item.server -eq 'playbook' })
    if ($condition -eq 'without' -and $mcp.Count) { throw 'Baseline consulted Playbook' }
    $usage = @($events | Where-Object type -eq 'turn.completed' | ForEach-Object usage)
    $inputs = Get-Content -LiteralPath "$directory/input-$condition.json" -Raw | ConvertFrom-Json
    foreach ($inputFile in $inputs | Where-Object { $_.path -notin @('Program.cs', 'Workflow.cs') }) {
        $relative = $inputFile.path
        if ($relative -match '\.(csproj|props|targets)$') { $relative += '.txt' }
        if ((Get-FileHash -LiteralPath (Join-Path "$directory/submission" $relative)).Hash -cne $inputFile.sha256) {
            throw "Frozen scaffold input changed: $condition/$relative"
        }
    }
    [pscustomobject]@{
        Condition = $condition
        ContextId = $started[0].thread_id
        Seconds = $timing.elapsedSeconds
        Timeout = $timing.timeout
        PlaybookCalls = $mcp.Count
        InputTokens = $(if ($usage.Count) { $usage[0].input_tokens } else { 'unavailable' })
        OutputTokens = $(if ($usage.Count) { $usage[0].output_tokens } else { 'unavailable' })
    } | Format-List
}
if ($threads[0] -eq $threads[1]) { throw 'Attempt contexts reused' }
$first = Get-Content "$root/csharp/without/scored-without.timing.json" -Raw | ConvertFrom-Json
$second = Get-Content "$root/csharp/with/scored-with.timing.json" -Raw | ConvertFrom-Json
if ([DateTimeOffset]$first.endedUtc -gt [DateTimeOffset]$second.startedUtc) { throw 'Runs overlap or order differs' }
if ($elapsed + 3600 -gt 4800) { throw 'Insufficient budget for the six remaining bounded runs' }
$withoutInputs = Get-Content "$root/csharp/without/input-without.json" -Raw
$withInputs = Get-Content "$root/csharp/with/input-with.json" -Raw
if ($withoutInputs -cne $withInputs) { throw 'Scaffold input hashes differ' }
"PASS: $($receipts.Count) file hashes; two isolated sequential attempts; total $elapsed seconds."
