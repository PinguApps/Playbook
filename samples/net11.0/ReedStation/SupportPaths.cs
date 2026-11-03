namespace Playbook.Samples;
public static class SupportPaths
{
    public static string Check()
    {
        if (PlotScorer.ScoreReedPlots([2, 5, 9], CancellationToken.None) != 48) throw new Exception("Score mismatch");
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        try { PlotScorer.ScoreReedPlots([2, 5, 9], canceled.Token); throw new Exception("Cancellation ignored"); }
        catch (OperationCanceledException) { }
        var fallback = 0;
        using (StationNetwork.WatchReedStation(() => { }, () => fallback++)) { }
        if (OperatingSystem.IsBrowser() && fallback != 1) throw new Exception("Browser fallback missing");
        return $"PASS ReedStation: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; threads={PlotScorer.CanUseThreads}; score=48; cancellation=passed; manualRefresh={fallback}";
    }
}
