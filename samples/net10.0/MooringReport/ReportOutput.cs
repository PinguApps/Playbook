// snippet:output
namespace MooringReport;

public static class ReportOutput
{
    public static async Task WriteAsync(TextWriter writer, string text, CancellationToken token)
    {
        await writer.WriteAsync(text.AsMemory(), token);
        await writer.WriteLineAsync(" | ready".AsMemory(), token);
        await writer.WriteLineAsync(ReadOnlyMemory<char>.Empty, token);
    }
}
// endsnippet:output
