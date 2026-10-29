using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Build.Execution;
#if NET11_0
using NuGet.Common;
#endif

namespace Gannet;

internal static class WorkshopChecks
{
    public static async Task Run(string sdkRoot)
    {
        var pins = JsonSerializer.Deserialize<Dictionary<string, Pin[]>>(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "artifact-pins.json")))!;
        foreach (var pin in pins[Path.GetFileName(sdkRoot)])
            Require(Hash(Path.Combine(sdkRoot, pin.path)) == pin.sha256, "Artifact changed: " + pin.path);
        Console.WriteLine($"ARTIFACTS {pins[Path.GetFileName(sdkRoot)].Length} SDK/compiler/ref/runtime/native-host hashes matched.");
        string folder = Path.Combine(Path.GetTempPath(), "gannet-workshop-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            CheckBuilds(folder);
            // Process retirement is intentionally irreversible. Real destructive lifecycle
            // controls run in owned instances of this same executable, then exit.
            foreach (string mode in Environment.Version.Major == 11 ? new[] { "end", "reset" } : new[] { "end" })
            {
                using var faultDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                string faultOutput = await CourierLauncher.Launch(Environment.ProcessPath!,
                    [Assembly.GetExecutingAssembly().Location, "--host-fault-probe", mode], [], faultDeadline.Token);
                Require(faultOutput.Contains("PASS process-wide retirement " + mode, StringComparison.Ordinal), "Fault child did not complete proof");
                Console.WriteLine(faultOutput.Trim());
            }
            string repository = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
            await AnalyzerChecks.Run(repository, sdkRoot);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            string marker = "fixture-token-" + Guid.NewGuid().ToString("N");
            string missing = Path.Combine(folder, marker, "missing-courier.exe");
            try
            {
                await CourierLauncher.Launch(missing, ["--api-key", marker], [marker], timeout.Token, $"<fixture>/{marker}/missing-courier.exe");
                throw new InvalidOperationException("Missing executable unexpectedly launched");
            }
            catch (InvalidOperationException error) when (error.Message.Contains("OS error", StringComparison.Ordinal))
            {
                Require(!error.Message.Contains(marker, StringComparison.Ordinal) && error.Message.Contains("[redacted]", StringComparison.Ordinal), "Secret leaked");
                Require(error.Message.Contains("missing-courier.exe", StringComparison.Ordinal), "No actionable path");
                Console.WriteLine("MISSING " + error.Message.Replace(folder, "<fixture>", StringComparison.OrdinalIgnoreCase));
            }
            // Even without a secret list, the default diagnostic never echoes the private
            // executable directory or arguments. The OS still attempts that actual path.
            try
            {
                await CourierLauncher.Launch(missing, [marker], [], timeout.Token);
                throw new InvalidOperationException("Missing executable unexpectedly launched");
            }
            catch (InvalidOperationException error) when (error.Message.Contains("OS error", StringComparison.Ordinal))
            {
                Require(!error.Message.Contains(marker, StringComparison.Ordinal) && error.Message.Contains("<configured courier>", StringComparison.Ordinal), "Default diagnostic leaked an unlisted path/argument secret");
            }
            string output = await CourierLauncher.Launch(Environment.ProcessPath!, [Assembly.GetExecutingAssembly().Location, "--courier-child"], [marker], timeout.Token);
            Require(output.Trim() == "Gannet courier ready", "Valid launch changed output");
            Console.WriteLine("PASS real missing executable OS spawn failure, secret-bearing path/arguments redacted; valid benign original child output and exit0 unchanged. No credential exchange.");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && a.Location.StartsWith(sdkRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).OrderBy(a => a.FullName, StringComparer.Ordinal))
            {
                string relative = Path.GetRelativePath(sdkRoot, assembly.Location).Replace('\\', '/');
                Require(pins[Path.GetFileName(sdkRoot)].Any(p => p.path == relative && p.sha256 == Hash(assembly.Location)), "Unqualified loaded assembly: " + relative);
                Console.WriteLine($"LOADED {assembly.FullName} tfm={assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName} sha256={Hash(assembly.Location)} product={FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion}");
            }
        }
        finally { Directory.Delete(folder, true); }
    }

    private static void CheckBuilds(string folder)
    {
        string project = Path.Combine(folder, "Gannet.proj"), receipt = Path.Combine(folder, "receipt.txt"), config = Path.Combine(folder, "harbour.txt");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Gannet.proj"), project);
        using var cache = new HarbourCache(() => File.ReadAllText(config));
        using var host = new PackageHost(cache);
        void Build(string value, bool reset, bool fail = false)
        {
            File.WriteAllText(config, value);
            var result = host.Build(project, new() { ["Expected"] = value, ["Receipt"] = receipt, ["FailBuild"] = fail.ToString(), ["HarbourAssembly"] = Assembly.GetExecutingAssembly().Location }, reset);
            Require(result.OverallResult == (fail ? BuildResultCode.Failure : BuildResultCode.Success), "Actual build result mismatch");
        }
        Build("moor-A", reset: false);
        Build("moor-B", reset: false);
        Require(File.ReadAllText(receipt) == $"{Environment.ProcessId}|moor-A|moor-B", "Stale control did not share the actual task/cache/PID");
        cache.Invalidate();
        Console.WriteLine($"STALE same host/task PID={Environment.ProcessId}; actual build2 expected=moor-B cached=moor-A.");
        int created = cache.Creations;
        Build("quay-A", reset: true);
        Require(cache.Creations == created + 1, "Reset recomputed ending configuration");
        Require(File.ReadAllText(receipt) == $"{Environment.ProcessId}|quay-A|quay-A", "First actual receipt mismatch");
        Build("quay-B", reset: true);
        Require(cache.Creations == created + 2, "Next build did not recreate lazily");
        Require(File.ReadAllText(receipt) == $"{Environment.ProcessId}|quay-B|quay-B", "Second actual receipt mismatch");
        using (var operation = new FileStream(Path.Combine(folder, "operation.dat"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
        {
            operation.WriteByte(41);
            Build("quay-failure", reset: true, fail: true);
            Require(cache.Creations == created + 3, "Failure reset read environment eagerly");
            operation.WriteByte(42); operation.Position = 0;
            Require(operation.ReadByte() == 41 && operation.ReadByte() == 42, "In-flight operation-owned file broken by reset");
            operation.Dispose();
            try { operation.WriteByte(43); throw new InvalidOperationException("File not disposed"); }
            catch (ObjectDisposedException) { }
        }
        int resets = cache.Invalidations;
        cache.Dispose();
#if NET11_0
        // A real build after unsubscription checks removal; the cache has no more task users.
        using var successor = new HarbourCache(() => File.ReadAllText(config));
        using var nextHost = new PackageHost(successor);
        File.WriteAllText(config, "after-unsubscribe");
        Require(nextHost.Build(project, new() { ["Expected"] = "after-unsubscribe", ["Receipt"] = receipt, ["FailBuild"] = "false", ["HarbourAssembly"] = Assembly.GetExecutingAssembly().Location }).OverallResult == BuildResultCode.Success, "Successor build failed");
        Require(cache.Invalidations == resets && successor.Invalidations == 1, "Unsubscription or duplicate NuGet assembly");
        Require(AppDomain.CurrentDomain.GetAssemblies().Count(a => a.GetName().Name == "NuGet.Common") == 1, "Duplicate event assembly");
#else
        Require(typeof(NuGet.Common.ILogger).Assembly.GetType("NuGet.Common.StaticState") is null, "Unexpected older event API");
#endif
        Console.WriteLine("PASS real in-process MSBuild tasks/config A+B, success/failure EndBuild then invalidation, lazy NEXT configuration, one shared host/cache/assembly; operation-owned open file usable across reset and actually disposed. Explicit old reset / current event unsubscription controls.");
    }
    public static void RunFaultProbe(string mode)
    {
        if (mode is not ("end" or "reset")) throw new ArgumentException("Unknown fault control.");
        string folder = Path.Combine(Path.GetTempPath(), "gannet-workshop-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string project = Path.Combine(folder, "Gannet.proj"), config = Path.Combine(folder, "harbour.txt");
            string ownerReceipt = Path.Combine(folder, "owner.txt"), siblingReceipt = Path.Combine(folder, "sibling.txt");
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Gannet.proj"), project);
            using var ownerCache = new HarbourCache(() => File.ReadAllText(config));
            using var owner = new PackageHost(ownerCache, mode == "end" ? new EndFailureLogger() : null);
            Action broken = () => throw new InvalidOperationException("reset-bug-control");
#if NET11_0
            if (mode == "reset") StaticState.BuildEnded += broken;
#else
            Require(mode == "end", "Old SDK has no reset-event probe");
#endif
            try
            {
                // Subscribe AFTER the throwing handler so multicast short-circuiting really
                // strands this later sibling's cached A value when reset fails.
                using var siblingCache = new HarbourCache(() => File.ReadAllText(config));
                using var sibling = new PackageHost(siblingCache);
                Dictionary<string, string?> Inputs(string receipt, string value) => new()
                {
                    ["Expected"] = value, ["Receipt"] = receipt, ["FailBuild"] = "false",
                    ["HarbourAssembly"] = Assembly.GetExecutingAssembly().Location
                };
                File.WriteAllText(config, "fault-A");
                Require(sibling.Build(project, Inputs(siblingReceipt, "fault-A"), reset: false).OverallResult == BuildResultCode.Success, "Sibling seed task failed");
                Require(File.ReadAllText(siblingReceipt) == $"{Environment.ProcessId}|fault-A|fault-A", "No real sibling seed receipt");
                File.WriteAllText(config, "fault-B");
                try { owner.Build(project, Inputs(ownerReceipt, "fault-B")); throw new InvalidOperationException("Lifecycle fault did not surface"); }
                catch (Microsoft.Build.Framework.LoggerException e) when (mode == "end" && e.Message.Contains("gannet-end-failure", StringComparison.Ordinal)) { }
                catch (InvalidOperationException e) when (mode == "reset" && e.Message == "reset-bug-control") { }
                Require(File.ReadAllText(ownerReceipt) == $"{Environment.ProcessId}|fault-B|fault-B", "Fault skipped actual owner task");
                Require(siblingCache.Invalidations == 0 && siblingCache.Creations == 1, "Late subscriber unexpectedly reset or recomputed");
                Refuse(() => sibling.Build(project, Inputs(siblingReceipt, "fault-B")));
                Refuse(() => { _ = siblingCache.Value; });
                Refuse(() => owner.Build(project, Inputs(ownerReceipt, "fault-B")));
                using var freshCache = new HarbourCache(() => File.ReadAllText(config));
                using var freshHost = new PackageHost(freshCache);
                Refuse(() => freshHost.Build(project, Inputs(ownerReceipt, "fault-B")));
                Require(siblingCache.Creations == 1 && freshCache.Creations == 0 &&
                    File.ReadAllText(siblingReceipt) == $"{Environment.ProcessId}|fault-A|fault-A", "Poisoned process executed another task/cache read");
                if (mode == "end") Require(ownerCache.Invalidations == 0, "Failed EndBuild broadcast a reset");
                Console.WriteLine($"PASS process-wide retirement {mode}: actual tasks samePID={Environment.ProcessId}; owner fault surfaces, late sibling stays unreset, sibling/direct-cache/new-host work refused; fresh process required.");
            }
            finally
            {
#if NET11_0
                if (mode == "reset") StaticState.BuildEnded -= broken;
#endif
            }
        }
        finally { Directory.Delete(folder, true); }
    }
    private static void Refuse(Action action)
    {
        try { action(); throw new InvalidOperationException("Retired process accepted work"); }
        catch (InvalidOperationException e) when (e.Message.Contains("process must restart", StringComparison.Ordinal)) { }
    }
    private sealed class EndFailureLogger : Microsoft.Build.Framework.ILogger
    {
        private bool failed;
        public Microsoft.Build.Framework.LoggerVerbosity Verbosity { get; set; }
        public string? Parameters { get; set; }
        public void Initialize(Microsoft.Build.Framework.IEventSource eventSource) { }
        public void Shutdown()
        {
            if (!failed) { failed = true; throw new Microsoft.Build.Framework.LoggerException("gannet-end-failure"); }
        }
    }
    private sealed record Pin(string path, string sha256);
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
