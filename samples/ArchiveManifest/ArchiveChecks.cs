using System.Collections.Concurrent;
using System.Formats.Asn1;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

internal static class ArchiveChecks
{
    internal static async Task Run(string mode)
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("This fixture measures Linux/OpenSSL AIA behavior.");
        Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}; mode={mode}");
        string cache = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Console.WriteLine($"privateHome={cache}; preexistingDotnetCache={Directory.Exists(Path.Combine(cache, ".dotnet", "corefx"))}");
        using var server = new Issuers();
        using var rootKey = RSA.Create(2048);
        using var firstKey = RSA.Create(2048);
        using var secondKey = RSA.Create(2048);
        using var thirdKey = RSA.Create(2048);
        using var signer = RSA.Create(2048);
        using (var process = System.Diagnostics.Process.GetCurrentProcess())
        {
            foreach (System.Diagnostics.ProcessModule module in process.Modules)
            {
                if (!Path.GetFileName(module.FileName).StartsWith("libcrypto.so", StringComparison.Ordinal)) continue;
                IntPtr library = NativeLibrary.Load(module.FileName);
                try
                {
                    var version = Marshal.GetDelegateForFunctionPointer<OpenSslVersion>(NativeLibrary.GetExport(library, "OpenSSL_version"));
                    Console.WriteLine($"NATIVE {Marshal.PtrToStringAnsi(version(0))}; path={module.FileName}; sha256={Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(module.FileName)))}");
                }
                finally { NativeLibrary.Free(library); }
            }
        }
        var rootRequest = Request("Archive fixture " + Guid.NewGuid().ToString("N"), rootKey, true, null);
        using var root = rootRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddDays(30));
        using var first = Issue("Archive first", firstKey, true, root, null);
        using var second = Issue("Archive second", secondKey, true, first, server.Url("first"));
        using var third = Issue("Archive third", thirdKey, true, second, server.Url("second"));
        using var leaf = Issue("Archive manifest signer", signer, false, third, server.Url("third"), mode == "invalid");
        foreach (var certificate in new[] { root, first, second, third, leaf })
            Console.WriteLine($"INPUT certificate={certificate.Subject}; sha256={certificate.GetCertHashString(HashAlgorithmName.SHA256)}; validFrom={certificate.NotBefore.ToUniversalTime():O}; validTo={certificate.NotAfter.ToUniversalTime():O}");
        string cryptoAssembly = typeof(X509Chain).Assembly.Location;
        Console.WriteLine($"MANAGED {cryptoAssembly}; sha256={Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(cryptoAssembly)))}");
        server.Certificates["/first"] = first.RawData;
        server.Certificates["/second"] = second.RawData;
        server.Certificates["/third"] = third.RawData;
        Task serving = server.Serve();
        try
        {
            byte[] manifest = Encoding.UTF8.GetBytes("Archive shelf F7: folios=184; seal=amber");
            byte[] signature = signer.SignData(manifest, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            Console.WriteLine($"INPUT manifestSha256={Convert.ToHexString(SHA256.HashData(manifest))}; signatureSha256={Convert.ToHexString(SHA256.HashData(signature))}");
            bool supplied = mode != "aia";
            using var chain = ArchivePolicy.Create(mode == "untrusted" ? null : root, supplied ? new[] { first, second, third } : [], supplied);
            if (mode == "untrusted") chain.ChainPolicy.ExtraStore.Add(root); // Available, deliberately not trusted.
            bool valid = chain.Build(leaf);
            var statuses = chain.ChainStatus.Select(s => s.Status).ToArray();
            Console.WriteLine($"chain={valid}; statuses={string.Join(',', statuses)}; elements={chain.ChainElements.Count}; requests={string.Join(',', server.Requests)}; downloadsDisabled={chain.ChainPolicy.DisableCertificateDownloads}; trust={chain.ChainPolicy.TrustMode}; revocation={chain.ChainPolicy.RevocationMode}");
            bool expected = mode switch { "aia" => Environment.Version.Major == 10, "supplied" => true, _ => false };
            int requests = mode == "aia" ? (Environment.Version.Major == 10 ? 3 : 2) : 0;
            if (valid != expected || server.Requests.Count != requests) throw new InvalidOperationException("Chain/download expectation failed");
            if (mode == "invalid" && !statuses.Contains(X509ChainStatusFlags.NotTimeValid)) throw new InvalidOperationException("Expected expired-chain rejection");
            if (mode == "untrusted" && !statuses.Contains(X509ChainStatusFlags.UntrustedRoot)) throw new InvalidOperationException("Expected untrusted-root rejection");
            using var publicKey = leaf.GetRSAPublicKey()!;
            bool signatureValid = publicKey.VerifyData(manifest, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            if (mode == "supplied" && !ManifestVerifier.Accept(manifest, signature, leaf, root, new[] { first, second, third }))
                throw new InvalidOperationException("Canonical verifier failed");
            manifest[0] ^= 1;
            bool tampered = publicKey.VerifyData(manifest, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            if (mode == "supplied" && ManifestVerifier.Accept(manifest, signature, leaf, root, new[] { first, second, third }))
                throw new InvalidOperationException("Canonical verifier accepted tampering");
            if (!signatureValid || tampered) throw new InvalidOperationException("Manifest signature control");
            Console.WriteLine($"PASS {mode}; manifestSignature={signatureValid}; tamperedAccepted={tampered}; applicationAccepts={valid && signatureValid}");
        }
        finally { server.Stop(); await serving; Console.WriteLine("CLEANUP issuer listener stopped; all private keys/certificates disposed on exit; no shared trust writes"); }
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr OpenSslVersion(int type);
    private static CertificateRequest Request(string subject, RSA key, bool ca, Uri? issuer)
    {
        var request = new CertificateRequest("CN=" + subject, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(ca, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(ca ? X509KeyUsageFlags.KeyCertSign : X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        if (issuer is not null)
        {
            var aia = new AsnWriter(AsnEncodingRules.DER);
            aia.PushSequence(); aia.PushSequence(); aia.WriteObjectIdentifier("1.3.6.1.5.5.7.48.2");
            aia.WriteCharacterString(UniversalTagNumber.IA5String, issuer.AbsoluteUri, new Asn1Tag(TagClass.ContextSpecific, 6));
            aia.PopSequence(); aia.PopSequence();
            request.CertificateExtensions.Add(new X509Extension("1.3.6.1.5.5.7.1.1", aia.Encode(), false));
        }
        return request;
    }
    private static X509Certificate2 Issue(string name, RSA key, bool ca, X509Certificate2 issuer, Uri? url, bool expired = false)
    {
        using var publicCertificate = Request(name, key, ca, url).Create(issuer, new DateTimeOffset(issuer.NotBefore.ToUniversalTime()).AddDays(1), expired ? DateTimeOffset.UtcNow.AddDays(-1) : new DateTimeOffset(issuer.NotAfter.ToUniversalTime()).AddDays(-1), RandomNumberGenerator.GetBytes(16));
        return publicCertificate.CopyWithPrivateKey(key);
    }
    private sealed class Issuers : IDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource stop = new();
        internal Dictionary<string, byte[]> Certificates { get; } = [];
        internal ConcurrentQueue<string> Requests { get; } = new();
        internal Issuers() => listener.Start();
        internal Uri Url(string name) => new($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/{name}");
        internal async Task Serve()
        {
            try
            {
                while (!stop.IsCancellationRequested)
                {
                    using var client = await listener.AcceptTcpClientAsync(stop.Token);
                    await using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.ASCII, false, leaveOpen: true);
                    string line = await reader.ReadLineAsync(stop.Token) ?? throw new IOException("Missing HTTP request");
                    string path = line.Split(' ')[1]; Requests.Enqueue(path);
                    while (!string.IsNullOrEmpty(await reader.ReadLineAsync(stop.Token))) { }
                    byte[] body = Certificates[path];
                    await stream.WriteAsync(Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/pkix-cert\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n"), stop.Token);
                    await stream.WriteAsync(body, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (SocketException) when (stop.IsCancellationRequested) { }
        }
        internal void Stop() { stop.Cancel(); listener.Stop(); }
        public void Dispose() { Stop(); listener.Dispose(); stop.Dispose(); }
    }
}
