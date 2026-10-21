# Run selected test projects with deliberate runner and limits

## Goal

Run the pier's berth checks with the intended runner, choose exactly which projects or built modules enter the run, and distinguish a failed assertion from an aborted whole run.

## Older approach

SDK 10 already supports the native Microsoft.Testing.Platform (MTP) mode. Pin SDK 10.0.402 in global.json and set its test section to `"test": { "runner": "Microsoft.Testing.Platform" }`. Without that selection, dotnet test uses VSTest. This is a valid older configuration, not a need to rewrite every test.

{{snippet:older}}

Run `dotnet test --project RunnerTests.csproj -c Release` in MTP mode. The accompanying .NET 10 NUnit comparison uses VSTest, Microsoft.NET.Test.Sdk 18.0.1, NUnit 4.6.1 and NUnit3TestAdapter 6.2.0: its command is `dotnet test NUnitTests.csproj -c Release --filter FullyQualifiedName~Reservation`. VSTest accepts a positional project/solution and VSTest-specific --filter/--logger options; do not forward those unchanged to an MTP-native run.

## Newer approach

For standalone C# projects, add `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>` inside the project PropertyGroup. The canonical comparisons obtain these settings from their isolated shared sample properties; do not depend on the Playbook checkout to supply them.

Under SDK 11, keep global.json's MTP selection or set DOTNET_TEST_RUNNER=Microsoft.Testing.Platform in the child process environment. A recognised environment value overrides global.json; VSTest is the other supported value. That environment override is an SDK 11 capability, separate from the test application's target. Pin the original SDK to 11.0.100-rc.2.26504.105 for reproduction.

{{snippet:newer}}

The original assertions are independent of runner selection. PIER_FAIL and PIER_DELAY exist only to exercise non-success policy outcomes:

{{snippet:tests}}

Use `dotnet test --project RunnerTests.csproj -c Release --maximum-failed-tests 1` with PIER_FAIL=1 to get whole-run exit 13. Use --timeout 1s with PIER_DELAY=1 for exit 3. Put these run-level policies before --. Options after -- reach each test application: for example `-- --report-xunit-trx --report-xunit-trx-filename berths.trx` belongs to this xUnit runner. Timeout measures active test-application time, not restore/build time. Our RC2 run returned exit 3 after the delayed test finished; it does not promise immediate process termination.

For a maintained project set, save the following under Traversal/dirs.proj; keep it out of the test project's own obj directory:

{{snippet:traversal}}

Run `dotnet test Traversal/dirs.proj -c Release --results-directory-layout per-module` from RunnerTests. SDK 11 recursively expands traversal references, honours Configuration/Platform metadata and deduplicates repeated projects. A traversal project selects projects; it is not itself a test module. Restore its pinned MSBuild SDK during preparation.

## Migration

1. Inspect global.json and the actual framework/runner packages. Use the creation recipe to migrate incompatible projects before selecting MTP for a whole solution.
2. Choose one selector: --project, --solution or --test-modules. Explicit project selection avoids accidentally running an unrelated test project.
3. For already-built modules use a semicolon-separated glob expression, for example `--root-directory .. --test-modules "**/bin/Release/net11.0/*Tests.dll; !**/NUnitTests.dll"`. Set a common search root for both inclusion and exclusion patterns. SDK 11 trims whitespace and applies leading-! exclusions within that root; separately enumerated literal parent paths did not exclude the sibling in our initial probe. Do not combine module selection with --configuration, --framework, --runtime, --arch, --os, --use-current-runtime or --no-dependencies; those require project evaluation.
4. With project selection, --no-dependencies skips building referenced projects; ensure their required artifacts already exist. --use-current-runtime selects the current runtime during restore/build; it does not change what --test-modules contains.
5. Put run policies before -- and framework-extension options after it. For reports, set --results-directory to a deliberate location and --results-directory-layout per-module to avoid module filename collisions. Without an explicit directory, SDK artifacts layout uses <ArtifactsPath>/test/<project>/<pivot>; flat remains the ordinary default.

## Constraints

MTP-native dotnet test requires Microsoft.Testing.Platform 1.7 or later and an executable test application. The pinned xUnit MTP-v2 route resolves its actual MTP dependencies in the lock file. The traversal example additionally needs Microsoft.Build.Traversal 4.1.82. NUnit is a separate framework, not an xUnit plugin. No database/provider, rendering mode, mobile workload or experimental opt-in is involved. The tests are C# examples; F#/VB template availability is documented but not execution evidence for those languages. This edition serves target 11 only; SDK 10 examples remain the comparison, not a ninth historical foundation. Mobile/device testing and the unavailable affected-test extension workflow are outside this goal.

## Evidence

The real original SDK-10 xUnit MTP and NUnit VSTest projects compile and execute; SDK 11 executes both as MTP applications. Focused commands check project selection, traversal's six results, module exclusions, a per-module xUnit TRX artifact, the environment override and the documented run-policy exit codes. --no-dependencies is exercised as an accepted project-mode option; the fixture has no required project-reference build, so the check does not establish build-skipping behavior. Sources establish recursive traversal/deduplication and runtime option semantics; no separate diamond or runtime-switch execution is claimed. Installed stdio list/get checks the full canonical body, goal, applicability and target-11 edition with local-only installation and network-isolated retrieval. Evidence retains exact RC2 SDK/package artifacts, not a GA claim.
