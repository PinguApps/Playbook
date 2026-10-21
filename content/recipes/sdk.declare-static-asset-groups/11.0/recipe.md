# Publish selected static asset groups and compiled TypeScript

## Goal

Publish a pier website's selected crew assets and generated berth script. Use the Static Web Assets SDK's group and extra-endpoint contracts when authoring SDK/component infrastructure.

## Older approach

SDK 10 publishes ordinary wwwroot files without the newer group selection contract. An explicit target can compile TypeScript and register the generated content before static-asset discovery:

{{snippet:older}}

Create Scripts/berths.ts as shown below, wwwroot/crew/index.html containing an original crew page, and tsconfig.json with `{ "compilerOptions": { "target": "es2020", "module": "es2020", "strict": true, "outDir": "wwwroot/bin" }, "files": ["Scripts/berths.ts"] }`. The generated bin directory is a build-output boundary, not authored JavaScript. Microsoft.TypeScript.MSBuild supplies the compiler; Node.js is required. This explicit earlier hook is valid for the comparison. It does not invent an old StaticWebAssetGroup equivalent.

## Newer approach

For standalone C# projects, add `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>` inside the project PropertyGroup. The canonical comparisons obtain these settings from their isolated shared sample properties; do not depend on the Playbook checkout to supply them.

SDK 11 connects Microsoft.TypeScript.MSBuild output to static-asset discovery and adds group-definition, group-selection and additional-endpoint item contracts:

{{snippet:newer}}

The Audience definition tags crew/** with Audience=crew. SourceId ties definitions/selections to this project's PackageId. Order must be an integer and unique within that source. IncludePattern matches asset-relative paths, not arbitrary filesystem paths. StaticWebAssetGroup selects the matching value for publication; changing PierAudience to visitor changes the selected endpoints while leaving ungrouped generated JavaScript available. The measured RC2 publish still contains the physical crew HTML.

The extra endpoint definition uses Pattern and Replacement; the ** stem in **/index.html lets the SDK map crew/index.html to crew. The ordinary file endpoint remains. This is a manifest/endpoint contract, not an authorization rule or SPA fallback implementation.

{{snippet:typescript}}

Use the same tsconfig and crew page as above. The minimal host maps the generated static endpoints:

{{snippet:program}}

Pin global.json to SDK 11.0.100-rc.2.26504.105 for this original RC2 build. From the project directory publish with `dotnet publish WebAssets.csproj -c Release -p:NodePath="<absolute-node-executable>" -o <fresh-output-directory>`. The result must contain wwwroot/bin/berths.js and wwwroot/crew/index.html; WebAssets.staticwebassets.endpoints.json includes the crew route.

For the library comparison, create ../RclAssets/RclAssets.csproj. Its older SDK-10 route keeps the explicit hook:

{{snippet:rclolder}}

The SDK-11 Razor library relies on the supported integration:

{{snippet:rcl}}

Give the library its own tsconfig with the same compiler options but files=["Scripts/deck.ts"], and compile this independent source:

{{snippet:deck}}

The host project reference publishes the result at wwwroot/_content/Pier.Deck.Assets/bin/deck.js. The PackageId determines that base path. Keep authoring configuration/evidence out of publish output to avoid duplicate metadata files from the library and host.

## Migration

1. Establish whether you are extending SDK/component metadata or simply publishing ordinary web content. Most applications should retain the ordinary wwwroot contract.
2. Pin Microsoft.TypeScript.MSBuild 5.9.3 and enable its NuGet target. Keep TS inputs and tsconfig in source control; generate JavaScript before discovery. If central package management applies, move the pin into that sample's central file or explicitly isolate it with ManagePackageVersionsCentrally=false.
3. Under SDK 11, remove a redundant old registration target after verifying clean and repeated publish. Declare group definitions and selections with matching SourceId/name/value; use deterministic Order and asset-relative patterns.
4. Add extra endpoints only for the intended pattern. Inspect the actual publish manifest's AssetGroups and the endpoint manifest; do not infer a route from the item name.
5. Publish again to a separate empty directory with `-p:PierAudience=visitor`. The crew endpoints should disappear; generated ungrouped JavaScript should remain. The recorded RC2 artifact still contains the physical crew HTML, so inspect both endpoints and files. Groups influence SDK asset metadata; they do not authenticate visitors.
6. For Razor class libraries, the same static-assets infrastructure integrates compiled TypeScript. Package consumers may require their own group conventions (StaticWebAssets.Groups.targets) and project-mode/base-path handling. The original project-reference consumer above verifies RCL JavaScript publication. A NuGet-packaged RCL consumer is not claimed.

## Constraints

Target net11.0/C#15 and the .NET 11 Static Web Assets SDK are the declared route; the SDK-10/net10.0/C#14 project is the older comparison. The script is TypeScript compiled to ES2020 JavaScript with Microsoft.TypeScript.MSBuild 5.9.3 and Node.js (actual recorded host version 24.21.0). Package build targets/compiler assets matter, not an application runtime TypeScript package. These are low-level SDK contracts; check final GA definitions before extending a production SDK. No database/provider, Blazor rendering mode, mobile workload or experimental API is required. This example proves published files/metadata, not browser rendering, HTTP caching or access control. Do not treat TypeScript rebuild fixes alone as a separate recipe.

## Evidence

Both actual project targets compile and publish from clean generated-output states. SDK 11 produces Audience=crew metadata and the captured-stem crew endpoint; changing the selection removes the grouped endpoints while retaining compiled JavaScript. The older explicit discovery hook produces its intended files too. Both hosts also publish the actual RCL-generated deck.js through a project reference. The TypeScript code is actually compiled by the pinned package, not copied from a handwritten JavaScript stub. Evidence records exact SDK, compiler package and Node.js baseline. Real installed stdio list/get verifies full body, canonical project/host/TypeScript, applicability and target-11 edition offline. No NuGet RCL-package-consumer, browser, native-platform, mock or GA execution claim follows.
