// snippet:read-report
namespace Playbook.Samples;

internal static class ReportReader
{
    // The caller owns/reuses the configured client and supplies an overall cancellation budget.
    public static async Task<byte[]> ReadAsync(HttpClient client, Uri address, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentEncoding.Count != 0)
            throw new NotSupportedException("Report still has an unsupported or stacked content encoding.");
        await using Stream body = await response.Content.ReadAsStreamAsync(cancellationToken);
        using MemoryStream result = new();
        byte[] buffer = new byte[4096];
        int count;
        while ((count = await body.ReadAsync(buffer, cancellationToken)) != 0)
        {
            if (result.Length + count > 65536) throw new InvalidDataException("Decoded report exceeds 64 KiB.");
            await result.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
        }
        return result.ToArray();
    }
}
// endsnippet:read-report
