// snippet:implementation
using System.Security.Cryptography;

namespace HarbourPackets;

public static class PacketAuthenticator
{
    // The protocol, rather than the untrusted received tag, chooses the length.
    public const int TagLength = 32;
    public static bool Verify(byte[] key, byte[] data, byte[] tag) =>
        tag.Length == TagLength && HMACSHA256.Verify(key, data, tag);

    public static ValueTask<bool> VerifyAsync(byte[] key, Stream stream, byte[] tag, CancellationToken token) =>
        tag.Length == TagLength ? HMACSHA256.VerifyAsync(key, stream, tag, token) : ValueTask.FromResult(false);

    public static bool Current(IncrementalHash hash, byte[] tag) => hash.VerifyCurrentHash(ProtocolTag(tag));
    public static bool Finish(IncrementalHash hash, byte[] tag) => hash.VerifyHashAndReset(ProtocolTag(tag));

    public static bool VerifyKmac(byte[] key, byte[] data, byte[] tag, byte[] customization) =>
        tag.Length == TagLength && Kmac128.IsSupported && Kmac128.Verify(key, data, tag, customization);

    // These incremental helpers are used only with protocol-length tags in this sample.
    public static bool CurrentKmac(Kmac128 hash, byte[] tag) => hash.VerifyCurrentHash(ProtocolTag(tag));
    public static bool FinishKmac(Kmac128 hash, byte[] tag) => hash.VerifyHashAndReset(ProtocolTag(tag));
    private static byte[] ProtocolTag(byte[] tag)
    {
        if (tag.Length != TagLength) throw new ArgumentException("Expected a32-byte protocol tag.", nameof(tag));
        return tag;
    }
}
// endsnippet:implementation
