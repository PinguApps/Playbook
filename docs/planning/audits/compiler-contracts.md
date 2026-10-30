# C# and VB compiler contracts audit

Reviewed **20 October 2026**. This is an official-source audit and delivery proposal, not a delivered recipe or proof of catalogue completeness. The two inventory identities remain `vb.release-review` and `languages.compiler-migrations`. No language version is inferred from the .NET release number.

## Compiler migration delivery reconciliation

### Public compiler-host inspection delivery — 30 October 2026

PIN-1035 and PIN-1036 deliver edition11.0
[`languages.inspect-coalescing-conversions`](../../../content/recipes/languages.inspect-coalescing-conversions/11.0/recipe.md)
and [`languages.inspect-type-layout`](../../../content/recipes/languages.inspect-type-layout/11.0/recipe.md).
The D/E drafts below are historical approved scope. Actual
Microsoft.CodeAnalysis.CSharp/VisualBasic/Common **5.9.0** and
**5.12.0-1.26504.105** packages restore, compile and execute with SDK10.0.402 /
SDK11.0.100-rc.2.26504.105 respectively. The current closure resolves
Analyzers5.12.0-1.26465.4. NuGet.org's observed latest was5.9.0; the exact5.12
pin is distributed on Microsoft's public `dotnet-tools` feed. Samples retain
narrow source mapping and locks; their dependencies remain author-maintained.

The package nuspec identifies **dotnet/dotnet** VMR
`820230c386db5b0106c5430b3e5aee92a7826bf5`, not a Roslyn-repository commit.
Matching public VMR TypeLayout/extension sources resolve. This supplies D/E's
package provenance without relabelling the separate Roslyn-main5.13 audit or
changing the historical release/build-metadata discrepancy below. Actual net11
consumers select `lib/net10.0` Roslyn assets. No older-target route or exact
first-nightly introduction is inferred; only target11 is served and no ninth
foundation is added.

`eng/Test-CompilerInspections.ps1 -Record` captures the original optional-reading
and packet consumers through the existing sample-evidence workflow. Actual C#
and VB-native callers classify nullable numeric, reference and identity cases;
new C# implicit and VB widening flags are checked. Both extensions reject null
and wrong-language operations. The actual older packages reject direct extension
calls with CS0117 and the layout property with CS1061. A bounded older
public-attribute reader remains valid for the explicit packet declarations.
Both closures verify Sequential/2/24, Explicit/1/32 and Auto/0/0: declared
source/metadata values, never computed runtime/interop layout.

The exact ordinary packed artifact is installed through the existing isolated
local feed/cache and inspected with real unrelated-directory stdio list/get.
Discovery ID/title/goal, complete canonical Markdown, target11/edition11,
applicability, current proof and target10 rejection are checked. D demonstrates
C# and native VB; E uses canonical C# for the shared property. No optional
native/provider/browser matrix, experimental opt-in or publication is added.
RC2 proof does not close PIN-959, PIN-961, unresolved API rows or Human gates.

Measured **30 October 2026**.

PIN-1032 and PIN-1034 deliver original edition11.0 guidance for
[`csharp.preserve-span-storage`](../../../content/recipes/csharp.preserve-span-storage/11.0/recipe.md)
and [`csharp.bind-dynamic-conditions`](../../../content/recipes/csharp.bind-dynamic-conditions/11.0/recipe.md).
`eng/Test-CompilerMigrations.ps1 -Record` captures their actual compilation and
execution through the existing sample-evidence workflow. The earlier proposal
text below is historical; approval was recorded in PIN-986 and its
[decision register](../audit-proposal-review.md#compiler-contracts).

| Actual set | Compiler | Reference/runtime | Observed result |
| --- | --- | --- | --- |
| SDK10.0.100, C#14 | 5.0.0-2.25523.111 (b0f34d51) | Microsoft.NETCore.App.Ref10.0.0 / CoreCLR10.0.0 | Inner Span/ReadOnlySpan escape compiles; not executed or recommended. Interface-left &&/\|\| compiles, then both throw RuntimeBinderException on the actual older runtime. |
| SDK10.0.402, C#14 | 5.9.0-1.26505.111 (a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e) | Ref10.0.13 / runtime10.0.13 | Both span forms report CS8352; && and \|\| separately report CS7083. Corrected samples build/run. |
| SDK11.0.100-rc.2.26504.105, C#15 | 5.12.0-1.26504.105 (820230c386db5b0106c5430b3e5aee92a7826bf5) | Ref/runtime11.0.0-rc.2.26504.105 | Same rejection diagnostics; corrected samples build/run. |

**Drift resolved for these two claims:** the pinned main document is a
post10.0.100-through11.0.100 change list. It does not mean every .NET10 compiler
accepts these forms. The actual10.0.402 compiler already rejects them. Metadata
`introducedIn:10.0` records the measured release-family presence, without dating
the exact first compiler build or claiming the syntax itself is new. Only target11
is served; actual corrected net10 comparisons add no ninth foundation. The SDK100
archive is a historical reproducer, downloaded portably with the official SHA512
pin in the helper, not a supported consumer prerequisite. The exact compiler,
reference and CoreCLR hashes and executed commands are in saved
`samples/net*/{SpanStorage,DynamicConditions}/migration-checks.evidence.json`.
No added NuGet dependencies, native assets, providers or optional workloads apply.

**A:** distinct arrays and distinct enclosing span declarations retain independent
manifest batches; mutation checks preserve the other batch. A deliberate shared
buffer proves both views alias the final contents. Both actual corrected
dependency sets execute successfully. The old escape fixture is compile-only.

**B / PIN-1033: evidenced exclusion for supported targets.** The actual10.0.0,
10.0.13 and11 RC2 `System.Runtime.dll` reference assemblies each define
`System.Runtime.InteropServices.InAttribute`. It is in the core reference
assembly, not an optional interop package that an ordinary net10/net11 project
can add back independently. Removing that assembly also removes core type
definitions. No supported deficient reference closure for Playbook's modern
targets was established. The ticket explicitly permits this outcome; no shim,
fabricated corlib, unsupported framework expansion or placeholder recipe is added
under the reserved goal `csharp.resolve-readonly-metadata`.

The paired original `ReadonlyMetadata` controls compile/run with the full real
references. Method-group and lambda synthesized delegates, plus a ref-readonly
local function, all return actual references: a retained value observes42→73.
Reflection over their emitted methods requires a byref return with
`modreq(System.Runtime.InteropServices.InAttribute)` for both delegate Invoke
methods and the local function. The helper also inspects the actual reference
type definition. Compilation/execution records remain beside those samples.
**No deficient-reference CS0518 reproduction or repair is claimed.** The source
entry remains relevant to custom/legacy reference universes outside this
established route; finding a supported deficient closure would reopen delivery.
Normal successful calls or metadata alone are not presented as proof of that
unavailable subgoal.

**C:** both corrected concrete and dynamic routes execute all16 truth-table
combinations, checking results and exact zero/one right-factory invocations.
An unknown implementation is rejected by the concrete guard and fails runtime
binding in the dynamic route. No arbitrary implementation contract is promised.

The declared consumer route remains net11/SDK11/C#15, stable syntax with no preview
opt-in, verified on Windows x64 RC2. Supported requirements and measured pins are
separate in each recipe. The missing compiler source SHA and release/build metadata
conflict below remain explicit provenance limits; current-main5.13 is not relabeled
as RC2. PIN-959, PIN-961, other outstanding delivery and Human gates stay open.
No comprehensive, actual-GA, native/platform-matrix or public publication claim.

## Scope, provenance and evidence

The scope is [PIN-682](https://linear.app/pinguapps/issue/PIN-682), SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9` (stories 47, 93, 95, 158, 159), and approved proposal `b3778cc`, key `SH_Q02`. Consulted the language and desktop inventories, agreed coverage/implementation/verification plans, glossary, contribution guidance and ADRs 0001–0004. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. Historical planning statements that the tool does not exist are superseded by the implemented repository; they do not change this source-only issue's boundary.

The [source manifest](compiler-contracts.sources.json) records 23 immutable raw-source URLs and SHA-256 hashes, plus every changed declaration in the reviewed compiler API files. It deliberately distinguishes:

- **Baseline:** Roslyn `NET-SDK-10.0.100`, annotated tag object `d373c18372f6ff6eb409a65fceb31b09edb50c11`, peeled commit `7463b65f5c25bd1e4180609bf8aaaa9436833a15`.
- **Candidate source:** observed Roslyn main `bd31e3f513f12c738f58ebca2400e588233f1d19`, whose Versions.props reports 5.13.0. This is not a .NET 11 RC2 release manifest.
- **Release documentation:** dotnet/core `ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`, including canonical RC1/RC2 C# and RC2 WinForms notes; dotnet/docs `7a5a9cf8549d2356a135e1686949f4f205cc4843`, including VB history and the compatibility index.

Official entry points: [C# compiler compatibility notes][cs-break], [VB compiler compatibility directory][vb-directory], [VB language source][vb-language], [VB history][vb-history], [RC1 C#][rc1], [RC2 C#][rc2], [RC2 WinForms][winforms], [compatibility index][compat]. The index expressly remains incomplete and has no C#/VB compiler section; it cannot establish a compiler no-change finding. The VB directory has no .NET 11 breaking-change file at the pinned tree. Its .NET 10 file and the actual API/language sources were therefore reviewed too.

## C# compatibility dispositions

Every level-two entry in the pinned .NET 11 compiler document is accounted for below. `included` means eligible delivery remains owed, not already implemented. A compiler change can affect a newer SDK compiling an older target; do not infer a runtime requirement from the document filename or Visual Studio introduction label.

| Source entry / trigger | Disposition and concrete consequence | Delivery owner |
| --- | --- | --- |
| Span/ReadOnlySpan collection expression escapes its declaration block; VS 18.3 | **Included.** Keep storage alive with an array or an appropriately enclosing declaration. Preserve value/lifetime semantics rather than weakening ref-safety checks. | Proposal A |
| Synthesized ref-readonly-return delegate needs `System.Runtime.InteropServices.InAttribute`; VS 18.3 | **Included, grouped with next entry.** Missing reference assemblies produce CS0518. Establish the real deficient reference set before recommending an added reference. | Proposal B |
| Ref-readonly local function needs the same InAttribute; VS 18.3 | **Grouped → B.** Same reference-closure goal, separate repro. Ordinary net10/net11 references must not be described as missing this type without evidence. | Proposal B |
| Interface-typed left operand with dynamic `&&`/`||`; VS 18.3 | **Included.** The compiler now rejects a path that previously failed in the runtime binder. Choose a concrete operator-owning type or deliberate dynamic binding, and check short-circuit behavior. | Proposal C |
| Attribute `nameof(this.member)` / `nameof(base.member)`; .NET 10.0.200 / VS 18.3 | **Excluded as a new .NET 11 goal.** Already a .NET 10 SDK correction; remove the instance qualifier. Do not create a ninth historical foundation. Retain as migration context where affected samples use it. | No new ticket |
| Switch-arm parsing involving a parenthesized constant and contextual `when`; VS 18.4 | **Excluded as an independent goal.** Parser correction, with no separate reusable authored migration established by the note. A real retained workaround would reopen the finding. | No new ticket |
| `with(...)` element changes meaning at C# 15; VS 18.4 | **Grouped → `csharp.configure-collection-construction`.** Use `@with(...)` when the intent is invoking a method named `with`. The language gate matters independently of the SDK. | [PIN-712](https://linear.app/pinguapps/issue/PIN-712) |
| Pointer types permitted in safe contexts; C# 16 / VS 18.7 | **Unresolved release routing.** The note explicitly says C# 16, so it is not ordinary C# 15 guidance. Explicit lambda parameter types can resolve the reported overload ambiguity. Reconcile against the RC experimental memory-safety route. | [PIN-715](https://linear.app/pinguapps/issue/PIN-715), [PIN-961](https://linear.app/pinguapps/issue/PIN-961) |
| `safe` becomes a contextual modifier; C# 16 / VS 18.9 | **Unresolved release routing, grouped with memory-safety reconciliation.** `@safe` preserves an identifier, but a current-main entry does not establish its RC2 opt-in or final version. | PIN-715 / PIN-961 |
| Additional pointer-bearing members require unsafe context; langversion 16 / VS 18.9 | **Unresolved release routing, grouped with memory-safety reconciliation.** Record the CS9363 scenario; do not prescribe the illustrated unsafe-expression syntax as stable C# 15. | PIN-715 / PIN-961 |
| `closed` used as a type/alias name; C# 15 / VS 18.10 | **Grouped → `csharp.closed-hierarchies`.** Escape declaration and relevant uses with `@`; retain the CS9380/CS1519 migration cases. | [PIN-711](https://linear.app/pinguapps/issue/PIN-711) |
| `union` used as a type name; C# 15 / VS 18.10 | **Grouped → `csharp.closed-results`.** Escape with `@`; preserve the CS9370 declaration-ambiguity case. | PIN-711 |

Canonical RC1 selects C# 15 for net11 and stabilizes unions, closed hierarchies, collection arguments, extension indexers, static interface helpers and labeled loop control. RC2 adds no highlighted C# feature. RC1 still puts Unsafe Evolution behind preview plus `updated-memory-safety-rules`. That conflicts with treating the C# 16 main-branch notes as ordinary RC2 features; this audit preserves the conflict instead of selecting an invented release boundary.

## VB findings and existing native-language coverage

The pinned history names **VB 17.13**. `LanguageVersion.VisualBasic17_13 = 1713` is the highest concrete value in the pinned compiler source; Default/Latest map to it. The documented unmanaged-constraint and overload-priority recognition are existing 17.13 capabilities, not evidence of a new VB language version for .NET 11. **No additional VB language-syntax goal was established within this reviewed boundary.** That is narrower than claiming no VB changes.

The VB .NET 10 compatibility document contains two entries: disposed iterator state machines no longer resume user code, and source declarations of `Microsoft.CodeAnalysis.EmbeddedAttribute` must have the specified Friend/NotInheritable/Attribute shape and public parameterless constructor. Both are marked VS 17.13 and belong to the previous release baseline. **Excluded as new .NET 11 recipes**; neither justifies expanding the exact eight historical foundations.

VB-specific framework authoring remains required and already has full delivery tickets:

- **Grouped → [PIN-893](https://linear.app/pinguapps/issue/PIN-893):** `ApplyApplicationDefaultsEventArgs.FormRevealMode` / `.VisualStylesMode`, ApplicationEvents.vb and application-framework defaults. C# startup is not a substitute.
- **Grouped → [PIN-896](https://linear.app/pinguapps/issue/PIN-896):** WFO3011 `WithEvents` declarations and WFO3012 named `Handles` handlers; retain the valid local-component AddHandler route and shared Designer diagnostics. The ticket already requires VB-native samples and compilation.

These are source-supported obligations, not checked-off recipes. No WinForms compilation, Designer round trip or UI execution ran for PIN-682. Their separate authoring/access and Human-proof requirements remain intact.

## Public compiler API comparison

Compared **both Shipped and Unshipped files together**, as ordinal declaration sets, for `src/Compilers/{Core,CSharp,VisualBasic}/Portable/`. Moving an unchanged API between files is not a new API. Counts are textual declaration changes, not distinct members or recipes; experimental markers, `params` and removal bookkeeping affect them.

| Surface | Added lines | Removed lines | Evidence |
| --- | ---: | ---: | --- |
| Core | 62 | 13 | Manifest `publicApiDiff[0]` |
| CSharp | 101 | 2 | Manifest `publicApiDiff[1]` |
| VisualBasic | 5 | 0 | Manifest `publicApiDiff[2]` |

The complete changed signatures are retained in the manifest. Their goal-level dispositions are:

| API family | Disposition / obligation |
| --- | --- |
| CSharp15 enum; union/closed syntax, visitors, factories, conversion flags and symbol queries | **Grouped** with PIN-711 and the existing language delivery slices. Compiler-host traversal support is evidence of the constructs; application recipes do not need to demonstrate every visitor overload. A distinct analyzer-authoring goal remains **unresolved** until an actual use case and release asset establish one. |
| WithElement syntax/factories/visitors; collection operation constructor arguments and placeholder operation | **Grouped** with PIN-712 for collection authoring; a distinct compiler-host goal remains **unresolved**, not implicitly delivered by an application sample. |
| Named break/continue syntax and factory overloads | **Grouped** with existing [PIN-713](https://linear.app/pinguapps/issue/PIN-713), `csharp.control-nested-loops`; analyzer migration is **unresolved** if it needs separate code. |
| MemorySafetyRulesVersion, RequiresUnsafeContext, unsafe expressions and RSEXPERIMENTAL006 members | **Grouped** with PIN-715 for application guidance; exact compiler-host package/opt-in mapping remains **unresolved** against RC2 and current-main differences. |
| Analysis registration / AssemblyMetadata ImmutableArray parameters become `params` | **Excluded** as a standalone goal: existing explicit-array calls remain an authored solution; no distinct migration or outcome was established. Do not count each changed annotation twice. |
| NullableAnalysisIsDisabled / HostOutputs gain RSEXPERIMENTAL001 / RSEXPERIMENTAL004 markers | **Unresolved.** These are potentially new consumer opt-in obligations, not automatic-upgrade exclusions. Verify diagnostics and package-specific attributes before deciding a delivery slice; retain for PIN-961 reconciliation. |
| EmitDifferenceOptions.MethodImplEntriesSupported | **Unresolved.** Requires a real Edit-and-Continue host capability contract and release asset; a Boolean property alone does not establish a complete user workflow. |
| IMethodSymbol / IPropertySymbol.ReduceExtensionMember | **Unresolved** as distinct compiler-host guidance; existing language use is in PIN-712. Method names also occur in SDK 10.0.402, so main's diff alone cannot date introduction. |
| INamedTypeSymbol.TypeLayout and TypeLayout kind/packing/size/equality | **Included source/API goal**, proposal E. RC2 metadata contains the public surface; actual package/target support and compilation remain delivery checks. |
| SourceHashAlgorithm.Sha384 / Sha512 | **Unresolved.** Establish supported Emit/PDB consumer and checksum option contracts before declaring a portable authoring route or a separate goal. |
| RegisterPreCompilationSourceOutput, PreCompilationSourceProductionContext, output-kind/name constants, RSEXPERIMENTAL007 | **Unresolved** as additional release-specific guidance. SDK 10.0.402 already contains the registration method name. Need exact package availability, experimental opt-in and generator-phase behavior, not a fabricated new .NET 11 introduction. |
| OperationExtensions.Descendants / DescendantsAndSelf predicate overloads | **Unresolved.** Compare actual overload signatures and previous package assets; name-only metadata cannot distinguish the new predicate overload from existing traversal. |
| ForEachStatementInfo awaitable properties; GetAwaitExpressionInfo using-statement/declaration overloads | **Unresolved.** Establish exact overload/asset introduction and analyzer use case; SDK 10.0.402 already has GetAwaitExpressionInfo methods. |
| CSharp / VB GetValueConversion(ICoalesceOperation) | **Included**, proposal D. Exposes language-specific conversion information for the non-null operand. VB implementation requires a VB operation and rejects null/wrong-language inputs; the VB example must use binary `If`, not copied C# `??`. |
| CSharp / VB TypedConstantFormattingOptions and ToCSharpString / ToVisualBasicString options overloads | **Unresolved release allocation.** Five VB added lines are GetValueConversion plus four formatting declarations. Formatting methods also occur in SDK 10.0.402: do not call this new VB syntax or a ninth foundation. Confirm the options signature and introduction before proposing a distinct release goal. |
| CSharp SymbolDisplay.FormatPrimitive and its `*REMOVED*` ledger entry disappear | **Unresolved migration evidence.** Two removed text lines do not prove two binary breaks, or a new .NET 11 removal. Inspect old/new shipped assemblies and supported replacement before adding guidance. |

Unresolved rows remain explicit publication work. PIN-961 already covers final language/compiler source/artifact reconciliation and follow-on preparation; this audit does not mark that ticket complete or replace necessary delivery with a future audit. Qualifying outcomes must be delivered before PIN-959's comprehensive gate can pass.

### Bounded artifact inspection

Read ECMA metadata from the locally installed SDK compiler assemblies using PowerShell's PEReader/MetadataReader; did not load or invoke compiler APIs. RC2 SDK `11.0.100-rc.2.26504.105` reports compiler `5.12.0-1.26504.105 (820230c386db5b0106c5430b3e5aee92a7826bf5)`. GitHub returned **422** for that source commit and **404** for content at that ref. Current main 5.13 cannot substitute for that missing provenance.

RC2 DLL SHA-256 values:

- Microsoft.CodeAnalysis: `a85da3cb908792177deea2716e71603836461a1ec930fad674371f78f26a5024`.
- Microsoft.CodeAnalysis.CSharp: `cf91f2a77d8dd79143d6c70bd506bbfc62aff49f46a8754169bd94264650cef1`.
- Microsoft.CodeAnalysis.VisualBasic: `572018c48716dfa1820f9c3776a02086ccaf82f5a9156de5e43d416442f9ba28`.

Observed names include CSharpExtensions.GetValueConversion, VisualBasicExtensions.GetValueConversion, TypedConstantExtensions.ToVisualBasicString and INamedTypeSymbol.get_TypeLayout. SDK 10.0.402 reports compiler 5.9.0 and contains the formatting method name but not those GetValueConversion or TypeLayout names. **Name presence is not a signature, package compatibility, compilation or behavior test.** No package version or language minimum is inferred from it.

The pinned RC2 `release.json` identifies SDK `11.0.100-rc.2.26504.105`, matching the installation; `build-metadata.json` instead names `11.0.100-rc.2.26475.137`. Preserve this source drift. Use the release manifest and measured artifact identity for subsequent verification, not the conflicting build metadata. A GA-looking Git tag also does not establish published GA artifacts; PIN-961 retains its actual-GA availability gate.

## Proposed delivery follow-ons — approval required

**Nothing below has been created or implemented.** The issue explicitly requires backlog approval. Proposed destination is the existing **02 - Language and library guidance** milestone, native ID `c5232470-50ac-4f56-aea8-bd8ff7d13b41`, in PinguApps / Playbook. A scoped, paginated overlap search included all 312 project issues and archived results on 20 October; the highest allocated prefix was 02-048. Proposed numbers are provisional and must be rechecked immediately before approved creation. Existing related tickets keep their names, estimates and states. New issues would be Backlog, natively related to PIN-682, and included as prerequisites of comprehensive coverage when approved.

Each proposal below is a **complete delivery boundary**, with these shared acceptance criteria:

1. Reconcile exact official release/compiler/package assets and the required API signatures. Keep measured SDK pins separate from supported consumer requirements; prove any older-target route. Resolve the relevant source conflict before making a release claim.
2. Author original older/newer samples for the same goal, normally .NET 10 versus .NET 11. Use fictional domains, honest unavailable older equivalents, fair comparisons and VB-native code where specified. Include complete offline prose, migration steps, metadata, applicability, sources and canonical snippets under stable goal IDs. No new .NET 10 historical foundation is authorized.
3. Compile both actual declared dependency sets and capture fresh evidence with the existing workflow. Perform the claim-specific checks below, retain exact outputs and distinguish metadata inspection, compilation and execution. Missing APIs or proof block delivery; do not publish a placeholder recipe.
4. Build/pack the ordinary tool and use its existing isolated installed stdio harness to inspect list/get: ID, title, goal, complete Markdown, target selection, applicability and evidence. Verify demonstrated-language filtering, experimental labels/opt-ins where needed and canonical snippet identity. Update the source register; retain incomplete coverage until all obligations are met. No new OS/native/client matrix or publication is authorized.

### A. 02-049 - Preserve span storage across nested scopes — M (3)

Proposed goal ID `csharp.preserve-span-storage`. Trigger: collection-expression ref-safety break. Deliver a small fictional manifest-batch operation that retains needed values outside an inner block without escaping short-lived storage. Reproduce the old compiler acceptance/new compiler rejection with exact diagnostics, then compile and execute the corrected array/enclosing-scope alternatives. Check that retained values and aliasing match the stated intent; do not accidentally reuse one buffer when independent values are needed. Estimate covers paired compiler repros, behavior, evidence and packed recipe retrieval. Source: first [compiler entry][cs-break].

### B. 02-050 - Restore readonly delegate and local-function references — M (3)

Proposed goal ID `csharp.resolve-readonly-metadata`. Trigger: CS0518 for the two InAttribute cases. Use a real minimal deficient reference-assembly set, not a standard net11 project falsely described as broken. Capture both failures, restore an assembly that actually defines the required type, then verify emitted readonly metadata and successful calls. If no supported Playbook target/dependency route can be established, retain an evidenced exclusion instead of inventing a shim or adding unsupported framework coverage. Estimate includes nonstandard reference closure and metadata checks, plus the complete delivery contract. Source: two [InAttribute entries][cs-break].

### C. 02-051 - Preserve dynamic logical operator binding — M (3)

Proposed goal ID `csharp.bind-dynamic-conditions`. Trigger: CS7083 with an interface left operand. Demonstrate the old RuntimeBinderException honestly, the new compile-time failure, and an intentional concrete-type or dynamic route using original fictional policy objects. Execute true/false and right-operand invocation-count checks to prove short-circuit behavior; casting must not silently promise arbitrary implementations. Estimate covers paired compiler/runtime checks and packed guidance. Source: [dynamic logical-operator entry][cs-break].

### D. 02-052 - Inspect null-coalescing conversions in C# and VB — L (5)

Proposed shared goal ID `languages.inspect-coalescing-conversions`. Trigger: language-specific GetValueConversion extensions expose conversion information omitted by a common-only inspection. Use a small original compiler-host analysis of fictional optional readings, with **VB-native binary If** and a C# counterpart. Pin real Microsoft.CodeAnalysis.CSharp / VisualBasic assets, verify the exact methods and determine honest introduction/target routing before authoring the edition. Compile actual VB and C# callers; check conversion classification for nullable/reference operands, wrong-language rejection and null input where documenting those constraints. Avoid private casts, reflection and a fake new VB language version. Estimate includes two languages, compiler-host dependencies and behavioral/installed retrieval evidence. Sources: [VB implementation][vb-conversion] and manifest API signatures.

### E. 02-053 - Inspect declared type layout through compiler symbols — M (3)

Proposed goal ID `languages.inspect-type-layout`. Trigger: INamedTypeSymbol.TypeLayout exposes kind, packing and size without inspecting internal symbols. Use an original small compiler-host inspection of fictional packet declarations. Verify public package signatures and baseline; compile callers and inspect sequential/explicit/automatic layout declarations with deliberate packing/size. Distinguish declared metadata from a runtime's actual object/interop layout; no sizeof or marshalling guarantee follows from this API alone. C# is canonical for this shared API; add VB only if the authored route materially differs. Estimate covers package/source reconciliation, symbol checks and full packed delivery. Source: manifest Core signatures.

Unresolved API rows above are **not approved extra goals**. Their exact missing release/contract fact is retained for reconciliation; if it confirms another goal, prepare another narrow fully verified delivery proposal for approval. Do not silently fold unrelated generator, debugging, checksum or analyzer workflows into A–E.

## Verification and reproduction

The following PowerShell 7 block selects this worktree explicitly and was verified from an unrelated starting directory. It fetches only pinned public files into a temporary directory, validates all 23 hashes, recomputes all three ordinal API differences and compares the exact signatures with the saved manifest. Network access is required for this source-review reproduction; installed Playbook remains offline. It does not compile or execute recipe examples.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
$manifest = Get-Content -Raw ./docs/planning/audits/compiler-contracts.sources.json | ConvertFrom-Json
$auditTemp = Join-Path ([IO.Path]::GetTempPath()) ('pin682-' + [guid]::NewGuid())
New-Item -ItemType Directory $auditTemp | Out-Null
try {
    foreach ($source in $manifest.sources) {
        $file = Join-Path $auditTemp ($source.id + '.txt')
        Invoke-WebRequest $source.url -OutFile $file
        if ((Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $source.sha256) {
            throw "Source hash mismatch: $($source.id)"
        }
    }
    foreach ($api in $manifest.publicApiDiff) {
        $sets = foreach ($rev in @($manifest.baselineRef, $manifest.candidateRef)) {
            $lines = foreach ($kind in @('Shipped', 'Unshipped')) {
                Get-Content (Join-Path $auditTemp "$($api.surface)-$kind-$($rev.Substring(0,7)).txt") |
                    Where-Object { $_ -and $_ -notmatch '^#' }
            }
            ,@($lines | Sort-Object -Unique -CaseSensitive)
        }
        $diff = @(Compare-Object $sets[0] $sets[1] -CaseSensitive)
        foreach ($direction in @('added', 'removed')) {
            $side = if ($direction -eq 'added') { '=>' } else { '<=' }
            $actual = @($diff | Where-Object SideIndicator -eq $side | ForEach-Object InputObject)
            $expected = @($api.$direction)
            if ($actual.Count -ne $expected.Count -or
                @(Compare-Object $actual $expected -CaseSensitive).Count) {
                throw "API diff mismatch: $($api.surface) $direction"
            }
        }
        "$($api.surface): +$($api.added.Count) -$($api.removed.Count)"
    }
    "PASS: $($manifest.sources.Count) source hashes and all API signatures"
}
finally {
    if ((Split-Path $auditTemp -Parent) -ne ([IO.Path]::GetTempPath()).TrimEnd('\', '/')) {
        throw 'Audit cleanup path is outside the temporary directory.'
    }
    Remove-Item -LiteralPath $auditTemp -Recurse -Force
}
& 'C:\Program Files\Git\cmd\git.exe' diff --check
```

Expected: `Core: +62 -13`, `CSharp: +101 -2`, `VisualBasic: +5 -0`, followed by `PASS: 23 source hashes and all API signatures`; diff checking produces no errors. A changed count/signature or hash fails explicitly. Cleanup removes only the freshly created temporary directory.

Acceptance coverage: compiler compatibility/API review maps to the complete source-entry and API-family dispositions; VB coverage maps to the bounded syntax finding, proposal D and retained PIN-893/PIN-896 obligations; additional confirmed goals map to approval drafts A–E with compilation, behavior and installed-package acceptance criteria. No production code, served recipe or MCP tool changed, so no server build/Inspector call is represented as evidence for this audit. Source review and metadata-name inspection are the only compiler evidence established here; signature/package compatibility, migration repros, original recipe compilation and execution remain delivery work.

Linear boundary: team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, project `b22380c7-6fde-49f4-8826-e5631dc5df7a`. PIN-682 retains the saved **L (5)** estimate: breadth of sources, release conflicts, API comparison and bounded follow-on preparation justify it. No related Linear issue changed. Related full issue reads that affected grouping: PIN-711, PIN-712, PIN-713, PIN-715, PIN-893, PIN-896 and PIN-961. The coverage gate remains [PIN-959](https://linear.app/pinguapps/issue/PIN-959); no claim is made that its requirements are satisfied.

[cs-break]: https://github.com/dotnet/roslyn/blob/bd31e3f513f12c738f58ebca2400e588233f1d19/docs/compilers/CSharp/Compiler%20Breaking%20Changes%20-%20DotNet%2011.md
[vb-directory]: https://github.com/dotnet/roslyn/tree/bd31e3f513f12c738f58ebca2400e588233f1d19/docs/compilers/Visual%20Basic
[vb-language]: https://github.com/dotnet/roslyn/blob/bd31e3f513f12c738f58ebca2400e588233f1d19/src/Compilers/VisualBasic/Portable/LanguageVersion.vb
[vb-history]: https://github.com/dotnet/docs/blob/7a5a9cf8549d2356a135e1686949f4f205cc4843/docs/visual-basic/whats-new/index.md
[vb-conversion]: https://github.com/dotnet/roslyn/blob/bd31e3f513f12c738f58ebca2400e588233f1d19/src/Compilers/VisualBasic/Portable/VisualBasicExtensions.vb
[rc1]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/rc1/csharp.md
[rc2]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/rc2/csharp.md
[winforms]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/rc2/winforms.md
[compat]: https://github.com/dotnet/docs/blob/7a5a9cf8549d2356a135e1686949f4f205cc4843/docs/core/compatibility/11.md
