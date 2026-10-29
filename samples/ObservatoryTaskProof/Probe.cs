using System.Collections;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Build.Framework;

namespace Observatory;

// Test instrumentation only. The canonical task has no rendezvous or process-state reads.
#if NET11_0
[MSBuildMultiThreadableTask]
#endif
public sealed class CheckedExposures : SummarizeExposures
{
    public string ProofFile { get; set; } = "";
    public bool Concurrent { get; set; }
    private static readonly Barrier Gate = new(2);

    public CheckedExposures() { }
#if NET11_0
    public CheckedExposures(TaskEnvironment environment) : base(environment) { }
#endif

    public override bool Execute()
    {
        var cwd = Environment.CurrentDirectory;
        string before = Snapshot();
        long start = DateTime.UtcNow.Ticks;
        if (Concurrent && !Gate.SignalAndWait(TimeSpan.FromSeconds(25)))
            throw new InvalidOperationException("Two tasks did not rendezvous in the same process.");
        bool result = base.Execute();
        if (cwd != Environment.CurrentDirectory || before != Snapshot())
            throw new InvalidOperationException("Task changed process directory or environment.");
        File.WriteAllText(ProofFile, JsonSerializer.Serialize(new
        {
            pid = Environment.ProcessId, thread = Environment.CurrentManagedThreadId,
            start, end = DateTime.UtcNow.Ticks, cwd, unchanged = true,
            runtimeDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
            frameworkPath = typeof(ITask).Assembly.Location,
            frameworkSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(ITask).Assembly.Location))).ToLowerInvariant(),
            utilitiesPath = typeof(Microsoft.Build.Utilities.Task).Assembly.Location,
#if NET11_0
            constructor = ConstructorUsed, project = TaskEnvironment.ProjectDirectory.Value,
            reportDirectory = ReportDirectory.Value,
            inputs = Sources.Select(i => i.Value.FullName).ToArray(),
#else
            constructor = "parameterless", project = ProjectDirectory,
            reportDirectory = Path.GetFullPath(ReportDirectory, ProjectDirectory),
            inputs = Sources.Select(i => Path.GetFullPath(i.ItemSpec, ProjectDirectory)).ToArray(),
#endif
            outputs = Reports.Select(i => new { path = i.ItemSpec, filter = i.GetMetadata("Filter") }).ToArray()
        }));
        return result;
    }

    private static string Snapshot() => string.Join("\n", Environment.GetEnvironmentVariables()
        .Cast<DictionaryEntry>().OrderBy(e => (string)e.Key, StringComparer.Ordinal)
        .Select(e => $"{e.Key}={e.Value}"));
}
