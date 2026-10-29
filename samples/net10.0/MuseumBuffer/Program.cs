// snippet:complete
// snippet:setup
using HarbourMuseum;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;

var directory = Path.Combine(Path.GetTempPath(), "museum-token-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
try
{
    using var services = CreateServices(Path.Combine(directory, "collection"));
    using var foreignServices = CreateServices(Path.Combine(directory, "unrelated"));
    var provider = services.GetRequiredService<IDataProtectionProvider>();
    var protector = provider.CreateProtector("HarbourMuseum.CollectionToken.v1");
    var wrongPurpose = provider.CreateProtector("HarbourMuseum.StaffToken.v1");
    var wrongKey = foreignServices.GetRequiredService<IDataProtectionProvider>()
        .CreateProtector("HarbourMuseum.CollectionToken.v1");
    // endsnippet:setup
    PrintArtifact(typeof(IDataProtector).Assembly);
    PrintArtifact(typeof(IKeyManager).Assembly);
    Console.WriteLine($"Runtime={RuntimeInformation.FrameworkDescription}; OS={RuntimeInformation.OSDescription}; architecture={RuntimeInformation.ProcessArchitecture}; protector={protector.GetType().FullName}");
#if NET11_0_OR_GREATER
    Check(protector is ISpanDataProtector, "real provider exposes span capability");
    var expected = "span";
#else
    Check(typeof(IDataProtector).Assembly.GetType("Microsoft.AspNetCore.DataProtection.ISpanDataProtector") is null,
        "previous abstraction has no span protector");
    Check(typeof(IKeyManager).Assembly.GetType("Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ISpanAuthenticatedEncryptor") is null,
        "previous implementation has no span encryptor");
    var expected = "array";
#endif
    foreach (var input in new[] { "collection:lantern-27;loan:4h"u8.ToArray(), [], new byte[1024 * 1024] })
    {
        if (input.Length > 100) RandomNumberGenerator.Fill(input);
        CheckRoute(protector, expected, input);
        // This wrapper only hides capability; both calls still use the real framework key ring.
        CheckRoute(new ArrayOnlyProtector(protector), "array", input);
        using var ciphertext = new OwnedBuffer(new byte[input.Length + 1024]);
        MuseumToken.Transform(protector, input, ciphertext, false);
        Reject(wrongPurpose, ciphertext.WrittenSpan, "wrong-purpose");
        Reject(wrongKey, ciphertext.WrittenSpan, "wrong-key-ring");
        var tampered = ciphertext.WrittenSpan.ToArray();
        tampered[^1] ^= 1;
        Reject(protector, tampered, "tampered");
        Reject(new ArrayOnlyProtector(protector), tampered, "fallback-tampered");
        CryptographicOperations.ZeroMemory(input);
    }
    // On the pinned net11 implementation, 24 bytes admits the header before
    // the encryptor requests more capacity. Failure must clear that written header.
    foreach (var capacity in new[] { 1, 24 })
    {
        var tooSmallStorage = new byte[capacity];
        using var tooSmall = new OwnedBuffer(tooSmallStorage);
        try { MuseumToken.Transform(protector, "loan"u8, tooSmall, false); throw new Exception("Expected capacity failure."); }
        catch (Exception ex) when (ex is CryptographicException or InvalidOperationException) { }
        Check(tooSmallStorage.All(b => b == 0) && tooSmall.WrittenSpan.IsEmpty, "capacity failure cleared");
    }
    var unadvanced = new byte[32];
    using (var owner = new OwnedBuffer(unadvanced)) owner.GetSpan().Fill(0x55);
    Check(unadvanced.All(b => b == 0), "dispose clears unadvanced bytes");
    foreach (var route in new[] { protector, new ArrayOnlyProtector(protector) })
    {
        var storage = Enumerable.Repeat((byte)0x55, 256).ToArray();
        using var destination = new OwnedBuffer(storage);
        RejectOverlap(() => MuseumToken.Transform(route, storage.AsSpan(1, 3), destination, false),
            destination, storage, "protect input overlaps destination");
    }
#if NET11_0_OR_GREATER
    var key = services.GetRequiredService<IKeyManager>().GetAllKeys().Single();
    var encryptor = key.CreateEncryptor();
    Check(encryptor is ISpanAuthenticatedEncryptor, "actual key creates span encryptor");
    Console.WriteLine($"Advanced encryptor={encryptor!.GetType().FullName}");
    var spanEncryptor = (ISpanAuthenticatedEncryptor)encryptor;
    var overlapStorage = Enumerable.Repeat((byte)0x55, 256).ToArray();
    using (var destination = new OwnedBuffer(overlapStorage))
    {
        RejectOverlap(() => AdvancedEnvelope.Transform(spanEncryptor, overlapStorage.AsSpan(1, 3),
            "museum:harbour"u8, destination, false), destination, overlapStorage, "advanced input overlaps destination");
        overlapStorage.AsSpan().Fill(0x55);
        RejectOverlap(() => AdvancedEnvelope.Transform(spanEncryptor, "loan"u8,
            overlapStorage.AsSpan(1, 3), destination, false), destination, overlapStorage, "advanced AAD overlaps destination");
    }
    foreach (var input in new[] { "collection:lantern-27"u8.ToArray(), [], new byte[1024 * 1024] })
    {
        using var encrypted = new OwnedBuffer(new byte[input.Length + 1024]);
        var plaintextStorage = new byte[input.Length + 1024];
        using var plaintext = new OwnedBuffer(plaintextStorage);
        AdvancedEnvelope.Transform(spanEncryptor, input, "museum:harbour;schema:1"u8, encrypted, false);
        AdvancedEnvelope.Transform(spanEncryptor, encrypted.WrittenSpan, "museum:harbour;schema:1"u8, plaintext, true);
        Check(plaintext.WrittenSpan.SequenceEqual(input), "advanced matched-AAD round trip");
        try
        {
            AdvancedEnvelope.Transform(spanEncryptor, encrypted.WrittenSpan, "museum:other;schema:1"u8, plaintext, true);
            throw new Exception("Expected AAD authentication failure.");
        }
        catch (CryptographicException) { }
        Check(plaintext.WrittenSpan.IsEmpty && plaintextStorage.All(b => b == 0), "wrong AAD discards full output");
        var tampered = encrypted.WrittenSpan.ToArray();
        tampered[^1] ^= 1;
        try
        {
            AdvancedEnvelope.Transform(spanEncryptor, tampered, "museum:harbour;schema:1"u8, plaintext, true);
            throw new Exception("Expected authentication failure.");
        }
        catch (CryptographicException) { }
        Check(plaintext.WrittenSpan.IsEmpty && plaintextStorage.All(b => b == 0), "advanced tampering discards full output");
    }
#endif
    Console.WriteLine("PASS: museum token ownership, capability/fallback, empty/token/1MiB round trips, purpose/key/tampering rejection, capacity and full-buffer cleanup.");
}
finally { Directory.Delete(directory, recursive: true); }

// snippet:registration
static ServiceProvider CreateServices(string keyDirectory)
{
    Directory.CreateDirectory(keyDirectory);
    var services = new ServiceCollection();
    services.AddDataProtection()
        .SetApplicationName("HarbourMuseum")
        .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
        .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        });
    return services.BuildServiceProvider();
}
// endsnippet:registration

static void CheckRoute(IDataProtector protector, string expected, byte[] input)
{
    var cipherStorage = new byte[input.Length + 1024];
    var plainStorage = new byte[input.Length + 1024];
    using (var cipher = new OwnedBuffer(cipherStorage))
    using (var plain = new OwnedBuffer(plainStorage))
    {
        Check(MuseumToken.Transform(protector, input, cipher, false) == expected, "protect route");
        Check(MuseumToken.Transform(protector, cipher.WrittenSpan, plain, true) == expected, "unprotect route");
        Check(plain.WrittenSpan.SequenceEqual(input), "round trip");
        Console.WriteLine($"PASS: {expected}, input={input.Length}, protected={cipher.WrittenSpan.Length}");
    }
    Check(cipherStorage.All(b => b == 0) && plainStorage.All(b => b == 0), "disposed storage cleared");
}

static void Reject(IDataProtector protector, ReadOnlySpan<byte> input, string scenario)
{
    var storage = Enumerable.Repeat((byte)0x55, input.Length + 1024).ToArray();
    using var output = new OwnedBuffer(storage);
    try { MuseumToken.Transform(protector, input, output, true); throw new Exception("Expected crypto rejection."); }
    catch (CryptographicException) { }
    Check(output.WrittenSpan.IsEmpty && storage.All(b => b == 0), "failed output discarded");
    Console.WriteLine($"PASS: rejected {scenario}, full destination cleared");
}

static void Check(bool condition, string claim)
{
    if (!condition) throw new Exception(claim);
    Console.WriteLine("PASS: " + claim);
}

static void RejectOverlap(Action operation, OwnedBuffer destination, byte[] storage, string scenario)
{
    try { operation(); throw new Exception("Expected overlap rejection: " + scenario); }
    catch (ArgumentException) { }
    Check(destination.WrittenSpan.IsEmpty && storage.All(b => b == 0), scenario + "; full output cleared");
}

static void PrintArtifact(Assembly assembly)
{
    var path = assembly.Location;
    Console.WriteLine($"Artifact={path}; version={FileVersionInfo.GetVersionInfo(path).ProductVersion}; sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
}

sealed class ArrayOnlyProtector(IDataProtector inner) : IDataProtector
{
    public IDataProtector CreateProtector(string purpose) => new ArrayOnlyProtector(inner.CreateProtector(purpose));
    public byte[] Protect(byte[] plaintext) => inner.Protect(plaintext);
    public byte[] Unprotect(byte[] protectedData) => inner.Unprotect(protectedData);
}
// endsnippet:complete
