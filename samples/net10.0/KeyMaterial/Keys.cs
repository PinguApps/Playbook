using System.Security.Cryptography;

namespace Harbour.Keys;

// snippet:compare
public static class SecretChecks
{
    public static bool IsBlank(ReadOnlySpan<byte> token)
    {
        if (token.Length != 32) throw new ArgumentException("Expected a 32-byte token.");
        Span<byte> blank = stackalloc byte[32];
        blank.Clear();
        return CryptographicOperations.FixedTimeEquals(token, blank);
    }
}
// endsnippet:compare

// snippet:exchange
public static class DockExchange
{
    // P-256 is a different wire protocol, not an X25519 implementation.
    public static ECDiffieHellman CreateKey() => ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

    public static byte[] Derive(ECDiffieHellman local, ReadOnlySpan<byte> trustedPeerSpki)
    {
        using ECDiffieHellman peer = ECDiffieHellman.Create();
        peer.ImportSubjectPublicKeyInfo(trustedPeerSpki, out int read);
        if (read != trustedPeerSpki.Length) throw new CryptographicException("Trailing key data.");
        return local.DeriveKeyFromHash(peer.PublicKey, HashAlgorithmName.SHA256);
    }
}
// endsnippet:exchange

// snippet:wrap
public static class DispatchKeyWrap
{
    public static byte[] Protect(ReadOnlySpan<byte> wrappingKey, ReadOnlySpan<byte> dispatchKey)
    {
        using Aes aes = Aes.Create();
        aes.SetKey(wrappingKey);
        return aes.EncryptKeyWrapPadded(dispatchKey);
    }

    public static byte[] Recover(ReadOnlySpan<byte> wrappingKey, ReadOnlySpan<byte> wrapped)
    {
        using Aes aes = Aes.Create();
        aes.SetKey(wrappingKey);
        return aes.DecryptKeyWrapPadded(wrapped);
    }
}
// endsnippet:wrap
