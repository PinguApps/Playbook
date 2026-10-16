# Implementation plan

Status: Pingu confirmed the [consolidated plan](README.md) on 16 October 2026 after eight design rounds. Product, contract, repository, content, release and verification policies are settled, including language-neutral filtering and native authoring cadence. The [source/goal inventory](inventory/README.md) makes candidate coverage concrete. Source/API reconciliation and actual sample/artifact evidence remain delivery work before publication. Implementation awaits a separate request.

## Accepted layout

- Root: `PinguApps.Playbook.slnx`, shared MSBuild files, `Directory.Packages.props`, pinned `global.json`, `NuGet.Config`, style/analyzer configuration, README, license, and GitHub configuration.
- `src/PinguApps.Playbook/`: the single console/tool project, organized around catalogue discovery and recipe retrieval, with internal content-loading support.
- `tests/PinguApps.Playbook.Tests/`: the focused Reqnroll-based tool/contract tests and packed-tool smoke scenario.
- `content/recipes/<recipe-id>/<edition>/`: one JSON metadata file and one Markdown narrative per edition. IDs remain stable; editions express release-specific guidance.
- `samples/`: independently buildable, original examples, organized by their target release. Include real older/newer target/dependency sets where the comparison requires both. Keep sample SDK/workload pins separate from the .NET 10 tool host.
- `eng/Verify.ps1`: the common verification entry point, following the packed-consumer boundary from BlazorSitemap.
- `docs/`: planning, decisions, contribution instructions, client setup, verification notes, and the source-backed coverage register.

Generate the catalogue and rendered recipe Markdown from these sources during build/pack, then embed the generated content in the tool assembly. Installed operation must not depend on the repository, the caller's working directory, a content database, or a network fetch. The served examples are extracted from the actual sample source files; do not maintain a separate hand-copied code version in Markdown.

## Accepted comparison baseline

Normally compare .NET 11 with .NET 10, and the selected .NET 10 foundations with .NET 9. Compare the same goal and application context, using actual target frameworks/dependencies rather than calling an emulation an old-runtime test.

When there was no equivalent older capability, say so. When an existing older approach remains valid, explain the tradeoff without describing it as broken or obsolete. A small recipe can reference supporting sample files rather than dumping an entire application into a response.

## Delivery milestones and checks

1. **Repository and tool scaffold.** Apply the accepted CodeStyle/Template/RazorStyle structure, central versions, analyzers, and packaging identity. Adapt the references rather than copying namespaces/package names blindly. Check: build the .NET 10 executable with the selected conventions and produce a tool package. Retain the accepted `PinguApps/Playbook` remote, `PinguApps.Playbook` package/project/solution, and `playbook` command; no rename is needed.
2. **MCP core and CLI.** Implement direct stdio, server identity/instructions, two tools, filtering/paging, target resolution, stable editions, errors, complete structured/text results, and help/version. Check: focused behavior scenarios prove explicit-target safety, optional filters, complete detail, and real process startup/shutdown from a packed artifact. Reserve stdout for MCP traffic.
3. **Content authoring pipeline.** Add typed metadata validation, edition/source references, snippet extraction, generated discovery data, and embedded content. Check: missing/invalid source references and incompatible metadata fail verification; the package serves the exact snippets from the canonical sample sources, from an unrelated working directory.
4. **Release-wide content.** Complete the .NET 11 source register and independently authored recipes across all accepted areas, plus the eight .NET 10 foundations. Preserve the original Blazor baseline, including Blazor AI, and label experimental APIs clearly in discovery and detail. Check: every reviewed change has a recipe, coverage link, exclusion reason, or visible unresolved disposition; no framework is silently omitted. Headline research examples are not a substitute for the register.
5. **Verification.** Keep focused Linux/core checks and actual packed-tool consumer verification. Windows/macOS each run only the agreed brief install/start/initialize/list/get/shutdown smoke. Check .NET 10 hosting and .NET 11 major roll-forward using real artifacts in suitable isolated environments. Require examples to compile against declared SDKs/dependencies and use focused behavior checks where useful. Resolve compilation gaps before those examples meet the gate. Do not add a recurring native UI/device/emulator matrix or imply compilation proves native runtime behavior; report exactly what was checked. The [verification plan](verification-plan.md) records accepted checks and native authoring cadence.
6. **Documentation and client setup.** Provide global .NET tool installation/update instructions, `playbook mcp`, four-client setup, companion agent instructions, recipe contribution guidance, and troubleshooting. Check: one brief real check per named client on one available supported OS establishes that both body and applicability reach the agent. Record actual versions; no four-client-by-three-OS matrix. Complete the accepted four-task/eight-run manual comparison before public launch, with at most 80 execution minutes plus preparation/review. See the [evaluation plan](evaluation-plan.md). OS smoke checks alone do not establish client integration or product value.
7. **Release infrastructure.** Copy the six accepted workflow roles, release labels/drafting, pinned actions, Blacksmith/Linux baseline, artifact uploads, and NuGet OIDC publishing. Add only the short Windows/macOS smoke jobs. Schedule weekly stable draft publication at Sunday 18:17 UTC after the first manual stable release; recheck organization schedules before activation. Preserve any-new-main-commit behavior without shipped-path or empty-notes guards. Check: pack once, verify that same artifact on the required lanes, and push it without repacking; trusted-publisher configuration matches the accepted repository/workflow identity. Apply the accepted [version/channel policy](release-plan.md); do not publish during planning.
8. **GA reconciliation.** When .NET 11 ships, compare the final release-family notes, breaking changes, and actual SDK/package APIs with the source register. Update SDK/workload pins, examples, applicability, omissions, and stale RC assumptions; rerun relevant checks and prepare a corrective/additional package release. Preserve the actual verification baseline rather than retroactively labeling RC checks as GA checks. This is a planned maintenance step, not an installed runtime updater or scheduled agent task.

Commit coherent milestones, keep work in one implementation PR unless Pingu approves a split, follow complete-work/finish-pr when implementation through a PR is authorized, and leave merging/releasing to Pingu unless explicitly requested.

## Round 5 decisions

- Include experimental-at-GA APIs and label them clearly. Blazor AI remains required; the proposed exclusion was rejected.
- Use the layout and embedded/generated-content approach above.
- Use the previous stable release as the normal older-approach baseline.
- Require code examples to compile against their stated target/dependencies; use focused behavior checks rather than a full native runtime gate. Windows/macOS CI remains limited to tiny tool smokes. Record native compilation gaps and resolve them before calling an example compile-verified; the feasibility of evidence on each sample's platform must not be assumed.
