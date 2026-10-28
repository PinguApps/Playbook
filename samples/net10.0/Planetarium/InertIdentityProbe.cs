// snippet:complete
using Microsoft.Extensions.Logging;

namespace Planetarium;

// Historical comparison only. The obsolete init setter was inert in .NET 10.
#pragma warning disable BL0006
public sealed class InertIdentityProbe : PlanetariumRenderer
{
    public InertIdentityProbe(IServiceProvider services, ILoggerFactory logs) : base(services, logs)
    {
#pragma warning disable CS0618 // Deliberate removed-member comparison, confined to this assignment.
        RendererId = 900;
#pragma warning restore CS0618
    }
}
#pragma warning restore BL0006
// endsnippet:complete
