// snippet:complete
using Microsoft.AspNetCore.Authentication.JwtBearer;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Never use the development signing key as a production identity provider.
if (!builder.Environment.IsDevelopment()) { throw new InvalidOperationException("Development example only."); }
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();
WebApplication app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/permit", (System.Security.Claims.ClaimsPrincipal user) => Results.Text($"{user.Identity!.Name} may inspect W3.")).RequireAuthorization();
await app.RunAsync();
// endsnippet:complete
