// snippet:implementation
using System.Security.Cryptography;

namespace HarbourPackets;

public static class PacketAuthenticator
{
    public const int TagLength = 32;
    public static bool Verify(byte[] key, byte[] data, byte[] tag) =>
        tag.Length == TagLength && CryptographicOperations.FixedTimeEquals(HMACSHA256.HashData(key, data), tag);

    public static async ValueTask<bool> VerifyAsync(byte[] key, Stream stream, byte[] tag, CancellationToken token)
    {
        if (tag.Length != TagLength) return false;
        byte[] computed = await HMACSHA256.HashDataAsync(key, stream, token);
        return CryptographicOperations.FixedTimeEquals(computed, tag);
    }

    public static bool Current(IncrementalHash hash, byte[] tag)
    {
        ProtocolTag(tag);
        return CryptographicOperations.FixedTimeEquals(hash.GetCurrentHash(), tag);
    }
    public static bool Finish(IncrementalHash hash, byte[] tag)
    {
        ProtocolTag(tag);
        return CryptographicOperations.FixedTimeEquals(hash.GetHashAndReset(), tag);
    }

    public static bool VerifyKmac(byte[] key, byte[] data, byte[] tag, byte[] customization) =>
        tag.Length == TagLength && Kmac128.IsSupported &&
        CryptographicOperations.FixedTimeEquals(Kmac128.HashData(key, data, TagLength, customization), tag);

    public static bool CurrentKmac(Kmac128 hash, byte[] tag)
    {
        ProtocolTag(tag);
        return CryptographicOperations.FixedTimeEquals(hash.GetCurrentHash(TagLength), tag);
    }
    public static bool FinishKmac(Kmac128 hash, byte[] tag)
    {
        ProtocolTag(tag);
        return CryptographicOperations.FixedTimeEquals(hash.GetHashAndReset(TagLength), tag);
    }
    private static byte[] ProtocolTag(byte[] tag)
    {
        if (tag.Length != TagLength) throw new ArgumentException("Expected a32-byte protocol tag.", nameof(tag));
        return tag;
    }
}
// endsnippet:implementation
