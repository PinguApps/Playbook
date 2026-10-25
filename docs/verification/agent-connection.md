# Agent connection evidence

Date: **20 October 2026**. [Repeat/setup/cleanup instructions](../t3-verification.md). This proves reusable fresh-thread MCP visibility and package replacement, not recipe-selection/application usability or a completed catalogue. No production source, recipes, tool contract, CI jobs, schedules or services changed.

## Actual route and identity

- T3 environment `PINGU-SERVER`, ID `e161a58f-5feb-4993-b10f-5dc13775bd35`; server/client route reported `0.1.1-nightly.20261019.3348` by `t3_environment_read`. Windows 11 Pro `10.0.26200`, x64; PowerShell `7.6.7`; Codex CLI `0.163.0`; .NET SDK `10.0.402`.
- Coordinator `309099aa-6153-46f5-82ed-3703e00d0138` discovered current selection through `orchestrator_capabilities` and `t3_thread_configuration`: instance `codex`, model `gpt-6.1-sol`, high effort, standard service tier, full-access/default modes. No model matrix was run.
- Dedicated project `mcp:cb660864-7bf7-43f9-b205-1a54c5e763cf`, **Playbook verification (PIN-976)**, seed `V:\_Scratch\PlaybookVerification`. Its initial commit `1f9a4dde85de1409de120ff0dc22fc6603b93987` contains only README, ignored `packages/`, and project-local `.codex/config.toml`.
- Existing Codex provider settings had empty custom/shadow home paths; the tested route uses `C:\Users\pingu\.codex` and its existing owner-held access. No credentials were read, copied, requested or committed. No account setup was needed. The only user-config addition was the exact seed's trust entry; hashing the original prefix confirmed all prior config bytes were preserved.

Initial actual local package: `PinguApps.Playbook` **1.0.0-preview.976.1**, assembly version output `1.0.0-preview.976.1+75ddc52bd06e7f4256c7ae072c0da6d3a8a69db6`, package SHA-256 **5519a16807ace13ae929d82370074ecb4e3c1f4970df29eeae5856e05f2d7ea4**. Built/packed from repository commit `75ddc52bd06e7f4256c7ae072c0da6d3a8a69db6`, installed through a local-only feed into `V:\_Scratch\PlaybookVerification\packages\pin976-1\tool\playbook.exe` with its own NuGet cache. These unpublished test identities are unrelated to target release `10.0`.

## Fresh ordinary-thread evidence

1. **Negative prerequisite probe:** [untrusted project](t3-thread:/v1/mcp:cc6a37f0-f5bd-44b0-8b50-cdf17f52b18e), launched with explicit `worktree` strategy, local `main`, `startFromOrigin:false`. It reported no Playbook tools and made no substitute calls. `codex mcp list --json` in that checkout also omitted Playbook. This was an actual failed check, retained rather than counted as success.
2. Added the scoped seed trust entry described in the setup guide. The same CLI listing then showed enabled `playbook`, correct absolute executable, `mcp` argument and 30-second startup timeout. No global Playbook MCP entry was added.
3. **Successful fresh thread:** [trusted project proof](t3-thread:/v1/mcp:ff3e697c-42f3-409e-a1ea-b96f14ab6323), launched through `t3_thread_launch`, returned `threadId` `mcp:ff3e697c-42f3-409e-a1ea-b96f14ab6323` and run `run:thread:mcp%3Aff3e697c-42f3-409e-a1ea-b96f14ab6323:ordinal:1`. T3 bound it to branch `verify/pin-976-trusted`, worktree `V:\_worktrees\PlaybookVerification\verify-pin-976-trusted`, before execution. Durable state confirms `parentThreadId:null`, ordinary top-level thread, no inherited parent messages, status `completed` and `activeRunId:null`.
4. `t3_thread_read(view:"activity")` returned completed `playbook.list_recipes` at position **3**, `2026-10-20T18:12:26.030Z`, and `playbook.get_recipe` at position **4**, `2026-10-20T18:12:27.947Z`. Full call items were recovered using `itemId`/`nextTextOffset`, including the large detail result. [Saved inputs and full outputs](agent-connection.calls.json) retain their durable item IDs. No Inspector, direct server call or mock substituted for either agent invocation.

Actual arguments:

```json
{"targetVersion":"10.0","query":"field","limit":1}
```

```json
{"id":"csharp.validate-property-assignment","targetVersion":"10.0"}
```

Discovery returned ID **csharp.validate-property-assignment**, title **Normalize and validate properties with the C# field keyword**, and the full goal: “Use C# 14's field keyword to normalize or validate values in a property setter without declaring a separate backing field. Apply when property assignments need trimming, case conversion or input checks, while preserving constructor validation and the behavior of an explicit-field implementation.”

Detail returned **6,286 UTF-16 characters of Markdown**, including both original implementations, migration steps, constructor/initializer/recursion/concurrency constraints, and verification notes. The complete two-route applicability arrived with it: primary .NET 10 SDK/runtime and C# 14; optional .NET 11 SDK retaining .NET 10 runtime/target and C# 14. Requirements, source URLs and saved evidence remain separate. Both responses resolved explicit `10.0`, `targetAssumed:false`; both coverage entries are incomplete. The agent's final receipt printed the Markdown and applicability and reported those limits.

## Replacement and preservation checks

The setup guide's first PowerShell block was executed verbatim from **C:\Windows\Temp**. Build and pack passed with **zero warnings/errors**. It installed **1.0.0-preview.976.2** into a fresh slot/cache and changed only the dedicated seed's connection, committed as `766aac805f7f8e9071cb69a2e4890e8d992ad4de`. The tested package SHA-256 is **7320cd083f3ac86c5cf415173dfcd15cf9fbb27c6c645df53445cd3a01a8c60e**; installed path `V:\_Scratch\PlaybookVerification\packages\9db0227e58da4ce6b91d05cb981d20ab\tool\playbook.exe`. An earlier staging install of preview.976.2 was not used for this proof; its identity must not be confused with this final artifact.

[Replacement proof](t3-thread:/v1/mcp:c58b4263-5e51-4113-8164-2358c16b17d5) was another fresh ordinary top-level thread, branch `verify/pin-976-replacement`, explicitly based on the new local seed. Its completed list/get activity items at positions 3/4, `18:15:57.713Z` / `18:16:00.312Z`, again returned full guidance. [Replacement metadata and item IDs](agent-connection.replacement.json) record the exact package, paths, seed commit and arguments. Programmatic comparison of the complete actual returned Markdown/applicability against the initial receipt passed. Full replacement call outputs were retained locally under ignored `artifacts/pin976/replacement.calls.json`; T3's durable item IDs allow reinspection independently of that temporary file.

Process, user and machine PATH were compared around build/install and the verbatim replacement block: **unchanged**. `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` was set in each .NET child environment; only process-scoped workload-notification/cache variables were used. No other persistent environment variables were written. Ordinary projects, threads, provider settings and unrelated MCP entries were preserved. An unused separate staging installation was successfully removed with `dotnet tool uninstall --tool-path` without touching either proof's installation.

## Cleanup and idle release

Both initial probe threads were archived through T3. The guide's guarded cleanup was executed from `C:\Windows\Temp`: Git removed their checkout files and worktree registrations, but Windows rejected final directory deletion because Codex/MCP processes retained open directory handles. A read-only native handle query identified the provider and its child processes. No shared process was terminated; interrupting the archived idle probe reported `no_active_run` and did not release the lock.

Pingu subsequently confirmed all other work was idle and authorized the required cleanup. At **19:03 UTC**, a fresh native handle query found no remaining handles on either residual directory. Both **empty directories** (`V:\_worktrees\PlaybookVerification\verify-pin-976-connection` and `V:\_worktrees\PlaybookVerification\verify-pin-976-trusted`) were then deleted nonrecursively from `C:\Windows\Temp`, with exact absolute-path/containment/emptiness checks and absence verified afterward. No process was restarted or terminated: the handles had cleared naturally while idle. Git's worktree list retains only the seed and intentional replacement-proof worktree. The installed replacement still reports its expected version.

Cleanup therefore passed in two stages: Git files/registration removal followed by empty-directory removal once the provider was idle. The setup guide preserves the initial failure and tested retry. Provider-restart cleanup was unnecessary and remains untested; this does not establish the independent PIN-942 client shutdown smoke. The seed, current replacement worktree and two evidence-bearing package slots remain intentionally available for reuse.
