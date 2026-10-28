// snippet:complete
namespace AsterObservatory;
public record Observation(string Name);
public sealed record SpecialObservation(string Name, int ExposureSeconds) : Observation(Name);
// endsnippet:complete
