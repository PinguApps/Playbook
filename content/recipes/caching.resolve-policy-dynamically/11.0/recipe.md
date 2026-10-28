# Select output caching from current tenant policy

## Goal

A station directory serves public cards for two customer stations. West pauses caching during maintenance while East keeps its cached card. When West changes its policy again, it must not revive a card stored under the previous revision. Unknown stations bypass caching.

Use `IOutputCachePolicyProvider` on ASP.NET Core 11 when named policies come from current settings rather than a fixed `AddPolicy` table. This edition compares .NET 10 with .NET 11 RC2. It is not an additional .NET 10 foundation or a claim that the provider exists on .NET 10.

## Older approach — register a policy that dispatches per request

.NET 10 supports a custom `IOutputCachePolicy`. Register one named policy whose request phase chooses a concrete policy and whose later phases delegate to the same selection. This is a valid dynamic implementation; the newer provider centralizes named-policy resolution rather than making older custom policies incorrect.

Save these files in an independent `StationCache` directory. The program starts an ephemeral loopback server, verifies its public cards and exits.

### Directory.Build.props
{{snippet:n10-directory-build-props}}

### Directory.Build.targets
{{snippet:n10-directory-build-targets}}

### Directory.Packages.props
{{snippet:n10-directory-packages-props}}

### global.json
{{snippet:n10-global-json}}

### StationCache.csproj
{{snippet:n10-stationcache-csproj}}

### Program.cs
{{snippet:n10-program-cs}}

## Newer approach — resolve the named policy through a provider

Call `AddOutputCache` first, then register the custom provider. Keep the endpoint's `CacheOutput("station-card")` reference; there is no fixed `AddPolicy` registration. `GetBasePolicies` returns an empty list because this example applies no policy globally. A replacement provider owns both base and named policies; it does not automatically inherit the options-based provider's registrations. Explicitly compose any base/default or other named policies your application needs. Unknown names return null; here that leaves caching disabled.

The provider has no request parameter. `IHttpContextAccessor` supplies the active request when resolving the station name. The singleton stores only dependencies and an immutable key: request-specific selections live in `HttpContext.Items`. Named-policy resolution can run during request, cache-hit and response phases. Capturing one immutable rule per request prevents a concurrent settings change from switching policy halfway through a response.

The example's concurrent dictionary is a real local policy source, not a simulated database. For an external source, load asynchronously in `GetPolicyAsync`, bound failures/time, and choose an explicit failure policy. The interface supplies no cancellation token. Do not capture a scoped database context in a singleton or retain a live `HttpContext`. A request snapshot intentionally takes effect on the next request, not halfway through this one.

Save the newer files in a separate directory.

### Directory.Build.props
{{snippet:n11-directory-build-props}}

### Directory.Build.targets
{{snippet:n11-directory-build-targets}}

### Directory.Packages.props
{{snippet:n11-directory-packages-props}}

### global.json
{{snippet:n11-global-json}}

### StationCache.csproj
{{snippet:n11-stationcache-csproj}}

### Program.cs
{{snippet:n11-program-cs}}

## Migration

1. Inventory the named and base policies your endpoints actually use. Move their selection to the provider, preserving defaults deliberately rather than silently dropping them.
2. Establish the tenant using the application's normal trusted routing/authentication boundary before output caching. These public station cards contain no private or user-specific data; a route value is sufficient for this fixture, not proof of customer authorization.
3. Snapshot the selected rule once per request. Keep shared provider state thread-safe and request state in the request.
4. Partition cache entries by every input that changes output. Here the URL and explicit station/revision vary values prevent cross-station and old-revision reuse. If output also depends on query values, culture or other inputs, add those deliberately. A policy change alone does not evict stored responses; a new revision changes the key. **Every enable/disable transition must increase the revision**, as both samples do. Re-enabling with the previous revision can revive its stored card until expiry. Old entries expire after 30 seconds.
5. Retain eligibility rules: unauthenticated GET/HEAD only, no Authorization header, 200 responses, no Set-Cookie. Do not use this public-card policy for private responses. Authentication/authorization middleware must run before caching when those features are present.
6. In each sample directory, use its declared SDK to run `dotnet build --configuration Release`, then `dotnet bin/Release/net10.0/StationCache.dll` or the corresponding `net11.0` path. Use absolute executable paths and set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` if using an isolated CLI home. Expect eleven real HTTP requests and seven endpoint executions.

## Constraints and alternatives

- Runtime/SDK/language: ASP.NET Core 11 for the provider; exact sample baselines are SDK 11.0.100-rc.2.26504.105 / C# 15 and SDK 10.0.402 / C# 14 with .NET 10.0.13. No .NET 10 route to the new provider is advertised.
- Packages/assets: only the matching ASP.NET Core and .NET shared frameworks. The local build/package boundaries prevent inherited package settings when copied. No workload, external provider, account or experimental opt-in is required.
- Platform/rendering: actual Windows x64 Kestrel HTTP execution. No browser, Blazor rendering mode, distributed cache, authenticated response cache, AOT, trimming or remote policy-store behavior was tested.
- Provider lookup and cache-entry storage are separate concerns. Choosing a policy neither invalidates old entries nor provides a tenant authorization check. Frequent revisions can retain multiple entries until expiry; budget cache memory and lifetime accordingly.
- Fixed options-based policies remain simpler for settings that never change at runtime. A custom policy remains useful when only one registered policy needs request-dependent behavior.
- Stable API intent is prepared using RC2 under ADR 0004. GA reconciliation is still required; these records establish neither GA behavior nor product publication readiness.

## Evidence

Both complete pinned projects compiled and executed with real loopback requests through `eng/Record-Evidence.ps1`. Each recorded eleven requests and seven handler executions, including hits for two stations, cache bypass while disabled, a fresh entry after a revision change, the unaffected station's retained hit, and two uncached unknown-station requests. One rule read per request establishes snapshot selection across cache stages. These are local runtime checks, not mocks, cross-compilation or external-provider evidence.

The existing packed-tool harness checks complete canonical Markdown, discovery title/goal, full applicability and edition 11.0 over real stdio list/get after an isolated local-feed/cache installation outside the checkout. Target 10.0 has no applicable edition. The stable identity is `caching.resolve-policy-dynamically` / WEB-072.

Official context: [provider introduction](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11#ioutputcachepolicyprovider-interface), [output-cache policies](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-11.0), and [RC2 named-policy stages](https://github.com/dotnet/aspnetcore/blob/v11.0.0-rc.2.26504.105/src/Middleware/OutputCaching/src/Policies/NamedPolicy.cs). Essential implementation and constraints are included offline above.
