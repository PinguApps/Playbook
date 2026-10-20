using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Playbook.Samples;

public static class Commands
{
    // snippet:capture
    public static async Task<(string Output, string Error, int ExitCode)> CaptureAsync(ProcessStartInfo start, CancellationToken token)
    {
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        ProcessTextOutput result = await Process.RunAndCaptureTextAsync(start, token);
        if (result.ExitStatus.Canceled) throw new OperationCanceledException(token);
        return (result.StandardOutput, result.StandardError, result.ExitStatus.ExitCode);
    }
    // endsnippet:capture

    // snippet:handles
    public static int RunOwned(ProcessStartInfo start)
    {
        start.UseShellExecute = false;
        using SafeFileHandle sink = File.OpenNullHandle();
        start.StandardOutputHandle = sink;
        start.StandardErrorHandle = sink;
        using SafeProcessHandle child = SafeProcessHandle.Start(start);
        // Both the process and redirected handles have explicit, independent owners.
        return child.WaitForExit().ExitCode;
    }
    // endsnippet:handles

    // snippet:prepare
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("macos")]
    public static int RunPrepared(ProcessStartInfo start, Action<int> prepare)
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Suspended startup requires Windows or macOS.");
        start.UseShellExecute = false;
        start.StartSuspended = true;
        using SafeProcessHandle child = SafeProcessHandle.Start(start);
        try
        {
            prepare(child.ProcessId);
            child.Resume();
            return child.WaitForExit().ExitCode;
        }
        catch
        {
            // Disposing the handle alone does not terminate a suspended child.
            child.Kill();
            child.WaitForExit();
            throw;
        }
    }
    // endsnippet:prepare

    // snippet:termination
    public static async Task<ProcessExitStatus> StopAsync(Process child)
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // false can mean the process raced with exit; always collect its status.
            _ = child.Signal(PosixSignal.SIGTERM);
        }
        else
        {
            try { child.Kill(); }
            catch (InvalidOperationException) { /* Already exited. */ }
        }
        return await child.WaitForExitStatusAsync();
    }
    // endsnippet:termination

    // snippet:lifetime
    public static int StartWithLifetime(ProcessStartInfo start, bool detached, bool parentBound)
    {
        if (detached && parentBound) throw new ArgumentException("Choose independent or parent-bound lifetime.");
        start.UseShellExecute = false;
        start.StartDetached = detached;
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            start.KillOnParentExit = parentBound;
        }
        else if (parentBound)
            throw new PlatformNotSupportedException("This parent-bound route supports Windows and Linux.");
        // Forget releases bookkeeping, not the child. Detach additionally changes OS attachment.
        return Process.StartAndForget(start);
    }
    // endsnippet:lifetime

    // snippet:color
    public static void ConfigureColor(ProcessStartInfo start, bool color)
    {
        start.Environment.Remove("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION");
        if (color)
        {
            start.Environment["FORCE_COLOR"] = "1";
            start.Environment.Remove("NO_COLOR");
        }
        else
        {
            start.Environment.Remove("FORCE_COLOR");
            start.Environment["NO_COLOR"] = "1";
        }
    }
    // endsnippet:color
}
