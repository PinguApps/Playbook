// snippet:complete
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
// snippet:registration
builder.Services.AddRateLimiter(options =>
{
    // One process-wide partition preserves the old middleware's shared capacity.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetConcurrencyLimiter("dock", _ => new ConcurrencyLimiterOptions
        {
            PermitLimit = 1, QueueLimit = 1, QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        }));
    options.RejectionStatusCode = StatusCodes.Status503ServiceUnavailable;
});
// endsnippet:registration
await using var app = builder.Build();
app.UseRateLimiter();
var gate = new DockGate();
app.Run(async context =>
{
    gate.Enter();
    try
    {
        gate.FirstEntered.TrySetResult();
        await gate.Release.Task.WaitAsync(context.RequestAborted);
        await context.Response.WriteAsync("slot completed", context.RequestAborted);
    }
    finally { gate.Exit(); }
});
await app.StartAsync();
string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var handler = new SocketsHttpHandler { MaxConnectionsPerServer = 4, UseProxy = false };
using var client = new HttpClient(handler) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
Task<HttpResponseMessage> first = client.GetAsync(new Uri("/work", UriKind.Relative));
try
{
    await gate.FirstEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
    Task<HttpResponseMessage> second = client.GetAsync(new Uri("/work", UriKind.Relative));
    Task<HttpResponseMessage> third = client.GetAsync(new Uri("/work", UriKind.Relative));
    Task<HttpResponseMessage> rejected = await Task.WhenAny(second, third).WaitAsync(TimeSpan.FromSeconds(5));
    using HttpResponseMessage rejection = await rejected;
    if (rejection.StatusCode != HttpStatusCode.ServiceUnavailable || gate.Total != 1) { throw new Exception("Saturation did not reject while one request occupied the permit."); }
    gate.Release.TrySetResult();
    Task<HttpResponseMessage> queued = ReferenceEquals(rejected, second) ? third : second;
    using HttpResponseMessage admittedFirst = await first;
    using HttpResponseMessage admittedQueued = await queued;
    if (admittedFirst.StatusCode != HttpStatusCode.OK || admittedQueued.StatusCode != HttpStatusCode.OK
        || await admittedQueued.Content.ReadAsStringAsync() != "slot completed" || gate.Total != 2 || gate.Maximum != 1)
    {
        throw new Exception("Queued request was lost or the concurrency permit was exceeded.");
    }
    using HttpResponseMessage recovered = await client.GetAsync(new Uri("/work", UriKind.Relative));
    if (recovered.StatusCode != HttpStatusCode.OK || gate.Total != 3 || gate.Maximum != 1) { throw new Exception("Permit was not released after completion."); }
}
finally
{
    gate.Release.TrySetResult();
    await app.StopAsync();
}
Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: 4 real requests; 1 executing + 1 queued + 1 rejected with 503, then recovery; maximum active=1. Global FIFO concurrency limiter, no timer-based request quota.");

sealed class DockGate
{
    private int _active;
    private int _total;
    private int _maximum;
    public TaskCompletionSource FirstEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int Total => Volatile.Read(ref _total);
    public int Maximum => Volatile.Read(ref _maximum);
    public void Enter()
    {
        Interlocked.Increment(ref _total);
        int active = Interlocked.Increment(ref _active);
        int maximum;
        do { maximum = Volatile.Read(ref _maximum); }
        while (active > maximum && Interlocked.CompareExchange(ref _maximum, active, maximum) != maximum);
    }
    public void Exit() => Interlocked.Decrement(ref _active);
}
// endsnippet:complete
