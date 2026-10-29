# Copy a map's intermediate nodes before changing its output

## Goal

A fictional map publisher retains the lowered coastal map for comparison while producing a ridge edition. Its original `MapNode` wraps actual Razor HTML nodes. Copy the writable tree before changing the layer, colour or markup; replacing a child collection alone still leaves its child objects shared.

This is an advanced, version-coupled compiler-host example. It is not a new application rendering API. The current `CloneNode` is a **protected shell hook**, not a public deep-copy method. The framework's recursive `Clone` entry point is internal. The public `CopyForTransform` below owns a deliberately small copy policy instead of calling private compiler APIs.

## Migration

1. Pin a coherent SDK tooling set. Both complete projects reference the selected SDK's Razor compiler and utility from `source-generators`, plus its matching Roslyn binaries. Do not substitute the audit's unrelated Razor.Language 6.0 metadata helper or combine DLLs from different SDKs. From each project directory, run `dotnet build MapCompiler.csproj -c Release` with that project's matching SDK executable; no NuGet compiler package or workload is required.
2. Inventory every mutable field and child kind before copying. This map owns a layer string, a colour list and an application annotation dictionary called `Notes`. Neither inspected compiler exposes `IntermediateNode.Annotations`. Do not invent that API from older examples. `MapNote` values are immutable records, so sharing those values is intentional.
3. Keep the valid older manual copy. Allocate a new custom node, list and dictionary; preserve source span, imported state and diagnostics. Recursively copy only `HtmlContentIntermediateNode` and `HtmlIntermediateToken`, including the HTML encoded-content flag and their common state. Reject any other kind. A larger host needs explicit policies and checks for each additional kind.
4. On the current compiler, override `CloneNode` to create an empty shell carrying only the custom node's own fields. Leave common state and `Children` to the framework's internal recursive caller. Returning children in this hook would make that caller append them again. The public map copy calls its own protected hook, then explicitly copies common state and the supported descendants. Do not use `MemberwiseClone`, which would retain writable collections.
5. Insert `MapPass` through `RazorProjectEngine.Create`'s feature builder. It wraps the first lowered literal HTML node in the original custom node, saves that node, copies it and replaces only the pipeline reference with the working node. On the working copy, change coast/blue to ridge/amber and replace `Estuary` in HTML tokens. The source fixture deliberately has one literal block; this is not a selector for every node in a general template.
6. Generate through the real Razor pipeline. `AcceptExtensionNode` dispatches the custom node and `WriteNode` emits its label using Roslyn's C# literal escaping, then renders its copied children. `MapTool.Compile(new MapPass())` returns the processed document. The current public `GetCSharpDocument()` reader exposes the generated output. The inspected older SDK has no public reader; retain an existing host integration if it needs to retrieve that document. Reflection is not a supported older consumer route.
7. Compare original and working trees, then compile and execute the generated C# in `MapTemplate`. With `Transform = false`, expect `[coast:blue]<p>Estuary route</p>`; with the default true, expect `[ridge:amber]<p>Ridge route</p>`. In both cases the retained original stays coast/blue with `Estuary` markup. Also test a shared-child negative control and unknown-node refusal. Compilation of the library alone does not prove isolation or generated behaviour.

## Constraints

The measured baselines are SDK **10.0.402**, `net10.0`/C#14, and SDK **11.0.100-rc.2.26504.105**, `net11.0`/C#15. Compiler products are **10.4.0-preview.26505.111** and **11.1.0-preview.26504.105**, with Roslyn **5.9.0-1.26505.111** and **5.12.0-1.26504.105**. Execution records retain exact DLL hashes, product commits, runtime and commands. These directly referenced SDK artifacts are the tooling prerequisites for this sample, separately from the .NET 10/11 host runtime families. They are not a claim that normal Razor applications require those exact SDKs. No additional framework, provider, native assets, optional workloads or experimental suppression is used.

The current hook is not designated experimental in the pinned metadata. This GA-targeted edition still has **RC2 proof only**. SDK compiler DLLs and their internal callers are version coupled; rebuild and requalify custom extensions together when changing toolsets. There is no older-target compiler backport, external-host ABI, arbitrary IDE/Hot Reload, MVC, browser, trimming or NativeAOT guarantee. PIN-959 comprehensive coverage and PIN-964 actual-GA reconciliation remain open. This adds no .NET 10 foundation.

The copy boundaries measured for this tree are:

| State | Copy policy and observed result |
| --- | --- |
| Custom root and writable colour/annotation containers | New objects; changing a copied list entry or dictionary entry leaves the original unchanged. |
| Layer, colour and token strings; `MapNote` values | Shared immutable values. Replacing a value affects only the copied container/property. This is not arbitrary object-graph cloning. |
| HTML nodes and tokens | New objects and writable child collections. Updating copied token content or clearing copied children preserves the original. Leaf tokens use the compiler's shared read-only empty child collection. |
| Source spans, imported state, HTML encoded-content flag | Retained values at root and descendants. Source coordinates describe original input; rewriting text does not recompute mappings. |
| Diagnostics | Shared immutable diagnostic objects, independently accumulated on each copied node. Adding to the copy does not add to the original. |
| Internal flags, lazy token representation, arbitrary annotations or properties aliasing children | Outside the public map-copy contract. Token copying materializes `Content`. This custom node has no child-alias property or children outside `Children`. |

The accepted input graph is an acyclic tree of the declared kinds. No cycle detection, shared-subtree identity preservation or concurrent-mutation contract is supplied. If a future custom field is mutable, either copy it explicitly or document intentional sharing before using it in a transformation. The framework hook's responsibilities also include children held outside `Children`; do not assume its generic recursion discovers those. Internal child-alias repair cannot be overridden by this external library, so such a design needs a separate qualified approach.

## Older approach

The previous compiler has no `CloneNode` hook. An original manual copy remains valid: it copies the exact public state this map owns and refuses unfamiliar nodes. Keep this route when retaining the older tooling. The library's public `Compile` runs generation; full generated-document retrieval still belongs to the older host integration because this exact older SDK's reader is internal.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `MapCompiler.csproj`

{{snippet:n10-mapcompiler-csproj}}

### .NET 10: `MapTool.cs`

{{snippet:n10-maptool-cs}}

## Newer approach

The new override supplies a shell for compiler-owned copying as well as the public map-specific copy. `CopyForTransform` still explicitly handles its known children because the compiler's recursive `Clone` is internal. Both paths intentionally share immutable annotation values, rather than claiming to deep-clone every field.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `MapCompiler.csproj`

{{snippet:n11-mapcompiler-csproj}}

### .NET 11: `MapTool.cs`

{{snippet:n11-maptool-cs}}

## Evidence

`eng/Record-MapCompiler.ps1` uses the existing sample-evidence workflow to build both exact dependency sets and run `eng/MapCompilerChecks.cs.txt`. The real pipeline generates both map variants; the matching Roslyn compiler compiles their actual C# and executes each generated `MapTemplate`. Checks assert original preservation, writable-container/node/token isolation, shared immutable values, retained public common state and independent diagnostic accumulation at all three levels. A shallow child-reference control demonstrates mutation leakage; an unsupported C# child is refused.

Current verification separately invokes the pinned private recursive entry point to prove that it calls the protected override and copies this tree's descendants/common state. A custom extension without an override throws `NotSupportedException`. A shell-only probe confirms the override leaves common state and children for that caller. This is a private implementation observation, **not** a public generic cloning route or proof that every compiler host will clone custom nodes. The ordinary public project-engine pipeline exercises the map's explicit copy route. Older verification alone observes the private generated-output reader; neither canonical module uses reflection.

The official pinned previous/current implementations, HTML/token shells, extension dispatch and internal source-generator caller are linked in the accompanying source metadata. The actual artifacts qualify availability and behaviour; metadata/source inspection alone does not stand in for execution. Installed stdio inspection separately verifies discovery, complete offline Markdown, exact canonical snippets, target/edition, applicability and fresh evidence. Other hosts, arbitrary trees and actual GA remain unverified.
