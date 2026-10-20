# Data, text, numerics and I/O API coverage: PIN-703

Reviewed **20 October 2026**. This is a bounded source/API audit, **not delivered recipe content or permission to publish**. Ten uncovered goals below are delivery proposals for review; no follow-up issues have been created.

## Scope and provenance

[PIN-703](https://linear.app/pinguapps/issue/PIN-703), `SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 96, 98, 99, 101, 158–160; approved proposal `b3778cc`, key `API_DATAIO`. Consulted the agreed planning README, coverage, implementation and verification plans, language/library inventory, glossary, ADRs 0001–0004, contribution/evidence rules, and the neighboring serialization audit. Root `PLAN.md`, `CONTEXT.md` and `CONTEXT-MAP.md` are absent. Historical planning statements that implementation has not begun are stale: the package, samples and evidence harness now exist.

The bounded family includes collections/LINQ, spans/buffers/text/encoding/globalization, numeric types and SIMD, files/pipes/processes, compression/TAR/ZIP, noncryptographic checksums and OPC packaging. It includes public/protected signatures, generic constraints, annotations, defaults, forwarding, obsoletions and regex public tooling. It excludes JSON/CBOR/XML serialization (PIN-704), transport/security/threading/interop outside these family surfaces (PIN-705), Extensions (PIN-706), SDK/build tooling (PIN-707), web/gRPC (PIN-709), and native UI controls. No absence-of-change claim is made for those excluded families.

Only native blockers are dependencies. The target has none. Milestone order and shared files do not create dependencies. The full scoped overlap query returned **312** issues in PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project `b22380c7-6fde-49f4-8826-e5631dc5df7a`; every returned boundary was checked. Existing source-finding owners PIN-683, PIN-684 and PIN-689 and the recipe owners named below retain their full obligations; this audit does not close or resize them.

## Reproducible artifact and source boundary

[Inspect-DataIoApis.ps1](../../../eng/Inspect-DataIoApis.ps1) reads metadata without executing inspected assemblies, restoring packages, running SDK setup or changing PATH. [The artifact manifest](data-io.assets.json) stores complete declaration deltas, complete obsolete declarations, assembly/package/tooling SHA-256 values, and whole-surface declaration counts/hashes. The report also hashes both input manifests. [Disposition rules](data-io.dispositions.json) assign **every** added/removed declaration to a named group; an unowned delta fails inspection. Rules classify audit responsibility, not sample correctness. Detailed member-level decisions follow below.

| Baseline | Actual SDK | Reference pack and matching NuGet packages | Compared assets |
| --- | --- | --- | --- |
| Previous | 10.0.402 | 10.0.13 | Microsoft.NETCore.App.Ref `ref/net10.0`; packages `lib/net10.0` |
| Current | 11.0.100-rc.2.26504.105 | 11.0.0-rc.2.26504.105 | Microsoft.NETCore.App.Ref `ref/net11.0`; packages `lib/net10.0` and `lib/net11.0` |

There are **48 selected reference assemblies per baseline**, including forwarding facades. In the large Runtime/Runtime.Extensions/Runtime.Handles assemblies, the script's explicit type predicate selects numeric primitives, string/character/span/memory/date/time types, the Buffers/Collections/Globalization/IO/Numerics/Text namespaces and file/pipe/process safe handles. Other selected assemblies are inspected in full, including protected members. Actual signatures, not XML member names, determine the diff. Attribute-only changes intentionally remain visible rather than being mistaken for additions/removals of callable methods.

NuGet packages: **System.IO.Hashing**, **System.IO.Packaging**, **System.Numerics.Tensors** at both versions above. Nuspecs identify package-matched dotnet/dotnet commits **`4099f83977f4d29604a9aeb971fb0a7bc1d5f699`** and **`820230c386db5b0106c5430b3e5aee92a7826bf5`**. Other packaged TFMs are enumerated and hashed, not signature-reviewed or advertised as compatible routes. All three RC2 packages have identical reviewed net10/net11 declaration surfaces; that is **metadata evidence only**, not runtime, trimming/AOT or older-target consumer proof.

The actual regex generator from each reference pack and MetadataLoadContext from each SDK are hashed. Package-matched regex generator descriptors are reviewed at both repository commits: six retained descriptors **SYSLIB1040–1045**, with no new descriptor or severity change. Regex `AnyNewLine` is an authored option; generator fixes and its improved code fixer do not themselves add another goal. Neither a generator hash nor source review proves generated code executes.

[The source manifest](data-io.sources.json) pins **151 immutable sources**, their SHA-256 values and headings:

- dotnet/core **`ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`**: all nine Preview 1–7/RC1/RC2 **library and runtime** notes, RC2 `release.json`, and every published preview API-diff file matching the family assembly names. RC1/RC2 do not provide the same per-assembly preview-diff series; the actual pack comparison supplies the end-to-end boundary.
- dotnet/docs **`dfe6ae54d3cf986ba03d95075e61ae006fefb9f9`**: complete cumulative library/runtime articles, .NET 11 compatibility index, **all 20** core-libraries/11 files, the globalization/11 and jit/11 entries, global obsolete index and relevant diagnostic articles. Some in-scope changes appear only in earlier preview notes or actual metadata, so the cumulative article alone is insufficient.
- Both package repository commits: 17 family reference declaration files plus regex diagnostic descriptors at each commit. This bounds public-tooling review to the actual release assets rather than runtime main.

These source boundaries are complete **at the recorded commits**, not a claim that Microsoft's evolving compatibility index is complete, that every internal runtime behavior is unchanged, or that RC proof is GA proof. PIN-962 owns final GA reconciliation; PIN-959 retains the comprehensive content/compilation gate.

## Dispositions of actual public differences

All changes require authored code unless otherwise stated. No uncovered markup, build configuration or platform-manifest goal was established in this family; the explicit file-locking configuration change remains assigned below. Grouping means an existing delivery owner must account for the named subcase; it does not assert that the recipe currently exists or has verified it.

| API group | Impact and disposition | Accountable delivery/evidence |
| --- | --- | --- |
| BitArray span constructors/PopCount; key-selector EqualityComparer.Create | **Grouped:** construct compact bits without ToArray; select consistent key equality/hash behavior. | [PIN-740](https://linear.app/pinguapps/issue/PIN-740): original bit counts, ordering and equality checks. |
| FrozenDictionary CollectionBuilder annotation | **Grouped:** compiler construction support, not a distinct collection algorithm. Do not copy Preview 1's dictionary-expression syntax into RC2 guidance without compiling it. | [PIN-712](https://linear.app/pinguapps/issue/PIN-712): collection-construction constraints; existing sample evidence is not FrozenDictionary proof. |
| Enumerable/Queryable/AsyncEnumerable FullJoin and tuple-returning Join/GroupJoin/LeftJoin/RightJoin | **Grouped:** retain both unmatched sides or return pairs without result-selector scaffolding. LeftJoin/RightJoin themselves already exist in .NET 10. Async overloads include cancellation-aware selectors. | [PIN-734](https://linear.app/pinguapps/issue/PIN-734); provider translation remains distinct from API availability. [PIN-735](https://linear.app/pinguapps/issue/PIN-735) retains the .NET 10 asynchronous-stream foundation. |
| Process.Run/RunAsync, text/byte/line capture, ProcessOutputLine/TextOutput/ExitStatus; detached/forgotten/parent-bound lifetime | **Grouped:** remove custom output/cancellation/lifetime plumbing. Preview positional parameter/type changes are not independent stable-release migrations. | [PIN-721](https://linear.app/pinguapps/issue/PIN-721); verify cancellation, output streams, exit and parent-lifetime behavior on each declared platform. |
| SafeProcessHandle start/open/lookup/wait/signal/resume; inherited and standard handles; Windows/Unix native-start callbacks; Console standard handles and File.OpenNullHandle | **Grouped:** explicit ownership and native startup setup; null/standard handles belong with the redirection goal rather than a new file recipe. | [PIN-722](https://linear.app/pinguapps/issue/PIN-722); OS/race/disposal checks remain real platform prerequisites. FORCE_COLOR/NO_COLOR configuration stays PIN-721. |
| ReadOnlyMemoryStream, WritableMemoryStream, StringStream, Buffers.ReadOnlySequenceStream; StringBuilder.MoveChunks | **Grouped:** remove buffer copies and explicitly transfer builder ownership. Preserve encoding, fixed size, seek/write capability and source lifetime. | [PIN-723](https://linear.app/pinguapps/issue/PIN-723); StringStream constructors and all stream overrides are one adapter goal, not a recipe per member. |
| Rune string/search/split/trim/replace, StringBuilder/TextWriter/TextInfo overloads; RunePosition and enumerators; Rune parsing interfaces | **Grouped:** scalar-safe text and position/replacement reporting. Offsets remain code units/bytes, not grapheme indexes. | [PIN-724](https://linear.app/pinguapps/issue/PIN-724). Exact RC2 RunePosition members replace assumptions from the early preview demonstration. |
| Char/Rune comparison, ordinal upper/lower casing on Char/String/Rune/spans, whitespace search helpers, UTF validation/invalid-subsequence search, RegexOptions.AnyNewLine | **Grouped:** text comparison, normalization, delimiter/error boundaries and newline policy. Ordinal casing is not culture-aware display casing. | PIN-724; check culture-sensitive input, supplementary scalars, malformed UTF, whitespace and anchors/dot semantics. No independent recipe is proposed for each overload. |
| Base64 character/UTF-8 buffer convenience and Try APIs | **Grouped:** remove Convert/adaptation plumbing while retaining consumed/written/final-block semantics. | [PIN-725](https://linear.app/pinguapps/issue/PIN-725); CBOR remains its separate serialization-owned subgoal. |
| TextWriter string/token and token-only newline overloads | **Included, uncovered D7:** cancellable output differs from Unicode transformation. Preview 1's claim of token overloads for *all* forms is broader than the actual new RC2 signatures; teach only verified signatures. | D7. Existing memory/token routes remain fair older alternatives. |
| IdnMapping.TryGetAscii/TryGetUnicode span destinations | **Included, uncovered D8:** IDN conversion into caller-owned buffers replaces intermediate strings. | D8; culture, IDNA/platform and malformed-input behavior require real evidence. |
| Decimal32/64/128, IDecimalFloatingPointIeee754, Complex<T> and supporting conversions/operators | **Grouped:** deliberate precision/range/special-value representation and generic math. New IEEE decimal types are not System.Decimal. | [PIN-736](https://linear.app/pinguapps/issue/PIN-736); corrected conversion/special-value migrations retain PIN-683/PIN-737. |
| TryParsePartial, NumberStyles.HexFloat, BigInteger/binary numeric conversions, BFloat16 bit/binary primitives | **Grouped:** consumed lengths and exact representation intent. IEEE hex formatting is not proof that NaN payloads survive every textual round trip. | [PIN-737](https://linear.app/pinguapps/issue/PIN-737); numeric/parsing behavior findings remain [PIN-683](https://linear.app/pinguapps/issue/PIN-683). |
| BFloat16 full numeric type | **Included, uncovered D10:** choosing reduced precision is a different goal from bit encoding. BitConverter/BinaryPrimitives support remains PIN-737; don't duplicate it. | D10; actual loss/range/NaN checks required. |
| DivisionRounding and IBinaryInteger Divide/DivRem/Remainder | **Included, uncovered D2:** select an integer quotient/remainder convention rather than adjusting `/` and `%` manually. | D2. Not covered merely by a floating conversion/Math.Round migration. |
| Primitive/IBinaryInteger Log10 | **Included, uncovered D3:** integer magnitude/digit budgets without floating conversion or a decimal string. | D3; zero/negative/large-integer contracts remain to execute. |
| Random.NextInteger/NextBinaryFloat | **Grouped:** generic constrained random generation. | [PIN-738](https://linear.app/pinguapps/issue/PIN-738); bounds and noncryptographic limits. |
| Vector/Vector64–512 sequences, Zip/Unzip/Concat/Reverse, SVE/SVE2/SVE crypto and x86 additions including AvxVnni.V512, Avx512Bmm, Avx10v2; other ISA overloads | **Grouped:** architecture-aware algorithms and lane arrangement, including new overloads not highlighted in the cumulative article. | [PIN-739](https://linear.app/pinguapps/issue/PIN-739); actual instruction support/fallback and hardware prerequisites, not a no-change finding about newly exposed ISA APIs. |
| RequiresUnsafe annotations on pointer constructors/encoding/MemoryHandle/SequenceMarshal/vector loads and stores | **Grouped:** changed safe/unsafe caller obligations. Many raw add/remove pairs differ only by this annotation; they are not thousands of new algorithms or removed methods. | [PIN-715](https://linear.app/pinguapps/issue/PIN-715); preserve its measured preview-feature limits and compile affected callers when delivering those cases. |
| Compiler-emitted vector extension marker types and public EditorBrowsable(Never) expression Closure storage/constructor | **Excluded as standalone goals:** compiler/code-generation support; no separate practical authoring change established. Keep raw declarations, including these generated public shapes, visible. | PIN-703 disposition. A future documented authored compiler-host scenario would require a bounded proposal; no general tooling compatibility claim. |
| Tensor twelve add/remove pairs | **Excluded as new algorithms:** identical callable shapes with added NullableContext(0), not twelve new tensor operations. Complete generic/parameter annotations remain in the manifest. | Tensor shape/layout/Clamp/Round correctness findings remain PIN-683; this metadata result does not establish behavioral equivalence. |
| Argb<T>/Rgba<T>, packed-endian conversion helpers | **Included, uncovered D5:** explicit channel order and component representation. | D5; no dependency on MAUI Color inheritance or a native painting API. |
| File.CreateHardLink/FileInfo.CreateAsHardLink | **Included, uncovered D1:** replace native creation code while preserving alias semantics. | D1. TAR hard-link archive policy is a different goal with existing owner PIN-684. |
| DirectoryNotFoundException DirectoryPath, constructors, formatting/serialization overrides | **Included, uncovered D9:** consume a structured failing path instead of parsing a localized message. Group supporting constructors/ToString/legacy serialization metadata; don't teach obsolete formatter use. | D9. |
| SafeFileHandle.Type/CreateAnonymousPipe/FileHandleType; NamedPipeClientStream replacement constructor and SYSLIB0063 | **Grouped:** handle type/ownership and remove the ineffective isConnected argument. | [PIN-741](https://linear.app/pinguapps/issue/PIN-741); [PIN-684](https://linear.app/pinguapps/issue/PIN-684) retains Unix async-state/permission migrations. |
| TAR format overloads, TarWriterOptions, TarExtractOptions, TarHardLinkMode | **Grouped:** format selection with PIN-732; preserving links versus copying contents with PIN-684's already assigned hard-link migration. | [PIN-732](https://linear.app/pinguapps/issue/PIN-732), PIN-684; include extraction on filesystems without link support when claimed. No duplicate hard-link-policy proposal here. |
| ZIP entry access/compression/encryption/version metadata, bulk creation/extraction options and async helpers | **Grouped:** entry-access/protection and bulk lifecycle with [PIN-731](https://linear.app/pinguapps/issue/PIN-731); corruption with PIN-732. VersionMadeBy/CompressionMethod are supporting inspection, not independent recipes. | PIN-731/732; passwords/modes/algorithms, overwrite and async failure timing need actual evidence. Existing migration source analysis remains PIN-684. |
| Streamless Deflate/ZLib/GZip encoder/decoder types and reset; Zstandard stream/buffer/dictionary/options APIs; Brotli options | **Grouped:** compression format/lifecycle choice and bounded buffers. `WindowLog2`/`MaxWindowLog2` supersede earlier preview spelling; separate Zstandard package removed in Preview 3. | [PIN-733](https://linear.app/pinguapps/issue/PIN-733); don't invent an old target route from a package name. |
| Adler32, Crc32/64 parameter sets, constructors and Hash/HashToUInt methods | **Included, uncovered D4:** interoperable noncryptographic checksums with deliberate polynomial/reflection parameters. | D4. ZIP's built-in corruption checks remain PIN-732, not manual CRC recipe delivery. |
| Remaining selected collection/facade, encoding, filesystem/watcher, pipelines, parallel-LINQ, globalization and packaging surfaces | **No additional API goal:** complete zero-delta surfaces are retained. Packaging's compressed-part behavior changes despite zero signature changes; see existing source owner below. | No standalone recipe for a zero delta or automatic implementation improvement. |

### Complete source-only disposition and drift

The full notes and compatibility files matter even when signatures are unchanged:

| Source finding | Disposition and owner |
| --- | --- |
| DateOnly/TimeOnly invalid-input TryParse; correctly rounded Decimal/BigInteger conversions; Math.Round/MathF.Round digit/mode validation; saturating small-integer conversion; Complex Annex G; tensor shape/layout and Japanese calendar boundary | **Already assigned PIN-683.** Its full-mitigation audit and proposals remain outstanding. PIN-737/736 provide established related delivery, not blanket proof of every migration. No repeated semantic analysis or invented duplicate ticket. |
| TAR checksum rejection/hard-link writing; compressed PackagePart nonseekable read-write streams; eager ZIP CreateAsync failures; Unix named-pipe permissions and IsAsync; apphost invocation name, TickCount and custom Type | **Already assigned PIN-684.** PIN-732 covers corruption/format content; PIN-741 covers pipe/mobile locking; PIN-742 covers reflection. The new TAR options are supporting mitigation APIs within that assigned audit. No existing finding is dropped because this task has an API table. |
| iOS/tvOS advisory file-locking default and runtimeconfig/environment opt-back | **Grouped PIN-741:** authored sharing policy/configuration; restoring locking restores suspension-termination risk. Mac Catalyst unchanged. Native proof is not supplied here. |
| FORCE_COLOR/NO_COLOR | **Grouped PIN-721:** scripts/environment configuration and redirected terminal output. No persistent environment changes in this audit. |
| Process lifetime/suspension discrepancy | **Drift:** cumulative article and old inventory say KillOnParentExit is Windows-only. Canonical Preview 5 says Linux/Android; Preview 7 adds macOS suspended startup. RC2 signatures do not carry enough OS annotations to resolve runtime support. PIN-721/722 must verify supported platforms against actual execution/source; neither headline is declared universal proof. |
| Process `silent`, IEnumerable arguments, native callbacks, startup rejection of embedded NUL; FreeBSD inspection fix | Parameter changes apply to earlier previews, not a .NET 10 migration. **Grouped PIN-721/722** for final signatures/failure handling; ordinary automatic fixes without a removed workaround produce no extra recipe. |
| ZIP central-directory/ZIP64 validation, data-descriptor preservation, Windows extraction/corrupt-output cleanup, encrypted empty entries; TAR metadata bound and GNU sparse 1.0; Zstandard multi-frame reads/max-window options and Deflate buffer-bound fix | **Grouped PIN-731/732/733 and assigned migration audit PIN-684.** Integrity/lifecycle behavior is tested when claimed. Sparse support or an internal fix alone is automatic; include a removed workaround only when the delivery sample demonstrates one. |
| Time-zone cache, BigInteger multiplication, determinant/Half/vectorization/Pipelines contention, queue growth, immutable-set allocation/correctness, index/range/ASCII/format escaping fixes, regex engine/source-generator correctness | **Excluded as standalone goals:** automatic benefit with no established new authored implementation. Explicit compatibility findings above are retained separately. Do not infer general semantic equivalence from this exclusion. |
| Generic Interlocked, reflection emit/function pointers, media mapping, syndication trimming, transport/security/Extensions changes within complete library notes | **Outside this bounded family:** retained owners PIN-705/706/704 as applicable; media guidance PIN-744. Reading complete notes does not silently transfer their scope. gRPC remains PIN-709's web boundary; this audit invents no gRPC recipe. |

## Obsolete-index ownership

Reviewed the complete global SYSLIB index and pinned .NET 11 obsolete article alongside actual obsolete declarations. The latter lists only **0064** (cryptography) and **0066** (startup validation); this is **incomplete for these actual RC2 assets**. The global index, the dedicated [SYSLIB0063 article](https://github.com/dotnet/docs/blob/dfe6ae54d3cf986ba03d95075e61ae006fefb9f9/docs/fundamentals/syslib-diagnostics/syslib0063.md) and NamedPipeClientStream metadata establish **0063** here. PIN-689 remains the overall index-to-family ownership task; it is not an unknown-sized semantic migration owner.

| Diagnostic | Family decision |
| --- | --- |
| **SYSLIB0063** | **New .NET 11 obsoletion, grouped PIN-741.** Replace the four-argument constructor's ineffective isConnected argument with the three-argument overload; preserve safe-handle ownership and real connection semantics. A warning suppression is not the recommended authored replacement. |
| 0001 UTF-7; 0008 CreatePdbGenerator; 0036 CompileToAssembly; 0052 old Regex extensibility | Historical obsoletions already present in .NET 10. **Excluded as new .NET 11 goals.** No ninth historical foundation and no endorsement of the obsolete mechanism. |
| 0051 formatter serialization support | Historical mechanism, including new overrides supporting the exception shape. **Grouped as a restriction of D9**, not a new formatter migration. |
| 0055 signed AdvSimd narrowing; 0061 Queryable MinBy/MaxBy wrong-comparer overloads | Historical .NET 9/.NET 10 obsoletions, retained unchanged; **excluded as new standalone .NET 11 goals**, relevant context for PIN-739/740/734 where necessary. |
| 0064 / 0066 and other global-index families | Outside this family: cryptography PIN-686/PIN-705; Extensions PIN-685/PIN-706. No delegation of newly discovered in-family gaps back to PIN-689. |

Obsolete declarations without custom IDs also remain in the artifact manifest. No unknown newly obsolete member is hidden by the index summary. Regex SYSLIB1040–1045 are generator diagnostics, not obsolete IDs; their six package-matched descriptors are unchanged.

## Proposed fully verified recipe tickets for review

These are **proposals, not created or completed tickets**. Each is a separate narrow PR boundary in existing milestone **02 - Language and library guidance**, ID `c5232470-50ac-4f56-aea8-bd8ff7d13b41`, with the same team/project IDs above. Before approved creation, repeat a scoped overlap search including archived issues, allocate unused `02-DDD` numbers, relate the target and comprehensive gate, and save/read back the native estimate. Preserve existing titles and stable recipe identities. No native milestone creation or invented issue identifiers are authorized here.

Native estimate examples confirm **XS=1, S=2, M=3, L=5**. Values below include original samples, prose/metadata, compilation, real behavior evidence and installed retrieval; they are not source-only coding estimates.

### Shared acceptance and exact prerequisite baseline

Each proposal must:

1. Deliver original same-goal .NET 10/11 C# samples, complete offline prose/metadata, official references and canonical snippets; fictional domains belong in examples, while titles/goals explain reusable capability and when to use it. Explain fair older alternatives and preserve stable goal/edition identity.
2. Use the existing delivered package/content/evidence harness (PIN-668/672), with actual SDK **10.0.402** / target **net10.0**, and **11.0.100-rc.2.26504.105** / **net11.0**, reference packs **10.0.13** / **11.0.0-rc.2.26504.105**. These are reproducible verification pins, not universal exact-SDK requirements. D4 additionally needs the matching System.IO.Hashing packages and reviewed lockfiles. No cloud service, account, native UI workload or secret is required.
3. Compile and execute both real consumers; record resolved dependencies, exact commands/outputs and fresh input hashes through the existing evidence workflow. State consumer runtime/language/package/platform conditions separately from measured pins. RC evidence stays RC evidence. Verify any optional net10-plus-RC2-package route with its own real consumer before advertising it; matching signatures alone do not qualify.
4. Build/pack the ordinary tool once; install that exact artifact using the existing isolated local feed/cache from an unrelated working directory. Call actual stdio `list_recipes` and `get_recipe` with explicit targets. Check ID/title/**goal**, full Markdown, applicability, evidence, canonical snippets, edition selection and structured/text equality offline. Reuse the harness; no client/OS/native matrix.
5. Update this register and the inventory, preserve unknowns and keep PIN-959 publication and PIN-962 GA reconciliation gates intact. Missing API, compilation, platform prerequisites or necessary runtime evidence blocks that proposal's delivery/publication. No package publication is authorized by a proposal.

All proposals depend on that existing harness, not on another unimplemented research/recipe ticket unless explicitly stated below. Their execution checks are **future acceptance**, not tests performed in this audit.

### D1 — Create file aliases with hard links — M (3)

Proposed goal `io.create-hard-links`. A fictional survey export needs two names for the same file without copying bytes. Compare a legitimate original .NET 10 native-wrapper approach with File.CreateHardLink/FileInfo.CreateAsHardLink. Keep this separate from TAR archive representation (PIN-684).

**Additional prerequisites:** temporary directories on a local filesystem supporting hard links; execute on each OS/filesystem declared in applicability. A Windows NTFS route is feasible without an Apple workload; a Unix claim requires a real Unix run. Do not assume links work across volumes, directories or arbitrary network shares.

**Real acceptance:** changing one alias changes the other; deleting one name preserves the remaining data; destination-exists, absent-target and unsupported/cross-volume behavior are documented only after actual evidence. Cleanup removes only the created temporary tree. Compare native and managed consumers' observable results and verify FileInfo receiver/target direction. M reflects a small filesystem lifecycle and paired implementations, not a broad backup system.

### D2 — Choose integer division and remainder rules — M (3)

Proposed goal `numerics.choose-integer-division`. A fictional coordinate tiler needs negative coordinates assigned to predictable tiles. Compare .NET 10 quotient/remainder adjustment with the actual IBinaryInteger APIs and DivisionRounding modes, including generic constraints and how callers reach static/default interface operations.

**Prerequisites:** shared baseline only. **Real acceptance:** signed positive/negative dividend/divisor examples for Truncate/Floor/Ceiling/AwayFromZero/Euclidean; verify `a = q*b + r` and each remainder rule; test zero divisor, signed minimum/-1 and unsupported enum values without inventing behavior. Include unsigned/generic use only where supported and executed. No floating Math.Round migration. M covers the five-mode arithmetic contract and generic consumer.

### D3 — Compute integer decimal magnitude without rounding — S (2)

Proposed goal `numerics.measure-integer-magnitude`. A fictional batch encoder computes decimal width from integer magnitude. Compare an original .NET 10 integer loop with IBinaryInteger.Log10/concrete methods; retain explicit sign and zero rules instead of passing integers through floating point.

**Prerequisites:** shared baseline only. **Real acceptance:** values just below/at/above powers of ten, Int128/UInt128 boundaries, zero and negative inputs; test generic-interface reachability, exact results/exceptions and no value loss. A digit count is an application rule around the returned logarithm, not a claim that Log10 itself returns character width. S is one small generic arithmetic operation plus boundary evidence.

### D4 — Compute the checksum a binary format requires — M (3)

Proposed goal `io.compute-format-checksums`. A fictional telemetry frame declares a particular checksum variant. Compare original .NET 10 checksum code or an explicitly pinned legitimate dependency with System.IO.Hashing Adler32 and configurable Crc32/Crc64 parameter sets.

**Additional prerequisites:** packages **System.IO.Hashing 10.0.13 / 11.0.0-rc.2.26504.105**, actual lib/net10.0 / lib/net11.0 and lockfiles. An older-target RC2 package route needs separate execution before publication.

**Real acceptance:** independently established reference vectors for Adler32 and at least CRC32C/CRC64-NVME, plus one explicit custom polynomial/reflection/initial/final setting; one-shot and segmented Append agree, Reset isolates messages, and byte order/HashToUInt results match the declared format. Verify short destination handling. State explicitly that CRC/Adler detects accidental corruption and is not authentication; ZIP validation remains PIN-732. M covers related noncryptographic format interoperability, not a cryptographic library.

### D5 — Preserve colour channel order across binary boundaries — M (3)

Proposed goal `numerics.preserve-color-channels`. A fictional signal legend exchanges RGBA/ARGB channel values. Compare original .NET 10 value structures/shifts with System.Numerics.Colors types and endian conversion helpers.

**Prerequisites:** shared baseline only; no MAUI/WinForms/UI device. **Real acceptance:** distinct channel values prove ARGB/RGBA conversion and big-/little-endian packed round trips; byte and one other supported component type preserve values; span construction/CopyTo lengths and equality/hash behavior match documented contracts. Don't imply colour-space conversion, normalization, blending or validation that these structural types do not supply. M covers representation and endianness, independent of PIN-903's native Color inheritance migration.

### D6 — Find extrema directly in an existing span — S (2)

Proposed goal `collections.find-span-extrema`. A fictional instrument batch is already in a span. Compare original .NET 10 iteration with MemoryExtensions.Min/Max; remove array/enumerable adapters rather than asserting an unmeasured speedup.

**Prerequisites:** shared baseline only. **Real acceptance:** value-type and reference-type inputs, explicit comparer, null values, and the distinct empty-span contracts; generic overload selection is compiled. Include floating NaN only if making a floating-specific claim. This is a span aggregation goal, separate from PIN-740's bit sets/key equality. S includes the overload/null/empty contract and installed retrieval.

### D7 — Cancel asynchronous text output explicitly — S (2)

Proposed goal `io.cancel-text-output`. A fictional report generator must stop writing when its operation is cancelled. Compare a valid .NET 10 ReadOnlyMemory<char>/token route with the actual new TextWriter string/token and token-only newline convenience APIs. Do not claim that .NET 10 lacks cancellation.

**Prerequisites:** shared baseline plus an original controlled writer/stream fixture; no network service. **Real acceptance:** pre-cancelled and in-flight cancellation at the real writer boundary, complete successful text/newline output, and the bounded partial-write behavior of the chosen implementation. Verify concrete StreamWriter execution as well as any controlled fixture; a fake override alone cannot prove a real writer's cancellation. Do not promise rollback or universal cancellation of arbitrary TextWriter subclasses. S is a small output contract, not a streaming framework.

### D8 — Convert international domain labels into caller buffers — S (2)

Proposed goal `text.convert-domain-labels`. A fictional routing validator exchanges Unicode and ASCII domain labels. Compare legitimate .NET 10 IdnMapping string methods with TryGetAscii/TryGetUnicode over spans.

**Prerequisites:** shared baseline and the real globalization mode/data of each declared platform; DNS/network lookup is unnecessary. **Real acceptance:** meaningful Unicode/ASCII round trips, sufficient/insufficient destination buffers, malformed and disallowed label inputs, and UseStd3AsciiRules/AllowUnassigned choices where claimed. Record IDNA/globalization differences rather than treating culture invariance as proof of identical platform policy. S covers buffer conversion, not domain authorization or spoofing prevention.

### D9 — Report the failing directory without parsing messages — S (2)

Proposed goal `io.report-directory-failures`. A fictional import utility reports a structured failing path. Compare an original .NET 10 operation retaining its attempted path with DirectoryNotFoundException.DirectoryPath and the new explicit-path constructors; do not require parsing localized exception text.

**Prerequisites:** shared baseline and an isolated temporary filesystem tree. **Real acceptance:** actual missing-directory operations establish which failures populate the property, explicit constructor and inner-exception paths retain intended values, and missing/unknown path cases have an honest fallback. Record path normalization/disclosure limits and avoid formatter-based serialization. A manually constructed exception alone does not prove the filesystem populates the property. S covers diagnostic output, not a filesystem error taxonomy.

### D10 — Choose reduced-precision binary values deliberately — M (3)

Proposed goal `numerics.choose-bfloat16`. A fictional sensor batch tolerates bounded precision loss while retaining a wide exponent range. Compare an original .NET 10 representation with System.Numerics.BFloat16 arithmetic/conversion and generic numeric use. Distinguish range from significand precision and Half from BFloat16; don't duplicate PIN-737's bit-encoding recipe.

**Prerequisites:** shared baseline only; no GPU/ML service. **Real acceptance:** exactly representable and deliberately rounded values, large/small magnitudes, signed zero/infinity/NaN conversions, and the chosen calculation's error budget. Verify actual generic constraints, conversion direction and serialization only if claimed (JSON belongs to PIN-729). M covers a numeric representation choice and precision evidence, not an ML tutorial or universal performance claim.

## Verification and limitations

PowerShell 7 and the two actual SDK/reference-pack directories above are required. Override roots for other installations. This authoring audit downloads matching NuGet packages and immutable sources; installed Playbook remains offline. No restore, build, server setup or persistent environment change is required for metadata inspection.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3d6390c6'
$pwsh = (Get-Command pwsh).Source
& $pwsh -NoProfile -File ./eng/Inspect-DataIoApis.ps1 `
  -Net10Root 'C:/Program Files/dotnet' `
  -Net11Root 'C:/Users/pingu/.dotnet-playbook-11' `
  -OutputPath ./artifacts/data-io.recheck.json `
  -CompareWith ./docs/planning/audits/data-io.assets.json
if ($LASTEXITCODE -ne 0) { throw 'Data/I/O audit failed' }
$audit = Get-Content ./artifacts/data-io.recheck.json -Raw | ConvertFrom-Json
$audit.publicApiDiff | ForEach-Object {
  '{0}: +{1} -{2}' -f $_.asset, $_.diff.added.Count, $_.diff.removed.Count
}
$audit.dispositions | Where-Object group -EQ 'checksums'
($audit.publicApiDiff | Where-Object asset -EQ 'framework/System.IO.Pipes.dll').diff.added
```

Expected: exact artifact/declaration/manifest comparison passes, all **151** immutable source hashes match, every delta has an owner, checksum declarations have D4 ownership, and pipe additions include the three-argument constructor plus SYSLIB0063 on the old constructor. A missing asset, changed source/manifest/hash, comparison mismatch or unowned delta is failure, not a no-change finding.

Actual checks: generated from both real packs and six package downloads; replayed the commands above from `C:\Users\pingu` before selecting the literal worktree, with exact baseline equality and all 151 source hashes passing. All **6,764 delta records** (including annotations and the separate package-target comparisons) reconcile to **60 asset/side/group summaries**. An intentionally omitted checksum ownership rule failed on the real Adler32 declaration. Independently checked parsed JSON equality, each asset/side ownership total, identical package-matched regex descriptor sources, PowerShell syntax and relative audit links. Process, saved user and saved machine PATH remained unchanged. Current CI/review results belong to the PR handoff.

No new recipe, old/new recipe consumer, filesystem/process/platform behavior, ISA execution or optional package route was implemented or executed here. All real runtime acceptance above remains owed by delivery. No MCP tool or served recipe changed, so this audit has no new server response to demonstrate. The source/API evidence satisfies the audit; it cannot establish comprehensive content, client usability, GA readiness or publication permission.

## Delivered process, buffer and text goals (PIN-721–PIN-725)

The [original editions and measured verification](../../verification/PIN-721-725.md) cover the existing process/output/lifetime/colour, native handles/preparation/termination, memory adapters/builder transfer, Unicode/comparison/validation/newline and Base64 goals. Stable inventory IDs and edition 11.0 are preserved. CBOR chunk processing is reconciled with the serialization register. Windows and Linux process execution resolve the parent-bound and Unix colour source discrepancies; Windows redirected Console emits no ANSI in either tested policy. macOS native behaviour remains explicitly unverified with Pingu's authorization; the six process/colour recipes retain pending Human proof. This does not establish product release readiness, Android support, native callback execution or GA proof. Audit declaration counts/hashes remain historical evidence; no source-only API delta is relabelled execution proof.
