// snippet:complete
using System.ComponentModel.DataAnnotations;

namespace MossObservatory;

// Keep the public model and AddValidation call in this assembly so generation discovers it.
public sealed class WatchRequest
{
    [Required, StringLength(24)]
    public string? Station { get; set; }

    [Range(1, 6)]
    public int Observers { get; set; }
}
// endsnippet:complete
