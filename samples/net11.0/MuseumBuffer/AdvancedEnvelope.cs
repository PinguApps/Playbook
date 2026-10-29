// snippet:complete
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;

namespace HarbourMuseum;

public static class AdvancedEnvelope
{
    // Use only inside an existing application-owned envelope/key-management contract.
    // This is NOT a DataProtection payload: it has no DataProtection key id/purposes.
    public static void Transform(ISpanAuthenticatedEncryptor encryptor,
        ReadOnlySpan<byte> input, ReadOnlySpan<byte> aad, OwnedBuffer destination, bool decrypt)
    {
        try
        {
            destination.ThrowIfOverlaps(input);
            destination.ThrowIfOverlaps(aad);
            destination.Clear();
            if (decrypt) encryptor.Decrypt(input, aad, ref destination);
            else encryptor.Encrypt(input, aad, ref destination);
        }
        catch { destination.Clear(); throw; }
    }
}
// endsnippet:complete
