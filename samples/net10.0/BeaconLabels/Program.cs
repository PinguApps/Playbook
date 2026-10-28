// snippet:complete
using BeaconLabels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var services = new ServiceCollection();
services.AddLogging();
services.AddControllers(options => options.ModelMetadataDetailsProviders.Add(new CaptionDetails()));
using var container = services.BuildServiceProvider();
var provider = new ExhibitMetadataProvider(
    container.GetRequiredService<ICompositeMetadataDetailsProvider>(),
    container.GetRequiredService<IOptions<MvcOptions>>());
var caption = provider.GetMetadataForProperties(typeof(ExhibitCard))
    .Single(item => item.PropertyName == nameof(ExhibitCard.Caption));
if (caption.ModelType != typeof(string) || caption.ContainerType != typeof(ExhibitCard) ||
    caption.GetDisplayName() != "Public exhibit caption")
    throw new InvalidOperationException("Property metadata lost its identity or display policy.");
var card = new ExhibitCard { Caption = "Beacon at dusk" };
if (!Equals(caption.PropertyGetter!(card), "Beacon at dusk"))
    throw new InvalidOperationException("Getter did not address the caption property.");
caption.PropertySetter!(card, "Beacon after rain");
if (card.Caption != "Beacon after rain")
    throw new InvalidOperationException("Setter did not address the caption property.");
Console.WriteLine($"PASS: custom model metadata, display name, getter and setter; {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; ASP.NET Core {typeof(MvcOptions).Assembly.GetName().Version}");
// endsnippet:complete
