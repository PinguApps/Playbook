// snippet:complete
using MaritimeGallery;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
var app = builder.Build();
app.UsePathBase("/museum");
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.Run();
// endsnippet:complete
