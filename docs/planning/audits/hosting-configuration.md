# Hosting and configuration failure migrations: PIN-685

Reviewed **21 October 2026**. This source-only audit dispositions the six assigned compatibility notes and two adjacent Extensions entries. Three additional delivery proposals await backlog approval. Closing this audit delivers no recipe and does not clear the comprehensive publication gate.

## Scope and evidence

[PIN-685](https://linear.app/pinguapps/issue/PIN-685), `SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 102, 158 and 159; approved proposal `b3778cc`, key `SH_Q05`; inventory `migration.hosting-config-reload`. Consulted the full target and empty comments/attachments, cited historical spec/proposal, current planning README, coverage/implementation/verification plans, glossary, ADRs 0001–0004, contribution rules, existing audit/proposal conventions and current content/samples. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. No requirement-bearing explicit provenance pointer is unavailable. Historical planning statements that implementation has not begun are stale: the package, samples and installed-tool harness exist.

Boundary: PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project `b22380c7-6fde-49f4-8826-e5631dc5df7a`. There is no native blocker. An archived-inclusive, paginated query returned **317** issues; both boundary IDs were verified on every result. Full PIN-745, PIN-746, PIN-706, PIN-709 and PIN-791 descriptions and comments were read after scoped discovery. Their existing delivery/reconciliation obligations are preserved; none is declared complete by this audit.

The [33-file source manifest](hosting-configuration.sources.json) records immutable URLs and SHA-256 values:

- dotnet/docs `f0021fde66c59559fa1ba3c9a044148738f12faf`: the complete compatibility index and all eight full Extensions notes. Read introduction, previous/new behavior, rationale, recommended action and affected APIs. Six are assigned here; generator ownership and framework composition are adjacent cross-checks.
- dotnet/core `ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`: complete canonical RC1/RC2 library notes. RC1 confirms the asynchronous startup contract; its handwritten validator implements both methods. RC2 adds no separate hosting/configuration goal in this bounded family.
- dotnet/dotnet `4099f83977f4d29604a9aeb971fb0a7bc1d5f699` / `820230c386db5b0106c5430b3e5aee92a7826bf5`: .NET 10 / pinned RC2 source baselines. The manifest includes old/new Host, FileConfigurationProvider, ChangeToken and PhysicalFilesWatcher implementations; old/new Primitives and Options reference declarations; newer options/startup/cache integration and three package project files. These establish source contracts and qualify broad note wording, **not** a binary package comparison or executed consumer.

The compatibility index explicitly remains incomplete. This is a bounded mitigation audit, not the release-wide API diff owned by PIN-706/PIN-709. Source presence and broad package target lists do not prove a particular package asset, older-target route or runtime result. RC evidence is not GA evidence; actual artifact, compilation, behavior and final GA reconciliation remain delivery/publication obligations. No MCP tool, served recipe, project, dependency or CI changes here.

## Full mitigation dispositions

**Included** retains additional authoring work proposed below. **Grouped** retains an existing delivery owner, rather than treating a ticket title or source audit as delivered content. No assigned finding is excluded or left without an owner.

| Finding / introduction | Exact scope and authored action | Disposition / owner |
| --- | --- | --- |
| [BackgroundService failure propagation][host-note], Preview 3 | With the default `BackgroundServiceExceptionBehavior.StopHost`, a failing `ExecuteAsync` stops the default Generic Host and its shutdown/run task now faults. One collected exception is rethrown; multiple background/shutdown failures can aggregate. `Run`, `RunAsync`, `WaitForShutdown`, `WaitForShutdownAsync`, timed `StopAsync` and default `IHost.StopAsync` are affected through their stop path. Preserve failure visibility at the process boundary; an explicit catch can choose an exit policy. Do not recommend swallowing failures or selecting Ignore merely to restore success. Normal stopping cancellation is filtered; a custom IHost need not share this implementation. | **Included → A.** `host-11` collects StopHost failures and adds them to shutdown errors; `host-10` only logs/stops for this case. `host-extensions-11` forwards shutdown and disposes after RunAsync. This is changed behavior, not a new RunAsync signature. |
| [Ignored file-load failure notification][reload-note], Preview 7 | The note says ignored failures no longer notify. The implementation requires a narrower distinction: ignored initial parse/load failure and ignored file-open IO failure return without OnReload; a failed **reload parse** clears Data, marks it updated and still notifies. Missing optional/reload files also clear Data and notify. Observe failures in OnLoadException, separately from data-change tokens; do not infer successful loading, retained old values or a last-known-good policy from a token. | **Included → B.** `file-provider-10/11` establish the changed control flow. Existing PIN-745 covers asynchronous consumers and binding exclusions; it does not cover provider load-failure/data-state policy. The matrix below preserves the note/implementation discrepancy for real consumer verification. |
| [OnLoadException receives IO failures][io-note], Preview 7 | File-open failures now enter the callback, including arbitrary exceptions from a custom IFileInfo.CreateReadStream. Handle Exception without unconditional InvalidDataException/FileNotFoundException casts; select Ignore deliberately. FileNotFoundException/DirectoryNotFoundException opening races use the missing-file path. GetFileInfo and stream-disposal failures are outside this catch; do not promise the callback catches every custom-provider operation. | **Included → B**, one provider-failure goal with the preceding note. `file-provider-11` preserves Data on generic open failure, wraps parse failure in InvalidDataException, and separately handles missing files. The automatic reload delegate catches escaped Load failures; report them in the callback rather than depending on UnobservedTaskException. Explicit Load may still throw when not ignored. |
| [ChangeToken async callback rebinding][callback-note], Preview 7 | Recompilation can bind an async lambda to `OnChange(Func<IChangeToken?>, Func<Task>)` or its `Func<TState,Task>` form instead of Action/async void. Rearm waits for completion and intermediate notifications coalesce; this is not an event queue. An explicit Action retains the old binding when intentionally required. No switch changes compile-time binding. | **Grouped → PIN-745 (E04)**, still Todo. `primitives-ref-10/11` and `change-token-10/11` confirm both overload sets. Synchronous delegate throws propagate to the triggering code; later task failures are left unobserved by the registration's discarded task, with rearm in finally. Awaiting completion internally does **not** give the subscriber a task to await or automatically log/handle its failure. Handle errors inside the callback. Dispose unregisters; it does not await or cancel an already running callback. |
| [PhysicalFilesWatcher path validation][watcher-note], Preview 4 | Both public constructors normalize root with Path.GetFullPath and reject null/invalid roots. A nonempty FileSystemWatcher.Path must equal, contain or be contained by root; unrelated paths throw ArgumentException. The comparison adds trailing separators, avoiding sibling-prefix acceptance. A nonexistent root remains valid; an empty watcher path supports deferred watching. A null FileSystemWatcher still requires polling. | **Included → C.** `watcher-10/11` confirm construction changes, deferred root creation and ancestor subdirectory handling. This is explicit watcher composition, not a generic promise that all platforms support FileSystemWatcher. It is distinct from PIN-745's callback lifetime and PIN-791's browser service lifetime. |
| [Synchronous access to async-validated options][options-note], RC1 (async feature Preview 6) | The before-state here is **Preview 6/7**, not .NET 10: IAsyncValidateOptions was independent/contravariant. RC1 makes it invariant and inherit IValidateOptions; direct implementations need Validate plus ValidateAsync and registration as IValidateOptions (or Validate<TValidator>()). ValidateOnStart and completed startup can seed built-in IOptions/default and monitor caches. Snapshot scopes, monitor reload and factory Create remain synchronous; no public CreateAsync/GetAsync is established by these sources. | **Grouped → PIN-746 (E05)**, still Todo. `options-ref-10/11`, `options-interface-11`, `options-startup-11`, `startup-validator-11`, `options-factory-11`, `async-validator-11`, `options-monitor-11` and `host-11` establish the contract. Built-in AsyncValidateOptions.Validate fails for the matching name; merely adding another synchronous validator does not undo that failure. Follow the qualification below. |

### File-load state and notification qualification

The pinned `FileConfigurationProvider.Load(bool reload)` implementation has these paths. These are source-derived expectations for proposal B, not measured runtime results. JSON/XML/INI derive from this provider; a direct `Load(Stream)` call is not automatically the same failure/notification boundary as the file-loading wrapper.

| Path, with the failure ignored where a callback is reached | Data state in the pinned newer source | Provider OnReload |
| --- | --- | --- |
| Successful initial load or reload | Loaded by the concrete parser | Yes |
| Parse/Load(Stream) failure during explicit initial-style Load() | Not cleared by the base; concrete parser may have modified it | No |
| Parse failure during reload | Cleared by the base | Yes |
| Generic open/CreateReadStream failure | Existing Data preserved | No |
| Missing optional file or missing file on reload, including handled open race | Cleared | Yes |
| Missing required file during initial load | Callback decides ignore/throw; no new data | No |

This contradicts a blanket reading of the note that every ignored failure suppresses notifications. Preserve both the full note and selected implementation provenance. Do not resolve that conflict by claiming execution or teaching universal last-known-good behavior. The later delivery must verify actual selected assets against this matrix and revise the recipe to the measured contract; an artifact/source mismatch is a delivery blocker, not grounds to omit the case.

### Async-options qualification and adjacent ownership

The full options note recommends keeping at least one synchronous validator for paths that remain synchronous. In this pinned implementation, `OptionsFactory.Create` calls **every** registered validator's synchronous Validate and collects every failure. Each matching built-in AsyncValidateOptions returns Fail. Adding a separate succeeding synchronous validator therefore cannot make that combination succeed. A custom validator can implement meaningful synchronous validation, or return Skip for a name it does not own, but the author must not bypass a required asynchronous check by returning Success without equivalent validation. Proposal acceptance for **PIN-746** must test the intended full validator collection, names, cache and lifecycle, not merely registration or startup success.

`ValidateOnStart` documents further limits: custom/derived factories use synchronous startup validation; a custom default-name IOptions implementation prevents the built-in asynchronous route; an existing monitor value wins, and a faulted cache entry can fail startup. A custom legacy IStartupValidator takes precedence and suppresses the asynchronous startup validators. IStartupValidator is obsolete with **SYSLIB0066**; IAsyncStartupValidator is the replacement. Source-generated synchronous validation, custom dual-capability validation and a built-in async-only delegate are distinct cases. Nothing here promises asynchronous monitor reload or seeds later IOptionsSnapshot scopes.

Two adjacent index entries were read in full without creating another audit:

- [Explicit Validate / SYSLIB1205][generator-note], RC1: the generator now detects a handwritten explicit IValidateOptions.Validate implementation and suppresses synchronous generation for that model with an error; async generation is independent. Choose manual or generated synchronous ownership. Suppressing the diagnostic does not reconcile two implementations. **Grouped → PIN-746**, whose assigned `options.validate-asynchronously` inventory row already names explicit generated implementations. The bounded audit does not compile the generator; its actual failing/fixed builds remain delivery acceptance. PIN-706 retains wider generator/API/obsolete reconciliation.
- [Nine Extensions libraries in the base shared framework][framework-note], Preview 4: **grouped → PIN-706** for package/assets reconciliation. The list is Caching.Abstractions, Configuration.Abstractions, DependencyInjection.Abstractions, Diagnostics.Abstractions, FileProviders.Abstractions, Hosting.Abstractions, Logging.Abstractions, Options and Primitives. It does not include full Hosting, Configuration.FileExtensions or FileProviders.Physical. Their reviewed project files still describe packable libraries. net11-only PackageReference removal/NU1510, older-target references, output-layout assumptions and type conflicts need actual consumer evidence; this audit does not assume all Microsoft.Extensions APIs are framework-provided or establish a new package-cleanup recipe. Historical pre-.NET 11 links in that note do not authorize a historical recipe backfill.

### Deduplication with web work

[PIN-745](https://linear.app/pinguapps/issue/PIN-745) owns binding exclusion plus asynchronous callback completion/failure; [PIN-746](https://linear.app/pinguapps/issue/PIN-746) owns DI/async startup/options restrictions. Neither delivers the provider-failure or explicit filesystem-watcher goals. [PIN-791](https://linear.app/pinguapps/issue/PIN-791) requires actual WASM hosted-service lifecycle evidence; that browser runtime is not the default Generic Host implementation audited here. Do not infer its shutdown behavior from Host.cs or imply durable background work. Keep the shared host-failure explanation canonical and link a separately verified web integration where useful.

[PIN-709](https://linear.app/pinguapps/issue/PIN-709) owns non-Blazor web API reconciliation. Existing web hosting candidates concern TLS/channel binding/protocol compliance, not Generic Host failure exits. The 317-issue overlap search and current registers/content found no delivery owner for A–C. PIN-706 retains release-wide Extensions API/obsoletion/composition reconciliation; these assigned semantic findings are not silently transferred to it. No related ticket has been edited or marked satisfied.

## Delivery proposals for backlog approval

**Approval only: none created or implemented.** Same team/project, existing **02 - Language and library guidance** milestone `c5232470-50ac-4f56-aea8-bd8ff7d13b41`, Backlog. Current allocated maximum: **02-053**. Provisional titles **02-068–02-070** follow PIN-683's uncreated 02-054–02-062 and PIN-684's uncreated 02-063–02-067 proposals. These are not reservations; recheck overlap, milestones and next free numbers in the approved creation order. No existing title/milestone/state changes are proposed.

The native S=2/M=3/L=5 convention is established by the target and comparable issues. Estimates include original samples/prose, actual compilation, focused behavior and installed offline retrieval. Each proposal is one independent PR; numeric order and shared files create no blocking relationship. Relate approved work to PIN-685, the comprehensive coverage gate and the adjacent owners below.

### Shared delivery acceptance

1. Reconcile the full notes with actual selected assemblies before authoring. Independently author a fictional example and a fair same-goal .NET 10/.NET 11 comparison. Preserve stable IDs/editions and the exact eight .NET 10 foundations; do not add another foundation or copy official demonstrations.
2. Deliver full offline metadata/Markdown, discovery ID/title/**goal**, canonical-source snippets, older/newer approaches, migration, constraints, alternatives and official references. Separate runtime, SDK, language, package asset, platform and stability requirements; inspect experimental attributes rather than treating RC as an experimental designation.
3. Starting reproducibility baselines: SDK **10.0.402**, runtime **10.0.13**, C# 14/net10.0; SDK **11.0.100-rc.2.26504.105**, runtime **11.0.0-rc.2.26504.105**, C# 15/net11.0. For the affected libraries, start with Microsoft.Extensions.Hosting / Configuration.Json / FileProviders.Physical **10.0.13 / 11.0.0-rc.2.26504.105** as appropriate. Those are proposed pins, not package assets verified by this audit. Confirm actual availability, asset selection and dependency locks; follow the dependency policy before adding projects/pins. Prefer a verified ordinary framework/package route. Any newer-package/older-target claim needs its own real consumer evidence.
4. Compile and execute actual old/new consumers plus the focused failures below; capture exact SDK, runtime, resolved packages/assets, commands, output and input hashes in the existing evidence harness. Source expectations, mocks and timer sleeps are not proof of real hosting/file notifications. Use bounded waits/signals, isolated temporary files/processes and cleanup. Missing required evidence blocks the delivery PR.
5. Build/pack the ordinary tool, install that exact artifact with the existing stdio harness and an isolated local feed/cache, then retrieve discovery goal and full get Markdown/applicability/edition/evidence offline from an unrelated directory. Check canonical snippets and unsupported targets. Update the register. Add no recurring OS/client/native matrix. RC proof leaves GA/publication gates outstanding; no release or resource provisioning is authorized.

### A. 02-068 - Preserve background-service failures at the host boundary — S (2)

Proposed ID `hosting.observe-background-failures`. A fictional aquarium's scheduled sensor importer fails after startup. Use Microsoft.Extensions.Hosting and the default Generic Host, with a controlled signal to prove startup completed before the failure. Execute real .NET 10/11 child processes; record thrown type/identity, process exit and logged failure separately. Verify RunAsync and explicit StartAsync/StopAsync or WaitForShutdownAsync; cover their synchronous wrappers when advertised. Under StopHost, test one background failure, deterministic multiple failures/aggregation and a shutdown failure. Keep normal cancellation distinct, and verify Ignore does not imply automatic host shutdown. Demonstrate an explicit boundary catch that preserves a nonzero failure exit code, rather than a generic swallow-all workaround.

No claim that StartAsync never threw previously, no new RunAsync API, no browser-host equivalence, no process-code guarantee independent of the application's catch/exit policy. Verify disposal on the selected run path and cleanup of every child process. S covers this bounded portable process/failure exercise. Related: PIN-791, PIN-709. Sources: [full host note][host-note], `host-10/11`, `host-extensions-11`.

### B. 02-069 - Observe configuration load failures separately from changes — M (3)

Proposed ID `configuration.observe-file-load-failures`. A fictional planetarium updates a local display schedule. Use real JSON files and the actual file configuration provider. Check initial load, explicit provider Load(), successful ReloadOnChange, malformed reload, missing required/optional files and deletion/recreation. Record callback exception type, Ignore, provider data and token notifications separately against the matrix above. Establish a successful initial value before testing a failed reload; prove whether values remain or clear rather than promising last-known-good.

Exercise an actual file-open IO failure where the authoring OS permits one (for example a Windows exclusive lock), then recovery; state the measured OS/access condition. Use a minimal controlled IFileProvider/IFileInfo throwing from CreateReadStream only for the arbitrary custom-exception contract. Label that synthetic fault separately: it is not physical IO evidence. Verify no unconditional casts, explicit ignored/unignored policy, no token after ignored initial/open failure, notification after reload clearing, and no reliance on a global unobserved-task event for provider reporting. Check GetFileInfo failure separately to bound callback coverage. Use latches/bounded waits, not an assumed notification count per OS file write. Dispose registrations/provider and remove test files.

Link PIN-745 for consumer coalescing and callback failure handling instead of duplicating it; valid loading and consumer completion are different events. M includes the physical/synthetic failure distinction and state/notification matrix. Related: PIN-745, PIN-746. Sources: [reload note][reload-note], [IO callback note][io-note], `file-provider-10/11`.

### C. 02-070 - Compose a filesystem watcher with the intended root — M (3)

Proposed ID `io.compose-file-watcher-roots`. A fictional botanical garden watches incoming observation files. Use Microsoft.Extensions.FileProviders.Physical and both actual constructor overloads where advertised. Compile/run old/new consumers; record null/invalid root failures, normalization of a relative root, equal/ancestor/descendant/empty watcher paths and unrelated siblings (including a shared text prefix such as root versus root-other). Do not describe previously accepted unrelated paths as functional monitoring.

With real temporary directories/files, prove a relevant token changes for each claimed valid layout, including an ancestor's subdirectories. Construct against a nonexistent root with an empty FileSystemWatcher.Path, create the root later, and observe subsequent file changes. Exercise null FileSystemWatcher with polling only if that route is taught; keep platform restrictions and polling semantics explicit. Dispose watchers and bounded waits, clean up all roots, and document OS/filesystem rather than claiming universal event ordering, symlink security or browser support. M covers real deferred creation/relationship checks and portable limitations; no device/native CI expansion. Related: PIN-745. Sources: [watcher note][watcher-note], `watcher-10/11`, `fileproviders.physical-project`.

## Reproduce source verification

PowerShell 7 and network access to public raw GitHub sources are required. This block verifies reviewed bytes and full-note availability; semantic review still requires reading the dispositions and linked implementations.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3aef0016'
$ErrorActionPreference = 'Stop'
$manifest = Get-Content -LiteralPath './docs/planning/audits/hosting-configuration.sources.json' -Raw | ConvertFrom-Json
$auditTemp = Join-Path ([IO.Path]::GetTempPath()) ('pin685-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $auditTemp | Out-Null
try {
    if ($manifest.sources.Count -ne 33) { throw 'Expected 33 reviewed sources' }
    if (@($manifest.sources.id | Sort-Object -Unique).Count -ne 33) { throw 'Duplicate source ID' }
    $noteIds = @('host-failure', 'ignored-load', 'io-callback', 'callback-rebinding', 'watcher-paths', 'async-options-access', 'explicit-validation', 'shared-framework')
    foreach ($id in $noteIds) {
        if ($id -notin $manifest.sources.id) { throw "Missing note: $id" }
    }
    foreach ($source in $manifest.sources) {
        $expectedUrl = "https://raw.githubusercontent.com/$($source.repository)/$($source.commit)/$($source.path)"
        if ($source.commit -notmatch '^[0-9a-f]{40}$' -or $source.url -cne $expectedUrl) { throw "Unpinned source: $($source.id)" }
        $file = Join-Path $auditTemp ($source.id + '.txt')
        Invoke-WebRequest -Uri $source.url -OutFile $file
        if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $source.sha256) {
            throw "Source hash mismatch: $($source.id)"
        }
        if ($source.id -in $noteIds) {
            $body = Get-Content -LiteralPath $file -Raw
            foreach ($section in @('Version introduced', 'Previous behavior', 'New behavior', 'Reason for change', 'Recommended action', 'Affected APIs')) {
                if ($body -notmatch ('(?m)^## ' + [regex]::Escape($section) + '\r?$')) { throw "Missing section: $($source.id) / $section" }
            }
        }
    }
    $proposals = @(Get-Content -LiteralPath './docs/planning/audits/hosting-configuration.md' | Select-String -Pattern '^### [A-C]\. ')
    if ($proposals.Count -ne 3) { throw 'Expected three approval-only proposals' }
    $proposals
    'PASS: 33 immutable source hashes; eight full notes; three approval-only delivery proposals'
}
finally {
    $resolvedTemp = [IO.Path]::GetFullPath($auditTemp)
    if (-not $resolvedTemp.StartsWith([IO.Path]::GetTempPath(), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected cleanup path' }
    Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
}
```


[host-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/ihost-runasync-stopasync-throw-backgroundservice-failure.md
[reload-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/fileconfigurationprovider-reload-token-load-failure.md
[io-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/fileconfigurationsource-onloadexception-io-errors.md
[callback-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/changetoken-onchange-async-overloads-rebind-callbacks.md
[watcher-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/physicalfileswatcher-validates-root-path.md
[options-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/async-options-validation-sync-access.md
[generator-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/options-validator-explicit-validate.md
[framework-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/extensions/11/extensions-in-shared-framework.md
