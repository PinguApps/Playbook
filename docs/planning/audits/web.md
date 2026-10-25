# Non-Blazor web authoring coverage: PIN-709

Reviewed **24 October 2026**. This is a bounded artifact/source audit, not recipe
delivery. Every proposal below is **delivery outstanding** and awaits Pingu's
decisions through [PIN-986](https://linear.app/pinguapps/issue/PIN-986).
Closing this audit does not clear comprehensive coverage, compilation, installed
content or GA publication gates. No served recipe or MCP contract changes here.

## Boundary and reproducible evidence

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
| Union binding | PIN-820 retains JSON-only body binding; no route/query/header/form claim. |
| ICsrfProtection/CsrfProtectionResult and automatic middleware | PIN-808 owns form/startup/CORS/token exceptions; **W02** owns custom service authoring. Source requires a specific allowed origin **and SupportsCredentials** for CORS-derived CSRF trust; AllowAnyOrigin is not trust. JSON endpoints remain unchanged. |
| AuthorizationPolicy.CombineAsync metadata overload | Delivered PIN-809 ([proof](../../verification/authorization-and-clock.md)); common requirement/policy metadata, with legacy non-endpoint MVC excluded from the new behavior. |
| Identity TimeProvider | Delivered PIN-810 ([proof](../../verification/authorization-and-clock.md)): real UserManager threshold/end/expiry with controlled TimeProvider. Token/cookie paths are not inferred from lockout proof. |
| Aaguid on UserPasskeyInfo/IdentityPasskeyData; template authenticator names | PIN-811; persisted/display mapping and rename fallback. Automatic built-in names alone are excluded. |
| Passkey denial and JSON StringSyntax annotations | PIN-812 owns real lockout/confirmation outcomes. StringSyntax is tooling guidance, not a new sign-in operation. Registration confirmation is distinct: **W07**. |
| Identity UI Bootstrap4 MSBuild warning | **W08**; authored project property and customized UI/assets. Automatic Bootstrap vendor updates alone are excluded. |
| SignalR refresh server/client/features/metadata/UserIdentifier and TypeScript contract | PIN-813; shared goal across transports and Blazor. Public feature and hub members group under the same lifecycle. Actual npm availability/final-name gap remains open below. |
| Non-streaming hub cancellation | PIN-814; protocol/runtime behavior can change without a new method signature. Matched clients/server and actual cancellation remain required. |
| OpenAPI binary files, QUERY, multiple responses, SSE, 3.2 default, environment, Obsolete metadata, model migration, server URLs | PIN-821–829 retain individual scopes. UnauthorizedHttpResult's new IEndpointMetadataProvider groups with PIN-821/823 response metadata; remove redundant custom 401 metadata only after generated-document proof. |
| IAdditionalOpenApiDocumentNameResolver, AddOpenApiCore | **W04**; dynamic document names/build enumeration differs from PIN-826's environment selection. AddOpenApiCore does not register an individual named document or an options-callback overload. |
| ApiDescription.RoutePattern | **W03**; authored API-explorer tools can consume parsed route constraints. Remaining ApiDescription nullable-context/accessor changes are not separate operations. |
| Kestrel handshake Exception and ClientHello listener; obsolete callback | PIN-830; registration order before UseHttps, failure snapshots and real handshake evidence. |
| TLS channel binding and HTTP.sys Legacy/Medium/Strict | PIN-831; retain actual Windows/HTTPS prerequisites. **ASPDEPR010 ITlsTokenBindingFeature is a different protocol**, owned by **W15**, not a mechanical CBT substitution. |
| HTTP.sys TryGetRequestProperty | **W05**; native property IDs/struct parsing, buffer negotiation and unsupported API behavior differ from the existing typed TLS feature. |
| Stricter H2/H3 headers and H1 framing | PIN-832. H1 still processes the request then closes; it is not universally an initial rejection. Servicing can bring the close behavior to the old baseline. Remove preview-only insecure-chunked switch; no replacement switch. |
| Absolute-form encoded slash preservation | **W09**; middleware/route expectations and removable decoding compensation, not a new transport API. |
| Native request traces/route tags/status descriptions | PIN-833. SuppressActivityOpenTelemetryData does not restore route substitution or StatusDescription. |
| IOutputCachePolicyProvider | PIN-834; dynamic base/named policy selection, rather than fixed registration. |
| Zstandard providers/decompression | PIN-835; explicit negotiation/decoding policy, not an automatic-default performance recipe. |
| Removed concurrency middleware, MVC compatibility types, ImageTagHelper constructor/properties, ForProperty overload, runtime compilation registrations | PIN-836–840 respectively. Preserve source/binary versus behavioral distinctions; ASPDEPR003 is not runtime removal. **W16** handles F# compiled-view migration, which needs more than removing a registration. |
| ISpanDataProtector/ISpanAuthenticatedEncryptor | **W01**; writer-based protection and advanced custom encryptor contracts. New authored buffer ownership qualifies even when performance motivates it. |
| StaticAssetDescriptor.Order | **W06**; authored endpoint manifest/routing precedence. Generated default changes do not each need a recipe. |
| QUIC transport four buffer/stream properties lose RequiresPreviewFeatures | **W12**; explicit budgets and removal of a now-unnecessary preview workaround. Existing properties are not newly invented APIs. |
| JSON Patch ShouldSerializeFrom obsoletion; from/value JSON metadata | **W13**; default compiler warning and actual wire representation, distinct from the unchanged Newtonsoft implementation. |
| WebApplicationFactory.ConfigureWebApplicationBuilder | **W14**; new protected host-configuration seam relative to .NET 10. Only earlier .NET 11 previews used the renamed ConfigureHostApplicationBuilder spelling. |
| gRPC server/client/factory 2.64.0 and JSON transcoding | Selected public APIs have **zero differences**. No standalone transport recipe inferred. **W10** preserves the separately removed Grpc.Swagger integration, so the original blanket no-gRPC-change row is qualified. |
| DirectTls | **W11**, availability/contract unresolved; Linux/OpenSSL/experimental opt-in. Current source example cannot be copied against the available RC1 package. |
| dev-certs/user-jwts, MCP template, web SDK/project inputs | PIN-817–819 and PIN-707 retain development CLI/template/build ownership. The actual SDK tool relocation and archive versions are recorded; neither trust nor authentication was executed. |
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
changed bytes must fail. The generated report can be removed afterward. Replay
records executed checks and remaining gaps. No runtime/recipe/MCP behavior is
established by this audit; every delivery proposal keeps those acceptance gates.
