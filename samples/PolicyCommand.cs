using System.Diagnostics;

internal static class PolicyCommand
{
    internal static string Run(string directory, string[] arguments, int expectedExit = 0, string? wave = null)
    {
        string? processPath = Environment.GetEnvironmentVariable("PATH");
        string? userPath = OperatingSystem.IsWindows() ? Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) : null;
        string? machinePath = OperatingSystem.IsWindows() ? Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) : null;
        string dotnet = Environment.ProcessPath!;
        ProcessStartInfo start = new(dotnet) { WorkingDirectory = directory, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (string argument in arguments) { start.ArgumentList.Add(argument); }
        start.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
        start.Environment["DOTNET_CLI_HOME"] = Path.Combine(directory, "home");
        start.Environment["NUGET_PACKAGES"] = Path.Combine(directory, "packages");
        start.Environment["NUGET_HTTP_CACHE_PATH"] = Path.Combine(directory, "http-cache");
        start.Environment["DOTNET_NOLOGO"] = "1";
        start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        start.Environment["DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE"] = "true";
        start.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "false";
        start.Environment["MSBUILDUSESERVER"] = "0";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        start.Environment.Remove("MSBUILDDISABLEFEATURESFROMVERSION");
        if (wave is not null) { start.Environment["MSBUILDDISABLEFEATURESFROMVERSION"] = wave; }
        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(90000)) { process.Kill(true); throw new TimeoutException(string.Join(' ', arguments)); }
        string output = stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult();
        if (processPath != Environment.GetEnvironmentVariable("PATH") ||
            (OperatingSystem.IsWindows() && (userPath != Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) || machinePath != Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine))))
        {
            throw new InvalidOperationException("Unexpected PATH change; inspect shared settings before changing them.");
        }
        Console.WriteLine($"{dotnet} {string.Join(' ', arguments)}\nexit={process.ExitCode}\n{output}");
        if (process.ExitCode != expectedExit) { throw new InvalidOperationException($"Expected exit {expectedExit}, actual {process.ExitCode}"); }
        return output;
    }

    internal static void Contains(string actual, string expected)
    {
        if (!actual.Contains(expected, StringComparison.OrdinalIgnoreCase)) { throw new InvalidOperationException($"Missing {expected}: {actual}"); }
    }
}
