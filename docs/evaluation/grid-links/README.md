# Independent static grid links preparation (PIN-948)

Current comparison controls: [the owner-approved protocol](../comparison-protocol.md) refreshes these scaffolds to RC3. The RC2 pins and export commits below describe the original preparation only; use the refreshed committed revision for the next comparison.

Evaluator material: export only `samples/GridLinksEvaluation/` at scaffold commit
`dbed01e8fa21279260846221f19445664681eb1b` into two separate clean directories.
Give TASK.md verbatim to both fresh contexts. Never expose this directory,
repository/history, canonical recipes or the other run's output. The original
scaffold builds but its static controls do not solve the task. This preparation
records no agent comparison results; PIN-953 owns later execution.

## Frozen controls

- SDK 11.0.100-rc.2.26504.105, net11.0, C# 15.0 and QuickGrid
  11.0.0-rc.2.26504.105. The committed lock pins transitive inputs. Separate build,
  package and feed files isolate the exported project from root inheritance.
  No workloads, accounts or external services. Restore both exports with locked
  mode and prepare identical caches before starting either clock.
- Same Windows machine, absolute SDK executable, shell and file/build tools.
  Disable network except loopback in both contexts. Only the with condition gets
  the frozen offline Playbook package and companion instruction. No repository
  search beyond the export, evaluator material or corrective follow-ups.
- Ten wall-clock minutes per context including builds/checks, without retries.
  Retain failures/timeouts and partial output. Without first for this pair,
  alternating the preceding reservation task. The eight fresh contexts and
  maximum 80 execution minutes belong to later execution tickets.
- At execution freeze one available client/model/version/settings across all eight
  runs, Playbook version/hash, companion instruction, actual tool inventory and
  network enforcement. Record elapsed time and tokens where exposed. Differences
  invalidate a controlled comparison. No model/package version is invented here.
- Intact Windows SDK archive SHA-512:
  `80996fb821ded98ec0439ea8cec9df8d9a1c8fc38ca49a828ca50acfc342ac8cd2f412f116a664f5f285fa06118b46235bc34ce034be3112a56a3bae0406ce06`.
  This is the official RC2 win-x64 tar.gz, extracted locally without installation
  or PATH/profile integration. The dotnetup shared muxer could not execute this
  SDK; use the intact extracted executable. RC2 evidence does not establish GA.

## Private behavior oracle

Test-Oracle.ps1 copies a submission recursively to a disposable directory, excluding
bin/obj/.git, builds its real pinned dependencies in locked mode, starts Kestrel
on an ephemeral loopback port and follows rendered ordinary anchors through HTTP.
It checks asymmetric bookmarked views, refresh, both grids' page links and sort
direction links, preserved filter spaces and the unchanged other grid's rows,
return to the bookmark, and defaults. Setup/build failures and missing links fail;
never repair a submission while scoring. Preserve the original and diagnostics.
The disposable copy is retained for diagnosis; only its server is stopped.

`-Reference` substitutes Reference.razor in the disposable copy only. It uses valid
manual URL restoration and ordinary links, intentionally demonstrating that a
correct older approach passes. It is calibration, never an agent run. Inspect
static-only rendering, unchanged data/pins, absence of interactive render modes,
and code-column sort links separately. HTTP history replay verifies restored URLs;
it does not claim an actual browser back/forward interaction. During scoring also
exercise those controls in a browser with JavaScript disabled, including code sort
and units sort from defaults, both directions, next and previous on both tables.
Record browser/version and actual observations separately. No browser calibration
or broad accessibility/OS evidence is claimed by this preparation.

Record consultation, behavior, applicability, newer-API adoption, unnecessary code,
elapsed time, tokens and limitations independently. Newer API use is optional for
correctness; do not fail a working manual implementation for omitting it.

## Expected guidance

`blazor.share-grid-view/11.0` is primary. Static SSR requires ordinary links;
QuickGrid/QueryParameterNameOptions and Paginator can restore independent query
namespaces with one PaginationState per grid. Actual RC2 uses column Title as its
stable sort key, not a fictional ColumnId property. Custom headings need anchors.
See [the actual artifact evidence](../../verification/grid-url-state.md) and
PIN-781. Its separate .NET 10 manual implementation remains valid comparison
guidance; it is not a selectable .NET 10 edition of this recipe. Retrieve actual
packaged applicability at execution and retain the response. This preparation
does not alter served guidance or require refreshing unchanged package evidence.

SPEC stories 154/156/157/160 and [the evaluation plan](../../planning/evaluation-plan.md)
constrain this bounded task. No optional root PLAN/CONTEXT files exist.

From PowerShell 7, supply the intact pinned SDK executable:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-b1d8a715'
& ./docs/evaluation/grid-links/Test-Oracle.ps1 `
  -Submission 'V:\_worktrees\Playbook\t3-b1d8a715\samples\GridLinksEvaluation' `
  -Dotnet 'V:\_worktrees\Playbook\t3-b1d8a715\artifacts\sdk\rc2\dotnet.exe' -Reference
```

Expect zero failed assertions. Omit Reference to observe failures on the unsolved
scaffold; for actual scoring point Submission at the preserved exported result.
