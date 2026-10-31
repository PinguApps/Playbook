using System.Collections.Concurrent;
using System.Formats.Asn1;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

await TlsChecks.Run(args.Single());

internal static class TlsChecks
{
    internal static async Task Run(string mode)
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Isolated Linux/OpenSSL fixture");
        Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}; mode={mode}");
        Console.WriteLine($"privateHome={Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}; preexistingDotnetCache={Directory.Exists("/root/.dotnet/corefx")}");
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(25));
        using var issuer = new Issuer();
        using var rootKey = RSA.Create(2048);
        using var intermediateKey = RSA.Create(2048);
        using var clientKey = RSA.Create(2048);
        using var serverKey = RSA.Create(2048);
        using var wrongKey = RSA.Create(2048);
        using var root = Request("Room private client root " + Guid.NewGuid().ToString("N"), rootKey, true, null).CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-20), DateTimeOffset.UtcNow.AddDays(20));
        using var wrongRoot = Request("Room unrelated root", wrongKey, true, null).CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-20), DateTimeOffset.UtcNow.AddDays(20));
        using var intermediate = Issue("Room client issuer", intermediateKey, true, root, null);
        using var clientCertificate = Issue("Room booking desk", clientKey, false, intermediate, issuer.Url, mode == "invalid");
        var serverRequest = Request("room.test", serverKey, false, null, server: true);
        var san = new SubjectAlternativeNameBuilder(); san.AddDnsName("room.test");
        serverRequest.CertificateExtensions.Add(san.Build());
        using var serverCertificate = serverRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        foreach (var cert in new[] { root, intermediate, clientCertificate, serverCertificate })
            Console.WriteLine($"INPUT subject={cert.Subject}; sha256={cert.GetCertHashString(HashAlgorithmName.SHA256)}; notBefore={cert.NotBefore.ToUniversalTime():O}; notAfter={cert.NotAfter.ToUniversalTime():O}");
        var serverContext = RoomContexts.Server(serverCertificate, root);
        var clientOptions = RoomContexts.Client(clientCertificate, serverCertificate);
        if (clientOptions.ClientCertificateContext!.IntermediateCertificates.Count != 0) throw new InvalidOperationException("Client must send leaf only");
        issuer.Certificate = intermediate.RawData;
        Task issuerTask = issuer.Serve();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        bool accepted = false, exchanged = false;
        SslPolicyErrors observedErrors = SslPolicyErrors.None;
        X509ChainStatusFlags[] observedStatus = [];
        bool? observedDownloadsDisabled = null;
        try
        {
            var serverOptions = DefaultOptions(serverContext);
            if (mode != "default") serverOptions = SuppliedOptions(serverContext, mode == "untrusted" ? wrongRoot : root, intermediate);
            serverOptions.RemoteCertificateValidationCallback = (_, certificate, chain, errors) =>
            {
                observedErrors = errors;
                observedStatus = chain?.ChainStatus.Select(s => s.Status).ToArray() ?? [];
                observedDownloadsDisabled = chain?.ChainPolicy.DisableCertificateDownloads;
                Console.WriteLine($"VALIDATION present={certificate is not null}; errors={errors}; statuses={string.Join(',', observedStatus)}; downloadsDisabled={observedDownloadsDisabled}");
                return certificate is not null && errors == SslPolicyErrors.None;
            };
            Task serverTask = Task.Run(async () =>
            {
                using var socket = await listener.AcceptTcpClientAsync(deadline.Token);
                await using var tls = new SslStream(socket.GetStream());
                try
                {
                    await tls.AuthenticateAsServerAsync(serverOptions, deadline.Token);
                    accepted = tls.IsAuthenticated && tls.IsMutuallyAuthenticated;
                    byte[] booking = new byte[3];
                    await tls.ReadExactlyAsync(booking, deadline.Token);
                    if (!booking.SequenceEqual("R17"u8.ToArray())) throw new InvalidOperationException("Booking payload mismatch");
                    await tls.WriteAsync("ACK"u8.ToArray(), deadline.Token);
                    exchanged = true;
                    Console.WriteLine($"SERVER protocol={tls.SslProtocol}; mutual={tls.IsMutuallyAuthenticated}");
                }
                catch (AuthenticationException ex) { Console.WriteLine($"SERVER rejected={ex.GetType().Name}; {ex.Message}"); }
            }, deadline.Token);
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port, deadline.Token);
            await using (var tls = new SslStream(client.GetStream()))
            {
                try
                {
                    await tls.AuthenticateAsClientAsync(clientOptions, deadline.Token);
                    await tls.WriteAsync("R17"u8.ToArray(), deadline.Token);
                    byte[] reply = new byte[3];
                    await tls.ReadExactlyAsync(reply, deadline.Token);
                    if (!reply.SequenceEqual("ACK"u8.ToArray())) throw new InvalidOperationException("Reply mismatch");
                }
                catch (Exception ex) when (ex is AuthenticationException or IOException) { Console.WriteLine($"CLIENT peer rejected={ex.GetType().Name}"); }
            }
            await serverTask;
            bool expected = mode == "supplied" || (mode == "default" && Environment.Version.Major == 10);
            int requests = mode == "default" && Environment.Version.Major == 10 ? 1 : 0;
            if (accepted != expected || exchanged != expected || issuer.Requests.Count != requests) throw new InvalidOperationException("TLS/download/data expectation failed");
            if (mode == "invalid" && !observedStatus.Contains(X509ChainStatusFlags.NotTimeValid)) throw new InvalidOperationException("Expired client not rejected");
            if (mode == "untrusted" && !observedStatus.Contains(X509ChainStatusFlags.PartialChain) && !observedStatus.Contains(X509ChainStatusFlags.UntrustedRoot)) throw new InvalidOperationException("Untrusted client not rejected");
            if (mode == "default" && Environment.Version.Major == 11 && !observedStatus.Contains(X509ChainStatusFlags.PartialChain)) throw new InvalidOperationException("Expected missing intermediate");
            if (observedDownloadsDisabled != (mode != "default" || Environment.Version.Major == 11)) throw new InvalidOperationException("Unexpected TLS download policy");
            Console.WriteLine($"PASS {mode}; authenticated={accepted}; exchanged={exchanged}; requests={string.Join(',', issuer.Requests)}; requestCount={issuer.Requests.Count}; errors={observedErrors}");
            NativeIdentity();
        }
        finally { listener.Stop(); issuer.Stop(); await issuerTask; Console.WriteLine("CLEANUP TLS and issuer listeners stopped; private certificates/keys disposed; no shared trust writes"); }
    }
    private static SslServerAuthenticationOptions DefaultOptions(SslStreamCertificateContext context)
    {
#if NET10_0
        return RoomTls.Create(context);
#else
        return new() { ServerCertificateContext = context, ClientCertificateRequired = true, CertificateRevocationCheckMode = X509RevocationMode.NoCheck };
#endif
    }
    private static SslServerAuthenticationOptions SuppliedOptions(SslStreamCertificateContext context, X509Certificate2 root, X509Certificate2 intermediate)
    {
#if NET11_0
        return RoomTls.Create(context, root, intermediate);
#else
        var options = RoomTls.Create(context);
        var policy = new X509ChainPolicy { TrustMode = X509ChainTrustMode.CustomRootTrust, RevocationMode = X509RevocationMode.NoCheck, DisableCertificateDownloads = true };
        policy.CustomTrustStore.Add(root); policy.ExtraStore.Add(intermediate);
        options.CertificateChainPolicy = policy;
        return options;
#endif
    }
    private static CertificateRequest Request(string name, RSA key, bool ca, Uri? issuer, bool server = false)
    {
        var request = new CertificateRequest("CN=" + name, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(ca, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(ca ? X509KeyUsageFlags.KeyCertSign : X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        if (!ca) request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid(server ? "1.3.6.1.5.5.7.3.1" : "1.3.6.1.5.5.7.3.2") }, true));
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
        using var certificate = Request(name, key, ca, url).Create(issuer, DateTimeOffset.UtcNow.AddDays(-10), expired ? DateTimeOffset.UtcNow.AddDays(-1) : DateTimeOffset.UtcNow.AddDays(10), RandomNumberGenerator.GetBytes(16));
        return certificate.CopyWithPrivateKey(key);
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr OpenSslVersion(int type);
    private static void NativeIdentity()
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        foreach (System.Diagnostics.ProcessModule module in process.Modules)
        {
            if (!Path.GetFileName(module.FileName).StartsWith("libcrypto.so", StringComparison.Ordinal) && !Path.GetFileName(module.FileName).StartsWith("libssl.so", StringComparison.Ordinal)) continue;
            Console.WriteLine($"NATIVE path={module.FileName}; sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(module.FileName)))}");
            if (!Path.GetFileName(module.FileName).StartsWith("libcrypto.so", StringComparison.Ordinal)) continue;
            IntPtr library = NativeLibrary.Load(module.FileName);
            try { Console.WriteLine($"NATIVE {Marshal.PtrToStringAnsi(Marshal.GetDelegateForFunctionPointer<OpenSslVersion>(NativeLibrary.GetExport(library, "OpenSSL_version"))(0))}"); }
            finally { NativeLibrary.Free(library); }
        }
        foreach (var assembly in new[] { typeof(SslStream).Assembly, typeof(X509Chain).Assembly })
            Console.WriteLine($"MANAGED path={assembly.Location}; sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))}");
    }
    private sealed class Issuer : IDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource stop = new();
        internal ReadOnlyMemory<byte> Certificate { get; set; }
        internal ConcurrentQueue<string> Requests { get; } = new();
        internal Issuer() => listener.Start();
        internal Uri Url => new($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/client-issuer");
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
                    Requests.Enqueue(line.Split(' ')[1]);
                    while (!string.IsNullOrEmpty(await reader.ReadLineAsync(stop.Token))) { }
                    await stream.WriteAsync(Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/pkix-cert\r\nContent-Length: {Certificate.Length}\r\nConnection: close\r\n\r\n"), stop.Token);
                    await stream.WriteAsync(Certificate, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (SocketException) when (stop.IsCancellationRequested) { }
        }
        internal void Stop() { stop.Cancel(); listener.Stop(); }
        public void Dispose() { Stop(); listener.Dispose(); stop.Dispose(); }
    }
}
