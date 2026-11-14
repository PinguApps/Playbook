// snippet:complete
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Execution;
using Microsoft.Build.Graph;
using Microsoft.Build.Logging;

namespace CwmSeedBank.Host
{
    public static class CancelBuild
    {
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                string path = Path.Combine(args[0], new AssemblyName(eventArgs.Name).Name + ".dll");
                if (!File.Exists(path))
                    path = Path.Combine(Path.GetDirectoryName(args[0]), new AssemblyName(eventArgs.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
            return Run(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Run(string[] args)
        {
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(args[0], "MSBuild.exe"));
            string started = Path.Combine(args[3], "cancel.started");
            var properties = new Dictionary<string, string>
            {
                ["ObserverAssembly"] = args[2], ["SeedRoot"] = args[3],
                ["SeedIo"] = args[4], ["Batch"] = "cancel", ["SeedMode"] = "wait"
            };
            var parameters = new BuildParameters
            {
                ReportFileAccesses = true, EnableNodeReuse = false,
                Loggers = new[] { new ConsoleLogger() }
            };
            using (var manager = new BuildManager())
            {
                if (args.Length > 5 && args[5] == "contexts")
                {
                    // The same project and IO with different global properties.
                    // Naive newline-delimited context serialization collides here.
                    properties["Batch"] = "first";
                    properties["SeedMode"] = "";
                    properties["A"] = "x\nB=y";
                    var other = new Dictionary<string, string>(properties);
                    other["A"] = "x";
                    other["B"] = "y";
                    var entries = new[]
                    {
                        new ProjectGraphEntryPoint(args[1], properties),
                        new ProjectGraphEntryPoint(args[1], other)
                    };
                    return manager.Build(parameters, new GraphBuildRequestData(entries, new[] { "Build" }))
                        .OverallResult == BuildResultCode.Success ? 0 : 3;
                }
                var cancel = Task.Run(() =>
                {
                    for (int i = 0; i < 400 && !File.Exists(started); i++) Thread.Sleep(50);
                    if (!File.Exists(started)) throw new InvalidOperationException("Build did not reach active IO.");
                    manager.CancelAllSubmissions();
                });
                GraphBuildResult result = manager.Build(parameters,
                    new GraphBuildRequestData(args[1], properties, new[] { "Build" }, null));
                cancel.GetAwaiter().GetResult();
                Console.WriteLine("Active build cancellation result: " + result.OverallResult);
                return result.OverallResult == BuildResultCode.Failure ? 0 : 3;
            }
        }
    }
}
// endsnippet:complete
