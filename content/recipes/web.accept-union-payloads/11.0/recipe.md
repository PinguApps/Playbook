# Accept several JSON body shapes through one explicit contract

## Goal

A parcel service accepts either a locker pickup or a street delivery through one endpoint. Its existing wire contract has no discriminator: `{"lockerCode":"B7"}` and `{"streetAddress":"4 Gull Lane"}` are the two supported object shapes. The contract should remain visible in the parameter type instead of routing each DTO through separate endpoints.

## Requirements

The .NET 10 sample uses an explicit converter on one wrapper type. This remains a valid way to preserve an established untagged contract. C# 14 has no native union declaration, so the previous sample does not pretend to use one. The .NET 11 sample replaces that converter/wrapper with `Delivery(LockerPickup, Courier)` and a built-in JSON structural classifier. Both expose the same Minimal API, MVC and SignalR methods.

Both union cases begin as JSON objects. A classifier is required to choose between them; the two distinct property names provide that choice. `JsonUnmappedMemberHandling.Disallow` makes mixed or unknown fields fail instead of being silently ignored. The structural classifier examines root property names and required properties, not values inside them. Conversion of the chosen case can still fail, for example when `lockerCode` is numeric. Model/business validation remains separate: neither version verifies an address or rejects every possible null/empty string.

## Migration

1. Preserve the wire shapes before changing the parameter model. Save the canonical files and the shared runner at the paths below, including the directory boundaries and linked check path.
2. On .NET 11 use C# **15.0**, the native union, the two case records and `[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]`. No `preview` language mode is needed on the pinned RC2 compiler.
3. Use the union directly as a Minimal API body parameter, MVC `[FromBody]` argument and SignalR method argument/result. Keep the SignalR **JSON** protocol. Do not migrate route, query, header or form binding to unions; MessagePack and Newtonsoft protocols do not implement this contract.
4. Run `dotnet run -c Release` in either canonical project directory. The executable starts real Kestrel, posts both bodies to `/minimal` and `/mvc`, invokes `Echo` through real SignalR JSON WebSocket frames and compares returned JSON structurally with the input.
5. The same runner sends `null`, `{}`, an unknown property, both case properties together, a numeric locker code and a Boolean body. HTTP returns 400; SignalR returns an invocation error. A changed status, accepted invalid argument or changed response shape fails the runner. It prints a single PASS summary and shuts down when all checks pass. For a normal long-running server replace the proof call with `app.RunAsync()`.

## Constraints

A default union has no active case. The .NET 11 endpoints reject that value and the hub returns an invocation error; the previous wrapper also rejects null. Use `json.serialize-closed-results` for the shared classifier, null/default and source-generation explanation, and `csharp.closed-results` for matching and declaration rules. This recipe adds web binding and protocol checks.

Structural classification scans each object; changing case properties can change classification and break existing payloads. It does not support nested unions, polymorphic cases or reference-preserving deserialization. An explicit discriminator remains a useful alternative when the wire format can change. Avoid indistinguishable object shapes rather than relying on property values to select them.

Numeric/string cases need care under Minimal API/MVC Web JSON defaults because numbers may be read from strings. SignalR JSON's defaults differ. This example deliberately uses two distinct object properties and verifies all three integrations; it does not claim arbitrary unions behave identically across serializers. The checks establish managed Windows x64 RC2 behavior, not GA, NativeAOT, trimming or a platform matrix.

## Older approach: .NET 10

Save these at the shown paths under an empty working directory. Keep the directory boundary files; they prevent unrelated parent build rules from changing this example.

### `samples/net10.0/HarbourBodies/Directory.Build.props`

{{snippet:n10-directory-build-props}}

### `samples/net10.0/HarbourBodies/Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### `samples/net10.0/HarbourBodies/Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### `samples/net10.0/HarbourBodies/HarbourBodies.csproj`

{{snippet:n10-harbourbodies-csproj}}

### `samples/net10.0/HarbourBodies/Program.cs`

{{snippet:n10-program-cs}}

### `samples/net10.0/HarbourBodies/global.json`

{{snippet:n10-global-json}}

## Newer approach: .NET 11

Save these at the shown paths under an empty working directory. Keep the directory boundary files; they prevent unrelated parent build rules from changing this example.

### `samples/net11.0/HarbourBodies/Directory.Build.props`

{{snippet:n11-directory-build-props}}

### `samples/net11.0/HarbourBodies/Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### `samples/net11.0/HarbourBodies/Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### `samples/net11.0/HarbourBodies/HarbourBodies.csproj`

{{snippet:n11-harbourbodies-csproj}}

### `samples/net11.0/HarbourBodies/Program.cs`

{{snippet:n11-program-cs}}

### `samples/net11.0/HarbourBodies/global.json`

{{snippet:n11-global-json}}

### Shared executable checks

Save this as `samples/net11.0/HarbourBodies/BodyChecks.cs` shared by both targets, matching the .NET 10 linked Compile path. The runner starts loopback Kestrel, sends real HTTP and WebSocket frames, then shuts down. For an ordinary long-running app, replace the `BodyChecks.RunAsync(app)` call with `app.RunAsync()` and omit the linked check file.

{{snippet:body-checks}}

## Evidence

The exact SDK, language and package inputs are recorded with the samples. The saved compile-and-run records execute the real request runner. No mocks or cross-compilation are used. Installed-package checks independently compare the entire expanded body and applicability over stdio from an unrelated directory; that delivery check does not replace the runtime checks. GA reconciliation and comprehensive product publication gates remain outstanding.

- [ASP.NET Core union boundaries and serializer differences](https://devblogs.microsoft.com/dotnet/unions-and-closed-hierarchies-in-aspnetcore/)
- [System.Text.Json union classifier contract](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries#union-types)
- [Current C# union language contract](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union)
