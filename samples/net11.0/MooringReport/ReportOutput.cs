// snippet:output
namespace MooringReport;

public static class ReportOutput
{
    public static async Task WriteAsync(TextWriter writer, string text, CancellationToken token)
    {
        await writer.WriteAsync(text, token);
        await writer.WriteLineAsync(" | ready", token);
        await writer.WriteLineAsync(token);
    }
}
// endsnippet:output
