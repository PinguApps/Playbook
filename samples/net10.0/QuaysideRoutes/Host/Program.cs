// snippet:main
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
if (app.Environment.IsDevelopment()) app.UseWebAssemblyDebugging();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapReverseProxy();
app.MapFallbackToFile("index.html");
await app.RunAsync();
// endsnippet:main
