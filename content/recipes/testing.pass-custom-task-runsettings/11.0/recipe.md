# Pass custom VSTest runsettings through the current task contract

## Goal

A fictional Tern build invokes the VSTest MSBuild task directly and needs custom
settings to reach its real tests unchanged. Migrate the former string-array task
parameter to its current scalar string contract. Preserve spaces, backslashes,
slashes, equals signs and switch-looking text; route literal semicolons through
an XML settings file instead of promising unsupported scalar quoting.

## Older approach

The actual18.7 VSTestTask and VSTestTask2 parameter is string[]. A C# caller assigning
an array compiles there and fails with CS0029 against18.11. A normal MSBuild task
attribute already looks scalar, but its old array binding preserves an escaped
%3B inside one item. That old behavior is executed as a control, not dismissed.

This canonical C# contract compiles the array branch against18.7 and the scalar
branch against18.11 for **both** task types. Define LEGACY_PLATFORM only for the old
branch. References must include the matching Build package DLL and the selected
SDK's Microsoft.Build.Framework/Microsoft.Build.Utilities.Core assemblies.

{{snippet:task-contract}}

Use samples/TernTesting/Directory.Packages.props and Tests/TernTests.csproj:

{{snippet:old-packages}}

{{snippet:old-tests}}

## Newer approach

Current VSTestCLIRunSettings is string on both task types. C# callers must assign
one scalar instead of an array. MSBuild callers supply entries separated by
semicolons or newlines. TestTaskUtils trims/splits the scalar after MSBuild
unescaping, quotes each entry and appends them after the -- boundary. Empty entries
are ignored. This is **CLI key=value settings**, not XML RunSettings content.

For standalone package acquisition, save this as Acquire.csproj in a separate
directory and restore with SDK10.0.402. It pins the public current packages; use
TernPlatformVersion=18.7.0 only for the historical comparison. Keep its generated
lock and use locked restore afterward. GeneratePathProperty exposes the actual
PkgMicrosoft_TestPlatform_CLI and PkgMicrosoft_TestPlatform_Build restore paths.

{{snippet:acquire}}

Save samples/TernTesting/RunTests.proj. TernTaskAssembly points to the actual matching
Build package DLL; TernConsole points to the complete matching CLI package's console.
VSTestSetting, when used, is a separate XML file path. The logger's escaped semicolon
belongs to its own array parameter and is not the new CLI-setting delimiter rule.

{{snippet:task}}

Use current11 packages and test project under samples/net11.0/TernHost:

{{snippet:new-packages}}

{{snippet:new-tests}}

The same current task/package route is independently executed on .NET10:

{{snippet:current10-packages}}

{{snippet:current10-tests}}

Save the real test adapter as samples/TernTesting/TernTests.cs. Source-run filtering
selects only Settings, so DeliberateFailure and Cancellation are not executed by
this task. The actual adapter reads IRunContext.RunSettings.SettingsXml and checks
three exact values. It logs the effective values into the real TRX; it does not
substitute a hand-built XML object for task/runner execution.
Assembly-based settings runs reject a missing Tern block. The shared session
fixture's selected-test runs check the fully reserved harbour when no custom
settings are supplied.

{{snippet:adapter}}

### Literal semicolons are not representable in the current scalar path

A setting value east;west is deliberately tried as an MSBuild-escaped item,
Tern.RejectSemicolon=east%3Bwest. Old array binding preserves it and its real test
passes. The current scalar receives a plain semicolon and splits it; the extra
west token is rejected. Adding quotes, doubling separators or using %3B is not a
supported repair. Never pass untrusted text as a new set of CLI settings.

For this value, create an XML file with a RunSettings root, a Tern element and
the following exact values. Save it as Semicolon.runsettings beside RunTests.proj:

{{snippet:xml-settings}}

Save XmlTests.proj in the same directory and invoke its Check target with the same
task/console/test/results properties as RunTests.proj. It sets TernSettingsFile
to the XML file and clears the CLI scalar:

{{snippet:xml-task}}

The real
current task/testhost reads that file and the same Settings test passes once,
including the literal semicolon assertion. The qualification helper generates and
executes this exact alternative. XML element text and CLI entry delimiters are
different contracts; do not teach an XML document as VSTestCLIRunSettings.
The helper verifies all four effective XML values in the real TRX, including
RejectSemicolon=east;west. A separate real run with an empty RunSettings document
must fail its selected settings test with the explicit missing-Tern error.

## Migration

1. Inventory the custom task assembly and console/testhost closure. Pin18.7/18.11
   independently of SDK versions. Ordinary dotnet test runner-selection guidance
   stays separate. Both task types expose the changed property, although the real
   invocation here uses VSTestTask; VSTestTask2 runtime execution is not claimed.
2. Create the package boundaries and test projects above. Pin each parent global.json
   to10.0.402 or11.0.100-rc.2.26504.105 with rollForward=disable; copy the pin
   beside each nested test project. Create the shown
   common sources. Restore/build Tests/TernTests.csproj in Release, retain the actual
   packages.lock.json and then use locked restore. TestSdk brings matching TestHost.
3. Acquire the matching Build/CLI packages through a normal PackageReference restore
   with ExcludeAssets=all and GeneratePathProperty=true, as in the companion TernHost
   sample. Their deployment is independent of SDK-bundled console files. Build uses
   lib/netstandard2.0/Microsoft.TestPlatform.Build.dll; retain the full CLI package's
   contentFiles/any/net10.0 directory with runtimeconfig/deps/extensions.
4. Invoke the shown RunTests.proj Check target using the intended absolute dotnet
   executable's msbuild command. Supply absolute TernTaskAssembly, TernConsole,
   TernTests (built Tern.TestAdapter.dll), TernAdapterDirectory and TernResults paths
   as MSBuild properties. Set DOTNET_HOST_PATH to that executable in the child
   environment so the task uses it without PATH changes. For arbitrary property
   values remember MSBuild command-line escaping is another boundary.
5. Verify the real settings.trx reports exactly total1/passed1, and TERN_EFFECTIVE
   contains Harbour=North Quay, `Pattern=^Tern\.Bay/West=13$` and
   Literal=--listTests=not a switch unchanged. The switch-looking value remains
   text after --; it must not change discovery or become an option.
6. Use the separately executed XML-file route for any semicolon-bearing value.
   Run eng/Test-TernTesting.ps1 -Record to repeat old/current10/current11 task
   invocations, old array/current assignment failure, old escaped-semicolon success,
   current scalar rejection and current XML success. It saves commands, selected
   dependencies, complete actual TRX and fresh compilation/execution evidence.

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
