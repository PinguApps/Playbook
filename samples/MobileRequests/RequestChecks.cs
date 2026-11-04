using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;
using Playbook.Samples;

RuntimeIdentity.Assert();
using var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
var endpoint = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/observation");
using var handler = new SocketsHttpHandler { UseProxy = false };
using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
foreach (int status in new[] { 200, 503 })
{
    Task server = Reply(status);
    try
    {
        string report = await MoorlandReports.ReadAsync(client, endpoint, CancellationToken.None);
        if (status != 200 || report != "moorland:17") throw new Exception("Unexpected response");
    }
    catch (HttpRequestException ex) when (status == 503 && ex.StatusCode == HttpStatusCode.ServiceUnavailable) { }
    await server;
}
Task<TcpClient> accept = listener.AcceptTcpClientAsync();
using var cancellation = new CancellationTokenSource();
Task<string> pending = MoorlandReports.ReadAsync(client, endpoint, cancellation.Token);
using (TcpClient connection = await accept.WaitAsync(TimeSpan.FromSeconds(10)))
{
    byte[] request = new byte[4096];
    _ = await connection.GetStream().ReadAsync(request).AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    await cancellation.CancelAsync();
    try { await pending; throw new Exception("Cancellation ignored"); }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
}
Task<TcpClient> failedAccept = listener.AcceptTcpClientAsync();
Task<string> failed = MoorlandReports.ReadAsync(client, endpoint, CancellationToken.None);
using (TcpClient connection = await failedAccept.WaitAsync(TimeSpan.FromSeconds(10)))
{
    connection.Client.LingerState = new LingerOption(true, 0);
}
try { await failed; throw new Exception("Transport failure ignored"); }
catch (HttpRequestException) { }
Console.WriteLine("PASS real desktop HTTP success/status failure/in-flight cancellation/transport failure");

async Task Reply(int status)
{
    using TcpClient connection = await listener.AcceptTcpClientAsync().WaitAsync(TimeSpan.FromSeconds(10));
    byte[] request = new byte[4096];
    _ = await connection.GetStream().ReadAsync(request).AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    byte[] response = System.Text.Encoding.ASCII.GetBytes($"HTTP/1.1 {status} Result\r\nContent-Length: 11\r\nConnection: close\r\n\r\nmoorland:17");
    // Body has eleven bytes; status failures still dispose the response.
    await connection.GetStream().WriteAsync(response);
}
