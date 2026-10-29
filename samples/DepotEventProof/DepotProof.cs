using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

// snippet:fixture
namespace Depot.Events;

internal static class DepotProof
{
    public static void Run(string sdkRoot)
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../..", "artifacts", "depot-events", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);
        fixtureRoot = root;
        string salt = Guid.NewGuid().ToString("N");
        var owned = new List<Process>();
        try
        {
            CheckArtifacts(sdkRoot);
            MakeGraph(root, sdkRoot);
            var spawned = Build("spawned", true, true);
#if NET11_0
            Require(spawned.Server is { Kind: MSBuildServerLifecycleKind.Spawned, ShortLived: false }, "Expected real spawned server");
            Process server = owned.Single();
            Require(spawned.Server!.ProcessId == server.Id, "Lifecycle PID differs from actual task receipt");
            var reused = Build("reused", true, true);
            Require(reused.Server is { Kind: MSBuildServerLifecycleKind.Reused } && reused.Server.ProcessId == server.Id, "Server was not reused");
            Require(!server.HasExited, "Reused server already exited");
            var noReuse = Build("no-reuse", true, false);
            Require(noReuse.Server is { Kind: MSBuildServerLifecycleKind.NotUsed, ProcessId: 0 } && !string.IsNullOrEmpty(noReuse.Server.ReasonCode), "Missing actual no-server reason");
            var disabled = Build("disabled", false, false);
            Require(disabled.Server is null, "Unrequested server should emit no lifecycle event");
            Console.WriteLine("PASS: actual Spawned/Reused share owned PID; requested/no-reuse emits NotUsed + stable reason code; disabled emits no lifecycle event.");
#else
            // The old SDK has no structured lifecycle/conflict types. A unique salt still isolates its server.
            // Its PID receipt is produced by an owned task inside the real build.
            Process server = owned.Single();
            int id = server.Id;
            Build("reused", true, true);
            Require(int.Parse(File.ReadAllText(Path.Combine(root, "reused.pid"))) == id, "Old server not reused");
            Build("no-reuse", true, false);
            Build("disabled", false, false);
            Require(int.Parse(File.ReadAllText(Path.Combine(root, "no-reuse.pid"))) != id && int.Parse(File.ReadAllText(Path.Combine(root, "disabled.pid"))) != id, "Old no-server control used resident server");
            Require(typeof(BuildEventArgs).Assembly.GetType("Microsoft.Build.Framework.AssemblyConflictWarningEventArgs") is null, "Unexpected old typed API");
            Console.WriteLine("PASS: actual SDK10 CLI spawn/reuse and no-server PID controls; legacy MSB3277 + diagnostic messages replayed without text parsing.");
#endif
            CheckConstructors();
            string failedLog = Path.Combine(root, "warning-error.binlog");
            RunProcess(new[] { Path.Combine(sdkRoot, "MSBuild.dll"), Path.Combine(root, "Depot.proj"), "-nologo", "-v:diag", "-nr:false", "-warnaserror:MSB3277", "-p:Receipt=" + Path.Combine(root, "failed.pid"), "-bl:" + failedLog + ";ProjectImports=None" }, new Dictionary<string, string> { ["MSBUILDUSESERVER"] = "0" }, expectFailure: true);
            var errors = new DepotEvents();
            var failedReplay = new BinaryLogReplayEventSource();
            errors.Initialize(failedReplay);
            failedReplay.Replay(failedLog);
            errors.Shutdown();
            Require(errors.ErrorConflict, "Warning-as-error did not fail with MSB3277");
            Console.WriteLine("PASS: actual -warnaserror:MSB3277 failed and replayed a BuildErrorEventArgs; do not require a typed warning after promotion.");
            Console.WriteLine("PASS: real conflicting DepotSignal reference graph; matched binlog replay; constructor controls; private raw binlogs retained only under ignored artifacts.");

            DepotEvents Build(string name, bool useServer, bool reuse)
            {
                var env = new Dictionary<string, string> { ["MSBUILDUSESERVER"] = useServer ? "1" : "0", ["MSBUILDNODEHANDSHAKESALT"] = salt };
                string binlog = Path.Combine(root, name + ".binlog");
                RunProcess(new[] { Path.Combine(sdkRoot, "MSBuild.dll"), Path.Combine(root, "Depot.proj"), "-nologo", "-v:diag", "-m:1", "-nr:" + reuse.ToString().ToLowerInvariant(), "-p:Receipt=" + Path.Combine(root, name + ".pid"), "-bl:" + binlog + ";ProjectImports=None" }, env);
                if (name == "spawned")
                {
                    // Register cleanup before replay/assertions can fail. This task executed only in our salted host.
                    Process server = Process.GetProcessById(int.Parse(File.ReadAllText(Path.Combine(root, name + ".pid"))));
                    owned.Add(server);
                    Require(server.StartTime.ToUniversalTime() >= started.AddSeconds(-2), "Server predates fixture");
                    Require(string.Equals(server.MainModule!.FileName, Environment.ProcessPath, StringComparison.OrdinalIgnoreCase), "Unexpected server executable");
                }
                var events = new DepotEvents();
                var replay = new BinaryLogReplayEventSource();
                events.Initialize(replay);
                replay.Replay(binlog);
                events.Shutdown();
                Require(events.LegacyConflict, "Real graph did not emit MSB3277");
#if NET11_0
                Require(events.ExpectedConflict && events.ExpectedSearch && events.ExpectedDetails, "Typed events lost expected victor/victim/dependency/search relationships");
#endif
                Console.WriteLine($"REPLAY {name}: MSB3277={events.LegacyConflict}; {events.Summary}");
                return events;
            }
        }
        finally
        {
            foreach (Process process in owned)
            {
                if (!process.HasExited) process.Kill();
                Require(process.WaitForExit(10000), "Owned server cleanup timed out");
                Console.WriteLine("CLEANUP: owned server handle exited; no global shutdown or unowned process touched.");
                process.Dispose();
            }
        }
    }

    private static DateTime started = DateTime.UtcNow;
    private static string fixtureRoot = "";

    private static void CheckArtifacts(string sdkRoot)
    {
        using var pins = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "artifact-pins.json")));
        foreach (var pin in pins.RootElement.GetProperty(Path.GetFileName(sdkRoot)).EnumerateArray())
        {
            string file = Path.Combine(sdkRoot, pin.GetProperty("path").GetString()!);
            string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();
            Require(hash == pin.GetProperty("sha256").GetString(), "SDK artifact changed");
            // Full closure hashes live in the canonical pin manifest; keep emitted proof compact.
        }
        Console.WriteLine($"ARTIFACTS {pins.RootElement.GetProperty(Path.GetFileName(sdkRoot)).GetArrayLength()} exact SDK/compiler/ref/native-host/complete MSBuild deps/resolver files verified; pins sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "artifact-pins.json")))).ToLowerInvariant()}");
        Require(typeof(BuildEventArgs).Assembly.Location == Path.Combine(sdkRoot, "Microsoft.Build.Framework.dll"), "Reader loaded unmatched Framework");
    }

    private static void MakeGraph(string root, string sdkRoot)
    {
        string version = Path.GetFileName(sdkRoot).StartsWith("11.", StringComparison.Ordinal) ? "11.0.0-rc.2.26504.105" : "10.0.13";
        string tfm = version.StartsWith("11.", StringComparison.Ordinal) ? "net11.0" : "net10.0";
        string reference = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "packs", "Microsoft.NETCore.App.Ref", version, "ref", tfm, "System.Runtime.dll");
        // Disposable strong-name identity only; never used as a credential or shipped key.
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.PersistKeyInCsp = false;
            File.WriteAllBytes(Path.Combine(root, "depot.snk"), rsa.ExportCspBlob(true));
        }
        Console.WriteLine($"REFERENCE System.Runtime.dll sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(reference))).ToLowerInvariant()}; runtime={Environment.Version}; frameworkAssemblyVersion={typeof(BuildEventArgs).Assembly.GetName().Version}");
        foreach (string v in new[] { "1", "2" })
        {
            string folder = Path.Combine(root, "v" + v);
            Directory.CreateDirectory(folder);
            string source = Path.Combine(folder, "Signal.cs");
            File.WriteAllText(source, $"using System.Reflection; [assembly: AssemblyVersion(\"{v}.0.0.0\")] public class Signal {{ public static int Value => {v}; }}");
            Compile(source, Path.Combine(folder, "DepotSignal.dll"));
            File.WriteAllText(source, $"using System.Reflection; [assembly: AssemblyVersion(\"{v}.0.0.0\")] public class Gauge {{ public static int Value => {v}; }}");
            Compile(source, Path.Combine(folder, "DepotGauge.dll"));
            string dependent = v == "1" ? "Dispatch" : "Inventory";
            source = Path.Combine(folder, dependent + ".cs");
            File.WriteAllText(source, $"public class {dependent} {{ public static int Read() => Signal.Value + Gauge.Value; }}");
            Compile(source, Path.Combine(folder, "Depot" + dependent + ".dll"), Path.Combine(folder, "DepotSignal.dll"), Path.Combine(folder, "DepotGauge.dll"));
        }
        string reader = Assembly.GetExecutingAssembly().Location;
        new XElement("Project",
            new XElement("UsingTask", new XAttribute("TaskName", "ResolveAssemblyReference"), new XAttribute("AssemblyFile", Path.Combine(sdkRoot, "Microsoft.Build.Tasks.Core.dll"))),
            new XElement("UsingTask", new XAttribute("TaskName", "Depot.Events.DepotPid"), new XAttribute("AssemblyFile", reader)),
            new XElement("ItemGroup",
                new XElement("DepotReference", new XAttribute("Include", "v1/DepotSignal.dll")),
                new XElement("DepotReference", new XAttribute("Include", "v1/DepotDispatch.dll")),
                new XElement("DepotReference", new XAttribute("Include", "v2/DepotInventory.dll")),
                new XElement("DepotReference", new XAttribute("Include", "DepotMissing"))),
            new XElement("Target", new XAttribute("Name", "Inspect"),
                new XElement("DepotPid", new XAttribute("Receipt", "$(Receipt)")),
                new XElement("ResolveAssemblyReference", new XAttribute("Assemblies", "@(DepotReference)"),
                    new XAttribute("SearchPaths", "{RawFileName};" + Path.Combine(root, "v1") + ";" + Path.Combine(root, "v2") + ";" + Path.GetDirectoryName(reference)),
                    new XAttribute("TargetFrameworkDirectories", Path.GetDirectoryName(reference)!),
                    new XAttribute("AutoUnify", "false"), new XAttribute("FindDependencies", "true"))))
            .Save(Path.Combine(root, "Depot.proj"));

        void Compile(string source, string output, params string[] dependencies)
        {
            var args = new List<string> { Path.Combine(sdkRoot, "Roslyn", "bincore", "csc.dll"), "-nologo", "-target:library", "-keyfile:" + Path.Combine(root, "depot.snk"), "-out:" + output, "-r:" + reference, source };
            foreach (string dependency in dependencies) args.Add("-r:" + dependency);
            RunProcess(args, new Dictionary<string, string> { ["MSBUILDUSESERVER"] = "0" });
        }
    }

    // snippet:constructors
    private static void CheckConstructors()
    {
        // Existing text constructors remain valid on both actual SDKs.
        var old = new BuildManager.DeferredBuildMessage("Depot scan starting", MessageImportance.High);
        Require(old.Text == "Depot scan starting" && old.Importance == MessageImportance.High, "Text constructor changed");
#if NET11_0
        // A hand-created event tests only the constructor, never actual server emission.
        var message = new BuildMessageEventArgs("Depot scan starting", null, "Depot", MessageImportance.High);
        var current = new BuildManager.DeferredBuildMessage(message);
        Require(current.Text == message.Message && current.Importance == message.Importance, "Event constructor lost text/importance");
#else
        Require(typeof(BuildManager.DeferredBuildMessage).GetConstructor(new[] { typeof(BuildEventArgs) }) is null, "Unexpected old event constructor");
#endif
        Console.WriteLine("PASS: actual old/current message constructor controls; no new API backport claimed.");
    }
    // endsnippet:constructors

    private static void RunProcess(IEnumerable<string> args, Dictionary<string, string> environment, bool expectFailure = false)
    {
        var start = new ProcessStartInfo(Environment.ProcessPath!) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (string arg in args) start.ArgumentList.Add(arg);
        // Exact arguments/environment are retained locally. Published proof redacts private roots.
        string command = JsonSerializer.Serialize(new { executable = start.FileName, arguments = start.ArgumentList, environment });
        File.AppendAllText(Path.Combine(fixtureRoot, "commands.jsonl"), command + Environment.NewLine);
        Console.WriteLine("COMMAND " + JsonSerializer.Serialize(new
        {
            executable = "<dotnet-root>/dotnet.exe",
            arguments = start.ArgumentList.Select(Redact),
            environment = environment.ToDictionary(p => p.Key, p => p.Key == "MSBUILDNODEHANDSHAKESALT" ? "<unique-fixture-salt>" : p.Value)
        }));
        start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
        start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        foreach (var pair in environment) start.Environment[pair.Key] = pair.Value;
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(60000)) { process.Kill(true); throw new InvalidOperationException("Owned child timed out"); }
        Task.WaitAll(output, error);
        // Raw output can contain private paths; save locally, never publish it.
        if ((process.ExitCode != 0) != expectFailure) throw new InvalidOperationException(output.Result + error.Result);

        static string Redact(string value) => value.Replace(fixtureRoot, "<graph>", StringComparison.OrdinalIgnoreCase)
            .Replace(Path.GetDirectoryName(Environment.ProcessPath)!, "<dotnet-root>", StringComparison.OrdinalIgnoreCase).Replace('\\', '/');
    }

    internal static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

public sealed class DepotPid : Microsoft.Build.Utilities.Task
{
    [Required] public string Receipt { get; set; } = "";
    public override bool Execute() { File.WriteAllText(Receipt, Environment.ProcessId.ToString()); return true; }
}
// endsnippet:fixture
