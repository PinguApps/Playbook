# Preserve request URLs when the generated server address changes

## Goal

Check generated server URLs with empty and non-empty PathBase, update exact fixtures, and verify the application's request composition before choosing an explicit compatibility transform. The fictional Alder Fen service publishes current and retiring water-depth observations. These three related goals share the complete original executable comparison below.

## Older approach

The original .NET 10 Alder Fen service generates a root server URL ending in / when PathBase is empty. Under /fen it generates the server URL with that prefix and no extra trailing slash. Its exact fixtures capture both cases. The application composes an operation URL by trimming trailing slashes from the server and adding the operation path; actual HTTP requests reach the same observation endpoint in both deployments.

## Newer approach

The .NET 11 generator drops the root trailing slash only when PathBase is empty. The v1 fixture therefore expects the bare origin; the /fen fixture stays unchanged. This sample's composition still works and real requests prove it. A separately named slash document demonstrates an explicit compatibility transformer that restores the root slash while preserving /fen. Prefer updating a cosmetic snapshot when that is the only difference; choose the transformer only for a measured downstream requirement.

{{snippet:focus}}

## Migration

1. Generate documents through the real request path at the origin root and with the actual non-empty PathBase. Record the effective scheme, host, port and prefix; account for trusted proxy configuration separately if used.
2. Update exact server URL fixtures for the empty-PathBase case. Keep the non-empty prefix expectation unchanged. The original assertions below compare every generated server URL, not just whether a string ends in a slash.
3. Run the application's real URL-composition logic against both forms and request the resulting endpoint. The sample trims the end of the server before adding /readings; it does not infer behavior for any named client generator.
4. If a consumer demonstrably needs the old empty-root spelling, register the optional document transformer shown in Program.cs and assert that /fen is not changed to /fen/. Regenerate any downstream client and test it separately.
5. Keep URI resolution rules distinct from concatenation. A root-relative /readings passed to new Uri(baseUri, path) discards /fen regardless of the root slash change. The check includes that control; preserve a prefix deliberately.
6. Recreate both independent directories below. Use each directory's pinned SDK and run dotnet run --configuration Release from that directory. Each example starts ephemeral loopback Kestrel, checks raw/v1/slash documents at the origin root and /fen, performs real requests, prints PASS and stops. These verification programs stop automatically; adapt hosting lifetime separately for a production service. Restore setup may need the network; installed guidance does not.

## Constraints

Only empty PathBase changes the generator's trailing-slash convention. Do not claim every client breaks: this original explicit HttpClient composition works on both versions. No third-party generated client is claimed to have been tested. OpenAPI operation paths normally begin with /, so naïve concatenation can duplicate a slash, while generic root-relative URI resolution can discard a deployment prefix. The optional transformer is narrowly for root URL spelling; it does not configure hosting, forwarded headers or routing.

This is an 11.0 edition; the .NET 10 source is a previous-release comparison, not a conditional older-target route. Use ASP.NET Core runtime/package assets for net11.0 and a suitable .NET 11 SDK. Canonical C# modes are explicitly 14.0 and 15.0. Verification pins are SDK 10.0.402 / OpenApi 10.0.13 / Microsoft.OpenApi 2.12.2 and SDK 11.0.100-rc.2.26504.105 / OpenApi 11.0.0-rc.2.26504.105 / Microsoft.OpenApi 3.10.2. They record the actual artifacts, not mandatory consumer patch upgrades. Stable APIs need no experimental opt-in. RC2 preparation under ADR 0004 does not prove final GA behavior. No database/provider, account, key, browser rendering mode or additional workload is required. Actual execution is Windows x64; no Native AOT, trimming or other-OS execution claim is made.

## Evidence

Actual declared .NET 10/11 SDK compilation and loopback Kestrel execution: raw/authored Obsolete operation/type/property flags with current controls; document/operation/schema transformers, inline traversal, response reference and override, object/scalar JSON extensions; exact empty/non-empty PathBase server fixtures, optional root-slash transform and composed HTTP requests. No mock, cross-compilation or third-party generated-client claim. Both projects have locked dependency graphs and saved compile-and-run records with exact inputs, SDKs and actual runtime output. The raw .NET 10 control lacks deprecation metadata; its authored v1 matches .NET 11 without a deprecation transformer. Current members remain unmarked. The root server spelling changes while /fen stays intact. These are execution assertions, not source-only conclusions. The package harness retrieves list/get through the real locally packed and installed stdio tool from an unrelated directory and isolated feed/cache; it checks full canonical Markdown, complete applicability and the 11.0 edition, rejects an older-target edition and preserves JSON/text equality. Local proxy blocking does not establish network isolation; the existing network-disabled CI lane supplies that check. This evidence covers this slice, not comprehensive catalogue or GA release readiness.

Official references: [Official empty-PathBase server URL compatibility change](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/openapi-server-url-trailing-slash); [OpenAPI generation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-11.0).


### Recreate samples/net10.0/FenContracts

**Directory.Build.props**

{{snippet:n10-directory-build-props}}

**Directory.Build.targets**

{{snippet:n10-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n10-directory-packages-props}}

**global.json**

{{snippet:n10-global-json}}

**FenContracts.csproj**

{{snippet:n10-fencontracts-csproj}}

**Program.cs**

{{snippet:n10-program-cs}}

**Readings.cs**

{{snippet:n10-readings-cs}}

**Transformers.cs**

{{snippet:n10-transformers-cs}}

**ContractChecks.cs**

{{snippet:n10-contractchecks-cs}}

**LegacyDeprecationTransformer.cs**

{{snippet:n10-legacydeprecationtransformer-cs}}


### Recreate samples/net11.0/FenContracts

**Directory.Build.props**

{{snippet:n11-directory-build-props}}

**Directory.Build.targets**

{{snippet:n11-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n11-directory-packages-props}}

**global.json**

{{snippet:n11-global-json}}

**FenContracts.csproj**

{{snippet:n11-fencontracts-csproj}}

**Program.cs**

{{snippet:n11-program-cs}}

**Readings.cs**

{{snippet:n11-readings-cs}}

**Transformers.cs**

{{snippet:n11-transformers-cs}}

**ContractChecks.cs**

{{snippet:n11-contractchecks-cs}}
