# Choose cache lifetimes after loading a value

## Goal

Choose a berth plan's cache lifetime from the value produced by its factory, while keeping the factory inside the cache's miss path.

## Older approach

.NET 10 HybridCache can accept options before a factory runs. When options depend on the result, an application can load first and call SetAsync with the derived policy. This is valid but that initial load is outside GetOrCreateAsync's miss coalescing. For a local-only cache, IMemoryCache's factory already permits changing ICacheEntry after loading.

{{snippet:older}}

## Newer approach

The documented .NET 11 overload supplies a mutable HybridCacheEntryContext to the factory. Set Expiration, LocalCacheExpiration, Flags or LocalSize after producing the value. A hit does not run this policy again. The original factory derives its overall lifetime from the string length and chooses a short local lifetime for a repeatable expiration check.

{{snippet:newer}}

**Known RC implementation gap:** the tested Microsoft.Extensions.Caching.Hybrid 10.11.0 net10.0 implementation with .NET 11 RC2 abstractions honours the derived local lifetime but ignores LocalSize. The sample requests 24 units and measures an actual L1 size of 12. The setter illustrates the documented .NET 11 contract; it is not verified custom sizing on this implementation. Do not depend on it to enforce an RC cache budget. Pingu accepts this explicit RC evidence gap for the GA-targeted recipe's PR delivery. GA reconciliation must recheck actual package behavior; a future fix is not assumed to have happened.

## Migration

1. Use a net11.0 console project with C# 15.0, Nullable/ImplicitUsings enabled and FrameworkReference Microsoft.AspNetCore.App. Add Microsoft.Extensions.Caching.Hybrid pinned to 10.11.0 and Microsoft.Extensions.Caching.Abstractions pinned to 11.0.0-rc.2.26504.105. These are independently versioned packages; do not invent HybridCache package 11 from the abstraction's version.
2. Import Microsoft.Extensions.Caching.Hybrid, Microsoft.Extensions.Caching.Memory and Microsoft.Extensions.DependencyInjection. Place the method inside public static class CacheExamples in your application namespace and await ValueOptionsAsync. The latter two imports support the real local-cache observation in this example.
3. Register AddHybridCache and a dedicated size-limited MemoryCache as shown. Use the context-taking factory overload and await the entire GetOrCreateAsync operation. Keep key/value types and cache policy consistent across callers of the same key.
4. Select both total and local lifetimes deliberately; local expiration cannot keep an item beyond the overall policy. Respect factory cancellation. Do not reuse the context outside its factory or treat per-call policy changes as retroactive changes to existing hits.
5. Treat LocalSize as a documented GA-targeted setting with the measured RC restriction above. Use direct MemoryCache Size for verified application-defined units now; recheck the actual HybridCache implementation before depending on custom L1 size accounting. Keep the RC regression check until GA artifacts establish the intended behavior.

## Constraints

Only this net11 route is advertised. The actual HybridCache implementation asset is lib/net10.0/Microsoft.Extensions.Caching.Hybrid.dll; the net11 abstraction package's empty asset defers to shared-framework types. The sample intentionally suppresses NU1510 for that explicit abstraction pin. Package TFM availability alone does not establish behavior or support for an older-target route.

The .NET 11 base implementation can adapt context overloads to an older implementation by suppressing the inner writes and performing a final SetAsync. This does not make an older implementation understand every new option. The LocalSize mismatch is real execution evidence, not a mocked cache or a successful size test. The separate MemoryCache recipe demonstrates supported application units; do not mix those units with this HybridCache implementation's own size estimate in a shared budget.

There is no distributed backend in this workload. Local expiration causes a new factory load because no L2 is configured. Overall distributed expiry, cross-node behavior, serializers, tags and backend failures need their own integration proof. No resource provisioning or publication is needed.

## Evidence

Actual .NET 10 and .NET 11 RC2 projects compile and execute the same original value/expiration goal. The newer path verifies a local hit, no duplicate factory call for that hit, another load after local expiry, and the explicit requested-24/observed-12 LocalSize discrepancy. Exact packages, resolved assets, SDK/framework baseline and source hashes are saved. The documented custom LocalSize contract remains unverified on the selected RC implementation, accepted by Pingu for this PR and still owed at GA. No GA, external backend or native execution is claimed.
