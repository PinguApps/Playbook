// snippet:complete
using Microsoft.AspNetCore.SignalR;

namespace Soundings;

public sealed class SoundingHub(SoundingWork work) : Hub
{
    public Task<string> Calculate(string id, CancellationToken cancellationToken) =>
        work.CalculateAsync(id, cancellationToken);

    public string Echo(string value) => value;
}
// endsnippet:complete
