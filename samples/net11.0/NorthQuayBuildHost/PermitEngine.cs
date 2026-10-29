// snippet:engine
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace NorthQuay.Building;

internal static class PermitEngine
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static int Run(string[] arguments)
    {
        bool missingTask = arguments.Contains("--missing-task");
#if !NET11_0
        if (typeof(SdkResolver).GetMethods().Any(method => method.Name == "Register") ||
            typeof(Microsoft.Build.Utilities.Task).GetMethods().Any(method => method.Name == "RegisterTask"))
            throw new InvalidOperationException("Older registration availability changed; reconcile the comparison");
        Console.WriteLine("SDK10 has no static resolver/task registration API; using managed discovery");
#endif
#if NET11_0
        if (!arguments.Contains("--missing-resolver")) SdkResolver.Register(new PermitResolver());
        if (!missingTask) Microsoft.Build.Utilities.Task.RegisterTask<IssueQuayPermit>();
#endif
        var evaluationLog = new PermitLog();
        using var projects = new ProjectCollection();
        projects.RegisterLogger(evaluationLog);
#if NET11_0
        // Never loaded: registration wins; missing registration must fail before reflection.
        string assembly = Environment.ProcessPath!;
#else
        string assembly = typeof(PermitEngine).Assembly.Location;
#endif
        var properties = new Dictionary<string, string> { ["PermitTaskAssembly"] = assembly };
#if !NET11_0
        if (missingTask) properties["PermitTaskAssembly"] = Path.Combine(AppContext.BaseDirectory, "absent-task.dll");
#endif
        try
        {
            var project = ProjectInstance.FromFile(Path.Combine(AppContext.BaseDirectory, "Quay.proj"),
                new ProjectOptions { ProjectCollection = projects, GlobalProperties = properties });
            if (arguments.Contains("--missing-resolver"))
                throw new InvalidOperationException("Missing resolver unexpectedly evaluated the project");
            if (project.GetPropertyValue("PermitOffice") != "North Quay" || project.GetPropertyValue("PermitCode") != "NQ-017")
                throw new InvalidOperationException("Imported permit properties differ");
            Console.WriteLine("EVALUATED North Quay NQ-017; no task has run");
            if (arguments.Contains("--evaluate-only")) return 0;

            var log = new PermitLog();
            using var manager = new BuildManager();
            // This public entry point retains RequiresUnreferencedCode / IL2026.
            // Registration does not make arbitrary build execution trim-safe.
            var result = manager.Build(new BuildParameters(projects)
            {
                MaxNodeCount = 1, EnableNodeReuse = false, Loggers = [log]
            }, new BuildRequestData(project, ["IssuePermit"]));
            if (missingTask)
            {
                if (result.OverallResult != BuildResultCode.Failure || !log.Errors.Any())
                    throw new InvalidOperationException("Missing task did not fail");
#if NET11_0
                if (!log.Errors.Contains("MSB4283")) throw new InvalidOperationException("Expected reflective-task rejection MSB4283");
#endif
                Console.WriteLine("EXPECTED missing-task failure: " + string.Join(',', log.Errors));
                return 0;
            }
            if (result.OverallResult != BuildResultCode.Success || !log.SawReceipt ||
                result.ResultsByTarget["IssuePermit"].Items.Single().ItemSpec != "NQ-017:approved")
                throw new InvalidOperationException("Task input/output or logger receipt differs");
            Console.WriteLine("PASS permit input/output and logger receipt");
            return 0;
        }
        catch (Microsoft.Build.Exceptions.InvalidProjectFileException error) when (arguments.Contains("--missing-resolver"))
        {
            if (error.ErrorCode != "MSB4282") throw;
            Console.WriteLine("EXPECTED missing-resolver failure MSB4282: " + error.BaseMessage);
            return 0;
        }
    }
}
// endsnippet:engine

// snippet:task
public sealed class IssueQuayPermit : Microsoft.Build.Utilities.Task
{
    [Required] public string Code { get; set; } = "";
    [Output] public string Receipt { get; set; } = "";

    public override bool Execute()
    {
        if (Code != "NQ-017") { Log.LogError("Unexpected quay permit code"); return false; }
        Receipt = Code + ":approved";
        Log.LogMessage(MessageImportance.High, "RECEIPT {0}", Receipt);
        return true;
    }
}
// endsnippet:task

// snippet:logger
internal sealed class PermitLog : ILogger
{
    private readonly System.Collections.Concurrent.ConcurrentQueue<string> errors = new();
    public IEnumerable<string> Errors => errors;
    public bool SawReceipt { get; private set; }
    public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Diagnostic;
    public string? Parameters { get; set; }
    public void Initialize(IEventSource eventSource)
    {
        eventSource.ErrorRaised += (_, error) => { errors.Enqueue(error.Code ?? ""); Console.WriteLine($"ERROR {error.Code}: {error.Message}"); };
        eventSource.MessageRaised += (_, message) =>
        {
            if (message.Message == "RECEIPT NQ-017:approved") { SawReceipt = true; Console.WriteLine(message.Message); }
        };
    }
    public void Shutdown() { }
}
// endsnippet:logger

#if NET11_0
// snippet:resolver
internal sealed class PermitResolver : SdkResolver
{
    public override string Name => "North Quay permit SDK";
    public override int Priority => 0;
    public override SdkResult Resolve(SdkReference sdkReference, SdkResolverContext resolverContext, SdkResultFactory factory) =>
        sdkReference.Name == "Quay.Permits"
            ? factory.IndicateSuccess(Path.Combine(AppContext.BaseDirectory, "Sdks/Quay.Permits/Sdk"), "1.0")
            : factory.IndicateFailure(["This resolver only provides Quay.Permits"]);
}
// endsnippet:resolver
#endif
