// snippet:complete
namespace BreakwaterLab;

public sealed record StationName(string Value);
public sealed record Scale(int Multiplier);
public sealed class SessionStamp
{
    public Guid Value { get; } = Guid.NewGuid();
}

// endsnippet:complete
