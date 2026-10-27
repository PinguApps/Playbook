# Describe a structured search carried by HTTP QUERY

## Goal

Map a read-only search body with HTTP QUERY and choose its native OpenAPI 3.2 operation or earlier-version extension after checking client support. The fictional archive service publishes reading summaries, performs structured searches and returns ZIP downloads. Its original .NET 10 and .NET 11 projects solve the same archive tasks; the three related recipes share the complete executable fixture.

## Older approach

Routing arbitrary methods is not new. The .NET 10 sample accepts QUERY with an explicitly bound JSON body and performs the same read-only search, but its built-in document generator omits that QUERY operation. A POST operation on the same path provides an ordinary documented body-search fallback. POST remains valid for that purpose; it does not communicate QUERY's safe/idempotent semantics to generic HTTP tooling. No equivalent native QUERY operation is invented for the older generator.

## Newer approach

Register documents explicitly for 3.2, 3.1 and 3.0, and map QUERY with `MapMethods`. `[FromBody]` makes the intended binding visible. The filter selects Soundings with a minimum count of two without changing the archive. In 3.2 the operation is `paths["/archive/search"].query`. In 3.1 and 3.0 it is `paths["/archive/search"]["x-oai-additionalOperations"].QUERY`. These are document-format choices within the .NET 11 sample, not a claim that the .NET 10 generator learned the extension.

{{snippet:focus}}

The complete canonical files below include service registration, document mapping, routes, MVC actions, models and executable checks. No external implementation file is needed to recreate the sample. Package setup requires a normal restore; consulting the installed recipe does not.

## Migration

1. Keep search execution read-only and repeatable. Define the structured filter and explicitly bind it from the body; configure production request-size and validation limits separately.
2. Use the .NET 11 OpenAPI package and explicitly select a document version your downstream tooling accepts. Register and map the OpenAPI endpoint as shown; request v32.json, v31.json or v30.json from the generated endpoint.
3. Send a real QUERY request with JSON, inspect the bound result and required request-body schema, and inspect the operation location for each document version. Exercise the actual proxy/client path before retiring a POST fallback.
4. Recreate the directory layout and files below. Run each ArchiveContracts.csproj from its own directory with the SDK named by its global.json, using dotnet run --configuration Release. Both programs start loopback Kestrel, fetch real generated documents, perform requests, print PASS and stop. These are executable verification examples, not production hosting defaults.

## Constraints

The inventory/release notes call QUERY a proposal; the linked IETF draft now resolves to the June 2026 Proposed Standard RFC 10008. That source correction does not establish universal adoption. Test method allowlists, browser CORS/preflight, gateways, caches and client generators in your deployment; the sample proves HttpClient to loopback Kestrel only. An extension can be retained by a parser and still be ignored by a generator. Request bodies do not remove size limits or supply validation/access control. Caching and automatic retries require the real method semantics and appropriate implementation; no cache or intermediary behavior is demonstrated.

The new recipe is a .NET 11 edition only. The .NET 10 example is a historical comparison, not a conditional route for newer package assets on an older target. No extra platform workload, database, account or key is required. The chosen C# modes are 14.0 for the older project and 15.0 for the newer project. All package/SDK pins below identify actual authoring artifacts; no experimental API suppression is used. The .NET 11 evidence is RC2, not proof of final GA behavior, trimming, Native AOT or every supported OS.

## Evidence

Real .NET 10/11 compilation and loopback execution: actual QUERY JSON body/search result; .NET 10 POST fallback and omitted QUERY; .NET 11 native 3.2 QUERY and 3.0/3.1 extension, required body properties and 200 response. Both projects compiled and executed with their declared SDKs and locked package graphs; no mocks or cross-compilation substitute for those checks. Sample verification-evidence.json records retain exact input hashes, commands, dependencies and runtime output.  Offline delivery is checked through the actual locally packed/installed tool over stdio, in an isolated feed/cache and unrelated directory; canonical body equality, applicability, evidence and target edition are checked. These checks establish the authored slice, not comprehensive catalogue or GA release readiness.

Official references: [RFC 10008 HTTP QUERY, superseding the safe-method-with-body draft](https://www.rfc-editor.org/rfc/rfc10008.html); [ASP.NET Core 11 canonical release notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview4/aspnetcore.md); [OpenAPI generation and explicit version selection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-11.0); [OpenAPI 3.2 Path Item operations](https://spec.openapis.org/oas/v3.2.0.html#path-item-object).


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
