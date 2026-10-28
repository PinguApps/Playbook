# Declare component behaviour inside a cached section

## Goal

A library's public shelf summary can be reused, while its reader badge must obey an explicit contract. Keep one badge live on each request; let a stricter badge refuse a cache boundary unless that boundary varies by user.

## Older approach

On .NET 10 keep the request-aware badge outside the cached data. IMemoryCache reuses only the public shelf value; every request still constructs the component markup. CacheView and its component annotations are unavailable on this baseline.

{{snippet:older-shelf}}

{{snippet:older-page}}

{{snippet:older-live}}

This explicit live boundary remains valid. The ticket parameter is freshly supplied each request and the badge reads the current authenticated HttpContext. Data reuse does not imply markup or lifecycle reuse.

## Newer approach

On .NET 11 CacheBehavior.Rerender keeps a reusable child live even inside a CacheView:

{{snippet:live}}

Read fresh request information from a request-scoped/cascading source. The Ticket parameter demonstrates a limitation: CacheView captures parameters on the first render and replays those same values on hits. A live lifecycle does not refresh parent-supplied parameters. Never pass the current identity as a parameter into a shared cached boundary and assume Rerender makes it fresh.

A stricter badge declares both refusal and a condition that allows capture:

{{snippet:guarded}}

When all CacheCondition dimensions are active on the enclosing boundary, the badge can be included in cached output. Otherwise its CacheBehavior applies: Rerender stays live; Throw refuses capture. The two annotations solve different parts of the contract. A condition alone is not a substitute for specifying refusal when refusal is required.

{{snippet:page}}

The /live route shares public markup but rerenders the live badge for each user. The /guarded route varies by user, captures the guarded badge once per identity and skips its lifecycle on hits. The /refuse route deliberately omits that variation and fails with an InvalidOperationException/HTTP 500 on the measured server.

## Migration

1. Use Microsoft.NET.Sdk.Web, net11.0/C# 15.0, nullable and implicit usings enabled. The exact measured pins are SDK 11.0.100-rc.2.26504.105/framework and implicit Internal.Assets 11.0.0-rc.2.26504.105; the older project uses SDK 10.0.402, net10.0/C# 14.0 and framework/Internal.Assets 10.0.13. Lock the implicit dependency in packages.lock.json. No extra package, workload, native toolchain, provider service or experimental opt-in is required.
2. Place the components and LibraryState in Playbook.Samples. In _Imports.razor set @namespace Playbook.Samples and import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Http and Playbook.Samples. Use this noninteractive document:

{{snippet:document}}

3. Register Razor components, the state counter and your actual authentication system. This host uses ordinary cookie authentication. The sample repository's Development-only /control/signin/ada and /control/signin/ben are fictional test controls, not a production login design. The older host has the same registrations; AddMemoryCache supplies its public-data cache.

{{snippet:host}}

{{snippet:run}}

4. The newer public shelf deliberately counts actual lifecycle execution. LibraryState is thread-safe demonstration instrumentation, not application persistence:

{{snippet:shelf}}

{{snippet:state}}

5. Apply Rerender to a child that must read fresh request state. Apply Throw plus CacheCondition(CacheVaryBy.User) when the child permits capture only with explicit user variation. Test both the allowed and refused embeddings. Keep authorization decisions and side effects outside captured subtrees.
6. Issue repeated real GETs using two separate authenticated cookie sessions. In /live, public build stays fixed, badge lifecycle counts advance and user text changes correctly, while Ticket remains the first captured parameter. In /guarded, each user's repeated badge stays fixed and the other user has a separate badge. On .NET 10 the public data stays fixed but badge/ticket are fresh on each request.

## Constraints

Static SSR cache contracts; only the documented static GET capture path is exercised. Refusal is a rendering guard, not a pre-lifecycle security boundary: in measured RC2 the refused badge's OnInitialized has already executed before capture throws. Do not place writes, audits or permission enforcement there expecting Throw to prevent them.

CacheCondition checks dimension categories, not the semantic completeness of the key. User variation distinguishes the authenticated identity; it does not automatically refresh changed permissions, tenant, culture or private anonymous-session state. This sample badge depends only on the signed-in name from two trusted fictional identities. Any additional output dependency needs its own correct variation or a live boundary. All anonymous users share the anonymous identity dimension.

Rerendered parameters are replayed unchanged, including strings supplied by the parent; only the child's own fresh request lookup is current here. Cached children skip lifecycle work on hits. Neither annotation is an authorization API or a guarantee of distributed consistency.

Use blazor.cache-static-subtree for canonical boundary placement, variation, capacity, expiration, invalidation and optional HybridCache provider guidance. This recipe intentionally uses the default local memory store and a two-minute lifetime only to make repeated requests observable; it does not duplicate provider behavior or establish a distributed/multi-node contract. Nested CacheView, streaming, interactive modes, WebAssembly/Auto, trimming/AOT and other OS behavior are outside this execution scope.

The 11.0 edition is GA-targeted guidance prepared from RC2, not actual-GA verification or an older-target backport. The inspected annotations have no experimental label. Existing recipe identities and the eight .NET 10 foundations remain unchanged.

## Evidence

Actual cookie authentication and repeated SSR GETs execute on Windows x64, with no mock renderer. The old consumer records one public-data build and three live badge initializations. The new consumer records three live badge initializations, one public-subtree build for /live, one per user for /guarded, and two guarded initializations before refusal. Refusal returns 500 and increments the guarded initialization count to three, confirming its timing limitation. Both users' displayed names and captured/live ticket values are asserted. Separate canonical compilation and execution records plus installed offline list/get checks retain distinct claims.
