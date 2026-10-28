// snippet:complete
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.RequestDecompression;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 1024);
// snippet:policy
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<ZstandardCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ["text/plain"];
    // HTTPS remains disabled. Assess secret-reflection risks before enabling it.
});
builder.Services.Configure<ZstandardCompressionProviderOptions>(options =>
    options.CompressionOptions = new ZstandardCompressionOptions { Quality = 3, WindowLog2 = 20 });
builder.Services.AddRequestDecompression(options =>
{
    // Retain the built-in instances: their concrete provider classes are internal.
    options.DecompressionProviders.Remove("br");
    options.DecompressionProviders.Remove("deflate");
});
// endsnippet:policy
await using var app = builder.Build();
app.UseResponseCompression();
app.UseRequestDecompression();
// Supported single encodings are removed by decompression. Reject remaining encodings explicitly.
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Content-Encoding")) { context.Response.StatusCode = 415; return; }
    await next(context);
});
string bulletin = string.Concat(Enumerable.Repeat("Harbour tide bulletin. ", 20));
app.MapGet("/bulletin", () => Results.Text(bulletin));
app.MapPost("/receipt", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        return Results.Text(await reader.ReadToEndAsync(context.RequestAborted));
    }
    catch (BadHttpRequestException error) when (error.StatusCode == 413) { return Results.StatusCode(413); }
    catch (InvalidDataException) { return Results.BadRequest(); }
});
await app.StartAsync();
string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.None, UseProxy = false, CheckCertificateRevocationList = true };
using var client = new HttpClient(handler) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
foreach ((string accept, string? expected) in new (string, string?)[]
{
    ("zstd", "zstd"), ("gzip", "gzip"), ("zstd;q=0.2, gzip;q=0.9", "gzip"),
    ("zstd;q=0, gzip", "gzip"), ("br", null), ("snappy", null), ("identity", null)
})
{
    using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/bulletin", UriKind.Relative));
    request.Headers.TryAddWithoutValidation("Accept-Encoding", accept);
    using HttpResponseMessage response = await client.SendAsync(request);
    string? actual = response.Content.Headers.ContentEncoding.SingleOrDefault();
    byte[] body = await response.Content.ReadAsByteArrayAsync();
    string restored = Encoding.UTF8.GetString(Decode(body, actual));
    if (response.StatusCode != HttpStatusCode.OK || actual != expected || restored != bulletin) { throw new Exception($"Negotiation failed: {accept} -> {actual}."); }
}
foreach (string encoding in new[] { "gzip", "zstd", "identity", "br", "snappy", "gzip, zstd" })
{
    byte[] payload = encoding is "gzip" or "zstd" ? Encode(bulletin, encoding) : Encoding.UTF8.GetBytes(bulletin);
    using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/receipt", UriKind.Relative)) { Content = new ByteArrayContent(payload) };
    request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
    if (encoding != "identity") { request.Content.Headers.TryAddWithoutValidation("Content-Encoding", encoding); }
    using HttpResponseMessage response = await client.SendAsync(request);
    bool supported = encoding is "gzip" or "zstd" or "identity";
    if (response.StatusCode != (supported ? HttpStatusCode.OK : HttpStatusCode.UnsupportedMediaType)) { throw new Exception($"Request decoding status failed: {encoding}/{response.StatusCode}."); }
    if (supported && await response.Content.ReadAsStringAsync() != bulletin) { throw new Exception($"Request decode changed body: {encoding}."); }
}
foreach (string encoding in new[] { "gzip", "zstd" })
{
    using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/receipt", UriKind.Relative)) { Content = new ByteArrayContent(Encode(new string('x', 2048), encoding)) };
    request.Content.Headers.ContentEncoding.Add(encoding);
    using HttpResponseMessage response = await client.SendAsync(request);
    if (response.StatusCode != HttpStatusCode.RequestEntityTooLarge) { throw new Exception("Decompressed size limit was not enforced."); }
}
await app.StopAsync();
Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: explicit zstd/gzip response selection and quality/exclusion; real request decoding; identity; unsupported/stacked 415; decompressed-size 413. No automatic provider defaults used.");

static byte[] Encode(string text, string encoding)
{
    using var bytes = new MemoryStream();
    using (Stream compressor = encoding == "zstd"
        ? new ZstandardStream(bytes, new ZstandardCompressionOptions { Quality = 3 }, leaveOpen: true)
        : new GZipStream(bytes, CompressionLevel.Fastest, leaveOpen: true))
    {
        byte[] input = Encoding.UTF8.GetBytes(text);
        compressor.Write(input);
    }
    return bytes.ToArray();
}
static byte[] Decode(byte[] body, string? encoding)
{
    if (encoding is null) { return body; }
    using var bytes = new MemoryStream(body);
    using Stream decoder = encoding == "zstd" ? new ZstandardStream(bytes, CompressionMode.Decompress) : new GZipStream(bytes, CompressionMode.Decompress);
    using var restored = new MemoryStream();
    decoder.CopyTo(restored);
    return restored.ToArray();
}
// endsnippet:complete
