# Inspect retained evidence only. Never launch another scored model attempt.
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$receipts = @(Get-Content "$root/sha256.json" -Raw | ConvertFrom-Json)
$actual = @(Get-ChildItem -LiteralPath $root -File -Recurse | Where-Object Name -ne 'sha256.json' | ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName) })
if (@(Compare-Object ($receipts.path | Sort-Object) ($actual | Sort-Object)).Count) { throw 'Evidence manifest does not cover the retained file set' }
foreach ($receipt in $receipts) {
    $path = [IO.Path]::GetFullPath((Join-Path $root $receipt.path))
    if (!$path.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Evidence path escapes bundle' }
    if ((Get-FileHash -LiteralPath $path).Hash -cne $receipt.sha256) { throw "Changed evidence: $($receipt.path)" }
}
$identity = Get-Content "$root/controls/sdk-identity.json" -Raw | ConvertFrom-Json
if ($identity.comparedFiles -ne 5467 -or @($identity.mismatches).Count) { throw 'SDK relocation identity not established' }
$ids = @()
$identity = Get-Content "$root/controls/frozen-identity.json" -Raw | ConvertFrom-Json
if ($identity.cli -cne '9D563EE4E0441FF9D59D612013E0460DDF8E5709735C2D9F6B98B7EF8CA4EB42' -or $identity.package -cne '9A96E01473994BB854BB93D49C4E8B4CFE90805003DC249D7A7BE60505F2243C' -or $identity.scaffold -cne '6d53662cdba54551a74b3ade063c49009c63561c') { throw 'Frozen identities differ' }
$firstLaunch = [DateTimeOffset](Get-Content "$root/without/scored-without.timing.json" -Raw | ConvertFrom-Json).startedUtc
foreach ($condition in @('without', 'with')) {
    $preflightTiming = Get-Content "$root/$condition/preflight-$condition.timing.json" -Raw | ConvertFrom-Json
    if ([DateTimeOffset]$preflightTiming.endedUtc -ge $firstLaunch) { throw 'Preflight must precede either scored attempt' }
}
$elapsed = 0.0
$previousEnd = $null
foreach ($condition in @('without', 'with')) {
    $directory = Join-Path $root $condition
    $timing = Get-Content "$directory/scored-$condition.timing.json" -Raw | ConvertFrom-Json
    if ($timing.deadlineSeconds -ne 600 -or $timing.elapsedSeconds -gt 600 -or $timing.timeout -ne $false -or $timing.exitCode -ne 0) { throw 'Recorded successful-run receipt changed' }
    if ($previousEnd -and $previousEnd -gt [DateTimeOffset]$timing.startedUtc) { throw 'Runs overlap or order differs' }
    $previousEnd = [DateTimeOffset]$timing.endedUtc
    $elapsed += $timing.elapsedSeconds
    $events = @(Get-Content "$directory/scored-$condition.jsonl" | ForEach-Object { $_ | ConvertFrom-Json })
    $starts = @($events | Where-Object type -eq 'thread.started')
    if ($starts.Count -ne 1 -or @($events | Where-Object type -eq 'turn.started').Count -ne 1) { throw 'Expected one fresh attempt' }
    $ids += $starts[0].thread_id
    $preflight = @(Get-Content "$directory/preflight-$condition.jsonl" | ForEach-Object { $_ | ConvertFrom-Json })
    if (!($preflight | Where-Object { $_.type -eq 'item.completed' -and $_.item.type -eq 'command_execution' -and $_.item.exit_code -eq 0 -and $_.item.aggregated_output -match 'PREFLIGHT_PASS' })) { throw 'Successful preflight missing' }
    $calls = @($events | Where-Object { $_.type -eq 'item.completed' -and $_.item.type -eq 'mcp_tool_call' -and $_.item.server -eq 'playbook' })
    if ($condition -eq 'without' -and $calls.Count) { throw 'Baseline consulted Playbook' }
    if ($condition -eq 'with' -and !($calls | Where-Object { $_.item.tool -eq 'get_recipe' -and $_.item.status -eq 'completed' })) { throw 'With-condition consultation missing' }
    foreach ($inputFile in (Get-Content "$directory/input-$condition.json" -Raw | ConvertFrom-Json)) {
        if ($inputFile.path -in @('App.razor', 'Shelf.razor')) { continue }
        $relative = $inputFile.path
        if ($relative -match '\.(csproj|props|targets)$') { $relative += '.txt' }
        if ((Get-FileHash -LiteralPath (Join-Path "$directory/submission" $relative)).Hash -cne $inputFile.sha256) { throw "Frozen input changed: $relative" }
    }
    $task = [IO.File]::ReadAllText("$directory/submission/TASK.md")
    if ($condition -eq 'with') { $task += "`nBefore implementing, consult Playbook for guidance relevant to the task and target framework. Apply only guidance whose applicability matches the project.`n" }
    if ([IO.File]::ReadAllText("$directory/prompt-$condition.txt") -cne $task) { throw 'Prompt changed' }
    [pscustomobject]@{ Condition = $condition; ContextId = $starts[0].thread_id; Seconds = $timing.elapsedSeconds; Timeout = $timing.timeout; PlaybookCalls = $calls.Count } | Format-List
}
if ($ids[0] -eq $ids[1]) { throw 'Contexts reused' }
$csharp = 0.0
foreach ($condition in @('without', 'with')) { $csharp += (Get-Content "$root/../csharp/$condition/scored-$condition.timing.json" -Raw | ConvertFrom-Json).elapsedSeconds }
if ($elapsed + $csharp + 338.0084435 + 1200 -gt 4800) { throw 'Insufficient budget for two remaining bounded attempts' }
"Retained hashes and receipts pass. Six observed attempts: $($elapsed + $csharp + 338.0084435) seconds."
