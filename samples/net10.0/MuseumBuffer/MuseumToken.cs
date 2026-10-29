// snippet:complete
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace HarbourMuseum;

public static class MuseumToken
{
    public static string Transform(IDataProtector protector, ReadOnlySpan<byte> input,
        OwnedBuffer destination, bool unprotect)
    {
        byte[]? source = null;
        byte[]? result = null;
        try
        {
            destination.ThrowIfOverlaps(input);
            destination.Clear();
            source = input.ToArray();
            result = unprotect ? protector.Unprotect(source) : protector.Protect(source);
            result.CopyTo(destination.GetSpan(result.Length));
            destination.Advance(result.Length);
            return "array";
        }
        catch { destination.Clear(); throw; }
        finally
        {
            if (source is not null) CryptographicOperations.ZeroMemory(source);
            if (result is not null) CryptographicOperations.ZeroMemory(result);
        }
    }
}
// endsnippet:complete
