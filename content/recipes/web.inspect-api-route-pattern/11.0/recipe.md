# Read parsed route contracts in an API explorer extension

## Goal

A marine archive's tool author wants to list specimen constraints, expedition
options, chart paths and station defaults. A route displayed as
`mvc/specimens/{id}` looks unconstrained even though the server accepts only
positive integers. Inspect the route contract supplied by API discovery before
presenting those details to the tool's user.

`ReadMarineContract` is an original, nullable explorer extension. It projects
parameter names, optional/catch-all flags, defaults and policy declarations into
a small report. The executable invokes it on eight descriptions from the actual
MVC and Minimal API providers, then sends matching and nonmatching HTTP requests.
It does not generate an OpenAPI document or implement a routing engine.

## Constraints: supported prerequisites and tested artifacts

The new property requires **ASP.NET Core 11** and matching reference/runtime
assets. Register `AddControllers` for visible attribute-routed MVC actions and
`AddEndpointsApiExplorer` for Minimal handlers, map the endpoints and initialize
the host before collecting descriptions. The built-in providers populate the
property for the routes tested here. A custom/legacy provider can leave it null;
never assume every description supplies a parsed contract.

`ApiDescription.RoutePattern` is a nullable read/write `RoutePattern` in
`Microsoft.AspNetCore.Mvc.Abstractions`. Discovery is in `Mvc.ApiExplorer`, and
the pattern types are in `Routing`. All come from the `Microsoft.AspNetCore.App`
framework reference. These standalone console hosts restore **zero NuGet
libraries**; they need no native assets, optional workloads, data provider,
browser, Blazor rendering mode or experimental opt-in. The actual property has
no ExperimentalAttribute. SDK/language release families and these reproduction
pins are separate: the example uses no C#15-only syntax.

The exact measured sets are SDK **10.0.402**, C# **14.0**, framework **10.0.13**
and SDK **11.0.100-rc.2.26504.105**, C# **15.0**, framework
**11.0.0-rc.2.26504.105**. Saved sample records contain build/run commands,
input hashes, empty resolved library graphs, actual Roslyn product versions and
hashes, loaded CoreLib/MVC/Routing paths, versions and hashes. Each execution
checks its three framework reference hashes against the bounded web audit.
This reconciles the actual assets independently from running the provider.

The .NET10 artifact has no such property. Its valid older route reparses MVC's
actual `ControllerActionDescriptor.AttributeRouteInfo.Template`, falling back to
provider-dependent `RelativePath` for the demonstrated Minimal endpoints. MVC's
displayed path has already lost constraints: parsing it cannot recover those.
Minimal's raw displayed template retains them in this fixture; that is not a
promise about all providers. Missing or malformed legacy text returns unavailable.
No package backport or target10 applicability route is advertised.

Execution covers managed Windows x64 with real local Kestrel. Other platforms,
third-party providers, conventional-controller discovery, trimming, NativeAOT and
deployment remain unqualified. Constraints select routes; they are not input
validation or authorization. Policy declarations may be strings or policy
objects; reporting an object's type does not serialize or execute it. Methods,
other endpoints, middleware and binding also affect actual request outcomes.

## Migration

1. Inventory the API explorer providers used by your tool. Keep the working older
   template-based extension when targeting .NET10; do not parse MVC display paths
   and label the result authoritative. Register the two built-in discovery paths
   and ensure endpoints are initialized before reading their collection.
2. On .NET11, read `description.RoutePattern` once. Return an unavailable result
   when it is null, even when `RelativePath` contains plausible text. Keep a
   deliberate older fallback separate if your tool needs one; the new extension
   does not silently downgrade accuracy.
3. Read `Parameters`, `Defaults` and parameter `ParameterPolicies`. Preserve
   `IsOptional` and `IsCatchAll` independently. A default value is not an optional
   marker: the station route supplies `pelagic` even though it has no `?` suffix.
   Keep an absent default null rather than an invented empty-string default.
4. Verify actual provider output and request matching together. Here `id` reports
   `int` and `min(1)`; `/specimens/7` succeeds and `0`/`not-a-number` return 404.
   Missing/present expedition seasons, empty/nested chart paths and default/
   explicit station basins must agree with the returned route values on both
   hosting styles. A handwritten description is only the null/legacy control.
5. Replace the fixture routes with your application routes and repeat discovery
   plus requests for each provider. Preserve nullable handling and test custom
   policies through the real router. Do not infer universal provider support,
   OpenAPI generation, GA readiness or a performance improvement from this report.

## Older approach: .NET 10 bounded template parsing

Create one standalone `MarineRoutes` directory with these files. `Program.cs`
starts and disposes its own loopback host, invokes the extension and fails on
any unexpected discovery, routing or artifact result. `MarineExplorer.cs` includes
the controller, extension/report and the reproducible artifact assertions.
The three empty Directory files isolate inherited MSBuild/package configuration.
The comment-delimited regions below are the canonical files, not pseudocode.
### global.json

{{snippet:n10-global-json}}

### MarineRoutes.csproj

{{snippet:n10-marineroutes-csproj}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### Program.cs

{{snippet:n10-program-cs}}

### MarineExplorer.cs

{{snippet:n10-marineexplorer-cs}}


## Newer approach: .NET 11 parsed contract

Create a separate directory with this complete set. The extension no longer
reparses any displayed URL; its null control deliberately contains plausible
legacy text to prove no implicit fallback occurs.

### global.json

{{snippet:n11-global-json}}

### MarineRoutes.csproj

{{snippet:n11-marineroutes-csproj}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### Program.cs

{{snippet:n11-program-cs}}

### MarineExplorer.cs

{{snippet:n11-marineexplorer-cs}}

## Evidence

Use the matching SDK installation from each sample directory. Disable first-run
PATH integration before using an isolated .NET home. These commands restore the
framework inputs during setup, build and execute locally; the sample opens only
a disposable loopback port and closes it in `finally`.

From the .NET10 sample directory, invoke `C:/Program Files/dotnet/dotnet.exe` with `build ./MarineRoutes.csproj -c Release`, then with `./bin/Release/net10.0/MarineRoutes.dll`. From the separate .NET11 directory, use `C:/Users/pingu/.dotnet-playbook-11/dotnet.exe` with the same build arguments, then `./bin/Release/net11.0/MarineRoutes.dll`. Set the child environment variable `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` before either command; restore the previous process value afterward. The repository evidence runner performs the locked build and execution with the explicit executable supplied by `-Dotnet`.

The `ArtifactIdentity` assertions intentionally require the recorded SDK layout,
framework versions and audited reference bytes. They are reproduction checks,
not general application prerequisites. Change them only after independently
qualifying another artifact set. Actual routing functionality is verified before
the artifact report, with no handcrafted replacement for built-in discovery.
Expect eight discovered route reports, 18 successful HTTP assertions per target,
three `AUDIT-MATCH` lines, loaded artifact/compiler identities and a final `PASS`.
Null returns unavailable. On .NET10 malformed fallback text also returns
unavailable; on .NET11 even valid-looking fallback text stays unavailable.

Saved compilation/execution records are current for these exact canonical inputs.
RC2 is not actual-GA proof. PIN-959 comprehensive content, PIN-964 actual-GA
reconciliation and existing Human gates stay open. This recipe preserves the
exact eight approved .NET10 foundations and all existing recipe/edition identities.

## Alternatives

For a runtime request, use the selected endpoint's routing metadata rather than
API discovery when appropriate. For a net10 tooling integration with richer
provider-owned source metadata, consume that actual source through a deliberate
provider-specific adapter. Do not attempt to recover discarded constraints from
a normalized display path. If the provider supplies neither contract nor usable
source, display unavailable and keep the original URL as presentation text.

## Sources

- [Nullable ApiDescription contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Mvc/Mvc.Abstractions/src/ApiExplorer/ApiDescription.cs).
- [MVC source-template and displayed-path behavior](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Mvc/Mvc.ApiExplorer/src/DefaultApiDescriptionProvider.cs).
- [Minimal endpoint provider](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Mvc/Mvc.ApiExplorer/src/EndpointMetadataApiDescriptionProvider.cs).
- [Routing fundamentals and limitations](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-11.0).
