// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MossGallery;

using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
var logs = services.GetRequiredService<ILoggerFactory>();
// These are deliberate framework integration checks, not ordinary application use.
#pragma warning disable BL0006
await using var renderer = new GalleryRenderer(services, logs);
foreach (var value in new string?[] { "Fern portrait", "Lichen <detail>", null, "Rehung" })
{
    await renderer.ShowAsync(value);
    if (renderer.Caption != value) throw new InvalidOperationException("Wrong custom-renderer caption.");
    await using var html = new HtmlRenderer(services, logs);
    var output = await html.Dispatcher.InvokeAsync(async () =>
        (await html.RenderComponentAsync<GalleryCard>(ParameterView.FromDictionary(
            new Dictionary<string, object?> { [nameof(GalleryCard.Caption)] = value }))).ToHtmlString());
    var expected = value is null ? "<figure>Moss study</figure>" :
        $"<figure data-caption=\"{System.Net.WebUtility.HtmlEncode(value)}\">Moss study</figure>";
    if (output != expected) throw new InvalidOperationException($"Unexpected HTML: {output}");
    Console.WriteLine(output);
}
if (renderer.Batches != 4) throw new InvalidOperationException("Missing real render updates.");
#pragma warning restore BL0006
Console.WriteLine($"PASS: MossGallery real renderer and HTML output on {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}.");
// endsnippet:complete
