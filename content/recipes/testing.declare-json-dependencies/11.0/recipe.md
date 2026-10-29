# Own JSON dependencies in test projects and extensions

## Goal

Keep tests and their VSTest extensions working when the test platform stops
bringing Newtonsoft.Json into the dependency graph. Declare the package where
its types are used, retain its runtime assets and verify the deployed output.

Gull Point weather station checks a rainfall total from an original JSON reading.
A tiny adapter discovers that check; a separate collector reads an independent
rain-gauge configuration. These are deliberately bounded teaching extensions,
not general test frameworks or production collection infrastructure.

## Older approach

The actual **Microsoft.NET.Test.Sdk 18.7.0** package depends on
**Microsoft.TestPlatform.TestHost 18.7.0**, which brings **Newtonsoft.Json 13.0.3**.
This .NET 10 project therefore compiles without declaring Newtonsoft itself:

{{snippet:older-project}}

Its isolated `Directory.Packages.props` pins the old Test.Sdk:

{{snippet:older-packages}}

Save the linked source as `samples/WeatherJson/WeatherChecks.cs`:

{{snippet:checks}}

The older consumer genuinely builds and executes this check on SDK **10.0.402**,
C# **14**, runtime **10.0.13**. Its lock records Newtonsoft 13.0.3 as **Transitive**,
selecting `lib/net6.0/Newtonsoft.Json.dll` for compile/runtime. This valid historical
route is retained; relying on a platform's unrelated dependency is fragile.

## Newer approach

Changing only Test.Sdk to **18.11.0** removes that transitive JSON dependency.
The unchanged source fails **CS0246** at its Newtonsoft import. This happens on
the tested .NET 10 target too: package version and dependency closure determine
the change, rather than the project's target alone. The compatibility note
introduces the platform change in .NET 11 Preview 4.

Declare Newtonsoft explicitly. This .NET 11 project uses the same check:

{{snippet:newer-project}}

{{snippet:newer-packages}}

The explicit pin is **13.0.4**, selected after reviewing the publisher's stable
release and comparing it with the actual old 13.0.3 closure. The stable package
index had 13.0.4 as the newest non-prerelease candidate at capture; 13.0.5/14
prereleases are not silently adopted. This is a reviewed sample baseline, not a
promise of perpetual security or compatibility. Both JSON versions execute the
same rainfall check. Commit each real `packages.lock.json`, including content
hashes, then restore with `--locked-mode`. Do not hand-author a lock or treat a
package's major version as a target-framework guarantee.

The newer TestHost selects **net8.0** assets on both tested targets; Newtonsoft
13.0.4 selects **net6.0** compile/runtime assets. The packages do not require a
net11 test target. For the independently executed .NET 10 route use:

{{snippet:current10-project}}

Its central Test.Sdk/Newtonsoft pins are identical to the newer ones. This is a
conditional newer-package route on target 10, not another .NET 10 foundation.

## Own the extensions' runtime files

Save this adapter as `samples/WeatherJson/WeatherAdapter.cs`:

{{snippet:adapter}}

Build it with this separate .NET 11 library project:

{{snippet:adapter-project}}

Its package boundary keeps the platform contract compile-only and retains JSON:

{{snippet:extension-packages}}

`PrivateAssets="all"` controls propagation to consumers; it does not itself remove
the library's runtime files. `IncludeAssets="compile"` on ObjectModel avoids
deploying a competing platform contract. **Do not apply that restriction to
Newtonsoft.** `CopyLocalLockFileAssemblies=true` copies its JSON runtime DLL into
the extension output. Ship that dependency with the extension, not just its DLL.

The collector is a separate assembly, with independently authored JSON input.
Save `samples/WeatherJson/WeatherCollector.cs`:

{{snippet:collector}}

{{snippet:collector-project}}

Use the same central pins in the collector's directory. The paired .NET 10
adapter/collector projects target `net10.0`, declare C# 14 and ObjectModel
**18.7.0**, and explicitly retain Newtonsoft **13.0.4**. They load under both real
18.7 and 18.11 runners. The .NET 11 extensions use ObjectModel **18.11.0**.
This proves those exact public adapter/collector contracts, not arbitrary binary
compatibility with older extensions.
These extensions deliberately match the **managed CLI process runtime**. The
publisher recommends portable netstandard extensions for broader use; a collector
loaded by Visual Studio can run in a different runtime from the tests. Do not
deploy these net10/net11-only examples into a .NET Framework collector process.

## Reproduce the deployment and failures

Keep current projects in their own directories under `samples/net10.0` or
`samples/net11.0`; the `../../WeatherJson` paths refer to the common original
sources. The historical project lives directly in `samples/WeatherJson` and
explicitly compiles only WeatherChecks.cs.
Each directory has its own central package file and SDK pin. In `global.json`,
set `sdk.version` to **10.0.402** or **11.0.100-rc.2.26504.105**,
`sdk.rollForward` to `disable`, and `test.runner` to **VSTest**. C# mode is
declared in the project. Use nuget.org as the explicit package source.

1. Use separate child-process CLI homes/caches, set
   `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` before first use, and keep shared PATH and
   persistent settings unchanged. Build each project with its matching absolute
   SDK executable: `build <project.csproj> -c Release`; capture the initial lock,
   then repeat `restore <project.csproj> --locked-mode`.
2. Acquire the **Microsoft.TestPlatform.CLI 18.7.0 / 18.11.0** ZIPs from nuget.org
   into isolated directories and extract them. Both measured ZIPs contain the
   real managed runner at `contentFiles/any/net10.0/vstest.console.dll`, with
   `rollForward: Major` in their runtime configuration. Run that exact DLL with
   the corresponding absolute .NET 10 or .NET 11 executable. Do not substitute an
   SDK-bundled runner and call it the same package artifact.
3. Copy `WeatherStation.TestAdapter.dll` beside `WeatherTests.dll`, retaining the
   test output's Newtonsoft DLL. Run
   `<runner.dll> <WeatherTests.dll> /ListTests '/Logger:console;verbosity=detailed'`.
   Expect `weather-adapter=loaded;schema=2` and the single
   `WeatherStation.WeatherChecks.RainfallTotal` test. The adapter can only discover
   this sample assembly; it is not a replacement for an established test framework.
4. Retain the collector's whole output directory, including Newtonsoft. Pass
   `/TestAdapterPath:<absolute-collector-output>` and `/Collect:WeatherStation`
   when executing `<runner.dll> <WeatherTests.dll>`. Save these runsettings,
   replace `COLLECTOR_DLL` with the XML-escaped absolute path to
   `WeatherStation.Collector.dll`, and pass `/Settings:<settings-file>` too:

{{snippet:collector-settings}}

   Expect `weather-collector=loaded;sensor=rain-gauge`,
   `weather-test=Gull Point;rainfall=5.0;runtime=...` and **exactly one passed test**
   in the TRX, not merely exit zero. The warning channel carries the collector's
   success marker solely to make this tiny example observable.
5. In a fresh copy of the old test project, change only Test.Sdk to 18.11.0.
   The same source must fail **CS0246**; assets must contain no Newtonsoft.
   Add the explicit 13.0.4 reference, rebuild and execute successfully.
6. In a fresh current test project, deliberately exclude JSON's runtime assets:

{{snippet:exclude-runtime}}

   With central versions, keep the version in `PackageVersion` and add only
   `ExcludeAssets="runtime"` to its `PackageReference`. Compile still succeeds.
   Ensure no Newtonsoft DLL remains in this new output; deploy only the adapter
   DLL beside it and run without a collector/other dependency-bearing extension
   path. The actual test fails with **FileNotFoundException** for Newtonsoft.
   An adapter/runner copy can otherwise mask this mistake.
7. Independently build fresh adapter and collector copies with the same runtime
   exclusion. Against the JSON-free test output, adapter discovery reports the
   missing assembly and discovers no weather test. Point the collector runsettings
   and adapter-path option at the JSON-free collector output: initialization
   reports `Data collector 'WeatherStation'` and the missing Newtonsoft assembly.
   A zero-test discovery can exit **zero** even though loading failed; assert the
   diagnostics and absence of the test. It is not passing test evidence.

`eng/Test-JsonDependencies.ps1` reproduces all these cases with actual packages,
locks, isolated child environments, real runner discovery/execution and TRX
counts. Its `-Record` option captures the seven sample projects through the
existing input-hash evidence workflow; disposable logs/packages/results remain
under `artifacts/json-dependencies`. It does not install a global tool or publish.

## Migration

1. Find every test, adapter and collector that uses JsonConvert, JObject or other
   Newtonsoft types. Inspect each project's resolved graph and deployed output.
2. Declare a reviewed direct Newtonsoft version in each owning project. Retain
   runtime assets, update its lock and remove dependency-exclusion workarounds.
3. Clean/rebuild into separate output, deploy extension dependencies together,
   then run real discovery, a test and collector initialization. Confirm expected
   test counts and loaded behavior; check failures independently of exit status.
4. Record SDK/runtime, runner/package versions, target and selected assets.
   Upgrade these sample pins deliberately; rerun affected consumers when they change.

## Constraints and alternatives

The exact evidence is Windows x64, SDK 10.0.402 / 11 RC2, C# 14 / 15 and runtimes
10.0.13 / 11.0.0-rc.2.26504.105. No C# 15 feature, provider, native compiler,
workload, rendering mode, device, cloud resource or Human-only step is required.
Test.Sdk brings CodeCoverage, but these checks run a **managed custom collector**;
they do not enable native coverage or promise every shipped native asset works.
Other operating systems, .NET Framework, trimming and Native AOT are unverified.

The .NET 11 SDK root's `dotnet.deps.json` retains Newtonsoft **13.0.4** for other
tools. Its VSTest/testhost manifests use **18.11.0-release-26504-105** platform
components without Newtonsoft. These are separate from the downloaded stable
18.11.0 packages, not byte-identical artifacts. An SDK-root DLL is not a project
reference, runtime deployment contract or reason to copy files out of the SDK.

This is **VSTest** guidance. `Microsoft.NET.Test.Sdk` and these extensions do not
turn an MTP application into a VSTest project. The existing RunnerTests examples
use Microsoft.Testing.Platform and cannot prove this dependency change. Select
the intended runner first; preserve a working MTP configuration when that is the
application's choice.

If the test owns simple JSON without Newtonsoft-specific behavior, an explicit
migration to System.Text.Json is an alternative; verify its serialization rules
rather than swapping imports. Keeping a supported older dependency set can be a
temporary choice, but it does not repair a newer deployment's missing assets.

Adding Newtonsoft cannot restore removed communication APIs such as
`Message.Payload`, `VersionedMessage` or the old public converter/resolver types.
That requires a separate supported API migration; this recipe deliberately uses
only the public adapter/collector contracts. RC execution does not establish
actual .NET 11 GA support. PIN-959 qualifying coverage/evidence and PIN-963 actual-GA
reconciliation remain open; the exact eight .NET 10 foundations are unchanged.

## Evidence

All seven project closures have fresh compilation/execution records. Three real
runner/test-host lanes each produce exactly one passed rainfall test, a successful
discovery and collector initialization. Separate fresh consumers demonstrate
CS0246, the test's missing-runtime FileNotFoundException, failed adapter discovery
and the collector's missing-runtime initialization diagnostic. The net10 current
lane executes Test.Sdk/TestHost 18.11.0 on runtime10; it is not cross-compilation
presented as execution. Four audited ZIP hashes, restored bytes, selected assets,
full locks, SDK-root manifests and exact CLI runner hashes qualify the claim.

The bounded source audit establishes introduction and removal facts; actual
consumers establish the behavior above. Installed-tool list/get inspection checks
offline delivery, canonical snippet identity, edition, target conditions and
evidence freshness. It does not establish untested platforms or GA readiness.

## Sources

- [VSTest Newtonsoft removal and mitigation](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/11/vstest-removes-newtonsoft-json)
- [VSTest versus MTP runner selection](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [Test extension authoring](https://github.com/microsoft/vstest/blob/main/docs/Overview.md)
- [Collector naming and deployment](https://github.com/Microsoft/vstest-docs/blob/main/docs/extensions/datacollector.md)
- [Newtonsoft.Json 13.0.4 release](https://github.com/JamesNK/Newtonsoft.Json/releases/tag/13.0.4)
- [Lock files and locked restore](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
