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
    // The caller must serialize ALL builds/cache access in this process, across hosts.
    // BuildEnded is process-global. This is not a concurrent cache.
    public string Value
    {
        get { PackageHost.ThrowIfProcessRetired(); return value.Value; }
    }
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
    private readonly Microsoft.Build.Framework.ILogger logger;
    // All original hosts/caches live in this application's default load context.
    // A failed global reset can strand ANY later subscriber; no in-process reset API.
    private static bool processRetired;
    internal static void ThrowIfProcessRetired()
    {
        if (processRetired) throw new InvalidOperationException("Gannet process must restart after an end-build/reset failure.");
    }
    public PackageHost(HarbourCache cache, Microsoft.Build.Framework.ILogger? logger = null)
    {
        this.cache = cache;
        this.logger = logger ?? new Microsoft.Build.Logging.ConsoleLogger();
    }
    public BuildResult Build(string project, Dictionary<string, string?> inputs, bool reset = true)
    {
        ThrowIfProcessRetired();
        using var collection = new ProjectCollection(inputs);
        var services = new HostServices();
        services.RegisterHostObject(project, "Build", "ReadHarbour", new HarbourHostObject(cache));
        var parameters = new BuildParameters(collection) { EnableNodeReuse = false, MaxNodeCount = 1, HostServices = services, Loggers = [logger] };
        manager.BeginBuild(parameters);
        try
        {
            return manager.BuildRequest(new BuildRequestData(project, inputs, null, ["Build"], services));
        }
        finally
        {
            // This host owns the real lifecycle seam. Raise only after EndBuild returns,
            // including when the actual task fails. Unmodified third-party hosts are unqualified.
            try
            {
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
            catch
            {
                // Failed EndBuild is not confirmed completion. A throwing event handler
                // also prevents later subscribers resetting. Retire ALL owned hosts/caches.
                processRetired = true;
                throw;
            }
        }
    }
    public void Dispose() => manager.Dispose();
}
// endsnippet:cache
