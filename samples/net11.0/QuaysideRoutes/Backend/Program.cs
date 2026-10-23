// snippet:main
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var reads = 0;
app.MapGet("/api/quays/{berth}", (string berth, HttpContext context) =>
{
    context.Response.Headers["X-Quayside-Backend"] = "real-loopback";
    return $"Quay {berth}; cargo={context.Request.Query["cargo"]}; read={Interlocked.Increment(ref reads)}";
});
await app.RunAsync();
// endsnippet:main
