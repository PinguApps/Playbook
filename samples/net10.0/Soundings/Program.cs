// snippet:complete
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Soundings;

if (args.Length == 3 && args[0] == "--client")
{
    await Proof.RunAsync(new Uri(args[1]), args[2] == "11");
    return;
}

WebApplicationBuilder builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddSignalR(options => options.MaximumParallelInvocationsPerClient = 2);
builder.Services.AddSingleton<SoundingWork>();
await using WebApplication app = builder.Build();
app.MapHub<SoundingHub>("/soundings");
// Loopback-only proof controls; these are not application endpoints to deploy.
app.MapGet("/proof/{id}/started", async (string id, SoundingWork work) =>
{
    await work.Get(id).Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
    return "started";
});
app.MapGet("/proof/{id}/finished", async (string id, SoundingWork work) =>
    await work.Get(id).Finished.Task.WaitAsync(TimeSpan.FromSeconds(10)));
app.MapGet("/proof/{id}/state", (string id, SoundingWork work) =>
    work.Get(id).Finished.Task.IsCompleted ? work.Get(id).Finished.Task.Result : "running");
app.MapPost("/proof/{id}/release", (string id, SoundingWork work) => work.Get(id).Release.TrySetResult());
await app.StartAsync();
string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
Console.WriteLine($"Server: {RuntimeInformation.FrameworkDescription}; ASP.NET Core {typeof(SoundingHub).BaseType!.Assembly.GetName().Version}");
try
{
    if (args.Length == 2 && args[0] == "--serve")
    {
        await File.WriteAllTextAsync(args[1], address);
        using CancellationTokenSource lifetime = new(TimeSpan.FromMinutes(2));
        await app.WaitForShutdownAsync(lifetime.Token);
    }
    else
    {
        await Proof.RunAsync(new Uri(address), false);
    }
}
finally
{
    await app.StopAsync();
}
// endsnippet:complete
