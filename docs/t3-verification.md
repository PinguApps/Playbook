# Isolated T3 Code verification

Use this owner-authorized setup for fresh Playbook agent checks outside CI. It uses the [documented Codex connection](clients.md#t3-code), a dedicated Git repository/T3 project, and an explicit installed-tool path. It adds no service, schedule or model matrix. The [PIN-976 receipt](verification/PIN-976.md) records the actual configuration, package and calls.

## Prepared environment

On Pingu's `PINGU-SERVER` environment:

- T3 project: `Playbook verification (PIN-976)`, ID `mcp:cb660864-7bf7-43f9-b205-1a54c5e763cf`.
- Seed repository: `V:\_Scratch\PlaybookVerification`, branch `main`. Test threads use separate app-created worktrees.
- Provider instance: `codex`, using its existing owner-held login and default `C:\Users\pingu\.codex` home. Credentials remain there; do not copy them into the seed, package, ticket or receipt.
- Playbook configuration: seed `.codex/config.toml`, committed so each new worktree receives it. Package/cache directories under seed `packages/` are ignored.

The T3 launch API does **not** install or inject Playbook. Provider configuration and project trust must already work. The first untrusted test failed to expose Playbook; adding only this entry to the provider's existing user `config.toml` enabled project configuration:

```toml
[projects.'V:\_Scratch\PlaybookVerification']
trust_level = 'trusted'
```

This exact repository trust entry was sufficient for both app-created worktrees. Do not trust a broader directory. Preserve all existing TOML. Project-scoped MCP configuration in trusted projects is documented by [OpenAI](https://developers.openai.com/codex/mcp/), with the [configuration trust rule](https://developers.openai.com/codex/config-basic/).

On another machine, the owner first enables/signs into the Codex provider through T3 **Settings → Providers**, checks its **CODEX_HOME path**, and authorizes a dedicated verification repository. Register that directory with `t3_project_create`; initialize/commit its seed before launching a worktree. Use that machine's returned project/provider IDs and absolute installed path. No new account/login was needed in the tested environment. Do not alter ordinary project defaults or add Playbook to the user's global `mcp_servers` table.

## Prepare or replace the installed package

Requires PowerShell 7 and repository SDK `10.0.402`. This block targets the already prepared seed above. It builds/packs an unpublished local version, installs into a **new** slot/cache even when reusing a version, and commits only the dedicated seed's MCP connection. Close completed test sessions before retiring their package slots; never replace a running server's files. Run in a fresh PowerShell process; environment assignments below affect only it and children. PATH is never assigned.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-ac9a569a'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$git = 'C:\Program Files\Git\cmd\git.exe'
$seed = 'V:\_Scratch\PlaybookVerification'
$version = '1.0.0-preview.976.2' # Local test identity; never published.
$slot = Join-Path $seed ('packages/' + [guid]::NewGuid().ToString('N'))
$paths = @('Process', 'User', 'Machine')
$before = @{}
foreach ($scope in $paths) { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = 'true'
$env:NUGET_PACKAGES = Join-Path $slot 'cache'

if (-not (Test-Path -LiteralPath "$seed/.git")) { throw 'Prepare the dedicated seed first.' }
if (& $git -C $seed status --porcelain) { throw 'Seed has changes; preserve them before proceeding.' }
& $dotnet build ./src/PinguApps.Playbook -c Release "-p:Version=$version"
$feed = Join-Path (Get-Location).Path 'artifacts/pin976/feed'
& $dotnet pack ./src/PinguApps.Playbook -c Release --no-build "-p:PackageVersion=$version" --output $feed
$package = Join-Path $feed "PinguApps.Playbook.$version.nupkg"
$config = Join-Path $feed 'local-only.config'
@"
<configuration><packageSources><clear /><add key="local" value="$([Security.SecurityElement]::Escape($feed))" /></packageSources></configuration>
"@ | Set-Content -LiteralPath $config
& $dotnet tool install PinguApps.Playbook --version $version --tool-path "$slot/tool" --configfile $config
$playbook = Join-Path $slot 'tool/playbook.exe'
& $playbook --version
Get-FileHash -LiteralPath $package -Algorithm SHA256

# Only this verification seed's connection changes. Keep earlier slots for old receipts.
$command = $playbook.Replace('\', '/')
@"
[mcp_servers.playbook]
command = '$command'
args = ['mcp']
required = true
startup_timeout_sec = 30
"@ | Set-Content -LiteralPath "$seed/.codex/config.toml"
& $git -C $seed add -- .codex/config.toml
& $git -C $seed commit -m "Select Playbook $version verification slot"
foreach ($scope in $paths) {
  if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) {
    throw "$scope PATH changed; inspect shared settings before taking further action."
  }
}
```

Record the emitted package hash, executable path, version and seed commit for each check. A fresh slot/local-only feed prevents an identically versioned NuGet.org/cache artifact from being selected. Existing threads keep their earlier configuration; launch a new thread from the new seed commit to verify replacement. This is safer than an in-place tool update during concurrent checks.

## Launch and inspect a fresh ordinary thread

The coordinating agent calls these **T3 MCP tools**, rather than asking another agent to create its own worktree:

1. `orchestrator_capabilities({})` and `t3_thread_configuration({})`: discover the live provider/model IDs and current selection. Select an available owner-authorized route; do not infer IDs from display names or create a matrix.
2. `t3_thread_launch` with the registered verification project and an explicit worktree strategy. Use a new title/branch per check. Retain the returned `threadId` and `runId`; there is no launch idempotency key. After an uncertain response, inspect `t3_thread_list` before retrying.

The tested launch shape was:

```json
{
  "projectId": "mcp:cb660864-7bf7-43f9-b205-1a54c5e763cf",
  "title": "Fresh Playbook connection check",
  "workspaceStrategy": {
    "type": "worktree",
    "baseRef": "main",
    "branch": "verify/unique-check-name",
    "startFromOrigin": false
  },
  "modelSelection": {
    "instanceId": "codex",
    "model": "gpt-6.1-sol",
    "options": [{ "id": "reasoningEffort", "value": "high" }]
  },
  "message": "Perform only a real Playbook MCP connection check. Discover live Playbook tools; do not dump the full catalog. Call list_recipes with targetVersion 10.0, query field and limit 1. Show id, title and goal. Call get_recipe for csharp.validate-property-assignment with targetVersion 10.0. Show full Markdown and applicability received, primary/optional requirements, evidence and coverage limits. Do not edit files, change configuration/environment, install tools or use shell/Inspector/mock substitutes. If Playbook is unavailable, report the prerequisite and stop. Stop after the two calls and receipt."
}
```

3. Use `t3_thread_wait({threadId, runId, timeoutMs:30000})` for bounded status waits. `preparing`/`running` is not success. Inspect `t3_thread_read({threadId, view:"activity", limit:100, maxCharsPerItem:20000})`. Follow `nextPosition` when `hasMore`; for truncated call items follow `itemId` and `textOffset: nextTextOffset` until complete. Offsets are UTF-16 positions. Save the actual tool inputs/outputs, not only the agent's final claim.
4. Expect two completed dynamic-tool items: `playbook.list_recipes` and `playbook.get_recipe`. Discovery returns `csharp.validate-property-assignment`, its reusable title/goal and compact applicability. Detail contains full offline Markdown and two .NET 10 routes: primary SDK 10/C# 14 and an optional SDK 11 alternative retaining `net10.0`/C# 14. Exact SDK pins belong to evidence. Both coverage entries remain incomplete; the SDK 11 evidence is RC, not GA verification.

Missing tools, startup errors, incomplete Markdown or absent routes fail the check. Inspect the selected provider home, scoped trust, committed project config and absolute executable first. Report owner/provider access or propagation as blocked if unavailable. Inspector can diagnose transport separately but never substitutes for this fresh-thread criterion. Subsequent usability tickets supply their own prepared situations and companion instruction; this setup receipt does not prove agents choose/apply recipes correctly.

## Cleanup

Keep the seed/project and current installed slot for later checks. First save call evidence, confirm the specific test thread has `activeRunId: null`, and archive it through `t3_thread_organize({action:"archive", threadId:"<that-test-thread>"})`. Archiving preserves its durable receipt. Never archive another project's threads.

After verifying the exact path belongs to this verification repository, remove only the completed test worktree. This example was tested for the successful initial receipt; it deliberately refuses a dirty worktree and does not use force:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-ac9a569a'
$git = 'C:\Program Files\Git\cmd\git.exe'
$seed = 'V:\_Scratch\PlaybookVerification'
$temporary = 'V:\_worktrees\PlaybookVerification\verify-pin-976-trusted'
$expected = [IO.Path]::GetFullPath('V:\_worktrees\PlaybookVerification') + '\'
$resolved = (Resolve-Path -LiteralPath $temporary).Path
if (-not $resolved.StartsWith($expected, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected worktree location.' }
$entries = (& $git -C $seed worktree list --porcelain) -join "`n"
if (-not $entries.Contains('worktree ' + $resolved.Replace('\', '/'))) { throw 'Not a worktree of the verification seed.' }
if (& $git -C $resolved status --porcelain) { throw 'Preserve test changes before cleanup.' }
& $git -C $seed worktree remove -- $resolved
if ($LASTEXITCODE -ne 0) { throw 'Worktree cleanup failed.' }
if (Test-Path -LiteralPath $resolved) { throw 'Worktree still exists.' }
```

Repeat with the exact completed test worktree path, never the seed or package root. To retire an old install after all its sessions have closed, run the absolute .NET executable's `tool uninstall PinguApps.Playbook --tool-path '<exact-old-slot>/tool'`; retain the package hash/receipt. Delete residual slot/cache files only after resolving the absolute slot and checking it is beneath this seed's `packages` directory. Do not recursively delete computed/unverified paths. No global tool uninstall, PATH restoration, login removal or ordinary project configuration cleanup is needed. Full retirement of this setup also removes only its scoped trust entry and dedicated T3 project after its threads are archived; keep unrelated settings and credentials.

Windows caveat: the initial probes exposed provider/MCP processes retaining open directory handles. Archiving/interrupting an idle thread does not release them. Git removed both probes' files and registrations but could not delete the empty `verify-pin-976-connection` and `verify-pin-976-trusted` directories. Do not terminate the shared provider while other threads are using it. Once the owner has finished those threads and normally restarted/closed the provider, verify each exact residual directory is empty and remove it with `Remove-Item -LiteralPath '<exact-residual-directory>'` (no recursion). Final residual-directory deletion awaits an idle provider; it has not been verified. A nonzero cleanup result must be retained as a limitation, not counted as success.
