using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Playbook.Samples;

using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
int produced = 0;
int lookups = 0;
bool disposed = false;
var output = Readings.Transform(Source(), Lookup);
if (produced != 0 || lookups != 0) { throw new InvalidOperationException("Pipeline ran eagerly."); }
await using (var iterator = output.GetAsyncEnumerator(timeout.Token))
{
    if (!await iterator.MoveNextAsync().AsTask().WaitAsync(timeout.Token) || iterator.Current != new Alert("north", 80, "North embankment") ||
        Volatile.Read(ref produced) != 2 || Volatile.Read(ref lookups) != 1)
    {
        throw new InvalidOperationException("First enriched result must precede producer completion.");
    }
    Task<bool> pending = iterator.MoveNextAsync().AsTask();
    if (pending.IsCompleted) { throw new InvalidOperationException("Producer gate did not block the next result."); }
    gate.SetResult();
    if (!await pending.WaitAsync(timeout.Token) || iterator.Current.Station != "south" || await iterator.MoveNextAsync().AsTask().WaitAsync(timeout.Token) || Volatile.Read(ref lookups) != 2)
    {
        throw new InvalidOperationException("Filtered/enriched result sequence changed.");
    }
}
if (!disposed) { throw new InvalidOperationException("Source was not disposed."); }

// Early disposal must unwind a suspended producer without completing the stream.
disposed = false;
await using (var iterator = Readings.Transform(Source(), Lookup).GetAsyncEnumerator(timeout.Token))
{
    if (!await iterator.MoveNextAsync().AsTask().WaitAsync(timeout.Token)) { throw new InvalidOperationException("Missing early result."); }
}
if (!disposed) { throw new InvalidOperationException("Early disposal did not reach producer finally."); }

// Cancellation while enrichment is genuinely pending reaches its operation and producer cleanup.
using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
disposed = false;
await using (var iterator = Readings.Transform(Source(), async (_, token) =>
{
    started.SetResult();
    await Task.Delay(Timeout.Infinite, token);
    return "unreachable";
}).GetAsyncEnumerator(cancellation.Token))
{
    Task<bool> pending = iterator.MoveNextAsync().AsTask();
    await started.Task.WaitAsync(timeout.Token);
    await cancellation.CancelAsync();
    try { await pending.WaitAsync(timeout.Token); throw new InvalidOperationException("Enrichment ignored cancellation."); }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
}
if (!disposed) { throw new InvalidOperationException("Cancellation leaked producer."); }

// Cancellation while MoveNextAsync is pending in the producer also propagates.
using var sourceCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
disposed = false;
await using (var iterator = Readings.Transform(Source(), Lookup).GetAsyncEnumerator(sourceCancellation.Token))
{
    if (!await iterator.MoveNextAsync().AsTask().WaitAsync(timeout.Token)) { throw new InvalidOperationException("Missing result before cancellation."); }
    Task<bool> pending = iterator.MoveNextAsync().AsTask();
    await sourceCancellation.CancelAsync();
    try { await pending.WaitAsync(timeout.Token); throw new InvalidOperationException("Producer ignored cancellation."); }
    catch (OperationCanceledException) when (sourceCancellation.IsCancellationRequested) { }
}
if (!disposed) { throw new InvalidOperationException("Source cancellation leaked producer."); }
Console.WriteLine($"PASS streaming: deferred, incremental, filtered/enriched, early disposal and pending producer/selector cancellation; {RuntimeInformation.FrameworkDescription}");

async IAsyncEnumerable<SensorReading> Source([EnumeratorCancellation] CancellationToken token = default)
{
    try
    {
        produced++;
        yield return new SensorReading("invalid", 99, false);
        produced++;
        yield return new SensorReading("north", 80, true);
        await gate.Task.WaitAsync(token);
        produced++;
        yield return new SensorReading("south", 75, true);
        produced++;
        yield return new SensorReading("low", 20, true);
    }
    finally { disposed = true; }
}

ValueTask<string> Lookup(string station, CancellationToken token)
{
    token.ThrowIfCancellationRequested();
    lookups++;
    return ValueTask.FromResult(station == "north" ? "North embankment" : "South embankment");
}
