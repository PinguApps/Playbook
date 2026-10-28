// snippet:complete
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

namespace BeaconLabels;

public sealed class ExhibitCard
{
    public string Caption { get; set; } = "";
}

// This bounded provider customizes one known model; other types use MVC's default path.
public sealed class ExhibitMetadataProvider(
    ICompositeMetadataDetailsProvider details, IOptions<MvcOptions> options)
    : DefaultModelMetadataProvider(details, options)
{
    public ModelMetadataIdentity CaptionIdentity { get; private set; }

    protected override DefaultMetadataDetails[] CreatePropertyDetails(ModelMetadataIdentity key)
    {
        if (key.ModelType != typeof(ExhibitCard)) return base.CreatePropertyDetails(key);
        var property = typeof(ExhibitCard).GetProperty(nameof(ExhibitCard.Caption))
            ?? throw new InvalidOperationException("The caption property is required.");
        var identity = ModelMetadataIdentity.ForProperty(property, property.PropertyType, key.ModelType);
        CaptionIdentity = identity;
        return
        [
            new DefaultMetadataDetails(identity,
                ModelAttributes.GetAttributesForProperty(key.ModelType, property, property.PropertyType))
            {
                PropertyGetter = instance => ((ExhibitCard)instance).Caption,
                PropertySetter = (instance, value) => ((ExhibitCard)instance).Caption = (string)value!
            }
        ];
    }
}

public sealed class CaptionDetails : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        if (context.Key.ContainerType == typeof(ExhibitCard) &&
            context.Key.Name == nameof(ExhibitCard.Caption))
            context.DisplayMetadata.DisplayName = () => "Public exhibit caption";
    }
}
// endsnippet:complete
