# Read VSTest messages through the supported typed contract

## Goal

Read a known test-platform message without depending on removed Newtonsoft-based
communication types. Parse its header, choose the known payload contract, then
validate the data. Cwm Laboratory's independent bridge receives a completed
calibration result for batch `vial-204`: the selected test, bench trait, outcome,
duration and run totals must all survive. This is an original local protocol
example, not an implementation of VSTest's process transport or negotiation.

## Older approach

The actual **TestHost 18.7.0** package supplies CommunicationUtilities and depends
on matching ObjectModel **18.7.0** plus Newtonsoft.Json **13.0.3**. This valid older
.NET 10 consumer uses SDK **10.0.402**, C# **14** and runtime **10.0.13**:

{{snippet:older-project}}

Its isolated `Directory.Packages.props` is:

{{snippet:older-packages}}

The original previous reader explicitly materializes the old `VersionedMessage`
before reading its `Payload`. Merely calling `DeserializeMessage` did **not**
populate `Payload` on the old default fast path either. The two public old
resolvers select the platform converters for protocol1 versus protocol2:

{{snippet:legacy}}

The old typed `DeserializeMessage`/`DeserializePayload<T>` path also works and is
preferable when its supported contract already meets the goal. Upgrading does not
make every previous technique invalid. The incompatible part is accessing removed
types/members or building Newtonsoft configuration around them.

## Newer approach

Acquire **Microsoft.TestPlatform.TestHost 18.11.0** from nuget.org using its
documented **direct reference for advanced hosting**. Its packaged README permits
that acquisition route; ordinary test projects normally get it through Test.Sdk.
An independent bridge can use the public typed reader. This does not promise a
stable custom-serializer extension API or make internal converters supported.
Do not copy SDK-root DLLs, add an imaginary CommunicationUtilities NuGet package,
or mix platform release sets. Keep the complete resolved runtime closure.

The new .NET 11 project uses SDK **11.0.100-rc.2.26504.105**, C# **15** and runtime
**11.0.0-rc.2.26504.105**. CommunicationUtilities/ObjectModel still select their
actual **lib/net8.0** compile and runtime assets:

{{snippet:newer-project}}

{{snippet:newer-packages}}

The independently compiled and executed .NET 10 route uses the same current
package pin, SDK10.0.402/C#14/runtime10.0.13:

{{snippet:current10-project}}

This conditional package-adoption route is not another .NET 10 foundation. No
.NET 8/9/Framework route is advertised from asset-directory presence alone.
TestHost18.11.0 is a stable public package; the .NET11 SDK/runtime proof remains
RC2. The SDK's **18.11.0-release-26504-105** copies are separate artifacts, not the
downloaded18.11 bytes used here. No experimental API or opt-in is required.

Save this common source as `samples/LaboratoryMessages/LabRun.cs`. The statistics
property deliberately uses the supported **ITestRunStatistics** contract:

{{snippet:payload}}

Save `samples/LaboratoryMessages/TypedBridge.cs`:

{{snippet:bridge}}

`LEGACY_PLATFORM` is defined only by the old project. The current branch uses
`Message.Version` directly; there is no current `VersionedMessage` cast.

### Removed surface and the actual replacement boundary

| Previous public surface named by the note | Treatment for this laboratory goal |
| --- | --- |
| `Message.Payload` (`JToken`) | Read the known `LabRun` with `DeserializePayload<LabRun>(message)`. No DOM extraction or substitute parser. |
| `VersionedMessage.Version` | Current `Message.Version` carries the header value; validate it before decoding. |
| `DefaultTestPlatformContractResolver` | Remove caller-owned platform resolver configuration; the typed serializer owns the protocol2 graph. |
| `TestPlatformContractResolver<T>` in the note | The actual18.7 assembly exports **nongeneric `TestPlatformContractResolver1`**, not that generic type. The old sample compiles the real class for protocol1; the new typed reader handles that known graph. No generic replacement is invented. |
| `TestCaseConverter`, `TestObjectConverter`, `TestResultConverter` | One original payload demonstrates their shared goal: selected `TestCase`, `TestObject` bench trait and `TestResult` outcome/duration all survive both protocol1/2 and old/new wire reads. The platform chooses its converters. |
| `TestRunStatisticsConverter` | Use **ITestRunStatistics** in the same payload. Direct typed interface reads preserve ExecutedTests and Passed counts. Do not assume concrete `TestRunStatistics` is equivalent. |

The measured18.11 concrete `DeserializePayload<TestRunStatistics>` retains
ExecutedTests=1 but returns Passed=0 for the original totals message. The interface
read returns1 for both. This limitation is an asserted control, not a fabricated
replacement for a missing extension point. Existing custom resolver inheritance,
arbitrary converter registration, custom Newtonsoft attributes/settings and all
other payload graphs need their own design/proof. There is **no public replacement
advertised here for those subgoals**. Do not access remaining internal converters.

Every named previous symbol compiles against the old package and fails against
the current one with CS1061/CS0246/CS0122, **even with a direct Newtonsoft13.0.4
reference**. The real old executable with a current matching platform assembly
set and its Newtonsoft DLL still fails at runtime on a removed contract. Rebuild
against supported APIs; adding Newtonsoft restores neither source nor binary
compatibility. JSON ownership in tests/extensions is a separate recipe.

### Header, payload and protocol semantics

- `Message.MessageType` is the routing key. `RawMessage` is the **entire unchanged
  received JSON envelope**, including its payload. Current `ToString()` returns
  that raw text; an empty current Message returns `{}`. It is not a payload API.
- Serializing with protocol1 omits Version; current `DeserializeMessage` reports
  **0**. Explicit protocol2 reports **2**. This bridge accepts only its negotiated
  0/1/2 set and exact completion kind. It rejects99 before decoding; the actual
  serializer also rejects99. This is not a claim that VSTest supports only0/1/2.
- Canonical header parsing can succeed on truncated payload JSON. Reordered or
  whitespace-prefixed input uses the JSON fallback. A successful header is not
  payload validation. Always decode and check required domain values.
- `DeserializePayload<LabRun>(null)` returns null on both packages; the new empty
  Message without RawMessage also returns null. The bridge rejects a null payload
  or missing domain fields with InvalidDataException. Malformed/incompatible JSON
  throws Newtonsoft.Json exceptions on18.7 and System.Text.Json.JsonException on
  the actual modern18.11 consumer. Handle failures at the receiving boundary;
  never count a header-only result as a completed laboratory batch.

The public `JsonDataSerializer.Instance`, `DeserializeMessage`,
`DeserializePayload<T>`, `Deserialize<T>(string,int)`, `SerializeMessage`, both
`SerializePayload` overloads, `Serialize<T>(T,int)` and `Clone<T>` remain callable.
The checks below execute every operation; no removed serializer-settings setter
or converter factory is invented.

## Migration

1. Inventory the caller's package/assembly provenance and payload contract. For
   this bounded bridge, pin TestHost18.7/18.11 separately and use the matching
   ObjectModel closure. The official removal note introduces the change in
   .NET11 Preview4; it does not replace actual package qualification.
2. Lay out the projects and linked common sources at the shown paths. Put the
   legacy reader beside LabRun; define LEGACY_PLATFORM only in the old project.
   Keep examples isolated from root central-package inheritance. Use the complete
   project/central-package snippets above, normal nullable/implicit usings and
   executable output. A standalone copy needs those explicit common properties.
3. Replace Payload access and VersionedMessage casts with the typed bridge.
   Remove public platform-resolver/converter imports. Use ITestRunStatistics and
   validate the graph; do not silently accept the measured concrete-count loss.
4. Add `SurfaceChecks.cs` and `Program.cs` below. Pin each SDK with global.json
   (`rollForward: disable`), restore from nuget.org and commit the actual generated
   packages.lock.json. Use `restore --locked-mode` thereafter. Full old closure is
   TestHost/ObjectModel18.7 + Newtonsoft13.0.3; full new closure is
   TestHost/ObjectModel18.11, with no Newtonsoft dependency.
5. Build and publish framework-dependent output, retaining package runtime
   assets. Do not mark this executable's TestHost reference `PrivateAssets=all`:
   that excludes the dependency from these publish outputs. Run the published
   **LaboratoryBridge.dll** from an unrelated directory with the intended runtime;
   do not invoke testhost.dll or assume a test extension will load these copies.
6. Run `eng/Test-VSTestMessages.ps1 -Record` with the two absolute healthy dotnet
   executables. It checks package/source hashes and selected compile/runtime
   assets, executes the real compilers, then builds/publishes all three consumers.
   It exchanges actual old/new serialized protocol1/2 messages in both directions,
   checks failures and all removed symbols, and saves fresh sample evidence.

The sample common `Directory.Build.props` sets OutputType=Exe, Nullable=enable,
ImplicitUsings=enable, TreatWarningsAsErrors=true and Deterministic=true; its
Directory.Build.targets is empty. No workloads, native dependencies, providers or
rendering modes are involved. Package pins stay author-maintained, outside
automatic dependency updates.

## Constraints

This route is bounded to the original known graph on Windows x64 with the exact
SDKs, runtimes and public packages stated above. No provider, rendering mode,
workload or native toolchain is required. Arbitrary custom resolver/converter
extension subgoals have no public replacement established here. Other payloads,
Framework/older targets, other OS/architectures, test execution/transport,
extension loading, trimming and Native AOT remain unverified. RC2 proof does not
clear actual-GA, comprehensive-coverage or independently pending Human gates.

## Verification

Save this as `samples/LaboratoryMessages/SurfaceChecks.cs`. Only public exported
types are inspected; no internal converter is accessed or invoked:

{{snippet:surface}}

Save `samples/LaboratoryMessages/Program.cs`. The original checks run against both
actual package sets; `--write <prefix>` saves real wire JSON, while `--read <file>`
checks a foreign message through the actual platform reader:

{{snippet:checks}}

Expected output includes two `PASS protocol=...` lines, null/malformed rejection,
`PASS fast-header-is-not-payload-validation`, `PASS retained-serializer-members`,
`PASS statistics-interface` and `PASS laboratory-bridge`. Independent deployed
net10/current-net10/net11 runs and foreign-wire checks are real execution evidence,
not cross-compilation or a substitute JSON parser. The harness preserves exact
commands, inputs, dependency hashes and runtime/SDK/compiler observations through
the existing verification.json/verification-evidence.json workflow.

## Evidence

Saved compile-and-run records cover the old-net10, current-net10 and current-net11
projects with exact dependency locks and input hashes. The compiler, published
runtime and foreign-wire commands actually ran through the platform assemblies.
The old/current CommunicationUtilities SHA-256 identities match the audit's
303a9349...a78cb5 and c3eff269...e7c717 entries respectively; these are not inferred
from an SDK-root file. Complete values and selected assets remain in the source
asset manifest and fresh sample records.

Installed-tool inspection separately verifies offline ID/title/goal, full expanded
canonical code, target/edition, applicability and current evidence. It is not
evidence of test execution, remote VSTest communication, extension injection,
other platforms, arbitrary custom contracts, trimming, Native AOT or actual GA.
PIN-959 qualifying coverage/evidence and PIN-963 actual-GA reconciliation remain
open; existing Human gates and the exact eight foundations remain unchanged.

## Sources

- [Official VSTest public API removal](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/11/vstest-removes-newtonsoft-json).
- [Publisher TestHost18.11 advanced-host acquisition](https://www.nuget.org/packages/Microsoft.TestPlatform.TestHost/18.11.0).
- [Public serializer at the actual18.11 package source commit](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.CommunicationUtilities/JsonDataSerializer.cs).
- [Modern serializer and protocol implementation](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.CommunicationUtilities/JsonDataSerializer.Stj.cs).
- [NuGet locked restoration](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies).
