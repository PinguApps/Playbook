using System.Runtime.InteropServices.JavaScript;

namespace Playbook.Samples;

// snippet:float-bridge
public static partial class BufferBridge
{
    [JSImport("echo", "soundings")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    private static partial double[] EchoDoubles([JSMarshalAs<JSType.Array<JSType.Number>>] double[] values);

    // Existing .NET 10 route: widen, cross the real JS boundary, then narrow.
    internal static float[] Echo(float[] values) => Array.ConvertAll(
        EchoDoubles(Array.ConvertAll(values, value => (double)value)), value => (float)value);

    [JSImport("editSpan", "soundings")]
    private static partial void EditDoubleSpan([JSMarshalAs<JSType.MemoryView>] Span<double> values);

    internal static void EditSpan(Span<float> values)
    {
        double[] temporary = Array.ConvertAll(values.ToArray(), value => (double)value);
        EditDoubleSpan(temporary);
        for (int i = 0; i < values.Length; i++) values[i] = (float)temporary[i];
    }

    [JSImport("keepSegment", "soundings")]
    private static partial void KeepDoubleSegment([JSMarshalAs<JSType.MemoryView>] ArraySegment<double> values);
    private static double[]? staged;
    private static ArraySegment<float> destination;

    internal static void KeepSegment(ArraySegment<float> values)
    {
        staged = Array.ConvertAll(values.Array!, value => (double)value);
        destination = values;
        KeepDoubleSegment(new ArraySegment<double>(staged, values.Offset, values.Count));
    }

    internal static void CopyBack()
    {
        for (int i = 0; i < destination.Count; i++) destination[i] = (float)staged![destination.Offset + i];
        staged = null;
        destination = default;
    }

    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static double[] ExportArray([JSMarshalAs<JSType.Array<JSType.Number>>] double[] values) => EchoDoubles(values);

    [JSExport]
    public static double ObserveSpan([JSMarshalAs<JSType.MemoryView>] Span<double> values) => values[0];

    [JSExport]
    public static double ObserveSegment([JSMarshalAs<JSType.MemoryView>] ArraySegment<double> values) => values[0];
}
// endsnippet:float-bridge
