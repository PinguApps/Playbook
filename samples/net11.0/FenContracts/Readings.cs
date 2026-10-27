// snippet:complete
namespace FenContracts;

public sealed class Reading
{
    public string Station { get; init; } = "";
    public double DepthMetres { get; init; }

    [Obsolete("Use DepthMetres; the old gauge scale is retired.")]
    public int GaugeTicks { get; init; }
}

[Obsolete("Use Reading and the /readings endpoint.")]
public sealed record LegacyReading(string Station, int GaugeTicks);

public static class ReadingRoutes
{
    [Obsolete("Use /readings.")]
#pragma warning disable CS0618 // The old response remains callable during migration.
    public static LegacyReading ReadLegacy() => new("Alder-7", 24);
#pragma warning restore CS0618
}
// endsnippet:complete