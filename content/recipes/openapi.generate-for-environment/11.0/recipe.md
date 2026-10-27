# Generate a contract with the intended app environment

## Goal

Select a build-time application environment with OpenApiGenerationEnvironment when endpoint inclusion or document transformers depend on configuration. The fictional Sky Ledger observatory includes a calibration endpoint in Development and stamps the environment name into one document's title. Its published contract must reflect the intended configuration.

## Older approach

The actual .NET 10 ApiDescription.Server package reads the process environment. Set DOTNET_ENVIRONMENT and ASPNETCORE_ENVIRONMENT for that build process, generate the document, then restore their previous values. This is a valid approach and remains useful in existing pipelines. The older build assets do not implement OpenApiGenerationEnvironment; silently setting an unknown MSBuild property would not establish that the requested environment was used.

## Newer approach

With the .NET 11 ApiDescription.Server build assets, pass -p:OpenApiGenerationEnvironment=Development or Production to dotnet build. The package forwards that property as the generation tool's environment argument. It does not persist environment settings or select the environment of a later normal application run. The same application code still decides which routes and transformers depend on the selected environment.

{{snippet:focus}}

The project below sets OpenApiGenerateDocuments=false so ordinary sample checks run without also creating build documents. Enable generation explicitly for the commands below. Both OpenApi and ApiDescription.Server references are required; the latter is a private build-time dependency, not a runtime provider.

## Migration

1. Recreate the full files below with their separate SDK and locked package baselines. Use an absolute executable matching each global.json. The older and newer examples share the same observable environment-dependent endpoint/title behavior.
2. From the .NET 11 SkyLedger directory, run dotnet build SkyLedger.csproj -c Release -t:Rebuild -p:OpenApiGenerateDocuments=true -p:OpenApiGenerationEnvironment=Development -p:OpenApiDocumentsDirectory=<absolute-scratch-directory>. Repeat with Production and a different directory. Rebuild forces generation instead of reusing an incremental cache from the other environment. Do not run both against the same project outputs concurrently.
3. Read SkyLedger_environment.json in each output directory. Development must have info.title equal to Sky ledger: Development and a /observatory/calibration path; Production must say Sky ledger: Production and omit that path. /observatory/status remains present in both. Inspect the complete document before distributing it.
4. For the .NET 10 comparison, set the child process's DOTNET_ENVIRONMENT and ASPNETCORE_ENVIRONMENT to the intended value and use the same build command without OpenApiGenerationEnvironment. Restore both previous values in a finally block. Do not change persistent user/machine environment variables. The repository's eng/Test-SkyLedger.ps1 performs both real older builds and both property-driven newer builds with those protections.
5. Run the application normally under its intended runtime environment and check its contract independently. The shared runtime checks below validate the actual title/endpoint choices. A successful build document is not evidence that a deployed app uses the same environment or configuration.

## Constraints

Build-time generation launches application startup with the framework's mock server. Route registration and transformers execute; external service calls or other startup side effects can execute too. Keep startup independent of unavailable production resources. This fixture detects GetDocument.Insider only to keep its loopback test runner out of the tool's mock host; it leaves all route and transformer registration intact. No production resource or account is used.

OpenApiGenerationEnvironment requires the supported .NET 11 build/tool assets, pinned here to 11.0.0-rc.2.26504.105. The .NET 10 comparison uses ApiDescription.Server 10.0.0 with OpenApi 10.0.13. The newer property is checked while both ambient environment variables say Production, so the Development result demonstrates selection by the actual property. An arbitrary property value accepted by MSBuild alone proves nothing. This goal does not cover dynamic document-name discovery, deployment configuration, secrets or Native AOT. No experimental opt-in is used; final GA reconciliation remains separate.

## Evidence

Both actual net10.0/net11.0 projects compile and execute their real loopback checks. contracts.evidence.json separately records four actual build-time generations: older Development/Production process environments and newer Development/Production MSBuild selection. Each checks the calibration path and transformed title in a real generated document. This is execution of ApiDescription.Server's supported mock-host pipeline, not a mock test result or a claim of deployed HTTP behavior. The same records also capture a generated typed consumer for the ordinary 3.0 contract; that check is distinct from environment selection.

Fresh input hashes, exact resolved packages, SDKs, commands and output are saved through the existing evidence workflow. Full original Markdown, canonical setup/snippets, applicability and the 11.0 target edition are checked through real installed stdio list/get, with the existing isolated local feed/cache and unrelated working directory offline. No new OS/client/native matrix or product-wide readiness claim is introduced.

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
