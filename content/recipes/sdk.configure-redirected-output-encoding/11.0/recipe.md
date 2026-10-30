# Read redirected SDK output with an explicit encoding

## Goal

Estuary's build monitor needs to read non-ASCII berth diagnostics without corrupting
them. Capture the SDK's pipe bytes, decode using an explicit UTF-8 policy, and
retain the build's exit code. Do not assume a console code page describes a pipe
reader, or that the SDK can discover that reader's encoding.

## Requirements and measured artifacts

This target11 edition retains a valid net10 comparison inside its body. It adds
no .NET10 foundation or older-target SDK11 route. The reader is an existing valid
approach on both measured SDKs; no newer-only reader API is claimed.

| Layer | Older comparison | Current example |
| --- | --- | --- |
| SDK | 10.0.402 | 11.0.100-rc.2.26504.105 |
| Compiler / language | 5.9.0-1.26505.111 / C#14 | 5.12.0-1.26504.105 / C#15 |
| Target / reference and runtime | net10.0 / 10.0.13 | net11.0 / 11.0.0-rc.2.26504.105 |
| Reader | PowerShell7; .NET ProcessStartInfo and explicit decoder | Same existing reader APIs |

The executed platform is Windows x64, with private CP1252 and CP65001 consoles,
redirected stdout/stderr and en-US SDK UI language. Complete SDK/compiler/ref/runtime
identities and hashes are recorded separately from prerequisites. No NuGet
dependency, workload, provider, native compiler or rendering mode is required for
this recipe. RC2 evidence is not GA qualification, and a new SDK or different
console/language condition requires a fresh byte-level check.

## Older approach

Save this original project and program under `samples/net10.0/EstuaryEncoding`.
Use `global.json` with SDK10.0.402 and `rollForward: "disable"`. Restore once before
calling the reader, which deliberately builds with `--no-restore`.

{{snippet:older}}

{{snippet:program}}

The target produces the actual non-ASCII warning `ESTUARY001: Estuary café: berth
fermé`. `EstuaryReject=true` produces error ESTUARY002 and a nonzero build exit.
The script disables warnings-as-errors only for this deliberate diagnostic
fixture; this is not advice to relax an application's build policy.

## Newer approach

Save this project under `samples/net11.0/EstuaryEncoding`, retaining its relative
link to the older program. Pin that directory's SDK to
11.0.100-rc.2.26504.105 with roll-forward disabled.

{{snippet:newer}}

Save this shared reader as `Read-BuildOutput.ps1`. Its executable and project
directory arguments are resolved to absolute paths; arguments are passed as
separate values, without constructing a shell command. Both pipes are drained
concurrently as bytes before decoding, and the returned object preserves the exit
code, decoded text and original bytes. Invalid UTF-8 throws rather than silently
replacing bytes. Never treat a successful decode as a successful build.

{{snippet:reader}}

**Measured opt-out limitation:** `DOTNET_CLI_FORCE_UTF8_ENCODING=false` did not
select a legacy pipe encoding on these actual SDKs. The
default, false and true all emitted UTF-8 for the tested diagnostics, even when
the inherited private console was CP1252. The source-level
`DOTNET_CLI_CONSOLE_USE_DEFAULT_ENCODING=1` control also retained UTF-8. Neither
switch is taught here as a working opt-out for this path. The original control
parameters remain diagnostic probes, not supported legacy-encoding routes.

The reliable measured route is an explicit UTF-8 reader. Its `ForceUtf8=true`
child setting records the requested producer policy; the observed bytes, not the
name of a flag, establish the encoding. Changing `ReaderCodePage` to `1252` does not
request a producer encoding and demonstrably corrupts the UTF-8 diagnostic.

## Migration

1. Pin the actual SDK and restore the original project. Start with an explicit
   reader policy instead of a locale-derived default. Use the absolute SDK
   executable and project directory with the supplied reader.
2. Call the reader with `-ReaderCodePage 65001` and the default `-ForceUtf8 true`.
   Require exit code 0 and the exact non-ASCII warning in Stdout. Inspect Stderr
   separately; SDK diagnostics are not guaranteed to use the same stream in every tool.
3. Retain raw bytes when diagnosing an encoding failure. For this fixture,
   `café` contains UTF-8 bytes `63 61 66 c3 a9`, not the CP1252 sequence
   `63 61 66 e9`. Do not infer a pipe's encoding from its destination's console.
4. Run `-Reject`, assert a nonzero ExitCode and ESTUARY002, and preserve that failure
   when integrating the result with automation. Decode failure and compiler failure
   are separate outcomes.
5. Compare default/false/true controls and `-ProbeSourceOptOut` only to qualify
   actual artifacts. All measured routes here still need the UTF-8 reader. Do not
   advertise an opt-out because a release note or source branch describes one.
6. Keep settings on ProcessStartInfo.Environment, as shown. Do not use persistent
   environment setters, shell-profile edits or `chcp` on the user's console.
   The authoring check allocates, hides and disposes its own console instead.
7. If a required consumer accepts only a legacy encoding, either explicitly
   transcode verified decoded text at that boundary or obtain a separately
   demonstrated producer route. No such producer opt-out is qualified by this
   recipe. Refresh real byte/failure evidence before changing SDK or conditions.

## Constraints

Scope is the original SDK build diagnostic, Windows x64 and the declared pipe/UI
conditions. This does not control arbitrary child tools, native applications,
terminal glyphs/fonts, every language/code page or display rendering. It does not
prove the SDK can discover a downstream pipe reader. An unknown or changed byte
encoding requires investigation, not a permissive decoder that hides corruption.

The shell reader itself is reused on both samples; new release-specific guidance
here corrects the proposed opt-out against actual artifacts. Its unavailable
subgoal remains explicit. No unsupported switch, older-target feature, invisible
performance change, production setting or recurring OS matrix is introduced.

## Evidence

`eng/Test-EstuaryWindows.ps1 -Record` builds both actual target/SDK sets and captures
non-ASCII diagnostics under private CP1252/65001 consoles. Default, false, true and
the source-level control must all yield the observed UTF-8 bytes; a mismatched
CP1252 reader must corrupt the diagnostic. Real failed builds preserve error text
and exit1. The private console's before/after code page is recorded, and process,
user and machine PATH plus persistent environment are checked unchanged. The
program executes after compilation. Before/after input hashes bind the saved
evidence to the canonical source. Raw logs remain ignored artifacts.

The evidence does not establish a working legacy-encoding opt-out. Full installed
Markdown/applicability/evidence retrieval is checked separately through the real
ordinary package. Eight foundations, PIN-959 qualifying coverage, PIN-963 actual-GA
and unrelated Human gates remain open.

## Sources

- [Audited SDK draft explaining the proposed opt-out](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview1/sdk.md)
- [Pinned CLI source; source availability is not pipe execution proof](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/sdk/src/Cli/dotnet/Program.cs)
- [.NET SDK environment variables](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables)
