using System.Text.Json;

namespace Playbook.Samples;

// snippet:streaming
public sealed record ParcelScan(string Depot, int Sequence);

public static class ScanFeed
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, NewLine = "\r\n" };
    public static Task WriteAsync(Stream destination, IAsyncEnumerable<ParcelScan> scans, CancellationToken cancellationToken) =>
        JsonSerializer.SerializeAsyncEnumerable(destination, scans, topLevelValues: true,
            options: Options,
            cancellationToken: cancellationToken);

    public static IAsyncEnumerable<ParcelScan?> ReadAsync(Stream source, CancellationToken cancellationToken) =>
        JsonSerializer.DeserializeAsyncEnumerable<ParcelScan>(source, topLevelValues: true, cancellationToken: cancellationToken);
}
// endsnippet:streaming

// snippet:writer
public static class LabelDocuments
{
    public static void Write(Stream first, Stream second)
    {
        using Utf8JsonWriter writer = new(first, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("depot", "Cwm");
        writer.WriteEndObject();
        writer.Flush();
        writer.Reset(second, new JsonWriterOptions { Indented = false });
        writer.WriteStartObject();
        writer.WriteString("depot", "Bay");
        writer.WriteEndObject();
        writer.Flush();
    }
}
// endsnippet:writer
