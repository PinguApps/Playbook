// snippet:complete
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

RuntimeIdentity.Verify();
await Run("raw");
await Run("preserve");
await Run("decode");

static async Task Run(string mode)
{
    var builder = WebApplication.CreateBuilder();
    builder.Logging.ClearProviders();
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    await using var app = builder.Build();
    app.Use(async (context, next) =>
    {
        context.Items["parsedPath"] = context.Request.Path.Value;
        // Deliberately retained failure control: decoding the WHOLE path changes route boundaries.
        // Never enable this in the recommended host. No decoded value is used for authorization.
        if (mode == "decode")
            context.Request.Path = Uri.UnescapeDataString(context.Request.Path.Value!);
        #if NET10_0
        // Bounded old-server alternative. Only restore this fixture's ASCII identifier shape,
        // from RawTarget (never Uri.AbsolutePath, which has already lost encoded slashes).
        var raw = context.Features.Get<IHttpRequestFeature>()!.RawTarget;
        if (mode == "preserve" && raw.StartsWith("http://", StringComparison.Ordinal))
        {
            var start = raw.IndexOf('/', "http://".Length);
            var path = start < 0 ? "" : raw[start..];
            if (System.Text.RegularExpressions.Regex.IsMatch(path, @"^/birds/[A-Za-z]+%2[Ff][0-9]+$"))
                context.Request.Path = new PathString(path);
        }
#endif
        context.Items["middlewarePath"] = context.Request.Path.Value;
        await next(context);
    });
    app.UseRouting();
    app.Use(async (context, next) =>
    {
        context.Items["selectedRoute"] = context.GetEndpoint()?.DisplayName;
        await next(context);
    });
    app.MapGet("/birds/{id}", (HttpContext context) => Reply(context, "single"))
        .WithDisplayName("bird-record");
    app.MapGet("/birds/{region}/{tag}", (HttpContext context) => Reply(context, "split"))
        .WithDisplayName("regional-record");
    await app.StartAsync();
    try
    {
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!
            .Addresses.Single();
        var port = new Uri(address).Port;
        foreach (var suffix in new[] { "reed%2F17", "reed%2f17", "reed/17", "REED%2F17", "reed%252F17", "reed%", "reed%GG", "reed%00", "reed%0D%0A" })
        {
            var origin = await Send(port, "/birds/" + suffix);
            var absolute = await Send(port, address + "/birds/" + suffix);
            Console.WriteLine(JsonSerializer.Serialize(new { mode, suffix, origin, absolute }));
            if (!suffix.Contains("%00", StringComparison.Ordinal)) Check(origin.Status == absolute.Status, "paired status");
            if (origin.Status == 200 && absolute.Status == 200)
            {
                using var first = JsonDocument.Parse(origin.Body);
                using var second = JsonDocument.Parse(absolute.Body);
                                var encoded = suffix.Contains("%2F", StringComparison.OrdinalIgnoreCase);
#if NET10_0
                var pairExpected = mode == "decode" || (mode == "preserve" && encoded) || !encoded;
#else
                var pairExpected = true;
#endif
                if (pairExpected && (encoded || suffix == "reed/17"))
                    foreach (var property in new[] { "middlewarePath", "selectedRoute", "handler", "values" })
                        Check(first.RootElement.GetProperty(property).GetRawText() == second.RootElement.GetProperty(property).GetRawText(), "paired " + property);
#if NET10_0
                if (mode == "raw" && encoded)
                    Check(second.RootElement.GetProperty("handler").GetString() == "split", "actual old absolute-form decoding");
#endif
                Check(first.RootElement.GetProperty("rawTarget").GetString() == "/birds/" + suffix, "origin bytes retained");
                Check(second.RootElement.GetProperty("rawTarget").GetString() == address + "/birds/" + suffix, "absolute bytes retained");
                if (suffix.Equals("reed%2F17", StringComparison.OrdinalIgnoreCase))
                    Check(first.RootElement.GetProperty("handler").GetString() == (mode == "decode" ? "split" : "single"), "decoding selects different handler");
                if (suffix == "reed/17")
                    Check(first.RootElement.GetProperty("handler").GetString() == "split", "ordinary slash is a route boundary");
            }
        }
        // Invalid request-line controls must never execute a handler.
        foreach (var target in new[] { "/birds/reed 17", address + "/birds/reed 17" })
            Check((await Send(port, target)).Status == 400, "malformed request line rejected");
        Console.WriteLine($"PASS BirdRoutes mode={mode}; runtime={Environment.Version}; framework={typeof(WebApplication).Assembly.Location}");
    }
    finally { await app.StopAsync(); }
}

static IResult Reply(HttpContext context, string handler) => Results.Text(JsonSerializer.Serialize(new
{
    rawTarget = context.Features.Get<IHttpRequestFeature>()!.RawTarget,
    parsedPath = context.Items["parsedPath"],
    middlewarePath = context.Items["middlewarePath"],
    selectedRoute = context.Items["selectedRoute"],
    handler,
    values = context.Request.RouteValues.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString())
}), "application/json");

static async Task<WireResponse> Send(int port, string target)
{
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, port, deadline.Token);
    await using var stream = client.GetStream();
    // No Uri/HttpClient normalization: these exact ASCII bytes are the HTTP/1.1 request line.
    var request = Encoding.ASCII.GetBytes($"GET {target} HTTP/1.1\r\nHost: 127.0.0.1:{port}\r\nConnection: close\r\n\r\n");
    await stream.WriteAsync(request, deadline.Token);
    using var reader = new StreamReader(stream, Encoding.UTF8);
    var response = await reader.ReadToEndAsync(deadline.Token);
    var separator = response.IndexOf("\r\n\r\n", StringComparison.Ordinal);
    Check(separator > 0, "complete HTTP response");
    return new(int.Parse(response.Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture), response[(separator + 4)..]);
}

static void Check(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}
internal sealed record WireResponse(int Status, string Body);
// endsnippet:complete
