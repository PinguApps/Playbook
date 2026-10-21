// snippet:helper
namespace Depot;

public static class Rack
{
    public static int Free(int capacity, int reserved)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(reserved);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(reserved, capacity);
        return capacity - reserved;
    }
}
// endsnippet:helper
