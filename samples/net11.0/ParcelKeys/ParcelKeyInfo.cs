// snippet:parcel-keys
using System.Security.Cryptography;

namespace Playbook.Samples;

internal static class ParcelKeyInfo
{
    public static int Describe(AesGcm cipher) => cipher.KeySizeInBytes;
    public static int Describe(AesCcm cipher) => cipher.KeySizeInBytes;
}
// endsnippet:parcel-keys
