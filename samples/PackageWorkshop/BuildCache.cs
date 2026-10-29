// snippet:cache
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Evaluation;
#if NET11_0
using NuGet.Common;
#endif

namespace Gannet;

public sealed class HarbourCache : IDisposable
{
    private Lazy<string> value;
    private readonly Func<string> read;
    public int Creations { get; private set; }
    public int Invalidations { get; private set; }
    public HarbourCache(Func<string> read)
    {
        this.read = read;
        value = Fresh();
#if NET11_0
        StaticState.BuildEnded += Invalidate;
#endif
    }
    // The custom host serializes builds and cache access. This is not a concurrent cache.
    public string Value => value.Value;
    private Lazy<string> Fresh() => new(() => { Creations++; return read(); });
    public void Invalidate() { Invalidations++; value = Fresh(); }
    public void Dispose()
    {
#if NET11_0
        StaticState.BuildEnded -= Invalidate;
#endif
    }
}

public sealed class ReadHarbour : Microsoft.Build.Utilities.Task
{
    [Required] public string Receipt { get; set; } = "";
    [Required] public string Expected { get; set; } = "";
    public bool FailBuild { get; set; }
    public override bool Execute()
    {
        // MSBuild may load tasks in another AssemblyLoadContext. Use the public HostObject
        // seam and BCL types rather than a second task-assembly static/cache/event.
        string value = ((Func<string>)((IDictionary<string, object>)HostObject)["Read"])();
        File.WriteAllText(Receipt, $"{Environment.ProcessId}|{value}|{Expected}");
        if (FailBuild) Log.LogError("Gannet deliberate build failure");
        return !FailBuild;
    }
}

public sealed class PackageHost : IDisposable
{
    private sealed class HarbourHostObject : Dictionary<string, object>, ITaskHost
    {
        public HarbourHostObject(HarbourCache cache) => Add("Read", (Func<string>)(() => cache.Value));
    }
    private readonly BuildManager manager = new();
    private readonly HarbourCache cache;
    public PackageHost(HarbourCache cache) => this.cache = cache;
    public BuildResult Build(string project, Dictionary<string, string?> inputs, bool reset = true)
    {
        using var collection = new ProjectCollection(inputs);
        var services = new HostServices();
        services.RegisterHostObject(project, "Build", "ReadHarbour", new HarbourHostObject(cache));
        var parameters = new BuildParameters(collection) { EnableNodeReuse = false, MaxNodeCount = 1, HostServices = services, Loggers = [new Microsoft.Build.Logging.ConsoleLogger()] };
        manager.BeginBuild(parameters);
        try
        {
            return manager.BuildRequest(new BuildRequestData(project, inputs, null, ["Build"], services));
        }
        finally
        {
            // This host owns the real lifecycle seam. Raise only after EndBuild returns,
            // including when the actual task fails. Unmodified third-party hosts are unqualified.
            manager.EndBuild();
            if (reset)
            {
#if NET11_0
                StaticState.RaiseBuildEnded();
#else
                cache.Invalidate(); // Valid older explicit per-build lifetime route.
#endif
            }
        }
    }
    public void Dispose() => manager.Dispose();
}
// endsnippet:cache
