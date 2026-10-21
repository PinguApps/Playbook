# Observe cache requests, entries, evictions and estimated size

## Goal

Observe a harbour's in-process tide-table cache using built-in instruments, with explicit size accounting in application-defined units.

## Older approach

.NET 10 already has TrackStatistics and GetCurrentStatistics. An application can adapt those snapshots to its own Meter instruments. This remains useful when an established monitoring vocabulary should stay unchanged; the original comparison exports a custom row-count gauge.

{{snippet:older}}

## Newer approach

.NET 11 adds built-in cache instruments and TotalEvictions. Enable statistics, register metrics so DI supplies IMeterFactory, and name the cache. The original workload reads a hit and a miss, observes a twelve-row entry, then compacts it and observes an eviction.

{{snippet:newer}}

## Migration

1. Use a net11.0 console project with C# 15.0, Nullable/ImplicitUsings enabled and FrameworkReference Microsoft.AspNetCore.App. Import System.Collections.Concurrent, System.Diagnostics.Metrics, Microsoft.Extensions.Caching.Memory and Microsoft.Extensions.DependencyInjection. Place the method inside public static class CacheExamples in your application namespace and await ObserveAsync.
2. Register AddMetrics and AddMemoryCache as shown. Turn TrackStatistics on before constructing the cache and choose a stable, low-cardinality Name. Do not put tenant identifiers or keys in the name.
3. Connect your metrics listener/exporter to Microsoft.Extensions.Caching.Memory.MemoryCache. The sample uses the real MeterListener without an exporter or network service. Record observable instruments when collecting measurements.
4. Supply Size on every entry in this dedicated size-limited cache using one consistent unit. The sample uses rows: its SizeLimit 100 permits a total budget of 100 declared rows, not 100 bytes.
5. Verify both snapshots and instruments before removing an old custom adapter. Ensure other consumers of a shared cache also supply compatible sizes; otherwise prefer a dedicated cache instance.

## Constraints

The instruments are dotnet.cache.requests (observable counter, hit/miss tag dotnet.cache.request.result), dotnet.cache.evictions (observable counter), dotnet.cache.entries (observable up/down counter) and dotnet.cache.estimated_size (observable gauge). RC2 identifies the cache with dotnet.cache.name. Estimated size has unit 1 and is application accounting, not managed heap or process memory. Without SizeLimit, CurrentEstimatedSize is null and that gauge has no value. With TrackStatistics disabled, GetCurrentStatistics is null and these statistical observations have no values.

The eviction count is not a count of every removal. In the pinned implementation, explicit Remove and Clear do not increment it; the sample uses compaction, which does. Request counts include only this cache's lookups, not backend requests or HybridCache-wide hit ratios. Metrics collection/export transport and distributed cache behavior are separate integrations. No account, rendering mode, external provider or native workload is required.

## Evidence

Actual .NET 10/11 RC2 projects compile and execute real MemoryCache/MeterListener workloads. The newer path checks hit=1, miss=1, entries=1, estimated_size=12 rows, name/result tags and compaction evictions=1. This establishes managed cache behavior, not an exporter/dashboard or exact process memory measurement. Canonical snippets, exact artifacts and fresh input hashes are bundled; GA reconciliation remains required.
