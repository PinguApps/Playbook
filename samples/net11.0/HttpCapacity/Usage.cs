namespace Playbook.Samples;

public static class Usage
{
    // snippet:upload-use
    public static async Task UploadAsync(Uri uploadUri, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        using HttpContent content = RequestPolicy.Compress("sensor 47: temperature=18", "gzip");
        using HttpResponseMessage response = await client.PostAsync(uploadUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
    // endsnippet:upload-use

    // snippet:retirement-use
    public static async Task RetireAsync(Uri serviceUri, CancellationToken cancellationToken)
    {
        int retirementRequested = 0;
        using var handler = ConnectionPolicy.Create(() => Volatile.Read(ref retirementRequested) == 1);
        using var client = new HttpClient(handler);
        using HttpResponseMessage before = await client.GetAsync(serviceUri, cancellationToken);
        before.EnsureSuccessStatusCode();
        // A separate policy update marks the existing pool for retirement.
        Volatile.Write(ref retirementRequested, 1);
        using HttpResponseMessage after = await client.GetAsync(serviceUri, cancellationToken);
        after.EnsureSuccessStatusCode();
    }
    // endsnippet:retirement-use

    // snippet:capacity-use
    public static async Task ReadAsync(Uri serviceUri, CancellationToken cancellationToken)
    {
        using var handler = StreamPolicy.Create();
        using var client = new HttpClient(handler);
        using var admission = new SemaphoreSlim(2);
        await StreamPolicy.SendAsync(client, serviceUri, admission, cancellationToken);
    }
    // endsnippet:capacity-use
}
