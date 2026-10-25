# Share access requirements across endpoints, views and hub methods

## Goal

A fictional jetty manifest is visible and callable only to a crew member holding the launch claim. Reuse the access rule across a Minimal API, MVC action, SignalR method, authorization view and routed component, then verify agreement for permitted and denied authenticated users.

## Older approach — .NET 10

A named policy is a valid cross-stack solution. The older CrewPermitAttribute sets Policy to crew-permit, and startup registers the handler and requirement once. All listed surfaces, including legacy MVC, use that policy name. This comparison does not claim .NET 10 lacks endpoint requirement metadata; it avoids relying on its newer cross-stack interpretation.

#### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### .NET 10: `global.json`

{{snippet:n10-global-json}}

#### .NET 10: `JettyAccess.csproj`

{{snippet:n10-jettyaccess-csproj}}

#### .NET 10: `Permit.cs`

{{snippet:n10-permit-cs}}

#### .NET 10: `PolicyMetadata.cs`

{{snippet:n10-policymetadata-cs}}

#### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

#### .NET 10: `Proof.cs`

{{snippet:n10-proof-cs}}

#### .NET 10: `Surfaces.cs`

{{snippet:n10-surfaces-cs}}

## Newer approach — .NET 11

CrewPermitAttribute now contributes the requirement directly while retaining IAuthorizeData semantics. Endpoint middleware, component metadata evaluation and hub-method authorization use the shared metadata combination. The registered named policy remains solely as the explicit legacy MVC fallback. The complete executable fixture includes a reusable view adapter, a route view, real hub calls and the new metadata overload; run the project to see each decision.

#### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### .NET 11: `global.json`

{{snippet:n11-global-json}}

#### .NET 11: `JettyAccess.csproj`

{{snippet:n11-jettyaccess-csproj}}

#### .NET 11: `Permit.cs`

{{snippet:n11-permit-cs}}

#### .NET 11: `PolicyMetadata.cs`

{{snippet:n11-policymetadata-cs}}

#### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

#### .NET 11: `Proof.cs`

{{snippet:n11-proof-cs}}

#### .NET 11: `Surfaces.cs`

{{snippet:n11-surfaces-cs}}

## Migration

1. Keep production authentication and register the requirement handler. Inspect whether MVC uses endpoint routing before changing attributes.
2. For supported .NET 11 paths, return the requirement from an IAuthorizationRequirementData attribute and apply that same metadata to endpoints, actions, hub methods and components as shown. Retain named policies for legacy MVC.
3. Use object-typed metadata when calling the new CombineAsync overload yourself. Preserve named-policy AuthorizeView usage or use the explicit PermitView adapter; do not change its Policy parameter type.
4. Create the complete standalone old/new file sets and build with the matching SDK in each directory. Run the resulting DLL: denied requests must be 403, allowed requests 200, hub results and rendered views must agree, and anonymous API access must be 401.
5. Keep the legacy negative control isolated. Confirm the named fallback rejects the denied user before migrating any real legacy application.

## Constraints

Keep the same server-side requirement handler across every protected operation. Hiding a view never secures a callable API or hub. Production authentication must establish trusted claims; never deploy the sample X-Probe-Permit header scheme.

Legacy MVC with EnableEndpointRouting=false is unchanged. The new attribute by itself only triggers the default authenticated-user policy on that path: the denied-but-authenticated probe receives 200. Retain the named crew-permit policy on /manifest/legacy-safe, which returns 403. That deliberate negative control must not become a production endpoint.

AuthorizeView.Policy remains a string policy name. PermitView overrides the public protected GetAuthorizeData hook and returns the same dual-interface attribute. Ordinary AuthorizeView can keep a named policy; AuthorizeRouteView reads the attribute on the routed component. Do not invent an object-valued Policy parameter or put component authentication schemes on the metadata.

The new CombineAsync overload accepts IEnumerable<object>. An IAuthorizeData-typed overload retains its older semantics and does not inspect requirement metadata. The helper deliberately uses object[]; the executable check verifies one CrewPermit requirement from the dual-interface attribute.

The focused proof starts ephemeral loopback Kestrel hosts, sends real allowed/denied HTTP and WebSocket SignalR invocations, and renders real components with isolated authentication-state scopes. It proves these server decisions and rendered strings, not interactive browser routing, client trust, resource-based authorization or all policy composition combinations.

## Evidence

Both complete original consumers compile with their declared SDK/target/language. Saved compilation records retain exact inputs and artifacts. `eng/Test-JettyGuidance.ps1` separately builds and executes both JettyAccess consumers. No fake authorization or mocked UserManager replaces the framework decision. The header credential and memory store are test fixtures, not production security or persistence. The report `docs/verification/PIN-809-810.md` records requests/rendered output, actual clock boundaries, failed initial assumptions and evidence limits. .NET 11 RC2 execution is not GA verification. These .NET 10 sources are comparisons, not a ninth curated foundation or a declared older-target route.

- [Consistent authorization metadata release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#consistent-authorization-metadata-across-the-stack)
- [Pinned metadata combination and overloads](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Security/Authorization/Core/src/AuthorizationPolicy.cs)
- [Pinned component metadata evaluation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Authorization/src/AuthorizeViewCore.cs)
- [Pinned component route metadata](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Authorization/src/AttributeAuthorizeDataCache.cs)
- [Pinned legacy MVC filter path](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Mvc/Mvc.Core/src/ApplicationModels/AuthorizationApplicationModelProvider.cs)
