// snippet:complete
using System.ComponentModel.DataAnnotations;

namespace MossObservatory;

// The .NET 9 endpoint filter validates this public model explicitly.
public sealed class WatchRequest
{
    [Required, StringLength(24)]
    public string? Station { get; set; }

    [Range(1, 6)]
    public int Observers { get; set; }
}
// endsnippet:complete
