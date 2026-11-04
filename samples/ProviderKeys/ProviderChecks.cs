using System.Security.Cryptography;
using System.Runtime.Versioning;
using Playbook.Samples;

RuntimeIdentity.Assert();
if (!OperatingSystem.IsLinux())
    throw new PlatformNotSupportedException("Run this provider fixture in the isolated Linux container.");
ProviderChecks.Run();

[SupportedOSPlatform("linux")]
internal static class ProviderChecks
{
    public static void Run()
    {
        string key = "file:/proof/rookery.pem";
        byte[] message = "Rookery Observatory: release weather balloon 17"u8.ToArray();
        using SafeEvpPKeyHandle handle = RookerySigner.Open(key);
        using RSAOpenSsl signer = new(handle);
        handle.Dispose(); // RSAOpenSsl owns its own reference.
        byte[] signature = signer.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using RSA verifier = RSA.Create();
        verifier.ImportSubjectPublicKeyInfo(signer.ExportSubjectPublicKeyInfo(), out _);
        if (!verifier.VerifyData(message, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)) throw new Exception("Signature rejected");
        message[0] ^= 1;
        if (verifier.VerifyData(message, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)) throw new Exception("Tamper accepted");
        Fails(() => SafeEvpPKeyHandle.OpenKeyFromProvider("rookery-missing-provider", key));
        Fails(() => RookerySigner.Open("file:/proof/missing.pem"));
#if NET11_0_OR_GREATER
        Fails(() => SafeEvpPKeyHandle.OpenKeyFromProvider(["default", "base"], key, "provider=rookery-missing"));
        // Reverse order is a different ordered context; both explicit configurations work here.
        using var reversed = SafeEvpPKeyHandle.OpenKeyFromProvider(["base", "default"], key, "provider=default");
        using var reverseSigner = new RSAOpenSsl(reversed);
        _ = reverseSigner.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Console.WriteLine("PASS ordered default/base and base/default; property-query success/rejection");
#endif
        signer.Dispose();
        try { _ = signer.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); throw new Exception("Disposed signer accepted"); }
        catch (ObjectDisposedException) { }
        Console.WriteLine("PASS provider RSA sign/verify/tamper; missing provider/key; independent handle ownership and disposal");
    }

    private static void Fails(Func<SafeEvpPKeyHandle> action)
    {
        try { using var unexpected = action(); throw new Exception("Expected provider failure"); }
        catch (CryptographicException) { }
    }
}
