// snippet:station-flags
using System.Threading;

namespace Playbook.Samples;

[Flags]
internal enum StationState : int { None = 0, Camera = 1, Radio = 2, Powered = 64 }

internal sealed class StationFlagStore
{
    private StationState _flags;

    public StationFlagStore(StationState initial) => _flags = initial;

    // Both operations return the value BEFORE their atomic update.
    public StationState Set(StationState flags) => Interlocked.Or(ref _flags, flags);
    public StationState Clear(StationState flags) => Interlocked.And(ref _flags, ~flags);

    // Atomic snapshot only; it can be out of date as soon as it is returned.
    public StationState Read() => Interlocked.Or(ref _flags, StationState.None);
}
// endsnippet:station-flags
