// snippet:complete
using System.Net.Http;
using System.Security.Cryptography;

namespace PlaybookCosmos;

// Certificate-pinned transport for the approved loopback emulator only.
public static class EmulatorTransport
{
    public static HttpClient CreateClient(string endpoint, string certificateHash)
    {
        var uri = new Uri(endpoint);
        if (uri.Scheme != "https" || uri.Host != "127.0.0.1" || uri.AbsolutePath != "/" ||
            uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0)
            throw new ArgumentException("Only an HTTPS 127.0.0.1 emulator endpoint is permitted.");
        var expected = Convert.FromHexString(certificateHash);
        if (expected.Length != 32) throw new ArgumentException("Expected a SHA-256 certificate hash.");
        var handler = new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false };
        handler.ServerCertificateCustomValidationCallback = (_, certificate, _, _) =>
            certificate != null && CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(certificate.RawData), expected);
        return new HttpClient(handler) { BaseAddress = uri, Timeout = TimeSpan.FromSeconds(60) };
    }

}
// endsnippet:complete
