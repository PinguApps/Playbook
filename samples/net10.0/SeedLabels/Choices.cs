namespace Playbook.Samples;

// snippet:choices
public readonly record struct SeedChoice(string? Text, SeedBatch? Batch);
public sealed record SeedBatch(string Variety, int Packets);
// endsnippet:choices
