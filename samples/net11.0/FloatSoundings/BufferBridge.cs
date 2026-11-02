using System.Runtime.InteropServices.JavaScript;

namespace Playbook.Samples;

// snippet:float-bridge
public static partial class BufferBridge
{
    [JSImport("echo", "soundings")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    internal static partial float[] Echo([JSMarshalAs<JSType.Array<JSType.Number>>] float[] values);

    [JSImport("editSpan", "soundings")]
    internal static partial void EditSpan([JSMarshalAs<JSType.MemoryView>] Span<float> values);

    [JSImport("keepSegment", "soundings")]
    internal static partial void KeepSegment([JSMarshalAs<JSType.MemoryView>] ArraySegment<float> values);

    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static float[] ExportArray([JSMarshalAs<JSType.Array<JSType.Number>>] float[] values) => Echo(values);

    [JSExport]
    public static double ObserveSpan([JSMarshalAs<JSType.MemoryView>] Span<float> values) => values[0];

    [JSExport]
    public static double ObserveSegment([JSMarshalAs<JSType.MemoryView>] ArraySegment<float> values) => values[0];
}
// endsnippet:float-bridge
