// snippet:complete
namespace CedarPlanetarium;

public sealed record ShowTicket(string Dome, int Seats);
// Deliberately absent from generated metadata: both resolvers return null.
public sealed record ExtraNote(string Text);
public sealed class VisitState
{
    public string Scope { get; } = Guid.NewGuid().ToString("N");
    public int Runs { get; set; }
}
// endsnippet:complete
