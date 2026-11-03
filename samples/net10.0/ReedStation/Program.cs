using Playbook.Samples;
RuntimeIdentity.Assert();
Console.WriteLine(SupportPaths.Check());
if (args is ["watch", var directory])
{
    var events = 0;
    using var subscription = StationNetwork.WatchReedStation(() => Interlocked.Increment(ref events), () => throw new Exception("Network events unavailable"));
    await File.WriteAllTextAsync(Path.Combine(directory, "ready"), "subscribed");
    var deadline = DateTime.UtcNow.AddSeconds(45);
    while (Volatile.Read(ref events) == 0 && DateTime.UtcNow < deadline) await Task.Delay(50);
    if (events == 0) throw new Exception("No real network event observed");
    Console.WriteLine($"EVENT count={events}");
    subscription.Dispose();
    await Task.Delay(500); // Let any already queued delivery finish before the second change.
    var afterDisposal = Volatile.Read(ref events);
    await File.WriteAllTextAsync(Path.Combine(directory, "detached"), "unsubscribed");
    deadline = DateTime.UtcNow.AddSeconds(45);
    while (!File.Exists(Path.Combine(directory, "changed")) && DateTime.UtcNow < deadline) await Task.Delay(50);
    if (!File.Exists(Path.Combine(directory, "changed"))) throw new Exception("Second interface change missing");
    await Task.Delay(1000);
    if (events != afterDisposal) throw new Exception("Callback received for change after disposal");
    Console.WriteLine("PASS real address event and unsubscription");
}