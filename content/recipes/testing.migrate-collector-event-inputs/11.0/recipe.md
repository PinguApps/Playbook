# Observe VSTest collector events without rewriting outcomes

## Goal

Quay's berth-test collector records which case ran and its outcome for a shift
report. Observe the events delivered by VSTest and copy their scalar values into
owned immutable records. A collector's report must preserve the runner's identity,
child-case flag and actual result even when the event properties become writable.

## Requirements and recorded artifacts

The original audit draft described removed setters. Its actual asset diff and
fresh qualification establish the opposite: TestCaseId, TestCaseName, IsChildTestCase,
TestElement and TestOutcome gain public setters. This recipe retains its stable
ID while applying Pingu's approved corrected scope. It teaches no nonexistent
removed-setter migration and no forced rewrite of a working older observer.

| Layer | Older comparison | Current consumer |
| --- | --- | --- |
| SDK / compiler / language | 10.0.402 / 5.9.0-1.26505.111 / C#14 | 11.0.100-rc.2.26504.105 / 5.12.0-1.26504.105 / C#15 |
| Framework / ref / actual runtime | net10.0 / 10.0.13 | net11.0 / 11.0.0-rc.2.26504.105 |
| ObjectModel / CLI / Test.Sdk / TestHost | 18.7.0 | 18.11.0 |
| ObjectModel compile/runtime assets | lib/net8.0 | lib/net8.0 |
| Public event setters | private, except internal TestElement | public for all five |

Public package builds and the SDK-bundled tooling are separate artifacts, not
assumed byte-identical from matching version labels. Both actual public CLI
packages provide their complete net10.0 console closure; the current console
runs under the selected .NET11 host. SDK/compiler/ref/runtime and package ZIP/
closure identities are recorded separately. Keep collector, test host and runner
versions matched. The recipe advertises target11 only; this actual older comparison
adds no ninth .NET10 foundation or unverified down-level route.

The measured platform is Windows x64 with PowerShell7 and complete matching SDKs.
There is no native compiler, optional workload, provider, account, UI rendering
mode or experimental opt-in. Public NuGet access prepares the package closure;
installed guidance itself is offline. The public packages are stable; SDK/runtime
execution is RC2 evidence, not actual-GA qualification. Other OSes, arbitrary test
frameworks, .NET Framework, MTP, trimming and NativeAOT remain unqualified.

## Older approach

Keep constructor-based unit fixtures and read-only observation of production
events. The older public package's setters are private/internal, so assigning
these properties from an external collector does not compile. The same safe
observer below works on both real package sets. Save this older project as
`QuayCollector.csproj` and central versions as `Directory.Packages.props`:

{{snippet:project-10}}

{{snippet:packages-10}}

## Newer approach

Use these current project/package files at the same paths:

{{snippet:project-11}}

{{snippet:packages-11}}

Save the following `Collector.cs`. Event callbacks create immutable observations;
they retain neither event arguments nor the mutable TestElement object. Log the
observed outcome as supplied. Do not use the new setters to normalize names, repair
IDs, turn a failure into success or pretend an event is an application DTO.

{{snippet:collector}}

Save `Program.cs` for the separate executable unit/contract check. Public
TestCase-based constructors support collector unit tests; they are not an instruction
to synthesize production event streams. This synthetic fixture is clearly separate
from the real test-run evidence.

{{snippet:construction}}

The project excludes `SetterControl.cs` by default. For the offline ordinary
consumer, omit its conditional Compile line when that authoring-only control is
not copied. Create `global.json` beside each project with
`{"sdk":{"version":"10.0.402","rollForward":"disable"}}` for the older directory
or `{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}` for the
current one. The explicit project fields make these copies independent of root
tool/package settings. Copy the matching global.json into Tests as well when
retaining the separate test project's evidence record.

## Migration

1. Inspect the real ObjectModel reference and complete runner/test-host closure.
   Apply the appropriate exact central versions above and use the corresponding
   SDK executable. Run `dotnet restore QuayCollector.csproj --packages <absolute-cache>`
   and `dotnet build QuayCollector.csproj -c Release --no-restore`. Retain/review
   the generated lock file and use `--locked-mode` on subsequent restores. Compare
   resolved package assets; do not infer their target framework from package major.
2. Subscribe to real TestCaseStart/TestCaseEnd and snapshot owned scalar values
   immediately, as shown. Preserve the supported older observation path. New
   public setters are not a requirement to mutate an incoming event. Keep public
   constructor-based unit tests separate from callbacks; run the compiled
   `Quay.Collector.dll false` fixture on the old host or `Quay.Collector.dll true`
   on the current host to check that actual setter visibility matches expectations.
3. To reproduce this original test run, save `Tests/QuayTests.csproj`:

{{snippet:tests-project}}

Save `Tests/BerthTests.cs`. This small real adapter runs arithmetic assertions
and reports their actual pass/failure to VSTest; the collector never chooses them.

{{snippet:adapter}}

4. Restore/build that test project with the same SDK and cache. The CLI package
   is acquired by the collector project but is not a compile dependency. Locate
   its real console at `<cache>/microsoft.testplatform.cli/18.11.0/contentFiles/any/net10.0/vstest.console.dll`
   (use18.7.0 for the older comparison). Invoke it with the selected absolute
   dotnet executable rather than whichever SDK's built-in test runner happens
   to be on PATH.
5. Save the following runsettings and replace `COLLECTOR_DLL` with the absolute
   compiled `Quay.Collector.dll` path, XML-escaped when needed:

{{snippet:settings}}

6. Save `Run-Collector.ps1` below. Run it in a PowerShell7 child with `-Dotnet`,
   `-Console`, `-TestAssembly`, `-CollectorDirectory`, `-Settings` and
   `-ResultsDirectory` set to the absolute paths prepared above; add `-Collect`.
   It preserves argument boundaries and explicitly selects the child hosts
   without changing PATH. Expect exit 1 because the second test's
   real assertion expects 12 while 20 minus 7 is 13. This intentional failure must
   remain Failed; the first test must remain Passed.
7. Match the four logged QUAY_EVENT start/end observations to TRX testId/testName.
   Require the same identity/name, IsChild=false for these top-level cases, and
   the actual end outcome. Repeat without settings/Collect and compare IDs and
   outcomes. The two real result sets must be identical, while only the enabled
   collector emits observations. A missing collector warning is a failure of
   this check, even if the tests themselves run.

{{snippet:run}}

The run helper forwards the real runner exit code; invoke it with
`pwsh -NoProfile -File ./Run-Collector.ps1` followed by those parameters if the
calling shell must remain open. Use a new results directory for each mode.

## Constraints

TestElement is mutable and is not retained in the record. Do not mutate it to
evade an event property's contract. The observations describe events VSTest
delivered; a start event supplies no final outcome. These real top-level tests
verify IsChild=false only. No child=true transport, nested-framework hierarchy,
event ordering across parallel hosts or universal callback delivery is inferred.
The collector is an out-of-process managed extension, not an in-process collector
or a substitute for ordinary test-result reporting.

The API direction control compiles assignment to all five properties only as an
authoring probe. It fails on 18.7 and succeeds on 18.11; it is excluded from the
ordinary collector build and never runs against production events. It does not
fabricate a removed-setter failure or count synthetic constructor fixtures as a
real test run. No approval to rewrite results follows from a public setter.

## Evidence

`eng/Test-QuayCollector.ps1 -Record` locks/restores/builds both actual dependency
sets, verifies the independently audited CLI ZIP, locked-restore dependency SHA512
content hashes and SDK/compiler/ref/runtime hashes, checks
separate constructor/visibility fixtures and the compile-only assignment control,
then starts the real matching CLI/test host and collector. The loaded ObjectModel
path identifies the matching CLI package. Real arithmetic assertions produce one
pass and one failure; all four observed events match TRX and the disabled control
retains identical identities/outcomes. Exact commands, closures, inputs and fresh
compile/run evidence use the existing sample workflow. Installed offline stdio
list/get checks full canonical Markdown, explicit target11/edition11, applicability
and evidence. PIN-959/PIN-963/actual-GA and unrelated Human gates stay open; no
recurring native/client matrix, deployment or public publication is added.
