// snippet:complete
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Trace;

bool suppress = args.Contains("--suppress", StringComparer.Ordinal);
// snippet:switch
if (suppress)
{
    AppContext.SetSwitch("Microsoft.AspNetCore.Hosting.SuppressActivityOpenTelemetryData", true);
}
// endsnippet:switch
var exported = new ConcurrentQueue<Activity>();
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddControllers();
// snippet:pipeline
using var provider = Sdk.CreateTracerProviderBuilder()
    .AddSource("Microsoft.AspNetCore")
    .AddProcessor(new LanternEnricher())
    .AddProcessor(new SimpleActivityExportProcessor(new LanternExporter(exported)))
    .Build();
// endsnippet:pipeline
await using var app = builder.Build();
app.MapControllerRoute("station", "{controller=Station}/{action=Index}/{id?}");
await app.StartAsync();
string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var client = new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
foreach (string path in new[] { "/Station/Index/7", "/Station/Fault/8" })
{
    using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
    if ((int)response.StatusCode != (path.Contains("Fault", StringComparison.Ordinal) ? 500 : 200)) { throw new Exception("Wrong response."); }
}
await app.StopAsync();
provider.ForceFlush();
var activities = exported.ToArray();
if (activities.Length != 2 || activities.Select(activity => activity.SpanId).Distinct().Count() != 2) { throw new Exception("Expected exactly one exported server span per request."); }
foreach (var activity in activities)
{
    bool fault = activity.GetTagItem("lantern.operation")?.ToString() == "fault";
    if (activity.Source.Name != "Microsoft.AspNetCore" || activity.OperationName != "Microsoft.AspNetCore.Hosting.HttpRequestIn" || activity.Kind != ActivityKind.Server) { throw new Exception("Wrong native source."); }
    if (activity.GetTagItem("lantern.station")?.ToString() != "north") { throw new Exception("Domain enrichment missing."); }
    if (!suppress && activity.GetTagItem("http.request.method")?.ToString() != "GET") { throw new Exception("Native HTTP tag missing."); }
    if (suppress && activity.GetTagItem("http.request.method") is not null) { throw new Exception("Suppression failed."); }
    string expectedRoute = fault ? "Station/Fault/{id?}" : "Station/Index/{id?}";
    if (activity.GetTagItem("http.route")?.ToString() != (suppress ? null : expectedRoute)) { throw new Exception($"Route mismatch: {activity.GetTagItem("http.route")}"); }
    if (fault && (activity.Status != (suppress ? ActivityStatusCode.Unset : ActivityStatusCode.Error) || activity.StatusDescription is not null)) { throw new Exception("Error status/description contract changed."); }
    if (!suppress && activity.GetTagItem("http.response.status_code")?.ToString() != (fault ? "500" : "200")) { throw new Exception("Native response tag missing."); }
    if (!suppress && fault && activity.GetTagItem("error.type")?.ToString() != "System.InvalidOperationException") { throw new Exception("Exception type missing."); }
    if (fault && activity.Events.Any()) { throw new Exception("Reconcile native RC2 exception-event behavior before updating this evidence."); }
    Console.WriteLine($"SPAN source={activity.Source.Name} route={activity.GetTagItem("http.route")} status={activity.Status} description={activity.StatusDescription ?? "<null>"} method={activity.GetTagItem("http.request.method") ?? "<suppressed>"} response={activity.GetTagItem("http.response.status_code") ?? "<suppressed>"}");
}
Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: two real requests, two exported native server spans; suppress={suppress}; domain enrichment only; no AspNetCore instrumentation dependency.");
if (!suppress)
{
    var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false };
    start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
    start.ArgumentList.Add("--suppress");
    start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
    using var child = Process.Start(start)!;
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    try { await child.WaitForExitAsync(timeout.Token); }
    finally { if (!child.HasExited) { child.Kill(true); } }
    if (child.ExitCode != 0) { throw new Exception("Fresh-process suppression check failed."); }
}

// snippet:enricher
sealed class LanternEnricher : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        // Keep application facts; let the framework own HTTP route/status/exception attributes.
        activity.SetTag("lantern.station", "north");
    }
}
sealed class LanternExporter(ConcurrentQueue<Activity> destination) : BaseExporter<Activity>
{
    public override ExportResult Export(in Batch<Activity> batch)
    {
        foreach (Activity activity in batch) { destination.Enqueue(activity); }
        return ExportResult.Success;
    }
}
// endsnippet:enricher
namespace LanternTraces
{
public sealed class StationController : ControllerBase
{
    public string Index(int id) { Activity.Current?.SetTag("lantern.operation", "read"); return $"Lantern {id}"; }
    public string Fault(int id) { Activity.Current?.SetTag("lantern.operation", "fault"); throw new InvalidOperationException($"Lantern {id} unavailable"); }
}
}
// endsnippet:complete
