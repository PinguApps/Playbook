// snippet:library
namespace Cwmglas.Station;

public static class ReadingScale
{
    public static decimal Calibrate(int ticks) => ticks / 4m;
    public static string PackageVersion => typeof(ReadingScale).Assembly.GetName().Version!.ToString(3);
}
// endsnippet:library
