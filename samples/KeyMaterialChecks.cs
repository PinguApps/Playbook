using Harbour.Keys;
using System.Security.Cryptography;

byte[] token = new byte[32];
Require(SecretChecks.IsBlank(token), "blank token");
foreach (int position in new[] { 0, 15, 31 })
{
    token[position] = 1;
    Require(!SecretChecks.IsBlank(token), "nonblank token");
    token[position] = 0;
}
Expect<ArgumentException>(() => SecretChecks.IsBlank(new byte[31]));
Console.WriteLine("PASS: fixed-length blank/nonblank checks; API use, not a timing benchmark.");

using var outbound = DockExchange.CreateKey();
using var inbound = DockExchange.CreateKey();
#if NET11_0_OR_GREATER
byte[] outboundPublic = outbound.ExportPublicKey();
byte[] inboundPublic = inbound.ExportPublicKey();
using var imported = X25519DiffieHellman.ImportSubjectPublicKeyInfo(outbound.ExportSubjectPublicKeyInfo());
Require(CryptographicOperations.FixedTimeEquals(outboundPublic, imported.ExportPublicKey()), "SPKI import");
byte[] privateKey = outbound.ExportPkcs8PrivateKey();
try
{
    using var restored = X25519DiffieHellman.ImportPkcs8PrivateKey(privateKey);
    Require(CryptographicOperations.FixedTimeEquals(outboundPublic, restored.ExportPublicKey()), "PKCS8 import");
}
finally { CryptographicOperations.ZeroMemory(privateKey); }
Expect<ArgumentException>(() => DockExchange.Derive(outbound, new byte[31]));
Expect<CryptographicException>(() => DockExchange.Derive(outbound, new byte[32]));
#else
byte[] outboundPublic = outbound.ExportSubjectPublicKeyInfo();
byte[] inboundPublic = inbound.ExportSubjectPublicKeyInfo();
#endif
byte[] first = DockExchange.Derive(outbound, inboundPublic);
byte[] second = DockExchange.Derive(inbound, outboundPublic);
try { Require(CryptographicOperations.FixedTimeEquals(first, second), "agreement"); }
finally { CryptographicOperations.ZeroMemory(first); CryptographicOperations.ZeroMemory(second); }
Console.WriteLine("PASS: original two-party key exchange and public-key import; no peer-authentication claim.");

byte[] kek = RandomNumberGenerator.GetBytes(32);
byte[] dispatch = RandomNumberGenerator.GetBytes(32);
byte[] recovered = [];
try
{
    byte[] wrapped = DispatchKeyWrap.Protect(kek, dispatch);
    recovered = DispatchKeyWrap.Recover(kek, wrapped);
    Require(CryptographicOperations.FixedTimeEquals(dispatch, recovered), "key recovery");
    wrapped[8] ^= 1;
    Expect<CryptographicException>(() => DispatchKeyWrap.Recover(kek, wrapped));
    byte[] wrong = RandomNumberGenerator.GetBytes(32);
    try { Expect<CryptographicException>(() => DispatchKeyWrap.Recover(wrong, DispatchKeyWrap.Protect(kek, dispatch))); }
    finally { CryptographicOperations.ZeroMemory(wrong); }
#if NET11_0_OR_GREATER
    Expect<ArgumentException>(() => DispatchKeyWrap.Protect(kek, new byte[15]));
    // RFC 3394 section 4.1 published conformance vector, not recipe prose/code.
    using Aes aes = Aes.Create();
    aes.SetKey(Convert.FromHexString("000102030405060708090A0B0C0D0E0F"));
    byte[] vector = aes.EncryptKeyWrap(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"));
    Require(Convert.ToHexString(vector) == "1FA68B0A8112B447AEF34BD8FB5A7B829D3E862371D2CFE5", "RFC3394 vector");
    Require(Aes.GetKeyWrapLength(32) == 40, "KW length");
#else
    byte[] odd = Enumerable.Range(0, 17).Select(value => (byte)value).ToArray();
    Require(CryptographicOperations.FixedTimeEquals(odd, DispatchKeyWrap.Recover(kek, DispatchKeyWrap.Protect(kek, odd))), "padded odd length");
#endif
}
finally
{
    CryptographicOperations.ZeroMemory(kek);
    CryptographicOperations.ZeroMemory(dispatch);
    CryptographicOperations.ZeroMemory(recovered);
}
Console.WriteLine("PASS: actual key-wrap round trip, tampering/wrong-key rejection and format-specific sizing.");

static void Require(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}

static void Expect<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}
