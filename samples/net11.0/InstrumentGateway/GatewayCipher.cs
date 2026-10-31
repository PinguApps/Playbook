// snippet:cipher
using System.Security.Cryptography;

namespace InstrumentGateway;

public static class GatewayCipher
{
    // Preserve the agreed wire format and the caller's existing CAPI key.
    public static byte[] Encrypt(RSACryptoServiceProvider key, byte[] reading, bool useOaep)
        => key.Encrypt(reading, Padding(useOaep));

    public static byte[] Decrypt(RSACryptoServiceProvider key, byte[] ciphertext, bool useOaep)
        => key.Decrypt(ciphertext, Padding(useOaep));

    private static RSAEncryptionPadding Padding(bool useOaep)
        => useOaep ? RSAEncryptionPadding.OaepSHA1 : RSAEncryptionPadding.Pkcs1;
}
// endsnippet:cipher
