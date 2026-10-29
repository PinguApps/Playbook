# Give a generated static endpoint deliberate route precedence

## Goal

Use this when two matching routes can serve different static resources and you
need a deliberate winner. Solmere Gallery has an archived amber exhibit card and
a curator's newer label. Both can occupy `/exhibits/amber.html`; selecting a route
must select the intended file, not just produce an attractive endpoint table.

For an ordinary application, keep the SDK's generated manifest and configure the
public endpoint convention. For a build integration that owns a separate generated
manifest, .NET 11 also exposes `StaticAssetDescriptor.Order`. Set that nullable
**string** before serializing and mapping the separate manifest. No internal
factory or data-source API is a consumer prerequisite.

## Requirements: supported boundary and exact reproduction

`MapStaticAssets` documents an explicit manifest path. A relative path is resolved
against `AppContext.BaseDirectory`; the examples pass a full temporary path. The
Web SDK discovers both physical HTML files and produces
`AmberGallery.staticwebassets.endpoints.json`. Each fixture copies its actual
version-1 `Build` envelope and selects the two uncompressed descriptors, retaining
`AssetFile`, selectors, endpoint properties and response headers. The label's route
is projected onto the exhibit's route to create an intentional competition.

On .NET 11 the projection uses public descriptor deserialization, setters and
serialization. On .NET 10 the order is supplied through public conventions.
`MapStaticAssets` then creates and executes the real framework endpoints. This
qualifies this generated-manifest integration on the measured version; the path
overload alone does not promise a stable arbitrary JSON authoring format. Do not
hand-edit files in `obj` or overwrite an SDK-generated output manifest. In a normal
app, apply the convention to `app.MapStaticAssets()` instead of making a copy.

The projects below are complete standalone reproductions. Their empty
`Directory.*` files deliberately stop repository build/package inheritance.
Keep each target in a separate directory, including both `wwwroot` subdirectories.
The exact SDK/reference/runtime/compiler/native-host identities are checked and
printed by `ArtifactIdentity`; these are **test pins**, separate from the .NET 11
release-family requirement for descriptor order. Only Windows x64 execution is
qualified. No packages, extra workloads, provider service, browser or rendering
mode is needed. The SDK's own static-web-assets task is used; no internal assets
package is added. SDK 10.0.402 uses C#14 and framework10.0.13; SDK
11.0.100-rc.2.26504.105 uses C#15 and framework11.0.0-rc.2.26504.105.
The feature needs no experimental switch, and uses no C#15-specific syntax.
RC2 proof is not GA proof; PIN-959/PIN-963/PIN-964 and other Human gates stay open.

## Constraints: choose the order and check the file

Lower endpoint `Order` wins among candidates that match. In this fixture the label
endpoint has order `0`; the exhibit endpoint changes as follows:

| .NET 11 descriptor input | Actual order | GET result |
| --- | --- | --- |
| Absent, `null`, or `""` | `-100` | Archived exhibit HTML, 200 |
| `"-200"` | `-200` | Archived exhibit HTML, 200 |
| `"50"` | `50` | Curator label HTML, 200 |
| `"0"` | `0` | Equal literal candidates: ambiguity, 500 |
| `"curator-first"` or `"2147483648"` | `-100` | Invalid text/Int32 overflow silently falls back; exhibit HTML, 200 |
| JSON number `50` | No endpoint | Manifest parsing throws `JsonException` |

Invalid strings do **not** fail startup on the measured framework. Validate
author-owned order values as invariant-culture Int32 strings before shipping a
manifest. The invalid inputs here are deliberate negative controls. A JSON number
is invalid because the public property is a string; `null` and omission are valid.

Endpoint order is distinct from route-template precedence. HTTP methods, route
specificity, constraints and selector policies still participate in selection.
Do not use a tie as an ordering policy or treat this as an authorization rule.
This example has two identical literal routes, no route constraints and no
short-circuit middleware. It does not teach catch-all/fallback ordering or make
claims about arbitrary compressed variants. The fixture selects uncompressed
descriptors only. Apply a real application's policy consistently to all affected
variants, and verify its negotiation separately.

Descriptors become frozen when the framework constructs endpoints, **before**
endpoint conventions run. Changing the descriptor from a convention or after
enumerating endpoint metadata throws `InvalidOperationException`. A convention
instead changes the mutable `RouteEndpointBuilder.Order`. Both projects verify a
rejected write on actual framework metadata; the newer project sets `Order`, and
the older one sets its existing `Route` property. An unfrozen .NET 11 descriptor is
mutable while preparing the isolated manifest.

## Migration

1. Identify the overlapping URL, HTTP methods and actual competing resource.
   Keep the SDK-generated manifest and physical web-root files as the starting
   inputs. Do not make an order choice from file names or registration order.
2. For ordinary application configuration, use `MapStaticAssets().Add(...)`,
   select the intended asset metadata and change `RouteEndpointBuilder.Order`.
   The `convention-wins`/`convention-yields` branches below demonstrate this on
   both actual frameworks. That older approach remains valid on .NET 11.
3. Only when owning the generated-manifest integration, build with SDK11, select
   descriptors from the generated JSON, assign strings such as `"-200"` or `"50"`
   before serialization, preserve the generated envelope/asset metadata, write
   an isolated manifest and pass its full path to public `MapStaticAssets`.
   Build assets and metadata together; cached headers must describe the real bytes.
4. Copy every file below, build each project from its own directory and execute
   its built DLL. The harness starts local ephemeral Kestrel hosts, requests the
   shared route, compares the full response with the chosen physical file, and
   checks empty HEAD bodies. It prints generated/projected JSON and SHA-256
   identities and checks that mapping did not modify either input manifest.
5. Run the negative controls. On .NET11, expect default/explicit/fallback results
   in the table, numeric JSON rejection, a captured ambiguity for the equal-order
   control and frozen-write rejection. On .NET10, expect ignored `Order` JSON to
   remain at `-100`; its reference contract has no `Order` property. Use conventions
   there, rather than pretending the new property was backported.
6. Keep your production manifest lifecycle separate from this temporary fixture.
   It deletes only its own temporary copies after disposing each host. A publish
   layout, incremental build integration, compression policy, NativeAOT/trimming,
   another OS or GA runtime needs its own relevant checks. This is not deployment
   or universal schema compatibility evidence.

For the repository reproduction, select this exact worktree first:
`Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1176-04-101-give-a-generated-static-endpoint-deliberate-route'`.
Select `samples/net10.0/AmberGallery` or `samples/net11.0/AmberGallery` before
invoking its SDK. For standalone copies, select the corresponding copied directory.
Set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` in the child process, build
`AmberGallery.csproj` in Release configuration, then execute its built DLL under
`bin/Release/net10.0` or `bin/Release/net11.0` with the matching absolute .NET
executable. The build acquires the generated manifest. Both hosts stop themselves.

Expect `PASS AmberGallery` on each target. Source hashes of the StaticAssets
references match the PIN-709 audit; compiler, loaded runtime and native-host hashes
identify the actual execution separately. Package closures are empty. No normal
application is required to carry the artifact-identity check; it makes this exact
reproduction fail honestly if its measured prerequisites differ.

## Older approach: .NET 10

The older project gives the competing label order0 through a public convention,
then chooses exhibit order-200 or50 in that same convention. The Order JSON
control is ignored on this actual framework. Copy every file at its relative path.


### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `AmberGallery.csproj`

{{snippet:n10-ambergallery-csproj}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `ArtifactIdentity.cs`

{{snippet:n10-artifactidentity-cs}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `wwwroot/exhibits/amber.html`

{{snippet:n10-wwwroot-exhibits-amber-html}}

### .NET 10: `wwwroot/labels/amber.html`

{{snippet:n10-wwwroot-labels-amber-html}}

## Newer approach: .NET 11

The new project sets public descriptor Order before serialization and public
mapping. The same endpoint convention alternative remains available. Its input
and failure controls deliberately include values applications should reject.
Copy every file at its relative path, preserving both physical assets.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `AmberGallery.csproj`

{{snippet:n11-ambergallery-csproj}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `ArtifactIdentity.cs`

{{snippet:n11-artifactidentity-cs}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `wwwroot/exhibits/amber.html`

{{snippet:n11-wwwroot-exhibits-amber-html}}

### .NET 11: `wwwroot/labels/amber.html`

{{snippet:n11-wwwroot-labels-amber-html}}

## Evidence

Saved compilation-and-run records use `eng/Record-Evidence.ps1` independently
with each sample project and its pinned SDK executable. The current consumer
executes eleven scenarios (ten real GET/HEAD pairs plus numeric JSON rejection);
the previous consumer executes four GET/HEAD pairs. Every successful GET compares
the entire physical file, and every successful HEAD checks an empty body. The
equal-order control captures the actual routing ambiguity and returns HTTP500.
Default, invalid-string and overflow cases return the archived exhibit;
explicit yield returns the curator label. The descriptor write fails after real
endpoint construction. Generated/projected input hashes remain unchanged.

Installed ordinary-package `list_recipes`/`get_recipe` inspection checks full
canonical Markdown, ID/title/goal, explicit target11/edition11, applicability,
fresh compilation evidence and target10's unavailable-edition response. The
returned Markdown is then the sole source for reconstructing both projects under
an unrelated temporary directory. They build with no remote NuGet feeds and run
all manifest/HTTP/failure checks. Inspection proxies establish proxy independence,
not operating-system network isolation. This .NET10 comparison adds no new
foundation or older-target descriptor route. No provider/native-device/production,
publish-layout, browser or actual-GA behavior is inferred.
