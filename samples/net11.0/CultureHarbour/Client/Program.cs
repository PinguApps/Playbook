// snippet:complete
using System.Globalization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddLocalization();
var host = builder.Build();
var js = host.Services.GetRequiredService<IJSRuntime>();
try
{
    await js.InvokeVoidAsync("cultureHarbour.observe", "before-run",
        CultureInfo.CurrentCulture.Name, CultureInfo.CurrentUICulture.Name);
    var selected = await js.InvokeAsync<string[]?>("cultureHarbour.selection");
    if (selected is { Length: 2 })
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(selected[0]);
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(selected[1]);
    }
}
catch (JSException error)
{
    throw new InvalidOperationException("Culture selection failed; stop before rendering.", error);
}
await host.RunAsync();
// endsnippet:complete
