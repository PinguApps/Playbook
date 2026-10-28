# Bound an uploaded file read

## Goal

An estuary survey imports one instrument log. Keep one explicit byte ceiling,
cancel an active read when requested or when the component is disposed, and only
count a completed log as processed. This is a bounded consumer example, without
disk storage, a bulk upload service or a file-format parser.

## Older approach

The .NET 10 app deliberately retains dead `RemoteBrowserFileStreamOptions`
registration as a migration fixture. It configures all three properties:
`MaxSegmentSize`, `MaxBufferSize` and `SegmentFetchTimeout`. Options activation
would use the public parameterless constructor, but this app never resolves the
unused options. The narrow CS0618 suppression surrounds only that registration. Its values no longer
control this transfer. The real size/cancellation policy is already at
`IBrowserFile.OpenReadStream(long, CancellationToken)`, which remains valid on .NET 10.
Removing the dead registration is also a supported .NET 10 cleanup.

Create a fresh .NET 10 sample directory and save every file below. The empty
Directory files stop unrelated parent build/package policy. No template or extra
imports, CSS, appsettings or launch profile is required.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `EstuaryLogs.csproj`

{{snippet:n10-estuarylogs-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `LogUpload.razor`

{{snippet:n10-logupload-razor}}

## Newer approach

The .NET 11 app removes the registration and its warning suppression. The
consumer stays the same: a 1,048,576-byte limit, a cancellation token passed both
to `OpenReadStream` and each asynchronous read, and stream/token disposal.
`RemoteBrowserFileStreamOptions`, its public constructor and all three property
accessor pairs are absent from the inspected current Components.Web asset.

There is no verified public `BrowserFileStreamOptions` rename in either inspected
Components surface. Do not replace the class with that name or assert a mapping
between the removed segment, buffer or timeout knobs and the file-size limit.
Those equivalences remain factual gaps; this recipe teaches the supported
consumer boundary only.

The 4 KiB application buffer and 50 ms simulated decoding delay make an in-flight
cancellation observable. Neither configures the transport's segments, buffers or
timeouts. Processing is represented by the single final counter increment;
cancelled, oversized and failed transfers never reach it.

Create a separate .NET 11 sample directory with these complete files.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `EstuaryLogs.csproj`

{{snippet:n11-estuarylogs-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `LogUpload.razor`

{{snippet:n11-logupload-razor}}

## Migration

1. Find old registrations and references to the entire removed options type.
   Remove them and the now-unused imports. Keep existing consumer byte limits;
   do not replace them with `file.Size`, a client-supplied value.
2. Pass an application cancellation token to `OpenReadStream` and every read.
   Dispose the stream, cancel when the owning component is disposed, and keep
   failed or partial content out of the successful-processing path.
3. Restore/build each complete example using its matching absolute .NET executable
   from that sample directory: `build -c Release`. Set process-only
   `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` first. The first restore generates the lock
   file; retain it and use `-p:RestoreLockedMode=true` for subsequent builds.
4. Set process-only `ASPNETCORE_ENVIRONMENT=Development` and
   `DOTNET_ENVIRONMENT=Development` to serve build-output static assets. Run
   `bin/Release/net10.0/EstuaryLogs.dll --urls http://127.0.0.1:6210` or
   `bin/Release/net11.0/EstuaryLogs.dll --urls http://127.0.0.1:6211` with the
   matching .NET executable. This is local execution, not deployment guidance.
5. Open each root in a real browser and wait for `Interactive: True`. Select a
   small log and expect Accepted, its exact byte count and one processed log.
   Select a 1,048,577-byte file: expect rejection, zero bytes read and no added
   processed log. `OpenReadStream` rejects its reported size before this sample
   starts reading. That browser-reported size is not a security guarantee.
6. Select a 1,048,576-byte file, wait until some bytes are read, then click Cancel
   read. Expect Cancelled with a partial byte count and no added processed log.
   The sample's simulated decoding delay leaves time to cancel. Close each tab
   and stop its local host when finished.

## Constraints

The one-file input has no `multiple` attribute. The maximum is file bytes,
not a SignalR message ceiling, hub buffer setting, timeout or memory reservation.
The framework's size check uses browser-supplied metadata; it is not a substitute
for server-side quotas, file validation, authorization or malicious-client controls
in a production upload system. The example never trusts a file name as a path,
stores bytes on disk, or claims that extension/MIME/type checks make content safe.

InputFile and its stream depend on the connected Interactive Server circuit and
the selected browser file. Avoid replacing the input or selection during a read;
the input is disabled while busy. Circuit/network failures can still abort the
read; IOException is reported as rejection or transfer failure without processing.
Cancellation is cooperative and does not undo already performed external effects;
this example performs none before EOF.

The current sample pins SDK 11.0.100-rc.2.26504.105, net11.0/C# 15 and
ASP.NET Core/runtime 11.0.0-rc.2.26504.105. The previous sample pins SDK 10.0.402,
net10.0/C# 14 and ASP.NET Core/runtime 10.0.13. These are exact verification
baselines, distinct from the release-family consumer prerequisites above.
Microsoft.NET.Sdk.Web implicitly resolves the matching
Microsoft.AspNetCore.App.Internal.Assets package; no other package/provider,
optional workload or native SDK is needed. The stable APIs need no experimental
opt-in. RC2 execution does not establish GA verification. Other browsers/OS,
WebAssembly, Hybrid, deployment and older-target/new-package routes are unverified.
This edition is selected only for target 11.0; it adds no .NET 10 foundation.

## Evidence

Fresh compile records retain the commands, exact resolved assets and input
hashes for both canonical projects. The pinned source/API audit was rerun against
the actual local reference assets; its artifact hashes and signatures match.
Separate real Chrome checks use the running components and browser transport,
not a DOM mock. See `docs/verification/blazor-streams-callbacks.md` for measured
observations, exact artifacts and limitations. Required browser evidence remains
distinct from compilation and ordinary installed-tool checks.

The ordinary packed tool is installed from an isolated local feed/cache, then
list_recipes/get_recipe are called over real stdio from an unrelated directory.
The inspection checks ID/title/goal, explicit target 11.0/edition 11.0, complete
expanded Markdown, canonical snippet identity, applicability, fresh evidence and
equal structured/JSON-text representations. No network fetch supplies the served
guidance. This does not waive the aggregate removal-family coverage or GA gates.

Official sources: [Previous dead stream options](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Web/src/Forms/InputFile/RemoteBrowserFileStreamOptions.cs), [Current browser file size check](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web/src/Forms/InputFile/BrowserFile.cs), [Current stream cancellation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web/src/Forms/InputFile/BrowserFileStream.cs).
