// snippet:complete
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

namespace JettyAccess;

public sealed class ProbeAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    // Local test credential only. Never deploy a caller-controlled claim header.
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string permit = Request.Headers["X-Probe-Permit"].ToString();
        if (permit.Length == 0) return Task.FromResult(AuthenticateResult.NoResult());
        ClaimsPrincipal user = ProbeState.User(permit == "yes");
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, Scheme.Name)));
    }
}

public sealed class ProbeState : AuthenticationStateProvider
{
    public ClaimsPrincipal Current { get; set; } = User(false);
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(Current));
    public static ClaimsPrincipal User(bool allowed) => new(new ClaimsIdentity([new Claim("jetty", allowed ? "launch" : "view")], "local-probe"));
}

public static class Proof
{
    public static async Task RunAsync()
    {
        foreach (bool legacy in new[] { false, true })
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.Logging.ClearProviders();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddAuthentication("probe").AddScheme<AuthenticationSchemeOptions, ProbeAuthentication>("probe", _ => { });
            builder.Services.AddAuthorization(options => options.AddPolicy("crew-permit", p => p.RequireAuthenticatedUser().AddRequirements(new CrewPermit())));
            builder.Services.AddSingleton<IAuthorizationHandler, CrewPermitHandler>();
            builder.Services.AddControllers(options => options.EnableEndpointRouting = !legacy);
            builder.Services.AddSignalR();
            builder.Services.AddScoped<ProbeState>();
            builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<ProbeState>());
            builder.Services.AddCascadingAuthenticationState();
            await using WebApplication app = builder.Build();
            app.UseAuthentication();
            if (legacy)
            {
                app.UseMvc();
            }
            else
            {
                app.UseAuthorization();
                app.MapGet("/launch", () => "manifest granted").RequireAuthorization(new CrewPermitAttribute());
                app.MapControllers();
                app.MapHub<ManifestHub>("/radio");
            }

            await app.StartAsync();
            AuthorizationPolicy policy = (await PolicyMetadata.CombineAsync(app.Services.GetRequiredService<IAuthorizationPolicyProvider>()))!;
            Check(policy.Requirements.OfType<CrewPermit>().Count() == 1, "Dual-interface metadata must contribute the requirement once");
            string address = app.Urls.Single();
            using HttpClient client = new() { BaseAddress = new Uri(address) };
            foreach (bool allowed in new[] { false, true })
            {
                client.DefaultRequestHeaders.Remove("X-Probe-Permit");
                client.DefaultRequestHeaders.Add("X-Probe-Permit", allowed ? "yes" : "no");
                foreach (string path in legacy ? new[] { "/manifest", "/manifest/legacy-safe" } : new[] { "/launch", "/manifest" })
                {
                    using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative));
                    bool expected = allowed || (legacy && path == "/manifest" && NewMetadata);
                    Check(response.StatusCode == (expected ? HttpStatusCode.OK : HttpStatusCode.Forbidden), $"HTTP {legacy}/{path}/{allowed}: {response.StatusCode}");
                    Console.WriteLine($"HTTP legacy={legacy} {path} allowed={allowed}: {(int)response.StatusCode}");
                }

                if (!legacy)
                {
                    using IServiceScope renderScope = app.Services.CreateScope();
                    renderScope.ServiceProvider.GetRequiredService<ProbeState>().Current = ProbeState.User(allowed);
                    await using HtmlRenderer renderer = new(renderScope.ServiceProvider, renderScope.ServiceProvider.GetRequiredService<ILoggerFactory>());
                    foreach (bool route in new[] { false, true })
                    {
                        string html = await renderer.Dispatcher.InvokeAsync(async () =>
                        {
                            var rendered = await renderer.RenderComponentAsync<AccessViews>(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(AccessViews.Route)] = route }));
                            return rendered.ToHtmlString();
                        });
                        Check(html.Contains(allowed ? "manifest granted" : "Not authorized", StringComparison.Ordinal), $"View {route}/{allowed}: {html}");
                        Console.WriteLine($"RENDER route={route} allowed={allowed}: {html}");
                    }

                    using ClientWebSocket socket = new();
                    socket.Options.SetRequestHeader("X-Probe-Permit", allowed ? "yes" : "no");
                    using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
                    await socket.ConnectAsync(new Uri(address.Replace("http:", "ws:", StringComparison.Ordinal) + "/radio"), timeout.Token);
                    await SendAsync(socket, "{\"protocol\":\"json\",\"version\":1}", timeout.Token);
                    _ = await ReceiveAsync(socket, timeout.Token);
                    await SendAsync(socket, "{\"type\":1,\"invocationId\":\"proof\",\"target\":\"Read\",\"arguments\":[]}", timeout.Token);
                    string completion;
                    do { completion = await ReceiveAsync(socket, timeout.Token); } while (!completion.Contains("proof", StringComparison.Ordinal));
                    using JsonDocument message = JsonDocument.Parse(completion);
                    Check(allowed ? message.RootElement.GetProperty("result").GetString() == "manifest granted" : message.RootElement.TryGetProperty("error", out _), $"Hub: {completion}");
                    Console.WriteLine($"HUB allowed={allowed}: {completion}");
                }
            }

            if (!legacy)
            {
                client.DefaultRequestHeaders.Remove("X-Probe-Permit");
                using HttpResponseMessage anonymous = await client.GetAsync(new Uri("/launch", UriKind.Relative));
                Check(anonymous.StatusCode == HttpStatusCode.Unauthorized, "Anonymous must be challenged");
            }

            await app.StopAsync();
        }

        Console.WriteLine($"PASS: actual HTTP, SignalR invocation, AuthorizeView/AuthorizeRouteView and legacy MVC controls. Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
    }

    private static void Check(bool success, string message)
    {
        if (!success) throw new InvalidOperationException(message);
    }

    private static Task SendAsync(ClientWebSocket socket, string json, CancellationToken cancellation) => socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json + '\u001e')), WebSocketMessageType.Text, true, cancellation);

    private static async Task<string> ReceiveAsync(ClientWebSocket socket, CancellationToken cancellation)
    {
        byte[] buffer = new byte[4096];
        using MemoryStream stream = new();
        WebSocketReceiveResult received;
        do
        {
            received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation);
            await stream.WriteAsync(buffer.AsMemory(0, received.Count), cancellation);
        } while (!received.EndOfMessage);
        return Encoding.UTF8.GetString(stream.ToArray()).TrimEnd('\u001e');
    }

    private const bool NewMetadata = true;
}
// endsnippet:complete
