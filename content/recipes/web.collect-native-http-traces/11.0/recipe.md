# Collect framework traces without duplicate enrichment

## Goal

A fictional lantern station exports a trace for each request so an operator can investigate failed readings. On .NET 11, collect the framework's HTTP information directly, preserve the station's own attributes, and update any exporter assumptions about route values and exception descriptions.

The activity source to subscribe to is **`Microsoft.AspNetCore`**. `Microsoft.AspNetCore.Hosting.HttpRequestIn` is the operation name, not the source registration. Subscribing to the wrong name can leave the request span unrecorded and affect sampling of application child spans.

## Older approach — use ASP.NET Core instrumentation

The .NET 10 application uses `OpenTelemetry.Instrumentation.AspNetCore` 1.20.1 to subscribe and enrich the framework request activity. Its custom processor adds only the fictional station identifier; its exporter receives that same activity, not a newly created duplicate request activity. This remains a valid pipeline for .NET 10.

The actual older requests show the raw conventional route template, `{controller=Station}/{action=Index}/{id?}`. The instrumentation already reports `Error` with no status description for the failed request, so do not claim every older instrumented application had an exception message there. An exporter that relies on the framework's older uninstrumented exception description needs migration even if another application's package already normalized it.

Save these complete files in an independent `LanternTraces` directory. The in-memory exporter is an actual OpenTelemetry `BaseExporter<Activity>` pipeline, requiring no remote account or collector.

### Directory.Build.props
{{snippet:n10-directory-build-props}}

### Directory.Build.targets
{{snippet:n10-directory-build-targets}}

### Directory.Packages.props
{{snippet:n10-directory-packages-props}}

### global.json
{{snippet:n10-global-json}}

### LanternTraces.csproj
{{snippet:n10-lanterntraces-csproj}}

### Program.cs
{{snippet:n10-program-cs}}

## Newer approach — collect one native span

The newer HTTP-only pipeline removes the instrumentation package and registers `AddSource("Microsoft.AspNetCore")` on one provider. Keep one enrichment/export path. The framework supplies HTTP method, response status, protocol, URI/server fields and the conventional route. Keep business facts in the station processor and endpoint tags; do not manufacture another server span or add the same framework tags again.

For the successful route, the measured native route is `Station/Index/{id?}`; for the error route it is `Station/Fault/{id?}`. The native failed activity has `ActivityStatusCode.Error`, a null `StatusDescription`, and `error.type=System.InvalidOperationException`. An exporter should read the supported tags and status rather than assuming a description contains diagnostic text.

**RC2 observation:** this fixture receives no native exception event. Current documentation describes one, but its presence was not established by these artifacts. If exception-event export is required, configure and verify that path explicitly, with appropriate handling of sensitive messages/stacks. Do not infer it from a null status description or silently claim it exists.

### Directory.Build.props
{{snippet:n11-directory-build-props}}

### Directory.Build.targets
{{snippet:n11-directory-build-targets}}

### Directory.Packages.props
{{snippet:n11-directory-packages-props}}

### global.json
{{snippet:n11-global-json}}

### LanternTraces.csproj
{{snippet:n11-lanterntraces-csproj}}

### Program.cs
{{snippet:n11-program-cs}}

## Migration

1. Inventory the application's tracing registrations, custom processors and exporters. Keep a single provider for the request path. Separate HTTP data already supplied by the framework from business attributes owned by the application.
2. For a .NET 11 HTTP-only pipeline, replace `AddAspNetCoreInstrumentation()` with `AddSource("Microsoft.AspNetCore")` and remove its package reference. Keep `OpenTelemetry` and the chosen processor/exporter dependencies. The supplied compatible verification pin is 1.20.1; restore generates the lock file, which should then be reviewed and retained.
3. Preserve other instrumentation the application actually uses. If the existing ASP.NET Core instrumentation registration also enables additional framework sources, metrics, filtering or enrichers, inventory those before removing it. Keeping a compatible instrumentation package can be appropriate; removing this example's HTTP-only dependency is not universal package-removal advice.
4. Update dashboards/exporters for resolved conventional routes and null exception descriptions. Keep route templates for aggregation; never replace `http.route` with raw user paths or identifiers. Do not duplicate the native HTTP attributes in the domain processor.
5. Save each complete file set separately, restore with its SDK, build Release and execute the built DLL using the absolute `dotnet` host. The newer executable starts a second fresh process for the suppression control; run the DLL through `dotnet`, as the evidence helper does.
6. Confirm two HTTP responses, two distinct exported server spans, the expected source and route/status attributes, and retained business tags. The fresh suppression process must continue exporting two spans but show absent native method/status/route tags, an `Unset` error activity and null description in RC2.
7. If temporarily enabling `Microsoft.AspNetCore.Hosting.SuppressActivityOpenTelemetryData`, do so before host creation. Treat it as a narrow switch, then rerun the actual exporter contract. It is not a full pre-11 rollback and does not recreate the old exception description or raw conventional-route tag. Reconcile these exact RC2 observations at GA.

## Constraints

- Applicability: ASP.NET Core 11 server requests. This edition does not expose a .NET 10 native-default route; the older example deliberately includes the compatible instrumentation dependency instead. C# 15 / SDK 11 RC2 and C# 14 / SDK 10.0.402 are verification pins.
- Managed dependencies: `OpenTelemetry` 1.20.1 plus its exact resolved API/provider-builder assets in the saved lock. The .NET 10 comparison additionally references `OpenTelemetry.Instrumentation.AspNetCore` 1.20.1. The framework source is not an extra NuGet package.
- Platform/rendering: actual Windows x64 loopback MVC execution; no browser, database, workload, secret or cloud provider. The exporter records real completed activities locally. It does not demonstrate remote collector delivery, retries, retention or backend pricing.
- Error metrics can gain an `error.type` dimension; review backend cardinality separately. No metrics/exporter-backend execution is claimed here. The suppression control proves continued activity creation and domain enrichment, not every switch interaction in every instrumentation package.
- Suppression in the measured RC2 also removes the error status and route tag. It cannot be relied on to restore older route formatting, exception descriptions or a complete former exporter contract. This observed qualification is retained alongside the current official description rather than relabelling RC proof as GA behavior.
- Stable API intent requires no experimental opt-in. Final .NET 11 GA source/runtime reconciliation remains outstanding under ADR 0004. Native error-event differences must be rechecked with final artifacts.

## Evidence

Both actual projects compile and run with their declared locked dependencies. Real successful and unhandled-error MVC requests flow through OpenTelemetry processors and a concrete exporter. Each run asserts exactly two distinct exported server spans, source identity, application enrichment, route format and relevant status/tags. The newer default run also launches a fresh-process switch control, avoiding an invalid attempt to reset static framework initialization in one process.

This is managed server/export execution, not mocked listener input, cross-compilation or a remote telemetry-service test. The older instrumentation already leaves exception descriptions empty; the newer native pipeline provides `error.type` without an exception event. Those measured conditions bound the migration advice.

The existing installed-package harness's `LanternTransport` scenario separately asserts complete canonical body expansion, discovery goal/title, full applicability and fresh records from actual offline stdio list/get. It preserves `web.collect-native-http-traces` / WEB-074 / the tracing part of WEB-085 as edition 11.0, without changing the separate channel-binding work.

Official references: [HTTP activity compatibility changes](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/http-activity-otel-semconv) and [native source collection](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview2/aspnetcore.md#native-opentelemetry-tracing-for-aspnet-core). The complete implementation and local checks are embedded above.
