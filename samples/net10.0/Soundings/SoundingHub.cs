// snippet:complete
using Microsoft.AspNetCore.SignalR;

namespace Soundings;

public sealed class SoundingHub(SoundingWork work) : Hub
{
    public Task<string> Calculate(string id) =>
        work.CalculateAsync(id, Context.ConnectionAborted);

    public string Echo(string value) => value;
}
// endsnippet:complete
