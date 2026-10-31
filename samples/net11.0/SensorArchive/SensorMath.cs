// snippet:generic
using System.Numerics;

namespace SensorArchive;

public static class SensorMath
{
    public static T Add<T>(T left, T right) where T : IFloatingPointIeee754<T> => left + right;
}
// endsnippet:generic
