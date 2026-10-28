# Choose metadata for application-owned circuit JSON

## Goal

Register generated metadata for application-owned payloads sent through Interactive Server JavaScript interop and DI protected browser storage when deliberately adopting the experimental circuit resolver hook. Keep reflection fallback and framework converters.

## Older approach

The .NET 10 circuit hook is absent. The application-owned ShowTicket and ExtraNote records use the supported reflection-based JSON behavior in JS interop and injected protected storage. This remains valid on .NET 11; generated metadata is an explicit experimental choice, not a required migration.

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `CedarPlanetarium.csproj`

{{snippet:n10-cedarplanetarium-csproj}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `PayloadPanel.razor`

{{snippet:n10-payloadpanel-razor}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `VisitState.cs`

{{snippet:n10-visitstate-cs}}

### .NET 10: `wwwroot/payload.js`

{{snippet:n10-wwwroot-payload-js}}

## Newer approach

Configure the two resolvers in registration order. The framework inserts them before its reflection fallback in both RemoteJSRuntime and scoped protected-storage options. TicketResolver binds generated metadata to the actual options passed by the framework; it does not replace converters or reuse a context with incompatible options. The diagnostic second resolver makes incorrect precedence visible. The page performs both round trips only from interactive events.

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `CedarPlanetarium.csproj`

{{snippet:n11-cedarplanetarium-csproj}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `PayloadPanel.razor`

{{snippet:n11-payloadpanel-razor}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `TicketResolver.cs`

{{snippet:n11-ticketresolver-cs}}

### .NET 11: `VisitState.cs`

{{snippet:n11-visitstate-cs}}

### .NET 11: `wwwroot/payload.js`

{{snippet:n11-wwwroot-payload-js}}

## Migration

1. Create all CedarPlanetarium files below, including wwwroot/payload.js, as one web project per release. Use the matching pinned SDK and runtime when reproducing proof.
2. Register generated metadata at startup with the narrowly scoped ASPNETCORE9004 acknowledgement. Leave unhandled types to reflection. Obtain protected storage from DI.
3. From the sample directory run `dotnet run --configuration Release --urls http://127.0.0.1:6259`; open that address and wait for the ready state. No external service, secret, workload or native SDK is required.
4. Click Round trip. Expect `passed:Cedar-<scope>:7`. Open another independent tab at the same address: it starts with zero runs and no stored ticket, then saves its own scope value.
5. Run `eng/Record-CedarPayload.ps1` with installed Chromium and the actual .NET executables for compilation and the real two-target browser checks. Expect first-only ShowTicket resolution across four options instances, first-then-second ExtraNote fallback, unchanged values, retained ElementReference conversion and rejected ciphertext tampering.

## Constraints

Cedar Planetarium sends ShowTicket through both real paths. TicketJson supplies generated metadata only for that record; ExtraNote deliberately falls through both resolvers to reflection. The first registered resolver wins; the second throws if asked to own ShowTicket.

Use injected ProtectedSessionStorage/ProtectedLocalStorage from framework DI. Manually constructing a storage instance does not use the configured resolver chain. Configure once at startup, before options are cached; do not put per-user mutable state in resolvers.

Resolvers return null for unhandled types. Preserve framework converters and reflection fallback. The sample passes a real ElementReference alongside each JS payload as a converter control; generated metadata does not make all framework messages source generated.

Protected session storage is server-side Data Protection over per-tab browser session storage. It is not authorization, a cross-tab/session data service or a guarantee against deletion/replay. Browser access is unavailable during prerender.

Ephemeral keys keep the demo disposable and create no persistent key ring. Restarting the host loses decryption ability. A production server needs its own reviewed persistent key lifecycle; this recipe does not deploy one.

The bounded trace observes options identities and type names, caps history at 64 entries and retains no payloads. It is test instrumentation. Distinct options/circuit state does not promise that configured resolver instances are per-circuit. Concurrent resolvers must be safe for shared use.

Server-side Blazor does not currently support trimming or NativeAOT. This is not a circuit-protocol, AOT, zero-reflection, external-provider or production-security qualification.

Real Interactive Server circuits after activation, same-origin JavaScript and server-side DI ProtectedSessionStorage; no prerender interop, WASM client, full circuit protocol or NativeAOT support. Local #pragma warning disable/restore ASPNETCORE9004 around JsonTypeInfoResolvers registrations. The API is experimental and EditorBrowsable(Never); no project-wide suppression.

## Evidence

Record-CedarPayload.ps1 captures exact compilation inputs/dependencies, then Test-CedarPayload.mjs runs both real local servers in Chromium with two separate circuits/tabs each. ShowTicket and ExtraNote round-trip over JS interop and DI protected session storage. Trace assertions distinguish four independent serializer options instances in RC2, preserve registration order and exercise reflection fallback; each app retains its own scope values and stored ticket. Ciphertext conceals the payload and an altered ciphertext is rejected by real Data Protection. Compile-only evidence is separate from saved execution records. These checks do not claim exhaustive security or production key management.

The exact test sets are .NET 10/C# 14, SDK 10.0.402, framework 10.0.13, and .NET 11/C# 15, SDK 11.0.100-rc.2.26504.105, framework 11.0.0-rc.2.26504.105. These independently pinned consumers are separate from the supported prerequisites above. No native assets or workloads are required. RC execution does not establish GA verification. The .NET 10 comparison adds no catalogue foundation or edition. PIN-959 and PIN-964 remain open.

Official source support:

- [Previous circuit options](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Server/src/CircuitOptions.cs)
- [Experimental resolver registration](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Server/src/CircuitOptions.cs)
- [Real remote interop resolver chain](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Server/src/Circuits/RemoteJSRuntime.cs)
- [Scoped storage serializer options](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Server/src/ProtectedBrowserStorage/ProtectedBrowserStorageJsonSerializerOptions.cs)
- [DI storage construction and trimming limitation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Server/src/DependencyInjection/ComponentServiceCollectionExtensions.cs)
- [Protected storage implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Server/src/ProtectedBrowserStorage/ProtectedBrowserStorage.cs)
