using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using InstrumentGateway;

if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("This fixture requires real Windows CAPI.");
if (args.Length != 2 || args[0] is not ("old-send" or "new-exchange" or "old-receive"))
    throw new ArgumentException("Use old-send|new-exchange|old-receive and a private empty fixture directory.");
string mode = args[0];
string directory = args[1];
byte[] reading = Encoding.UTF8.GetBytes("buoy=Gannet-7;depth-mm=14320;sequence=81");
using var key = new RSACryptoServiceProvider(2048, new CspParameters(24)
{
    Flags = CspProviderFlags.CreateEphemeralKey
});
key.PersistKeyInCsp = false;
string keyFile = Path.Combine(directory, "disposable-key.blob");
if (mode == "old-send") File.WriteAllBytes(keyFile, key.ExportCspBlob(true));
else key.ImportCspBlob(File.ReadAllBytes(keyFile));
Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.OSArchitecture}");
Console.WriteLine($"CAPI provider={key.CspKeyContainerInfo.ProviderName}; type={key.CspKeyContainerInfo.ProviderType}; keyBits={key.KeySize}; persist={key.PersistKeyInCsp}; publicKeySha256={Hash(key.ExportSubjectPublicKeyInfo())}");
Console.WriteLine($"INPUT plaintextSha256={Hash(reading)}; plaintextBytes={reading.Length}; mode={mode}");
foreach (bool useOaep in new[] { false, true })
{
    string name = useOaep ? "oaep-sha1" : "pkcs1";
    var padding = useOaep ? RSAEncryptionPadding.OaepSHA1 : RSAEncryptionPadding.Pkcs1;
    byte[] own = GatewayCipher.Encrypt(key, reading, useOaep);
    Equal(reading, GatewayCipher.Decrypt(key, own, useOaep), "canonical roundtrip");
    Equal(reading, key.Decrypt(own, padding), "explicit overload decrypts canonical output");
    Equal(reading, GatewayCipher.Decrypt(key, key.Encrypt(reading, padding), useOaep), "canonical decrypts explicit output");
    Reject(() => GatewayCipher.Decrypt(key, own, !useOaep), "wrong padding");
    byte[] tampered = (byte[])own.Clone();
    Array.Clear(tampered); // Destroy the real ciphertext; deterministic invalid RSA block.
    Reject(() => GatewayCipher.Decrypt(key, tampered, useOaep), "zeroed ciphertext");
    if (mode == "old-send") File.WriteAllBytes(Path.Combine(directory, $"old-{name}.bin"), own);
    else
    {
        string peer = mode == "new-exchange" ? "old" : "new";
        byte[] ciphertext = File.ReadAllBytes(Path.Combine(directory, $"{peer}-{name}.bin"));
        Equal(reading, GatewayCipher.Decrypt(key, ciphertext, useOaep), "cross-runtime decryption");
        Console.WriteLine($"PASS {peer}->current-consumer {name}; ciphertextSha256={Hash(ciphertext)}");
        if (mode == "new-exchange") File.WriteAllBytes(Path.Combine(directory, $"new-{name}.bin"), own);
    }
    Console.WriteLine($"PASS {name}: canonical/explicit compatibility; wrong-padding/zeroed-input rejection; ciphertextBytes={own.Length}");
}
Reject(() => key.Encrypt(reading, RSAEncryptionPadding.OaepSHA256), "same CAPI provider OAEP-SHA256 encrypt");
Reject(() => key.Decrypt(new byte[key.KeySize / 8], RSAEncryptionPadding.OaepSHA256), "same CAPI provider OAEP-SHA256 decrypt");
Console.WriteLine($"PASS {mode}; no named/persistent/production container used");

static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
static void Equal(byte[] expected, byte[] actual, string claim)
{
    if (!expected.AsSpan().SequenceEqual(actual)) throw new Exception(claim);
}
static void Reject(Func<byte[]> action, string claim)
{
    try { action(); }
    catch (CryptographicException error) { Console.WriteLine($"REJECT {claim}: {error.GetType().Name}; hresult={error.HResult:X8}"); return; }
    throw new Exception($"Accepted {claim}");
}
