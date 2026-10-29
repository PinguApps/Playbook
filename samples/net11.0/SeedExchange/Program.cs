// snippet:complete
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using SeedExchange;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
bool tokens = builder.Configuration.GetValue<bool>("tokens");
bool standard = builder.Configuration.GetValue<bool>("standard");
ExchangeOrigins origins = new(builder.Configuration.GetSection("TrustedOrigins").Get<string[]>() ?? []);
builder.Services.AddSingleton(origins);
builder.Services.AddSingleton<Observations>();
if (!standard && !tokens)
{
    builder.Services.AddSingleton<ICsrfProtection, ExchangeProtection>();
}
builder.Services.AddAntiforgery();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddCors(options =>
{
    options.AddPolicy("credentialed", policy => policy.WithOrigins("http://localhost:6181").AllowAnyHeader().AllowAnyMethod().AllowCredentials());
    options.AddPolicy("public", policy => policy.WithOrigins("http://localhost:6181").AllowAnyHeader().AllowAnyMethod());
    options.AddPolicy("wildcard", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
WebApplication app = builder.Build();
app.Use(async (context, next) =>
{
    try { await next(context); }
    finally { context.RequestServices.GetRequiredService<Observations>().Completed(context); }
});
app.UseStaticFiles();
app.UseCors();
// Separate token-required execution mode; do not infer two enforced independent layers.
if (tokens) { app.UseAntiforgery(); }
app.MapGet("/", () => Results.Content("<title>Fern Ledger seed exchange</title><h1>Fern Ledger</h1><p>Disposable botanical exchange browser controls.</p>", "text/html"));
app.MapGet("/session", (HttpContext context) =>
{
    context.Response.Cookies.Append("seed-session", "demo", new CookieOptions { SameSite = SameSiteMode.Lax, HttpOnly = true });
    return Results.Text("Demonstration cookie only; no authenticated identity.");
});
app.MapGet("/form", (HttpContext context, IAntiforgery antiforgery) =>
{
    string hidden = tokens ? $"<input type=hidden name=__RequestVerificationToken value='{antiforgery.GetAndStoreTokens(context).RequestToken}'>" : "";
    return Results.Content($"<form method=post action='/seed'>{hidden}<input name=seed value=Fern><button>Offer seed</button></form>", "text/html");
});
foreach (string path in new[] { "/seed", "/credentialed", "/public", "/wildcard" })
{
    var endpoint = app.MapPost(path, ([FromForm] string seed, HttpContext context, Observations observations) =>
    {
        observations.Handled(context);
        return Results.Text("Seed accepted: " + seed);
    });
    if (path != "/seed") { endpoint.RequireCors(path[1..]); }
}
// JSON has no automatic form metadata. It needs its own authentication/authorization design.
app.MapPost("/json", (SeedOffer offer, HttpContext context, Observations observations) =>
{
    observations.Handled(context);
    return Results.Text("JSON accepted: " + offer.Seed);
});
app.MapGet("/observations", (Observations observations) => observations.Snapshot());
if (builder.Configuration.GetValue<bool>("verify"))
{
    app.Urls.Clear();
    app.Urls.Add("http://127.0.0.1:0");
    await app.StartAsync();
    try { await RequestChecks.Run(app, tokens, standard); }
    finally { await app.StopAsync(); await app.DisposeAsync(); }
}
else { await app.RunAsync(); }


// endsnippet:complete
