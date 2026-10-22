using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Playbook.Samples;

public static class CacheExamples
{
    // snippet:observe
    public static async Task ObserveAsync()
    {
        ServiceCollection services = new();
        services.AddMetrics(); // Supplies IMeterFactory to the MemoryCache constructor.
        services.AddMemoryCache(o => { o.TrackStatistics = true; o.SizeLimit = 100; o.Name = "quayside"; });
        using ServiceProvider provider = services.BuildServiceProvider();
        ConcurrentDictionary<string, long> readings = new();
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, observer) =>
        {
            if (instrument.Meter.Name == "Microsoft.Extensions.Caching.Memory.MemoryCache")
            {
                observer.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
        {
            string result = "";
            bool named = false;
            foreach (KeyValuePair<string, object?> tag in tags)
            {
                if (tag.Key == "dotnet.cache.request.result") { result = ":" + tag.Value; }
                if (tag.Key == "dotnet.cache.name") { named = Equals(tag.Value, "quayside"); }
            }
            if (!named) { throw new Exception("Missing cache name tag."); }
            readings[instrument.Name + result] = value;
        });
        listener.Start();
        MemoryCache cache = (MemoryCache)provider.GetRequiredService<IMemoryCache>();
        TaskCompletionSource evicted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int[] rows = Enumerable.Range(1, 12).ToArray();
        cache.Set("tide-table", rows, new MemoryCacheEntryOptions().SetSize(rows.Length)
            .RegisterPostEvictionCallback((_, _, _, _) => evicted.TrySetResult()));
        _ = cache.TryGetValue("tide-table", out _);
        _ = cache.TryGetValue("absent-table", out _);
        MemoryCacheStatistics before = cache.GetCurrentStatistics()!;
        listener.RecordObservableInstruments();
        if (before.TotalHits != 1 || before.TotalMisses != 1 || before.CurrentEntryCount != 1 || before.CurrentEstimatedSize != 12
            || readings["dotnet.cache.requests:hit"] != 1 || readings["dotnet.cache.requests:miss"] != 1
            || readings["dotnet.cache.entries"] != 1 || readings["dotnet.cache.estimated_size"] != 12)
        { throw new Exception("Cache measurements differ from the workload."); }
        cache.Compact(1.0);
        await evicted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        listener.RecordObservableInstruments();
        if (cache.GetCurrentStatistics()!.TotalEvictions != 1 || readings["dotnet.cache.evictions"] != 1)
        { throw new Exception("The compaction eviction was not counted."); }
        // Size 12 means twelve application rows, not twelve bytes or process memory.
        Console.WriteLine("PASS metrics requests(hit=1,miss=1), entries=1, estimated_size=12 rows, evictions=1; dotnet.cache.name=quayside");
    }
    // endsnippet:observe

    // snippet:value
    public static async Task ValueOptionsAsync()
    {
        ServiceCollection services = new();
        services.AddMemoryCache(o => { o.TrackStatistics = true; o.SizeLimit = 100; });
        services.AddHybridCache();
        using ServiceProvider provider = services.BuildServiceProvider();
        HybridCache cache = provider.GetRequiredService<HybridCache>();
        int loads = 0;
        ValueTask<string> LoadAsync(HybridCacheEntryContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            loads++;
            string result = "twelve rows!";
            context.Expiration = TimeSpan.FromSeconds(result.Length);
            context.LocalCacheExpiration = TimeSpan.FromMilliseconds(500);
            context.LocalSize = result.Length * 2;
            return ValueTask.FromResult(result);
        }
        string first = await cache.GetOrCreateAsync<string>("berth-plan", LoadAsync);
        string hit = await cache.GetOrCreateAsync<string>("berth-plan", LoadAsync);
        if (loads != 1 || first != hit) { throw new Exception("Expected a local hit."); }
        MemoryCacheStatistics stats = ((MemoryCache)provider.GetRequiredService<IMemoryCache>()).GetCurrentStatistics()!;
        // Hybrid 10.11.0 uses the new abstraction's fallback. It does NOT read LocalSize.
        if (stats.CurrentEstimatedSize != 12) { throw new Exception($"Recheck LocalSize: entries={stats.CurrentEntryCount}, size={stats.CurrentEstimatedSize}."); }
        await Task.Delay(750);
        _ = await cache.GetOrCreateAsync<string>("berth-plan", LoadAsync);
        if (loads != 2) { throw new Exception("Value-derived local expiration was ignored."); }
        Console.WriteLine("PASS Hybrid 10.11.0/net10.0 implementation + abstractions 11 RC2: local TTL honoured; LocalSize ignored (requested LocalSize=24, actual L1 size=12)");
    }
    // endsnippet:value

    public static async Task RunAsync() { await ObserveAsync(); await ValueOptionsAsync(); }
}
