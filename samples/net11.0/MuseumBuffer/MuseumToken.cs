// snippet:complete
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace HarbourMuseum;

public static class MuseumToken
{
    public static string Transform(IDataProtector protector, ReadOnlySpan<byte> input,
        OwnedBuffer destination, bool unprotect)
    {
        try
        {
            destination.ThrowIfOverlaps(input);
            destination.Clear();
            if (protector is ISpanDataProtector spanProtector)
            {
                if (unprotect) spanProtector.Unprotect(input, ref destination);
                else spanProtector.Protect(input, ref destination);
                return "span";
            }
            // Legitimate IDataProtector implementations need not offer the span capability.
            byte[] source = input.ToArray();
            byte[]? result = null;
            try
            {
                result = unprotect ? protector.Unprotect(source) : protector.Protect(source);
                result.CopyTo(destination.GetSpan(result.Length));
                destination.Advance(result.Length);
                return "array";
            }
            finally
            {
                CryptographicOperations.ZeroMemory(source);
                if (result is not null) CryptographicOperations.ZeroMemory(result);
            }
        }
        catch { destination.Clear(); throw; } // No failed output escapes.
    }
}
// endsnippet:complete
