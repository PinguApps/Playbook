using System.Runtime.InteropServices;

namespace ShoreCalibration;

// snippet:calibration
public static class Calibration
{
    public static int ConvertReading(int raw)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(raw);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(raw, 10000);
        return ConvertNative(raw);
    }

    [DllImport("shore_calibration", EntryPoint = "shore_convert")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    private static extern int ConvertNative(int raw);
}
// endsnippet:calibration
