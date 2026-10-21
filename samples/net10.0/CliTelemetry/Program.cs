// snippet:telemetry
using System.Diagnostics;

// Usage: CliTelemetry <absolute-dotnet-path>
if (args.Length != 1 || !Path.IsPathFullyQualified(args[0]))
{
    throw new ArgumentException("Supply the absolute path of the selected SDK's dotnet executable.");
}

ProcessStartInfo command = new(args[0]);
command.ArgumentList.Add("--info");
command.UseShellExecute = false;
command.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
command.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
using Process process = Process.Start(command) ?? throw new InvalidOperationException("CLI launch failed.");
await process.WaitForExitAsync();
return process.ExitCode;
// endsnippet:telemetry
