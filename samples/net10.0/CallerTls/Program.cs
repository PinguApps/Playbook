using Samples;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using X509Certificate2 certificate = TransportCertificate.Create();
using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(15));
using TcpListener listener = new(IPAddress.Loopback, 0);
listener.Start();
byte[] parcel = "dispatch:bay-17:ready"u8.ToArray();
Task peer = Task.Run(async () =>
{
    using TcpClient socket = await listener.AcceptTcpClientAsync(timeout.Token);
    using SslStream secure = new(socket.GetStream());
    await secure.AuthenticateAsServerAsync(new SslServerAuthenticationOptions { ServerCertificate = certificate, EnabledSslProtocols = SslProtocols.None }, timeout.Token);
    byte[] buffer = new byte[parcel.Length];
    await secure.ReadExactlyAsync(buffer, timeout.Token);
    await secure.WriteAsync(buffer, timeout.Token);
});
using TcpClient client = new();
await client.ConnectAsync((IPEndPoint)listener.LocalEndpoint, timeout.Token);
byte[] reply = await DispatchChannel.SendAsync(client.GetStream(), TransportCertificate.Client(certificate), parcel, timeout.Token);
await peer;
if (!reply.SequenceEqual(parcel)) { throw new InvalidOperationException("TLS echo mismatch."); }
Console.WriteLine($"PASS: SslStream TLS 1.3 real TCP echo; {RuntimeInformation.FrameworkDescription}");
