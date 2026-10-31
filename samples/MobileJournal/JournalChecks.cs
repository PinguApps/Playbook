using System.Runtime.InteropServices;
using MobileJournal;

Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}");
string path = Path.Combine(Path.GetTempPath(), "playbook-journal-" + Guid.NewGuid().ToString("N"));
try
{
#if NET11_0_OR_GREATER
    using Journal journal = new(path);
    await Task.WhenAll(Enumerable.Range(0, 30).Select(i => journal.AppendAsync(System.Text.Encoding.UTF8.GetBytes($"shorebird-{i:D2}\n"), CancellationToken.None)));
#else
    Journal journal = new(path);
    for (int i = 0; i < 30; i++) { await journal.AppendAsync(System.Text.Encoding.UTF8.GetBytes($"shorebird-{i:D2}\n"), CancellationToken.None); }
#endif
    string[] lines = await File.ReadAllLinesAsync(path);
    if (lines.Length != 30 || lines.Distinct(StringComparer.Ordinal).Count() != 30) { throw new Exception("Lost or interleaved records"); }
    byte[] before = await File.ReadAllBytesAsync(path);
    using CancellationTokenSource cancelled = new();
    await cancelled.CancelAsync();
    try { await journal.AppendAsync("discarded\n"u8.ToArray(), cancelled.Token); throw new Exception("Cancellation ignored"); }
    catch (OperationCanceledException) { }
    if (!(await File.ReadAllBytesAsync(path)).SequenceEqual(before)) { throw new Exception("Cancelled write changed journal"); }
    Console.WriteLine("PASS portable journal data/cancellation; current single-instance concurrent callers serialized; no Apple locking, suspension or cross-process execution claim");
}
finally { File.Delete(path); }
