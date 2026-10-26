# Recipe inventory

Documentation-backed planning pass, 16 October 2026, using the .NET 11 RC2 preparation baseline. Guidance targets GA, with explicitly experimental APIs included and labelled. This expands the earlier headline coverage map into proposed goals and source dispositions; it does not establish that samples compile or that every public API change has been audited.

## Read by area

- [Languages, shared libraries, runtime, SDK, MSBuild, NuGet and containers](01-languages-libraries-sdk.md): 163 source-disposition rows, including 118 included candidates, 17 grouped rows, 13 exclusions and 15 unresolved findings. The grouped rows include cross-references to the exact eight approved .NET 10 foundations.
- [ASP.NET Core, the complete original Blazor baseline, and EF Core](02-web-data.md): 156 rows, including 127 included candidates, 18 grouped rows, 10 exclusions and one unresolved availability finding. Explicit G01–G25, A01–A07 and E01–E03 mappings retain the 25 guide topics, seven additions and three expansions. Blazor AI's distinct conversation/tool/approval/state goals remain included.
- [WPF, WinForms, MAUI, Android and Apple workloads](03-desktop-mobile.md): 87 rows, including 65 included candidates, nine grouped rows, six exclusions and seven unresolved findings. Includes real VB-specific WinForms authoring rather than assuming there are no VB changes.

The initial planning pass recorded **406 source-disposition rows and 310 included candidate rows**. Later audit additions are identified separately below and in the area registers; those historical counts are not a current delivery total. These are not 310 finalized distinct recipes: source families overlap, shared subtopics need linking, and some goals may split or merge after original samples establish the right boundary. The eight curated .NET 10 foundations remain the previously approved historical backfill, not a full additional release audit.

`included` means eligible under the agreed authoring criterion, pending exact API and example evidence. `grouped` assigns coverage to another goal. `excluded` records an upgrade-only benefit, an out-of-boundary product, or another stated reason. `unresolved` records a missing source/API/availability fact. It does not mean experimental features are disallowed. `unavailable` closes an availability finding for stated artifacts, retaining its history and separate GA recheck.

## Cross-family shaping

Keep one canonical explanation/sample source for shared APIs and link relevant framework integrations. Distinguish genuinely different goals, such as in-memory reconciliation versus a provider-translated database query, or direct async validation versus wiring it into an endpoint/form. Avoid cloning shared explanations merely to appear under several area filters.

Consolidate validation localization, JSON union infrastructure, SDK/mobile test templates, URI/media integration and migration references before freezing IDs. The draft IDs are goal-based review labels, not published identifiers. Once published, preserve them and their release editions under the accepted compatibility policy.

First-party MAUI template integration can document its required Avalonia dependency under the accepted release-family boundary. This does not add standalone Avalonia framework coverage. Fictional example domains and focused migration grouping are authoring choices within the agreed goals, not new product scope.

## Current corrections and remaining evidence

The [EF/provider API audit](../audits/ef.md) compares actual EF10/EF11 RC2
model/query/migration, first-party provider and shipped tooling assets against
123 pinned official files. Existing PIN-842–889 owners remain responsible for
their recipes; 31 additional delivery drafts await PIN-986 review. Default
CS0618/CS0619 removals and unresolved extension contracts remain visible alongside
the global obsolete-index map. Audit closure supplies no database execution,
delivered guidance or comprehensive/GA publication permission.

The [obsolete diagnostic ownership map](../audits/obsolete-index.md) enumerates
all 68 published SYSLIB/EXTOBS obsoletions and separately owns every other row in
the pinned runtime diagnostic registry. Core-summary omissions are explicit;
pipe delivery and two cryptography proposals retain their existing owners.
Index closure does not deliver those recipes or clear PIN-986/PIN-959 gates.

The [automatic/no-change exclusion recheck](../audits/automatic-exclusions.md)
dispositions all thirteen assigned rows against 34 pinned official files and
seven actual FSharp.Core metadata assets. Three compound exclusions are partially
reversed: explicit F# callback annotations, IEEE-decimal result comparisons and
Linux Negotiate channel-binding policy. Three narrow delivery drafts await
PIN-986 approval. Earlier watcher, transport,
WebAssembly and build owners remain explicit; automatic residuals stay excluded.
The initial exclusion counts above are historical. Source/metadata closure does
not deliver these recipes or waive consumer, GA or publication evidence.

The [non-Blazor web API audit](../audits/web.md) compares actual .NET 10 and pinned
.NET 11 RC2 framework, package, web-tooling, C#/F# template and published SignalR
TypeScript assets against 212 immutable official files. Every selected API delta
has an owner. Existing web/migration tickets retain their scope; sixteen additional
recipe drafts await PIN-986 review. gRPC's unchanged selected transport APIs do not
erase its removed Swagger adapter. DirectTls and final TypeScript refresh APIs
retain artifact/documentation gaps. Audit closure delivers no content, runtime
proof or GA/publication permission.

The [native deployment-artifact audit](../audits/native-deployment.md) resolves the
three assigned mitigations against 21 pinned official files. Runtime-setting
precedence and Unix native-library naming retain two approval-only delivery
proposals; caller inspection stays with delivered PIN-742 guidance and saved
Windows native evidence. Static-library naming does not establish supported static
publication. Actual new consumers, Unix toolchains and GA reconciliation remain
delivery obligations; source closure does not clear comprehensive publication.

The [runtime/transport/security API audit](../audits/runtime-security.md) compares
actual .NET 10 and pinned .NET 11 RC2 references, security packages, interop tooling
and packaged browser TypeScript contracts against 167 immutable official sources.
Every declaration difference has an owner. Existing migration/recipe assignments
are preserved; 25 narrow delivery drafts await review. Unqualified native tooling,
required recipe execution and GA evidence remain explicit publication gaps.

The [cryptography platform migration audit](../audits/cryptography-platforms.md) reviews every cryptography compatibility entry, the cryptographic general-index obsoletion and adjacent TLS chain-download policy against 30 immutable sources. It distinguishes native support from API presence, bounds HKDF/AIA behavior and prevents an unsupported RSA provider/padding substitution. Seven narrow delivery drafts await approval; existing key-operation, post-quantum encryption and API/index audit owners retain their scopes. Source closure does not deliver recipes, native execution or publication readiness.

The [SDK/build/package authoring audit](../audits/sdk-build.md) compares actual .NET 10 and pinned .NET 11 RC2 public tooling and build XML against all available canonical preview/RC component notes and the bounded full compatibility/obsolete sources. Existing SDK and migration owners retain their scope; 29 narrow delivery drafts await review, with exact prerequisites, native estimates and real consumer acceptance. Source wording conflicts and residual public-contract questions remain explicit. Audit closure delivers no recipe or comprehensive/GA publication permission.

The [SDK/package-host and affected-test audit](../audits/sdk-package-hosts.md) reconciles seven full migration notes, 32 pinned sources and 23 actual package ZIPs. It corrects template-host framework claims and finds experimental affected-test assets on the public test-tools feed, while retaining unproved .NET 11 SDK interoperability. Nine narrow delivery proposals await approval; source/asset discovery does not deliver recipes or clear publication gates.

The [hosting/configuration failure audit](../audits/hosting-configuration.md) reviews all six assigned full notes and two adjacent Extensions entries against 33 immutable official files. Callback and async-options delivery stays with PIN-745/PIN-746; three additional host-failure, provider-failure and watcher-root delivery proposals await approval. The selected source qualifies broad reload-notification and synchronous-validation claims. Source closure does not establish runtime behavior, deliver recipes or clear publication gates.

The [numeric/parsing compatibility audit](../audits/numeric-and-parsing.md) reviews all eight named full mitigations plus inherited tensor Clamp/Round findings against 24 immutable official source files. It distinguishes delivered numeric subcases from remaining coverage and proposes nine narrow delivery follow-ons for approval. Source-audit completion does not deliver those recipes or clear publication gates; the pinned Clamp implementation qualifies the release-note headline for nonvectorizable types.

The [archive/path/handle/clock compatibility audit](../audits/archive-path-handle-clock.md) resolves all ten assigned notes and cross-checks empty compression against 35 immutable official files. Delivered archive cases stay with PIN-732/733; pending Unix pipe and custom-Type cases stay with PIN-741/742. Five additional delivery proposals await approval. Source closure leaves their authoring, real behavior evidence and installed offline retrieval outstanding; it does not clear comprehensive publication gates.

The [data/text/numerics/I/O API audit](../audits/data-io.md) records actual .NET 10 / pinned .NET 11 RC2 reference, package and regex-tooling differences against complete canonical notes and compatibility sources. Every changed declaration has an audit owner. Ten additional delivery proposals await review; existing numeric/archive migration and recipe owners retain their obligations. The audit does not deliver content or clear publication gates.

The [serialization API audit](../audits/serialization.md) records the bounded .NET 10 / .NET 11 RC2 asset and canonical-source comparison, including package target differences, generator diagnostics and historical obsoletions. It assigns existing goals and proposes two missing delivery slices; their content/evidence and final GA reconciliation remain outstanding.

The web register resolves stale cumulative guidance against canonical RC2 notes for QuickGrid experimental diagnostics, validation-message formatting, browser-option names/lookup, and BasePath namespace. [PIN-692's DBSC audit](../audits/dbsc-availability.md) closes the historical unresolved candidate as **unavailable on pinned RC2**: actual package/API artifacts corroborate the removal. Its RC1 history remains visible and PIN-964 / GA_WEB must separately recheck final artifacts. Include genuinely experimental APIs, but do not teach unavailable RC-only contracts as GA features.

The [mobile composition/status audit](../audits/mobile-baselines.md) reconciles
both RC2 sets against nine actual packages and pinned status sources. Their
18-entry manifest maps are identical despite different set provenance; Apple12212
overrides the historical 12211 dependency/comment for these preparation sets.
Preview XAML, experimental platform NativeAOT and MAUIAI0001 stay visible.
The separately pinned AI package has net10 assets and is not a MAUI RC2 workload
pack. These two source findings are reconciled; historical counts above remain
unchanged. Native compilation and GA evidence remain delivery work.

Native workload evidence includes source/version conflicts and real Mac/Xcode requirements. The documented macios contributor `--no-xcode` mode is not a consumer Apple application build route. No native compilation or runtime behavior was established here.

Remaining delivery work includes full linked migration/compiler-note review, earlier milestone reconciliation where noted, public API diffs, exact SDK/package/workload/native pins, older-target route verification, original previous/current examples, compilation, focused behavior checks, packed-tool/client checks, and GA reconciliation. No unresolved qualifying change may silently disappear or be labelled verified. The compatibility index itself is still evolving; each file states the actual source boundary and gaps.

This is planning evidence only. No sample projects, SDK installs, native/UI/device tests, database execution, tool implementation, package publication, or remote changes were performed for this inventory.
