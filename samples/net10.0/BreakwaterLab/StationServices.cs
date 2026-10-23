// snippet:complete
namespace BreakwaterLab;

public sealed record StationName(string Value);
public sealed record Scale(int Multiplier);
public sealed class SessionStamp
{
    public Guid Value { get; } = Guid.NewGuid();
}

public sealed class StationServices(
    StationName station,
    [Microsoft.Extensions.DependencyInjection.FromKeyedServices("depth")] Scale scale,
    SessionStamp session)
{
    public StationName Station { get; } = station;
    public Scale Scale { get; } = scale;
    public SessionStamp Session { get; } = session;
}
// endsnippet:complete
