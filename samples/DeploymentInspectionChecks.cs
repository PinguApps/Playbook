using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Playbook.Samples;

CallerReport report = ReadCaller();
string? location = DeploymentReport.Location();
bool native = !RuntimeFeature.IsDynamicCodeSupported;
bool stackTrace = !AppContext.TryGetSwitch("System.Diagnostics.StackTrace.IsSupported", out bool enabled) || enabled;
#if NET11_0_OR_GREATER
bool expectCaller = stackTrace;
#else
bool expectCaller = !native;
#endif
string? expectedName = typeof(DeploymentReport).Assembly.GetName().Name;
if (expectCaller ? report.AssemblyName != expectedName || report.UnavailableReason is not null
    : report.AssemblyName is not null || report.UnavailableReason is null)
{
    throw new InvalidOperationException($"Caller mismatch: {report}.");
}

if (args.Contains("--no-location") && location is not null) { throw new InvalidOperationException("Expected unavailable assembly location."); }
if (args.Contains("--physical-location") && (location is null || !File.Exists(location))) { throw new InvalidOperationException("Expected existing assembly file."); }
Console.WriteLine($"PASS: NativeAOT={native}; StackTraceSupport={stackTrace}; caller={report.AssemblyName ?? report.UnavailableReason}; location={location ?? "(unavailable)"}; {RuntimeInformation.FrameworkDescription}.");

[MethodImpl(MethodImplOptions.NoInlining)]
static CallerReport ReadCaller() => DeploymentReport.Caller();
