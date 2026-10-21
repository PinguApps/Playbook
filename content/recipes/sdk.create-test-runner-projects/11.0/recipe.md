# Create executable tests with an explicit framework and runner

## Goal

Create original berth-reservation tests whose framework, executable entry point and selected dotnet test runner agree. Choose xUnit v3 or NUnit intentionally.

## Older approach

SDK 10 can run xUnit v3 on MTP with an explicitly authored project, as the runner-configuration recipe demonstrates. An existing NUnit VSTest project also remains valid; this independent older comparison uses the test SDK and adapter rather than an MTP entry point:

{{snippet:older}}

Its global.json pins SDK 10.0.402 and selects `"test": { "runner": "VSTest" }`. Run `dotnet test NUnitTests.csproj -c Release`. The test SDK supplies VSTest hosting, while the adapter connects NUnit assertions to it. Changing only the CLI runner selection does not add an executable MTP host to this project.

## Newer approach

For standalone C# projects, add `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>` inside the project PropertyGroup. The canonical comparisons obtain these settings from their isolated shared sample properties; do not depend on the Playbook checkout to supply them.

The SDK-11 built-in template accepts `dotnet new xunit --xunit-version v3`, which defaults to MTP; `--test-runner VSTest` deliberately chooses the other runner. NUnit opts into MTP with `dotnet new nunit --test-runner Microsoft.Testing.Platform`. Run template creation in a directory with its own global.json: the template's post-action can edit the nearest global.json runner setting. Do not inadvertently change a shared repository runner.

The original NUnit MTP project keeps its runner flags and explicit package assets:

{{snippet:newer}}

The alternative original xUnit MTP project uses its matching entry-point package and runner property:

{{snippet:xunit}}

Choose one framework project; do not add both framework package sets to the same project merely to select a runner. With a global.json SDK pin of 11.0.100-rc.2.26504.105 and `"test": { "runner": "Microsoft.Testing.Platform" }`, run `dotnet test --project NUnitTests.csproj -c Release` or the equivalent --project RunnerTests.csproj command. These are original assertions rather than template tests:

{{snippet:tests}}

## Migration

1. Identify the existing framework and runner. Preserve a working VSTest configuration if its integration is still appropriate.
2. Generate a new SDK-11 project with the explicit framework/runner options, inspect its project and global.json changes, then replace template assertions with the intended behavior.
3. For xUnit MTP-v2, use xunit.v3.mtp-v2 and UseMicrosoftTestingPlatformRunner. For NUnit MTP, use EnableNUnitRunner, IsTestingPlatformApplication and OutputType=Exe with a supported adapter. Do not leave a library-only test project while expecting direct MTP execution.
4. Pin packages and SDKs, restore a lock file, and run the actual selected project. If copying these standalone project files beneath central package management, either add the versions to that sample's central file or set ManagePackageVersionsCentrally=false in its Directory.Packages.props; do not use forbidden per-project version overrides.
5. Keep runner configuration consistent across every selected project. Use explicit project selection while migrating a mixed solution. MTP uses --project/--solution and framework-specific options; VSTest's positional project and --filter/--logger contract is different.

## Constraints

These samples target net10.0/C#14 and net11.0/C#15 respectively, with independently pinned SDKs. The SDK-11 templates expose the options for C#, F# and VB, but only these original C# projects were compiled and executed. xunit.v3.mtp-v2 3.2.2 and NUnit 4.6.1/NUnit3TestAdapter 6.2.0 are the alternative example pins, not a requirement to install both sets in a consumer. Native MTP requires version 1.7 or later; exact transitive versions are in recorded evidence. No mobile workload, provider, rendering mode or experimental API is required. No account, service or package publication is involved.

## Evidence

The harness creates both SDK-11 templates with --no-restore inside an isolated global.json boundary and checks the generated runner contracts. The independently authored projects then compile and execute real assertions through their declared runners: older NUnit VSTest, xUnit MTP on both targets, and newer NUnit MTP. The template's default assertions are not presented as original verification. Actual packed-tool retrieval compares all canonical source snippets and the complete offline narrative with target-11 applicability. No F#/VB, mobile or GA execution claim is made.
