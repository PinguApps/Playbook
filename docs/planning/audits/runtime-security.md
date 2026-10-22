# Runtime, native interop, transport and security coverage: PIN-705

Reviewed **22 October 2026**. This is a bounded artifact/source audit, not recipe
delivery or permission to publish. **25 narrow delivery drafts** below await
review; none is created or implemented by this audit.

## Scope, provenance and evidence

[PIN-705](https://linear.app/pinguapps/issue/PIN-705), SPEC.md at
`0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 100, 101, 103, 158–160;
approved proposal `b3778cc`, key `API_RUNTIME`. Consulted the historical spec and
proposal, current planning README, coverage/implementation/verification plans,
glossary, ADRs 0001–0004, contribution/dependency policies, library inventory and
neighboring data/I/O, serialization, Extensions and cryptography audits. Root
PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. The historical claim that
implementation has not begun is stale: current recipes, samples and the installed
MCP harness exist. No explicit target provenance pointer is unavailable. Linear's
optional shared-context call returned `Tool get_agent_context not found`.

Boundary: PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project
`b22380c7-6fde-49f4-8826-e5631dc5df7a`. Both IDs were used in paginated overlap
queries and checked on related reads. No native blocker exists. Numeric order,
shared files and possible merge conflicts do not create prerequisites. Only
PIN-705 is mutated; related tickets retain their titles, estimates and states.
The archived-inclusive overlap search returned 317 issues with both IDs verified.

The family includes remaining runtime/compiler infrastructure and reflection,
threading/synchronization, managed/native/JavaScript interop, HTTP client, sockets,
DNS, QUIC, TLS, security/cryptography and corresponding public tooling. It excludes
data/text/numeric/I/O algorithms (PIN-703), serialization (PIN-704), Extensions and
managed observability (PIN-706), SDK/build authoring (PIN-707), ASP.NET/gRPC and web
authentication integration (PIN-709), and native UI/workload APIs. Shared artifacts
can contain another family's types: ownership below prevents duplicate delivery.
DirectoryServices/LDAP, platform SDKs, native provider internals and third-party
gRPC packages are not included in the selected binary surface; no absence-of-change
claim is made for them.

[Inspect-RuntimeSecurityApis.ps1](../../../eng/Inspect-RuntimeSecurityApis.ps1)
reads metadata without executing inspected assemblies, restoring consumers,
installing SDKs/workloads or invoking first-run setup. It downloads exact NuGet
ZIPs and immutable source files. It never changes PATH. The
[asset manifest](runtime-security.assets.json) stores assembly/ZIP/tool hashes,
whole-surface fingerprints, complete declaration deltas, all bounded obsolete
declarations and actual browser TypeScript line differences. These are declaration
counts, including accessors/annotations/constraints/defaults/forwarders, not recipe
or distinct method counts. This is not a binary compatibility certification.

| Baseline | SDK directory | Reference pack and matching packages | Reviewed contracts |
| --- | --- | --- | --- |
| Previous | 10.0.402 | 10.0.13 | net10.0 references and package lib/net10.0 |
| Current | 11.0.100-rc.2.26504.105 | 11.0.0-rc.2.26504.105 | net11.0 references; package lib/net10.0 and lib/net11.0 |

Each baseline selects **62** framework reference assemblies. In the large Runtime,
Runtime.Extensions and Runtime.Handles assemblies, the explicit type predicate
selects Runtime/Reflection/Threading infrastructure, GC/environment/application
runtime types and safe handles, excluding already-owned file/pipe/process handles,
process-start arguments and SequenceMarshal. Other selected assemblies are read in
full, including public/protected members and forwarding facades.

Six matching packages per baseline: **Microsoft.Bcl.Cryptography**,
**System.Security.Cryptography.Pkcs**, **System.Security.Cryptography.Xml**,
**System.Security.Cryptography.ProtectedData**, **System.Net.Http.WinHttpHandler**,
**System.Threading.AccessControl**. Nuspec repository commits are dotnet/dotnet
`4099f83977f4d29604a9aeb971fb0a7bc1d5f699` and
`820230c386db5b0106c5430b3e5aee92a7826bf5`. Pkcs requires the actual Bcl.Cryptography
dependency to resolve metadata; the inspector chooses its matching/nearest lower
reviewed asset. Other packaged lib TFMs are enumerated and hashed, not declared
compatible consumer routes. The stable System.Security.AccessControl package does
not have these version numbers; its framework reference is compared instead.

Actual reference-pack **Microsoft.Interop** generator/support DLLs and localized
resources are hashed, together with the SDK's MetadataLoadContext. LibraryImport
and COM generator descriptors/entry points are compared at the package-matched
source commits. LibraryImport adds error **SYSLIB1064** for missing explicit safety
modifiers under updated memory-safety rules; COM adds property/accessor diagnostics
under existing diagnostic IDs. Hashing a generator is not executing generated code.

Actual **Microsoft.NETCore.App.Runtime.Mono.browser-wasm** ZIPs at both versions
and the new RC2 **Microsoft.NETCore.App.Runtime.browser-wasm** ZIP supply the
reviewed `runtimes/browser-wasm/native/dotnet.d.ts`. There is no invented .NET 10
CoreCLR browser package. Both current declarations are compared with the previous
Mono contract; new CoreCLR-specific differences are not presented as Mono removals.
The actual packages differ from the VMR TypeScript source snapshot, so packaged
contracts, not source alone, bound R14. Archive presence does not prove workloads,
Emscripten, browser execution or native compilation are available.

The [167-file source manifest](runtime-security.sources.json) pins immutable URLs,
SHA-256 values and headings:

- dotnet/core `ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`: all nine Preview 1–7,
  RC1 and RC2 library **and runtime** notes, RC2 release.json, and all published
  preview API-diff files matching the selected Runtime/Reflection/Threading/Net/
  Security families. RC1/RC2 lack that per-assembly diff series; actual end-to-end
  assets provide the comparison instead.
- dotnet/docs `9b5f5f81de8fd9851fc46273e93e8664d9cc3556`: complete cumulative
  runtime/library articles, compatibility index, every full .NET 11 core-library,
  cryptography, networking, deployment, interop and JIT compatibility file, global
  obsolete overview, and SYSLIB0064/0065 articles. Full behavior, rationale,
  mitigation and affected-API sections were considered, not only index titles.
- Both VMR commits above: 17 paired family reference files, LibraryImport/COM
  diagnostics, LibraryImport entry points and browser TypeScript source. Current
  memory-safety generator support, native-export/layout definitions and selected
  provider/HTTP/XML implementation paths qualify the new surfaces. Paired XML
  mitigation and SOCKS implementations distinguish servicing backports and new
  authoring from release-note history.

The manifests are complete within these recorded selections/commits, not proof
that an evolving Microsoft index lists every behavior change. PIN-962 retains GA
reconciliation; PIN-959 retains delivery/compilation and publication closure.

## Actual API and tooling dispositions

[Ordered rules](runtime-security.dispositions.json) assign **every** added and
removed declaration. Unmatched lines fail inspection. A group is an accountable
audit disposition, never proof that every subcase has a recipe. In particular,
large safety and stability annotation changes are not hundreds of new algorithms.
The selected framework/package comparisons retain **1,165 added / 421 removed**
lines, including repeated target-asset comparisons.

| Difference | Authored impact and accountable disposition |
| --- | --- |
| HMAC*/Kmac*/KmacXof* Verify/VerifyAsync; IncrementalHash VerifyCurrentHash/VerifyHashAndReset; CryptographicOperations.VerifyHmac | **Included R1:** remove compute/compare plumbing, preserve fixed-length comparison, stream cancellation and incremental/reset semantics. Unkeyed hash verification is integrity comparison, not authentication. |
| X25519 classes/platform adapters; fixed-time repeated-byte predicate; AES-KW | **Delivered PIN-750:** [actual evidence](../../verification/PIN-750-751.md). Supporting imports/exports, wrong-peer/key/tamper checks and KW versus KWP remain with the established three goals. No blanket platform or timing benchmark claim. |
| CompositeMLKem/Cng/algorithm sets, HPKE suite/sender/recipient, CNG algorithm identifiers; Pkcs CMS overloads | **Grouped PIN-751:** delivered bounded composite, classical HPKE and pure ML-KEM CMS operations in the same evidence report. RC2 PQ-HPKE and composite CMS limitations remain explicit. Additional overload presence is not additional native success evidence. |
| MLDsa/MLKem PKCS#8/SPKI methods and certificate accessors lose experimental annotations; Bcl.Cryptography net11 forwarders | **Grouped:** publisher stability/asset correction alongside PIN-751, not newly invented algorithms. The same methods are present with SYSLIB5006 in the previous declarations. Composite contracts retain their own experimental status. Bcl net10 certificate-accessor annotations do not establish net11-only CMS availability on net10. |
| SAN AddRegisteredId(Oid) | **Included R2:** encode a registered identifier rather than manually building the ASN.1 extension. Not a DNS identity or certificate trust mechanism. |
| CngProperty ReadOnlySpan constructor and readonly/nullable annotations; SafeEvpPKeyHandle.OpenKeyFromProvider(IEnumerable<string>, keyUri, propertyQuery) | **Included R15:** deliberate provider/key configuration. CNG span input is supporting no-intermediate-array property setup, not a new algorithm. OpenSSL provider list/URI and ownership require actual provider evidence, separate from generic X25519. |
| AesGcm/AesCcm KeySizeInBytes | **Included R16:** inspect an existing AEAD instance's key size without retaining duplicate state. Not tag/nonce/ciphertext size, nor a reason to choose a weaker key. |
| Typed DNS envelopes, resolver/server options and Address/SRV/MX/TXT/CNAME/PTR/NS records | **PIN-754** retains the SRV service-discovery recipe. **R3** covers non-SRV record inspection, response codes, per-record TTL and negative TTL. Shared resolver/envelope plumbing is explained once. Preview 7's Windows-only claim is superseded by RC1 Linux support; no universal OS claim. |
| Socket.ConnectAsync(..., ConnectAlgorithm), Default/Parallel | **Included R4:** explicit dual-stack connection racing replaces custom address scheduling; real controlled IPv4/IPv6 and cancellation evidence required. |
| IPEndPoint UTF-8 Parse/TryParse, char/byte TryFormat, parsing/formatting interfaces and nullable adjustments | **Included R5:** parse/format caller-owned endpoint text without intermediate strings. Existing constructor/address APIs are annotation changes, not removed functionality. |
| NetworkChange.IsSupported; NetworkInterface/IPGlobalProperties/event platform annotations | **Included R6:** guard network-change monitoring and provide an explicit fallback. Other annotation corrections are supporting platform constraints, not distinct networking features. |
| Experimental TlsContext/TlsSession/TlsBufferSession/TlsSocketSession/status | **Grouped PIN-752:** SYSLIB5007, ownership, state/certificate transitions and actual I/O remain its required evidence. This audit does not claim a handshake. |
| QUIC Priority/DefaultPriority | **Grouped PIN-755:** actual native transport/support checks; a relative scheduler hint is not guaranteed bandwidth/order. |
| HTTP compressed-content wrappers, connection IDs/eviction contexts, ShouldEvictConnection and initial HTTP/2 stream bound | **Grouped PIN-753:** all compression options, correlation and pool-lifetime support belong to its existing HTTP connection goals. Preserve experimental eviction diagnostic/opt-in and peer SETTINGS constraints. |
| DecompressionMethods.Zstandard | **Included R21:** response decompression is distinct from PIN-753's outgoing request compression. |
| Synchronous HttpClient/handler/invoker Send adds Android/iOS/tvOS unsupported annotations | **Included R20:** source/analyzer migration to asynchronous I/O on affected platforms; no claim that synchronous mobile I/O previously worked. |
| Video MIME constants | **Delivered PIN-744** in [the URI/media evidence](../../verification/PIN-742-744.md). **R19** separately covers MediaTypeMap's extension/type lookups; constants alone do not deliver that mapping goal. |
| Generic Interlocked.And/Or; CompareExchange/Exchange annotations | **Included R8:** atomic enum/integer flags without hand-written casts/CAS loops. Annotation-only overload pairs are not new atomic operations. |
| RuntimeFeature.IsMultithreadingSupported | **Included R7:** choose a supported threading path with a useful single-thread fallback; compile-time platform guards are not evidence of actual parallelism. |
| ExtendedLayoutAttribute/Kind.CStruct/CUnion, LayoutKind.Extended, TypeAttributes.ExtendedLayout | **Included R9:** runtime-computed C aggregate layout, with actual native ABI checks. A declaration is not proof of every compiler/architecture combination. |
| ILGenerator.EmitCalli(Type) | **Included R10:** emit a call with the real function-pointer signature, including calling convention. Dynamic-code availability is a prerequisite, not a Native AOT promise. |
| JSMarshalerArgument float[], Span<float>, ArraySegment<float> ToJS/ToManaged | **Included R11:** remove manual float conversion/copy adapters; real browser/lifetime/segment evidence required. Supporting generated APIs are not ordinary hand-written marshaller instructions. |
| UnmanagedCallersOnly.AssociatedSourceType; EntryPoint/type nullable metadata | **Included R12:** condition a native export on retention of its associated type. EntryPoint already existed; named exports themselves are not new. Actual trimming/export-table evidence is required. |
| LibraryImport SYSLIB1064 and broad RequiresUnsafe/MemorySafetyRules annotations on Marshal, COM, Java/Objective-C/Swift, GC handles, MemoryMarshal, Unsafe, SafeBuffer, crypto/native handles and reflection pointers | **Grouped PIN-715:** caller/declaration safety obligations; its existing measured compiler tests do not certify every API here. Keep safe/unsafe/preview/build flags precise. MarshalUsing property AttributeUsage additionally supports **R13**. |
| COM generator property/accessor support and new diagnostics under existing IDs | **Included R13:** author complete generated COM properties with correct accessor marshalling; not merely warning suppression. |
| RuntimeHelpers.AllocateTypeAssociatedMemory(type,size,alignment), Box | **R22** owns deliberate aligned lifetime-bound allocation. Box's RequiresUnsafe annotation stays PIN-715. |
| ConstructorInfo.GetGenericArguments/nullable adjustments, TypeDelegator and Emit-builder nullable overrides; AssemblyLoadContext location override | **Delivered PIN-742:** [metadata/deployment evidence](../../verification/PIN-742-744.md). Constructor generic arguments are distinct from constructor value parameters. |
| AsyncHelpers/await intrinsic shapes | **Grouped PIN-757**, original F# builder PIN-980. Compiler-targeted helpers must not be called as general application helpers. |
| IUnion/UnionAttribute/IsClosedTypeAttribute | **Grouped PIN-711**, serialization/closed-hierarchy behavior PIN-704. Compiler infrastructure does not create an extra runtime recipe. |
| NFloat partial parsing; PosixSignal.SIGKILL | **Grouped PIN-737** numeric parsing and **PIN-721/722** process signalling. SIGKILL is not a catchable signal registration. |
| CurrencyWrapper/Objective-C EditorBrowsable metadata; other nullable/readonly/AttributeUsage-only pairs | **Excluded as standalone goals:** no distinct implementation outcome established; raw differences remain visible. This does not exclude the separate safety/COM property changes above. |
| Actual Mono/CoreCLR dotnet.d.ts changes | **Included R14:** custom startup/run lifetime, deprecated withConfigSrc/run/environment setter, changed asset exports/configuration and diagnostic naming. New CoreCLR types are tracked separately from a changed Mono contract. |
| Zero-delta selected forwarding, claims/access-control, WebSocket, HTTP JSON/SSE, XML/ProtectedData/WinHTTP/access-control package contracts | **No additional API recipe:** zero declarations do not imply identical behavior. Source-only changes and unknowns below remain visible. |

## Complete source-only and obsolete-index ownership

| Finding | Disposition and limits |
| --- | --- |
| All five cryptography compatibility notes, general-index SYSLIB0064 and adjacent server TLS AIA default | **Already assigned PIN-686.** Consume its [full mitigation analysis and seven drafts A–G](cryptography-platforms.md); do not redo or declare those recipes delivered. DSA macOS removal, native composite/HKDF limits, OpenSSL AIA budget and AsnEncodedData setter semantics remain distinct. |
| SYSLIB0064 / SYSLIB0065 in actual deltas and full obsolete sources | **PIN-686 semantic owner**, PIN-689 index map. Only the RawData setter is newly obsolete; explicit RSA padding must retain provider/protocol support. No blanket suppression or unsupported OAEP-SHA256 replacement. |
| Global obsolete index through SYSLIB0066 | **PIN-689 retains exhaustive cross-family ownership.** SYSLIB0066 belongs PIN-685/PIN-706, SYSLIB0063 PIN-684/PIN-741, SYSLIB0061 PIN-703, SYSLIB0062 PIN-704. Older SSL/crypto/COM/CAS/remoting/formatter obsoletions present on both selected baselines are retained in complete manifests; no full historical backfill or new .NET 11 recipe is inferred. SYSLIB5006/5007/5008/5009 are experimental diagnostics, not obsolete replacements. |
| Unix named mutex scope/storage, introduced in both 10.0.13 and RC2 | **Included R23:** explicit scope and writable user-owned runtime directory for UID-only containers. No action for unaffected same-user callers. Never describe this as an 11-only change or use HOME as the user-database fallback. |
| runtimeconfig.dev precedence and Unix NativeAOT lib prefix | **Already assigned PIN-687**, not a duplicate deployment audit. NativeAOT/stack-disabled caller inspection is delivered in PIN-742; broader deployment migration remains open. |
| Minimum hardware, in-process Unix crash-report variables and NativeAOT createdump tool path | **Grouped PIN-756:** authoring configuration/scripts and real output, not invisible performance. Keep CPU requirements distinct from optional R2R targets, and crash-only versus minidump paths distinct. |
| Runtime async compiler opt-in, removed DOTNET_RuntimeAsync/UNSUPPORTED_RuntimeAsync switches, live stack versus exception traces | **Grouped PIN-757/PIN-980.** Later no-preview-API gate supersedes early notes; do not turn Preview 4's UseRuntimeAsync wording into verified current project configuration without the compiler check. Automatic continuation/context/tail-call optimizations alone are excluded. |
| CoreCLR WASM native relinking/NativeFileReference/Emscripten | **Grouped PIN-758:** actual workload/native function/browser proof. WebCIL default, JS minification and repaired satellites alone are automatic; custom loader contract changes remain R14. |
| HTTP observable instruments | **Delivered PIN-749**, [real listener/loopback evidence](../../verification/PIN-748-749.md); not a new transport goal. |
| iOS/tvOS file-locking default, pipe permissions, clock/apphost identity and custom-Type nullable behavior | **Already assigned PIN-684**, PIN-741 and delivered PIN-742 as applicable. No duplicate native/UI matrix. |
| SOCKS5h scheme | **Included R18:** proxy-side DNS selection is authored URI configuration. Paired SocksHelper sources establish the new accepted scheme; no local-DNS privacy or production proxy claim is inferred. |
| HTTP/2 Windows-authentication downgrade | **Included R17:** remove a manual request-version workaround only after actual policy/auth/protocol checks. Automatic benefit without removing that workaround is excluded. |
| TLS Linux certificate alerts, malformed-handshake bounds checks, Apple AEAD acceleration, MsQuic update, proxy/socket/WebSocket/SMTP fixes | **Excluded as standalone goals:** automatic correctness/performance; no specific authored workaround removal established. Server TLS certificate-supply policy remains PIN-686; Unix channel binding web integration remains PIN-709. |
| XML signature/encryption hardening in Preview 4 | Depth/DTD/transform mitigations already exist in the serviced 10.0.13 source, so these are not all new .NET 11 features. **Included R25:** SignedInfo's former fixed 100-reference limit becomes configurable through `System.Security.Cryptography.MaxReferencesPerSignedInfo`, still defaulting to 100. The MaxDecryptedDataElements key was already spelled that way in the old constant; only its comment was corrected. No invented key migration or advice to disable security limits. |
| 32-bit GC heap hard limit support | **Included R24:** deliberately bounded heap configuration or removal of custom 32-bit enforcement; actual 32-bit runtime evidence required. Ordinary unchanged GC/DATAS/cgroup fixes are excluded. |
| Remaining JIT/VM/GC/NativeAOT dispatch, >1024 CPU startup, Half/SIMD acceleration, marshalling fixes | **Excluded as standalone goals:** upgrading alone obtains the benefit. Explicit authored intrinsics remain PIN-739 and numeric conversion migrations PIN-683. Source-build-only interpreter bring-up options are not advertised as consumer SDK properties. |

### Retained unknowns

- **U1 — native/public contracts:** generated source declarations and reference
  attributes do not establish supported custom marshaller/provider/ABI routes on
  every platform. R9–R15 name the actual consumer/native evidence still required;
  no support is inferred from a public type or a source comment alone.
- **U2 — public runtime tooling beyond declarations:** interpreter perf-map settings
  (`DOTNET_PerfMapEnabled=1`, `DOTNET_PerfMapStubGranularity=2`), Mono compressed
  interface-bitmap switches/finalizer hooks, and the Preview 3 HttpListener kernel
  buffering claim lack a demonstrated supported consumer route in the inspected
  public surface. Retain source findings under PIN-705; no made-up public property
  or assertion that these are only automatic. Qualify actual configuration/native
  tooling before proposing a recipe. Native profiler/cDAC contracts are not
  certified by this managed metadata audit.
- **U3 — platform/GA execution:** no complete Mac/CNG/OpenSSL/QUIC/browser ABI or
  algorithm matrix, final GA assets, native toolchain availability, or older-target
  package interoperability was established. These are delivery prerequisites, not
  audit verification failures or permission to relabel RC proof.

## Delivery drafts for review

**Approval-only proposals, not new Linear tickets.** Use the existing
02 - Language and library guidance milestone; allocate unused `02-DDD` titles only
after approval and a fresh numbering check. Native estimates follow observed
S=2, M=3, L=5. Each row is one complete recipe PR. Estimates include source
reconciliation, original implementation, tests and packaged verification.

Every draft requires original fictional-domain .NET 10 versus pinned .NET 11
consumers, complete offline Markdown/metadata/canonical snippets, explicit runtime,
SDK/language/package target assets/platform/provider/native requirements, sources,
numbered migration steps and honest alternatives. Explain unavailable older
capabilities; add no ninth .NET 10 foundation. Compile both actual consumers;
capture exact inputs/artifacts/toolchain and focused positive/failure evidence.
Retain experimental opt-ins where the actual API requires them. No API presence,
mock or negative support check substitutes for required positive execution.

Each slice must then build/pack once and use the existing isolated installed-tool
stdio list/get harness offline from an unrelated directory: ID/title/**goal**,
full body, exact target/edition, applicability, stability and evidence. Update the
register and evidence freshness. No recurring native/client/OS matrix, provisioning
or publication is added. Missing compilation/native proof blocks that slice. GA
reconciliation and the comprehensive gate remain separate obligations.

| Draft / proposed goal | Size and exact prerequisites beyond the common baseline | Required real evidence |
| --- | --- | --- |
| **R1 `crypto.verify-message-authenticators`** — authenticate a harbour sensor packet or stream | **M (3)**; available SHA-256 HMAC and explicitly supported KMAC provider for any KMAC claim; original known-answer inputs | Previous HashData + FixedTimeEquals versus Verify; good/bad/truncated MAC, wrong key/data, actual stream cancellation, incremental preserve/reset behavior and KMAC customization/support. No timing benchmark claim. |
| **R2 `certificates.encode-registered-identifiers`** — attach an OID identity to a survey certificate | **S (2)**; synthetic local certificate/key and supported ASN.1 reader | Read back the actual SAN registeredID tag/OID, multiple entries, invalid OIDs and unrelated DNS entries; compare manual older encoding. No trust or network identity-validation claim. |
| **R3 `dns.inspect-typed-records`** — inspect zone routing/metadata beyond SRV | **M (3)**; controlled authoritative DNS fixture and explicit Windows/Linux resolver/native prerequisites; no public-zone dependency | Actual A/AAAA, MX/TXT/CNAME/PTR/NS fields/TTLs, NODATA versus NXDOMAIN, negative TTL, server override and cancellation. Reuse PIN-754's envelope explanation; SRV selection stays there. |
| **R4 `sockets.race-dual-stack-connections`** — connect to a dual-stack survey gateway | **M (3)**; controlled IPv4/IPv6 listeners, DNS answers and reachable/unreachable routes | Execute Default/Parallel, one-family failure, selected peer, event completion and cancellation/cleanup. Compare the old explicit scheduling without claiming fixed latency wins. |
| **R5 `networking.parse-endpoint-buffers`** — read/write endpoint fields in UTF-8 buffers | **S (2)**; actual span consumers, no external service | IPv4/bracketed IPv6/ports, malformed/range failures, small destination/written length and unchanged buffer boundaries; fair older string adapter. |
| **R6 `networking.guard-change-monitoring`** — monitor connectivity with a supported fallback | **M (3)**; one real supported OS and an actual unsupported target/runtime; controlled interface change access | IsSupported, real event/unsubscription, fallback when unavailable and platform-analyzer compilation. A successful build alone is not an event. |
| **R7 `runtime.select-threading-path`** — choose parallel work only where supported | **M (3)**; normal multithreaded runtime and real single-thread browser/WASI route with exact workload | Exercise both branches, same result and cancellation; show single-thread fallback never schedules unsupported threads. No parallelism claim from platform annotations. |
| **R8 `threading.update-enum-flags-atomically`** — update independent station flags concurrently | **M (3)**; supported integer/enum backing sizes, ordinary desktop runtime | Real contended And/Or preserves unrelated bits and reports prior values; compare original CAS/cast code, reject unsupported structs, do not infer transactional multi-field updates. |
| **R9 `interop.match-native-c-layout`** — exchange an original C struct/union | **L (5)**; matching C compiler, target ABI/pointer width, native helper and actual supporting .NET 11 runtime | Native sizeof/alignof/offset and round trips for struct/union, incorrect-layout negative control; compare explicit older layout. Record compiler/OS/architecture, not a universal ABI claim. |
| **R10 `reflection.emit-function-pointer-calls`** — generate a typed native callback invocation | **M (3)**; dynamic-code-supported runtime, actual managed/unmanaged function-pointer signatures and native helper if claimed | Execute emitted call, arguments/return and calling convention; older EmitCalli signature setup comparison. Guard unavailable dynamic code; no Native AOT execution claim. |
| **R11 `interop.exchange-float-buffers-with-javascript`** — pass survey samples across JS/.NET | **L (5)**; actual browser workload/build output, browser and exact JS generator/runtime | Real float[]/span/segment round trips including segment offsets and lifetime/disposal, NaN/infinity where supported; inspect generated imports/exports. Compare original conversion adapter, not a managed-only mock. |
| **R12 `interop.retain-native-exports-with-types`** — trim a plug-in export with its source type | **L (5)**; supported Native AOT compiler, linker/export-inspection tool and native loader on a stated OS | Publish retained and trimmed original types, inspect exported symbols and invoke retained entry point from native code; absent export control. Keep EntryPoint's existing role separate from AssociatedSourceType. |
| **R13 `interop.generate-com-properties`** — expose a generated COM property contract | **L (5)**; Windows COM authoring route, exact COM generator and native/independent COM consumer | Execute get/set/marshalling across the real boundary; compile all-or-nothing accessor and invalid MarshalUsing diagnostics. Previous explicit methods remain valid. Test updated safety modifiers separately from property support. |
| **R14 `wasm.migrate-custom-runtime-hosts`** — migrate a hand-written browser loader | **L (5)**; actual old/current Mono browser packages, RC2 CoreCLR package/workload, TypeScript compiler, browser and preserved original assets | Compile against each actual dotnet.d.ts; run startup/main lifetime, configuration/resource loading and diagnostic provider naming. Separate Mono changes from new CoreCLR APIs; verify removed/deprecated asset/config paths without assuming the VMR source equals the shipped contract. |
| **R15 `crypto.configure-provider-backed-keys`** — open a key with deliberate provider selection | **L (5)**; isolated OpenSSL provider/key URI/store and measured provider versions; Windows CNG route only for its separately labelled span-property subcase | Actual ordered provider selection/property query, sign/verify, missing provider/key failures, ownership/disposal; CNG property copy behavior when included. No FIPS/HSM/provider access is assumed. |
| **R16 `crypto.inspect-aead-key-size`** — report a configured AEAD instance's key size | **S (2)**; supported AES-GCM/CCM provider and test keys | Check all supported key sizes and existing encryption/decryption behavior, distinguish tag/nonce sizes and disposed behavior; compare explicit older bookkeeping. |
| **R17 `http.remove-manual-windows-auth-downgrade`** — let a gateway negotiate authentication | **L (5)**; isolated authorised Windows-auth endpoint, HTTP/2/TLS, test identity/SPN as required and captured protocol | Actual authenticated request downgrades where allowed; strict-version policy, unauthenticated and failure cases; remove only the verified manual override. No credential/account provisioning or production endpoint use. |
| **R18 `http.resolve-dns-through-socks-proxy`** — use proxy-side DNS for an isolated relay | **M (3)**; controlled SOCKS5 server and target hostname intentionally unavailable to local DNS | Actual socks5h proxy receives hostname and connects, failure/auth/cancellation behavior; previous supported proxy spelling/adapter comparison. Do not infer global DNS-leak prevention from one request. |
| **R19 `mime.map-file-and-content-types`** — label a survey attachment by known extension | **S (2)**; actual MediaTypeMap consumer and original file-name inputs | String/span mappings, casing/unknowns/path handling and reverse extension ambiguity; compare older curated map. Mapping does not validate file bytes or make untrusted uploads safe. |
| **R20 `http.replace-mobile-synchronous-sends`** — await a mobile request | **L (5)**; actual Android or Apple target/runtime, matched native workload/toolchain (Mac/Xcode for Apple) and controlled endpoint | Compile old/new platform warnings and execute async success/cancellation/failure; verify the exact affected Send annotations. No assertion that old sync mobile requests succeeded; no recurring device CI matrix. |
| **R21 `http.decode-zstandard-responses`** — receive a compressed response | **M (3)**; controlled HTTP fixture emitting real zstd and other/stacked encodings | Assert request negotiation, body and headers, unsupported/malformed responses and cancellation; compare manual older decoder. Distinct from request compression and no unsupported package backport. |
| **R22 `interop.allocate-aligned-type-lifetime-memory`** — allocate native state tied to a type | **M (3)**; dynamic/collectible type route where supported and measured alignment | Actual address alignment, usable storage, invalid size/alignment and unload/lifetime safety; compare older allocator/free ownership. Never use freed storage to demonstrate a negative. |
| **R23 `threading.choose-unix-mutex-scope`** — coordinate station processes under explicit users | **L (5)**; isolated Unix environment with two test UIDs, writable owned runtime directory, actual 10.0.13/RC2 and controlled older servicing baseline when demonstrating the historical change | Same-user coordination, cross-user isolation and explicit global opt-in, unknown-UID failure then XDG_RUNTIME_DIR remedy; real process behavior and cleanup. Both pinned baselines already have the new default. |
| **R24 `runtime.bound-a-32-bit-gc-heap`** — bound a 32-bit worker's managed heap | **M (3)**; actual supported 32-bit runtime/OS, isolated disposable worker and measured memory | Real configured limit observed under controlled allocation and failure handling; compare older support/custom enforcement honestly. Separate managed heap budget from total RSS/native allocations. |
| **R25 `crypto.bound-xml-signature-references`** — enforce a deliberate XML signature reference budget | **M (3)**; exact old/current Xml packages, original signed local XML fixtures and separate processes for static configuration | Verify the default 100-reference boundary and a stricter configured budget through actual SignedInfo parsing/signature checks; malformed/over-budget rejection and unaffected valid signatures. Compare the old fixed limit/application validation. Configure before first use; do not disable other XML protections or treat a valid signature as authorisation. |

R1–R25 do not consume PIN-686's seven existing proposals. Unknowns U1/U2 need
bounded factual qualification before an additional delivery ticket is justified;
they remain explicit coverage/publication gaps. No related ticket is marked
satisfied merely because a broad owner or a similar API name exists.

## Reproduce and inspect

Requires PowerShell 7, network access, and the exact existing SDK/reference-pack
directories in the table. `-Net10Root` / `-Net11Root` can select installations at
other paths without modifying PATH. No consumer build or workload installation is
needed for this metadata/source check. Run the following from any directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-d704162a'
& ./eng/Inspect-RuntimeSecurityApis.ps1 `
  -OutputPath ./artifacts/runtime-security.replay.json `
  -CompareWith ./docs/planning/audits/runtime-security.assets.json
$audit = Get-Content ./artifacts/runtime-security.replay.json -Raw | ConvertFrom-Json
$audit.dispositions | Group-Object group | Select-Object Name, Count
$audit.publicToolingDiff | Select-Object asset, owner
Get-Content ./docs/planning/audits/runtime-security.md
```

Expect exact package/tool/reference hashes and complete declaration differences to
match, all 167 source hashes to pass, and two browser-tooling comparisons owned by
R14. An unowned declaration or changed source/artifact fails the command. The
[verification ledger](../../verification/PIN-705.md) records actual replay and
negative checks. This research changes no served recipe/tool, so no changed MCP
response or Inspector example is asserted. Real installed MCP checks are mandatory
acceptance for every eventual delivery draft above.
