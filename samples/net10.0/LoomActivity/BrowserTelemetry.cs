// snippet:telemetry
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LoomActivity;

public sealed class BrowserTelemetry : IDisposable
{
    public static readonly ActivitySource Work = new("LoomActivity.Work");
    private readonly MeterProvider _metrics;
    private readonly TracerProvider _traces;

    public BrowserTelemetry(string edition)
    {
        var resource = ResourceBuilder.CreateDefault()
            .AddService("LoomActivity", serviceVersion: edition,
                serviceInstanceId: Guid.NewGuid().ToString("N"));
        // Build and retain providers before the renderer starts emitting signals.
        _metrics = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resource)
            .AddMeter("Microsoft.AspNetCore.Components", "Microsoft.AspNetCore.Components.Lifecycle")
            .AddOtlpExporter((exporter, reader) =>
            {
                exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                exporter.Endpoint = new Uri("http://localhost:5933/v1/metrics");
                reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
                reader.PeriodicExportingMetricReaderOptions.ExportTimeoutMilliseconds = 5000;
            }).Build();
        _traces = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resource)
            .SetSampler(new AlwaysOnSampler())
            .AddSource("Microsoft.AspNetCore.Components", "LoomActivity.Work")
            .AddOtlpExporter(exporter =>
            {
                exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                exporter.Endpoint = new Uri("http://localhost:5933/v1/traces");
                exporter.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 500;
            }).Build();
    }

    public void Dispose()
    {
        _traces.Dispose();
        _metrics.Dispose();
    }
}
// endsnippet:telemetry
