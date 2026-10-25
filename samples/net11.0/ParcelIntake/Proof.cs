// snippet:complete
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace ParcelIntake;

public static class Proof
{
    public static async Task RunAsync()
    {
        foreach (bool throws in new[] { false, true })
        {
            var counts = new Counters();
            await using var app = Intake.Create(counts, throws);
            await app.StartAsync();
            using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
            if (throws)
            {
                using var rejected = await client.GetAsync(new Uri("/intake/letters?slots=3", UriKind.Relative));
                Check(rejected.StatusCode == HttpStatusCode.BadRequest && counts.Filters == 0 && counts.Shaped == 0 && counts.Handlers == 0,
                    "ThrowOnBadRequest=true must bypass the filter and shaper");
                Console.WriteLine("Development throw control: 400, filter=0, shaped=0, handler=0");
                continue;
            }

            foreach (string path in new[] { "/intake/letters?slots=3", "/intake/8?slots=many", "/intake/8" })
            {
                using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
                string body = await response.Content.ReadAsStringAsync();
                using var problem = JsonDocument.Parse(body);
                Check(response.StatusCode == HttpStatusCode.BadRequest && response.Content.Headers.ContentType?.MediaType == "application/problem+json", "Expected shaped 400");
                Check(problem.RootElement.GetProperty("code").GetString() == "intake_binding_failed" && problem.RootElement.GetProperty("status").GetInt32() == 400, "Wrong error contract");
                Check(counts.Handlers == 0, "Failed binding must not execute handler");
                Console.WriteLine($"{path}: 400 intake_binding_failed, handler=0");
            }

            Check(counts.Filters == 3 && counts.Shaped == 3, "Every bad scalar binding must reach the new filter");
            using var good = await client.GetAsync(new Uri("/intake/8?slots=3", UriKind.Relative));
            var payload = await good.Content.ReadFromJsonAsync<JsonElement>();
            Check(good.IsSuccessStatusCode && payload.GetProperty("dock").GetInt32() == 8 && payload.GetProperty("slots").GetInt32() == 3 && counts.Handlers == 1, "Valid arguments must reach handler once");
            using var domain = await client.GetAsync(new Uri("/intake/8?slots=-1", UriKind.Relative));
            Check(domain.StatusCode == HttpStatusCode.BadRequest && (await domain.Content.ReadAsStringAsync()).Contains("capacity_closed", StringComparison.Ordinal) && counts.Shaped == 3 && counts.Handlers == 2, "Do not replace a handler's domain error");
            using var malformed = await client.PostAsync(new Uri("/notes", UriKind.Relative), new StringContent("{", Encoding.UTF8, "application/json"));
            Check(malformed.StatusCode == HttpStatusCode.BadRequest && counts.BodyFilters == 0 && counts.BodyHandlers == 0, "Malformed JSON is an early read failure, not a filter-observed scalar failure");
            Check(string.IsNullOrEmpty(await malformed.Content.ReadAsStringAsync()), "Control has no ProblemDetails service or status pages");
            using var wrongType = await client.PostAsync(new Uri("/notes", UriKind.Relative), new StringContent("label", Encoding.UTF8, "text/plain"));
            Check(wrongType.StatusCode == HttpStatusCode.UnsupportedMediaType && counts.BodyFilters == 0 && counts.BodyHandlers == 0, "Unsupported media type must remain 415 before filter");
            Console.WriteLine("Valid: 200; domain error preserved; malformed JSON: empty 400/filter=0; wrong media: 415/filter=0");
        }

        Console.WriteLine($"PASS: ParcelIntake real Kestrel requests on {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException(message); }
    }
}
// endsnippet:complete
