#region endpoint-text
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace Playbook.Samples;

public static class EndpointText
{
    // The ledger stores numeric endpoints, not hostnames or URIs.
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static bool TryRead(ReadOnlySpan<byte> field, [NotNullWhen(true)] out IPEndPoint? endpoint)
    {
        endpoint = null;
        try { return IPEndPoint.TryParse(StrictUtf8.GetString(field), out endpoint); }
        catch (DecoderFallbackException) { return false; }
    }

    public static bool TryWrite(IPEndPoint endpoint, Span<byte> field, out int written)
    {
        string text = endpoint.ToString();
        written = 0;
        if (Encoding.UTF8.GetByteCount(text) > field.Length) return false;
        written = Encoding.UTF8.GetBytes(text.AsSpan(), field);
        return true;
    }
}
#endregion
