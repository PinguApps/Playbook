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
Check(await rendererA.Dispatcher.InvokeAsync(() => Html(rootA.ToHtmlString())) == "<p>letters:0; maps:0</p>", "initial selection");
await rendererA.Dispatcher.InvokeAsync(a.Letters.AddAsync);
Check(await rendererA.Dispatcher.InvokeAsync(() => Html(rootA.ToHtmlString())) == "<p>letters:1; maps:0</p>", "letters notification rendered");
await rendererA.Dispatcher.InvokeAsync(a.Maps.AddAsync);
Check(await rendererA.Dispatcher.InvokeAsync(() => Html(rootA.ToHtmlString())) == "<p>letters:1; maps:1</p>", "maps notification rendered independently");
Check(await rendererB.Dispatcher.InvokeAsync(() => Html(rootB.ToHtmlString())) == "<p>letters:0; maps:0</p>", "other scope unchanged");
await hostA!.HideAsync();
Check(a.Letters.SubscriptionsCreated == 1 && a.Maps.SubscriptionsCreated == 1, "one subscription per property");
Check(a.Letters.DisposeCalls == 1 && a.Maps.DisposeCalls == 1 && a.Letters.ActiveSubscribers == 0 && a.Maps.ActiveSubscribers == 0, "exactly-once unsubscribe on removal");
await rendererA.Dispatcher.InvokeAsync(a.Letters.AddAsync);
Check((await rendererA.Dispatcher.InvokeAsync(() => Html(rootA.ToHtmlString()))).Length == 0, "removed subscriber stays absent");
await rendererA.DisposeAsync();
Check(a.Letters.DisposeCalls == 1 && a.Maps.DisposeCalls == 1, "renderer teardown does not dispose removed subscriptions twice");
await rendererB.DisposeAsync();
// The pinned renderer teardown does not call these subscription disposals.
// Dispose the owning DI scope after its renderer, so no callback retains a dead component.
second.Dispose();
first.Dispose();
Check(b.Letters.DisposeCalls == 1 && b.Maps.DisposeCalls == 1 && b.Letters.ActiveSubscribers == 0 && b.Maps.ActiveSubscribers == 0, "scope fallback cleans remaining subscriptions");
Check(a.Letters.DisposeCalls == 1 && a.Maps.DisposeCalls == 1, "scope fallback does not repeat removed subscription disposal");
Console.WriteLine($"PASS ReedArchive: real HtmlRenderer selection, updates, scope isolation and disposal; runtime {Environment.Version}.");
static void Check(bool condition, string claim) { if (!condition) throw new InvalidOperationException(claim); }
static string Html(string value) => System.Text.RegularExpressions.Regex.Replace(value, @"<!-- (?:end)?snippet:complete -->\s*", "").Trim();
// endsnippet:complete
