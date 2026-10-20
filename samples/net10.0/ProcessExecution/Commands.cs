using System.Diagnostics;

namespace Playbook.Samples;

public static class Commands
{
    // snippet:capture
    public static async Task<(string Output, string Error, int ExitCode)> CaptureAsync(ProcessStartInfo start, CancellationToken token)
    {
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        using Process process = Process.Start(start) ?? throw new InvalidOperationException("No child process.");
        Task<string> output = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        Task<string> error = process.StandardError.ReadToEndAsync(CancellationToken.None);
        try
        {
            await process.WaitForExitAsync(token);
        }
        catch (OperationCanceledException)
        {
            // Cancellation of WaitForExitAsync alone does not terminate the child.
            try { process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) { /* It exited between cancellation and Kill. */ }
            await process.WaitForExitAsync(CancellationToken.None);
            await Task.WhenAll(output, error);
            throw;
        }
        return (await output, await error, process.ExitCode);
    }
    // endsnippet:capture

    // snippet:handles
    public static int RunOwned(ProcessStartInfo start)
    {
        start.UseShellExecute = false;
        using Process owner = Process.Start(start) ?? throw new InvalidOperationException("No child process.");
        // SafeHandle is borrowed from owner; do not dispose it independently.
        if (owner.SafeHandle.IsInvalid) throw new InvalidOperationException("Invalid child handle.");
        owner.WaitForExit();
        return owner.ExitCode;
    }
    // endsnippet:handles

    // snippet:prepare
    public static int RunPrepared(ProcessStartInfo start, Action prepare)
    {
        // Managed .NET 10 has no StartSuspended/Resume pair. Prepare before launch.
        // Native post-creation/pre-execution setup would require an OS-specific launcher.
        prepare();
        return RunOwned(start);
    }
    // endsnippet:prepare

    // snippet:termination
    public static async Task<int> StopAsync(Process child)
    {
        try { child.Kill(); }
        catch (InvalidOperationException) { /* Already exited. */ }
        await child.WaitForExitAsync();
        // The integer alone cannot reliably distinguish normal exit from a POSIX signal.
        return child.ExitCode;
    }
    // endsnippet:termination

    // snippet:lifetime
    public static async Task RunScopedAsync(ProcessStartInfo start, CancellationToken stop)
    {
        start.UseShellExecute = false;
        using Process child = Process.Start(start) ?? throw new InvalidOperationException("No child process.");
        try { await child.WaitForExitAsync(stop); }
        finally
        {
            if (!child.HasExited)
            {
                child.Kill(entireProcessTree: true);
                await child.WaitForExitAsync(CancellationToken.None);
            }
        }
    }
    // endsnippet:lifetime

    // snippet:color
    public static void ConfigureColor(ProcessStartInfo start, bool color)
    {
        // .NET 10's Unix Console uses the earlier runtime-specific switch.
        start.Environment["DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION"] = color ? "1" : "0";
        start.Environment.Remove("FORCE_COLOR");
        if (color) start.Environment.Remove("NO_COLOR");
        else start.Environment["NO_COLOR"] = "1";
    }
    // endsnippet:color
}
