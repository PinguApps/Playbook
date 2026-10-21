# Choose CLI telemetry collection and export explicitly

## Goal

Make a deliberate choice about SDK usage collection and optional export for a command process. Keep that choice separate from application diagnostics, from suppressing the welcome banner, and from the environment of unrelated builds.

This guidance is theoretical: no collector, telemetry transport, child command or comparison build has been exercised.

## Older approach

Microsoft-distributed .NET SDKs collect CLI usage telemetry by default. `DOTNET_CLI_TELEMETRY_OPTOUT=1` or `true` is the documented opt-out on .NET 10 as well as .NET 11. The original older wrapper passes that choice only to its child process:

{{snippet:older-project}}

{{snippet:older}}

The wrapper accepts an absolute SDK executable path. Its ordinary managed runtime invocation is separate from the child SDK invocation. `UseShellExecute=false` allows the child environment to be set without changing user or machine settings.

There is no asserted SDK-10 equivalent of the newer CLI OTLP exporter configuration here. A pipeline may already collect application or MSBuild diagnostics by other supported means; those are distinct mechanisms, not substitutes for this SDK telemetry contract.

## Newer approach

SDK 11 moves its CLI telemetry implementation to OpenTelemetry with Azure Monitor and OTLP exporter support. The familiar opt-out remains. The new original wrapper defaults to opt-out and enables collection/export only when its caller supplies `export`:

{{snippet:newer-project}}

{{snippet:newer}}

The documented OpenTelemetry settings choose an existing local collector at `http://127.0.0.1:4318` and HTTP/protobuf transport. This is not a collector implementation or startup command. The SDK source recognizes configured standard OTLP exporter environment variables; setting an endpoint can therefore enable export without an additional custom SDK switch. Check existing signal-specific endpoint/protocol/header settings too: inherited OpenTelemetry configuration may affect the result.

Choosing OTLP does **not** establish that Microsoft's ordinary telemetry destination is disabled or redirected. The example's `export` branch enables collection; it is unsuitable when policy requires no CLI telemetry. For that policy, retain `DOTNET_CLI_TELEMETRY_OPTOUT=1` instead.

## Migration

1. Decide separately whether to allow CLI usage collection, whether to export it to a collector, and whether application diagnostics should exist. Record which process tree the choice covers.
2. Set `DOTNET_CLI_TELEMETRY_OPTOUT=1` or `true` **before starting** each opted-out command or its launcher. A parent shell/CI-job environment applies to subsequently launched children; it does not rewrite the environment of an already-running process. Set it before an SDK installer too if installer telemetry is in scope.
3. Use the canonical wrapper without `export` for the opt-out route. Supply the absolute `dotnet` path for the intended SDK, and launch from the sample directory containing the matching `global.json`. The wrapper and its child command are unrun here.
4. If export is deliberately required, arrange an existing compatible OTLP collector first, inspect inherited `OTEL_EXPORTER_OTLP_*` settings, and adapt the newer child's endpoint/protocol to the chosen destination. The `export` argument is an explicit collection decision, not a privacy-preserving reroute. Configure any needed authentication in the environment's secret store; never embed headers/tokens in a recipe or source file.
5. Keep the settings at process or CI-job scope unless a persistent machine-wide policy is separately intended. The sample writes only `ProcessStartInfo.Environment`; it does not modify PATH, shell profiles, user variables or machine variables. `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` is a separate first-run guard, not a telemetry switch.
6. A future behavior check would launch representative SDK commands with collection disabled/enabled and observe an isolated collector/network boundary. A successful `--info`, quiet output or absence of a welcome message is not proof of absent or delivered telemetry. No such observation has been made.

## Constraints

CLI telemetry covers SDK commands such as build, pack and run. Starting an already-built app with `dotnet path-to-app.dll` is runtime execution and does not itself collect SDK command telemetry. Application OpenTelemetry, third-party tool telemetry, IDE policies and services started by an application need their own configuration.

`DOTNET_NOLOGO=true` hides the SDK welcome/telemetry notice; it does not opt out. `OTEL_SDK_DISABLED` concerns OpenTelemetry SDK behavior and is not a replacement for the documented .NET CLI opt-out. Distribution/build defaults may differ from Microsoft's SDK; inspect the actual distribution rather than assuming all source builds use the same telemetry configuration.

OTLP generic endpoints are base addresses; transport and per-signal overrides affect the final request paths. Collector receipt, contents, batching, flushing and failure behavior cannot be inferred from environment values alone. The sample deliberately does not make a delivery guarantee or claim `--info` produces a particular span. No additional NuGet package, provider, rendering mode or workload is required by the wrapper.

## Evidence

Reviewed the official CLI scope/default/opt-out documentation, .NET 11 SDK overview, CLI exporter activation source and OpenTelemetry exporter contract. The older/newer wrappers are independently authored canonical C# sources, intended for SDK 10.0.402/C# 14 and SDK 11.0.100-rc.2.26504.105/C# 15.

The saved records are source reviews, not compilations or measured telemetry evidence. No environment policy was applied to a live consumer, no data was sent and no collector/account was provisioned. Compilation, opt-out precedence, actual affected-command behavior, transport delivery and installed offline MCP retrieval remain unverified. Pending proof blocks release-ready packaging and is visible in discovery/detail. This target-11 edition does not add an unverified older-target route; RC documentation/source review is not GA verification.
