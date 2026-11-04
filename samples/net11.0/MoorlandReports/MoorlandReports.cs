// snippet:moorland-async
internal static class MoorlandReports
{
    public static async Task<string> ReadAsync(HttpClient client, Uri endpoint, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
// endsnippet:moorland-async
