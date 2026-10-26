// snippet:complete
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace BeaconKeys;

// Deterministic protocol shape, fresh cryptographic key. Test driver, never a real authenticator.
public sealed class SoftwareKey : IDisposable
{
    private readonly ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly byte[] credentialId = RandomNumberGenerator.GetBytes(24);
    private uint counter;

    public UserPasskeyInfo Stored()
    {
        ECParameters parameters = key.ExportParameters(false);
        // COSE EC2 / ES256 / P-256, with 32-byte x and y coordinates.
        byte[] cose = [0xa5, 0x01, 0x02, 0x03, 0x26, 0x20, 0x01, 0x21, 0x58, 0x20,
            .. parameters.Q.X!, 0x22, 0x58, 0x20, .. parameters.Q.Y!];
        return new UserPasskeyInfo(credentialId, cose, DateTimeOffset.UtcNow, 0, ["internal"], true, false, false, [], []);
    }

    public string Assert(string options, string userId, string origin)
    {
        using JsonDocument request = JsonDocument.Parse(options);
        string challenge = request.RootElement.GetProperty("challenge").GetString()!;
        byte[] clientData = JsonSerializer.SerializeToUtf8Bytes(new { type = "webauthn.get", challenge, origin, crossOrigin = false });
        byte[] authenticatorData = new byte[37];
        SHA256.HashData(Encoding.UTF8.GetBytes("localhost")).CopyTo(authenticatorData, 0);
        authenticatorData[32] = 0x05; // User present and verified; no backup or attestation flags.
        BinaryPrimitives.WriteUInt32BigEndian(authenticatorData.AsSpan(33), ++counter);
        byte[] signed = [.. authenticatorData, .. SHA256.HashData(clientData)];
        byte[] signature = key.SignData(signed, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        return JsonSerializer.Serialize(new
        {
            id = Base64Url.EncodeToString(credentialId), rawId = Base64Url.EncodeToString(credentialId), type = "public-key",
            response = new
            {
                clientDataJSON = Base64Url.EncodeToString(clientData),
                authenticatorData = Base64Url.EncodeToString(authenticatorData),
                signature = Base64Url.EncodeToString(signature), userHandle = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(userId))
            }, clientExtensionResults = new { }
        });
    }

    public void Dispose() => key.Dispose();
}
// endsnippet:complete
