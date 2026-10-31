using System.Security.Cryptography;

namespace TelemetryKeys;

// snippet:keys
public static class SessionKeys
{
    public static bool TryDerive(byte[] material, byte[] salt, byte[] context, out byte[]? key)
    {
        key = null;
        key = HKDF.DeriveKey(HashAlgorithmName.SHA256, material, 32, salt, context);
        return true;
    }

    public static bool TryExpand(byte[] pseudorandomKey, byte[] context, out byte[]? key)
    {
        key = null;
        key = HKDF.Expand(HashAlgorithmName.SHA256, pseudorandomKey, 32, context);
        return true;
    }
}
// endsnippet:keys
