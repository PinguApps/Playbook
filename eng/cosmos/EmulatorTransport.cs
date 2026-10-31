using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PlaybookCosmos;

// Only for the loopback legacy emulator. This is Microsoft's public emulator key.
public sealed class EmulatorTransport : IDisposable
{
    public const string Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    private readonly HttpClient client;

    public EmulatorTransport(string endpoint, string certificateHash)
        => client = CreateClient(endpoint, certificateHash);

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

    public static string CertificateHash(string pem)
    {
        using var certificate = X509Certificate2.CreateFromPem(pem);
        return Convert.ToHexString(SHA256.HashData(certificate.RawData));
    }

    public string Request(string verb, string path, string resourceType, string resourceId, string body = "")
    {
        if (!path.StartsWith("/", StringComparison.Ordinal) || path.StartsWith("//", StringComparison.Ordinal) ||
            path.Contains("..") || path.Contains('\\') || path.Contains('?') || path.Contains('#'))
            throw new ArgumentException("Expected a local resource path.");
        var date = DateTime.UtcNow.ToString("r", System.Globalization.CultureInfo.InvariantCulture);
        var payload = $"{verb.ToLowerInvariant()}\n{resourceType.ToLowerInvariant()}\n{resourceId}\n{date.ToLowerInvariant()}\n\n";
        using var hmac = new HMACSHA256(Convert.FromBase64String(Key));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        using var request = new HttpRequestMessage(new HttpMethod(verb), path);
        request.Headers.TryAddWithoutValidation("authorization", Uri.EscapeDataString($"type=master&ver=1.0&sig={signature}"));
        request.Headers.Add("x-ms-date", date);
        request.Headers.Add("x-ms-version", "2020-07-15");
        if (resourceType == "docs") request.Headers.Add("x-ms-documentdb-partitionkey", "[\"route\"]");
        if (body.Length != 0) request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = client.SendAsync(request).GetAwaiter().GetResult();
        var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Cosmos HTTP {(int)response.StatusCode}: {text}");
        return text;
    }

    public void Dispose() => client.Dispose();
}
