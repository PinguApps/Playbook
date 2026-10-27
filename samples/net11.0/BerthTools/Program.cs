// snippet:complete
using BerthTools;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer().WithHttpTransport(options => options.Stateless = true).WithTools<BerthLookup>();
WebApplication app = builder.Build();
app.MapMcp();
await app.RunAsync();
// endsnippet:complete
