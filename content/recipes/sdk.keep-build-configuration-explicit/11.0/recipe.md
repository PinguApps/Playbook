# Select build and publish configurations explicitly

## Goal

Keep a depot development loop on Debug and its release artifact on Release, even when a parent shell or CI runner supplies Configuration. Make that intent visible at the command boundary.

## Older approach

On SDK 10, an inherited Configuration already affects ordinary run/MSBuild evaluation. Do not claim it is ignored everywhere. However, publishing a modern target defaults to Release: the real SDK-10 check with ambient Configuration=Debug still produces a Release artifact. Scripts relying on that default may therefore change behavior after an SDK upgrade.

{{snippet:older}}

## Newer approach

SDK 11's shared -c/--configuration option reads Configuration from the environment when no explicit CLI value is provided. In the same publish check, ambient Debug now produces a Debug artifact. Pass `dotnet publish SdkWorkflows.csproj -c Release -o ./published` to state the release intent. For the local loop, use `dotnet run --project SdkWorkflows.csproj -c Debug --no-launch-profile` or forward the explicit Debug configuration through watch.

{{snippet:newer}}

The compiled value can be observed directly rather than inferred from a directory name:

{{snippet:program}}

## Migration

1. Find scripts that omit -c while inheriting Configuration. Inspect both the SDK command and the project's MSBuild defaults.
2. Add -c Debug for the intended local build/run loop and -c Release for the intended release artifact.
3. Build and execute the actual result to check its AssemblyConfiguration metadata. The tests also run each published DLL.
4. Avoid using whitespace as an environment cleanup strategy. The shared CLI option ignores empty/whitespace values, but MSBuild still imports the process environment; explicit -c avoids ambiguity.

## Constraints

This is an SDK/default-selection change, not a change to Debug or Release semantics in the runtime. Explicit project properties and custom targets can introduce their own behavior. CLI option precedence does not make every downstream MSBuild property immune to authored overrides. Publish here means creating files locally; no deployment, account or registry publication is performed.

The canonical C# managed projects require no provider, rendering mode, package or native workload. No performance, trimming or AOT claim follows from their configuration labels. SDK 11 evidence uses RC2.

## Evidence

Actual SDK-10/11 publish commands with ambient Debug produce Release/Debug respectively. Both run commands honour an explicit -c Debug even under ambient Release or whitespace. The published assemblies are executed and report their compiled configurations as JSON. Saved compilation and command records plus installed full-body/applicability checks keep the documented distinction traceable.
