// snippet:complete
using AsterObservatory;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
await renderer.Dispatcher.InvokeAsync(async () =>
{
    var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
    {
        ["Item"] = new SpecialObservation("Comet & dust", 30)
    });
    var root = await renderer.RenderComponentAsync<ObservationView>(parameters);
    if (root.ToHtmlString() != "<section>Aster: Comet &amp; dust</section>")
        throw new InvalidOperationException("Derived item must render through the base template with HTML encoding.");
    root = await renderer.RenderComponentAsync<ObservationView>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Item"] = null }));
    if (root.ToHtmlString() != "<section>No observation selected</section>")
        throw new InvalidOperationException("Null policy must render.");
});
Console.WriteLine($"PASS AsterObservatory: real HtmlRenderer derived content, encoding and null policy; runtime {Environment.Version}.");
// endsnippet:complete
