// snippet:complete
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
await using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
var html = await renderer.Dispatcher.InvokeAsync(async () =>
    (await renderer.RenderComponentAsync<ExhibitCompiler.Exhibit>()).ToHtmlString());
const string expected = "<p>Lantern collection: 42</p>\n<strong>Lantern &lt;Archive&gt;</strong>";
if (html.Replace("\r\n", "\n", StringComparison.Ordinal) != expected) throw new InvalidOperationException($"Unexpected generated output: {html}");
Console.WriteLine($"PASS generated exhibit: {html}; runtime {RuntimeInformation.FrameworkDescription}");
// endsnippet:complete
