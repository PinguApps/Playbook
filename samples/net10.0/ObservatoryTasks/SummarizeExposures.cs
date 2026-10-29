using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Observatory;

// snippet:legacy
// No thread-safety opt-in: ordinary hosts and -mt sidecars retain isolation.
public class SummarizeExposures : Microsoft.Build.Utilities.Task
{
    [Required] public string ProjectDirectory { get; set; } = "";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819", Justification = "MSBuild binds item-list parameters as arrays.")]
    [Required] public ITaskItem[] Sources { get; set; } = [];
    [Required] public string ReportDirectory { get; set; } = "";
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819", Justification = "MSBuild binds item-list outputs as arrays.")]
    [Output] public ITaskItem[] Reports { get; set; } = [];

    public override bool Execute()
    {
        if (!Path.IsPathFullyQualified(ProjectDirectory))
        {
            Log.LogError("Supply an absolute ProjectDirectory from MSBuildProjectDirectory.");
            return false;
        }
        var reports = new List<ITaskItem>();
        foreach (var source in Sources)
        {
            string input = Path.GetFullPath(source.ItemSpec, ProjectDirectory);
            if (!File.Exists(input))
            {
                Log.LogError($"Missing exposure: {input}");
                return false;
            }
            string output = Path.Combine(Path.GetFullPath(ReportDirectory, ProjectDirectory),
                Path.GetFileNameWithoutExtension(input) + ".report");
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            File.WriteAllText(output, $"{source.GetMetadata("Filter")}|{File.ReadAllText(input).Trim()}");
            var report = new TaskItem(output);
            report.SetMetadata("Filter", source.GetMetadata("Filter"));
            reports.Add(report);
        }
        Reports = reports.ToArray();
        return true;
    }
}
// endsnippet:legacy

// snippet:compatibility
// This separate net10.0 task uses only APIs present in the older SDK.
public sealed class ConstructorProbe : Microsoft.Build.Utilities.Task, IMultiThreadableTask
{
    public TaskEnvironment TaskEnvironment { get; set; }
    public string ConstructorUsed { get; }
    [Output] public string DefaultDirectory { get; }
    [Output] public string Construction { get; }

    public ConstructorProbe() : this(TaskEnvironment.Fallback, "parameterless") { }
    public ConstructorProbe(TaskEnvironment environment) : this(environment, "injected") { }

    private ConstructorProbe(TaskEnvironment environment, string construction)
    {
        TaskEnvironment = environment;
        ConstructorUsed = construction;
        DefaultDirectory = environment.GetAbsolutePath(
            environment.GetEnvironmentVariable("OBSERVATORY_REPORT_DIR") ?? "reports").Value;
        Construction = construction;
    }

    public override bool Execute() => true;
}
// endsnippet:compatibility
