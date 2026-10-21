// snippet:telemetry
using System.Diagnostics;

// Usage: CliTelemetry <absolute-dotnet-path> [export]
if (args.Length is < 1 or > 2 || !Path.IsPathFullyQualified(args[0]) ||
    (args.Length == 2 && args[1] != "export"))
{
    throw new ArgumentException("Supply an absolute dotnet path and optionally 'export'.");
}

ProcessStartInfo command = new(args[0]);
command.ArgumentList.Add("--info");
command.UseShellExecute = false;
command.Environment["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0";
bool export = args.Length == 2;
command.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = export ? "0" : "1";
if (export)
{
    // An existing, explicitly chosen local collector must already be listening.
    command.Environment["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://127.0.0.1:4318";
    command.Environment["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf";
}
using Process process = Process.Start(command) ?? throw new InvalidOperationException("CLI launch failed.");
await process.WaitForExitAsync();
return process.ExitCode;
// endsnippet:telemetry
