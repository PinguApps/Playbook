# Choose a document version your client can use

## Goal

Select the OpenAPI specification version independently of the OpenAPI.NET package API, and verify a generated client before distributing a service contract. The fictional Sky Ledger observatory has a modern event contract and a small ordinary status contract for an existing client generator. These need not use the same document version.

## Older approach

The actual .NET 10 comparison defaults to OpenAPI 3.1 and can explicitly emit 3.0. Its ordinary dome-status endpoint is sufficient for a typed generated client. Keeping that explicit contract is a valid compatibility choice. The older framework does not supply this .NET 11 default 3.2 event description; no .NET 11 package route on net10.0 is claimed.

## Newer approach

The .NET 11 sample leaves v1 at its default 3.2, and explicitly registers v31 and v30. The client document deliberately selects 3.0 and includes only /observatory/status. This lets the event document retain standard itemSchema while the ordinary client contract stays within a measured consumer path. The names v1, v31 and client identify documents; they are not interchangeable with the openapi version field inside each document or with application API versions.

{{snippet:focus}}

Choosing 3.0 is a measured example, not a claim that every generator needs it. Test the actual consumer before selecting a newer or older contract. Reducing a specification version can move unsupported constructs into extensions that tools ignore; it does not upgrade or repair the client.

## Migration

1. Inventory the documents actually distributed and their consumers. Set OpenApiOptions.OpenApiVersion explicitly for contracts that require a fixed specification version. Inspect the generated openapi field; do not infer it from the filename or document name.
2. Rebuild and compare actual documents. This fixture checks .NET 10's default 3.1, .NET 11's default 3.2 and explicit 3.1/3.0 outputs. Keep a 3.2 event document when standard itemSchema matters.
3. Generate, compile and exercise a representative downstream client using the exact selected document. Recreate the complete files below and run each sample with its pinned SDK. The independent generated-client steps below use NSwag.ConsoleCore 14.7.1 and System.Text.Json against the unmodified client document, then call the real server.
4. Audit transformer source separately when upgrading Microsoft.AspNetCore.OpenApi. The saved graphs resolve Microsoft.OpenApi 2.12.2 on .NET 10 and 3.10.2 on .NET 11. Changing OpenApiVersion does not restore an older library object model or binary compatibility. Recompile custom transformers against the resolved package and inspect affected nested schema/reference APIs; the simple title transformer here compiles unchanged on both sets.

## Constraints

OpenAPI specification 3.2 and Microsoft.OpenApi package 3.x are different version systems. A package upgrade can require source changes even when you retain a 3.0 document. This example deliberately uses the common OpenApiSpecVersion and document-title surface; it does not teach a full transformer migration or assert that all custom transformer code remains compatible. Consult the linked official migration guide for affected object-model APIs and validate against the actual locked artifacts.

The verified downstream route is a single ordinary JSON model and operation. NSwag 14.7.1/NJsonSchema 11.6.1 generated typed DomeStatus code from the selected 3.0 document; an independent net10.0 build and real HTTP request passed for each server. OpenAPI 3.2 parsing, SSE generation, unions and every third-party generator remain unverified. No experimental API opt-in, account, cloud service, UI or database is needed. RC2 is not final GA evidence.

## Evidence

Actual .NET 10/11 builds and Kestrel requests check the generated version fields. The saved compile-and-run records also cover typed SSE versus raw JSON and runtime metadata. The separate contracts.evidence.json records capture Development/Production build commands, actual documents, NSwag generation, independent .NET 10 compilation and a real typed client request against each running server. Compilation, execution and the build tool's mock hosting server are identified separately. The independent client is created outside repository/shared-build inheritance and is not a mock.

The existing installed-tool harness retrieves discovery and complete canonical body/applicability/evidence offline using an isolated local feed/cache and unrelated working directory. It checks the 11.0 edition and rejects target 10.0 for this new recipe. The older sample is comparison evidence, not a new foundation or conditional route. Comprehensive coverage and GA reconciliation remain open.

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
