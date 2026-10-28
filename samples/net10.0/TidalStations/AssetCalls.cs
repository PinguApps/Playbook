using Microsoft.AspNetCore.Components;

namespace Playbook.Samples;

// snippet:assets
public static class AssetCalls
{
    // These calls compile unchanged on both actual targets. The second argument is optional.
    public static ResourceAsset Plain() => new("/observatory.css");
    public static ResourceAsset ExplicitNull() => new("/observatory.css", null);
    public static ResourceAsset WithProperties() => new("/observatory.css",
        new[] { new ResourceAssetProperty("label", "station chart") });
}
// endsnippet:assets
