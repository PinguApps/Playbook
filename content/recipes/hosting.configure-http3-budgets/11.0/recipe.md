# Configure HTTP/3 connection budgets without preview scaffolding

## Goal

An estuary observatory serves live readings and bulk exports. Its author wants
bounded connection resources while a slow receiver catches up. Configure the
QUIC transport, then check real streams and backpressure. A successful build
after deleting a suppression does not establish that those settings work.

## Requirements

Use Kestrel HTTP/3 on an OS with supported QUIC/TLS 1.3 and native MsQuic, plus
an actual HTTP/3 client. This complete reproduction executes on Windows x64;
the assembly platform annotation declares that tested lane. It does not assert
Linux/macOS support from a managed build. It fails if QUIC is unavailable and
requires HTTP/3 exactly rather than accepting a silent HTTP/2 fallback.

The exact older set is SDK10.0.402, .NET/ASP.NET10.0.13 and C#14. The newer set is
SDK11.0.100-rc.2.26504.105, .NET/ASP.NET11.0.0-rc.2.26504.105 and C#15. Both load
MsQuic2.5.11.159415436. These are measured reproduction pins, not minimum patches.
The newer migration targets the .NET11 release family; RC2 evidence is not actual
GA evidence. No additional NuGet library, workload, hosted provider or browser
rendering is required. The empty build/package boundary files prevent inherited
project settings from changing this standalone example.

The four options already exist on .NET10 with RequiresPreviewFeatures. Its
working consumer acknowledges CA2252 only around those assignments. The local
editorconfig makes that diagnostic visible even if an outer repository lowers
its severity. .NET11 removes those annotations; its consumer has no preview
suppression and no EnablePreviewFeatures property. No C#15-only syntax is used.
The preview acknowledgement remains required when retaining the older API set.

## Constraints

MaxBidirectionalStreamCount limits concurrent inbound streams per connection.
The example selects two request streams. MaxUnidirectionalStreamCount is three:
HTTP/3 needs control and QPACK streams. Lowering it carelessly can prevent the
protocol from working. The separate raw QUIC control opens those three streams
and confirms a fourth waits for credit; it is not an application request.

MaxReadBufferSize and MaxWriteBufferSize are 16KiB transport backpressure
thresholds. They are not hard process-memory bounds, total connection budgets,
request-body limits or guaranteed wire-window sizes. Native/protocol/client
buffering also participates. The observed pause position can exceed 16KiB and
vary between runs. The import handler intentionally copies its fixed test upload
into memory to check completed byte counts. Application endpoints should enforce
a deliberate body-size cap and stream to storage instead of copying that fixture
sink. Do not size a production memory budget by multiplying this
number alone, or set null/unbounded buffers merely to avoid a slow consumer.

These settings are distinct from per-user quotas, authorization, endpoint rate
limits and limits across multiple connections. The client explicitly disables
additional HTTP/3 connections; otherwise another connection could hide the
per-connection stream limit. Choose workload-specific values and measure them.

The private certificate is exported/reloaded for Windows TLS private-key support.
It is not added to shared trust. Both clients pin only that fixture certificate;
do not replace production certificate validation with an allow-all callback.
The fixture opens ephemeral loopback listeners and disposes them automatically.

## Migration

1. Identify actual request concurrency, streaming payloads and slow readers.
   Separate the connection's transport policy from application/user quotas.
2. Configure QuicTransportOptions through services before building the host.
   Keep protocol stream credit and explicit finite read/write thresholds.
3. On .NET10, retain the four-property CA2252 acknowledgement. On .NET11 remove
   it and any now-unneeded broad preview opt-in after rebuilding with the actual
   newer references. Do not remove preview handling for other experimental APIs.
4. Copy the complete corresponding project below. Set
   DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in the child process, select that project's
   directory and build Release with its absolute matching dotnet executable.
   Initial restore creates a package lock; subsequent restores use --locked-mode.
   Execute bin/Release/net10.0/EstuaryBudgets.dll or net11.0/EstuaryBudgets.dll
   with the same matching executable.
5. Expect two active requests and a queued third, followed by three completed
   responses on one connection. A 16MiB export pauses while the client does not
   read, then completes. A 16MiB import pauses while the server does not read,
   then completes. The fourth unidirectional open cancels while credit is exhausted.
6. Retain the native version/hash and actual HTTP/3 observations. Requalify a
   different OS/native/runtime lane rather than inferring it from compilation.
   Keep final-GA reconciliation and comprehensive publication gates open.

## Older approach: .NET 10

Copy every file below at its relative path into an independent directory.

### .editorconfig

{{snippet:n10-editorconfig}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### EstuaryBudgets.csproj

{{snippet:n10-estuarybudgets-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach: .NET 11

Copy every file below at its relative path into an independent directory.

### .editorconfig

{{snippet:n11-editorconfig}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### EstuaryBudgets.csproj

{{snippet:n11-estuarybudgets-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Evidence

Run eng/Test-WebResourceRecipes.ps1 -RecordEvidence from the task worktree to
refresh the six actual consumers, diagnostic controls and audited artifact hashes.
The saved sample records include exact commands, locked assets, native identities,
inputs and observed output. Run the installed-artifact Inspector and returned-body
consumer commands in docs/inspect-guidance.md. No recurring OS/client/native matrix
is added. The eight approved .NET10 foundations and existing edition IDs remain
unchanged; this comparison is part of one new 11.0 edition.

## References

- [QUIC transport option contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Servers/Kestrel/Transport.Quic/src/QuicTransportOptions.cs)
- [Transport flow-control implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Servers/Kestrel/Transport.Quic/src/Internal/QuicStreamContext.cs)
- [Previous preview annotations](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/Servers/Kestrel/Transport.Quic/src/QuicTransportOptions.cs)
- [Kestrel HTTP/3 platform requirements](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-11.0)
