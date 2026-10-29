# Replace removed VSTest session orchestration

## Goal

A fictional Tern harbour-reservation editor needs to discover tests, run selected
checks repeatedly, cancel a slow check and close its owned processes after the
platform removes pooled test-session APIs. Use the publisher-documented console
protocol connection and synchronous discovery/run calls. Do not promise the same
pooled testhost lifetime or startup performance.

## Older approach

The original legacy call below compiles against TranslationLayer18.7.0 and fails
against18.11.0. It is an explicit unfinished/obsolete API control; its narrow CS0618
suppression does not apply to the replacement or claim stable old pooling support.
The old pooled route is compiled, not claimed as newly executed pooling proof.

{{snippet:legacy-session}}

Existing StartSession/DiscoverTests/RunTests/EndSession callers remain a valid
older approach: the complete session-free consumer below really executes with
18.7 on .NET10 as well as the current packages. There is no reason to invent a
.NET11-only discovery/run capability.

Save the old host as samples/TernTesting/TernHost.csproj and its package boundary
as samples/TernTesting/Directory.Packages.props:

{{snippet:old-project}}

{{snippet:old-packages}}

Its test project is samples/TernTesting/Tests/TernTests.csproj:

{{snippet:old-tests}}

## Newer approach

The public TranslationLayer README describes custom tools communicating with
vstest.console. Its StartSession owns that **console connection**; it is not
StartTestSession and creates no promise of a pooled ITestSession. Keep the complete
matching Microsoft.TestPlatform.CLI console directory, including its deps/runtime
config/extensions; do not copy a single SDK DLL or mix platform release sets.

Save the current11 host and packages under samples/net11.0/TernHost:

{{snippet:new-project}}

{{snippet:new-packages}}

{{snippet:new-tests}}

The current package route is also independently compiled/executed on .NET10,
under samples/net10.0/TernHost. This is a conditional package-adoption alternative,
not another foundation:

{{snippet:current10-project}}

{{snippet:current10-packages}}

{{snippet:current10-tests}}

Save the common host as samples/TernTesting/Host.cs. The synchronous platform call
runs on an owned task only for the cancellation exercise; no obsolete async VSTest
API is used. Cancellation waits for a real adapter readiness message. Each run has
fresh result state, including final chunks, distinct identities and platform totals.
Finally closes the console connection and verifies observed owned testhosts exit.

{{snippet:host}}

Save samples/TernTesting/TernTests.cs. This original adapter is loaded into the real
VSTest testhost. Its arithmetic assertions produce two passes and one deliberate
failure; cancellation is acknowledged by Cancel. Source-based execution honours
the real platform filter. It also supplies the separate settings recipe's test.

{{snippet:adapter}}

## Migration

1. Record the caller's actual SDK and package artifacts. Replace the old pooled
   StartTestSession/StopTestSession/ITestSession dependency with console connection
   ownership plus ordinary discovery/selected-test runs. The publisher's public
   README supports this route. No automatic pool/session adapter or MTP conversion
   is advertised. Retain a working old session-free caller unchanged when suitable.
2. Create the directories and complete files above. Pin each global.json with
   sdk.version=10.0.402 or11.0.100-rc.2.26504.105, rollForward=disable. Each project
   already declares target/language/output/nullable/implicit-usings. Put the same
   SDK pin beside each nested test project too. Put LegacySession
   in a separate compiler probe, not in the current host's compilation.
3. Restore and build TernHost.csproj and Tests/TernTests.csproj with that directory's
   absolute dotnet executable in Release. Keep the generated packages.lock.json and
   use restore --locked-mode thereafter. Source paths are relative to the shown
   layout. The public packages stay manually maintained outside root central pins.
4. Locate the matching CLI package's contentFiles/any/net10.0/vstest.console.dll in
   the actual restore cache. Keep that entire directory. Run the built TernHost.dll
   using the same absolute dotnet executable, passing the absolute console DLL and
   built Tests/bin/Release/net10.0-or-net11.0/Tern.TestAdapter.dll as its two arguments.
   Run from an unrelated directory. DOTNET_HOST_PATH and DOTNET_ROOT/X64 must select
   the intended child runtime; use the qualification helper for exact setup.
5. Expect four distinct discovered tests. Each of two ordinary runs and the run
   after cancellation returns total3/passed2/failed1, with no double-counted final
   chunk. The ready-signaled cancelled run returns IsCanceled=true and zero results.
   The intentional failed assertion is expected data; the host exits0 only when all
   aggregate checks and owned-testhost cleanup pass. A platform error/abort is failure.
6. Run eng/Test-TernTesting.ps1 -Record to repeat all three real dependency/runtime
   lanes and compile the old session calls successfully against18.7 and unsuccessfully
   against18.11. Keep console connection cleanup in finally in production callers.

## Constraints

The measured comparison is SDK **10.0.402**, C# **14**, runtime **10.0.13**
versus SDK **11.0.100-rc.2.26504.105**, C# **15**, runtime
**11.0.0-rc.2.26504.105**, on Windows x64. Public platform packages are pinned
independently: **18.7.0** and **18.11.0**, with NuGet-generated locks. Both CLI
packages actually ship a **net10.0** console, not a console TFM inferred from
package major numbers. TranslationLayer/TestHost/ObjectModel select their actual
**net8.0** assets; Build supplies **netstandard2.0**. The original adapter uses
matching Microsoft.NET.Test.Sdk/TestHost/ObjectModel. No workload, native compiler,
provider, rendering mode, device, credential or external service is required.

Public18.11 is a stable package set. The .NET11 SDK/runtime proof remains RC2;
no experimental opt-in is required for the replacement route. The SDK's bundled
18.7.0-release-26505-111 and 18.11.0-release-26504-105 files are separate artifacts,
not claimed byte-identical to public packages. The original package source commits
are 6f58ced50b40e074a07ffc21fb2d2eef95d31b60 and
36206450d47fec22e281ba9846445edc796f5080. Exact ZIP hashes, resolved asset paths,
compiler output, runtime output and SDK inventory are retained in sample evidence.

Only the original bounded Windows x64 consumers are qualified. No .NET Framework,
older runtime, other OS/architecture, arbitrary third-party adapter/collector,
remote execution, trimming, NativeAOT or general plugin-security promise follows.
No change to ordinary runner selection is needed. MTP is not assumed to be a
drop-in replacement. JSON API removal and pooled-session removal are separate facts;
neither is repaired by adding Newtonsoft.Json. Preserve existing working older
routes and the exact eight .NET10 foundations.

## Evidence

Run eng/Test-TernTesting.ps1 with -Record and the two absolute dotnet executable
paths to capture fresh verification-evidence.json records. The helper uses the
existing Evidence.ps1 input-hash/dependency workflow, fresh isolated CLI homes and
package caches, real locked builds, real console processes, real testhost/adapter
execution, compiler failure controls, plugin loading and effective-settings TRX.
It sets DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in each child, checks process/user/machine
PATH and never edits persistent settings. Disposable raw output stays in artifacts/.
Compilation records are not mocks or cross-compilation. All source, shared config,
locked dependencies and capture inputs must remain unchanged when reusing proof.

Build and pack the ordinary Playbook tool, then use the Inspector walkthrough's
isolated local-feed installation. Real installed stdio list_recipes/get_recipe from
an unrelated directory must return this ID/title/goal, full canonical Markdown,
explicit target/edition, applicability and current evidence. Retrieval uses embedded
content; unusable proxies establish proxy independence, not network isolation.
PIN-959 qualifying coverage/evidence, PIN-963 actual-GA reconciliation and pending
Human gates remain open. This delivery authorizes no public publication.

## Sources

- [Publisher TranslationLayer custom discovery/execution contract](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.VsTestConsole.TranslationLayer/README.md).
- [Current public console protocol wrapper](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.VsTestConsole.TranslationLayer/VsTestConsoleWrapper.cs).
- [Current resolver event construction and requestor contract](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.PlatformAbstractions/Interfaces/Runtime/IAssemblyResolver.cs).
- [Actual .NET resolver has no requesting assembly](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.PlatformAbstractions/netcore/Runtime/PlatformAssemblyResolver.cs).
- [Current task quoting and explicit semicolon limitation](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.Build/Tasks/TestTaskUtils.cs).
- [Public independently distributed console package](https://www.nuget.org/packages/Microsoft.TestPlatform.CLI/18.11.0).
- [Public translation-layer package](https://www.nuget.org/packages/Microsoft.TestPlatform.TranslationLayer/18.11.0).
- [Public test task package](https://www.nuget.org/packages/Microsoft.TestPlatform.Build/18.11.0).
- [SDK source baseline audited for actual compiler/tooling contracts](https://github.com/dotnet/dotnet/tree/a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e).
- [Locked NuGet restore](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies).
