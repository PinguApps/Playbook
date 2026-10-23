# Cache static sections

## Goal

A quarry team reads an expensive survey summary many times between revisions. Reuse that section while the rest of the page renders for each request. A cached summary must never greet the wrong signed-in reader or retain an obsolete access level.

## Older approach

On .NET 10, cache the public survey data with IMemoryCache and build the Razor markup for every request. This preserves fresh user output and works well when aggregation costs more than rendering. CacheView's framework subtree capture is unavailable on this baseline; this comparison does not claim a backported package route.

SurveySection.razor keeps identities out of the data entry:

{{snippet:older}}

The static page takes its identity from the authenticated HttpContext and snapshots the server revision before constructing the child:

{{snippet:older-page}}

The older host uses AddRazorComponents, AddMemoryCache and the cookie authentication/authorization middleware shown below, without CacheView options or AddHybridCache. The data key includes revision and shelf because both affect the aggregate. IMemoryCache.GetOrCreateAsync can run concurrent factories; the older comparison makes no request-coalescing promise. Raw user HTML is never stored in this shared entry.

## Newer approach

On .NET 11, surround only the expensive section with CacheView. The request marker stays outside. Use a distinct CacheKey for each logical boundary, especially repeated instances. The key also incorporates the boundary's tree location; CacheKey is not a documented cache-removal handle.

{{snippet:newer}}

VaryByQuery covers the shelf used by the child. VaryByUser separates authenticated identities. VaryBy adds the server revision and permission value used in the rendered section. An identity key alone does not capture every claim: the pinned resolver prefers a nonempty name-identifier claim. Include every output-affecting entitlement, tenant, locale or other value in the relevant variation policy, or move that markup outside. All anonymous requests share the anonymous user dimension; private guest/session data needs its own trusted dimension or an uncached boundary.

The child deliberately records construction and lifecycle work. A hit returns the same section, including its stamp, without calling OnInitializedAsync again:

{{snippet:section}}

The original local aggregation and content revision are explicit:

{{snippet:state}}

## Migration

1. Use Microsoft.NET.Sdk.Web, net11.0 and C# 15.0. The measured SDK/runtime are 11.0.100-rc.2.26504.105 / 11.0.0-rc.2.26504.105. The older project uses net10.0, C# 14.0, SDK 10.0.402 and runtime/browser assets 10.0.13. Each project locks its SDK-aligned Internal.Assets. No optional workload or experimental opt-in is required. These are RC2 results, not GA proof.
2. Put these components in Playbook.Samples. In _Imports.razor set @namespace Playbook.Samples and import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Http and Playbook.Samples. Use this static App.razor; no interactive activation or browser script is needed:

{{snippet:document}}

3. Use this Program.cs host plus the final map/run statements. Supply your application's real login and authorization policy before rendering. The canonical sample's Development-only sign-in and revision controls are local test fixtures, not production authentication or write endpoints.

{{snippet:host}}

{{snippet:run}}

4. The optional host branch compiles against Microsoft.Extensions.Caching.Hybrid 10.11.0, whose net10.0 asset is actually executed here by net11.0. Pin it in the sample's isolated central package file and lock restore. Omit that branch and package for a default-memory-only host. Registering HybridCache selects it automatically; CacheViewHybridCache can instead select a specific instance. The sample configures no distributed backing store or external service.
5. Choose a bounded lifetime: the page defaults to 120 seconds via CacheSeconds. ExpiresAfter measures from creation; ExpiresOn sets a deadline. Without either, RC2 uses 30 seconds. The default memory store also accepts ExpiresSliding, bounded by ExpiresOn, then ExpiresAfter, then that default absolute lifetime. HybridCache rejects sliding expiration; choose an absolute lifetime. Memory capacity defaults to 100 MB; this host sets 16 MB. CacheViewSizeLimit does not limit HybridCache, which needs its own storage/size policy.
6. After a successful content change, advance a server-owned revision and include the same revision snapshot in both VaryBy and child inputs. New requests use new entries; old entries remain until expiry. This is logical invalidation, not a public CacheView.Remove API. The sample's volatile counter is appropriate only for this local demonstration. Production revisions must track the authoritative data and be coordinated across nodes; asynchronous distributed eviction alone is not an authorization guarantee. Avoid a race between committing data and publishing its revision. Keep checks that must be current on every request outside the cached section, and use Enabled=false to bypass reads and writes when freshness cannot be established.

## Constraints

Static SSR only; cache hits skip child lifecycle; isolate user-specific markup.

- Only static GET output is cached. Interactive rendering and a CacheView inside a streaming subtree bypass the cache. Nested CacheView boundaries are rejected. Keep request-specific controls, side effects and fresh authorization outside the boundary. A cache hit cannot run the child's data fetching, audit or permission checks again.
- Variation is explicit, not inferred from parameters. VaryByRoute, VaryByHeader, VaryByCookie and VaryByCulture can cover other real dependencies; select the exact names consumed by the output. Query variation supports named parameters or all parameters with an asterisk. Arbitrary client values can create unbounded keys: validate finite selections and apply an entry lifetime/capacity policy. This sample executes query, identity and custom revision/permission variation; the other dimensions are pinned-source support, not additional execution claims.
- AuthorizeView requires user variation, but user variation alone does not refresh changed permissions. AntiforgeryToken and HeadOutlet remain live according to the pinned component annotations. Do not infer that every descendant stays fresh: cached parameters, including those passed to live children, are replayed unchanged. Authoring reusable live-child/refusal contracts is separate guidance; moving private markup outside the boundary is the simplest safe option.
- Do not compute the framework's hashed key yourself to invalidate entries. The pinned HybridCache implementation uses an internal tag for best-effort hot-reload eviction; that implementation detail is not an application invalidation contract. A shared provider needs independently tested multi-node behavior, serialization/type availability, key/schema deployment isolation and eviction policy. Only local memory providers are verified here.
- The .NET 10 comparison caches data, so child markup and lifecycle remain fresh. The .NET 11 edition is new guidance, not another curated .NET 10 foundation or an optional older-target route. No WebAssembly/Auto, browser interaction, distributed service, trimming/AOT, other OS or GA behavior is established by these tests.

## Evidence

Both real Web SDK projects compile with their declared SDK/language and locked dependencies. Separate real loopback HTTP execution on Windows x64 uses actual cookie authentication and rendered sections, not calculated keys or a mock renderer. It verifies repeated hits and skipped lifecycle; distinct users, anonymous requests, query and changed permission keys; revision invalidation for multiple users; eight concurrent same-key requests and concurrent distinct users; absolute expiry for both providers; memory sliding/absolute bounds; HybridCache's real sliding refusal; and Enabled=false. The older comparison proves public data reuse while markup renders separately for each reader.

A Development-only negative fixture omits user variation and reproduces a cross-user leak with fictional identities. The same test checks that the safe page never reproduces it. No real accounts or external resources are involved. Reports bind exact app inputs and built DLL/deps/runtime artifacts; compilation is recorded separately as compile evidence. Official pinned RC2 sources support unexecuted restrictions and other variation dimensions. The repository's PIN-780 verification record contains commands and execution reports; all implementation instructions needed to apply this guidance are included above.
