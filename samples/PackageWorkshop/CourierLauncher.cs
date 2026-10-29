// snippet:launcher
using System.ComponentModel;
using System.Diagnostics;

namespace Gannet;

public static class CourierLauncher
{
    public static async Task<string> Launch(string executable, IReadOnlyList<string> arguments,
        IReadOnlyList<string> secrets, CancellationToken cancellationToken)
    {
        if (!Path.IsPathFullyQualified(executable)) throw new ArgumentException("Use an absolute executable path.");
        string Redact(string text)
        {
            foreach (string secret in secrets.Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length))
                text = text.Replace(secret, "[redacted]", StringComparison.Ordinal);
            return text.Replace('\r', ' ').Replace('\n', ' ');
        }
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
        using var child = new Process { StartInfo = start };
        try
        {
            try
            {
                if (!child.Start()) throw new InvalidOperationException("Child process did not start.");
            }
            catch (Win32Exception error)
            {
                // OS text from the numeric code contains neither arguments nor the original
                // Process.Start message's potentially secret-bearing executable/working path.
                throw new InvalidOperationException($"Cannot launch Gannet courier '{Redact(executable)}': " +
                    $"OS error {error.NativeErrorCode}: {Redact(new Win32Exception(error.NativeErrorCode).Message)}");
            }
            Task<string> stdout = child.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> stderr = child.StandardError.ReadToEndAsync(cancellationToken);
            await child.WaitForExitAsync(cancellationToken);
            string output = await stdout;
            await stderr; // Never include untrusted child stderr in diagnostics.
            if (child.ExitCode != 0) throw new InvalidOperationException($"Gannet courier exited {child.ExitCode}.");
            return Redact(output);
        }
        finally
        {
            // Only this launcher's own child is stopped, including on cancellation/read failure.
            try { if (!child.HasExited) { child.Kill(entireProcessTree: true); await child.WaitForExitAsync(CancellationToken.None); } }
            catch (InvalidOperationException) { /* Start failed; no owned process exists. */ }
        }
    }
}
// endsnippet:launcher
