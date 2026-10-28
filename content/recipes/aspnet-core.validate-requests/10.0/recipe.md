# Reject invalid Minimal API input before the handler runs

## Goal

The Moss observatory accepts watch requests with a station name and one to six observers. Invalid fields must return 400 before the handler performs work. A valid request may still fail a business decision, such as a station being closed; that is a separate 409 outcome inside the handler.

This is the synchronous .NET 10 foundation, retained for .NET 11. The later asynchronous validation edition is separate work. Shared asynchronous validation guidance does not turn this synchronous filter into an asynchronous business-rule pipeline.

## Older approach — an explicit .NET 9 endpoint filter

.NET 9 has no built-in Minimal API AddValidation registration. A custom endpoint filter is a valid way to validate bound data before invoking the handler. The comparison uses Validator.TryValidateObject on the same flat public model and returns ValidationProblem when annotations fail. TryValidateObject does not recursively validate arbitrary nested object graphs; this example deliberately has no nested model.

Save each project in a separate MossRequests directory. Keep the target directories under samples/net9.0, samples/net10.0 and samples/net11.0. Save the shared MossRequestChecks.cs inside samples/net10.0/MossRequests; the other two projects link to that canonical file. For a standalone copy, place the fixture in the chosen project and remove its linked Compile item. The check fixture is supplied below once for all three projects. It sends real HTTP requests and observes handler-call counts.

### Directory.Build.props

{{snippet:n9-directory-build-props}}

### Directory.Build.targets

{{snippet:n9-directory-build-targets}}

### Directory.Packages.props

{{snippet:n9-directory-packages-props}}

### global.json

{{snippet:n9-global-json}}

### MossRequests.csproj

{{snippet:n9-mossrequests-csproj}}

### Program.cs

{{snippet:n9-program-cs}}

### WatchRequest.cs

{{snippet:n9-watchrequest-cs}}

## Newer approach — generated validation with AddValidation

Call AddValidation before building the application, in the assembly that declares the endpoint and public request model. The web SDK supplies the shared-framework validation infrastructure and generator; no explicit Microsoft.Extensions.Validation package reference is needed for these pinned web projects. Keep the annotations and remove the custom endpoint filter. The built-in validation filter now rejects invalid model fields before the handler.

The invalid-field request in the fixture is the discovery check: attributes without generated metadata/registration can silently allow invalid data through. A successful build alone cannot prove the model is covered. Keep WatchRequest in its own .cs source file and accessible to generated code. For split assemblies, call a registration method that invokes AddValidation from the endpoint/model assembly, as well as registering the host's own models when needed; that split-assembly route is explained, not separately exercised here.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### MossRequests.csproj

{{snippet:n10-mossrequests-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

### WatchRequest.cs

{{snippet:n10-watchrequest-cs}}

### Shared HTTP check fixture — samples/net10.0/MossRequests/MossRequestChecks.cs

{{snippet:request-checks}}

### Same synchronous foundation on .NET 11

This separate compiled consumer verifies the target-11 route while preserving edition 10.0. It uses the same synchronous annotations, registration and handler; it does not add asynchronous validation.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### MossRequests.csproj

{{snippet:n11-mossrequests-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

### WatchRequest.cs

{{snippet:n11-watchrequest-cs}}

## Migration

1. Keep input-shape rules on the public request model. In this example Required and StringLength bound the station name; Range bounds the observer count. Keep stateful business decisions such as station availability separate from those annotations.
2. Upgrade the web project to .NET 10 or a verified later target with a compatible SDK. Add AddValidation in the endpoint/model assembly before builder.Build. Preserve the shown package/build boundary; no new package is required for these web-framework assets.
3. Remove the old custom validation filter only after testing equivalent rejection before handler execution. Keep other filters that implement authorization, business policy or a deliberate response contract. Do not call DisableValidation on the protected endpoint.
4. Build each directory with its declared SDK using `dotnet build --configuration Release`, then run the corresponding `bin/Release/net9.0/MossRequests.dll`, `net10.0` or `net11.0` output with that runtime. Expect four requests: invalid fields → 400 with Station/Observers errors and zero handler calls; malformed JSON → 400 and still zero calls; valid data → 200 and one call; valid data for a closed station → 409 and two total calls.
5. Repeat the invalid-field check after moving endpoints or models to another assembly. Registration in the host alone does not establish generator discovery in a library. Treat a 200 response to the invalid payload as a missing validation boundary, even if the application compiled successfully.

## Constraints

Synchronous DataAnnotations validation of a public same-assembly request model before the Minimal API handler. Binding/JSON parsing and business validation are separate. AddValidation must execute in the assembly containing the endpoint/model for generated metadata; moving models/endpoints requires registration in their assembly too. No asynchronous IAsyncValidatableObject implementation, external business check, auth, persistence or strict unmapped-JSON-member contract.

Validation is not authorization, duplicate detection, stock reservation or a database transaction. Malformed JSON is rejected by binding, independently of annotations. Unknown JSON properties are ignored by default; AddValidation does not supply a strict JSON contract. Configure JSON unmapped-member handling separately if that is your requirement. Error wording may vary; the fixture asserts status, field keys and handler counts instead of localized text.

The top-level AddValidation API and built-in filter are stable. Some .NET 10 low-level validation resolver/attribute APIs are experimental; none are used here and there is no broad suppression. The samples require C# 13/SDK 9.0.319, C# 14/SDK 10.0.402 and C# 15/SDK 11.0.100-rc.2.26504.105 respectively. Only targets 10 and 11 are catalogue routes; .NET 9 is the canonical historical comparison, not a new catalogue target. Requirements, exact test pins and language mode remain separate.

## Evidence

Fresh compile-and-run records identify each project's exact SDK, runtime output, inputs and resolved framework-only dependency set. Four real loopback requests on each target establish rejection, generated discovery, valid admission and the distinct business conflict. No mock, external service, cross-compilation or asynchronous validation is involved. Execution was Windows x64 only, with RC2 rather than GA for .NET 11. The installed package scenario separately verifies complete offline body, applicability and edition selection on targets 10 and 11.

Official sources: [Minimal API validation](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0?view=aspnetcore-10.0#validation-support-in-minimal-apis), [generation and assembly prerequisites](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/validation?view=aspnetcore-10.0), [.NET 9 filters](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters?view=aspnetcore-9.0), [TryValidateObject limits](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validator.tryvalidateobject?view=net-9.0).
