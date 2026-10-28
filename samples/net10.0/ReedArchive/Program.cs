// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReedArchive;

using var services = new ServiceCollection().AddLogging().AddArchive().BuildServiceProvider(validateScopes: true);
using var first = services.CreateScope();
using var second = services.CreateScope();
var a = first.ServiceProvider.GetRequiredService<ArchiveState>();
var b = second.ServiceProvider.GetRequiredService<ArchiveState>();
await using var rendererA = new HtmlRenderer(first.ServiceProvider, first.ServiceProvider.GetRequiredService<ILoggerFactory>());
await using var rendererB = new HtmlRenderer(second.ServiceProvider, second.ServiceProvider.GetRequiredService<ILoggerFactory>());
ArchiveHost? hostA = null;
var capture = ParameterView.FromDictionary(new Dictionary<string, object?> { ["Capture"] = (Action<ArchiveHost>)(host => hostA = host) });
var rootA = await rendererA.Dispatcher.InvokeAsync(() => rendererA.RenderComponentAsync<ArchiveHost>(capture));
var rootB = await rendererB.Dispatcher.InvokeAsync(() => rendererB.RenderComponentAsync<ArchiveHost>());
Check(!ReferenceEquals(a, b) && !ReferenceEquals(a.Letters, b.Letters), "independent DI scopes");
Check(await rendererA.Dispatcher.InvokeAsync(rootA.ToHtmlString) == "<p>letters:0; maps:0</p>", "initial selection");
await rendererA.Dispatcher.InvokeAsync(a.AddLettersAsync);
Check(await rendererA.Dispatcher.InvokeAsync(rootA.ToHtmlString) == "<p>letters:1; maps:0</p>", "letters notification rendered");
await rendererA.Dispatcher.InvokeAsync(a.AddMapsAsync);
Check(await rendererA.Dispatcher.InvokeAsync(rootA.ToHtmlString) == "<p>letters:1; maps:1</p>", "maps notification rendered independently");
Check(await rendererB.Dispatcher.InvokeAsync(rootB.ToHtmlString) == "<p>letters:0; maps:0</p>", "other scope unchanged");
await hostA!.HideAsync();
await rendererA.Dispatcher.InvokeAsync(a.AddLettersAsync);
Check((await rendererA.Dispatcher.InvokeAsync(rootA.ToHtmlString)).Length == 0, "removed subscriber stays absent");
Console.WriteLine($"PASS ReedArchive: real HtmlRenderer selection, updates, scope isolation and removal; runtime {Environment.Version}.");
static void Check(bool condition, string claim) { if (!condition) throw new InvalidOperationException(claim); }
// endsnippet:complete
