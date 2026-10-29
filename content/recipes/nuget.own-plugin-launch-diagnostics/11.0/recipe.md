# Remove dependency on an obsolete credential-plugin diagnostic helper

## Goal

Gannet Harbour owns a custom credential-plugin launcher. When the executable
cannot start, report the configured path and OS error using the launcher's own
redaction policy. Stop calling a helper NuGet no longer uses; no public
replacement helper is introduced.

## Requirements and exact comparison

The real comparison uses Windows x64 SDK10.0.402/runtime+ref10.0.13/C#14 and
SDK11.0.100-rc.2.26504.105/runtime+ref11.0.0-rc.2.26504.105/C#15. These pins are
tested artifacts, not a universal release-family compatibility promise. No
provider installation, feed, account, real credential, native compiler, workload,
device, UI/rendering mode or preview-language opt-in is required. Current SDK
stability is RC2/preview, not actual GA.

The old helper's actual metadata reference is the complete SDK10 NuGet.Credentials
7.9.0-rc.611 net8.0 closure (source a8dd0577), compared with SDK11's
8.0.0-rc.605 net10.0 closure (source820230c3). DLL TFMs differ from the executable
net10/net11 targets. Both compiler/reference/native-host and SDK assembly
products/hashes are pinned separately in `samples/net11.0/GannetWorkshop/Shared/artifact-pins.json`.
No independent NuGet package8 is inferred from those SDK files. The fixed
launcher needs only framework APIs and removes the credential-helper reference;
the isolated expected-warning fixture retains the actual SDK reference to
demonstrate the compiler change. No mixed-ABI, reflection or copied NuGet
implementation is needed.

## Older approach

{{snippet:obsolete-call}}

{{snippet:warning-project}}

Compile with `HarbourTarget=net10.0` under SDK10 or `net11.0` under SDK11.
The old call compiles without this obsolete warning on SDK10. SDK11 emits real
**CS0618**: "This method is unused and will be removed in a future release."
`CreatePathNotFoundMessage(string path, string attempted)` returns a
`PluginException`; its name does not make it a string formatter. The probe
explicitly allows that expected warning without suppressing it. It is separate
from the zero-warning fixed production consumer; do not add `NoWarn`, a pragma,
or an obsolete-warning exception to production merely to retain the call.

## Newer approach

{{snippet:launcher}}

## Constraints

Supply a fully qualified executable path and structured arguments separately.
Pass known sensitive values to the redactor using secure caller inputs; this
example never prints arguments or the environment. The execution path is private
by default: its error uses `<configured courier>`. For an actionable path, supply
the optional `diagnosticPath` as a caller-owned safe alias such as
`<plugin-root>/courier.exe`; do not forward a raw path containing unlisted secrets.
The alias identifies the private configuration location without disclosing its
directory. Redaction is exact-value
replacement, longest first, with CR/LF neutralized. A production policy must
identify every sensitive value and allowed path detail; it cannot discover
arbitrary unknown secrets. The fixture includes a synthetic marker in both
the executable path, diagnostic alias and an argument, and asserts that none
leaks. A second real failure with an empty secret list verifies that the default
message still never echoes the private executable path or arguments.

`Process.Start` is the authority: a prior `File.Exists` check would race and
would not prove OS launch failure. The catch formats `Win32Exception.NativeErrorCode`
and the OS message constructed from that numeric code, rather than copying the
original exception's secret-bearing command/path text. It exposes the useful
safe diagnostic path and OS failure without preserving an unsafe inner
exception. It does not invent a NuGet replacement helper.

On success, the method returns redacted stdout and preserves exit0. A nonzero
exit produces a bounded code-only diagnostic; stderr is consumed but not echoed.
Cancellation or read failure terminates and waits for only its owned child.
The caller supplies an appropriate deadline and output-volume policy: this
bounded benign fixture uses a 20-second token and tiny output; unbounded or
hostile child output needs streaming limits. Nothing here implements the full
NuGet credential-plugin protocol or validates an exchange with a feed.
The token bounds normal work, not OS termination: after a successful kill the
owned child is awaited without cancellation so cleanup completes. A failed kill
can surface its OS error instead of the work failure. This bounded benign proof
does not establish reliable termination of arbitrary hostile/stuck processes.

The compiled fixture's valid child is the same original executable's
`--courier-child` mode, printing `Gannet courier ready` and exiting0. It launches
through the absolute matching dotnet executable with the DLL as a structured
argument. Its missing case instead gives `Process.Start.FileName` an actual
nonexistent `.exe`; a missing DLL handed to dotnet would be a different failure.

Both actual consumers use this shared bootstrap; it checks the benign-child
mode before loading SDK assemblies. The default `Run` executes the complete
workshop below. The evidence build additionally sets `GannetProof=true` for
separate proof fixtures, which the default build does not require. To adapt the
launcher, supply your authorized executable/arguments/deadline:

Launch the built DLL through the matching absolute dotnet executable so
Environment.ProcessPath identifies its complete SDK installation.

{{snippet:bootstrap}}

The original workshop demo includes two real harbour builds as well as the
benign courier launch. Its complete companion host is included for a runnable
default project; `CourierLauncher` can be used on its own without MSBuild:

{{snippet:demo}}

{{snippet:cache}}

The actual demo task project uses the host-supplied absolute HarbourAssembly property:

{{snippet:build-project}}

The sample boundary and paired project settings are:

{{snippet:boundary}}

{{snippet:old-settings}}

{{snippet:settings}}

Use an exact `global.json` SDK pin with `rollForward=disable` and explicit
`allowPrerelease=true` for11, and clear remote sources in `NuGet.Config`. These
fixed executables restore no packages; shared source includes are canonical
repository inputs. The MSBuild/NuGet.Common references support the companion
cache proof only; `CourierLauncher` itself needs neither.

## Migration

1. Find direct calls to the helper in your custom launcher. Preserve the actual
   old SDK/dependency set as the comparison, rather than guessing availability
   from a package version or API name.
2. Compile the old call with the current compiler and retain the visible CS0618
   warning in a separate expected-warning probe.
3. Own an actionable missing-executable message around real `Process.Start`.
   Use an absolute executable, `UseShellExecute=false`, structured arguments and
   `CreateNoWindow=true`; do not search PATH or concatenate a shell command.
4. Redact configured sensitive values and omit secret-bearing arguments,
   environment and raw exception chains. Scope child environment/cleanup to the
   owned launch; never install a provider or change persistent credentials.
5. Rebuild the fixed consumer with warnings as errors, then verify an actual
   missing-executable OS error and a valid benign child's output/exit behaviour.
   Qualify actual provider protocol/authentication separately before claiming it.

## Evidence and limits

`eng/Record-PackageWorkshop.ps1` captures the paired consumers through the
existing sample-evidence workflow. Both fixed projects compile with zero
warnings. The isolated old-call probe has no CS0618 on SDK10 and the exact
visible obsolete warning on SDK11. Real missing-child launches return Windows
OS error2 with the useful `missing-courier.exe` path and synthetic secret
redacted. Real benign child launches retain `Gannet courier ready` and exit0.
Owned processes are waited/disposed, including failures; no real credential or
feed is used. Fresh canonical input hashes and compact redacted results remain
in sample evidence; raw commands/diagnostics are ignored artifacts.

Real stdio list/get against the exact ordinary installed Playbook artifact
checks discovery ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and fresh paired evidence from an unrelated
directory. Target10 has no selectable edition, while its actual valid older
comparison remains here. Retrieval launches no child. Proxy independence is not
network isolation. No general cross-OS diagnostic text, full provider protocol,
credential exchange or GA claim is made. The eight approved .NET10 foundations,
PIN-959 qualifying coverage/evidence, PIN-963 actual-GA and unrelated Human gates
remain open.

## Official reference

- [Actual obsolete declaration and constructors](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/nuget-client/src/NuGet.Core/NuGet.Credentials/PluginException.cs).
- [Actual older helper](https://github.com/dotnet/dotnet/blob/a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e/src/nuget-client/src/NuGet.Core/NuGet.Credentials/PluginException.cs).
