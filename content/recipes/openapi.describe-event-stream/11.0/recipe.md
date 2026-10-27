# Describe each event in a typed server stream

## Goal

Return typed Server-Sent Events and document each payload with OpenAPI 3.2 itemSchema when clients need to interpret a live event stream. The fictional Sky Ledger observatory sends dome alignment readings. A reader needs the payload shape, event name and event ID, rather than a description of one large response string.

## Older approach

The .NET 10 comparison already sends correctly framed events through TypedResults.ServerSentEvents. Keep that valid transport. Its default document is OpenAPI 3.1 and cannot express the standard 3.2 itemSchema contract. A separately documented payload or independently maintained client remains a valid older solution; changing a raw asynchronous sequence into an SSE result is a transport decision, not merely an annotation change. The older project below uses its actual .NET 10 dependencies, not a simulated older compiler mode on .NET 11.

## Newer approach

Use the dedicated SseItem<T> result overload without an eventType argument. Each item carries the typed DomeStatus payload, the alignment event name and a scan ID. The .NET 11 default document describes one event with itemSchema, whose required data property references the DomeStatus component. Optional event/id fields describe the SSE envelope. The same iterator returned directly from /observatory/raw is an application/json array; it is not an SSE endpoint.

{{snippet:focus}}

This finite example sends two readings so execution can check the entire response and finish. It makes no continuous-delivery latency or reconnection guarantee. A production source should observe the supplied cancellation token during its own waits and reads.

## Migration

1. Keep the domain payload separate from SSE envelope fields. Return TypedResults.ServerSentEvents with the SseItem<T> sequence. For a plain IAsyncEnumerable<T> and one shared event name, the separate eventType overload is another valid design; do not combine it with this typed-item overload.
2. Generate an OpenAPI 3.2 document and inspect responses/200/content/text~1event-stream/itemSchema. Resolve its data reference and check the payload properties, event and id. A 3.1 export uses x-oai-itemSchema in the newer generator; extension presence does not prove the client's support.
3. Read an actual response with ResponseHeadersRead. Check text/event-stream, blank-line frame boundaries, event names, IDs and each JSON data payload. The complete shared checks below perform this and independently verify that the raw route is JSON.
4. Recreate the complete paired files below, preserving their relative directory layout. From each SkyLedger directory, use the absolute dotnet executable matching global.json to run dotnet run -c Release. Both programs start loopback Kestrel, perform checks, print PASS and exit. Restore requires package access; the installed recipe is wholly offline.

## Constraints

Standard itemSchema requires OpenAPI 3.2. Choosing an earlier document version leaves the actual SSE response unchanged but moves this metadata into an extension; confirm your exact parser, generator or UI understands it. The generated NSwag client included in the shared fixture consumes only the ordinary status endpoint from a deliberately selected 3.0 document. It is not evidence of generated SSE support. The real stream check uses HttpClient and parses each received frame.

The sample's case-sensitive schema checks follow its actual System.Text.Json web defaults. No union, browser EventSource, authentication, replay store, heartbeat or reconnect policy is claimed. Stable framework APIs use no experimental opt-ins. .NET 11 RC2 evidence prepares the release-family recipe; final GA behavior remains to be reconciled.

## Evidence

Both actual SDK/package sets compiled and executed. The .NET 11 check resolves itemSchema's payload reference, checks the integer exposure type and envelope fields, and confirms the 3.1 extension distinction. Both servers deliver two real frames with expected payload/name/id and a separate raw JSON array. This is actual local execution, not source review, cross-compilation or a mock HTTP response. Saved verification-evidence.json records identify exact inputs, resolved dependencies, commands and runtime output. The build generator itself uses the framework's mock hosting server; those build documents are separate evidence from Kestrel/SSE execution.

Shared contracts.evidence.json records cover two build environments and the selected ordinary generated consumer. The installed package scenario checks complete canonical Markdown, discovery ID/title/goal, exact applicability, target edition and fresh evidence through real stdio list/get in an isolated feed/cache outside the checkout. These are focused recipe checks, not comprehensive catalogue, native-platform or GA readiness claims.

### Complete canonical projects

Use the following exact SDK/package pins to reproduce the recorded checks. They identify verification artifacts, rather than requiring consumers to adopt these RC builds. Consumer requirements are .NET/ASP.NET Core 11 with matching OpenAPI build assets, C# 15 for the shown source, a supported server platform and ordinary restore access. The older comparison uses .NET 10/C# 14. Execution was measured on Windows x64; no browser rendering mode, native workload, database or cloud provider is required. Installed Playbook retrieval itself needs no network, keys or checkout.

### Recreate samples/net10.0/SkyLedger

**Directory.Build.props**

{{snippet:n10-directory-build-props}}

**Directory.Build.targets**

{{snippet:n10-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n10-directory-packages-props}}

**global.json**

{{snippet:n10-global-json}}

**SkyLedger.csproj**

{{snippet:n10-skyledger-csproj}}

**Program.cs**

{{snippet:n10-program-cs}}


### Recreate samples/net11.0/SkyLedger

**Directory.Build.props**

{{snippet:n11-directory-build-props}}

**Directory.Build.targets**

{{snippet:n11-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n11-directory-packages-props}}

**global.json**

{{snippet:n11-global-json}}

**SkyLedger.csproj**

{{snippet:n11-skyledger-csproj}}

**Program.cs**

{{snippet:n11-program-cs}}

### Shared runtime checks

Save this file at samples/net11.0/SkyLedger/ContractChecks.cs. The older project links the same file, so preserve the relative layout. It uses a bounded HTTP timeout, consumes the finite stream, and shuts down the host. The GetDocument.Insider branch allows actual build-time generation instead of trying to connect to a mock server.

{{snippet:checks}}

### Reproduce the selected generated consumer

1. Build either project with generation enabled as described above. The output SkyLedger_client.json contains the explicit 3.0 ordinary status contract. It is the unmodified input to the generator.
2. Set SKY_LEDGER_SERVE=1 only for the sample process and run its built DLL with its absolute dotnet host. It first performs the real checks, prints SKY_ADDRESS with the loopback address, then waits. Keep that process running; reset the variable afterwards.
3. Using the .NET 10 host, install NSwag.ConsoleCore version 14.7.1 with tool install --version 14.7.1 --tool-path <absolute-local-tool-directory>. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 for that child process. Invoke the installed nswag executable by absolute path with openapi2csclient, /Input:<absolute-SkyLedger_client.json>, /Output:<absolute-consumer-directory>/Client.cs, /ClassName:SkyGenerated, /Namespace:SkyConsumer, /JsonLibrary:SystemTextJson, /UseBaseUrl:false and /OperationGenerationMode:SingleClientFromOperationId. Installation/restore requires network access; later Playbook consultation does not.
4. In an unrelated directory create Consumer.csproj using Microsoft.NET.Sdk, OutputType Exe, TargetFramework net10.0, ImplicitUsings enable, Nullable disable and TreatWarningsAsErrors true. Add global.json pinning SDK 10.0.402 with rollForward disable. Save the source below as Program.cs beside generated Client.cs. Run the .NET 10 executable with run -c Release -- <the-printed-SKY_ADDRESS>. Expect PASS and the typed West dome/7 result; an exception, build failure or mismatched payload is a failure.
5. Stop the sample server and delete only the scratch consumer/tool/output directories you created. The repository's eng/Test-SkyLedger.ps1 automates these same steps for both servers, compares build environments, restores its process settings and verifies PATH is unchanged.

{{snippet:consumer}}

Official references: [OpenAPI generation and build environments](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-11.0), [SSE item metadata](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/fundamentals/openapi/include-metadata.md), [OpenAPI.NET source API migration](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/microsoft-openapi-3x), [NSwag C# generation](https://github.com/RicoSuter/NSwag/wiki/CSharpClientGenerator), [canonical RC1 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/aspnetcore.md), [canonical Preview7 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview7/aspnetcore.md).
