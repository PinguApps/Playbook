// snippet:all
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace FossilWorker;

[SupportedOSPlatform("browser")]
public static partial class WorkerMethods
{
    private static string _shelf = "unlabelled";
    private static int _completed;

    [JSExport]
    public static void Label(string shelf) => _shelf = shelf;

    [JSExport]
    public static string Report() => $"{_shelf}:{_completed}";

    [JSExport]
    public static int Score(int milliseconds)
    {
        if (milliseconds is < 1 or > 5000) throw new ArgumentOutOfRangeException(nameof(milliseconds));
        var clock = Stopwatch.StartNew();
        uint score = 17;
        do
        {
            for (var specimen = 0; specimen < 10000; specimen++)
                score = unchecked(score * 33 + (uint)specimen) % 1000003;
        } while (clock.ElapsedMilliseconds < milliseconds);
        _completed++;
        return (int)score;
    }
}
// endsnippet:all
