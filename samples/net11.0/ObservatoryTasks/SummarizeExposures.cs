using Microsoft.Build.Framework;

namespace Observatory;

// snippet:typed
[MSBuildMultiThreadableTask]
public class SummarizeExposures : Microsoft.Build.Utilities.Task, IMultiThreadableTask
{
    public TaskEnvironment TaskEnvironment { get; set; }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819", Justification = "MSBuild binds item-list parameters as arrays.")]
    [Required] public ITaskItem<FileInfo>[] Sources { get; set; } = [];
    public AbsolutePath ReportDirectory { get; set; }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819", Justification = "MSBuild binds item-list outputs as arrays.")]
    [Output] public ITaskItem<AbsolutePath>[] Reports { get; set; } = [];
    public string ConstructorUsed { get; }

    public SummarizeExposures() : this(TaskEnvironment.Fallback, "parameterless") { }
    public SummarizeExposures(TaskEnvironment environment) : this(environment, "injected") { }

    private SummarizeExposures(TaskEnvironment environment, string construction)
    {
        TaskEnvironment = environment;
        ConstructorUsed = construction;
        ReportDirectory = environment.GetAbsolutePath(
            environment.GetEnvironmentVariable("OBSERVATORY_REPORT_DIR") ?? "reports");
    }

    public override bool Execute()
    {
        var reports = new List<ITaskItem<AbsolutePath>>();
        foreach (var source in Sources)
        {
            // Binding resolves relative item identities against this invocation's project.
            FileInfo input = source.Value;
            if (!input.Exists)
            {
                Log.LogError($"Missing exposure: {input.FullName}");
                return false;
            }
            AbsolutePath output = TaskEnvironment.GetAbsolutePath(Path.Combine(
                ReportDirectory.Value, Path.GetFileNameWithoutExtension(input.Name) + ".report"));
            Directory.CreateDirectory(Path.GetDirectoryName(output.Value)!);
            File.WriteAllText(output.Value,
                $"{source.GetMetadata("Filter")}|{File.ReadAllText(input.FullName).Trim()}");
            var report = new TaskItem<AbsolutePath>(output);
            report.SetMetadata("Filter", source.GetMetadata("Filter") ?? "");
            reports.Add(report);
        }
        Reports = reports.ToArray();
        return true;
    }
}
// endsnippet:typed
