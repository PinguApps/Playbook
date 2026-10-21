using System.Diagnostics.Metrics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Playbook.Samples;

public static class CacheExamples
{
    // snippet:observe
    public static Task ObserveAsync()
    {
        using MemoryCache cache = new(new MemoryCacheOptions { TrackStatistics = true, SizeLimit = 100 });
        using Meter meter = new("Quayside.Cache");
        // .NET 10 already exposes statistics; an application can adapt them to metrics.
        meter.CreateObservableGauge("quayside.cache.rows", () => cache.GetCurrentStatistics()!.CurrentEstimatedSize ?? 0);
        long measured = -1;
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, observer) =>
        {
            if (instrument.Meter == meter) { observer.EnableMeasurementEvents(instrument); }
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => measured = value);
        listener.Start();
        int[] rows = Enumerable.Range(1, 12).ToArray();
        cache.Set("tide-table", rows, new MemoryCacheEntryOptions().SetSize(rows.Length));
        _ = cache.TryGetValue("tide-table", out _);
        _ = cache.TryGetValue("absent-table", out _);
        listener.RecordObservableInstruments();
        MemoryCacheStatistics stats = cache.GetCurrentStatistics()!;
        if (stats.TotalHits != 1 || stats.TotalMisses != 1 || stats.CurrentEntryCount != 1 || measured != 12)
        { throw new Exception("Cache statistics or application metric changed."); }
        // Size 12 means twelve application rows, not bytes.
        return Task.CompletedTask;
    }
    // endsnippet:observe

    // snippet:value
    public static async Task ValueOptionsAsync()
    {
        ServiceCollection services = new();
        services.AddHybridCache();
        using ServiceProvider provider = services.BuildServiceProvider();
        HybridCache cache = provider.GetRequiredService<HybridCache>();
        string result = await LoadPlanAsync();
        // Selecting options before SetAsync is valid, but this load is outside miss coalescing.
        await cache.SetAsync("berth-plan", result, new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromSeconds(result.Length),
            LocalCacheExpiration = TimeSpan.FromMilliseconds(500),
        });
        string hit = await cache.GetOrCreateAsync("berth-plan", _ => ValueTask.FromResult("unexpected reload"));
        if (hit != result) { throw new Exception("Expected the stored plan."); }
        await Task.Delay(750);
        string expired = await cache.GetOrCreateAsync("berth-plan", _ => ValueTask.FromResult("reloaded"));
        if (expired != "reloaded") { throw new Exception("Expected local expiration."); }
    }
    private static async Task<string> LoadPlanAsync() { await Task.Yield(); return "twelve rows!"; }
    // endsnippet:value

    public static async Task RunAsync() { await ObserveAsync(); await ValueOptionsAsync(); }
}
