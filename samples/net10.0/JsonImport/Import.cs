using System.Text.Json;

namespace Playbook.Samples;

// snippet:import
public sealed record BerthImport(string Vessel, int Crates);
public static class BerthReader
{
    // Strict combines required constructor parameters, unmapped/duplicate rejection,
    // nullable annotations and the strict number-handling contract.
    public static BerthImport Read(string json) =>
        JsonSerializer.Deserialize<BerthImport>(json, JsonSerializerOptions.Strict)
        ?? throw new JsonException("Null import.");
}
// endsnippet:import
