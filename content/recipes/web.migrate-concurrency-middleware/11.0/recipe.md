# Preserve queued request capacity when replacing retired middleware

## Goal

A dock service can complete one booking calculation at a time and hold one caller in a waiting queue. A third caller gets 503 while the service is full. Moving to ASP.NET Core 11 must preserve that capacity policy and release permits after work completes.

Replace the retired ConcurrencyLimiter middleware with the supported rate-limiting middleware's concurrency limiter. A concurrency limit measures simultaneous work; it is not a fixed number of requests per minute.

## Older approach — the retired queue middleware

The .NET 10 comparison uses Microsoft.AspNetCore.ConcurrencyLimiter **10.0.13**, its actual `net10.0` asset, `AddQueuePolicy` and `UseConcurrencyLimiter`. It deliberately suppresses CS0618 only around the historical calls so warnings-as-errors can compile the comparison. This is migration evidence, not advice to add the obsolete package to a new application. Rate-limiting middleware is already a valid alternative on .NET 10.

Save the following files in their own `DockCapacity` directory. Restore once to create `packages.lock.json`, retain that lock for repeatable builds, and run the fixture. It holds the first request, starts two more, observes one rejection, releases work and verifies the queued caller and a later recovery request.

### Directory.Build.props
{{snippet:n10-directory-build-props}}

### Directory.Build.targets
{{snippet:n10-directory-build-targets}}

### Directory.Packages.props
{{snippet:n10-directory-packages-props}}

### global.json
{{snippet:n10-global-json}}

### DockCapacity.csproj
{{snippet:n10-dockcapacity-csproj}}

### Program.cs
{{snippet:n10-program-cs}}

## Newer approach — one global concurrency partition

Remove the package and register a global `PartitionedRateLimiter` whose constant key gives the whole process one shared capacity pool. `PermitLimit` replaces `MaxConcurrentRequests`; `QueueLimit` replaces `RequestQueueLimit`; `OldestFirst` retains FIFO queue intent. Register `UseRateLimiter` before the protected work. This global policy needs no endpoint-specific `RequireRateLimiting` metadata.

The replacement explicitly returns **503** to preserve this service's old rejection response. The rate-limiting middleware otherwise uses its own rejection default; choosing 429 instead can be a reasonable deliberate API change, but is not silently done here. A concurrency limiter cannot predict when work will finish, so do not invent a Retry-After duration.

Save the newer files separately. There is no replacement NuGet dependency: the matching ASP.NET Core shared framework supplies the middleware.

### Directory.Build.props
{{snippet:n11-directory-build-props}}

### Directory.Build.targets
{{snippet:n11-directory-build-targets}}

### Directory.Packages.props
{{snippet:n11-directory-packages-props}}

### global.json
{{snippet:n11-global-json}}

### DockCapacity.csproj
{{snippet:n11-dockcapacity-csproj}}

### Program.cs
{{snippet:n11-program-cs}}

## Migration

1. Record the actual old capacity, queue size, FIFO/stack choice, pipeline coverage, rejection body/status and cancellation expectations. The fixture's values are intentionally tiny to make saturation observable, not production sizing recommendations.
2. Remove `UseConcurrencyLimiter`, its old queue/stack registration and the retired package reference. Replace them with `AddRateLimiter` and `UseRateLimiter`.
3. Keep a constant global partition to preserve a shared limit. Switching to a named per-endpoint or per-tenant policy changes what competes for permits; do that only as a separate deliberate design choice. Place the new middleware at the equivalent point so earlier short-circuiting middleware has the intended treatment. Endpoint-specific policies additionally need routing before the limiter.
4. Use `OldestFirst` for an old queue policy. An old stack policy needs an explicit `NewestFirst` decision and tests for eviction/rejection semantics; this fixture does not claim that path was exercised.
5. Preserve the old rejection contract explicitly. Reproduce saturation with real overlapping requests and a blocked handler; do not infer queuing from configuration alone. Verify that the queued request completes after release and subsequent requests regain capacity.
6. Build each directory with its declared SDK (`dotnet build --configuration Release`), then run `dotnet bin/Release/net10.0/DockCapacity.dll` or the corresponding `net11.0` path. Use absolute executable paths and `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` with isolated CLI homes. Expect four real requests, three completed handlers, one rejected caller and maximum active work of one.

## Constraints and temporary fallback

- ASP.NET Core 11 removes the old middleware source and ships no new Microsoft.AspNetCore.ConcurrencyLimiter 11.x package. An existing older package may temporarily be retained while migrating; this example verifies 10.0.13 only on .NET 10 and does **not** advertise or establish a retained-package .NET 11 route. Do not infer that the lack of an 11.x package makes every older asset intrinsically incompatible.
- The official removal note contains broad unavailable/restore wording alongside its retained-package workaround. Interpret the concrete change as no newly shipped 11.x package/source, not proof that explicitly pinning any older package must fail. The supported replacement avoids that ambiguity.
- Exact sample baselines: SDK 10.0.402 / C# 14 / runtime and package 10.0.13; SDK 11.0.100-rc.2.26504.105 / C# 15 / matching RC2 shared frameworks. The new recipe targets 11.0 only; the older application remains a comparison.
- Kestrel server HTTP on supported server platforms; actual execution is Windows x64. No browser or Blazor rendering requirement, external provider, account, workload or experimental opt-in applies. No native, cross-platform, AOT or trimming execution is claimed.
- Limits are process-local. Multiple instances each admit their own work. Do not present a constant partition as a distributed quota, and do not partition by unbounded untrusted input without a resource strategy.
- The real fixture verifies saturation, queue admission, 503 rejection and release/recovery. With only one waiting caller it does not establish relative ordering among multiple queued callers, stack eviction, client-disconnect behavior or fairness under sustained load.
- Stable API intent is prepared with RC2 evidence under ADR 0004. GA reconciliation and product-wide publication gates remain separate.

## Evidence

Both complete projects were restored, compiled and executed through `eng/Record-Evidence.ps1`. The older dependency lock and saved input hashes identify the actual package. Four actual loopback HTTP requests establish one running handler plus one waiting caller, one 503 rejection during saturation, completion of the queued request after release, and successful later recovery with maximum concurrency one. No sleep determines admission and no mock limiter substitutes for middleware execution.

The existing actual packed-tool harness checks complete canonical Markdown, full applicability, fresh evidence, discovery title/goal and target-11 edition through real stdio list/get after isolated feed/cache installation outside the checkout. Target 10.0 does not select this edition. Stable identity: `web.migrate-concurrency-middleware` / WEB-084.

Official context: [removal and temporary fallback](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/concurrencylimiter-removed), [supported rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-11.0), and [the historical package](https://www.nuget.org/packages/Microsoft.AspNetCore.ConcurrencyLimiter/10.0.13). Essential migration instructions remain available offline.
