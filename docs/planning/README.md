# PinguApps.Playbook: agreed plan

Status: shared understanding confirmed by Pingu on 16 October 2026, after eight design rounds and 51 decisions. Planning is complete. This is a repository/package delivery plan; no tool, samples or workflows have been implemented, and nothing has been published.

## Product and coverage

Build one small, deterministic, offline MCP documentation server for coding agents. The agent inspects the consumer project and writes/builds its code; Playbook supplies practical goal-based recipes with prerequisites and evidence.

Cover the .NET 11 release family wherever a change affects authored code, markup, scripts, project/build configuration or implementation design. This includes languages, shared libraries, SDK/tooling, ASP.NET Core, EF Core, desktop and MAUI/platform workloads. Exclude benefits obtained solely by upgrading without a different authored approach. Document external dependencies where a qualifying recipe needs them; independently versioned standalone products remain outside this boundary.

Preserve the original 25 Blazor topics, seven additions and three expansions, accepted migrations, and the exact eight curated .NET 10 foundations. Include experimental APIs with clear labels and opt-ins. Prepare GA guidance from current RC evidence, then reconcile final release-family notes and actual APIs at GA. Retain release-specific editions as future .NET releases join the same product.

The [inventory](inventory/README.md) records 406 source dispositions, including 310 included candidate rows. These overlap and remain provisional; they are not a final distinct recipe count, complete public-API audit or verification claim. Full API/migration reconciliation and original samples remain delivery work.

## Tool and content contract

Run `playbook mcp` directly over stdio, one client-launched process. Help/version are the only other CLI functions; bare invocation shows help. Host on `net10.0` with `Major` roll-forward, separately from sample SDKs/workloads.

Expose two tools: `list_recipes` and `get_recipe`, both requiring an explicit target. Discovery has optional area, demonstrated-language and keyword filters, deterministic paging, and shared guidance enabled by default with `includeShared: false` available. Include genuinely language-neutral configuration recipes when applicable to a requested language, explicitly labelled neutral. Unknown IDs/targets/filters fail clearly; valid zero-match combinations succeed empty.

PIN-971 supersedes the historical omitted-target default: require a supported target release or deliberate `latest` on every call. Resolve `latest` numerically from the installed catalogue, echo requested/resolved targets and retain `targetAssumed: false`. Missing/null/blank targets fail helpfully. Inspect application target frameworks and project/`global.json` context; repeat the target on both tools. A target release is not an exact SDK build; `latest` is for exploratory/theoretical requests or unknown targets and does not prove consumer compatibility. Preserve older editions and never silently substitute an incompatible target. Separate introduction release from verified runtime/SDK/language/package/platform requirements, including supported conditional older-target routes.

Return complete applicability, stability, evidence, references and full Markdown in one structured object with an output schema and matching JSON text fallback. All necessary recipe guidance is bundled offline; package updates deliver new content. Use original explanations and wholly original examples, comparing with the previous .NET release. C# is canonical for shared examples, with native-language examples for F#/VB-specific changes and translations where authoring differs materially. [Detailed contract](tool-contract.md), [coverage](coverage.md), [glossary](../../GLOSSARY.md).

## Repository and infrastructure

Keep GitHub `PinguApps/Playbook`, NuGet/project/solution `PinguApps.Playbook`, and command `playbook`. Use one production tool project, one focused Reqnroll test project, independent samples, JSON metadata/Markdown under `content/recipes/<id>/<edition>/`, and shared root configuration with centralized package management. Generate catalogue/rendered snippets from canonical sample source during build/pack and embed the result in the tool assembly.

Apply PinguApps structure/style and analyzers from the agreed reference map. Reuse the six GitHub Actions roles, SHA-pinned actions, Blacksmith/Linux baseline, Release Drafter and NuGet OIDC publishing. Roslynk supplies MCP mechanics; Playbook's accepted architecture uses direct stdio. Preserve required source attribution if substantial reference implementation code is reused. [Reference map](reference-repositories.md), [layout and milestones](implementation-plan.md).

## Verification and launch

Keep focused Linux/core checks and verify the actual packed-tool install/stdio/list/get/shutdown path. Windows and macOS each get only one brief tool smoke; no native UI/device/emulator or client-by-OS matrix. Pack once, test the same artifact across the required lanes, and publish it without repacking.

Examples must compile against declared dependencies/toolchains. Native compilation happens during authoring on appropriate machines, with exact recorded evidence reused only while relevant inputs stay unchanged. Repeat it when source, target, dependencies or toolchain change. Use focused runtime evidence where needed and distinguish it from source review or compilation. Suitable native environments, including Mac/Xcode for affected Apple samples, are delivery prerequisites still to establish.

Check T3 Code, Codex CLI, Claude Code and VS Code Copilot once each on a supported OS, confirming both body and applicability reach the agent. Include companion instructions and setup guidance. Before public launch, run the accepted four original tasks with/without Playbook: eight fresh runs on one fixed client/model, at most ten minutes each, capped at 80 execution minutes plus preparation/review. [Verification plan](verification-plan.md), [evaluation plan](evaluation-plan.md).

## Release policy

Use independent package SemVer: stable `1.0.0`, manual `1.0.0-preview.N` if publishing before GA, patch corrections, minor compatible additions, major breaking contract changes. First public publication requires agreed coverage and evidence; stable additionally requires GA reconciliation.

After the first manual stable release, publish stable drafts weekly at **Sunday 18:17 UTC**, rechecking organization schedules before activation. Preserve Pingu's standard any-new-main-commit behavior, including docs/CI/tests/maintenance; do not add shipped-path, package-difference or empty-release-note guards. The accepted stable-channel gate checks both versions and GitHub prerelease flags. [Release plan](release-plan.md), [schedule audit](publishing-schedule.md).

## Delivery sequence and completion checks

1. Scaffold the repository/tool with the agreed conventions; build and pack the .NET 10 host.
2. Implement the MCP contract/content pipeline; verify caller behavior and exact bundled snippets through a packed artifact.
3. Reconcile the source/API register and author original release-specific samples; compile against actual declared artifacts and record focused behavior evidence.
4. Complete minimal OS/client checks, offline setup documentation and the bounded manual evaluation; fix defects found.
5. Configure accepted release infrastructure and verify the exact release artifact/identity. At GA reconcile final notes/APIs, rerun affected checks and prepare the stable or corrective release.

Exact current SDK/workload/package/action pins, generated schemas, sample details and evidence records are implementation tasks within this design. No missing proof or unresolved qualifying source item may silently disappear. Follow the [full milestones](implementation-plan.md) when implementation is requested, using an isolated implementation branch/worktree and complete-work/finish-pr. Leave merging/releasing to Pingu except the separately approved future weekly automation.

The [brief](brief.md) retains the complete decision history and superseded proposals. Pingu confirmed this shared understanding with “Agreed”. Implementation is a separate request.
