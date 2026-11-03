// snippet:threading-path
using System.Runtime.CompilerServices;
namespace Playbook.Samples;
public static class PlotScorer
{
    public static bool CanUseThreads =>
        RuntimeFeature.IsMultithreadingSupported;


    public static int ScoreReedPlots(int[] counts, CancellationToken cancellationToken)
    {
        var scores = new int[counts.Length];
        if (CanUseThreads)
        {
            Parallel.For(0, counts.Length, new ParallelOptions { CancellationToken = cancellationToken },
                i => scores[i] = checked(counts[i] * 3));
        }
        else
        {
            for (var i = 0; i < counts.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                scores[i] = checked(counts[i] * 3);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return scores.Sum();
    }
    }
// endsnippet:threading-path
