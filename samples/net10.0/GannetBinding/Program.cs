using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Text;

Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
Console.WriteLine($"ExtendedProtection={ExtendedProtectionPolicy.OSSupportsExtendedProtection}");
bool missingBindingAccepted = false;
bool noCredentials = args.Contains("--no-credentials", StringComparer.Ordinal);
using TlsPair station = await TlsPair.Connect();
using TlsPair otherStation = await TlsPair.Connect();
using ChannelBinding serverBinding = EndpointBinding.FromLocalCertificate(station.Server);
using ChannelBinding matching = station.Client.TransportContext!.GetChannelBinding(ChannelBindingKind.Endpoint)!;
using ChannelBinding different = otherStation.Client.TransportContext!.GetChannelBinding(ChannelBindingKind.Endpoint)!;
if (serverBinding is null || matching is null || different is null) throw new AuthenticationException("TLS endpoint binding unavailable.");
byte[] serverBytes = new byte[serverBinding.Size]; Marshal.Copy(serverBinding.DangerousGetHandle(), serverBytes, 0, serverBytes.Length);
byte[] clientBytes = new byte[matching.Size]; Marshal.Copy(matching.DangerousGetHandle(), clientBytes, 0, clientBytes.Length);
if (!serverBytes.AsSpan(32).SequenceEqual(clientBytes.AsSpan(32))) throw new AuthenticationException("Server certificate binding differs from the actual TLS client's binding.");
Console.WriteLine($"TLS endpoint binding verified against SslStream client; {serverBinding.Size} bytes; handles remain live throughout each exchange.");
foreach ((string scenario, ChannelBinding? binding, string target) in new[]
{
    ("matching", matching, "HTTP/station.gannet.test"),
    ("mismatched", different, "HTTP/station.gannet.test"),
    ("missing-client", (ChannelBinding?)null, "HTTP/station.gannet.test"),
    ("unknown-service", matching, "HTTP/absent.gannet.test")
})
{
    try
    {
        using NegotiateAuthentication server = GannetProtection.Create(serverBinding);
        using NegotiateAuthentication client = new(new NegotiateAuthenticationClientOptions
        {
            Package = "Kerberos", TargetName = target, Credential = CredentialCache.DefaultNetworkCredentials,
            Binding = binding, RequiredProtectionLevel = ProtectionLevel.EncryptAndSign
        });
        byte[]? token = client.GetOutgoingBlob(ReadOnlySpan<byte>.Empty, out NegotiateAuthenticationStatusCode clientStatus);
        NegotiateAuthenticationStatusCode serverStatus = NegotiateAuthenticationStatusCode.ContinueNeeded;
        for (int round = 0; round < 8 && token is not null; round++)
        {
            token = await Transfer(station.Client, station.Server, token);
            token = server.GetOutgoingBlob(token, out serverStatus);
            if (token is not null) token = client.GetOutgoingBlob(await Transfer(station.Server, station.Client, token), out clientStatus);
            if (serverStatus != NegotiateAuthenticationStatusCode.ContinueNeeded) break;
        }
        Console.WriteLine($"{scenario}: client={clientStatus}; server={serverStatus}; authenticated={server.IsAuthenticated}; mechanism={server.Package}");
#if NET11_0_OR_GREATER
        bool expected = !noCredentials && scenario == "matching";
#else
        bool expected = !noCredentials && scenario is "matching" or "missing-client";
#endif
        bool accepted = serverStatus == NegotiateAuthenticationStatusCode.Completed && server.IsAuthenticated && client.IsAuthenticated;
        if (scenario == "missing-client" && accepted) missingBindingAccepted = true;
        if (accepted != expected || (expected && server.Package != "Kerberos")) throw new InvalidOperationException($"Authentication expectation failed: {scenario}; expected={expected}; accepted={accepted}; client={clientStatus}; server={serverStatus}; runtime={RuntimeInformation.FrameworkDescription}.");
        if (accepted && (!server.IsEncrypted || !server.IsSigned || !client.IsEncrypted || !client.IsSigned)) throw new AuthenticationException("Required message protection absent.");
        if (!noCredentials && scenario == "mismatched" && serverStatus != NegotiateAuthenticationStatusCode.BadBinding) throw new AuthenticationException("Expected actual mismatch rejection.");
#if NET11_0_OR_GREATER
        if (!noCredentials && scenario == "missing-client" && serverStatus != NegotiateAuthenticationStatusCode.BadBinding) throw new AuthenticationException("Expected GA missing-binding rejection.");
#endif
    }
    catch (PlatformNotSupportedException) when (!ExtendedProtectionPolicy.OSSupportsExtendedProtection)
    {
        Console.WriteLine($"{scenario}: required policy unsupported; service refused before exchange");
    }
}
try { using var missing = GannetProtection.Create(null); throw new InvalidOperationException("Missing server binding accepted."); }
catch (AuthenticationException) { Console.WriteLine("missing-server: refused before exchange"); }
if (missingBindingAccepted)
{
    Console.WriteLine("UNMET: MIT Kerberos accepts a client with no binding. This route does not meet mandatory missing-binding rejection; do not enable a service requiring it.");
    if (args.Contains("--require-missing-rejection", StringComparer.Ordinal)) return 2;
}
return 0;

static async Task<byte[]> Transfer(SslStream writer, SslStream reader, byte[] token)
{
    using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
    await writer.WriteAsync(token, deadline.Token);
    byte[] received = new byte[token.Length];
    await reader.ReadExactlyAsync(received, deadline.Token);
    return received;
}



sealed class TlsPair(TcpClient clientSocket, TcpClient serverSocket, SslStream client, SslStream server, X509Certificate2 certificate) : IDisposable
{
    public SslStream Client => client;
    public SslStream Server => server;
    public static async Task<TlsPair> Connect()
    {
        using RSA key = RSA.Create(2048);
        CertificateRequest request = new("CN=station.gannet.test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        SubjectAlternativeNameBuilder names = new(); names.AddDnsName("station.gannet.test");
        request.CertificateExtensions.Add(names.Build());
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, true));
        using X509Certificate2 generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
        X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
        using TcpListener listener = new(IPAddress.Loopback, 0); listener.Start();
        TcpClient clientSocket = new();
        await clientSocket.ConnectAsync((IPEndPoint)listener.LocalEndpoint);
        TcpClient serverSocket = await listener.AcceptTcpClientAsync();
        SslStream client = new(clientSocket.GetStream()); SslStream server = new(serverSocket.GetStream());
        X509ChainPolicy trust = new() { TrustMode = X509ChainTrustMode.CustomRootTrust, RevocationMode = X509RevocationMode.NoCheck, DisableCertificateDownloads = true };
        trust.CustomTrustStore.Add(certificate);
        await Task.WhenAll(client.AuthenticateAsClientAsync(new SslClientAuthenticationOptions { TargetHost = "station.gannet.test", CertificateChainPolicy = trust }), server.AuthenticateAsServerAsync(new SslServerAuthenticationOptions { ServerCertificate = certificate })).WaitAsync(TimeSpan.FromSeconds(15));
        return new(clientSocket, serverSocket, client, server, certificate);
    }
    public void Dispose() { client.Dispose(); server.Dispose(); clientSocket.Dispose(); serverSocket.Dispose(); certificate.Dispose(); }
}

// snippet:endpoint-binding
sealed class EndpointBinding : ChannelBinding
{
    private readonly int size;
    private EndpointBinding(byte[] applicationData) : base(true)
    {
        size = 32 + applicationData.Length;
        SetHandle(Marshal.AllocHGlobal(size));
        Marshal.Copy(new byte[size], 0, handle, size);
        Marshal.WriteInt32(handle, 24, applicationData.Length);
        Marshal.WriteInt32(handle, 28, 32);
        Marshal.Copy(applicationData, 0, handle + 32, applicationData.Length);
    }
    public override int Size => size;
    public static EndpointBinding FromLocalCertificate(SslStream tls)
    {
        if (!tls.IsAuthenticated || !tls.IsServer || tls.LocalCertificate is null)
            throw new AuthenticationException("An authenticated server TLS connection is required.");
        using X509Certificate2 certificate = new(tls.LocalCertificate);
        // This bounded route supports SHA256-with-RSA server certificates only.
        if (certificate.SignatureAlgorithm.Value != "1.2.840.113549.1.1.11")
            throw new AuthenticationException("This sample requires a SHA256-with-RSA certificate.");
        byte[] prefix = Encoding.ASCII.GetBytes("tls-server-end-point:");
        byte[] hash = SHA256.HashData(certificate.RawData);
        return new EndpointBinding([.. prefix, .. hash]);
    }
    protected override bool ReleaseHandle() { Marshal.FreeHGlobal(handle); return true; }
}
// endsnippet:endpoint-binding
