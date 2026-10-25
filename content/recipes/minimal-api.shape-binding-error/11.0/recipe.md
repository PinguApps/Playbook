# Return a consistent explanation before a bound endpoint can run

## Goal

A parcel depot accepts a numeric dock and slot count. An unreadable value must return a predictable ProblemDetails code without allocating any intake work. Put this check at filter entry, where the framework has already attempted scalar binding. It is distinct from a handler deliberately returning a business error.

## Requirements

Use the .NET 11 ASP.NET Core shared framework/reference pack and SDK for the newer consumer. The previous consumer targets .NET 10. Exact SDK and language pins are included in every complete directory; no NuGet packages, provider account, workloads, native tools or browser renderer are required. Restore framework assets before offline builds. The snippets below contain all source and configuration needed to build and run each self-checking application. Run from inside the directory containing its global.json. Neither example is a persistent public server: it opens ephemeral loopback Kestrel ports and stops them automatically.

## Older approach — .NET 10

The .NET 10 consumer already supports this goal with a registered endpoint filter. Keep that implementation when it fits the application. Both the official v10.0.0 source and the actual 10.0.13 runtime support the scalar-failure path. There is no reason to invent manual parsing or describe the older code as broken. The complete older consumer below makes the same three bad requests, valid request and domain-error control as the newer one.

#### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### .NET 10: `ParcelIntake.csproj`

{{snippet:n10-parcelintake-csproj}}

#### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

#### .NET 10: `Proof.cs`

{{snippet:n10-proof-cs}}

#### .NET 10: `global.json`

{{snippet:n10-global-json}}

## Newer approach — .NET 11

The .NET 11 consumer retains the same technique. After a failed numeric conversion or missing required query value, its filter observes status 400 before calling next. It returns the depot-specific ProblemDetails result immediately. The handler counter stays zero across those failures. Valid values continue once; a handler-produced capacity_closed error remains unchanged. The later release-note heading does not establish first availability.

#### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### .NET 11: `ParcelIntake.csproj`

{{snippet:n11-parcelintake-csproj}}

#### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

#### .NET 11: `Proof.cs`

{{snippet:n11-proof-cs}}

#### .NET 11: `global.json`

{{snippet:n11-global-json}}

## Migration

1. Create every file for one standalone target directory, retaining the isolated Directory files and its SDK pin. Restore the matching ASP.NET Core reference pack during preparation.
2. Register ThrowOnBadRequest=false before building the Development app. Add the filter to the endpoint you own; inspect status before reading arguments or invoking next.
3. Return the application error result directly for the observed binding failure. Let successful binding proceed, and preserve domain errors returned by the handler.
4. Run the complete proof. Three scalar failures must return application/problem+json with code intake_binding_failed and no handler calls. Then check a valid request and the separate capacity_closed result.
5. Keep the malformed-JSON, unsupported-content-type and throw-enabled controls. Add a separate error-writing policy if the application needs to shape those earlier failures; do not infer that this filter covers them.

From the chosen standalone directory, build and run the declared project:

- `$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'`
- `dotnet build ./ParcelIntake.csproj -c Release`
- `dotnet ./bin/Release/net11.0/ParcelIntake.dll`

For the previous directory, change only the run path to `net10.0`. Install the exact SDK declared in that directory and the matching ASP.NET Core runtime first. These commands prepare/run a local consumer; Playbook never executes them for the caller. Expect a final `PASS: ParcelIntake` line and the request observations below.

## Constraints

Set RouteHandlerOptions.ThrowOnBadRequest=false explicitly in Development. Otherwise BadHttpRequestException leaves binding before a filter can shape the failure. This setting is the normal non-Development default; it does not suppress arbitrary handler exceptions.

Inspect HttpContext.Response.StatusCode==400 at filter entry, before next(context). Failed arguments may contain defaults or nulls: do not dereference or repair them, call business services or call next on that path. A 400 produced later by the handler is a separate domain decision and must retain its own body.

Scope the filter to the endpoints whose binding-error contract you own. A status code is not a typed diagnostic: an earlier component could also set 400. Do not install another earlier status writer and assume every 400 means numeric binding failed.

In the tested .NET 10/11 RequestDelegateFactory path, malformed JSON and unsupported content types return before the filter: empty 400 and 415 respectively in this app. No ProblemDetails service or status pages are configured in the control. Use a separately configured ProblemDetails/status-code middleware policy for broader body-read failures; this filter is not universal exception or JSON error handling.

The original .NET 10 comparison uses the same valid filter technique and passes the same actual requests. No older workaround is required. The .NET 10 source already contains scalar-failure filter invocation. The .NET 10 directory is comparison evidence, not an additional curated foundation or advertised target-10 route.

Numeric route values must reach binding: a route constraint such as {dock:int} can reject the route earlier with 404. These samples deliberately use {dock}. Binding failures are different from model/business validation; no automatic validation service is enabled.

## Evidence

Each target performs eight real requests across two ephemeral Development hosts: three scalar failures, valid arguments, a handler domain error, malformed JSON, wrong content type and the throw-enabled control. The checks distinguish filter/handler counters and response contracts. Malformed JSON returns an empty 400 before its filter on both runtimes; wrong media remains 415. No handler runs on failed binding.

Saved compilation records identify exact source hashes, SDK/target/language, dependency inventory, host and build output. `eng/Test-EndpointGuidance.ps1` separately rebuilds and executes all four consumers and records artifact hashes. Source inspection, actual compilation and real requests are distinct evidence; none is a mock, cross-compilation or GA claim. The packed-tool test compares the entire independently expanded Markdown and applicability through actual installed stdio list/get. See `docs/verification/PIN-815-816.md` for the authoring receipt. Only edition 11.0 is served: the .NET 10 code remains a comparison, not a ninth foundation.

- [Official release-note description and Development prerequisite](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#endpoint-filters-observe-parameter-binding-failures)
- [Pinned RequestDelegateFactory scalar checks and earlier JSON read path](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Http/Http.Extensions/src/RequestDelegateFactory.cs)
- [Official .NET 10 source already invokes scalar-failure filters](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Http/Http.Extensions/src/RequestDelegateFactory.cs)
- [Binding sources and distinct failure statuses](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-10.0#binding-failures)
