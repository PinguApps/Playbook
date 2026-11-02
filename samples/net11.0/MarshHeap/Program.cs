using System.Diagnostics;
using System.Runtime.InteropServices;

if (IntPtr.Size != 4) throw new InvalidOperationException("Run this worker with the x86 runtime.");
Console.WriteLine($"RUNTIME={RuntimeInformation.FrameworkDescription};ARCH={RuntimeInformation.ProcessArchitecture}");
// snippet:budget
// Marsh Observatory retains at most 16 one-MiB image tiles on older runtimes.
const int tileBytes = 1024 * 1024;
const int tileBudget = 16;
var tiles = new List<byte[]>();
bool applicationBudget = args.Contains("application");
bool exhausted = false;
try
{
    for (int i = 0; i < 128; i++)
    {
        if (applicationBudget && tiles.Count == tileBudget) break;
        var tile = new byte[tileBytes];
        tile.AsSpan().Fill((byte)i);
        tiles.Add(tile);
    }
}
catch (OutOfMemoryException)
{
    // Stop accepting tiles; do not retry the same allocation indefinitely.
    exhausted = true;
}
// endsnippet:budget
long configured = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
long retained = (long)tiles.Count * tileBytes;
long rss = Process.GetCurrentProcess().WorkingSet64;
Console.WriteLine($"LIMIT={configured};RETAINED={retained};RSS={rss};OOM={exhausted}");
if (applicationBudget)
{
    if (tiles.Count != tileBudget || exhausted) throw new InvalidOperationException("Application budget failed.");
}
else if (!exhausted || configured != 32L * tileBytes || retained >= configured)
{
    throw new InvalidOperationException("The actual x86 runtime did not enforce the configured heap limit.");
}
tiles.Clear();
GC.Collect();
Console.WriteLine("PASS marsh heap worker");
