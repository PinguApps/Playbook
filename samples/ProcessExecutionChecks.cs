using System.Diagnostics;
using System.Reflection;
using System.Text;
using Playbook.Samples;
#if NET11_0_OR_GREATER
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
#endif

static ProcessStartInfo Self(params string[] arguments)
{
    string executable = Environment.ProcessPath ?? throw new InvalidOperationException("No process executable.");
    var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true };
    if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
    foreach (string argument in arguments) start.ArgumentList.Add(argument);
    start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
    return start;
}

if (args.Length > 0)
{
    switch (args[0])
    {
        case "--report":
            Console.WriteLine("beacon report");
            await Console.Error.WriteLineAsync("calibration warning");
            await Console.Out.WriteAsync(new string('o', 70000));
            await Console.Error.WriteAsync(new string('e', 70000));
            return 7;
        case "--sleep":
            await File.WriteAllTextAsync(args[1], Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            await Task.Delay(TimeSpan.FromSeconds(30));
            return 0;
        case "--stamp":
            await File.WriteAllTextAsync(args[1], "prepared");
            return 7;
        case "--color":
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("beacon");
            Console.ResetColor();
            return 0;
#if NET11_0_OR_GREATER
        case "--parent":
            using (SafeFileHandle sink = File.OpenNullHandle())
            {
            ProcessStartInfo worker = Self("--sleep", args[1]);
            worker.StandardOutputHandle = sink;
            worker.StandardErrorHandle = sink;
            int pid = Commands.StartWithLifetime(worker, detached: args[2] == "detached", parentBound: args[2] == "bound");
            await WaitForFile(args[1]);
            if (pid <= 0) throw new InvalidOperationException("Missing child PID.");
            }
            return 0;
#endif
    }
    throw new ArgumentException("Unknown worker mode.");
}

string directory = Path.Combine(Path.GetTempPath(), "playbook-process-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
try
{
    var captured = await Commands.CaptureAsync(Self("--report"), CancellationToken.None);
    if (!captured.Output.StartsWith("beacon report", StringComparison.Ordinal) || !captured.Error.StartsWith("calibration warning", StringComparison.Ordinal) ||
        captured.Output.Length < 70000 || captured.Error.Length < 70000 || captured.ExitCode != 7)
        throw new InvalidOperationException("Concurrent output drain/nonzero exit failed.");
    string ready = Path.Combine(directory, "cancel.pid");
    using (var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
    {
        Task operation = Commands.CaptureAsync(Self("--sleep", ready), cancel.Token);
        await WaitForFile(ready);
        int pid = int.Parse(await File.ReadAllTextAsync(ready), System.Globalization.CultureInfo.InvariantCulture);
        await cancel.CancelAsync();
        try { await operation; throw new InvalidOperationException("Cancellation did not propagate."); }
        catch (OperationCanceledException) { }
        for (int attempt = 0; IsRunning(pid) && attempt < 100; attempt++) await Task.Delay(50);
        if (IsRunning(pid)) throw new InvalidOperationException("Cancellation left child alive.");
    }
    if (Commands.RunOwned(Self("--stamp", Path.Combine(directory, "owned"))) != 7)
        throw new InvalidOperationException("Owned-handle exit result failed.");
    string prepared = Path.Combine(directory, "prepared");
#if NET11_0_OR_GREATER
    if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
    {
        bool callback = false;
        int exit = Commands.RunPrepared(Self("--stamp", prepared), pid =>
        {
            callback = true;
            if (!IsRunning(pid) || File.Exists(prepared)) throw new InvalidOperationException("Child ran before setup.");
        });
        if (!callback || exit != 7 || !File.Exists(prepared)) throw new InvalidOperationException("Resume failed.");
        int failedPid = 0;
        try { Commands.RunPrepared(Self("--stamp", prepared + ".failed"), pid => { failedPid = pid; throw new FormatException("setup failed"); }); }
        catch (FormatException) { }
        if (failedPid == 0 || IsRunning(failedPid) || File.Exists(prepared + ".failed"))
            throw new InvalidOperationException("Setup failure left suspended child.");
    }
    using (SafeProcessHandle current = SafeProcessHandle.Open(Environment.ProcessId))
        if (current.ProcessId != Environment.ProcessId) throw new InvalidOperationException("Open ownership failed.");
    if (SafeProcessHandle.TryOpen(int.MaxValue, out SafeProcessHandle? missing))
    {
        missing.Dispose();
        throw new InvalidOperationException("Opened missing PID.");
    }
#else
    if (Commands.RunPrepared(Self("--stamp", prepared), () => { if (File.Exists(prepared)) throw new InvalidOperationException(); }) != 7)
        throw new InvalidOperationException("Prelaunch preparation failed.");
#endif
    ready = Path.Combine(directory, "stop.pid");
    using (Process child = Process.Start(Self("--sleep", ready)) ?? throw new InvalidOperationException())
    {
        await WaitForFile(ready);
#if NET11_0_OR_GREATER
        ProcessExitStatus status = await Commands.StopAsync(child);
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            if (status.Signal != PosixSignal.SIGTERM) throw new InvalidOperationException("Lost POSIX termination signal.");
        }
        else if (status.Signal != null) throw new InvalidOperationException("Invented Windows POSIX signal.");
#else
        _ = await Commands.StopAsync(child);
#endif
    }
#if NET11_0_OR_GREATER
    using (Process exited = Process.Start(Self("--stamp", Path.Combine(directory, "exit"))) ?? throw new InvalidOperationException())
    {
        ProcessExitStatus status = await exited.WaitForExitStatusAsync();
        if (status.ExitCode != 7 || status.Signal != null || status.Canceled) throw new InvalidOperationException("Normal exit status failed.");
    }
    foreach (string mode in new[] { "bound", "forgotten", "detached" })
    {
        if (mode == "bound" && !OperatingSystem.IsWindows() && !OperatingSystem.IsLinux()) continue;
        ready = Path.Combine(directory, mode + ".pid");
        using (Process parent = Process.Start(Self("--parent", ready, mode)) ?? throw new InvalidOperationException())
        {
            await parent.WaitForExitAsync();
            if (parent.ExitCode != 0) throw new InvalidOperationException("Lifetime parent failed.");
        }
        int pid = int.Parse(await File.ReadAllTextAsync(ready), System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            for (int attempt = 0; mode == "bound" && IsRunning(pid) && attempt < 50; attempt++) await Task.Delay(100);
            if (IsRunning(pid) == (mode == "bound")) throw new InvalidOperationException("Wrong " + mode + " lifetime.");
        }
        finally
        {
            if (IsRunning(pid)) { using Process child = Process.GetProcessById(pid); child.Kill(); await child.WaitForExitAsync(); }
        }
    }
#else
    ready = Path.Combine(directory, "scoped.pid");
    using (var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
    {
        Task operation = Commands.RunScopedAsync(Self("--sleep", ready), cancel.Token);
        await WaitForFile(ready);
        int pid = int.Parse(await File.ReadAllTextAsync(ready), System.Globalization.CultureInfo.InvariantCulture);
        await cancel.CancelAsync();
        try { await operation; } catch (OperationCanceledException) { }
        if (IsRunning(pid)) throw new InvalidOperationException("Scoped cleanup failed.");
    }
#endif
    var color = Self("--color");
    color.Environment["TERM"] = "xterm-256color";
    Commands.ConfigureColor(color, true);
    string colored = (await Commands.CaptureAsync(color, CancellationToken.None)).Output;
    Commands.ConfigureColor(color, false);
    string plain = (await Commands.CaptureAsync(color, CancellationToken.None)).Output;
    if (!plain.Contains("beacon", StringComparison.Ordinal)) throw new InvalidOperationException("Lost plain output.");
    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
    {
        if (!colored.Contains('\x1B', StringComparison.Ordinal) || plain.Contains('\x1B', StringComparison.Ordinal)) throw new InvalidOperationException("Piped ANSI environment contract failed.");
    }
    Console.WriteLine($"PASS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}; output/error, cancellation cleanup, handles, preparation and termination; piped ANSI forced={colored.Contains('\x1B', StringComparison.Ordinal)}, disabled={plain.Contains('\x1B', StringComparison.Ordinal)}.");
}
finally { Directory.Delete(directory, recursive: true); }
return 0;

static async Task WaitForFile(string path)
{
    for (int attempt = 0; attempt < 200; attempt++)
    {
        if (File.Exists(path) && new FileInfo(path).Length > 0) return;
        await Task.Delay(50);
    }
    throw new TimeoutException("Child readiness: " + path);
}

static bool IsRunning(int pid)
{
    try { using Process child = Process.GetProcessById(pid); return !child.HasExited; }
    catch (ArgumentException) { return false; }
}
