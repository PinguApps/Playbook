using System.Security.Cryptography;

namespace TelemetryKeys;

// snippet:keys
public static class SessionKeys
{
    // This protocol deliberately accepts at most 2,048 input bytes on every host.
    public static bool TryDerive(byte[] material, byte[] salt, byte[] context, out byte[]? key)
    {
        key = null;
        ArgumentNullException.ThrowIfNull(material);
        ArgumentNullException.ThrowIfNull(salt);
        ArgumentNullException.ThrowIfNull(context);
        if (material.Length > 2048) { return false; }
        key = HKDF.DeriveKey(HashAlgorithmName.SHA256, material, 32, salt, context);
        return true;
    }

    public static bool TryExpand(byte[] pseudorandomKey, byte[] context, out byte[]? key)
    {
        key = null;
        ArgumentNullException.ThrowIfNull(pseudorandomKey);
        ArgumentNullException.ThrowIfNull(context);
        if (pseudorandomKey.Length is < 32 or > 2048) { return false; }
        key = HKDF.Expand(HashAlgorithmName.SHA256, pseudorandomKey, 32, context);
        return true;
    }
}
// endsnippet:keys
