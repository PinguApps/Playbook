# Start a callable web tool from the SDK-bundled MCP template

## Goal

A harbour assistant needs one callable operation that returns a berth's load limit. Generate a separate example web server, then replace its demonstration with the original lookup below. This application is independent of Playbook's read-only stdio documentation server.

## Migration

1. Select the pinned SDK in an empty directory. Disable first-run PATH integration with `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`. Run `dotnet new mcpserver --transport remote --self-contained false --no-restore --no-update-check --name BerthTools --output BerthTools`. No separate template installation is required.
2. Inspect the generated project before restoring. Both measured SDK archives emit `ModelContextProtocol.AspNetCore 2.1.0`; keep that baseline rather than guessing a version from the .NET release number. The remote option is deliberate: the default template transport is local stdio.
3. Replace generated `Program.cs` with the matching original file below and add `BerthLookup.cs`. The unused random-number demonstration can be removed. Keep `WithHttpTransport`, stateless mode and `MapMcp`; register the original class with `WithTools<BerthLookup>()`.
4. The generated project uses an explicit package version. Either retain that outside CPM, as the generation check does, or use the complete canonical project/central-package boundary below. Do not mix an explicit reference with a parent rule that forbids overrides.
5. Build, then run with `dotnet run --no-launch-profile -- --urls http://127.0.0.1:5094` from the generated directory. Connect an MCP Streamable HTTP client to `http://127.0.0.1:5094/`. Initialize, send the initialized notification, list tools and call `load_limit` with `{"berth":"W3"}`. Its text result is `W3 permits 18 tonnes.`; `X9` returns `No load limit is registered for that berth.`. Stop the server after the check.

## Requirements

The cumulative .NET 11 release note describes the move into the SDK. Actual SDK **10.0.402** also contains a bundled MCP template archive and generates this server in a fresh CLI home without installing anything. The .NET 10 sample is therefore the same valid approach targeting net10.0, not an invented manual-template workaround. This observation establishes those exact baselines; it does not date the earliest servicing release or imply every .NET 10 SDK bundles it. The catalogue retains edition 11.0 and does not add a ninth .NET 10 foundation.

The template's packaging status is separate from the library API and framework status. These managed examples need no preview language switch or experimental suppression. Evidence uses .NET 11 RC2 and package 2.1.0; later package/GA claims need fresh checks. Public hosting would require its own authentication and authorization design; this loopback demonstration provisions and publishes nothing.

## Older approach: .NET 10

Save these at the shown paths under an empty working directory. Keep the directory boundary files; they prevent unrelated parent build rules from changing this example.

### `samples/net10.0/BerthTools/Directory.Build.props`

{{snippet:n10-directory-build-props}}

### `samples/net10.0/BerthTools/Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### `samples/net10.0/BerthTools/Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### `samples/net10.0/BerthTools/BerthTools.csproj`

{{snippet:n10-berthtools-csproj}}

### `samples/net10.0/BerthTools/Program.cs`

{{snippet:n10-program-cs}}

### `samples/net10.0/BerthTools/BerthLookup.cs`

{{snippet:n10-berthlookup-cs}}

### `samples/net10.0/BerthTools/global.json`

{{snippet:n10-global-json}}

## Newer approach: .NET 11

Save these at the shown paths under an empty working directory. Keep the directory boundary files; they prevent unrelated parent build rules from changing this example.

### `samples/net11.0/BerthTools/Directory.Build.props`

{{snippet:n11-directory-build-props}}

### `samples/net11.0/BerthTools/Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### `samples/net11.0/BerthTools/Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### `samples/net11.0/BerthTools/BerthTools.csproj`

{{snippet:n11-berthtools-csproj}}

### `samples/net11.0/BerthTools/Program.cs`

{{snippet:n11-program-cs}}

### `samples/net11.0/BerthTools/BerthLookup.cs`

{{snippet:n11-berthlookup-cs}}

### `samples/net11.0/BerthTools/global.json`

{{snippet:n11-global-json}}

## Constraints

SDK-bundled mcpserver: no dotnet new install is needed on either measured SDK 10.0.402 or 11.0.100-rc.2.26504.105. This corrects the historical release-note implication of .NET 11 exclusivity; earliest servicing introduction was not established.

Choose --transport remote explicitly: the template defaults to local stdio. The measured route uses stateless Streamable HTTP, framework-dependent managed execution and ModelContextProtocol.AspNetCore 2.1.0. Neither AOT nor self-contained publication is claimed.

The SDK template emits an explicit package version. The canonical repository sample uses an isolated central version file and a versionless reference to keep the same dependency. Review generated references when moving it under another CPM boundary.

Loopback demonstration only; the sample has no authentication, authorization or internet exposure. Production hosting security and resource access require separate design. No package publication, account or remote service provisioning is required.

This is an example agent-facing application, separate from Playbook's installed stdio documentation host. Template availability, MCP library maturity and .NET RC verification are separate facts.

## Evidence

The exact SDK, language and package inputs are recorded with the samples. Saved compilation records establish the canonical code builds; the separate repeatable `eng/Test-WebTooling.ps1` command establishes real CLI/generated-server behavior. No mocks or cross-compilation are used. Installed-package checks independently compare the entire expanded body and applicability over stdio from an unrelated directory; that delivery check does not replace the runtime checks. GA reconciliation and comprehensive product publication gates remain outstanding.

- [Bundled template release note](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#mcp-server-template-ships-with-the-net-sdk)
- [Official MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Template transports and extension points](https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/build-mcp-server)
- [Verified package baseline](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore/2.1.0)
