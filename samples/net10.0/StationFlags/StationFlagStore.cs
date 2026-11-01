// snippet:station-flags
using System.Threading;

namespace Playbook.Samples;

[Flags]
internal enum StationState : int { None = 0, Camera = 1, Radio = 2, Powered = 64 }

internal sealed class StationFlagStore
{
    private int _bits;

    public StationFlagStore(StationState initial) => _bits = (int)initial;

    public StationState Set(StationState flags)
    {
        int observed = Volatile.Read(ref _bits);
        while (true)
        {
            int prior = Interlocked.CompareExchange(ref _bits, observed | (int)flags, observed);
            if (prior == observed) return (StationState)prior;
            observed = prior;
        }
    }

    public StationState Clear(StationState flags)
    {
        int observed = Volatile.Read(ref _bits);
        while (true)
        {
            int prior = Interlocked.CompareExchange(ref _bits, observed & ~(int)flags, observed);
            if (prior == observed) return (StationState)prior;
            observed = prior;
        }
    }

    public StationState Read() => (StationState)Volatile.Read(ref _bits);
}
// endsnippet:station-flags
