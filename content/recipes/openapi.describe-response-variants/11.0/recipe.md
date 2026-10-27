# Preserve every supported response shape

## Goal

Declare all response media and shapes for one status code, inspect same-media anyOf, and check the concrete client before accepting a changed generated contract. The fictional archive service publishes reading summaries, performs structured searches and returns ZIP downloads. Its original .NET 10 and .NET 11 projects solve the same archive tasks; the three related recipes share the complete executable fixture.

## Older approach

The original .NET 10 archive exposes separate JSON reading, empty-result and text routes. Each has one unambiguous 200 contract, so consumers can retrieve every supported representation. This is a valid compatibility-oriented design. Repeating declarations on a single status code in the older framework would retain just one response type; adding more annotations alone cannot provide the new aggregate contract.

## Newer approach

The .NET 11 archive offers one summary route per API style. Its declarations preserve `application/json` and `text/plain` separately. The JSON entry combines ReadingSummary and EmptySummary through `anyOf`; a response must satisfy at least one alternative, not necessarily exactly one. The explicit `kind` selector makes every branch reproducible; this example does not implement Accept-driven negotiation. Program.cs also shows a small result-metadata cleanup: `TypedResults.Unauthorized()` supplies 401 metadata in .NET 11, while the older sample declares it explicitly. This always-denied endpoint demonstrates metadata only.

{{snippet:focus}}

The complete canonical files below include service registration, document mapping, routes, MVC actions, models and executable checks. No external implementation file is needed to recreate the sample. Package setup requires a normal restore; consulting the installed recipe does not.

## Migration

1. List the real response branches by status, media and model. Remove stale or duplicate declarations before upgrading; declarations previously hidden by overwrite can now become visible in the contract.
2. Use multiple Produces calls or MVC ProducesResponseType attributes for the actual variants. Fetch the generated document, resolve schema references, check both media entries and both anyOf cases, and confirm retired declarations are absent.
3. Run a concrete consumer against each real response branch. The canonical HttpClient consumer checks both JSON models and the text body on Minimal API and MVC; extend those checks when the contract changes.
4. Regenerate and compile the actual downstream client. The measured NSwag 14.7.1 check uses the unmodified 3.0 document emitted by this .NET 11 server. Both JSON cases survive in AdditionalProperties; text throws a JSON-deserialization ApiException. Use the explicit HttpClient branch for text or separate routes/a verified generator configuration. Do not claim a typed union from this result.
5. Recreate the directory layout and files below. Run each ArchiveContracts.csproj from its own directory with the SDK named by its global.json, using dotnet run --configuration Release. Both programs start loopback Kestrel, fetch real generated documents, perform requests, print PASS and stop. These are executable verification examples, not production hosting defaults.

## Constraints

Response declarations describe behavior; they do not make the handler return those models, negotiate content, or remove an obsolete runtime branch. `anyOf` is not exclusive and introduces no discriminator. The sample's distinct property names let its explicit consumer choose the shape; version wire models deliberately if they become ambiguous. NSwag 14.7.1/NJsonSchema 11.6.1 with System.Text.Json generated code was compiled and executed on .NET 10 against the RC2 server: it does not produce strongly typed anyOf cases, and status 200 chooses JSON even for the text branch. This result is a concrete limitation, not evidence that every generator fails or succeeds. QUERY extension support and OpenAPI 3.2 parsing were not established by this generator check.

The new recipe is a .NET 11 edition only. The .NET 10 example is a historical comparison, not a conditional route for newer package assets on an older target. No extra platform workload, database, account or key is required. The chosen C# modes are 14.0 for the older project and 15.0 for the newer project. All package/SDK pins below identify actual authoring artifacts; no experimental API suppression is used. The .NET 11 evidence is RC2, not proof of final GA behavior, trimming, Native AOT or every supported OS.

## Evidence

Real .NET 10/11 compilation and loopback execution: separate older routes and .NET 11 all media/schema variants on Minimal API/MVC; two same-media anyOf references and no stale media; actual typed consumer handles every branch; 401 metadata and response. Both projects compiled and executed with their declared SDKs and locked package graphs; no mocks or cross-compilation substitute for those checks. Sample verification-evidence.json records retain exact input hashes, commands, dependencies and runtime output. The separate generated-client.evidence.json captures actual generation, compilation, execution and its checked limitation. Offline delivery is checked through the actual locally packed/installed tool over stdio, in an isolated feed/cache and unrelated directory; canonical body equality, applicability, evidence and target edition are checked. These checks establish the authored slice, not comprehensive catalogue or GA release readiness.

Official references: [Official response-metadata compatibility change](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/openapi-multiple-produces-per-status); [ASP.NET Core 11 canonical release notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview5/aspnetcore.md); [OpenAPI generation and explicit version selection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-11.0); [NSwag C# client generation](https://github.com/RicoSuter/NSwag/wiki/CSharpClientGenerator).


### Recreate samples/net10.0/ArchiveContracts

**Directory.Build.props**

{{snippet:n10-directory-build-props}}

**Directory.Build.targets**

{{snippet:n10-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n10-directory-packages-props}}

**global.json**

{{snippet:n10-global-json}}

**ArchiveContracts.csproj**

{{snippet:n10-archivecontracts-csproj}}

**Program.cs**

{{snippet:n10-program-cs}}


### Recreate samples/net11.0/ArchiveContracts

**Directory.Build.props**

{{snippet:n11-directory-build-props}}

**Directory.Build.targets**

{{snippet:n11-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n11-directory-packages-props}}

**global.json**

{{snippet:n11-global-json}}

**ArchiveContracts.csproj**

{{snippet:n11-archivecontracts-csproj}}

**Program.cs**

{{snippet:n11-program-cs}}


### Shared executable checks

Save this as samples/net11.0/ArchiveContracts/ContractChecks.cs. The .NET 10 project links this same file; keep the directory layout above.

{{snippet:contract-checks}}

### Generated-client check

The repository's eng/Test-ArchiveConsumer.ps1 repeats generation, independent compilation and real requests, with a fresh consumer directory outside the checkout. To reproduce without that helper:

1. Run the .NET 11 sample with environment ARCHIVE_CONTRACT_SERVE=1 and ARCHIVE_CONTRACT_OUTPUT set to an existing absolute scratch directory. It writes openapi.json from v30 and prints ARCHIVE_ADDRESS before waiting. Keep this terminal running.
2. Install NSwag.ConsoleCore 14.7.1 into a local tool directory with .NET 10's tool install and --tool-path. Do not rely on a global installation. Invoke its absolute executable with openapi2csclient, /Input pointing to that exported document, /Output pointing to Client.cs, /ClassName:ArchiveGenerated, /Namespace:ArchiveConsumer, /JsonLibrary:SystemTextJson, /UseBaseUrl:false and /OperationGenerationMode:SingleClientFromOperationId.
3. In a separate independent net10.0 console project, compile Client.cs together with the following Program.cs. Enable implicit usings, disable nullable for the generator output, and use SDK 10.0.402. No application package is required by the generated System.Text.Json client. Pass the printed server address as the first argument.
4. Expect PASS for both JSON cases on Minimal API and MVC, and EXPECTED LIMIT for the text request. AdditionalProperties retains raw JSON values; this is not a typed anyOf union. Stop the server and clear the two environment variables afterwards. Remove only your own scratch directories.

{{snippet:generated-consumer}}
