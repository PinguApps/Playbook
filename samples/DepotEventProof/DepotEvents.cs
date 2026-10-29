using System.Text.Json;
using Microsoft.Build.Framework;

namespace Depot.Events;

// snippet:reader
public sealed class DepotEvents : ILogger
{
    public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Diagnostic;
    public string? Parameters { get; set; }
    public bool LegacyConflict { get; private set; }
    public bool ErrorConflict { get; private set; }
    public int ConflictCount { get; private set; }
    public int DetailsCount { get; private set; }
    public int SearchCount { get; private set; }
#if NET11_0
    public MSBuildServerLifecycleEventArgs? Server { get; private set; }
    public bool ExpectedConflict { get; private set; }
    public bool ExpectedDetails { get; private set; }
    public bool ExpectedSearch { get; private set; }
#endif
    public string Summary => $"conflicts={ConflictCount}, dependency-details={DetailsCount}, searches={SearchCount}";
    private IEventSource? source;

    public void Initialize(IEventSource eventSource)
    {
        source = eventSource;
        source.AnyEventRaised += Observe;
    }

    private void Observe(object sender, BuildEventArgs e)
    {
        // SDK10's supported fallback retains the diagnostic/binlog and stable code.
        // Do not reconstruct structured relationships by parsing localized Message.
        if (e is BuildWarningEventArgs { Code: "MSB3277" }) LegacyConflict = true;
        if (e is BuildErrorEventArgs { Code: "MSB3277" }) ErrorConflict = true;
#if NET11_0
        switch (e)
        {
            case AssemblyConflictWarningEventArgs conflict:
                ConflictCount++;
                if (conflict.Victor.FusionName == conflict.Victim.FusionName)
                    throw new InvalidOperationException("Conflict lost assembly identities");
                ExpectedConflict |= conflict.SimpleAssemblyName == "DepotSignal"
                    && conflict.LossReason == AssemblyConflictLossReason.WasNotPrimary
                    && conflict.Victor.IsPrimary && conflict.Victor.IsResolved && conflict.Victim.IsResolved
                    && conflict.Victor.FusionName.Contains("Version=1.0.0.0", StringComparison.Ordinal)
                    && conflict.Victim.FusionName.Contains("Version=2.0.0.0", StringComparison.Ordinal)
                    && conflict.Victor.PrimarySourceItemSpecs.Count > 0
                    && conflict.Victim.Dependees.Any(d => d.DependeeFullPath.EndsWith("DepotInventory.dll", StringComparison.Ordinal) && d.SourceItemSpecs.Count > 0);
                Console.WriteLine(JsonSerializer.Serialize(new
                {
                    conflict.SimpleAssemblyName, loss = conflict.LossReason.ToString(),
                    victor = Reference(conflict.Victor), victim = Reference(conflict.Victim)
                }));
                break;
            case AssemblyConflictDependencyDetailsMessageEventArgs details:
                DetailsCount++;
                ExpectedDetails |= details.Victor.FusionName.StartsWith("DepotGauge, Version=2.0.0.0", StringComparison.Ordinal)
                    && details.Victim.FusionName.StartsWith("DepotGauge, Version=1.0.0.0", StringComparison.Ordinal)
                    && details.Victor.Dependees.Count > 0 && details.Victim.Dependees.Count > 0;
                Console.WriteLine(JsonSerializer.Serialize(new { details = true, victor = Reference(details.Victor), victim = Reference(details.Victim) }));
                break;
            case AssemblyResolutionSearchTraceEventArgs search:
                SearchCount++;
                ExpectedSearch |= search.RequestedAssemblyName == "DepotMissing"
                    && search.SearchAttempts.Any(a => a.Result == AssemblyResolutionSearchResult.FileNotFound);
                Console.WriteLine(JsonSerializer.Serialize(new
                {
                    search.RequestedAssemblyName, search.TargetProcessorArchitecture,
                    attempts = search.SearchAttempts.Select(a => new
                    {
                        a.AssemblyName, file = SafePath(a.FileNameAttempted), parent = SafePath(a.ParentAssembly),
                        a.ProcessorArchitecture, a.IsAssemblyFoldersExSearch, result = a.Result.ToString()
                    })
                }));
                break;
            case MSBuildServerLifecycleEventArgs server:
                Server = server;
                Console.WriteLine(JsonSerializer.Serialize(new { server = server.Kind.ToString(), server.ProcessId, server.ReasonCode, server.ShortLived }));
                break;
        }
#endif
    }

#if NET11_0
    private static object Reference(AssemblyConflictReferenceDetails r) => new
    {
        r.FusionName, path = SafePath(r.FullPath), r.IsPrimary, r.IsResolved,
        unresolved = SafePath(r.UnresolvedPrimaryItemSpec),
        sources = r.PrimarySourceItemSpecs.Select(SafePath),
        dependees = r.Dependees.Select(d => new { path = SafePath(d.DependeeFullPath), sources = d.SourceItemSpecs.Select(SafePath) })
    };

    // Raw binlogs remain local. Preserve graph-relative v1/v2 identity in compact evidence.
    private static string? SafePath(string? path)
    {
        if (path is null) return null;
        string normalized = path.Replace('\\', '/');
        foreach (string segment in new[] { "/v1/", "/v2/" })
        {
            int index = normalized.IndexOf(segment, StringComparison.Ordinal);
            if (index >= 0) return "<graph>" + normalized[index..];
        }
        return Path.IsPathRooted(path) ? "<external>/" + Path.GetFileName(path) : normalized;
    }
#endif

    public void Shutdown()
    {
        if (source != null) source.AnyEventRaised -= Observe;
        source = null;
    }
}
// endsnippet:reader
