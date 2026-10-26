# Bypass unneeded middleware for a deliberately standalone response

## Goal

A fictional signal cabin publishes a constant open/closed notice. It needs the common trace before routing, but no crew identity or chart-origin processing. Mark only this standalone response for immediate routing execution. Keep the crew manifest and origin-restricted chart on the normal path.

## Requirements

Use the .NET 11 ASP.NET Core shared framework/reference pack and SDK for the newer consumer. The previous consumer targets .NET 10. Exact SDK and language pins are included in every complete directory; no NuGet packages, provider account, workloads, native tools or browser renderer are required. Restore framework assets before offline builds. The snippets below contain all source and configuration needed to build and run each self-checking application. Run from inside the directory containing its global.json. Neither example is a persistent public server: it opens ephemeral loopback Kestrel ports and stops them automatically.

## Older approach — .NET 10

The .NET 10 comparison uses the existing ShortCircuit() endpoint convention. It is a supported way to achieve this goal and remains useful when the mapping code owns the decision. A second conventional endpoint deliberately selects status 202. The ordinary notice provides the same text while traversing the full middleware pipeline.

#### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### .NET 10: `FixtureAuthentication.cs`

{{snippet:n10-fixtureauthentication-cs}}

#### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

#### .NET 10: `Proof.cs`

{{snippet:n10-proof-cs}}

#### .NET 10: `SignalCabin.csproj`

{{snippet:n10-signalcabin-csproj}}

#### .NET 10: `global.json`

{{snippet:n10-global-json}}

## Newer approach — .NET 11

On .NET 11, attach [ShortCircuit] to the standalone Minimal API handler. Routing reads IShortCircuitMetadata and executes that endpoint immediately. The declaration sits beside the handler; it does not move the middleware or disable protections globally. The retained convention endpoint still works unchanged. The attribute also supports controller/action placement; no MVC execution proof is claimed by this Minimal API sample.

#### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### .NET 11: `FixtureAuthentication.cs`

{{snippet:n11-fixtureauthentication-cs}}

#### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

#### .NET 11: `Proof.cs`

{{snippet:n11-proof-cs}}

#### .NET 11: `SignalCabin.csproj`

{{snippet:n11-signalcabin-csproj}}

#### .NET 11: `global.json`

{{snippet:n11-global-json}}

## Migration

1. Audit the proposed endpoint and every middleware after UseRouting. Select only public self-contained responses that do not need that work. Keep identity, CORS, form-protection or custom processing dependencies on ordinary endpoints.
2. Create the complete target directory below and restore its matching framework assets. Keep UseRouting explicit so the before/after boundary can be inspected.
3. On .NET 10 retain ShortCircuit(); on .NET 11 optionally replace it with [ShortCircuit] beside the Minimal API handler. Keep the convention if it better expresses the mapping. Do not apply both just to duplicate the intent.
4. Run the real request proof. Both standalone endpoints must retain X-Cabin-Before and omit X-Cabin-After; the ordinary endpoint must retain both and all three must produce Cabin open.
5. Verify the protected endpoint still returns 401/403/200 for anonymous/denied/permitted fixture callers and that the normal chart emits its configured CORS header. Deliberately invalid short-circuit plus authorization/CORS controls must fail before their handlers execute. Remove those proof-only mappings from any deployed application.

From the chosen standalone directory, build and run the declared project:

- `$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'`
- `dotnet build ./SignalCabin.csproj -c Release`
- `dotnet ./bin/Release/net11.0/SignalCabin.dll`

For the previous directory, change only the run path to `net10.0`. Install the exact SDK declared in that directory and the matching ASP.NET Core runtime first. These commands prepare/run a local consumer; Playbook never executes them for the caller. Expect a final `PASS: SignalCabin` line and the request observations below.

## Constraints

Use only for a deliberately public, self-contained response that does not need skipped authentication, CORS, antiforgery, rate limiting, session, telemetry enrichment or any other downstream middleware. Audit default/fallback policies and custom checks too: the framework cannot discover every application-specific dependency.

Short-circuiting runs the endpoint during UseRouting and omits the remaining middleware. Middleware placed before UseRouting still executes. Place any checks that must apply to every request before routing or keep the endpoint on the ordinary pipeline. The endpoint still executes; short-circuiting does not mean returning without its handler.

With the default security-metadata checks, combining short-circuit metadata with authorization or CORS metadata throws InvalidOperationException. Required antiforgery metadata on relevant form methods is also rejected. Do not suppress those checks to make an unsafe endpoint work; remove short-circuiting from endpoints that need those protections.

The optional status code supplies the response status before the endpoint executes; it does not remove the endpoint, authorize callers or guarantee the final result will not set another status. The retained convention sample returns plain text with status 202.

The new attribute is in Microsoft.AspNetCore.Routing and implements IShortCircuitMetadata. It targets methods/classes, is not inherited and is single-use per target. It can be placed on a controller/action or Minimal API handler; this canonical proof selects Minimal APIs. The existing ShortCircuit() convention remains valid; the .NET 10 sample uses it for the same goal.

The X-Test-Crew authentication scheme and developer exception page exist only to expose the local comparison. Never trust that client header as a production credential or expose developer exception details in production. No performance saving, browser CORS enforcement, proxy behavior or production security configuration is measured here.

## Evidence

Nine real requests per target exercise three notices, three actual authorization decisions, a normal CORS response and two incompatible security-metadata controls. The safe handlers still return text, the before-routing trace remains, only marked endpoints skip the after-routing trace, and unsafe handlers stay at zero. CORS headers are checked at HTTP level, not through a browser.

Saved compilation records identify exact source hashes, SDK/target/language, dependency inventory, host and build output. `eng/Test-EndpointGuidance.ps1` separately rebuilds and executes all four consumers and records artifact hashes. Source inspection, actual compilation and real requests are distinct evidence; none is a mock, cross-compilation or GA claim. The packed-tool test compares the entire independently expanded Markdown and applicability through actual installed stdio list/get. See `docs/verification/endpoint-policies.md` for the authoring receipt. Only edition 11.0 is served: the .NET 10 code remains a comparison, not a ninth foundation.

- [Official ShortCircuit attribute release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#short-circuit-endpoints-with-an-attribute)
- [Routing order and short-circuit security restrictions](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-11.0#short-circuit-middleware-after-routing)
- [Pinned ShortCircuitAttribute metadata contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Http/Routing/src/ShortCircuit/ShortCircuitAttribute.cs)
- [Pinned routing execution and security guards](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Http/Routing/src/EndpointRoutingMiddleware.cs)
- [Previous-release endpoint convention](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Http/Routing/src/ShortCircuit/RouteShortCircuitEndpointConventionBuilderExtensions.cs)
