// snippet:complete
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.OutputCaching;

var store = new StationRules();
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
// snippet:registration
builder.Services.AddSingleton(store);
builder.Services.AddOutputCache(options => options.AddPolicy("station-card", new StationDispatchPolicy(store)));
// endsnippet:registration
await using var app = builder.Build();
app.UseOutputCache();
app.MapGet("/cards/{station}", (string station) => store.NextCard(station))
    .CacheOutput("station-card");
await app.StartAsync();
string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var client = new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
string west = await client.GetStringAsync(new Uri("/cards/west", UriKind.Relative));
if (west != await client.GetStringAsync(new Uri("/cards/west", UriKind.Relative))) { throw new Exception("Enabled station did not hit cache."); }
string east = await client.GetStringAsync(new Uri("/cards/east", UriKind.Relative));
if (east == west || east != await client.GetStringAsync(new Uri("/cards/east", UriKind.Relative))) { throw new Exception("Station cache keys collided."); }
store.Set("west", new StationRule(false, 2));
string disabled = await client.GetStringAsync(new Uri("/cards/west", UriKind.Relative));
if (disabled == west || disabled == await client.GetStringAsync(new Uri("/cards/west", UriKind.Relative))) { throw new Exception("Disabled station reused cached output."); }
store.Set("west", new StationRule(true, 3));
string revised = await client.GetStringAsync(new Uri("/cards/west", UriKind.Relative));
if (revised == west || revised == disabled || revised != await client.GetStringAsync(new Uri("/cards/west", UriKind.Relative))) { throw new Exception("New revision reused stale output or failed to cache."); }
if (east != await client.GetStringAsync(new Uri("/cards/east", UriKind.Relative))) { throw new Exception("Unchanged station lost its cache."); }
string unknown = await client.GetStringAsync(new Uri("/cards/unknown", UriKind.Relative));
if (unknown == await client.GetStringAsync(new Uri("/cards/unknown", UriKind.Relative))) { throw new Exception("Unknown station reused cached output."); }
if (store.Reads != 11 || store.Executions != 7) { throw new Exception($"Expected one rule snapshot per request and seven handler calls: {store.Reads}/{store.Executions}."); }
await app.StopAsync();
Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: 11 real HTTP requests, 7 endpoint executions; tenant separation, disable/re-enable, revision invalidation and unknown-station bypass; one rule read/request through the registered request-dispatch policy.");

sealed record StationRule(bool Enabled, int Revision);
sealed class StationRules
{
    private readonly ConcurrentDictionary<string, StationRule> _rules = new(StringComparer.Ordinal)
    {
        ["west"] = new(true, 1), ["east"] = new(true, 1)
    };
    private int _reads;
    private int _executions;
    public int Executions => Volatile.Read(ref _executions);
    public string NextCard(string station) => $"{station}:{Interlocked.Increment(ref _executions)}";
    public int Reads => Volatile.Read(ref _reads);
    public StationRule Read(string station)
    {
        Interlocked.Increment(ref _reads);
        return _rules.TryGetValue(station, out StationRule? rule) ? rule : new(false, 0);
    }
    public void Set(string station, StationRule rule) => _rules[station] = rule;
}

// snippet:dispatcher
sealed class StationDispatchPolicy(StationRules rules) : IOutputCachePolicy
{
    private static readonly object SnapshotKey = new();
    private static IOutputCachePolicy Selected(OutputCacheContext context) => (IOutputCachePolicy)context.HttpContext.Items[SnapshotKey]!;
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        string station = (string)context.HttpContext.Request.RouteValues["station"]!;
        StationRule rule = rules.Read(station);
        context.HttpContext.Items[SnapshotKey] = new StationCachePolicy(station, rule);
        return Selected(context).CacheRequestAsync(context, cancellation);
    }
    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => Selected(context).ServeFromCacheAsync(context, cancellation);
    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) => Selected(context).ServeResponseAsync(context, cancellation);
}
// endsnippet:dispatcher

sealed class StationCachePolicy(string station, StationRule rule) : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        HttpRequest request = context.HttpContext.Request;
        bool enabled = rule.Enabled && (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
            && !context.HttpContext.User.Identities.Any(identity => identity.IsAuthenticated)
            && !request.Headers.ContainsKey("Authorization");
        context.EnableOutputCaching = enabled;
        context.AllowCacheLookup = enabled;
        context.AllowCacheStorage = enabled;
        context.AllowLocking = enabled;
        context.CacheVaryByRules.VaryByValues["station"] = station;
        context.CacheVaryByRules.VaryByValues["revision"] = rule.Revision.ToString(System.Globalization.CultureInfo.InvariantCulture);
        context.ResponseExpirationTimeSpan = TimeSpan.FromSeconds(30);
        return ValueTask.CompletedTask;
    }
    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        if (context.HttpContext.Response.StatusCode != StatusCodes.Status200OK || context.HttpContext.Response.Headers.ContainsKey("Set-Cookie"))
        {
            context.AllowCacheStorage = false;
        }
        return ValueTask.CompletedTask;
    }
}
// endsnippet:complete
