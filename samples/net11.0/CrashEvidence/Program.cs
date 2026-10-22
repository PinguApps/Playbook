// snippet:crash-check
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Samples;

if (args is ["--crash"])
{
    DispatchFailure();
    return;
}
if (!OperatingSystem.IsLinux()) { throw new PlatformNotSupportedException("This crash evidence check requires the isolated Linux route."); }
string directory = Path.Combine(Path.GetTempPath(), $"dispatch-crash-{Guid.NewGuid():N}");
Directory.CreateDirectory(directory);
try
{
    ProcessStartInfo start = new(Environment.ProcessPath!)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        WorkingDirectory = directory
    };
    start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
    start.ArgumentList.Add("--crash");
    // Remove inherited diagnostic switches so this check measures its own configuration.
    foreach (string name in start.Environment.Keys.Where(k => k.StartsWith("DOTNET_Dbg", StringComparison.Ordinal) ||
        k.StartsWith("DOTNET_EnableCrash", StringComparison.Ordinal) || k.StartsWith("DOTNET_CrashReport", StringComparison.Ordinal) ||
        k.StartsWith("COMPlus_Dbg", StringComparison.Ordinal) || k.StartsWith("COMPlus_EnableCrash", StringComparison.Ordinal)).ToArray())
    {
        start.Environment.Remove(name);
    }
    DispatchCrash.Configure(start, directory);
    using Process child = Process.Start(start) ?? throw new InvalidOperationException("Cannot start crash fixture.");
    Task<string> stdout = child.StandardOutput.ReadToEndAsync();
    Task<string> stderr = child.StandardError.ReadToEndAsync();
    using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(30));
    try { await child.WaitForExitAsync(deadline.Token); }
    catch (OperationCanceledException) { child.Kill(entireProcessTree: true); throw; }
    string log = await stdout + await stderr;
    string[] reports = Directory.GetFiles(directory, "*.crashreport.json", SearchOption.AllDirectories);
    Console.WriteLine(log);
    foreach (string file in reports) { Console.WriteLine($"REPORT: {Path.GetRelativePath(directory, file)} ({new FileInfo(file).Length} bytes)"); }
    if (child.ExitCode == 0 || reports.Length == 0) { throw new InvalidOperationException("No crash evidence from failing child."); }
    string evidence = string.Join('\n', reports.Select(File.ReadAllText));
    if (Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length != reports.Length)
    {
        throw new InvalidOperationException("Report-only configuration produced an unexpected artifact.");
    }
    if (!evidence.Contains("DispatchFailure", StringComparison.Ordinal)) { throw new InvalidOperationException("Report lacks the original crash frame."); }
    Console.WriteLine($"PASS: configured crash report contains DispatchFailure; child exit={child.ExitCode}; {RuntimeInformation.FrameworkDescription}; Linux x64 actual crash, no mock.");
}
finally { Directory.Delete(directory, recursive: true); }

[MethodImpl(MethodImplOptions.NoInlining)]
static void DispatchFailure() => Environment.FailFast("dispatch-fixture-failure");
// endsnippet:crash-check
