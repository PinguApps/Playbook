# Collect browser component activity

## Goal

Find which component work accompanies a slow browser interaction. Enable runtime diagnostics, subscribe to the framework's component meters and activity source, then export both signals to an observable collector. The fictional Loom shuttle board keeps the interaction small enough to follow from a button click through asynchronous business work and a route change.

WASM only here. These are stable diagnostic APIs in a prerelease framework: the actual newer baseline is .NET 11 RC2, not GA. There is no Experimental diagnostic on the APIs used. Timing collection adds work; the sample is not a benchmark or CPU profiler.

## Older approach

.NET 10 already provides public component metrics and tracing registration. Call the static methods on ComponentsMetricsServiceCollectionExtensions in Microsoft.AspNetCore.Components.Infrastructure; their IServiceCollection parameter is not an extension-method receiver. Explicitly preserve runtime metrics, EventSource and HTTP activity propagation through the public MSBuild properties. This is a valid supported approach, not a missing historical API or a hand-written substitute for framework instrumentation.

Create an empty net10 application directory. Save LoomActivity.csproj and Directory.Packages.props below. These complete settings work without this repository's parent build/package files. Save global.json containing exactly `{"sdk":{"version":"10.0.402","rollForward":"disable","allowPrerelease":false}}`. The actual browser runtime/packages are 10.0.13 and the language mode is C# 14.

{{snippet:project-10}}

{{snippet:packages-10}}

Save Program.cs. Register framework instrumentation, then construct the providers before the renderer starts. Registration creates instruments/activity sources; it does not choose an exporter or prove delivery.

{{snippet:program-10}}

## Newer approach

Create a separate net11 application directory. Save global.json containing exactly `{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable","allowPrerelease":true}}`. Save these project/package files. The actual framework/runtime/browser assets are 11.0.0-rc.2.26504.105; canonical language mode is C# 15, without requiring new language syntax.

{{snippet:project-11}}

{{snippet:packages-11}}

BlazorWebAssemblyDiagnosticsEnabled defaults to true in this SDK and is explicit here. It sets MetricsSupport, EventSourceSupport and HttpActivityPropagationSupport to true only when each was unset. An explicit false on an individual property still wins. In the recorded evaluated projects and generated dotnet.js runtimeConfig, Meter.IsSupported, EventSource.IsSupported and EnableActivityPropagation are all true. Inspect your own effective build; setting a property after publishing cannot recover trimmed code.

The .NET 11 host registers component metrics and tracing when metrics support and its enablement switch permit them. Program.cs therefore needs no duplicate manual registration. Keeping the .NET 10 public calls is also valid when explicit registration suits a shared startup path.

{{snippet:program-11}}

## Complete shared application

Save the following identical files in both directories. BrowserTelemetry.cs uses OpenTelemetry.Exporter.OpenTelemetryProtocol **1.20.1**, whose actual restored compile/runtime asset is lib/net10.0 in both applications. Its OpenTelemetry SDK supports task-based browser batching/periodic readers; an arbitrary older exporter version is not an equivalent prerequisite. This example builds providers directly, rather than relying on a hosting extension to start them. There is no external persistence provider or service account.

{{snippet:telemetry}}

Use OTLP HTTP/protobuf, not raw gRPC from a browser. Supply the complete signal paths: /v1/metrics and /v1/traces. AddMeter selects both Microsoft.AspNetCore.Components and Microsoft.AspNetCore.Components.Lifecycle; AddSource selects Microsoft.AspNetCore.Components. LoomActivity.Work is an additional business activity source used only to show correlation. AlwaysOnSampler is deliberate for this tiny local demonstration; choose production sampling and cardinality limits separately.

The providers must remain alive. Here using owns them across the host's RunAsync lifetime. Metrics use a one-second reader and traces a 500 ms batch delay. Keep the page open until the collector has received both. A synchronous ForceFlush return is not an awaited network receipt in single-threaded WASM. This sample makes no final-upload promise on close, reload, suspension or termination; browser unload cannot reliably await arbitrary cleanup. Dispose releases providers when the owned scope actually ends, but is not a durability mechanism.

Save App.razor. Every Razor file declares its own namespace/imports, so no hidden _Imports.razor is needed.

{{snippet:app}}

Save Board.razor. The real event handler awaits a short fictional operation. Its custom child span inherits the framework event's trace, while the framework event links back to navigation rather than making navigation its parent.

{{snippet:board}}

Save ShuttleBadge.razor. Its changed parameter exercises lifecycle measurements as the count rerenders.

{{snippet:badge}}

Create wwwroot/index.html, including all CSS and the matching framework startup script. No extra JavaScript module or initializer is required.

{{snippet:document}}

## Local collection and execution

Save collector.yaml outside both applications. This is the official OpenTelemetry Collector Contrib, not a fake HTTP receiver. Its debug exporter displays decoded telemetry and its file exporter writes received OTLP JSON. The receiver's trace/metric signals are stable; the file exporter is alpha and its field format can change. Use it for these pinned local diagnostic receipts, not a promised durable storage format. Bind only the published loopback port; the container's internal receiver listens on its own interface. The exact tested image is version 0.138.0 at the digest below. Docker is an authoring prerequisite for this local route, not a tool installation requirement.

{{snippet:collector}}

From the directory containing collector.yaml, use PowerShell and an absolute Docker executable. Check that ports 5931, 5932 and 5933 are free first; do not stop an unrelated listener. The command creates only a disposable local collector and receipts directory. On another machine choose the actual absolute Docker path. The image can require an initial registry download; no remote resource is provisioned.

Create receipts with `New-Item -ItemType Directory -Force receipts`. Start the collector with `& 'C:\Program Files\Docker\Docker\resources\bin\docker.exe' run -d --name loom-collector -p 127.0.0.1:5933:4318 --mount "type=bind,source=$((Resolve-Path collector.yaml).Path),target=/etc/otelcol-contrib/config.yaml,readonly" --mount "type=bind,source=$((Resolve-Path receipts).Path),target=/receipts" otel/opentelemetry-collector-contrib@sha256:4a951ff0a9772f4fa8e9c7a326434370301b98b2b813933468dcb54aa6a6b5e5`. Inspect it with `docker logs loom-collector`.

In each application directory, use its matching SDK. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in the child environment. Run `dotnet restore LoomActivity.csproj`, then `dotnet build LoomActivity.csproj -c Release --no-restore`, then `dotnet run --project LoomActivity.csproj -c Release --no-build --no-restore --no-launch-profile --urls http://localhost:5931` for net10 or port 5932 for net11. Use `C:\Program Files\dotnet\dotnet.exe` for .NET 10 and `C:\Users\pingu\.dotnet-playbook-11\dotnet.exe` for the recorded .NET 11 installation. Each server occupies a terminal. Restore obtains the matching SDK/runtime WASM packs; interpreted execution with WasmBuildNative=false needs no native/AOT workload. Repository lock files retain the exact dependency closure; standalone restore creates its own lock. The pinned RC2 DevServer emits its Gateway migration warning; this development server is not a production hosting recommendation.

Open http://localhost:5931 and http://localhost:5932 in the real browser. Click **Schedule shuttle** once and expect **Scheduled: 1** and **Shuttles ready: 1**. Then select **Open dispatch route** and expect /dispatch with the count retained. Leave both pages alive for at least two export cycles, then inspect `docker logs loom-collector` and receipts/otlp.json. A healthy UI alone does not prove telemetry delivery.

Confirm service.name LoomActivity and the distinct service.version 10.0/11.0 and service.instance.id. In decoded collector output require:

- Microsoft.AspNetCore.Components: positive aspnetcore.components.navigate and aspnetcore.components.handle_event.duration measurements; the event tags identify LoomActivity.Board and ScheduleShuttle.
- Microsoft.AspNetCore.Components.Lifecycle: positive aspnetcore.components.update_parameters.duration, aspnetcore.components.render_diff.duration and aspnetcore.components.render_diff.size measurements.
- Framework route spans for / and /dispatch, and Event onclick -> LoomActivity.Board.ScheduleShuttle. The loom.schedule span must share the event traceId and have the event spanId as parentSpanId. The event's link must identify the received / route span.

Navigation is a counter. Duration histograms use seconds; render-diff size counts modified elements. These measures describe framework event handling, parameter work and diff generation, not browser paint/layout, network time, CPU samples or one universal end-to-end duration. Metrics aggregate by their tags and do not automatically become trace children. Do not infer delivery from AddMeter, an enabled instrument, a local listener, or an exporter configured in DI.

The supplied CORS permits only the two local sample origins and Content-Type. A changed scheme/host/port is a different origin and needs a reviewed collector configuration. Browser configuration is public: never embed an API key. Plain HTTP is for loopback development only. A real deployment needs trusted HTTPS, appropriate access control/rate limits, privacy review, safe tag cardinality and a collector endpoint designed for browser traffic. Neither disabled certificate validation nor wildcard credentialed CORS is a fix.

Stop each server with Ctrl+C. Stop and remove only your collector with `docker stop loom-collector` followed by `docker rm loom-collector`; retain receipts for inspection. Periodic/batch export may lose work if the page or collector ends before delivery. This is observable local collection, not guaranteed telemetry persistence.

## Migration

1. Preserve .NET 10's valid explicit registration when retaining that target. This recipe's .NET 10 code is a comparison, not a new .NET 10 foundation or a route to the .NET 11 edition.
2. For .NET 11, enable the public diagnostics setting, remove redundant registration if desired, and verify effective build/runtime switches.
3. Keep explicit subscriptions, exporter package/assets, full endpoint paths and long-lived providers. Test the actual browser's CORS and collector receipts.
4. Use shared `tracing.configure-source-rules/11.0` for source/listener selection and local/global scope; retrieve it with get_recipe targetVersion 11.0. AddTracing alone is not an exporter and does not replace these OpenTelemetry subscriptions/export settings. Its identity and existing guidance remain separate from this WASM collection recipe.

## Constraints

This route is browser WebAssembly. Preserve public runtime switches, package assets, CORS and exporter/provider lifetimes together. The measured single Windows Chrome path does not establish other browsers, operating systems, rendering modes, AOT or production performance. Keep browser data public and review privacy, sampling and metric cardinality before deployment.

## Evidence

Both canonical applications compile with their matching SDKs. Separate actual Windows Chrome WASM execution through OpenTelemetry 1.20.1 and the official loopback collector receives the five framework instruments and correlated spans above. Compilation evidence is labelled compile, not compile-and-run: an installed tool does not launch these applications. Source review, evaluated/boot settings, compilation, subscription and collector receipt are distinct evidence. Exact artifact/input/payload fingerprints and independent reconstruction from installed Markdown are recorded in docs/verification/PIN-797.md. No mocks, source-only execution substitute, cross-compilation, Server/SSR/Auto, AOT, other-browser/OS, production performance or GA guarantee is asserted.
