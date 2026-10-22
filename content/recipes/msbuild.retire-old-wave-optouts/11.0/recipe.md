# Retire build change-wave opt-outs

## Goal

Remove a temporary MSBuild compatibility switch after checking the custom build's observable result. Recognize a retired-wave warning rather than assuming the requested historical behavior was restored.

## Older approach

An original dispatch-file target writes and reads two destinations. Its older wrapper launches the selected SDK with MSBUILDDISABLEFEATURESFROMVERSION=18.3. That wave controlled the newer atomic file-replacement path; a temporary opt-out could support investigation while a build was adapted.

{{snippet:older}}

The flag is an environment variable, not a project property. It affects a wave and subsequent waves, not every change in the named MSBuild release. A successful dispatch-file check establishes these file contents; it does not prove which filesystem write strategy ran or that every custom build needs an opt-out.

## Newer approach

Waves 17.10, 17.12, 17.14 and 18.3 are retired in the .NET 11 MSBuild family. Their affected newer behavior is unconditional. RC2 accepts each old value only by clamping it to 18.4 and reporting warning MSB4272. Clamping does not resurrect the removed behavior and may still disable later waves.

The migrated wrapper clears the inherited switch for its child. It preserves the same explicit dispatch-file contract:

{{snippet:newer}}

## Migration

1. Search build launchers, environment configuration and CI for MSBUILDDISABLEFEATURESFROMVERSION. Record its purpose and the real behavior that originally needed adaptation. Do not add a switch to builds that never used one.
2. Consult the affected wave's source-backed changes. For 18.3, inspect file-writing consumers, destination permissions and retries. Other retired waves include evaluation/logging/resolver changes; their fixes are specific to the affected custom build.
3. Run the old and new SDKs with each relevant setting, then without it. For the standalone example, save the targets as BuildInputs.proj and pass the real executable: `dotnet msbuild BuildInputs.proj -t:RunDispatch -p:DotnetExecutable=<absolute-dotnet-path>`. Quote the whole property argument when the path contains spaces.
4. Check dispatch.txt contains QUAY-7 and QUAY-8 and the target reports both values. Add the corresponding observable check for the actual custom build, rather than relying on a warning disappearing.
5. Remove the retired setting from its owning launcher/environment after adapting the build. Use a fresh process; persistent MSBuild processes retain their starting environment. For a development session using CLI servers, coordinate `dotnet build-server shutdown` before restarting; do not terminate unrelated shared builds.

## Constraints

The exact measured clamped value is 18.4 on the recorded RC2 artifact; recheck diagnostics on a different MSBuild baseline. MSB4272 is a warning in these commands, not inherently a build failure. A stricter caller can promote warnings. Do not replace a retired value with the earliest active wave merely to hide the warning: that can retain other opt-outs.

The original XML wrapper is language-neutral. The verification projects' C# modes describe the harness, not application requirements. Only the declared target-11 SDK route is served. No packages, provider, rendering mode or native workload are required. No .NET Framework MSBuild.exe, concurrent-write stress or exhaustive historical-wave behavior was executed.

## Evidence

Actual SDK-10 runs produce the dispatch artifact without MSB4272 for the four tested values. Actual SDK-11 RC2 runs produce MSB4272 and identify 18.4 for all four, while preserving those file contents. Both migrated/default runs succeed; the default RC2 run has no retired-wave warning. The original older/newer C# harnesses compile and execute on Windows x64. These checks establish the diagnostic and dispatch contract, not every internal feature behind each wave. Installed offline list/get checks separately retain the full comparison and applicability. GA reconciliation remains separate.
