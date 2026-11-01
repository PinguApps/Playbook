// snippet:endpoint-text
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Playbook.Samples;

public static class EndpointText
{
    // Parsing still creates an IPEndPoint/IPAddress; this is not allocation-free.
    public static bool TryRead(ReadOnlySpan<byte> field, [NotNullWhen(true)] out IPEndPoint? endpoint) =>
        IPEndPoint.TryParse(field, out endpoint);

    public static bool TryWrite(IPEndPoint endpoint, Span<byte> field, out int written) =>
        endpoint.TryFormat(field, out written);
}
// endsnippet:endpoint-text
