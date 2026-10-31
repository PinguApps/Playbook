using System.Security.Cryptography;

namespace HarbourPackets;

public static class RawApiChecks
{
    public static void Run()
    {
        byte[] key = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
        byte[] data = "Mallow Quay|buoy=7|offset=-9|sequence=42"u8.ToArray();
        foreach (byte[] invalid in new[] { Array.Empty<byte>(), new byte[31], new byte[33] })
        {
            try { HMACSHA256.Verify(key, data, invalid); throw new InvalidOperationException("Expected length failure."); }
            catch (ArgumentException) { Console.WriteLine($"RAW HMAC length={invalid.Length} throws ArgumentException"); }
        }
        if (Kmac128.IsSupported)
        {
            byte[] custom = "Mallow packet v1"u8.ToArray();
            byte[] shorter = Kmac128.HashData(key, data, 31, custom);
            if (!Kmac128.Verify(key, data, shorter, custom)) throw new InvalidOperationException("KMAC variable output contract changed.");
            try { Kmac128.Verify(key, data, Array.Empty<byte>(), custom); throw new InvalidOperationException("Expected empty failure."); }
            catch (ArgumentException) { Console.WriteLine("RAW KMAC nonempty tag chooses output length; empty throws ArgumentException. Application still requires32."); }
        }
    }
}
