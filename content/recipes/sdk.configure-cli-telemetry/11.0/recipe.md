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
3. Use the canonical wrapper without `export` for the opt-out route. Supply the absolute `dotnet` path for the intended SDK, and launch from the sample directory containing the matching `global.json`. Both opt-out wrappers and the GA export branch have run against a local collector; see Evidence for the bounded result.
4. If export is deliberately required, arrange an existing compatible OTLP collector first, inspect inherited `OTEL_EXPORTER_OTLP_*` settings, and adapt the newer child's endpoint/protocol to the chosen destination. The `export` argument is an explicit collection decision, not a privacy-preserving reroute. Configure any needed authentication in the environment's secret store; never embed headers/tokens in a recipe or source file.
5. Keep the settings at process or CI-job scope unless a persistent machine-wide policy is separately intended. The sample writes only `ProcessStartInfo.Environment`; it does not modify PATH, shell profiles, user variables or machine variables. `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` is a separate first-run guard, not a telemetry switch.
6. Run `eng/Test-CliTelemetry.ps1` to exercise the actual wrappers against a temporary loopback HTTP/protobuf endpoint. It checks paired opt-out controls and GA export receipt. Successful `--info` output alone is not the check; the collector must receive SDK traces and metrics.

## Constraints

CLI telemetry covers SDK commands such as build, pack and run. Starting an already-built app with `dotnet path-to-app.dll` is runtime execution and does not itself collect SDK command telemetry. Application OpenTelemetry, third-party tool telemetry, IDE policies and services started by an application need their own configuration.

`DOTNET_NOLOGO=true` hides the SDK welcome/telemetry notice; it does not opt out. `OTEL_SDK_DISABLED` concerns OpenTelemetry SDK behavior and is not a replacement for the documented .NET CLI opt-out. Distribution/build defaults may differ from Microsoft's SDK; inspect the actual distribution rather than assuming all source builds use the same telemetry configuration.

OTLP generic endpoints are base addresses; transport and per-signal overrides affect the final request paths. Collector receipt, contents, batching, flushing and failure behavior cannot be inferred from environment values alone. The sample deliberately does not make a delivery guarantee or claim `--info` produces a particular span. No additional NuGet package, provider, rendering mode or workload is required by the wrapper.

## Evidence and limitations

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The canonical .NET10 and .NET11 projects now have actual locked compilation records from Windows, using their declared SDKs and dependencies. The original official-source review is retained separately. Compilation alone does not exercise the export branch or measure telemetry configuration/network behavior.

The existing-format execution receipts record the actual local collector observations; historical RC/source-review and compilation receipts remain separate. This bounded qualification resolves this edition's preview exception, not the global stable gate.

Windows SDK10.0.403 and SDK11.0.100 opt-out wrappers returned successfully with no local collector requests. GA export delivered dotnet-cli traces and metrics over HTTP/protobuf to a temporary loopback collector. Microsoft trace export was disabled only in the verification process. No global network absence, Azure Monitor behavior, retry or production delivery guarantee is established.
