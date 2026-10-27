# Describe a file download accurately

## Goal

Describe binary download content and concrete file results when Minimal API or MVC endpoints return files. The fictional archive service publishes reading summaries, performs structured searches and returns ZIP downloads. Its original .NET 10 and .NET 11 projects solve the same archive tasks; the three related recipes share the complete executable fixture.

## Older approach

The archive already returns ZIP bytes on .NET 10. Its `Produces<Stream>` and MVC `ProducesResponseType<Stream>` annotations describe a binary payload without pretending the returned file object is JSON. This is still a useful choice when the concrete file result is unknown. No custom schema patch is needed for this comparison.

## Newer approach

Use the actual result type in the response declaration: `FileContentHttpResult` or `FileStreamHttpResult` for Minimal APIs, and `FileContentResult` or `FileStreamResult` for MVC. The `application/zip` declaration agrees with the file result's content type. The `soundings.zip` filename comes from the actual result; the schema is not a promise about every download header. Both byte-backed and stream-backed routes return the same valid empty ZIP archive.

{{snippet:focus}}

The complete canonical files below include service registration, document mapping, routes, MVC actions, models and executable checks. No external implementation file is needed to recreate the sample. Package setup requires a normal restore; consulting the installed recipe does not.

## Migration

1. Identify the real result, status code and media type for each download. Keep streaming and buffered responses distinct when choosing the metadata type.
2. On .NET 11 replace a file-specific schema transformer or generic placeholder with the matching file-result declaration shown in Program.cs. Keep a correct Stream declaration when the concrete result is intentionally unavailable.
3. Fetch the generated document and the real file. Resolve component references, verify string/binary for the 3.1 document, then verify status, media, filename and byte identity. The executable checks cover all four result types.
4. Recreate the directory layout and files below. Run each ArchiveContracts.csproj from its own directory with the SDK named by its global.json, using dotnet run --configuration Release. Both programs start loopback Kestrel, fetch real generated documents, perform requests, print PASS and stop. These are executable verification examples, not production hosting defaults.

## Constraints

Do not use `byte[]` as a synonym for a raw binary download: it describes encoded JSON data rather than this file-result contract. Metadata does not enforce authorization, perform content negotiation, or implement file validation/range handling. The checks establish full 200 responses only, not range requests, large-file performance or file-system safety.

The new recipe is a .NET 11 edition only. The .NET 10 example is a historical comparison, not a conditional route for newer package assets on an older target. No extra platform workload, database, account or key is required. The chosen C# modes are 14.0 for the older project and 15.0 for the newer project. All package/SDK pins below identify actual authoring artifacts; no experimental API suppression is used. The .NET 11 evidence is RC2, not proof of final GA behavior, trimming, Native AOT or every supported OS.

## Evidence

Real .NET 10/11 compilation and loopback execution: all four Minimal API/MVC buffered/streamed file contracts resolved as string/binary in OpenAPI 3.1 and matched to 200 ZIP responses, filename and exact bytes. Both projects compiled and executed with their declared SDKs and locked package graphs; no mocks or cross-compilation substitute for those checks. Sample verification-evidence.json records retain exact input hashes, commands, dependencies and runtime output.  Offline delivery is checked through the actual locally packed/installed tool over stdio, in an isolated feed/cache and unrelated directory; canonical body equality, applicability, evidence and target edition are checked. These checks establish the authored slice, not comprehensive catalogue or GA release readiness.

Official references: [Official file-response metadata and older Stream route](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/fundamentals/minimal-apis/responses.md); [ASP.NET Core 11 canonical release notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview4/aspnetcore.md); [OpenAPI generation and explicit version selection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-11.0).


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
