// snippet:complete
using System.Net;
using System.Text.Json.Nodes;

namespace SeedExchange;

internal static class RequestChecks
{
    public static async Task Run(WebApplication app, bool tokens, bool standard)
    {
        foreach (string[] invalid in new[] { Array.Empty<string>(), new[] { "*" }, new[] { "https://*.example.test" }, new[] { "https://garden.example.test/path" }, new[] { "http://garden.example.test" } })
        {
            try { _ = new ExchangeOrigins(invalid); }
            catch (InvalidOperationException) { continue; }
            throw new InvalidOperationException("Unsafe origin configuration accepted");
        }
        using HttpClient client = new(new HttpClientHandler { CookieContainer = new CookieContainer(), CheckCertificateRevocationList = true }) { BaseAddress = new(app.Urls.Single()) };
        string? token = null;
        if (tokens)
        {
            string form = await client.GetStringAsync(new Uri("/form", UriKind.Relative));
            token = System.Text.RegularExpressions.Regex.Match(form, "name=__RequestVerificationToken value='([^']+)'").Groups[1].Value;
        }
        async Task Post(string id, string path, string? origin, int expected, bool omitToken = false)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, path + "?case=" + id);
            if (origin is not null) { request.Headers.Add("Origin", origin); }
            Dictionary<string, string> fields = new() { ["seed"] = "Fern" };
            if (token is not null && !omitToken) { fields["__RequestVerificationToken"] = token; }
            request.Content = new FormUrlEncodedContent(fields);
            using HttpResponseMessage response = await client.SendAsync(request);
            if ((int)response.StatusCode != expected) { throw new InvalidOperationException($"{id}: expected {expected}, got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}"); }
            JsonNode entry = JsonNode.Parse(await client.GetStringAsync(new Uri("/observations", UriKind.Relative)))![id]!;
            int handlers = entry["handlerCalls"]!.GetValue<int>();
            if (handlers != (expected == 200 ? 1 : 0)) { throw new InvalidOperationException(id + ": handler count"); }
            if (!tokens && !standard && entry["providerCalls"]!.GetValue<int>() != 1) { throw new InvalidOperationException(id + ": DI provider not invoked exactly once"); }
            Console.WriteLine($"PASS HTTP {id}: {expected}, handlers={handlers}, provider={entry["providerCalls"]}");
        }
        await Post("own", "/seed", standard ? client.BaseAddress!.GetLeftPart(UriPartial.Authority) : "http://localhost:6180", 200);
        await Post("partner", "/seed", "http://localhost:6181", tokens || standard ? (standard ? 400 : 200) : 200);
        await Post("hostile", "/seed", "http://127.0.0.1:6181", 400);
        await Post("missing", "/seed", null, 400);
        await Post("opaque", "/seed", "null", 400);
        await Post("wildcard", "/wildcard", "http://localhost:6181", standard ? 400 : 200);
        await Post("public", "/public", "http://localhost:6181", standard ? 400 : 200);
        await Post("credentialed", "/credentialed", "http://localhost:6181", 200);
        if (tokens) { await Post("no-token", "/seed", "http://localhost:6180", 400, true); }
        using HttpResponseMessage json = await client.PostAsync(new Uri("/json?case=json", UriKind.Relative), new StringContent("{\"seed\":\"Moss\"}", System.Text.Encoding.UTF8, "application/json"));
        if (json.StatusCode != HttpStatusCode.OK) { throw new InvalidOperationException("JSON boundary"); }
        JsonNode jsonEntry = JsonNode.Parse(await client.GetStringAsync(new Uri("/observations", UriKind.Relative)))!["json"]!;
        if (jsonEntry["handlerCalls"]!.GetValue<int>() != 1 || jsonEntry["providerCalls"]!.GetValue<int>() != 0) { throw new InvalidOperationException("JSON unexpectedly protected"); }
        foreach (var assembly in new[] { typeof(HttpContext).Assembly, typeof(WebApplication).Assembly, typeof(object).Assembly })
        {
            Console.WriteLine($"ARTIFACT {assembly.Location} | {assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).Cast<System.Reflection.AssemblyInformationalVersionAttribute>().Single().InformationalVersion} | sha256={Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(assembly.Location)))}");
        }
        Console.WriteLine("CONTRACT ICsrfProtection=" + (typeof(HttpContext).Assembly.GetType("Microsoft.AspNetCore.Antiforgery.ICsrfProtection") is not null));
        Console.WriteLine("PASS JSON unchanged; actual framework=" + typeof(HttpContext).Assembly.GetName().Version + "; runtime=" + Environment.Version);
    }
}
// endsnippet:complete
