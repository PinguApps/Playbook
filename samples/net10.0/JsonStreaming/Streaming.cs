using System.Text;
using System.Text.Json;

namespace Playbook.Samples;

// snippet:streaming
public sealed record ParcelScan(string Depot, int Sequence);

public static class ScanFeed
{
    public static async Task WriteAsync(Stream destination, IAsyncEnumerable<ParcelScan> scans, CancellationToken cancellationToken)
    {
        // One compact JSON value plus LF at a time; retain destination ownership.
        await foreach (ParcelScan scan in scans.WithCancellation(cancellationToken))
        {
            byte[] record = JsonSerializer.SerializeToUtf8Bytes(scan);
            await destination.WriteAsync(record, cancellationToken);
            await destination.WriteAsync("\n"u8.ToArray(), cancellationToken);
            await destination.FlushAsync(cancellationToken);
        }
    }

    public static IAsyncEnumerable<ParcelScan?> ReadAsync(Stream source, CancellationToken cancellationToken) =>
        JsonSerializer.DeserializeAsyncEnumerable<ParcelScan>(source, topLevelValues: true, cancellationToken: cancellationToken);
}
// endsnippet:streaming

// snippet:writer
public static class LabelDocuments
{
    public static void Write(Stream first, Stream second)
    {
        // .NET 10 Reset can change the target but retains the original options.
        using (Utf8JsonWriter writer = new(first, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("depot", "Cwm");
            writer.WriteEndObject();
            writer.Flush();
        }
        using (Utf8JsonWriter writer = new(second, new JsonWriterOptions { Indented = false }))
        {
            writer.WriteStartObject();
            writer.WriteString("depot", "Bay");
            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
// endsnippet:writer
