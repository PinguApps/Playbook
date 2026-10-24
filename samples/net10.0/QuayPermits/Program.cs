// snippet:complete
using QuayPermits;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
bool keepTokens = true;
builder.Services.AddSingleton(new ProtectionMode(keepTokens));
builder.Services.AddRazorComponents();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddCors(options => options.AddPolicy("dispatch",
    policy => policy.WithOrigins("http://localhost:6019").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
if (keepTokens)
{
    builder.Services.AddAntiforgery();
    builder.Services.AddSingleton<IAntiforgeryAdditionalDataProvider>(
        new PermitAdditionalData(builder.Configuration.GetValue<bool>("reject-additional")));
}
WebApplication app = builder.Build();
app.UseCors();
if (keepTokens) { app.UseAntiforgery(); }
app.MapGet("/minimal", (HttpContext context, IAntiforgery antiforgery) =>
    Results.Content(FormDocument.Create("/minimal", context, antiforgery, keepTokens), "text/html"));
app.MapPost("/minimal", ([FromForm] string cargo) => Results.Text("Minimal accepted: " + cargo));
app.MapPost("/trusted", ([FromForm] string cargo) => Results.Text("Trusted accepted: " + cargo)).RequireCors("dispatch");
app.MapPost("/json", (PermitRequest request) => Results.Text("JSON accepted: " + request.Cargo));
app.MapControllers();
app.MapRazorPages();
app.MapRazorComponents<App>();
app.Run();
// endsnippet:complete
