# Non-Blazor web authoring coverage: PIN-709

Reviewed **24 October 2026**. This is a bounded artifact/source audit, not recipe
delivery. Every proposal below is **delivery outstanding** and awaits Pingu's
decisions through [PIN-986](https://linear.app/pinguapps/issue/PIN-986).
Closing this audit does not clear comprehensive coverage, compilation, installed
content or GA publication gates. No served recipe or MCP contract changes here.

## Boundary and reproducible evidence

PIN-813's `signalr.refresh-live-credentials/11.0` comparison qualifies actual
10.0.13 and 11.0.0-rc.2.26504.105 .NET client/server sets. Direct and locally
redirected WebSocket checks observe accepted claims, policy/same-user rejection,
success/failure and hub events, and unchanged current connection/user routing.
The original gateway address needs its own refresh implementation; a negotiate
redirect alone is insufficient. This local gateway proves no Azure Service support.
The npm gap below remains a published-artifact fact. The authored TypeScript
comparison instead builds and executes the first-party source at
`4a70870818bf5662f21b0c28f9a8c267dccb77f0`, with a checked source archive,
locked build/runtime dependencies and retained MIT notice. It is explicitly a
development source build. ArchiveAccess separately observes actual Interactive
Server authentication-state notifications and user-content updates; automatic
SignalR/provider propagation is context, not a demonstrated production integration.
See canonical sample records and `eng/Record-LiveIdentity.ps1`; actual GA and
comprehensive publication gates remain outstanding.

[PIN-709](https://linear.app/pinguapps/issue/PIN-709), planning key `API_WEB`,
implements SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47,
107–113 and 158–160; approved proposal `b3778cc`. ADRs 0001–0004 retain the one
offline, read-only product, author-facing release-family scope, experimental
inclusion and GA reconciliation. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are
absent. The [original web register](../inventory/02-web-data.md) supplies the
existing goal identities; its headline inventory was not an exhaustive API audit.

The [replay](../../../eng/Inspect-WebApis.ps1), [artifact report](web.assets.json),
[source pins](web.sources.json) and [ownership rules](web.dispositions.json) record:

- Previous: **SDK 10.0.402**, framework references **10.0.13**, `net10.0`.
- Current: **SDK 11.0.100-rc.2.26504.105**, references
  **11.0.0-rc.2.26504.105**, `net11.0`.
- All 82 selected non-Components/non-Razor framework assemblies, including MVC's
  Razor integration, Identity Core/Stores and HTTP headers. Inspect public and
  protected types, constructors, methods, accessors, properties, fields/constants,
  events, interface lists, generic constraints, defaults, modifiers, nullable,
  obsolete/experimental attributes and forwarders. Full-surface fingerprints
  bound unchanged declarations; complete added/removed declarations are retained.
- 24 primary packages per baseline: OpenAPI, JWT/OIDC/WS-Federation/Negotiate,
  Identity UI/EF integration, Razor runtime compilation, Newtonsoft integration,
  both JSON Patch implementations, SignalR clients/protocols, ApiDescription.Server,
  MVC.Testing/TestHost, gRPC server/client/factory and JSON transcoding. Matching
  ASP.NET packages use each framework version. Actual gRPC template dependencies
  use **Grpc 2.64.0** on both sides. Swagger integration uses the last published
  stable **Microsoft.AspNetCore.Grpc.Swagger 0.10.11** on the previous side; there
  is no matching 11 RC2 package. DirectTls uses the available **RC1** package
  **11.0.0-rc.1.26425.128**, explicitly not an RC2 replacement.
- Package ZIP hashes, embedded repository commits, selected compile-asset TFMs,
  dependency minima and build/tool assets. Dependency packages are metadata
  resolution helpers, not additional product scope or a restored application.
  In particular, OpenAPI's model dependency has its own version line; PIN-828
  retains transformer migration ownership. No ordinary restore/build has run on
  these inspected packages. Framework identities take precedence for resolution.
- The actual Web.ProjectSystem task assembly's public surface, SDK props/targets,
  SDK-bundled `aspnetcoretools` binaries and old separate dev-certs/user-jwts
  layouts, package build files, C#/F# web/MVC/Razor Pages/worker/gRPC templates,
  and both bundled MCP template archives. Vendor assets retain hashes; automatic
  vendor refreshes are not independently authored goals.
- Actual npm **@microsoft/signalr 10.0.11** and
  **11.0.0-preview.7.26381.103** archives, ESM declaration differences and JS hashes.
  These are the published packages reviewed, not invented matching RC2 assets.

Inspection uses MetadataLoadContext and archive readers; it never executes the
inspected code, changes certificate trust, starts HTTP.sys, installs an SDK or
changes PATH. Text differences are ordinal line-membership comparisons; hashes
retain reorderings and duplicate-line changes. They do not prove semantic
equivalence of build scripts, templates or JS. Reconstruct complete archives for
body/order review. Different binaries/TFMs and metadata-only exclusions do not
establish NativeAOT, trimming, browser, provider or operating-system support.

### Canonical-source boundary

The 212 immutable files include **all nine** canonical ASP.NET Preview 1–7/RC1–2
snapshots; the full cumulative release root and every Markdown include; the full
ASP.NET 11 breaking-change directory and overview; the migration root and all
four includes; ASPDEPR overview/002 article; .NET 11 compatibility and custom
obsolete tables and the general SYSLIB index. Non-web/Blazor entries encountered
there are assigned below, not silently discarded. Framework `PublicAPI.Unshipped`
files, analyzer descriptors and focused implementations constrain interpretation.

- AspNetCore.Docs: `ecf7aca5136aea60aa8db7330f66ac799d3edcf7`.
- dotnet/docs: `5e19883f3aca93e649d9e3fb72b0b89091d7079f`.
- dotnet/core: `ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`.
- ASP.NET implementation/API sources: `4a70870818bf5662f21b0c28f9a8c267dccb77f0`;
  package repository commits are recorded independently, including VMR commits.

Every source has an immutable raw URL, SHA-256 and heading inventory. The first
three revisions were resolved from the live official repositories during this
audit; ASP.NET implementation is the pinned preparation source, not moving main.
Source wording is evidence of intent; actual artifacts determine callable APIs.
The bounded selected assets do not enumerate all historic TFMs/RIDs, private APIs
or independently released ecosystems. Later GA assets still require reconciliation.

## API and source dispositions

Each raw declaration difference must match an ownership rule or the replay fails.
Overloads/accessors/generated members group by reusable goal, not one recipe per
line. The only automatic metadata exclusion requires an opposite-side declaration
that becomes exactly equal after removing compiler state-machine/generated
attributes and normalizing whitespace. Other annotation changes are assigned
explicitly. This preserves real changes hidden among runtime-async conversions.

| Surface / source finding | Authored impact and accountable disposition |
| --- | --- |
| Async Minimal API validation, localization, stabilized validation attributes | PIN-774/775 and PIN-748/PIN-706 retain shared validation ownership; PIN-841 retains the .NET 9/10 synchronous foundation. No duplicate endpoint-only catalogue. |
| Binding-failure filters | Delivered PIN-815 ([proof](../../verification/endpoint-policies.md)). Scalar failure filters also work on actual .NET 10 and in official v10.0.0 source; release-note placement does not establish novelty. Development requires ThrowOnBadRequest=false. Malformed JSON returns before filters on both tested runtimes. |
| ShortCircuitAttribute/IShortCircuitMetadata | Delivered PIN-816 ([proof](../../verification/endpoint-policies.md)). Minimal API attribute/convention comparison with real before/after-routing traces, ordinary authorization/CORS and rejected conflicting metadata. The .NET 10 convention remains valid. |
| Union binding | PIN-820 delivers `web.accept-union-payloads/11.0`: original .NET 10 converter and .NET 11 native union/classifier; real Minimal API, MVC and SignalR JSON WebSocket requests check both shapes and invalid inputs. No route/query/header/form or MessagePack/Newtonsoft claim. |
| ICsrfProtection/CsrfProtectionResult and automatic middleware | PIN-808 retains standard form/startup/CORS/token guidance; **W02 / PIN-1172 delivered** `web.supply-origin-protection-policy/11.0` supplies an operator-owned exact-origin DI service. Actual old token+origin/current custom/default/token consumers and Chrome same-origin/site/cross-site/CORS controls qualify the bounded policy. Standard CORS-derived trust requires a specific allowed origin **and SupportsCredentials**; custom write trust is explicit and independent. JSON remains outside form metadata. No net10 API backport or actual-GA claim. |
| AuthorizationPolicy.CombineAsync metadata overload | Delivered PIN-809 ([proof](../../verification/authorization-and-clock.md)); common requirement/policy metadata, with legacy non-endpoint MVC excluded from the new behavior. |
| Identity TimeProvider | Delivered PIN-810 ([proof](../../verification/authorization-and-clock.md)): real UserManager threshold/end/expiry with controlled TimeProvider. Token/cookie paths are not inferred from lockout proof. |
| Aaguid on UserPasskeyInfo/IdentityPasskeyData; template authenticator names | PIN-811; persisted/display mapping and rename fallback. Automatic built-in names alone are excluded. |
| Passkey denial and JSON StringSyntax annotations | PIN-812 owns real lockout/confirmation outcomes. StringSyntax is tooling guidance, not a new sign-in operation. Registration confirmation is distinct: **W07 / PIN-1177 delivered** `identity.confirm-before-registration-signin/11.0`, actual paired UI/store/HTTP registration controls. |
| Identity UI Bootstrap4 MSBuild warning | **W08 / PIN-1178 delivered** `identity.migrate-ui-framework-selection/11.0`: actual warning, explicit5/default selection, original markup/CSS, published HTTP and Chromium login/validation. Automatic vendor updates alone remain excluded. |
| SignalR refresh server/client/features/metadata/UserIdentifier and TypeScript contract | PIN-813; shared goal across transports and Blazor. Public feature and hub members group under the same lifecycle. Actual npm availability/final-name gap remains open below. |
| Non-streaming hub cancellation | PIN-814; protocol/runtime behavior can change without a new method signature. Matched clients/server and actual cancellation remain required. |
| OpenAPI binary files, QUERY, multiple responses, SSE, 3.2 default, environment, Obsolete metadata, model migration, server URLs | PIN-821–829 retain individual scopes. UnauthorizedHttpResult's new IEndpointMetadataProvider groups with PIN-821/823 response metadata; remove redundant custom 401 metadata only after generated-document proof. |
| IAdditionalOpenApiDocumentNameResolver, AddOpenApiCore | **W04 / PIN-1174 delivered** `openapi.discover-dynamic-documents/11.0`; actual public/staff runtime and GetDocument build outputs, named options, exact duplicates, explicit/additional names and missing/empty/null controls. The .NET10 explicit-registration alternative remains valid; no backport or GA claim. AddOpenApiCore has no options-callback overload. |
| ApiDescription.RoutePattern | **W03**; authored API-explorer tools can consume parsed route constraints. Remaining ApiDescription nullable-context/accessor changes are not separate operations. |
| Kestrel handshake Exception and ClientHello listener; obsolete callback | PIN-830; registration order before UseHttps, failure snapshots and real handshake evidence. |
| TLS channel binding and HTTP.sys Legacy/Medium/Strict | PIN-831; retain actual Windows/HTTPS prerequisites. **ASPDEPR010 ITlsTokenBindingFeature is a different protocol**, owned by **W15**, not a mechanical CBT substitution. |
| HTTP.sys TryGetRequestProperty | **W05**; native property IDs/struct parsing, buffer negotiation and unsupported API behavior differ from the existing typed TLS feature. |
| Stricter H2/H3 headers and H1 framing | PIN-832. H1 still processes the request then closes; it is not universally an initial rejection. Servicing can bring the close behavior to the old baseline. Remove preview-only insecure-chunked switch; no replacement switch. |
| Absolute-form encoded slash preservation | **W09**; middleware/route expectations and removable decoding compensation, not a new transport API. |
| Native request traces/route tags/status descriptions | PIN-833. SuppressActivityOpenTelemetryData does not restore route substitution or StatusDescription. |
| IOutputCachePolicyProvider | PIN-834; dynamic base/named policy selection, rather than fixed registration. |
| Zstandard providers/decompression | PIN-835; explicit negotiation/decoding policy, not an automatic-default performance recipe. |
| Removed concurrency middleware, MVC compatibility types, ImageTagHelper constructor/properties, ForProperty overload, runtime compilation registrations | PIN-836–840 respectively. PIN-837–839 deliver the three MVC/Razor migrations below. Preserve source/binary versus behavioral distinctions; ASPDEPR003 is not runtime removal. **W16** handles F# compiled-view migration, which needs more than removing a registration. |
| ISpanDataProtector/ISpanAuthenticatedEncryptor | **W01 / PIN-1171 delivered** `security.protect-into-buffer/11.0`: actual framework10 array and framework11 span/capability fallback, isolated real keys, empty/token/1MiB recovery, purpose/key/tampering and advanced AAD rejection; full failed-output cleanup. Reference hashes match this audit; no allocation-count, older-target span or actual-GA claim. |
| StaticAssetDescriptor.Order | **W06 / PIN-1176 delivered** `web.order-static-asset-routes/11.0`; public descriptor serialization of an isolated SDK-generated manifest, real competing static endpoints, default/explicit/invalid/frozen checks and retained .NET10 conventions. Generated default changes do not each need a recipe. Actual GA remains open. |
| QUIC transport four buffer/stream properties lose RequiresPreviewFeatures | **W12 / PIN-1182 delivered** `hosting.configure-http3-budgets/11.0`; actual paired HTTP/3 concurrency, unidirectional credit and slow upload/download backpressure. Existing properties are not new APIs; .NET10 retains its narrow CA2252 acknowledgement. |
| JSON Patch ShouldSerializeFrom obsoletion; from/value JSON metadata | **W13 / PIN-1183 delivered** `json-patch.serialize-operation-intent/11.0`; actual CS0618, six-operation wire/HTTP controls and old operation-aware modifier. New from metadata omits null, not every non-null irrelevant source. Newtonsoft remains separate. |
| WebApplicationFactory.ConfigureWebApplicationBuilder | **W14 / PIN-1184 delivered** `testing.configure-web-application-builder/11.0`; separate real entry assembly, TestServer early/late config/service controls and async disposal. Legacy host paths reject the early hook. Only earlier .NET11 previews used ConfigureHostApplicationBuilder. |
| gRPC server/client/factory 2.64.0 and JSON transcoding | Selected public APIs have **zero differences**. No standalone transport recipe inferred. **W10** preserves the separately removed Grpc.Swagger integration, so the original blanket no-gRPC-change row is qualified. |
| DirectTls | **W11**, availability/contract unresolved; Linux/OpenSSL/experimental opt-in. Current source example cannot be copied against the available RC1 package. |
| dev-certs/user-jwts, MCP template, web SDK/project inputs | PIN-817 delivers `web.test-file-app-authentication/11.0` with actual project/file JWT selectors and signed/absent/invalid-token HTTP checks. PIN-819 delivers `web.scaffold-mcp-host/11.0` with fresh-home bundled generation and real HTTP MCP initialize/list/call. Both SDK 10.0.402 and 11 RC2 bundle the MCP template; no exclusive .NET 11 introduction is inferred. PIN-818 retains certificate trust and PIN-707 retains other build inputs. |
| Generic Host/background failures, reload/options/watcher changes | PIN-685's three proposals and PIN-706 own these; do not confuse IWebHost signatures/state-machine changes with proof of Generic Host failure behavior. |
| SqlClient Azure extension, EF-generated identity schema/project edits | PIN-842 and PIN-710; preserve actual identity/provider evidence, rather than duplicating SQL configuration analysis. |
| Blazor, Razor compiler, WASM, QuickGrid, Gateway, Hybrid entries in the complete sources | PIN-708/PIN-691 and their retained proposals, including the completed examples linked in those audits; Hybrid/native owners keep platform obligations. No non-Blazor duplicate. |
| Runtime-async conversion, metadata spelling fixes, UserOptions nullable context, trimming-justification spelling | Exact pairs or specific rules retain raw evidence; no new callable operation or blanket trimming compatibility is inferred. |
| HTTP/3 early processing, trailer timeout enforcement, rate-limiter refill/RetryAfter corrections, Vary default and ordinary performance/fixes | Automatic corrections excluded as standalone recipes. Existing custom workaround removal still needs goal-specific proof. Trailer policy uses the existing RequestHeadersTimeout; a default enforcement fix alone is not new authored guidance. Keep Retry-After propagation in the existing limiter migration where taught. |

### Obsolete and analyzer ownership

Both baseline manifests retain every obsolete/experimental declaration in the
selected assemblies, including historical warnings; the API delta detects newly
warned members separately. ASPDEPR's overview explicitly says it is incomplete
and lists only ASPDEPR002. The full 002 article describes WithOpenApi's earlier
deprecation; it is already present on .NET 10 and is not an additional .NET 11
goal. Existing PIN-828 transformer guidance retains supported customization.

New/different bounded warnings: **ASPDEPR003 → PIN-840**, **ASPDEPR010 → W15**,
ClientHello callback's default Obsolete warning **→ PIN-830**, and JSON Patch's
default Obsolete warning **→ W13**. Compiler-required-members/ref-struct
Obsolete(error) markers are compatibility encodings, not deprecation migrations.
DirectTls **ASPNETCORE_DIRECTTLS_001** is experimental, not obsolete. Historical
ASPDEPR001/other retained warnings do not authorize a full .NET 10 backfill.

The .NET 11 custom-ID table's SYSLIB0064 and SYSLIB0066 stay with cryptography
PIN-686 and Extensions PIN-685/PIN-706. The general index remains PIN-689's
cross-family mapping obligation. The ASP analyzer descriptor source is included;
this audit does not invent a new diagnostic from changed help-link text or claim
an analyzer executed. No unowned API delta or index item is silently excluded.

## Retained conflicts and availability findings

1. **DirectTls:** the cumulative include advertises DirectTlsEndpoint and endpoint
   selection. The available RC1 assembly exposes UseDirectTls and WorkerCount,
   with no DirectTlsEndpoint type. Its package embeds VMR
   `3551975be08744f0418857c5bed8ab1545c5dd47`. No RC2 package was listed on NuGet
   during review. **W11** must find a matching usable contract and run it on Linux;
   source review is not a supported RC2 sample.
2. **SignalR TypeScript:** the actual published 11 Preview7 ESM declarations do
   not expose the final RC authentication-refresh surface advertised by the
   cumulative/RC1 source. The reviewed declaration delta is only pkg-version.
   PIN-813/PIN-814 must obtain a matching published client or explicitly built,
   pinned first-party client before teaching or claiming those paths. .NET client
   refresh members exist in the actual RC2 artifacts. Client-side redirect support
   does not establish Azure SignalR Service support.
3. **HTTP.sys hardening:** the full breaking article says configuration failure
   logs and continues; the TLS include and pinned UrlGroup.SetChannelBindingProperty
   distinguish Strict with `throwOnError: true`. PIN-831 must exercise the actual
   selected Windows route; do not promise fail-open Strict or guaranteed support
   from a DLL. No Windows service configuration was changed here.
4. **DBSC:** cumulative documentation still advertises the experimental package,
   while canonical RC2 explicitly removes its implementation. Keep the original
   availability finding and PIN-692 ownership; experimental eligibility is not
   rejected. Its relation is context, not a native prerequisite of this audit.
   **25 October follow-up:** [PIN-692](dbsc-availability.md) now closes that pinned
   RC2 finding as unavailable using actual RC1 package/RC2 reference metadata and
   the official version index. The history above is retained; PIN-964 / GA_WEB
   independently rechecks actual final artifacts before stable publication.
5. **ConcurrencyLimiter:** the full note describes no new 11 package and also
   permits temporarily retaining an older package on net11. PIN-836 must verify
   the actual package/TFM rather than interpret absent 11.x as a universal restore
   ban for older binaries.
6. **Servicing and preview migrations:** encoded slash/framing, identity policy
   or other fixes can appear in the serviced .NET 10 baseline too. Proposals must
   report measured comparisons honestly. Preview-only symbol/switch removal is
   not a .NET 10 API removal. GA reconciliation remains open for every proposal.

## Recipe drafts for review

These are **sixteen proposals**, not created Linear tickets, final milestone
numbers or permission to implement. W labels are stable audit-local IDs. Native
estimates use the verified team convention **S (2), M (3), L (5)**, including
authoring, integration proof and installed-content verification. PIN-986 owns
consolidation/approval and exact milestone/title allocation; no related issue was
renamed, re-estimated, moved or created by this audit.

All drafts share these acceptance requirements:

1. Author original everyday-language story, reusable title/goal, complete offline
   prose/metadata and canonical old/new samples in a fictional domain. Preserve
   introduced release, actual applicability, opt-ins and experimental labels;
   do not present exact tested pins as minimum requirements.
2. Start with the exact SDK/framework baseline above. Pin each additional
   package/tool/native dependency and isolated sample boundary; any earlier-target
   package route needs its own actual compile/runtime proof. Build real .NET 10
   and 11 consumers, retaining a valid previous approach or honestly documenting
   absence. Compare installed artifacts, not mocks of changed framework APIs.
3. Run each draft's real behavioral checks below, including the relevant negative
   control. Record commands, input hashes, package/compiler versions, platform,
   results and limitations. No source-only, compilation-only or mock result may
   stand in for required requests, UI, cryptographic or native execution.
4. Pack/install the ordinary Playbook tool once with the existing isolated-feed
   harness; from an unrelated directory perform real stdio discovery/get for
   title/goal, full Markdown, applicability, edition, evidence and unsupported
   target behavior, offline. Update the coverage register. Missing assets,
   environment or required proof blocks that delivery PR and publication.

### W01. Protect data directly into an owned buffer — M (3)

Story: A service author wants to encrypt a museum's short-lived collection token
into an existing buffer so that ownership stays clear without extra array plumbing.
Proposed `security.protect-into-buffer`. Prerequisites: framework DataProtection
and the two actual span interfaces; use its real key ring in an isolated directory.
Check actual IDataProtector capability discovery and fallback, round trip,
empty/large payloads, wrong purpose/key and tampering. For the advanced encryptor
path, verify matching additional authenticated data and failed authentication;
never promise a usable partial plaintext after failure or a universal allocation
count from interface shape. M covers one shared buffer-ownership goal; custom
cryptographic algorithm design is excluded.

### W02. Supply an application-specific cross-origin verdict — L (5)

Delivered by **PIN-1172** as `web.supply-origin-protection-policy/11.0`.
Original SeedExchange consumers compile/run on the actual SDK10.0.402/framework
10.0.13 and SDK11 RC2/framework11 RC2 sets. Http.Abstractions/default-builder
reference hashes match this audit. The custom contract is in the shared framework;
the non-Razor samples restore no NuGet libraries and need no direct Internal.Assets
package. The older consumer retains token plus explicit origin enforcement and
confirms ICsrfProtection is absent; no backport is inferred. Real DI/handler counts,
missing/opaque Origin and JSON/token checks are captured by
`eng/Record-SeedExchange.ps1`. Actual Chrome156 loopback form/fetch observations
are retained in [the existing form evidence](../../verification/streaming-and-form-protection.md#application-owned-origin-policy).
Installed local-feed stdio inspection qualifies complete offline delivery, not
additional browser/deployment/GA execution. L5 remains justified by security policy,
real browser controls and installed guidance. PIN-959/PIN-964 and existing Human
gates stay open; the exact eight approved .NET10 foundations remain unchanged.

Story: A portal author wants a carefully defined origin rule for a botanical
exchange so that legitimate browser submissions work while hostile sites fail.
Proposed `web.supply-origin-protection-policy`. Prerequisites: RC2 ICsrfProtection,
automatic antiforgery metadata, actual browser/loopback hosts; PIN-808's standard
policy is the comparison, not a native prerequisite merely because files overlap.
Author the smallest custom provider with an explicit trust source. Prove actual
DI invocation and same-origin, same-site, cross-site, missing-header, wildcard
and credentialed/noncredentialed CORS cases; keep token-required clients and JSON
endpoints separate. No allow-all custom provider, forged-header unit test alone,
or replacement of authorization counts as completion. L covers security policy,
real browser controls and installed guidance.

### W03. Read parsed route contracts in an API explorer extension — M (3)

Delivered by **PIN-1173** as `web.inspect-api-route-pattern/11.0`. Original
MarineRoutes consumers use actual MVC and Minimal API discovery, invoke the
extension on eight discovered routes and compare parsed constraints, defaults,
optional/catch-all flags with 18 real HTTP matching/nonmatching requests per
target. A handcrafted description is only a nullable/legacy control. The actual
.NET10 artifact lacks the property; its valid comparison parses MVC's source
attribute template and the demonstrated Minimal provider's display text. Parsing
normalized MVC display text loses constraints. .NET11 consumes the nullable
property directly and never silently reparses a null description's display path.

SDK10.0.402/framework10.0.13 and SDK11.0.100-rc.2.26504.105/framework11 RC2
compile and run independently. Saved records retain Roslyn and loaded runtime
identities and assert Mvc.Abstractions/Mvc.ApiExplorer/Routing reference hashes
against this audit. Both consumers resolve no NuGet libraries or native assets.
Only the built-in attribute-routed MVC and described Minimal providers are
qualified; other providers can leave RoutePattern null. Installed ordinary-tool
inspection verifies complete canonical offline guidance and unsupported-target
behavior. M3 retains real provider/routing integration and the older workaround.
The exact eight .NET10 foundations, stable identities, PIN-959/PIN-964 and
actual-GA/Human gates remain unchanged.

Story: A tool author wants to understand a marine archive's routes without
re-parsing displayed URLs so that optional values and constraints stay accurate.
Proposed `web.inspect-api-route-pattern`. Prerequisites: MVC/Minimal API explorer
and actual ApiDescription.RoutePattern. Build an original explorer extension;
inspect discovered constrained, optional and catch-all routes plus a null/legacy
description. Compare parsed parameters/defaults with real matching requests and
retain nullable handling. Do not assume every provider populates RoutePattern
or use a handwritten ApiDescription as the only proof. M covers integration and
the old parsing workaround.

### W04. Generate contracts for dynamically named API documents — M (3)

Delivered by **PIN-1174** as `openapi.discover-dynamic-documents/11.0`.
Original PlanetariumContracts managed server consumers compile/run with SDK10.0.402,
framework10.0.13/C#14 and SDK11 RC2/framework11 RC2/C#15. Separate locked OpenApi
10.0.13/ApiDescription.Server10.0.0/Microsoft.OpenApi2.12.2 and matched 11 RC2/
Microsoft.OpenApi3.10.2 closures retain actual selected assets and ZIP hashes.
The older tool package differs from this audit's metadata-only 10.0.13 selection;
it is qualified by actual execution, not an assertion of identical bytes. Both
consumers generate real public/staff documents with distinct paths and options.
The newer real provider/tool also checks exact duplicates, mixed explicit/additional
names, missing/empty names and null rejection. Ordinal provider distinctness differs
from case-insensitive HTTP lookup; lowercase safe names avoid filename collisions.
`eng/Test-Planetarium.ps1` captures fresh compilation and execution records; installed
ordinary-tool stdio checks qualify complete offline delivery. M3 retains actual
host/tool enumeration, paired samples, requirements and runtime/build proof. No
client/browser/native matrix or universal provider contract is added. The exact
eight foundations, PIN-959/PIN-963/PIN-964, actual-GA and unrelated Human gates stay open.

Story: A service author wants separate public and staff descriptions for a
planetarium service so that build output includes the documents actually offered.
Proposed `openapi.discover-dynamic-documents`. Prerequisites: matching OpenApi and
ApiDescription.Server packages, IAdditionalOpenApiDocumentNameResolver and
AddOpenApiCore. The pinned provider concatenates registered and additional names
and applies Distinct; the actual GetDocument tool enumerates that provider.
Generate real build-time and runtime documents with two names,
options and distinct endpoints; verify missing/duplicate-name and explicitly
registered-name behavior. Compare PIN-826's environment selection without cloning
it. A resolver returning strings alone is insufficient; no nonexistent
AddOpenApiCore(options) overload. M covers actual host/tool enumeration.

### W05. Query an HTTP.sys request property safely — L (5)

Story: A Windows service author wants to inspect a supported native request
property so that diagnostics use the actual request instead of guessed values.
Proposed `hosting.read-native-request-property`. Prerequisites: actual Windows
HTTP.sys, matching Windows HTTP API/SDK struct definitions and an isolated HTTPS
URL/certificate binding authorized for testing. Choose one documented property
and exact qualifier/struct layout, then exercise empty/small/exact buffers,
bytesReturned, unsupported API/property and native errors over a real request.
No success claim from an interface implementation stub. Preserve existing service
bindings and clean only test resources; owner-controlled configuration is a
specific prerequisite if unavailable. L includes native integration uncertainty;
no recurring platform matrix is proposed.

### W06. Give a generated static endpoint deliberate route precedence — M (3)

Delivered by **PIN-1176** as `web.order-static-asset-routes/11.0`. Original
AmberGallery projects build with the actual SDK10.0.402/framework10.0.13/C#14
and SDK11.0.100-rc.2.26504.105/framework11 RC2/C#15 sets. The StaticAssets
reference hashes match this audit; saved evidence separately records Roslyn,
loaded frameworks, native host/CoreCLR/fxr and SDK static-web-assets tasks/targets.
No NuGet libraries, package-native assets or workloads are required.

The supported consumer boundary is public `MapStaticAssets` with a full manifest
path, public descriptor serialization and public endpoint conventions. Both
fixtures acquire the ordinary Web SDK's generated version1 Build manifest and
project two actual uncompressed asset descriptors into an isolated copy. They
preserve physical files and generated metadata and never edit SDK-owned output.
The .NET11 copy sets string Order before mapping; the older public convention
remains valid. This establishes the bounded consumer, not a general promise for
arbitrarily authored manifests or a production build/publish integration.

Real Kestrel GET/HEAD checks compare full physical response content. Current
controls cover absent/null/empty -100, explicit wins/yields, invalid text/overflow
silently falling back, numeric JSON parsing failure and equal-order ambiguity.
Actual metadata rejects frozen Order writes; older metadata rejects Route writes
and ignores unknown Order JSON. Input JSON/hashes and unchanged SDK output are
checked. Installed ordinary-artifact inspection and reconstruction from its full
Markdown cover both actual consumers without remote NuGet feeds. M3 retains
boundary qualification, paired manifest/HTTP controls and offline reconstruction.
The eight approved .NET10 foundations, stable identities, PIN-959/PIN-963/PIN-964
and unrelated Human gates remain unchanged. Windows x64 RC2 is not actual-GA proof.

Story: A site author wants a local exhibit asset to win or yield intentionally
when routes overlap so that the served resource is predictable.
Proposed `web.order-static-asset-routes`. Prerequisites: actual StaticAssets
manifest schema/descriptor Order and framework endpoint factory. Create an
original isolated manifest with two competing real endpoints; verify null/empty
default **-100**, explicit order, invalid value and immutable/frozen descriptor
behavior. Inspect generated JSON and actual HTTP response, then reconstruct the
documented consumer from served guidance. Do not expose internal factory APIs
or claim every normal application should hand-edit generated files. M includes
establishing the supported authoring boundary; retain a factual gap if no such
consumer can be established rather than inventing a recipe.

### W07. Keep registration aligned with confirmation policy — M (3)

Delivered by **PIN-1177** as `identity.confirm-before-registration-signin/11.0`.
Original AquariumVolunteers projects compile and execute actual Identity.UI,
Identity.EntityFrameworkCore and EF SQLite10.0.13 versus matching 11 RC2 packages,
with SDK10.0.402/C#14 and SDK11 RC2/C#15. UI/EF package ZIPs and Identity reference
hashes match this audit; full locked selected assets, compiler/runtime/native-host
and loaded SQLite identities are recorded through `eng/Record-AquariumVolunteers.ps1`.
Old SQLitePCLRaw2.1.13 loads SQLite3.53.3; current SQLitePCLRaw3.0.5 plus SQLite3.53.4
is a separately qualified compatible store closure, not a metadata-only assumption.

Real Razor Pages HTTP forms/antiforgery, fresh SQLite queries, redirects,
application versus temporary external cookies and a fresh authenticated request
cover both password and external registration under each individual email/phone/
account requirement and an isolated allowed control. The actual serviced .NET10
default UI still bypasses email-only/phone-only confirmation during registration;
later sign-in denies those users. Original custom pages gate both paths through
CanSignInAsync and remain valid on both targets. .NET11 packaged UI already applies
that gate. Four complete immutable UI implementations and the official registration
change note qualify the source interpretation; the current UI is a package, not
inferred from framework references. No compatibility switch restores the bypass.

Controlled local provider challenge/callback and Identity external registration
are real; third-party OAuth/OIDC interoperability is not claimed. Captured email
links and actual phone-token checks establish confirmation without messages.
Confirmed later password and external sign-ins succeed; phone-only policy remains
denied after email confirmation alone. Missing antiforgery/external state, tampered
provider state and original-page hostile returns grant no user/session. Default
IUserConfirmation uses confirmed email; custom confirmation services remain separate.
Full installed canonical guidance and applicability/evidence are checked offline
from the ordinary local-feed artifact. M3 retains both registration paths and real
UI/store/confirmation controls. PIN-812 remains separate; exactly eight foundations,
stable identities, PIN-959/PIN-963/PIN-964, actual-GA and Human gates remain unchanged.

Story: An account-interface author wants new aquarium volunteers to complete the
required confirmation step before receiving a session so that registration does
not bypass the sign-in rules.
Proposed `identity.confirm-before-registration-signin`. Prerequisites: actual
Identity UI/store and original password/external registration routes. Exercise
RequireConfirmedEmail/PhoneNumber/Account individually and an allowed control;
inspect redirect, stored user and cookie/session issuance. Test the real local
external-registration pipeline; label a controlled local provider separately
from third-party interoperability. Preserve checks instead of recommending global
disablement. PIN-812's passkey denial remains separate. M includes both registration
paths; source says there is no compatibility switch for the former bypass.

### W08. Move customized Identity UI off Bootstrap 4 — M (3)

Delivered by **PIN-1178**, retaining M3 for actual build/publish/browser proof.
Original VolunteerAccess compares matching Identity UI/store/SQLite10.0.13 and
11 RC2. Actual targets warn for Bootstrap4. Explicit Bootstrap5 and a separate
complete copy with the property removed both publish and render; an empty global
MSBuild property is not removal. Form/checkbox/button markup and authored CSS
change alongside selection. Published HTTP verifies local CSS, validation,
antiforgery, confirmation denial/allowed session and local return URLs. Chromium152
renders login/validation for all three publishes at1280x800 with loaded local CSS,
labels, alignment, readable errors and no clipping/overflow. Retained
[screenshots](../../verification/identity-ui/) and sample browser observations
support that bounded rendering claim, not a client-JS/all-accessibility matrix.

Story: A site author wants its customized volunteer sign-in pages to keep rendering
with a supported UI framework so that an upgrade does not leave stale build setup.
Proposed `identity.migrate-ui-framework-selection`. Prerequisites: Identity.UI
package **11.0.0-rc.2.26504.105**, the Bootstrap4 warning in its actual targets,
and original custom view/CSS assets. Reproduce the warning, choose Bootstrap5 or
remove IdentityUIFrameworkVersion, build/publish and render actual login/validation
pages with working static assets and layout. Compare valid .NET 10 customization;
do not equate changing a property with migrating all bespoke Bootstrap markup.
M includes build and browser verification; no standalone Bootstrap catalogue.

### W09. Preserve encoded route segments in proxy-style requests — M (3)

Delivered by **PIN-1179**, retaining M3 for actual paired raw-wire requests.
Original BirdRoutes executes origin/absolute targets on real loopback Kestrel.
Serviced10.0.13 still decodes an absolute `%2F`; RC2 preserves it on both forms.
The measured older ASCII-only restoration remains separate from the whole-path
decode failure control, which changes handlers on both versions. Ordinary slash,
case, double encoding, malformed escapes, NUL/CRLF and invalid request-line
controls retain their actual parser/middleware/handler outcomes. Accepted control
values are diagnostic observations, not business validation or authorization.
No intermediary, IIS, other OS or universal serviced-old claim is inferred.

Story: A service author wants a bird-record identifier containing an encoded slash
to keep its intended routing meaning so that a proxy request cannot select a
different handler from the equivalent direct request.
Proposed `routing.preserve-encoded-segment`. Prerequisites: real loopback Kestrel
and a client that preserves absolute-form request bytes. Compare origin-form
and absolute-form `%2F`, ordinary slash, case and malformed controls; inspect
RawTarget, Path, selected route and authored middleware. Remove only demonstrated
decoding compensation. Record serviced-old behavior honestly and do not turn
decoding into an authorization bypass. M covers wire-level request proof.

### W10. Replace the retired gRPC Swagger integration — L (5)

Delivered by **PIN-1180**, retaining L5 for real tool/package compatibility.
The actual historical net10.0 Swagger0.10.11/JsonTranscoding10.0.13/Swashbuckle6.6.2
service remains working. No matching Swagger11RC2/drop-in replacement is claimed.
Before implementing the replacement, pinned protoc26.1 from Grpc.Tools2.64.0
and the independently checked Windows x64 protoc-gen-openapiv2v2.31.0 release
generated nonempty operations, response schema and protobuf comments from the
original TideSurvey proto using matching RC2 imports. The new ASP.NET service
keeps JsonTranscoding11RC2 and serves that Swagger2.0 build artifact; no Go server
or generic AddOpenApi coverage is inferred. Both actual services execute gRPC,
JSON, unknown-station/wrong-method/path controls and compare the actual response
against the resolved generated schema and comments. Only the bounded primitive
unary GET mapping is qualified, not all annotations/options or generated clients.

Story: An API author wants clients to keep discovering a tide service's JSON
interface after an old documentation adapter is retired.
Proposed `grpc.replace-swagger-adapter`. Prerequisites: the actual previous
Grpc.Swagger **0.10.11**, matching old JsonTranscoding/Swashbuckle dependencies,
new JsonTranscoding RC2, actual Grpc **2.64.0**, protobuf tooling and an available
supported document-generation route established before implementation. Build
original old/new services; perform a real gRPC call and transcoded JSON request,
then inspect correct operations, schemas and protobuf comments in the generated
document. Removal alone or an empty document is not delivery; do not invent a
drop-in AddGrpcSwagger replacement. Retaining an older adapter is only a separate
route if its actual dependencies/TFM work. L retains real compatibility uncertainty.

### W11. Select an available direct TLS transport — L (5), availability blocked

Story: A Linux server author wants to choose a direct TLS transport deliberately
so that supported endpoints use the intended connection implementation.
Proposed `hosting.select-direct-tls`. Prerequisites: **a matching usable package
contract first**, Linux/OpenSSL and exact runtime native TLS support. Current
RC1 artifact **11.0.0-rc.1.26425.128** has only UseDirectTls/WorkerCount; current
DirectTlsEndpoint documentation does not compile against that public surface.
Resolve that mismatch before authoring a sample. Then build original old/new
hosts, label ASPNETCORE_DIRECTTLS_001, perform actual HTTPS success/failure and
default-socket versus explicitly selected transport controls. No Windows-only
compile or source mock proves Linux TLS. L includes native qualification;
availability stays open if no supported route exists, rather than teaching a
temporary preview contract as GA.

### W12. Configure HTTP/3 connection budgets without preview scaffolding — M (3)

Delivered by PIN-1182 with original EstuaryBudgets .NET10/11 consumers. Both actual
Windows x64 runtime sets exercise two concurrent HTTP/3 requests, a queued third
on the same connection, inbound unidirectional stream credit, and slow 16MiB
upload/download pause/resume. Loaded MsQuic2.5.11.159415436 and its hash are
recorded separately from audited framework references. Current builds need no
preview opt-in; older assignments retain a local CA2252 acknowledgement. These
are transport thresholds, not exact process-memory bounds or application quotas.
No new OS/client/native matrix, ninth foundation or actual-GA claim is added.

Story: A server author wants deliberate limits for an observation stream so that
connection resources match the application's workload.
Proposed `hosting.configure-http3-budgets`. Prerequisites: actual Kestrel QUIC
transport, supported OS/native MsQuic and HTTP/3 client. The four existing stream
count/read/write buffer properties lose RequiresPreviewFeatures in RC2. Build
without the former broad preview opt-in and run bounded real concurrent HTTP/3
requests/slow consumers to verify the chosen contract and configured limits.
Keep transport settings separate from application-level request quotas and
verify old valid behavior; removing a suppression alone is insufficient. M covers
one portable native-capable authoring lane, not a full OS matrix.

### W13. Preserve JSON Patch operation serialization intent — M (3)

Delivered by PIN-1183 with original GlasshousePatch .NET10/11 consumers. Exact
SystemTextJson ZIPs/assets match this audit; actual default CS0618, metadata,
wire bytes, all six operations and HTTP failure controls are retained. The older
operation-aware modifier remains valid. New from metadata omits null; a spurious
non-null from still serializes, so stronger custom policy must be retained.
Both measured readers accept missing replace value as null; strict presence,
authorization and path/complexity policy remain application responsibilities.
No change to the separate Newtonsoft implementation is asserted.

Story: A service author wants changed fields in a garden record to serialize
correctly so that updates neither omit required values nor add invalid fields.
Proposed `json-patch.serialize-operation-intent`. Prerequisites: matching
Microsoft.AspNetCore.JsonPatch.SystemTextJson packages and real formatter/serializer.
Reproduce ShouldSerializeFrom's new default Obsolete warning; replace direct
calls with OperationType.Move/Copy checks. Serialize/deserialize/apply original
add/remove/replace/move/copy/test operations, absent versus null values and invalid
paths over real HTTP. Inspect from/value metadata changes and actual wire bytes,
not only reflected attributes. Keep the unchanged Newtonsoft path distinct;
document patch authorization/allowed paths without inventing new framework
security guarantees. M includes round-trip and invalid-operation proof.

### W14. Configure a web integration test through the application builder — M (3)

Delivered by PIN-1184 with original StrataTests/StrataApi project pairs. Actual
Mvc.Testing/TestHost archives and compile assets match this audit; the matching
DependencyModel closure is additionally qualified by real entry/deps discovery
and TestServer startup. HTTP requests distinguish early startup capture from late
configuration, retain the production control and verify awaited async singleton
disposal. Current explicit IHostBuilder/IWebHostBuilder paths reject the early
hook. The .NET10 late setup remains valid; no preview-package execution/backport,
physical network transport or actual-GA guarantee is inferred.

Story: A service tester wants an isolated test configuration for a geology API
so that a test can exercise its real startup without changing production settings.
Proposed `testing.configure-web-application-builder`. Prerequisites:
Mvc.Testing/TestHost RC2 and a real entry assembly. Override the new protected
ConfigureWebApplicationBuilder(IHostApplicationBuilder), register one observable
test service/configuration source, send actual TestServer HTTP requests and prove
the production control is unchanged. Check configuration order, async disposal
and the advertised hosting entry paths. .NET 10 uses its valid existing setup;
only a .NET 11 preview fixture needs a rename from ConfigureHostApplicationBuilder.
M includes actual startup/lifecycle integration, not a mocked builder assertion.

### W15. Remove reliance on obsolete browser token binding — S (2)

Story: An authentication maintainer wants to remove a browser mechanism that was
not adopted so that its sign-in logic does not depend on an unavailable feature.
Proposed `authentication.remove-token-binding-assumption`. Prerequisites: real
Http.Features **11 RC2**, ASPDEPR010 and original old/new middleware. Reproduce
the warning; remove ITlsTokenBindingFeature dependence while preserving the
application's actual authentication decision and HTTPS handling. Exercise real
authenticated/denied requests and missing-feature handling. TLS Token Binding
RFC 8471 and RFC 5929 channel binding are different; PIN-831 is not an automatic
replacement. If the original security goal requires another mechanism, establish
its supported route explicitly before publishing. S covers bounded removal with
request proof; new protocol design is excluded.

### W16. Publish F# MVC views without runtime compilation — M (3)

Story: An F# site author wants its observatory pages to render after publishing
so that removing obsolete development setup does not leave missing views.
Proposed `mvc.compile-fsharp-views`. Prerequisites: actual F# SDK 10/11 MVC
templates, Razor build tooling and runtime-compilation comparison package.
Author original .fsproj/.fs/.cshtml files with UseRazorSourceGenerator=false,
RazorCompileOnBuild/Publish and ordered compiled-view registration. Build and
publish, then run from a clean publish directory without source views/runtime
compilation package and request the real page; include the build-success but
view-missing negative control. Verify .Views assembly discovery/load context.
PIN-840 retains ordinary registration removal; it cannot substitute for this
F#-specific artifact path. M includes actual published consumption.

## Fen contract delivery: PIN-827–PIN-829

The stable goals `openapi.retire-contract-member`,
`openapi.migrate-transformer-model` and `openapi.retain-server-url-contract` now
have original 11.0 editions. The matching source-disposition rows in the
[web inventory](../inventory/02-web-data.md) retain their identities. The
`samples/net10.0/FenContracts` and `samples/net11.0/FenContracts` projects compile
and execute against SDK 10.0.402 / 11.0.100-rc.2.26504.105, ASP.NET OpenApi
10.0.13 / 11.0.0-rc.2.26504.105, and transitive Microsoft.OpenApi 2.12.2 / 3.10.2.
Their saved compilation records contain the locked graphs and actual runtime
output; these are managed Windows x64 checks, not GA or native-platform proof.

The source/artifact reconciliation matters: the official 3.x compatibility
article's illustrative Any/Models code does not describe the actual 2.12.2 API.
That baseline already has interface schemas, reference holders and
JsonNodeExtension. The original comparison retains those valid patterns and
changes the explicit response-reference metadata type from
OpenApiReferenceWithDescription to OpenApiReferenceWithDescriptionAndSummary,
plus the response content dictionary from OpenApiMediaType to IOpenApiMediaType.
The package repository commits are
[`a24aac5`](https://github.com/microsoft/OpenAPI.NET/blob/a24aac560adbf013ff5aa63a873e5a7babce38d2/src/Microsoft.OpenApi/Models/References/OpenApiResponseReference.cs)
and
[`c3e9fad`](https://github.com/microsoft/OpenAPI.NET/blob/c3e9fad2fb3191ca0cb96aa4265c65df38ebad67/src/Microsoft.OpenApi/Models/References/OpenApiResponseReference.cs).

Real loopback documents verify retiring operations/types/properties, current
controls, custom document/operation/schema transformers, response references and
description overrides, and JSON object/scalar extensions. The raw .NET 10 control
lacks deprecation flags; its custom transformers match .NET 11's built-in result.
Empty PathBase changes the root slash; `/fen` remains unchanged. Exact URL
fixtures and real composed HTTP requests pass on both versions, including the
optional root-only compatibility transform. No universal client failure or
third-party generated-client compatibility is inferred.

The `FenContracts` scenario reuses the actual packed/installed stdio harness with
isolated feed/cache and unrelated working directory. It checks canonical body
equality, discovery goal, full applicability/sources/evidence, JSON/text equality
and target edition. Local proxy blocking is separate from the existing
network-disabled CI lane. Full offline inspection commands remain in the
[Inspector walkthrough](../../inspect-guidance.md); the recipe bodies contain all
canonical files. No new OS, browser, provider or native matrix is needed.

## Reproduce and inspect

PIN-814 now delivers `signalr.cancel-active-operation/11.0` with original
.NET 10/11 consumers and actual .NET client/Kestrel cancellation checks, including
both mixed-version controls. This is a behavioral change in existing invocation
APIs, so the public-signature diff alone cannot establish it. The TypeScript
inventory does not imply a unary cancellation API: this guidance is explicitly
.NET-client-only. Authentication-refresh TypeScript findings remain with PIN-813.
See [the exact artifacts and offline retrieval proof](../../verification/invocation-cancellation.md).

Prerequisites: PowerShell 7 on the recorded Windows lane, the two exact SDK/root
layouts above, and network access to public NuGet/npm/raw GitHub. No installation,
trust operation, native server setup or application build is required for this
metadata audit. Optional Net10Root/Net11Root parameters select equivalent existing
installations; the pinned versions remain checked by artifact equality.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-0ffcb848'
& ./eng/Inspect-WebApis.ps1 `
    -OutputPath ./artifacts/web.replayed.json `
    -CompareWith ./docs/planning/audits/web.assets.json
$audit = Get-Content ./artifacts/web.replayed.json -Raw | ConvertFrom-Json
$audit.baselines | Select-Object sdk, referencePack, target
$audit.dispositions | Group-Object group | Select-Object Name, Count
```

Expect exact artifact/tooling/template/API equality, 212 verified immutable source
hashes and no unowned difference. Missing roots, archives, hashes, ownership or
changed bytes must fail. The generated report can be removed afterward.
No runtime/recipe/MCP behavior is
established by this audit; every delivery proposal keeps those acceptance gates.

## Lantern transport delivery (PIN-830, PIN-832, PIN-833)

The inventory now marks three original 11.0 editions delivered: TLS handshake
inspection, compliant outbound requests, and native request traces. Separate
.NET 10.0.13 / SDK 10.0.402 and .NET 11 RC2 / SDK
11.0.100-rc.2.26504.105 projects retain canonical files, locked dependencies,
compilation/execution records and input hashes. Final GA reconciliation remains
open under ADR 0004. No package publication or resource provisioning occurred.

`LanternTls` observes both real ClientHellos, completes HTTPS and rejects an
incompatible TLS client. Only the newer connection middleware reads the retained
`ITlsHandshakeFeature.Exception`; the .NET 10 callback cannot expose that property.
Certificates remain local to the fixture and are not added to shared trust.

`LanternForwarding` sends raw HPACK HTTP/2 and QPACK HTTP/3 requests, preventing
high-level client normalization from disguising malformed headers. RC2 rejects
six malformed cases per protocol and accepts their corrected controls. The old
runtime tolerates four connection-specific fields per protocol; Connection and
invalid TE already fail. Actual handler counts distinguish server rejection from
accidental acceptance. The HTTP/1.1 dual-framing request is processed then closed
on both measured runtimes (the change was serviced); corrected framing permits
connection reuse. HTTP/3 ran on Windows x64 with msquic.dll 2.5.11.159415436.
HTTP/2 protocol failures can surface as RST_STREAM, GOAWAY or a transport abort.

`LanternTraces` exports real MVC success/error requests through OpenTelemetry
1.20.1 with one application processor and one exporter. The .NET 10 comparison
adds AspNetCore instrumentation 1.20.1; the native .NET 11 HTTP-only pipeline omits
that dependency. Source `Microsoft.AspNetCore`, exactly one server span per
request, business enrichment, resolved conventional routes, status and HTTP tags
are checked. This does not claim remote collector or metrics execution.

Two current-documentation qualifications are explicit in the served recipe:
RC2's native error activity carries error.type but no exception event here;
a fresh-process suppression control removes method/status/route tags and leaves
the error activity Unset, while still exporting the native spans and business
tags with a null description. The switch does not recreate the old route tag or
exception description. The older OTel instrumentation already leaves descriptions
null. These are actual artifact results, not a claim to reproduce every older
uninstrumented framework behavior. Recheck these points at GA rather than
relabeling RC2 records. The official references are the
[TLS release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11#tls-handshake-observability-in-kestrel),
[protocol change](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/kestrel-strict-protocol-compliance),
and [HTTP tracing change](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/http-activity-otel-semconv).

The `LanternTransport` scenario reuses the existing actual-package isolated
feed/cache and unrelated-directory stdio lifecycle. It checks full canonical
Markdown, ID/title/goal, all applicability/source metadata, evidence freshness,
structured/text equality and target editions. No new OS/client/native test matrix
is added. The grouped WEB-085 tracing portion is complete; its independent
channel-binding work remains with PIN-831 and was not changed here.

## Delivered MVC and Razor migrations: PIN-837–839

The original QuayArchive, FernPosters and BeaconLabels .NET 10/11 pairs now back
three stable goal identities: `mvc.remove-compatibility-shims`,
`razor.update-image-helper-extension` and `mvc.update-property-metadata-identity`,
each edition 11.0. These are migrations, not extra .NET 10 foundations.

Actual SDK 10.0.402 / runtime 10.0.13 and SDK/runtime 11 RC2
11.0.100-rc.2.26504.105 / 11.0.0-rc.2.26504.105 compile and execute the consumers.
Saved evidence beside each project contains the actual commands, output and input
hashes. Retargeting the historical sources to .NET 11 reproduces removed-API
compiler errors; the migrated sources build and retain their intended outcomes.
The local obsolete suppressions exist only in the .NET 10 migration inputs.

QuayArchive starts Kestrel and checks a real MVC controller request after removing
the compatibility registration. FernPosters uses real Razor activation of a
derived ImageTagHelper and checks resolved/versioned image markup, encoded custom
attributes and retained alt text. BeaconLabels executes a custom metadata provider
and checks model type/container, display label and getter/setter; the new identity
also retains the reflected PropertyInfo. The supported helper constructor and
property-identity overload already worked in .NET 10; no novelty is invented.
Recompile dependent binaries rather than assuming host recompilation fixes them.

The `MvcMigration` scenario reuses the installed-package feed/cache and unrelated
working-directory harness. It compares complete canonical Markdown, discovery
ID/title/goal, full applicability and source metadata, current execution evidence,
structured/text equality and target editions. No new platform/client matrix,
resource provisioning or package publication is added. These managed Windows x64
checks establish the named consumer behavior, not browser rendering, arbitrary
third-party binary compatibility, trimming/NativeAOT or GA verification.

## Owned-buffer delivery: W01 / PIN-1171

The original M (3) scope remains one shared buffer-ownership goal, not algorithm
design. The museum examples compile and execute on SDK10.0.402/framework10.0.13
and SDK11.0.100-rc.2.26504.105/framework11 RC2, with original canonical source
and fresh sample records. Both DataProtection reference assemblies match the
original asset hashes; runtime identities are recorded separately. Actual
IDataProtector discovery and a capability-hiding wrapper exercise span and real
array fallback. Checks cover token/empty/1MiB, purpose/key/tampering, full-buffer
cleanup and real framework-key encryptor AAD success/failures. Only Windows x64
CNG execution is qualified; no custom algorithm, package backport, allocation
count, other backend/platform or actual-GA claim. Installed ordinary-artifact
list/get inspection uses the existing parameterized Inspector harness. The eight
.NET10 foundations, retained identities, Human gates, PIN-959 and PIN-964 remain
unchanged. This bounded delivery does not change the other outstanding drafts.
