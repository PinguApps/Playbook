// snippet:parcel-keys
using System.Security.Cryptography;

namespace Playbook.Samples;

internal sealed class GcmParcelKey : IDisposable
{
    public GcmParcelKey(ReadOnlySpan<byte> key)
    {
        Cipher = new AesGcm(key, tagSizeInBytes: 16);
        KeySizeInBytes = key.Length;
    }

    public AesGcm Cipher { get; }
    public int KeySizeInBytes { get; }
    public void Dispose() => Cipher.Dispose();
}

internal sealed class CcmParcelKey : IDisposable
{
    public CcmParcelKey(ReadOnlySpan<byte> key)
    {
        Cipher = new AesCcm(key);
        KeySizeInBytes = key.Length;
    }

    public AesCcm Cipher { get; }
    public int KeySizeInBytes { get; }
    public void Dispose() => Cipher.Dispose();
}
// endsnippet:parcel-keys
