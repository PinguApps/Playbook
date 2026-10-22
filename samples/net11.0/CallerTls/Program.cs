using Samples;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable SYSLIB5007
using X509Certificate2 certificate = TransportCertificate.Create();
// snippet:tls-use
using TlsContext clientContext = TlsContext.CreateClient(TransportCertificate.Client(certificate));
using TlsContext serverContext = TlsContext.CreateServer(new()
{
    ServerCertificate = certificate,
    EnabledSslProtocols = SslProtocols.None
});
using TlsBufferSession client = new();
using TlsBufferSession server = new();
client.SetContext(clientContext);
server.SetContext(serverContext);
DispatchHandshake.Authenticate(client, server);
byte[] parcel = "dispatch:bay-17:ready"u8.ToArray();
byte[] received = DispatchHandshake.Transfer(client, server, parcel);
// endsnippet:tls-use
if (!received.SequenceEqual(parcel) || !DispatchHandshake.Transfer(server, client, received).SequenceEqual(parcel))
{
    throw new InvalidOperationException("TLS payload mismatch.");
}
byte[] large = Enumerable.Range(0, 50000).Select(i => (byte)i).ToArray();
if (!DispatchHandshake.Transfer(client, server, large).SequenceEqual(large))
{
    throw new InvalidOperationException("Multi-record payload mismatch.");
}
using TlsContext wrongHost = TlsContext.CreateClient(TransportCertificate.Client(certificate, "wrong.example.test"));
using TlsBufferSession rejectedClient = new();
using TlsBufferSession rejectedServer = new();
rejectedClient.SetContext(wrongHost);
rejectedServer.SetContext(serverContext);
try
{
    DispatchHandshake.Authenticate(rejectedClient, rejectedServer);
    throw new InvalidOperationException("Wrong host was accepted.");
}
catch (AuthenticationException exception) when (exception.Message == "Peer certificate rejected.") { }
byte[] alert = new byte[16384];
client.Shutdown(alert, out int alertSize);
TlsOperationStatus close = server.Read(alert.AsSpan(0, alertSize), new byte[16384], out _, out _);
if (close != TlsOperationStatus.Closed) { throw new InvalidOperationException($"Expected close_notify: {close}"); }
Console.WriteLine($"PASS: caller-owned TLS {client.NegotiatedProtocol}; fragmented handshake, bidirectional data, 50000 bytes, wrong-host rejection, close_notify; {RuntimeInformation.FrameworkDescription}");
#pragma warning restore SYSLIB5007
