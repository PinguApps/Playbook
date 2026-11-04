# Inspect retained evidence only. Never launch another scored model attempt.
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$receipts = @(Get-Content "$root/sha256.json" -Raw | ConvertFrom-Json)
$actual = @(Get-ChildItem -LiteralPath $root -File -Recurse | Where-Object Name -ne 'sha256.json' | ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName).Replace('\','/') })
if (@(Compare-Object ($receipts.path | Sort-Object) ($actual | Sort-Object)).Count) { throw 'Evidence manifest does not cover the retained file set' }
foreach ($receipt in $receipts) {
    $path = [IO.Path]::GetFullPath((Join-Path $root $receipt.path))
    if (!$path.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Evidence path escapes bundle' }
    if ((Get-FileHash -LiteralPath $path).Hash -cne $receipt.sha256) { throw "Changed evidence: $($receipt.path)" }
}
$identity = Get-Content "$root/controls/sdk-identity.json" -Raw | ConvertFrom-Json
if ($identity.comparedFiles -ne 5467 -or @($identity.mismatches).Count) { throw 'SDK relocation identity not established' }
$ids = @()
foreach ($case in @('baseline','reference','with','without')) {
    $log = [IO.File]::ReadAllText("$root/controls/oracle-$case.log")
    $failures = if ($case -eq 'baseline') { 9 } else { 0 }
    if ([regex]::Matches($log, '(?m)^(PASS|FAIL):').Count -ne 10 -or [regex]::Matches($log, '(?m)^FAIL:').Count -ne $failures -or $log -notmatch "Failed assertions: $failures(?:\r?\n|$)") { throw "Retained oracle counts differ: $case" }
}
$leftInputs = Get-Content "$root/with/input-with.json" -Raw | ConvertFrom-Json
$rightInputs = Get-Content "$root/without/input-without.json" -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath "$root/controls/Baseline.cs.txt").Hash -cne ($leftInputs | Where-Object path -eq 'Reconciliation.cs').sha256) { throw 'Calibration baseline differs from frozen export' }
if (@(Compare-Object ($leftInputs | ForEach-Object { "$($_.path):$($_.sha256)" }) ($rightInputs | ForEach-Object { "$($_.path):$($_.sha256)" })).Count) { throw 'Export inputs differ' }
$before = Get-Content "$root/controls/path-before.json" -Raw | ConvertFrom-Json
$after = Get-Content "$root/controls/path-after.json" -Raw | ConvertFrom-Json
if ($before.user -cne $after.user -or $before.machine -cne $after.machine) { throw 'Persistent PATH changed' }
$identity = Get-Content "$root/controls/frozen-identity.json" -Raw | ConvertFrom-Json
if ($identity.cli -cne '9D563EE4E0441FF9D59D612013E0460DDF8E5709735C2D9F6B98B7EF8CA4EB42' -or $identity.package -cne '9A96E01473994BB854BB93D49C4E8B4CFE90805003DC249D7A7BE60505F2243C' -or $identity.scaffold -cne '6d53662cdba54551a74b3ade063c49009c63561c') { throw 'Frozen identities differ' }
if ((Get-FileHash -LiteralPath "$root/../controls/PinguApps.Playbook.0.1.0-pin950.nupkg").Hash -cne $identity.package) { throw 'Retained package changed' }
# The CLI receipt records the historical executable; replay does not require it.
foreach ($pair in @(@('Test-Oracle.ps1.txt', 'Test-Oracle.ps1'), @('Reference.cs.txt', 'Reference.cs'), @('Oracle.cs.txt', 'Oracle.cs'))) {
    if ((Get-FileHash -LiteralPath "$root/controls/$($pair[0])").Hash -cne (Get-FileHash -LiteralPath "$root/../../../manifest-reconciliation/$($pair[1])").Hash) { throw 'Retained evaluator differs from repository original' }
}
$firstLaunch = [DateTimeOffset](Get-Content "$root/with/scored-with.timing.json" -Raw | ConvertFrom-Json).startedUtc
foreach ($condition in @('with', 'without')) {
    $preflightTiming = Get-Content "$root/$condition/preflight-$condition.timing.json" -Raw | ConvertFrom-Json
    if ([DateTimeOffset]$preflightTiming.endedUtc -ge $firstLaunch) { throw 'Preflight must precede either scored attempt' }
}
$elapsed = 0.0
$previousEnd = $null
foreach ($condition in @('with', 'without')) {
    $directory = Join-Path $root $condition
    $timing = Get-Content "$directory/scored-$condition.timing.json" -Raw | ConvertFrom-Json
    if ($timing.deadlineSeconds -ne 600 -or $timing.elapsedSeconds -gt 600 -or $timing.timeout -ne $false -or $timing.exitCode -ne 0) { throw 'Recorded successful-run receipt changed' }
    if ($previousEnd -and $previousEnd -gt [DateTimeOffset]$timing.startedUtc) { throw 'Runs overlap or order differs' }
    $previousEnd = [DateTimeOffset]$timing.endedUtc
    $elapsed += $timing.elapsedSeconds
    $events = @(Get-Content "$directory/scored-$condition.jsonl" | ForEach-Object { $_ | ConvertFrom-Json })
    $usageEvents = @($events | Where-Object type -eq 'turn.completed')
    if ($usageEvents.Count -ne 1) { throw 'Expected one usage event' }
    $usage = Get-Content "$directory/usage.json" -Raw | ConvertFrom-Json
    foreach ($property in $usageEvents[0].usage.PSObject.Properties) {
        if ($usage.($property.Name) -ne $property.Value) { throw "Usage mismatch: $condition/$($property.Name)" }
    }
    $starts = @($events | Where-Object type -eq 'thread.started')
    if ($starts.Count -ne 1 -or @($events | Where-Object type -eq 'turn.started').Count -ne 1) { throw 'Expected one fresh attempt' }
    $ids += $starts[0].thread_id
    $preflight = @(Get-Content "$directory/preflight-$condition.jsonl" | ForEach-Object { $_ | ConvertFrom-Json })
    if (!($preflight | Where-Object { $_.type -eq 'item.completed' -and $_.item.type -eq 'command_execution' -and $_.item.exit_code -eq 0 -and $_.item.aggregated_output -match 'PREFLIGHT_PASS' })) { throw 'Successful preflight missing' }
    $calls = @($events | Where-Object { $_.type -eq 'item.completed' -and $_.item.type -eq 'mcp_tool_call' -and $_.item.server -eq 'playbook' })
    if ($condition -eq 'without' -and $calls.Count) { throw 'Baseline consulted Playbook' }
    if ($condition -eq 'with' -and !($calls | Where-Object { $_.item.tool -eq 'get_recipe' -and $_.item.status -eq 'completed' })) { throw 'With-condition consultation missing' }
    foreach ($inputFile in (Get-Content "$directory/input-$condition.json" -Raw | ConvertFrom-Json)) {
        if ($inputFile.path -in @('Reconciliation.cs', 'Program.cs')) { continue }
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
$total = $elapsed
foreach ($task in @('csharp','reservation','grid')) {
    foreach ($condition in @('with','without')) { $total += (Get-Content "$root/../$task/$condition/scored-$condition.timing.json" -Raw | ConvertFrom-Json).elapsedSeconds }
}
if ($total -gt 4800) { throw 'Eight-run budget exceeded' }
"Retained hashes and receipts pass. Eight observed attempts: $total seconds."
