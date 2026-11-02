// snippet:bridge-checks
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

namespace Playbook.Samples;

public static partial class BridgeChecks
{
    [JSImport("finishSegment", "soundings")]
    private static partial void FinishSegment();

    [JSImport("checkExpiredSpan", "soundings")]
    private static partial bool CheckExpiredSpan();

    [JSExport]
    public static string Run()
    {
        float[] source = [1.25f, -3.5f, float.NaN, float.PositiveInfinity, float.NegativeInfinity];
        float[] actual = BufferBridge.Echo(source);
        Check(source[0] == 1.25f && actual[0] == 1.75f && actual[1] == -3f, "array copy/round trip");
        Check(float.IsNaN(actual[2]) && float.IsPositiveInfinity(actual[3]) && float.IsNegativeInfinity(actual[4]), "special values");
        Check(BufferBridge.Echo([]).Length == 0, "empty array");
        float[] buffer = [99, 2.5f, -4, 88];
        BufferBridge.EditSpan(buffer.AsSpan(1, 2));
        Check(buffer.SequenceEqual(new float[] { 99, 3.5f, -3, 88 }), "span offsets and boundary sentinels");
        Check(CheckExpiredSpan(), "span must expire after synchronous call");
        BufferBridge.KeepSegment(new ArraySegment<float>(buffer, 1, 2));
        FinishSegment();
#if OLDER_ADAPTER
        BufferBridge.CopyBack();
#endif
        Check(buffer.SequenceEqual(new float[] { 99, 5.5f, -1, 88 }), "retained segment offsets, mutation, disposal");
        return $"PASS FloatSoundings: {RuntimeInformation.FrameworkDescription}; arrays, span, segment, offsets, NaN/infinities, lifetime/disposal, generated imports/exports";
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
// endsnippet:bridge-checks
