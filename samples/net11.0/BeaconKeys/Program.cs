// snippet:complete
using BeaconKeys;
using System.Buffers.Text;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Logging.ClearProviders();
builder.Services.AddRazorComponents();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<MemoryKeyStore>();
builder.Services.AddScoped<IUserStore<IdentityUser>>(services => services.GetRequiredService<MemoryKeyStore>());
builder.Services.AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddSignInManager();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.Configure<IdentityPasskeyOptions>(options => options.ServerDomain = "localhost");
// Keep data-protection keys in memory for this disposable loopback fixture.
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
await using WebApplication app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/options", async (SignInManager<IdentityUser> signIn) => Results.Content(await signIn.MakePasskeyRequestOptionsAsync(null), "application/json"));
app.MapPost("/signin", async (HttpRequest request, SignInManager<IdentityUser> signIn) =>
{
    using StreamReader reader = new(request.Body);
    SignInResult result = await signIn.PasskeySignInAsync(await reader.ReadToEndAsync());
    return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<SignInPanel>(new { Result = result });
});
app.MapGet("/keys/rename/{id}", async (string id, HttpContext context, UserManager<IdentityUser> users, IAntiforgery antiforgery) =>
{
    IdentityUser? owner = await users.GetUserAsync(context.User);
    byte[] credential;
    try { credential = Base64Url.DecodeFromChars(id); }
    catch (FormatException) { return Results.BadRequest(); }
    UserPasskeyInfo? passkey = owner is null ? null : await users.GetPasskeyAsync(owner, credential);
    if (passkey is null) { return Results.NotFound(); }
    return (IResult)new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<RenamePanel>(new
    {
        Token = antiforgery.GetAndStoreTokens(context).RequestToken!, Name = passkey.Name
    });
}).RequireAuthorization();
app.MapPost("/keys/rename/{id}", async (string id, HttpContext context, UserManager<IdentityUser> users, IAntiforgery antiforgery) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest(); }
    IdentityUser? owner = await users.GetUserAsync(context.User);
    byte[] credential;
    try { credential = Base64Url.DecodeFromChars(id); }
    catch (FormatException) { return Results.BadRequest(); }
    IFormCollection form = await context.Request.ReadFormAsync();
    return owner is not null && await PasskeyLabels.RenameAsync(users, owner, credential, form["name"].ToString())
        ? Results.Redirect("/keys") : Results.BadRequest();
}).RequireAuthorization();
await app.StartAsync();
try { await Proof.RunAsync(app); }
finally { await app.StopAsync(); }
// endsnippet:complete
