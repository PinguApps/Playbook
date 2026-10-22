using System.Globalization;
using System.Resources;

namespace Playbook.Samples;

// snippet:resource
public static class Captions
{
    private static readonly ResourceManager Resources = new("Playbook.Samples.Captions", typeof(Captions).Assembly);
    public static string Contact => Resources.GetString(nameof(Contact), CultureInfo.CurrentUICulture)!;
}
// endsnippet:resource
