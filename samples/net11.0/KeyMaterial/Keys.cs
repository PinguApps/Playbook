using System.Security.Cryptography;

namespace Harbour.Keys;

// snippet:compare
public static class SecretChecks
{
    public static bool IsBlank(ReadOnlySpan<byte> token)
    {
        if (token.Length != 32) throw new ArgumentException("Expected a 32-byte token.");
        return CryptographicOperations.FixedTimeEquals(token, (byte)0);
    }
}
// endsnippet:compare

// snippet:exchange
public static class DockExchange
{
    public static X25519DiffieHellman CreateKey()
    {
        if (!X25519DiffieHellman.IsSupported)
            throw new PlatformNotSupportedException("This provider cannot exchange X25519 keys.");
        return X25519DiffieHellman.GenerateKey();
    }

    // Authenticate the public key and bind both identities/transcript in the protocol.
    public static byte[] Derive(X25519DiffieHellman local, ReadOnlySpan<byte> trustedPeerKey)
    {
        byte[] raw = new byte[X25519DiffieHellman.SecretAgreementSizeInBytes];
        try
        {
            local.DeriveRawSecretAgreement(trustedPeerKey, raw);
            if (CryptographicOperations.FixedTimeEquals(raw, (byte)0))
                throw new CryptographicException("Rejected an all-zero agreement.");
            byte[] result = new byte[32];
            HKDF.DeriveKey(HashAlgorithmName.SHA256, raw, result,
                salt: "harbour-dispatch-v1"u8, info: "key-encryption"u8);
            return result;
        }
        finally { CryptographicOperations.ZeroMemory(raw); }
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
        return aes.EncryptKeyWrap(dispatchKey);
    }

    public static byte[] Recover(ReadOnlySpan<byte> wrappingKey, ReadOnlySpan<byte> wrapped)
    {
        using Aes aes = Aes.Create();
        aes.SetKey(wrappingKey);
        return aes.DecryptKeyWrap(wrapped);
    }
}
// endsnippet:wrap
