// snippet:complete
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SignalCabin;

public static class Proof
{
    public static async Task RunAsync()
    {
        await using var app = Signals.Create();
        int unsafeHandlers = 0;
        // Deliberately invalid controls belong only to this proof.
        app.MapGet("/unsafe/auth", [ShortCircuit] () => Interlocked.Increment(ref unsafeHandlers)).RequireAuthorization("Crew");
        app.MapGet("/unsafe/cors", [ShortCircuit] () => Interlocked.Increment(ref unsafeHandlers)).RequireCors("Chart");
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };

        foreach (string path in new[] { "/signal/standalone", "/signal/convention", "/signal/ordinary" })
        {
            using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
            int status = path.EndsWith("convention", StringComparison.Ordinal) ? 202 : 200;
            Check((int)response.StatusCode == status && await response.Content.ReadAsStringAsync() == "Cabin open", "Endpoint must still execute and produce its response");
            Check(response.Headers.Contains("X-Cabin-Before"), "Before-routing middleware must always run");
            Check(response.Headers.Contains("X-Cabin-After") == path.EndsWith("ordinary", StringComparison.Ordinal), "Only standalone endpoints bypass after-routing middleware");
            Console.WriteLine($"{path}: {status}, before=visited, after={response.Headers.Contains("X-Cabin-After")}");
        }

        foreach (var (crew, status) in new[] { ("", HttpStatusCode.Unauthorized), ("no", HttpStatusCode.Forbidden), ("yes", HttpStatusCode.OK) })
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/crew/manifest", UriKind.Relative));
            if (crew.Length > 0) { request.Headers.Add("X-Test-Crew", crew); }
            using var response = await client.SendAsync(request);
            Check(response.StatusCode == status && response.Headers.Contains("X-Cabin-After"), "Protected endpoint must retain its middleware and policy");
            Console.WriteLine($"crew={crew}: {(int)status}, after=visited");
        }

        using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/chart", UriKind.Relative)))
        {
            request.Headers.Add("Origin", "https://chart.example");
            using var response = await client.SendAsync(request);
            Check(response.IsSuccessStatusCode && response.Headers.GetValues("Access-Control-Allow-Origin").Single() == "https://chart.example" && response.Headers.Contains("X-Cabin-After"), "Ordinary CORS endpoint must retain its policy");
        }

        foreach (string path in new[] { "/unsafe/auth", "/unsafe/cors" })
        {
            using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
            string body = await response.Content.ReadAsStringAsync();
            Check(response.StatusCode == HttpStatusCode.InternalServerError && body.Contains("InvalidOperationException", StringComparison.Ordinal) && body.Contains("short circuit", StringComparison.OrdinalIgnoreCase), "Framework must reject conflicting security metadata");
            Check(Volatile.Read(ref unsafeHandlers) == 0 && !response.Headers.Contains("X-Cabin-After"), "Unsafe endpoint must never execute");
            Console.WriteLine($"{path}: 500 InvalidOperationException, unsafeHandlers=0");
        }

        Console.WriteLine($"PASS: SignalCabin real Kestrel routing/authentication/authorization/CORS on {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException(message); }
    }
}
// endsnippet:complete
