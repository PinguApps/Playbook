// snippet:trace
namespace EstuaryBoard;

public sealed record ProviderTrace(string Name, string? Endpoint, string? Caption);
public sealed record ConfigurationTrace(string InitialEndpoint, IReadOnlyList<ProviderTrace> Providers, int EnvironmentProviders);
// endsnippet:trace
