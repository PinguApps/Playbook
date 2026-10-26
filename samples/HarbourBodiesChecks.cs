// snippet:complete
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace HarbourBodies;

internal static class BodyChecks
{
    public static async Task RunAsync(WebApplication app)
    {
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient client = new() { BaseAddress = new Uri(address) };
            string[] valid = ["""{"lockerCode":"B7"}""", """{"streetAddress":"4 Gull Lane"}"""];
            string[] invalid = ["{}", """{"other":"B7"}""", """{"lockerCode":"B7","streetAddress":"4 Gull Lane"}""", """{"lockerCode":12}""", "true"];
            foreach (string route in new[] { "/minimal", "/mvc" })
            {
                foreach (string json in valid.Concat(invalid))
                {
                    using StringContent content = new(json, Encoding.UTF8, "application/json");
                    using HttpResponseMessage response = await client.PostAsync(new Uri(route, UriKind.Relative), content);
                    HttpStatusCode expected = valid.Contains(json) ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
                    if (response.StatusCode != expected) { throw new InvalidOperationException($"{route} {json}: {response.StatusCode}"); }
                    if (expected == HttpStatusCode.OK)
                    {
                        using JsonDocument actual = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        using JsonDocument input = JsonDocument.Parse(json);
                        if (!JsonElement.DeepEquals(input.RootElement, actual.RootElement)) { throw new InvalidOperationException("Wire shape changed."); }
                    }
                }
            }
            foreach (string json in valid.Concat(invalid))
            {
                using ClientWebSocket socket = new();
                await socket.ConnectAsync(new Uri(address.Replace("http:", "ws:", StringComparison.Ordinal) + "/deliveries"), CancellationToken.None);
                await SendAsync(socket, """{"protocol":"json","version":1}""");
                using JsonDocument handshake = JsonDocument.Parse(await ReceiveAsync(socket));
                if (handshake.RootElement.EnumerateObject().Any()) { throw new InvalidOperationException("SignalR handshake failed."); }
                await SendAsync(socket, """{"type":1,"invocationId":"1","target":"Echo","arguments":[""" + json + "]}");
                using JsonDocument completion = JsonDocument.Parse(await ReceiveAsync(socket));
                JsonElement root = completion.RootElement;
                if (root.GetProperty("type").GetInt32() != 3) { throw new InvalidOperationException("Expected completion."); }
                if (valid.Contains(json))
                {
                    using JsonDocument input = JsonDocument.Parse(json);
                    if (!JsonElement.DeepEquals(input.RootElement, root.GetProperty("result"))) { throw new InvalidOperationException("Hub result shape changed."); }
                }
                else if (!root.TryGetProperty("error", out _)) { throw new InvalidOperationException("Invalid hub argument accepted."); }
            }
            Console.WriteLine("PASS Minimal API, MVC and real SignalR JSON WebSocket: both delivery shapes round-trip; ambiguous, unknown, empty, wrong-value and non-object payloads rejected.");
        }
        finally { await app.StopAsync(); await app.DisposeAsync(); }
    }

    private static Task SendAsync(ClientWebSocket socket, string json) =>
        socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json + '\u001e')), WebSocketMessageType.Text, true, CancellationToken.None);

    private static async Task<string> ReceiveAsync(ClientWebSocket socket)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(15));
        byte[] buffer = new byte[8192];
        StringBuilder frame = new();
        while (true)
        {
            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), timeout.Token);
            if (result.MessageType == WebSocketMessageType.Close) { throw new InvalidOperationException("Unexpected socket close."); }
            frame.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            string text = frame.ToString();
            int separator = text.IndexOf('\u001e', StringComparison.Ordinal);
            if (separator >= 0) { return text[..separator]; }
        }
    }
}
// endsnippet:complete
