// snippet:cipher
using System.Security.Cryptography;

namespace InstrumentGateway;

public static class GatewayCipher
{
    // useOaep is the existing peer contract, not a new algorithm choice.
    public static byte[] Encrypt(RSACryptoServiceProvider key, byte[] reading, bool useOaep)
        => key.Encrypt(reading, useOaep);

    public static byte[] Decrypt(RSACryptoServiceProvider key, byte[] ciphertext, bool useOaep)
        => key.Decrypt(ciphertext, useOaep);
}
// endsnippet:cipher
