# Languages, shared libraries, runtime and SDK inventory

Documentation-backed planning inventory, reviewed **16 October 2026** against the .NET 11 RC2 evidence baseline. This is proposed recipe allocation inside accepted scope, not a catalogue implementation, an exhaustive public-API diff, or sample verification.

## Boundary and conventions

The audit covers the cumulative .NET11 overview and its **language/library/runtime/SDK** subjects, canonical **RC1 and RC2** notes for C#, F#, libraries, runtime, SDK, MSBuild, NuGet and container images, VB's current language history, and the relevant **top-level .NET11 compatibility-index entries**. Every top-level change family in the consulted cumulative library/runtime/SDK documents has a disposition below; subfeatures are grouped where they serve one goal. Linked compatibility entries were triaged by title; their full mitigation texts are **not** all reviewed. Earlier F# preview notes, full compiler changes, standalone Extensions release histories and complete API diffs remain reconciliation work.

Other inventory owners cover ASP.NET/Blazor, EF and desktop/mobile specifics. Shared BCL/validation/SDK goals here should be linked rather than copied into framework recipes. Bare group targets such as `uri.handle-schemes` are proposed cross-cutting identities requiring reconciliation, not existing implemented recipes.

- **included**: eligible goal assigned a proposed stable ID; exact signatures/SDK/package pins and original compiling samples still required.
- **grouped → ID**: subcase of another goal; do not count as a second independent recipe.
- **excluded**: no distinct authoring change established, or an explicitly empty RC component.
- **unresolved**: eligibility/contract/migration facts remain missing; no silent omission.
- Unless a row says otherwise, its subject is introduced or expanded in **.NET11**, uses **C# canonical examples**, and is documented in RC evidence without an explicit experimental designation. This is **not** a promise of GA stability or older-target compatibility. F# features use F#; VB-specific changes require VB examples.
- Introduction release, applicable target, compiler, package, workload, platform and demonstrated languages are separate metadata. Older-target package/SDK routes must be documented and compiled; do not infer them from a broad package TFM.
- Experimental features remain included with labels and opt-ins under Round5. Shared discovery/language filtering follow Round6. Windows/macOS CI stays tiny tool smokes. Guidance about native testing does not authorize a native test matrix.
- This inventory makes no change to the accepted weekly publication flow: any new main commit can publish through the reference draft/stable-channel gates; there is no docs/path/package-difference guard.

## Sources actually consulted

[O]: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/overview
[C]: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15
[VB]: https://learn.microsoft.com/en-us/dotnet/visual-basic/whats-new/
[L]: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries
[R]: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/runtime
[S]: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/sdk
[B]: https://learn.microsoft.com/en-us/dotnet/core/compatibility/11
[CR1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/csharp.md
[CR2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/csharp.md
[FR1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/fsharp.md
[FR2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/fsharp.md
[LR1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/libraries.md
[LR2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/libraries.md
[RR1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/runtime.md
[RR2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/runtime.md
[SR1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/sdk.md
[SR2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/sdk.md
[MR1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/msbuild.md
[MR2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/msbuild.md
[NR1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/nuget.md
[NR2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/nuget.md
[IR1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/containers.md
[IR2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/containers.md
[F10]: ../coverage.md#approved-initial-eight-recipes

- [Cumulative release overview][O]; [C#15][C]; [VB history][VB].
- Cumulative [libraries][L], [runtime][R], [SDK][S]; [compatibility index][B].
- Canonical RC1/RC2: [C#][CR1]/[RC2][CR2], [F#][FR1]/[RC2][FR2], [libraries][LR1]/[RC2][LR2], [runtime][RR1]/[RC2][RR2], [SDK][SR1]/[RC2][SR2], [MSBuild][MR1]/[RC2][MR2], [NuGet][NR1]/[RC2][NR2], [container images][IR1]/[RC2][IR2].

## Language authoring

| Proposed stable ID | Goal and authoring impact | Applicability / stability / verification constraint | Official evidence | Disposition |
| --- | --- | --- | --- | --- |
| `csharp.closed-results` | Represent one of several unrelated outcomes; replace object-based contracts with unions. | C# 15/net11; RC1 stabilized; default/null cases need explicit treatment. | [C], [CR1] | included; edition 11.0 delivered with actual .NET 10/11 compilation, execution and consumer exhaustiveness checks; [PIN-711 evidence](../../verification/PIN-711.md). GA reconciliation remains pending. |
| `csharp.closed-hierarchies` | Close an inheritance family so consumers can match every permitted case. | C# 15; assembly/type boundary and downstream evolution matter. | [C], [CR1] | included; edition 11.0 delivered with sealed leaves, external-assembly rejection and exhaustive matching; [PIN-711 evidence](../../verification/PIN-711.md). Closure is not transitive. |
| `csharp.configure-collection-construction` | Construct collections with required capacity/comparer arguments instead of follow-up mutation. | C# 15 collection-expression arguments; exact supported builder forms need compilation. | [C] | included |
| `csharp.index-existing-types` | Offer indexer access on a type without editing its declaration. | C# 15 extension indexers; receiver/member lookup constraints. | [C], [CR1] | included |
| `csharp.control-nested-loops` | Exit or continue an outer loop without auxiliary flags or goto plumbing. | C# 15 labeled break/continue; preserve cleanup/control flow. | [C], [CR1] | included |
| `csharp.static-interface-helpers` | Place non-virtual static helpers on interfaces without making them implementation requirements. | RC1 explicitly includes this stabilized feature; verify declaration/access rules. | [CR1] | included |
| `csharp.audit-memory-boundaries` | Make unsafe caller obligations and safe declarations explicit under the revised model. | Experimental language mode plus updated-memory-safety-rules; not ordinary C# 15 defaults. | [C], [CR1] | included |
| `fsharp.compose-records` | Reuse fields from existing named/anonymous records instead of repeating declarations and copies. | F# 11 record spreads; stable in RC1. | [FR1] | included |
| `fsharp.construct-records` | Create record values through all-fields constructors when positional/named construction fits. | Preview in RC1; recheck final compiler gate. | [FR1] | included |
| `fsharp.pass-methods-as-delegates` | Pass compatible methods/functions to .NET delegate parameters without explicit adapter lambdas. | F# 11; distinguish changed authoring from automatic closure removal. | [FR1] | included |
| `fsharp.format-without-reflection` | Produce record/union diagnostics in reflection-free applications. | F# --reflectionfree; contained values must support safe formatting. | [FR1] | included |
| `fsharp.preserve-interpolation-culture` | Keep intentional display culture when interpolation now uses invariant formatting. | F# 11; choose explicit culture and avoid printf holes in AOT paths. | [FR1] | included |
| `fsharp.reuse-api-documentation` | Reuse inherited/external documentation instead of maintaining copied summaries. | F# inheritdoc/include; compiler output and editor resolution differ. | [FR1] | included |
| `fsharp.start-async-on-caller` | Run work immediately until first suspension when call-stack semantics matter. | Async.RunSynchronouslyImmediate; does not honor SynchronizationContext. | [FR1] | included |
| `fsharp.adapt-generic-operations` | Allow generic algorithms to use public extensions on existing types. | Preview SRTP extension/operator resolution; open defining module for external extensions. | [FR2] | included |
| `fsharp.rethrow-in-computations` | Preserve the original stack when rethrowing inside computation-expression handlers. | Preview reraise in async/task/custom builders. | [FR2] | included |
| `fsharp.require-named-arguments` | Require named calls for selected APIs to reduce argument-order mistakes. | Preview; RC2 BCL lacks RequireNamedArgumentsAttribute: supplied definition/dependency required. | [FR2] | included |
| `fsharp.bound-async-work` | Limit work before tasks start while retaining input-order results. | FSharp.Core Async/Task parallelLimit and related helpers; exact assets/overloads must be pinned. | [FR2] | included |
| `fsharp.author-runtime-async-builder` | Build a custom computation-expression API over runtime-async intrinsics. | Preview; runtimeTask is a compiler-test prototype, not a shipped builder; net11 runtime required. | [FR2] | unresolved |
| `fsharp.configure-compiler-service-memory` | Control imported-assembly sharing in compiler-hosting tools. | FSharpChecker.Create shareImportedAssemblies; FCS version/standalone-tool boundary to confirm. | [FR2] | unresolved |
| `fsharp.migrate-enum-flags` | Replace unsupported non-integer enum flag operations with supported representation. | F# 11 FS0001; previously compiled but failed at runtime. | [FR2] | included |
| `fsharp.collection-compiler-speedups` | Collection callback inlining, compiler memory reductions, empty-array reuse and ordinary compiler fixes. | No distinct authoring change established; avoid recipes for automatic speedups. | [FR2] | excluded |
| `fsharp.aot-and-fsi-lifetimes` | Use supported Array2D operations and async-disposal bindings under AOT/FSI. | Separate remaining Based restrictions and FSI fix from broad automatic trimming benefits; signatures outstanding. | [FR2] | unresolved |
| `vb.release-review` | Identify VB-specific authoring changes rather than inventing a new language version. | [PIN-682 audit](../../verification/PIN-682.md): bounded no-new-syntax finding at VB17.13; VB compiler API delivery proposed; native WinForms obligations retained in PIN-893/PIN-896. Source/API release-routing gaps remain explicit. | [VB], [O], [pinned sources](../../verification/PIN-682.sources.json) | unresolved — delivery/approval and release evidence remain; syntax review complete within stated boundary |
| `languages.compiler-migrations` | Resolve source-breaking compiler changes independently of new feature recipes. | [PIN-682 audit](../../verification/PIN-682.md) accounts for all 12 pinned C# compatibility entries and Core/C#/VB compiler API declaration differences. Proposals A–E await approval; grouped goals and C#16/main-versus-RC2 conflicts remain visible. | [CR1], [FR2], [B], [pinned sources](../../verification/PIN-682.sources.json) | unresolved — source audit recorded; qualifying delivery and release reconciliation remain |

## Shared libraries: processes, buffers and text

| Proposed stable ID | Goal and authoring impact | Applicability / stability / verification constraint | Official evidence | Disposition |
| --- | --- | --- | --- | --- |
| `process.capture-output` | Launch a child and collect/stream stdout, stderr and exit status without manual event plumbing. | Run/RunAndCaptureText/ReadAllLines APIs; cancellation, buffering and exit semantics need exact artifacts. | [L] | included |
| `process.control-lifetime` | Choose detached, forgotten or parent-bound child lifetime deliberately. | StartAndForget/StartDetached; KillOnParentExit Windows-only. | [L] | included |
| `process.control-native-handles` | Launch/wait/kill through SafeProcessHandle and choose inherited/redirection handles. | Advanced OS-specific handles; own/dispose handles correctly. | [L] | included |
| `process.prepare-before-start` | Start suspended, finish setup, then resume; locate/open existing processes without throwing. | StartSuspended Windows/macOS; TryGetProcessById/TryOpen; permissions/races apply. | [L] | included |
| `process.inspect-termination` | Send a signal and distinguish signal termination from normal exit. | Process.Signal and ProcessExitStatus; POSIX platform applicability. | [LR1] | included |
| `console.color-piped-output` | Choose whether redirected CLI output retains terminal colour. | FORCE_COLOR/NO_COLOR; configuration recipe, not a default rendering speedup. | [L] | included |
| `io.adapt-existing-buffers` | Pass memory, segmented bytes or encoded text to stream consumers without manual copying. | ReadOnlyMemoryStream/WritableMemoryStream/ReadOnlySequenceStream/StringStream; mutability/lifetime differ. | [L] | included |
| `text.handle-unicode-scalars` | Search/trim/replace/case-map Unicode scalars without splitting surrogate pairs. | Rune string/TextInfo overloads; keep StringComparison and indices explicit. | [L] | included |
| `text.compare-characters` | Select ordinal/culture-aware character comparison directly. | Char.Equals with StringComparison; not equivalent to arbitrary string collation. | [L] | included |
| `text.transfer-builder-content` | Transfer accumulated text into a new builder rather than copying it. | StringBuilder.MoveChunks empties the source; ownership change matters. | [L] | included |
| `text.encode-base64` | Encode/decode directly between character and UTF-8 buffers. | New Base64 convenience/span APIs; buffer/status semantics need exact overloads. | [L] | included |
| `text.locate-invalid-unicode` | Report the first invalid UTF subsequence rather than generic decoding failure. | Utf16.IsValid and Utf8/Utf16.IndexOfInvalidSubsequence; byte/code-unit offsets differ. | [L] | included |
| `regex.handle-unicode-newlines` | Match mixed newline conventions deliberately. | RegexOptions.AnyNewLine; changed anchor/dot semantics should be demonstrated. | [L] | included |
| `regex.automatic-fixes` | Regex engine correctness, non-backtracking performance and source-fixer improvements. | No changed application implementation established. | [L] | excluded |
| `cbor.read-chunks` | Decode incrementally received CBOR instead of buffering the whole payload. | NeedsMoreData/SlideData; incremental mode requires Lax conformance. | [LR2] | included |

## Shared libraries: JSON contracts

| Proposed stable ID | Goal and authoring impact | Applicability / stability / verification constraint | Official evidence | Disposition |
| --- | --- | --- | --- | --- |
| `json.serialize-closed-results` | Serialize C# unions and resolve ambiguous object cases deliberately. | JsonUnion structural/custom classifiers; root-property matching limits, null/default semantics, AOT context. | [L], [LR1] | included; edition 11.0 delivered with generated-metadata case/payload round trips and unknown/ambiguous/null checks; [PIN-711 evidence](../../verification/PIN-711.md). Native AOT publication and custom-factory implementation are not claimed. |
| `json.serialize-fsharp-unions` | Round-trip F# discriminated unions without a custom converter. | Reflection-only; no source generation, trimming or Native AOT; native F# sample justified. | [L] | included |
| `json.stream-json-lines` | Write async records as JSONL instead of assembling an array or line-writing loop. | SerializeAsyncEnumerable topLevelValues; LF/compact output regardless of indent/newline settings. | [L] | included |
| `json.infer-closed-polymorphism` | Infer allowed derived contracts instead of listing every closed subtype. | Global/context/per-type inference; explicit registrations replace inference; source-gen metadata must agree. | [L], [LR1] | included; edition 11.0 delivered with generated round trips, explicit-registration precedence, runtime-only metadata failure and SYSLIB1243 rejection; [PIN-711 evidence](../../verification/PIN-711.md). Discriminator migration is explicit. |
| `json.configure-generic-contracts` | Register open generic derived types/converters instead of manual factories per closed type. | Arity/constraints must resolve unambiguously; SYSLIB1229 warning is not a working fallback. | [L] | included |
| `json.generate-private-contracts` | Generate contracts for intentionally included inaccessible members/constructors. | JsonInclude/JsonConstructor access shapes, unsafe-generator compatibility; do not expose arbitrary internals. | [L] | included |
| `json.preserve-omitted-initializers` | Retain init-property initializers when JSON omits the property. | Source-generation contract behavior; distinguish missing from explicit values. | [L] | grouped → json.generate-private-contracts |
| `json.bind-reference-constructors` | Deserialize contracts with in/ref/out/ref-readonly constructor parameters. | Out parameters do not consume JSON constructor arguments; actual generated/reflection behavior to verify. | [L] | included |
| `json.retrieve-typed-metadata` | Resolve JsonTypeInfo<T> without casts or exception-based misses. | GetTypeInfo<T>/TryGetTypeInfo<T>; resolver/configuration exceptions can propagate. | [L] | included |
| `json.configure-member-conventions` | Apply naming and ignore policies at the type/member boundary. | PascalCase/JsonNamingPolicy; precedence and invalid type-level Always matter. | [L] | included |
| `json.use-numeric-collection-contracts` | Use built-in numeric converters and read-only set/extension-data contracts. | BFloat16/IEEE decimal, IReadOnlySet, IReadOnlyDictionary extension data and JsonObject overflow; split by goal if needed. | [L], [LR1] | included |
| `json.describe-binary-schema` | Export binary buffer contracts with correct schema metadata. | byte[]/Memory/ReadOnlyMemory JSON schema support; distinguish author API from automatic generated improvements. | [LR1] | grouped → json.use-numeric-collection-contracts |
| `json.reuse-writer` | Reuse a flushed writer across output targets/options without reconstructing it. | Utf8JsonWriter.Reset; flush/reset and stream ownership must be explicit. | [L] | included |

## Shared libraries: archives, collections, numerics and I/O

| Proposed stable ID | Goal and authoring impact | Applicability / stability / verification constraint | Official evidence | Disposition |
| --- | --- | --- | --- | --- |
| `archive.control-entry-access` | Choose ZIP entry access and inspect its compression method. | Open(FileAccess)/async and CompressionMethod; archive mode constraints. | [L] | included |
| `archive.protect-entries` | Read/write password-protected ZIP entries through built-in APIs. | Password/encryption overloads; exact encryption support/platform/security guidance remains to verify. | [L] | included |
| `archive.handle-corruption` | Handle newly rejected CRC/header-corrupt archives explicitly. | CRC32 and TAR header validation; do not silently disable integrity checking. | [L], [B] | included |
| `compression.handle-empty-payloads` | Remove assumptions that compressing no bytes produces no output. | Deflate/GZip now emit valid framing; migration rather than new encoding API. | [L], [B] | included |
| `compression.process-buffers` | Encode/decode compressed buffers without stream adapters; reset for independent messages. | Deflate/ZLib/GZip span APIs and Reset state; disposal/status and reusable buffer sizing. | [L], [LR1] | included |
| `compression.use-zstandard` | Adopt built-in Zstandard instead of a separate wrapper/package. | System.IO.Compression namespace; package/target support must be verified individually. | [L] | included |
| `archive.select-tar-format` | Choose a tar format that the receiving tool accepts. | CreateFromDirectory format overloads; GNU sparse1.0 reading is an automatic benefit unless removing a workaround. | [L] | included |
| `linq.reconcile-all-rows` | Retain unmatched data from both sides without composing several joins. | FullJoin plus tuple-returning joins across Enumerable/Queryable/AsyncEnumerable; provider translation separate. | [L] | included |
| `numerics.choose-ieee-decimal` | Represent IEEE decimal NaN/infinity/precision rather than assuming System.Decimal semantics. | Decimal32/64/128; generic math/serialization and conversion constraints. | [L] | included |
| `numerics.preserve-conversion-intent` | Avoid accidental binary-to-decimal value changes during migration. | Correct rounding changes results; use decimal literals for decimal intent; no old-behavior switch. | [L], [LR1], [B] | included |
| `numerics.parse-delimited-values` | Parse numeric prefixes while retaining consumed-length information. | INumberBase.TryParsePartial; generic constraints and delimiter/error handling. | [L] | included |
| `numerics.select-complex-precision` | Use complex arithmetic with a chosen floating-point representation. | Complex<T>; distinguish new generic API from corrected special-value results. | [L], [B] | included |
| `numerics.roundtrip-float-bits` | Persist exact floating-point values in hex or BFloat16 bit representations. | Hex formatting/parsing and BitConverter BFloat16 APIs; endianness and NaN details. | [L] | included |
| `numerics.generate-generic-random` | Generate random values directly for supported generic numeric types. | Random.NextInteger/NextBinaryFloat; constraints; not cryptographic randomness. | [L] | included |
| `simd.arrange-lanes` | Construct/interleave/reorder vector lanes instead of manual platform shuffles. | Create sequences/Zip/Unzip/Concat/Reverse; supported vector types and ISA fallback. | [L], [R] | included |
| `simd.select-supported-instructions` | Build SVE loop predicates and dispatch only to supported instruction families. | Expanded CreateWhile variants and SVE2 feature-specific IsSupported checks; Arm hardware prerequisites. Overview also names AVX-VNNI-512: exact API diff still required. | [O], [R] | included |
| `io.use-pipe-handles` | Create/inspect pipe handles and perform low-level I/O without seeking assumptions. | SafeFileHandle.Type/CreateAnonymousPipe and RandomAccess pipes; OS-specific async behavior. | [L] | included |
| `io.migrate-mobile-locking` | Choose explicit file-sharing behavior when iOS/tvOS defaults stop enforcing locks. | Restore switch has suspension-termination risk; Mac Catalyst unchanged. | [LR2], [RR2] | included |
| `collections.count-and-build-bits` | Build bit sets from spans and count enabled positions without manual iteration. | BitArray span constructors/PopCount; existing span lifetime is not retained as mutable backing memory. | [L], [LR1] | included |
| `collections.compare-by-key` | Create equality semantics from a key selector instead of a bespoke comparer class. | EqualityComparer<T>.Create; optional key comparer, hash/equality consistency. | [L] | included |
| `numerics.automatic-speedups` | Faster determinant/Half arithmetic, more accurate IEEE-decimal functions and queue reliability. | No new implementation needed; numeric correctness migrations remain separately reviewed. | [L], [LR2], [R] | excluded |

## Shared libraries: extensions, security and networking

| Proposed stable ID | Goal and authoring impact | Applicability / stability / verification constraint | Official evidence | Disposition |
| --- | --- | --- | --- | --- |
| `reflection.inspect-loaded-metadata` | Find a metadata-only assembly's context and inspect nullable/generic types directly. | MetadataLoadContext/Type/ConstructorInfo additions; custom Type implementations need migration audit. | [L], [B] | included |
| `hosting.report-assembly-location` | Report meaningful assembly locations from a custom loader. | AssemblyLoadContext.SetAssemblyLocationOverride is set-once; host/resource consumer implications. | [L] | included |
| `tooling.annotate-code-strings` | Tell tooling which language a string parameter contains. | StringSyntax CSharp/FSharp/VisualBasic constants; no parser/runtime validation implied. | [L] | included |
| `uri.identify-data-uris` | Use framework scheme names instead of repeating URI literals. | UriSchemeData; small subcase in URI/input handling, not an invented large recipe. | [L] | grouped → uri.handle-schemes |
| `configuration.exclude-properties` | Prevent selected model properties being bound without custom binding code. | ConfigurationIgnore attribute; distinguish from automatic empty-array/watcher fixes. | [L] | included |
| `cache.observe-memory-cache` | Enable and collect built-in cache statistics without a custom metrics adapter. | TrackStatistics, IMeterFactory; estimated size uses application units; RC tag rename is freshness context. | [L], [LR2] | included |
| `options.validate-with-di` | Register an options validator by type instead of a factory delegate. | OptionsBuilder.Validate generic overload; validator registered in DI. | [L] | included |
| `options.validate-asynchronously` | Check I/O-dependent settings and fail startup without sync blocking. | IAsyncStartupValidator replaces obsolete IStartupValidator; synchronous access/explicit generated implementations need migration. | [LR1], [B] | included |
| `configuration.await-change-callback` | Await reload work without async void and register the next notification afterwards. | Task-returning ChangeToken.OnChange overloads can rebind old calls; concurrency semantics matter. | [L], [B] | included |
| `cache.select-options-from-value` | Choose cache expiry/size based on the newly loaded value. | HybridCache mutable factory context overloads; package-specific asset support. | [L] | included |
| `validation.validate-asynchronously` | Run model/property/value I/O validation through supported async contracts. | AsyncValidationAttribute/IAsyncValidatableObject/Validator methods; framework adapters share the same goal. | [L] | included |
| `validation.format-localized-rules` | Let attributes fill rule-specific arguments in externally supplied message templates. | ValidationAttribute.FormatMessage; group framework localization examples under one canonical shared goal. | [LR2] | included |
| `tracing.configure-source-rules` | Configure activity sources/operations/listeners instead of wiring raw listeners. | Microsoft.Extensions.Diagnostics AddTracing/ActivitySourceFactory; exact package/asset and refresh semantics. | [L] | included |
| `logging.generate-generic-messages` | Generate logging methods without boxing generic arguments. | LoggerMessage generic methods; allows ref struct unsupported, SYSLIB1011. | [L] | included |
| `configuration.automatic-watcher-fixes` | Missing-root/relative-path binding and file-watcher reliability improvements. | No new configuration required; migration-breaking watcher semantics tracked separately. | [L] | excluded |
| `crypto.exchange-x25519` | Exchange X25519 keys through the platform abstraction instead of native calls. | GenerateKey platform implementations/import-export; check algorithm and OS support. | [L] | included |
| `crypto.compare-fixed-secret` | Compare a secret buffer with a known byte in constant time. | FixedTimeEquals overload; avoid turning a general comparison into a timing promise. | [L] | included |
| `crypto.wrap-keys` | Wrap keys using built-in unpadded AES-KW instead of a custom implementation. | RFC3394 sizing; padded KWP is older context, not falsely introduced in .NET 11. | [LR1] | included |
| `crypto.encapsulate-hybrid-secrets` | Use composite ML-KEM or HPKE through explicit algorithm negotiation. | Experimental SYSLIB5006/5009; platform/support checks; HPKE currently classical DHKEM only. | [LR2] | included |
| `crypto.encrypt-cms-with-mlkem` | Encrypt CMS for ML-KEM recipients through the supported envelope API. | Pure ML-KEM; non-Windows only according to cumulative notes. | [L] | included |
| `tls.drive-handshake-io` | Integrate TLS with caller-owned buffers and scheduling. | Experimental SYSLIB5007; TlsContext/Session/BufferSession/SocketSession status machine. | [LR1] | included |
| `http.compress-request-body` | Stream compressed request content instead of prebuffering custom wrappers. | GZip/Brotli/ZstandardCompressedContent; server support and Content-Encoding contract. | [L] | included |
| `http.retire-connections` | Retire a connection using endpoint/DNS policy beyond fixed lifetimes. | SocketsHttpHandler.ShouldEvictConnection; callback/lifetime safety. | [L] | included |
| `http.bound-initial-h2-streams` | Bound the initial HTTP/2 burst before peer settings arrive. | InitialHttp2MaxConcurrentStreams >=1; later peer limits still govern. | [LR2] | included |
| `dns.resolve-service-records` | Resolve typed records/custom DNS servers without a native wrapper. | Dns.Resolve*/DnsResolver; Windows/Linux/macOS, negative TTL/status handling. | [LR1] | included |
| `quic.prioritize-streams` | Assign transport priority to competing QUIC streams. | Priority 0–255; default 127; unsupported platform/transport prerequisites. | [L] | included |
| `mime.describe-video-content` | Use framework video media-type constants in content metadata. | Small grouped media-type subcase; no new transport capability. | [L] | grouped → mime.describe-content |
| `metrics.read-observable-http-data` | Update custom listeners to read newly observable HTTP instruments. | MeterListener.RecordObservableInstruments; exporter users may need no code change. | [LR1], [B] | included |
| `networking.automatic-benefits` | TLS hardening/Unix Negotiate channel binding, HTTP2 auth downgrade, Apple crypto fast paths and Pipelines contention fixes. | Exclude automatic effects; remove explicit version-workarounds only where a concrete migration is established. | [L], [LR1] | excluded |
| `unsafe.check-accessibility-contract` | Reconcile pointer-API annotation changes with C# unsafe compilation requirements. | Library overview's AllowUnsafeBlocks wording conflicts with language memory-safety requirements; do not repeat as verified. | [L], [C] | unresolved |

## Runtime and SDK authoring

| Proposed stable ID | Goal and authoring impact | Applicability / stability / verification constraint | Official evidence | Disposition |
| --- | --- | --- | --- | --- |
| `runtime.configure-crash-reporting` | Enable in-process crash diagnostics when out-of-process capture is unsuitable. | Linux/macOS/mobile; DOTNET_EnableCrashReport/Only and existing minidump precedence. | [RR1], [R] | included |
| `runtime.audit-hardware-baseline` | Adapt deployment support checks to the new CPU baseline. | Hardware requirement is a compatibility prerequisite; no blanket performance-switch recipe. | [R], [B] | included |
| `runtime.choose-async-codegen` | Select runtime-async for clearer live diagnostic stacks, or preserve an application's required async implementation boundary. | Preview; UseRuntimeAsync project configuration replaces removed DOTNET_RuntimeAsync/UNSUPPORTED_RuntimeAsync variables. Distinguish live stacks from exception stacks; verify runtime/package interoperability. | [R] | included |
| `runtime.link-native-wasm-code` | Relink a CoreCLR WebAssembly app with custom native code. | NativeFileReference/Emscripten pipeline; experimental platform/workload setup must be pinned. | [R] | included |
| `runtime.inspect-nativeaot-callers` | Use caller-assembly information where NativeAOT stack metadata permits it. | Assembly.GetCallingAssembly availability; diagnostics use case versus auto dispatch speedup. | [R], [B] | grouped → reflection.inspect-loaded-metadata |
| `runtime.automatic-improvements` | Runtime-async internals, JIT guards/devirtualization/constant folding/branch elimination, R2R comparers, VM dispatch, GUID caching, FP16 and >1024CPU initialization. | All top-level cumulative optimization families reviewed; no authored change established. | [R], [RR1], [RR2] | excluded |
| `runtime.automatic-wasm-diagnostics` | WebCIL defaults/loading, symbols, marshaling efficiency, minification, satellite fix and test-suite progress. | Custom native relinking is separate; no standalone recipes for internal/runtime fixes. | [R] | excluded |
| `runtime.automatic-reliability` | GC/cgroup/unload/queue fixes and unified profiler event coverage. | Profiler notification-list parsing fix alone does not establish a new author workflow. | [R], [RR1], [RR2] | excluded |
| `sdk.filter-solutions` | Build/load selected projects with CLI-managed solution filters. | dotnet sln .slnf; SDK version separate from application target. | [S] | included |
| `sdk.compose-file-apps` | Split file applications and manage project/DLL references without inventing a project. | Include/directives/reference --file; include exists on .NET SDK 10.0.300+, DLL inclusion/metadata routes differ. | [S], [SR2] | included |
| `sdk.configure-file-directives` | Express spaced properties and dependency metadata directly in a file application. | Quoted values, Aliases and matching duplicate directives; colon-containing property names invalid. | [SR1], [SR2] | grouped → sdk.compose-file-apps |
| `sdk.format-targeted-code` | Format a file app or one target framework with predictable configuration discovery. | Use .globalconfig for global policy, not unrelated is_global editorconfig; no speedup claim. | [S], [SR1] | included |
| `sdk.pass-launch-settings` | Pass process environment and evaluated build properties without shell-specific scripts. | dotnet run -e and launch-profile commandLineArgs expansion; avoid untrusted secret interpolation. | [S], [SR2] | included |
| `sdk.watch-app-hosts-and-devices` | Select app-host/mobile targets for the watch loop instead of custom launch orchestration. | Aspire/mobile integrations are dependencies; no broad Playbook native CI expansion. | [S] | included |
| `sdk.keep-build-configuration-explicit` | Preserve intended build configuration despite ambient Configuration values. | Pass -c explicitly or control environment; empty values ignored. | [SR1] | included |
| `sdk.install-preview-templates` | Select prerelease template packages deliberately. | new install --prerelease; exact version still wins; content recipe does not authorize project publication. | [SR1] | included |
| `sdk.configure-test-runs` | Choose runner/runtime/dependencies/modules and run-level limits explicitly. | MTP-only options versus VSTest, before/after --, timeout/failure codes and per-module reports. | [S], [SR1] | included |
| `sdk.choose-test-projects` | Use traversal projects and controlled module exclusions instead of repeated scripts. | Microsoft.Build.Traversal metadata/deduplication; --no-dependencies/--test-modules compatibility. | [S] | grouped → sdk.configure-test-runs |
| `sdk.create-test-runner-projects` | Create xUnitv3/NUnit projects with an explicit runner. | Built-in C#/F#/VB templates; package/runner versions are prerequisites, not extra runtime languages. | [S] | included |
| `sdk.run-mobile-tests` | Use workload test templates/device selection rather than a custom deployment runner. | MTP/mobile workloads and devices; guidance coverage does not add Playbook native test jobs. | [S], [SR1] | included |
| `sdk.select-affected-tests` | Run only change-affected tests through the documented mapping extension. | Cumulative SDK advertises it; RC1 says no complete shipped workflow/extension contract. Resolve actual distribution first. | [S], [SR1] | unresolved |
| `sdk.migrate-single-file-output` | Choose extraction settings without relying on legacy full extraction. | NETSDK1244; remove IncludeAllContentForSelfExtract or use native-only extraction when appropriate. | [SR1] | included |
| `sdk.package-tools-correctly` | Use SDK tool packing instead of an incompatible custom nuspec. | PackAsTool/NuspecFile NETSDK1235; package ID/layout conventions. | [S] | included |
| `sdk.declare-static-asset-groups` | Author grouped static assets and additional endpoints declaratively. | Static Web Assets SDK task/item contracts; SDK/component authors rather than all app consumers. | [S] | included |
| `sdk.include-typescript-assets` | Integrate compiled RCL TypeScript output with static asset discovery. | Verify Microsoft.TypeScript.MSBuild hook/dependency; auto rebuild fix alone is not a new sample. | [S] | grouped → sdk.declare-static-asset-groups |
| `sdk.select-local-container-engine` | Keep publishing directed at an intended container engine after default preference changes. | LocalRegistry override; platform-native wslc/container precede Docker/Podman. | [S], [B] | included |
| `sdk.publish-reproducible-containers` | Produce stable image digests from stable inputs. | SOURCE_DATE_EPOCH; registry cache bypass only when meaningful, not an invented speedup recipe. | [SR1] | included |
| `sdk.publish-multiarch-with-podman` | Publish multi-architecture images without assuming Docker. | Podman support; engine/manifest/platform requirements. | [S] | included |
| `sdk.configure-cli-telemetry` | Export SDK telemetry using standard OTLP environment settings. | SDK tooling configuration; inspect opt-out/explicit export behavior, never runtime telemetry in Playbook. | [S] | included |
| `sdk.cli-build-internals` | Installer deduplication, native CLI startup, default build server, partialR2R hooks, fish completion, watch recovery and diagnostic-noise fixes. | No distinct required implementation; explicit compatibility problems remain migration rows. | [S], [SR1] | excluded |
| `sdk.cli-output-and-fixes` | Reference-directory fallback, stderr launch notice, tool approval prompt/help fixes, NativeAOT reuse, corrected workload-version messages and reporter/native-DLL reliability. | Automatic defaults/fixes; file formatting/artifact layout covered in author-facing recipes. | [S], [SR1] | excluded |
| `sdk.analyzer-guided-logging` | Avoid expensive disabled logging work when a meaningful diagnostic identifies it. | CA1873 noise/message updates are not a new API; existing logging recipe can absorb guidance. | [S] | grouped → logging.generate-generic-messages |

## MSBuild, NuGet and migration review

| Proposed stable ID | Goal and authoring impact | Applicability / stability / verification constraint | Official evidence | Disposition |
| --- | --- | --- | --- | --- |
| `msbuild.archive-build-artifacts` | Create/extract tar artifacts with inbox build tasks instead of shell commands. | TarDirectory/Untar are .NET-MSBuild-only, not Framework MSBuild.exe; overwrite/incremental semantics. | [MR1] | included |
| `msbuild.inspect-item-globs` | Expose unevaluated include/exclude/remove patterns to build tooling. | MSBuildProvideItemGlobs/MSBuildItemGlob; patterns/order not expanded files. | [MR1] | included |
| `msbuild.migrate-xml-inputs` | Remove DTD dependence from build XML or explicitly allow only trusted input. | XmlPeek/XmlPoke/XslTransformation ProhibitDtd now true. | [MR1] | included |
| `msbuild.retire-old-wave-optouts` | Update custom builds instead of relying on retired feature-wave switches. | Retired17.10/17.12/17.14/18.3; clamping/MSB4272; exact affected custom behavior needs audit. | [MR1] | included |
| `msbuild.automatic-improvements` | Wildcard matcher speedups and server/task/build-engine reliability fixes. | No project change required; do not claim speed alone is a recipe. | [MR1], [MR2] | excluded |
| `nuget.delay-new-package-selection` | Configure dependency cooldown without hand-editing source settings. | minPublishAgeHours/--min-publish-age-hours; selection is not universal restore exclusion; floating conflicts NU1020. | [NR2] | included |
| `nuget.update-vulnerable-dependencies` | Update vulnerable packages while preserving warnings-as-errors policy. | package update --vulnerable; audit preview-restore relaxation does not ignore unrelated failures. | [NR1] | included |
| `nuget.make-package-timestamps-reproducible` | Choose deterministic package timestamps explicitly. | DeterministicTimestamp opt-in; distinct from container SOURCE_DATE_EPOCH. | [NR1] | included |
| `nuget.migrate-client-library-hosts` | Retarget tools referencing newer NuGet client libraries. | Client libraries now net10 assets; ordinary package-consuming app TFMs are unchanged. | [NR1] | included |
| `nuget.automatic-pack-fixes` | Evaluation reuse, nuspec substitution/fallback, source-provider and PMC Unicode fixes. | No new author workflow established. | [NR1] | excluded |
| `containers.rc-image-review` | RC1/RC2 container-image component changes. | Both canonical component notes explicitly report no image changes; SDK publishing changes are above. | [IR1], [IR2] | excluded |
| `migration.numeric-and-parsing` | Review DateOnly/TimeOnly TryParse invalid inputs, Complex special values, Math.Round, saturating small-integer conversions, tensor shape/layout and Japanese-calendar boundary changes. | Compatibility-index titles reviewed; per-note mitigations and exact APIs not yet audited. | [B] | unresolved |
| `migration.archive-streams` | Review compressed PackagePart non-seekable streams, Tar hard links, ZIP eager loading and checksums. | Do not claim migration details from a title alone; corruption recipe covers confirmed CRC portion. | [B] | unresolved |
| `migration.hosting-config-reload` | Review BackgroundService failure propagation, ignored reload failures/IO callbacks, watcher validation and async-options sync access. | Split concrete old/new behavior after linked note audit; async callbacks/options rows cover established APIs. | [B] | unresolved |
| `migration.paths-handles-and-clocks` | Review apphost invocation name, Unix IsAsync/pipe permissions, TickCount and custom Type nullable behavior. | Concrete compatibility topics from index; linked migration notes still need inspection. | [B] | unresolved |
| `migration.crypto-platforms` | Review macOS DSA removal, native CompositeMLDsa/HKDF, AIA limits and crypto obsoletions. | Algorithm/platform migration is eligible; exact alternatives and switches remain unverified. | [B] | unresolved |
| `migration.deploy-native-artifacts` | Review runtimeconfig.dev precedence, Unix NativeAOT lib prefix and stack-disabled caller-assembly behavior. | Deployment/package artifacts change authored consumers; detailed mitigation audit outstanding. | [B] | unresolved |
| `migration.sdk-package-hosts` | Review dnx global.json bypass, Framework mono launch target, package IDs/assets, restore selection, template TFMs and VSTest Newtonsoft removal. | Index titles reviewed; pins/routes and source-breaking contracts need linked-note audit. | [B] | unresolved |
| `migration.obsolete-api-index` | Replace specifically obsoleted APIs rather than blanket-suppressing diagnostics. | Core/cryptography non-default obsoletion inventories still need API-level review. | [B] | unresolved |

## Exact eight approved .NET10 foundations: cross-reference only

These rows preserve the eight already approved goals; they do not expand historical backfill. Proposed IDs are provisional aliases for consolidation with other inventory files. Compare their original examples with .NET9, while new .NET11 editions normally compare with .NET10.

| Proposed stable ID | Approved goal | Constraint | Existing approval | Disposition |
| --- | --- | --- | --- | --- |
| `csharp.extend-uneditable-types` | Extend a type you cannot edit with a computed property. | C#14 extension members. | [F10] | grouped → approved .NET10 foundation |
| `csharp.validate-property-assignment` | Validate or normalize property assignments without maintaining a backing field. | C#14 field-backed properties. | [F10] | grouped → approved .NET10 foundation |
| `aspnet-core.validate-requests` | Reject invalid Minimal API input before the handler runs. | .NET10 synchronous foundation; .NET11 async edition links shared validation. | [F10] | grouped → approved .NET10 foundation |
| `ef.include-archived-with-tenant-filter` | Include archived records while retaining a tenant filter. | EF10 named filters; not write protection/authorization. | [F10] | grouped → approved .NET10 foundation |
| `linq.retain-unmatched-rows` | Join two data sets while retaining unmatched rows. | .NET10 LeftJoin/RightJoin; .NET11 FullJoin goal remains distinct. | [F10] | grouped → approved .NET10 foundation |
| `json.reject-malformed-contracts` | Reject malformed JSON contracts at the import boundary. | .NET10 strict options; structural checks versus business validation. | [F10] | grouped → approved .NET10 foundation |
| `linq.transform-async-stream` | Filter and enrich an asynchronous stream without buffering it first. | .NET10 AsyncEnumerable; cancellation and old-package ambiguity. | [F10] | grouped → approved .NET10 foundation |
| `sdk.run-file-utility` | Run a small C# repository utility without creating a throwaway project. | .NET10 file apps; inherited CPM/build configuration. | [F10] | grouped → approved .NET10 foundation |

## Conflicts and remaining reconciliation

1. **C# status:** the cumulative language/library pages contain older preview wording for unions. Canonical RC1 stabilizes unions/closed hierarchies and RC2 points back to RC1. Memory-safety evolution remains separately gated; never carry its preview flag onto all C#15 recipes.
2. **Unsafe API claim:** the library overview suggests pointer annotation removal eliminates a project unsafe requirement, while language guidance still requires AllowUnsafeBlocks for relevant syntax. Treat this as unresolved until exact compiler/attribute behavior is checked.
3. **Testing workflow:** the cumulative SDK page describes affected-test selection; RC1 explicitly says its separately distributed extension/storage workflow was not complete. Experimental inclusion is approved, but a nonexistent dependency is not a runnable recipe.
4. **F# runtime boundaries:** runtimeTask is test code, not a library symbol; the named-arguments attribute is absent from the RC2 BCL. Native examples must use real shipped APIs or clearly supply original support code. F# JSON unions explicitly lack trimming/AOT/source-generation support.
5. **Introduced earlier:** padded AES-KWP and LeftJoin/RightJoin already existed in .NET10; new unpadded wrapping/FullJoin and tuple convenience are the .NET11 deltas. File-app include is also serviced into .NET SDK 10.0.300; DLL routes and metadata require their own version facts.
6. **Migration completeness:** the compatibility index is itself work in progress. Its unreviewed linked notes, C#/VB compiler breaking changes, earlier F# previews and public API diffs remain visible source gaps. No review/no-change finding here proves absence across every public API.
7. **Cross-family identity:** consolidate shared validation, media/URI and framework-specific example IDs before freezing catalogue metadata. Review standalone Extensions versions against the accepted release-family boundary; do not silently add every independently versioned Microsoft library.
8. **Evidence gate:** every included goal still needs original older/newer source, exact SDK/packages, compilation and focused behavior evidence where useful. No SDK/package installation, example code, tests, actual platform checks or agent evaluations were performed for this inventory. Source claims and compiled/executed evidence remain separate.

The next work is source/API reconciliation and recipe shaping inside accepted scope, not another decision about whether F#/VB, shared libraries or experimental APIs belong.
