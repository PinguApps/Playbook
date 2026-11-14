// snippet:complete
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace CwmSeedBank.Tests;
internal static class Probe
{
    [ModuleInitializer]
    internal static void Startup()
    {
        if (Environment.GetEnvironmentVariable("SEED_STARTUP_FAIL") == "1") Environment.Exit(7);
        if (AppContext.TargetFrameworkName != ".NETCoreApp,Version=v11.0")
            throw new InvalidOperationException("The consumer target changed.");
    }
    internal static void Record(string module, bool first)
    {
        string identity = module + (first ? ".first" : ".second");
        string? directory = Environment.GetEnvironmentVariable("SEED_MARKERS");
        if (directory is not null) File.WriteAllText(Path.Combine(directory, identity), "executed");
        if (Environment.GetEnvironmentVariable("SEED_WAIT") == identity) Thread.Sleep(60000);
        if (Environment.GetEnvironmentVariable("SEED_FAIL") == identity) Assert.Fail("Intentional affected-test failure");
    }
}
// endsnippet:complete
