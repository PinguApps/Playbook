# Serialization API coverage: PIN-704

Reviewed **20 October 2026**. This closes a bounded source/API audit, not recipe delivery. Two additional recipe proposals need review. Existing guidance, missing content and GA reconciliation remain subject to the comprehensive publication gate.

## Scope and provenance

Requirement: [PIN-704](https://linear.app/pinguapps/issue/PIN-704), `SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 97, 158–160; approved proposal `b3778cc`, key `API_JSON`. Consulted the agreed plan, implementation/coverage/verification plans, language/library and web inventories, glossary, ADRs 0001–0004, contribution/evidence rules and PIN-711's delivered serialization evidence. Root `PLAN.md`, `CONTEXT.md` and `CONTEXT-MAP.md` are absent. Historical planning statements that implementation has not started are stale; the installed-tool and evidence infrastructure now exists.

The boundary is System.Text.Json, CBOR, XML serialization and data-contract/schema APIs in the .NET release family. It includes public source-generator diagnostics. It excludes unrelated general data APIs, compression containers, cryptographic ASN.1/CMS contracts and standalone third-party serializers. Those remain with their existing bounded owners; this audit makes no no-change claim about their APIs. gRPC has no distinct candidate in these library sources; its web/source/API review remains PIN-709's responsibility, not an invented serialization recipe.

### Actual artifacts

[The asset manifest](serialization.assets.json) records SHA-256 for every inspected assembly, package and JSON generator binary; complete changed declarations; whole-surface declaration counts/hashes; forwarded types; and the complete obsolete declarations in the bounded surfaces. [The script](../../../eng/Inspect-SerializationApis.ps1) reads metadata without running inspected code or invoking .NET CLI first-run setup.

| Baseline | SDK directory | Reference pack / matching packages | Actual target assets |
| --- | --- | --- | --- |
| Previous | 10.0.402 | 10.0.13 | Microsoft.NETCore.App.Ref `ref/net10.0`; packages `lib/net10.0` |
| Current | 11.0.100-rc.2.26504.105 | 11.0.0-rc.2.26504.105 | Microsoft.NETCore.App.Ref `ref/net11.0`; packages `lib/net10.0` and `lib/net11.0` |

Packages: **System.Text.Json**, **System.Formats.Cbor**, **System.Runtime.Serialization.Schema**. Matching **System.CodeDom** is hashed and used only to resolve Schema metadata; its independent API family is not audited here. All three Roslyn 3.11/4.0/4.4 System.Text.Json generator binaries in each package are hashed. Their supported diagnostic descriptors are compared from the **actual package repository commits**, not runtime main:

- .NET 10: dotnet/dotnet `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`.
- RC2: dotnet/dotnet `820230c386db5b0106c5430b3e5aee92a7826bf5`.

Other packaged TFMs are enumerated and hashed, **not signature-reviewed or declared supported routes**. In particular RC2 removes the dedicated net8/net9 assets while retaining net462/netstandard2.0. This does not prove those applications cannot consume the package through a compatible fallback, or that each API exists there. Playbook's supported target comparison here is net10/net11. Metadata presence alone does not establish consumer compatibility, compilation, trimming or Native AOT behavior.

### Complete canonical source boundary

[The source manifest](serialization.sources.json) pins **73** immutable URLs, source hashes and headings, with all **15 previous / 22 current** generator diagnostic descriptors. It covers:

- dotnet/core `ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`: all nine Preview 1–7 / RC1 / RC2 library notes, all eight published JSON/CBOR preview API diffs, and RC2 `release.json`.
- dotnet/docs `dfe6ae54d3cf986ba03d95075e61ae006fefb9f9`: the complete cumulative libraries article, complete .NET 11 compatibility index, **all 20** files in its core-libraries/11 directory, global obsolete index and relevant historical serialization diagnostic articles, JSON diagnostic articles, and the eleven linked JSON feature articles.
- Both package repository commits above: JSON and CBOR reference declarations, Schema reference declarations and generator diagnostics. RC2 also supplies CBOR reader/writer options, classifier-factory implementation documentation and the net11-only JSON numeric converter reference file.

The compatibility index explicitly says it is incomplete. The source manifest is the exact reviewed boundary, not a promise that future notes or unlisted implementation changes cannot add obligations. The RC2 library note's API-verification comment names **11.0.0-rc.2.26475.137**; the release manifest and actual installed/distributed assets name **11.0.0-rc.2.26504.105**. Use the measured latter baseline and retain this drift. None of this is GA evidence; [PIN-962](https://linear.app/pinguapps/issue/PIN-962) owns final shared-library reconciliation.

## Mechanical comparison and authoring dispositions

**Delivery update — PIN-726–730:** [eleven original JSON recipes and actual managed execution/installed-tool evidence](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-726-730.md) now cover the assigned streaming/writer, generic/typed metadata, encapsulated-model, naming/value/schema and strict-import goals. This supersedes the outstanding delivery wording for those rows below only within the report's measured scope. Stream/options JSONL and both writer Reset destinations are executed; other JSONL overload families remain source-reviewed. Generic resolution checks direct binding and one invalid shape; custom metadata factories, every generic shape, trimming/Native AOT and GA remain outside this proof. The two independent S1/S2 proposals and comprehensive publication gates remain outstanding.

Counts below are declaration-line changes, **not** distinct API or recipe counts. Properties/accessors, constructors, enum fields, nullable attributes and type forwarders can each contribute lines. Flags, optional defaults, generic constraints, custom modifiers and attribute values are retained. Assembly hashes also expose differences outside this declaration representation. This is not a binary-compatibility certification or an implementation diff.

| Actual surface comparison | Added | Removed |
| --- | ---: | ---: |
| Framework System.Text.Json, net10 → net11 | 138 | 2 |
| System.Text.Json package, previous net10 → RC2 net10 | 146 | 18 |
| System.Text.Json package, previous net10 → RC2 net11 | 154 | 18 |
| RC2 System.Text.Json net10 → net11 | 8 | 0 |
| CBOR package, previous net10 → either RC2 asset | 40 | 0 |
| RC2 CBOR net10 → net11 | 0 | 0 |
| All reviewed XML / Runtime.Serialization framework assemblies and Schema package comparisons | 0 | 0 |
| System.Data.SerializationFormat only (System.Data.Common) | 0 | 0 |

The **16** framework assemblies are individually enumerated, including forwarding shims. System.Data.Common is restricted to SerializationFormat; no general System.Data API audit is claimed.

Every changed declaration belongs to one of these groups. `grouped` is an accountable delivery assignment, not a claim that an outstanding ticket has shipped.

| Public API / source finding | Authored impact and disposition | Owner / evidence required |
| --- | --- | --- |
| Four SerializeAsyncEnumerable overloads: Stream/PipeWriter × options/JsonTypeInfo | **Grouped.** Replace manual line framing or array buffering; topLevelValues selects JSONL, compact LF records including the final LF. | [PIN-726](https://linear.app/pinguapps/issue/PIN-726), `json.stream-json-lines`. Real independent record consumption, output framing and cancellation. |
| Two Utf8JsonWriter.Reset overloads accepting options | **Grouped.** Reuse only after flushing, with deliberate new target/options and ownership. | PIN-726, `json.reuse-writer`; reset/flush/stream lifetime checks. |
| Generic GetTypeInfo / TryGetTypeInfo | **Grouped.** Remove casts and distinguish resolution misses from resolver/configuration exceptions. | [PIN-727](https://linear.app/pinguapps/issue/PIN-727), `json.retrieve-typed-metadata`. |
| InferClosedTypePolymorphism options, context and attribute properties | **Grouped.** Explicitly choose inferred versus registered contracts; source generation must contain the types. | [PIN-711](https://linear.app/pinguapps/issue/PIN-711), `json.infer-closed-polymorphism`; existing round-trip/precedence proof, not every possible hierarchy. |
| JsonUnionAttribute; built-in structural classifier; Union kind; JsonUnionCaseInfo; JsonTypeInfo union cases/constructor/deconstructor; JsonUnionInfoValues / CreateUnionInfo; new PolymorphismOptions on object/collection info values | **Grouped.** Union and polymorphic metadata infrastructure supports the existing goal. Delegate Invoke/BeginInvoke/EndInvoke and generated-contract DTO accessors do not each justify a recipe. Bespoke manual metadata tooling has no independently established goal here; do not claim its behavior verified. | PIN-711, `json.serialize-closed-results`. Its existing report explicitly does not implement a custom factory. |
| JsonTypeClassifier delegate/context/kind, JsonTypeClassifierFactory and generic factory; attribute TypeClassifier, options/context TypeClassifiers, metadata TypeClassifier/TypeClassifierFactory hooks | **Included, uncovered use case → proposal S2.** Authored value-based classification exceeds the built-in root-property-name matching already demonstrated. Same extension point serves ordinary polymorphic types and unions. | S2 owns custom implementation/registration, not duplicate structural classification or language syntax. |
| PascalCase policy/enum; JsonNamingPolicyAttribute constructors/property; JsonIgnoreAttribute AttributeUsage 384 → 1420 | **Grouped.** Type/member naming and type-level ignore defaults change authored annotations; precedence and invalid type-level Always must be explicit. The removed old AttributeUsage declaration is replaced, not a removed type. | [PIN-729](https://linear.app/pinguapps/issue/PIN-729), `json.configure-member-conventions`. |
| Four numeric converter properties and CreateIReadOnlySetInfo | **Grouped.** Numeric/collection contracts can replace custom conversion and concrete-set scaffolding. **The eight net11-only lines are the four numeric converter properties plus getters.** They are absent from RC2 net10. | PIN-729, `json.use-numeric-collection-contracts`; no blanket older-target numeric route. |
| JsonObjectInfoValues type nullable-context annotations; existing package getters on JsonPolymorphismOptions, JsonPropertyInfo and JsonTypeInfo; Utf8JsonWriter async state-machine attribute renumbering; ParamCollection/ScopedRef type forwarders | **Excluded as independent recipes.** Existing member signatures remain; these are annotation/implementation/forwarding bookkeeping, not eighteen removed callable APIs or an established different application workflow. Preserve exact lines for compatibility review. | No additional ticket. Do not discard actual new DTO members grouped above. |
| CborReader options/final-block constructors, Reset with final block, SlideData, NeedsMoreData, TrySkipValue and TrySkipToParent | **Grouped.** Replace whole-input buffering and handle incomplete reads without losing retained bytes/state. TrySkip is part of incremental processing, not a separate recipe. | [PIN-725](https://linear.app/pinguapps/issue/PIN-725), `cbor.read-chunks`; actual chunk splits, Lax-only incremental mode, incomplete skip/retry and final-block failure. |
| CborReaderOptions / CborWriterOptions, MaxDepth on options/readers/writers and options constructors | **Included, uncovered migration → proposal S1.** Reader/writer depth limits require deliberate payload acceptance and rejection. Other option properties retain existing conformance, multi-root, indefinite-encoding and initial-capacity choices; group them with the options migration rather than inventing an allocation recipe. | S1; keep the incremental algorithm in PIN-725. |
| XML serializers, DataContractJsonSerializer, DataContractSerializer, serialization attributes/formatter types, XsdDataContractExporter/Importer and related Schema APIs | **Excluded as new goals within this boundary.** No declaration/forwarder delta and no qualifying new feature or migration in the pinned family sources. Implementation-only fixes do not imply a changed authored approach. | No standalone no-change recipe. Source/API evidence is narrower than a behavioral-equivalence claim. |

### Source-only findings without a new signature

These are essential: an API diff alone cannot capture changed serializer behavior.

| Source finding | Disposition |
| --- | --- |
| Preview 1 read-only sets; Preview 2 typed metadata; Preview 3 naming/ignore defaults; Preview 4 F# unions/private accessors/writer reset; Preview 5 JSONL; Preview 6 C# unions; Preview 7 inference; RC1 structural classification/numerics/binary schema; RC2 CBOR/JSON fixes | All map to the API groups above or the behavior rows below. Earlier preview C# `preview` labels are superseded by RC1 C# 15 release evidence; do not copy them into every RC2 requirement. |
| F# class/struct/recursive unions and naming rules; reflection-only, no source generation/trimming/Native AOT | **Grouped → [PIN-716](https://linear.app/pinguapps/issue/PIN-716)**, `json.serialize-fsharp-unions`; [original .NET 10/11 F# samples and packed-tool evidence](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-716-718.md), not translated C# union proof. |
| Open generic derived registrations / converters, constraint/arity resolution and SYSLIB1229 | **Grouped → PIN-727**, `json.configure-generic-contracts`. Warning suppression is not a functioning resolution strategy. |
| Private included members/constructors; omitted init initializers; in/ref/out/ref-readonly constructors; generic unsafe accessor constraints and RC2 unsafe fixes | **Grouped → [PIN-728](https://linear.app/pinguapps/issue/PIN-728)**, the existing three encapsulated-model goals. Actual generation/constructor round trips remain required. |
| IReadOnlyDictionary extension data, JsonObject overflow flattening; byte[] / Memory<byte> / ReadOnlyMemory<byte> schema contentEncoding | **Grouped → PIN-729**, including existing `json.describe-binary-schema`. Automatic schema metadata alone adds no independent authoring goal; remove a demonstrably redundant custom transform where useful. |
| RC2 nested closed inference, union number-handling and corrected custom-converter diagnostic | **Grouped → PIN-711** for existing union/inference behavior; custom-factory behavior is S2. Existing evidence is not proof of every newly enumerated case. |
| Faster JsonElement small-integer reads / internal ThrowHelper | **Excluded:** automatic benefit with unchanged authored code. |

### Generator diagnostic inventory

The package-matched source has **15 → 22** public descriptors: two removed, nine added, thirteen retained. The script checks extraction against the total descriptor count, including the final declaration in each file.

- **Removed SYSLIB1038 / SYSLIB1222:** inaccessible included member / constructor restrictions; grouped with PIN-728's real supported generation route. Absence of a warning is not proof that arbitrary internals serialize.
- **Added SYSLIB1226:** invalid type-level Always; PIN-729. **1227/1228:** unclassifiable/unsupported union cases; PIN-711 and S2 where custom classification is the remedy. **1229:** unresolved open generic derived registration; PIN-727.
- **Added SYSLIB1240–1244:** unsupported derived type, inaccessible inferred type, discriminator collision, non-closed opt-in (error), explicit derived registrations overriding inference (warning); grouped with PIN-711's inference constraints. Detailed diagnostic coverage remains a content obligation, not a new recipe per warning.
- **Retained SYSLIB1030–1036, 1039, 1220/1221, 1223–1225:** no new standalone release goal established. In particular **1225 already exists in 10.0.13**; do not invent a new .NET 11 ref-like-member migration from the newer descriptor list.

The pinned documentation has no 1240–1244 diagnostic article. Package-matched descriptors retain these exact IDs/severities despite that documentation gap.

### Obsolete-index ownership

Reviewed both the complete global SYSLIB index and the .NET 11 obsolete-API article. The latter lists only **0064** (cryptography) and **0066** (Extensions startup validation); neither belongs to this family. PIN-689 remains the overall index-to-owner map. No newly obsolete serialization declaration appears in these artifact comparisons.

| Retained diagnostic | Family disposition |
| --- | --- |
| SYSLIB0011, 0038, 0050, 0051 | BinaryFormatter, binary DataSet format and formatter-support mechanisms are historical obsoletions already present in the previous artifacts. **Excluded as new .NET 11 recipe goals**, not endorsed for new applications; no extra .NET 10 historical foundation. |
| SYSLIB0020 | IgnoreNullValues is historical; use DefaultIgnoreCondition when a relevant authored contract needs it. **Grouped as migration context with PIN-729**; no new standalone obsoletion. |
| SYSLIB0049 | AddContext is historical; resolver/context registration is **grouped with PIN-727/728**. Do not imply the replacement first appeared in .NET 11. |
| SYSLIB0047 / 0062 | XML external-resolution / unsupported XSLT script mechanisms are historical, unchanged here. **Excluded as new serialization goals**. Keep XML build-task DTD migrations with the existing SDK/build audit PIN-707, rather than duplicating it. |

The manifest preserves obsolete declarations lacking a custom SYSLIB ID too. No unknown newly obsolete serialization member is silently assigned to the old index task.

## Proposed recipe tickets for review

**No follow-up ticket is created by this proposal.** Both are complete delivery slices in existing milestone **02 - Language and library guidance** (`c5232470-50ac-4f56-aea8-bd8ff7d13b41`), PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project `b22380c7-6fde-49f4-8826-e5631dc5df7a`. The scoped paginated overlap review covered 312 issues, including completed issues; neither goal is explicit in the existing tickets read above. Before approved creation, requery including archived issues, allocate unused `02-DDD` numbers after the then-highest allocation, relate each to PIN-704 and the comprehensive gate, and save/read back its estimate. Do not rename existing tickets or invent native identifiers. Native examples confirm XS=1, S=2, M=3, L=5; these proposals use **M (3)** including authoring and verification.

Shared acceptance for **each** proposal:

1. Author wholly original same-goal .NET 10/11 samples, offline prose/metadata and canonical snippets, retaining stable goal/edition identity and fictional examples. No copied guide demonstration or ninth historical foundation. State when the older solution remains valid.
2. Use actual declared SDK/target/package assets and source-supported consumer requirements; record exact compilation and claim-specific real execution, resolved dependencies and fresh input hashes through the existing evidence workflow. RC proof stays RC proof. Experimental opt-ins, trimming/AOT limitations and any optional package route need actual evidence; unavailable APIs or required proof block delivery.
3. Build/pack the ordinary tool, install that artifact with the existing isolated-feed/cache harness from an unrelated directory, and call real stdio list/get with explicit targets. Verify ID/title/**goal**, full Markdown, applicability, snippets, evidence, edition selection and matching structured/text JSON offline. Reuse the harness; no OS/client/native matrix.
4. Update this register and the existing inventory without marking comprehensive coverage/publication ready. Link PIN-962's future GA reconciliation and PIN-959's publication gate. These are delivery/evidence dependencies, not authorization to release.

### S1 — Bound nesting when reading and writing CBOR — M (3)

Proposed goal: `cbor.bound-nesting`. As a developer exchanging nested measurements, I want explicit depth limits so valid batches remain accepted and excessive nesting fails predictably.

- **Exact prerequisites:** existing packed-tool/evidence pipeline (PIN-668, delivered); actual .NET 10 SDK 10.0.402 with System.Formats.Cbor 10.0.13 `lib/net10.0`; .NET 11 SDK 11.0.100-rc.2.26504.105 with package 11.0.0-rc.2.26504.105 `lib/net11.0`. These are reproducible test pins, not universal SDK requirements. No cloud resource, account, workload or Human/native proof prerequisite. PIN-725's streaming delivery is independent; do not invent a blocker between them.
- **Changed authoring:** a fictional observatory imports nested calibration groups. The old sample applies its own finite budget; the new sample configures CborReaderOptions/CborWriterOptions and handles their distinct failures. Keep conformance, multiple-root and indefinite-encoding choices explicit when moving to options. This is one finite-payload goal; chunk processing stays with PIN-725.
- **Real evidence:** execute actual older/newer consumers, proving identical accepted values, deliberate rejection at the configured boundary and no silent truncation. Reproduce reader default 64 / writer default 1000 and exception types from the breaking note, then test explicit finite limits. Verify **0 means no nesting**, **-1 selects runtime defaults**, and values below -1 fail. Do not recommend an imaginary AppContext unlimited-depth switch. Include arrays/maps and indefinite containers where claimed; capture reader and writer outputs separately.
- **Sizing:** paired package consumers, boundary behavior, complete original recipe and installed retrieval; M is defensible without expanding into an incremental parser or generic security guide.

### S2 — Choose JSON variants from payload values — M (3)

Proposed goal: `json.classify-payload-values`. As a developer receiving records with the same field names, I want to select their meaning from their values so both record shapes deserialize predictably.

- **Exact prerequisites:** PIN-668's delivered pipeline; ordinary .NET 10 and .NET 11 SDK/target pins above and their matching System.Text.Json assets. Use an ordinary polymorphic hierarchy to keep this slice independent of C# union syntax; canonical source confirms the factory supports polymorphic derived types as well as unions. PIN-711's existing structural-classifier recipe supplies cross-reference context, not proof of a custom factory. No external service or native/Human prerequisite.
- **Changed authoring:** a fictional tide instrument reports positive/negative offset records with the same root properties. Compare a valid older custom converter with an RC2 JsonTypeClassifierFactory (or its typed base) and a reader delegate that chooses only registered candidate types by a value rule. Do not reuse the already delivered property-name structural example. Document immutable context, factory configuration versus delegate invocation, reader-position contract, attribute/global registration precedence and generated metadata requirements. Keep JsonCollectionInfoValues/JsonObjectInfoValues manual scaffolding as supporting metadata, not a generator-tooling framework.
- **Real evidence:** compile and execute both consumers; verify selected concrete type and complete payload values, a read-only/copy-based lookahead that preserves deserialization state, property order independence, unknown/malformed values, no-match/unregistered returns and the chosen null behavior. Demonstrate per-type registration winning over a conflicting global factory and source-generated metadata containing all candidate types. Verify any claimed net10-plus-package route independently before publishing it; the audit proves signatures only.
- **Unresolved source conflict to settle during delivery:** the pinned factory remarks say a non-nullable union rejects JSON null, whereas the cumulative article and PIN-711's measured union examples describe a default union value. This proposal uses ordinary polymorphism and must verify its own null contract; do not generalize either claim to custom union factories without actual execution. Union-specific custom-classifier null semantics remain explicitly unresolved under this goal's future extension / PIN-962 reconciliation; they are not evidence that existing compiled examples are wrong.
- **Sizing:** one value-based classifier, fair older comparison, bounded registration/error checks and packed retrieval; M. Additional union/AOT/native-generator products are outside this slice and require a fresh bounded proposal if separately justified.

## Verification and limitations

From a PowerShell 7 shell, with the two exact SDK/reference-pack directories available (override both roots for other installations):

```powershell
Set-Location V:\_worktrees\Playbook\t3-def9847c
$pwsh = (Get-Command pwsh).Source
& $pwsh -NoProfile -File ./eng/Inspect-SerializationApis.ps1 `
  -Net10Root 'C:/Program Files/dotnet' `
  -Net11Root 'C:/Users/pingu/.dotnet-playbook-11' `
  -OutputPath ./artifacts/serialization.recheck.json `
  -CompareWith ./docs/planning/audits/serialization.assets.json
if ($LASTEXITCODE -ne 0) { throw 'Serialization audit failed' }

$audit = Get-Content ./artifacts/serialization.recheck.json -Raw | ConvertFrom-Json
$audit.publicApiDiff | ForEach-Object {
  '{0}: +{1} -{2}' -f $_.asset, $_.diff.added.Count, $_.diff.removed.Count
}
($audit.publicApiDiff | Where-Object asset -EQ 'system.text.json/RC2-net10.0->net11.0').diff.added
```

Expected: exact asset/surface comparison passes, all 73 immutable official source hashes match, generator extraction includes every descriptor, and the eight net11-only numeric converter declaration lines appear. XML/serialization/Schema comparisons report zero changes. Network access to NuGet and immutable GitHub source URLs is required **for this authoring audit**, not for installed Playbook. No restore/build/server is needed for metadata inspection. A changed hash, missing artifact/source, omitted descriptor or comparison mismatch is a failure, not a no-change finding.

Actual checks: generated the complete inventory from real packages/reference assemblies; reran the saved comparison and all source hashes; verified rejection of a modified comparison baseline and an omitted final generator diagnostic; checked PowerShell syntax and unchanged user/machine PATH. See the PR for current CI/review results. No serializer recipe sample, custom factory, CBOR execution, Native AOT publish or MCP content change was implemented or claimed here. Missing real behavior proof belongs to the delivery acceptance above. Existing source findings were cross-referenced, not marked delivered. All three PIN-704 criteria are covered by the artifact/source manifests, dispositions and the two fully specified delivery proposals.

## Delivered incremental CBOR goal (PIN-725)

[Original net10/net11 comparisons and execution evidence](../../verification/process-platforms.md) now cover stable goal cbor.read-chunks, edition 11.0: Lax/non-final reader creation, NeedsMoreData, unconsumed-tail SlideData, every split including multi-byte tokens, one-byte chunks, final truncation and trailing-root rejection. System.Formats.Cbor 10.0.13 and 11.0.0-rc.2.26504.105 use their actual net10/net11 assets and lock files. This delivery does not satisfy proposed S1 nesting or S2 classification, advertise a net10-plus-new-package route, or establish GA/publication readiness.
