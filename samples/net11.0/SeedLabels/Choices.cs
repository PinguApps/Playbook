using Microsoft.AspNetCore.Components;

namespace Playbook.Samples;

// snippet:choices
public union BadgeLabel(string, RenderFragment);
public union SeedChoice(string, SeedBatch);
public sealed record SeedBatch(string Variety, int Packets);
// endsnippet:choices
