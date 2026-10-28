// snippet:complete
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace MossObservatory;

internal static class MossRequestChecks
{
    internal static async Task RunAsync(WebApplication app, Func<int> handlerCalls)
    {
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false }) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
            using (var invalid = await client.PostAsJsonAsync(new Uri("/watches", UriKind.Relative), new { station = "", observers = 0 }))
            {
                if (invalid.StatusCode != HttpStatusCode.BadRequest || handlerCalls() != 0) { throw new Exception("Invalid data reached the handler."); }
                using var body = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync());
                var errors = body.RootElement.GetProperty("errors");
                if (!errors.TryGetProperty("Station", out _) || !errors.TryGetProperty("Observers", out _)) { throw new Exception("Validation problem lost field errors."); }
            }
            using (var malformed = await client.PostAsync(new Uri("/watches", UriKind.Relative), new StringContent("{", Encoding.UTF8, "application/json")))
            {
                if (malformed.StatusCode != HttpStatusCode.BadRequest || handlerCalls() != 0) { throw new Exception("Malformed JSON reached the handler."); }
            }
            using (var accepted = await client.PostAsJsonAsync(new Uri("/watches", UriKind.Relative), new { station = "north", observers = 2 }))
            {
                if (accepted.StatusCode != HttpStatusCode.OK || handlerCalls() != 1) { throw new Exception("Valid request did not execute exactly once."); }
                using var body = JsonDocument.Parse(await accepted.Content.ReadAsStringAsync());
                if (body.RootElement.GetProperty("accepted").GetString() != "north") { throw new Exception("Wrong accepted station."); }
            }
            using (var business = await client.PostAsJsonAsync(new Uri("/watches", UriKind.Relative), new { station = "closed", observers = 2 }))
            {
                if (business.StatusCode != HttpStatusCode.Conflict || handlerCalls() != 2) { throw new Exception("Business validation was confused with model validation."); }
            }
            Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: 4 real HTTP requests; invalid fields and malformed JSON return 400 with zero handler calls; valid request executes once; business conflict is separate. No mocks or async validation.");
        }
        finally { await app.StopAsync(); }
    }
}
// endsnippet:complete
