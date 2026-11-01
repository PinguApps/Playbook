using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Playbook.Samples;

internal static class AeadChecks
{
    public static void Run()
    {
        if (!AesGcm.IsSupported || !AesCcm.IsSupported)
            throw new PlatformNotSupportedException("Both AES-GCM and AES-CCM must execute for this evidence.");

        foreach (int size in new[] { 16, 24, 32 })
        {
            byte[] key = RandomNumberGenerator.GetBytes(size);
#if NET11_0_OR_GREATER
            using AesGcm gcm = new(key, 16);
            using AesCcm ccm = new(key);
            Func<int> gcmSize = () => ParcelKeyInfo.Describe(gcm);
            Func<int> ccmSize = () => ParcelKeyInfo.Describe(ccm);
#else
            using GcmParcelKey trackedGcm = new(key);
            using CcmParcelKey trackedCcm = new(key);
            AesGcm gcm = trackedGcm.Cipher;
            AesCcm ccm = trackedCcm.Cipher;
            Func<int> gcmSize = () => trackedGcm.KeySizeInBytes;
            Func<int> ccmSize = () => trackedCcm.KeySizeInBytes;
#endif
            // The caller's key buffer is not retained by the example owners.
            CryptographicOperations.ZeroMemory(key);
            Equal(size, gcmSize());
            Equal(size, ccmSize());
            byte[] message = Encoding.UTF8.GetBytes("Mossbank parcel 48: fragile lenses");
            byte[] aad = Encoding.UTF8.GetBytes("route=reed-jetty;revision=3");
            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] encrypted = new byte[message.Length];
            byte[] recovered = new byte[message.Length];
            byte[] tag = new byte[16];
            gcm.Encrypt(nonce, message, encrypted, tag, aad);
            gcm.Decrypt(nonce, encrypted, tag, recovered, aad);
            if (!message.AsSpan().SequenceEqual(recovered)) throw new InvalidOperationException("GCM round trip.");
            tag[0] ^= 1;
            Throws<AuthenticationTagMismatchException>(() => gcm.Decrypt(nonce, encrypted, tag, recovered, aad));
            Equal(16, gcm.TagSizeInBytes!.Value);
            Equal(size, gcmSize());
            // A fresh nonce for this independent encryption; never reuse a key/nonce pair.
            nonce = RandomNumberGenerator.GetBytes(12);
            tag = new byte[8];
            ccm.Encrypt(nonce, message, encrypted, tag, aad);
            ccm.Decrypt(nonce, encrypted, tag, recovered, aad);
            if (!message.AsSpan().SequenceEqual(recovered)) throw new InvalidOperationException("CCM round trip.");
            tag[0] ^= 1;
            Throws<AuthenticationTagMismatchException>(() => ccm.Decrypt(nonce, encrypted, tag, recovered, aad));
            Equal(size, ccmSize());
            Console.WriteLine($"KEY bytes={size} bits={size * 8}; GCM nonce=12 tag=16; CCM nonce=12 tag=8; roundtrip/tamper=pass");
            gcm.Dispose();
            ccm.Dispose();
            Equal(size, gcmSize());
            Equal(size, ccmSize());
            Throws<ObjectDisposedException>(() => gcm.Encrypt(nonce, message, encrypted, new byte[16], aad));
            Throws<ObjectDisposedException>(() => ccm.Encrypt(nonce, message, encrypted, tag, aad));
            Throws<ObjectDisposedException>(() => gcm.Decrypt(nonce, encrypted, new byte[16], recovered, aad));
            Throws<ObjectDisposedException>(() => ccm.Decrypt(nonce, encrypted, tag, recovered, aad));
        }
        foreach (int invalid in new[] { 0, 15, 17, 31, 33 })
        {
            Throws<CryptographicException>(() => { using AesGcm cipher = new(new byte[invalid], 16); });
            Throws<CryptographicException>(() => { using AesCcm cipher = new(new byte[invalid]); });
        }
        // Windows provider evidence is measured after real encryption, not inferred from IsSupported.
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("This capture declares Windows CNG.");
        foreach (string name in new[] { "bcrypt.dll", "bcryptprimitives.dll" })
        {
            ProcessModule module = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Single(m => string.Equals(m.ModuleName, name, StringComparison.OrdinalIgnoreCase));
            Console.WriteLine($"NATIVE {module.FileName} version={module.FileVersionInfo.ProductVersion} sha256={Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(module.FileName)))}");
        }
    }

    private static void Equal(int expected, int actual)
    {
        if (expected != actual) throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }
}
