# Preserve production runtime settings while allowing development overrides

## Goal

Use this when a CoreCLR application needs a stable production runtime setting
and an intentional, separately deployed development override. This concerns the
native host's `runtimeOptions.configProperties`, not `appsettings.json`, launch
profiles or Microsoft.Extensions.Configuration providers.

Gullhaven tidal station prints a harmless application-specific diagnostic mode.
The production value lives in a source project item. The development sidecar
also adds an operator name so that ordinary merging is observable independently
of duplicate precedence. No performance measurement or real runtime tuning switch
stands in for the selected value.

## Older approach

The .NET 10 comparison uses SDK 10.0.402, C# 14, hostfxr and runtime 10.0.13:

{{snippet:older-project}}

## Newer approach

The .NET 11 version uses SDK 11.0.100-rc.2.26504.105, C# 15 and matching RC2
hostfxr/runtime. These exact pins reproduce the evidence; they are separate from
the feature requirement, .NET 11's host behavior introduced in Preview 6.

{{snippet:newer-project}}

Both projects contain this independently authored report:

{{snippet:report}}

`RuntimeHostConfigurationOption` contributes to generated
`TidalDiagnostics.runtimeconfig.json`. For this example keep the item in the
project; `runtimeconfig.template.json` is another supported source input. Editing
the generated output is temporary: regeneration can replace it. The check changes
the source value to `survey`, corrupts the generated output, rebuilds, and observes
`survey`; restoring the source and rebuilding restores `production`.

## Deliberately add development settings

Save this separately as `development.json`. It is an input to a deliberate copy,
not a magic filename the host reads automatically:

{{snippet:instructions-1}}

After building, copy it beside the DLL with the exact name
`TidalDiagnostics.runtimeconfig.dev.json`. Run the DLL with the intended
installation's absolute `dotnet` path. From a directory containing the matching
project, `Program.cs`, `development.json` and a `global.json` pin for the stated
SDK (`rollForward: "disable"`, `allowPrerelease: true` for RC2), use:

{{snippet:instructions-2}}

For an old-host comparison, identify the selected hostfxr as well as the runtime;
an installation containing a newer hostfxr is unsuitable. Observe:

| Files present | .NET 10 host | .NET 11 host |
| --- | --- | --- |
| Production and development duplicate | mode=production; operator=night-watch | mode=development; operator=night-watch |
| Dev file contains only Operator | mode=production; operator=night-watch | mode=production; operator=night-watch |
| No dev file | mode=production; operator=none | mode=production; operator=none |

The old host's production-first result is a valid older approach. To prevent an
unintended override on .NET 11, remove the duplicate property from the dev file
or omit that file. Preserve nonduplicate development properties when required.
Do not assume the target framework selects the host parser: a net10 DLL launched
by hostfxr 11 is not the .NET 10 host comparison. The saved check exposes exactly
one hostfxr/runtime in each temporary installation and verifies both host traces
and the reported CoreLib path.

## Build and publish intentionally

The actual SDK 10.0.402 and RC2 paths were evaluated and executed:

- For `Configuration=Release`, `GenerateRuntimeConfigDevFile` evaluates to
  `false` in SDK 10 and is unset in RC2; neither emits a sidecar. RC2 Debug
  evaluation enables dev-file generation for hot reload. Explicitly building
  Release with `-p:GenerateRuntimeConfigDevFile=true` generates a dev file with
  `additionalProbingPaths`; it does not invent these application overrides.
- Ordinary builds and framework-dependent publishes of these projects produced
  no dev sidecar. A stale file can still remain in a reused output directory.
- Explicit copy metadata can include the sidecar in **both** build and publish
  output. The check runs this real project addition:

{{snippet:instructions-3}}

Publishing with that item demonstrates the duplicate behavior again. Therefore
`GenerateRuntimeConfigDevFile=false` is not a universal copy/publish exclusion.
For this dependency-free, framework-dependent example, deploy an allowlist into
a **new empty directory**: `TidalDiagnostics.dll`,
`TidalDiagnostics.deps.json`, and `TidalDiagnostics.runtimeconfig.json`.
There is no development sidecar in that artifact set, so both hosts print
`production` and `operator=none`. This three-file list is specific to this sample;
derive and test a complete list for an application with dependencies or content.

## Migration

1. Inspect the actual executable, hostfxr/runtime, project inputs and output
   directories. Identify any duplicate `configProperties` keys and explicit
   dev-file generation/copy items.
2. Put the intended production value in durable source inputs and rebuild. Keep
   development duplicates only when the .NET 11 override is intentional.
3. Run with and without the duplicate and with the sidecar absent. Use an
   application-specific `AppContext.GetData` observation and verify an unrelated
   property still merges. Do not generalize one setting to every environment
   variable or AppContext API.
4. Publish, inspect the actual files and deploy only the intended artifact set
   into a fresh directory. Run that set with the production host; verify the
   development value cannot be supplied by a leftover sidecar.

## Constraints

This target-11 edition retains an actual .NET 10 comparison; it adds no historical
.NET 10 foundation. Both samples are workload-free CoreCLR console applications
with no NuGet dependencies, provider, browser/rendering mode or native compiler.
The example's C# modes are sample pins, not language requirements for host merging.
Execution evidence is Windows x64, SDK 10.0.402 / 11 RC2, hostfxr/CoreCLR
10.0.13 / 11.0.0-rc.2.26504.105. Other platforms need their own artifact check.
No API here is experimental; RC2 evidence is not GA verification.

## Evidence

`eng/Test-RuntimeOverrides.ps1` compiles detached consumers in an isolated cache,
runs all cases above and records host identity and copy/generation observations.
Its `-Record` mode saves execution evidence through the existing sample input
freshness system after ordinary compilation evidence has been captured. Child
processes disable .NET first-run PATH integration and compare all PATH scopes;
they make no persistent environment change.

This is not a Native AOT sidecar contract, a claim about single-file bundling,
universal publish exclusion, or application-configuration provider precedence.
Full coverage and actual GA reconciliation remain separate publication gates.

## Official references

- [Runtime configProperties precedence change](https://learn.microsoft.com/en-us/dotnet/core/compatibility/deployment/11/runtimeconfigdev-configproperties-precedence)
- [Runtime configuration source inputs](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/)
- [SDK runtime configuration properties](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#generateruntimeconfigurationfiles)
