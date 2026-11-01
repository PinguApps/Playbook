namespace Playbook.Samples;

internal static class FlagsChecks
{
    public static void Run()
    {
        const StationState both = StationState.Camera | StationState.Radio;
        var store = new StationFlagStore(StationState.Powered);
        Check(store.Set(StationState.Camera) == StationState.Powered, "Set prior");
        Check(store.Set(StationState.Camera) == (StationState.Powered | StationState.Camera), "idempotent prior");
        Check(store.Clear(StationState.Camera) == (StationState.Powered | StationState.Camera), "Clear prior");
        Check(store.Clear(StationState.Camera) == StationState.Powered, "absent prior");

        // Two dedicated OS threads meet the coordinator at each operation boundary.
        // No assertions may throw inside a worker and strand the barrier.
        using var barrier = new Barrier(3);
        StationState[] setPriors = new StationState[2], clearPriors = new StationState[2];
        Task[] workers = Enumerable.Range(0, 2).Select(worker => Task.Factory.StartNew(() =>
        {
            StationState own = worker == 0 ? StationState.Camera : StationState.Radio;
            for (int round = 0; round < 128; round++)
            {
                barrier.SignalAndWait();
                setPriors[worker] = store.Set(own);
                barrier.SignalAndWait();
                barrier.SignalAndWait();
                clearPriors[worker] = store.Clear(own);
                barrier.SignalAndWait();
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray();
        bool passed = true;
        for (int round = 0; round < 128; round++)
        {
            barrier.SignalAndWait(); barrier.SignalAndWait();
            passed &= store.Read() == (StationState.Powered | both);
            passed &= (setPriors[0] == StationState.Powered && setPriors[1] == (StationState.Powered | StationState.Camera))
                || (setPriors[1] == StationState.Powered && setPriors[0] == (StationState.Powered | StationState.Radio));
            barrier.SignalAndWait(); barrier.SignalAndWait();
            passed &= store.Read() == StationState.Powered;
            passed &= (clearPriors[0] == (StationState.Powered | both) && clearPriors[1] == (StationState.Powered | StationState.Radio))
                || (clearPriors[1] == (StationState.Powered | both) && clearPriors[0] == (StationState.Powered | StationState.Camera));
        }
        Task.WaitAll(workers);
        Check(passed, "contended prior-value chain/unrelated power bit");
        int errors = 0;
        Parallel.For(0, 2, worker =>
        {
            StationState own = worker == 0 ? StationState.Camera : StationState.Radio;
            for (int i = 0; i < 10_000; i++)
            {
                StationState beforeSet = store.Set(own), beforeClear = store.Clear(own);
                if ((beforeSet & (StationState.Powered | own)) != StationState.Powered
                    || (beforeClear & (StationState.Powered | own)) != (StationState.Powered | own))
                    Interlocked.Increment(ref errors);
            }
        });
        Check(errors == 0 && store.Read() == StationState.Powered, "mixed And/Or contention");
        Console.WriteLine("CHECK flags: 128 synchronized set/clear rounds, prior-value chains; 40000 mixed updates preserve unrelated bit");
    }

    internal static void Check(bool condition, string scope)
    {
        if (!condition) throw new InvalidOperationException(scope);
    }
}
