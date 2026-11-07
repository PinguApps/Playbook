# Run platform checks inside a native app process

## Goal

Use native MTP test templates when a check needs platform APIs and an application process rather than the desktop test host.

## Newer approach

Sandpiper checks that the platform cache location used for survey checkpoints is an absolute native path. The Android and iOS examples replace generated placeholder tests with that original assertion. Keep the template-generated instrumentation or UIApplication bootstrap: a class containing MSTest attributes alone is not a native test app.

Create an Android project with `dotnet new androidtest -n SandpiperPlatformAndroid --no-restore`. Create an Apple project with `dotnet new iostest -n SandpiperPlatformIos`; the pinned Apple templates reject --no-restore. The other supported short names are tvostest, macostest and maccatalysttest. Each generates an MSTest4.3.2/MTP app; Android uses instrumentation, iOS/tvOS use an app process on a device/simulator, and macOS/Mac Catalyst use a desktop app process. These are workload templates, not MAUI UI automation templates.

Select Microsoft.Testing.Platform in global.json's test.runner (the templates generate it), select the compatible SDK/workload and run `dotnet test --project ./SandpiperPlatformAndroid/SandpiperPlatformAndroid.csproj` or the corresponding Apple project. Start/connect the appropriate device first. Use `dotnet test --help` with that SDK for available device selection options; do not pass VSTest-only arguments blindly. On Apple, build and run on a Mac with the matching Xcode and any required signing. No connected mobile devices or Apple environment were available here.

{{snippet:net11-sandpiperplatformandroid}}

{{snippet:net11-sandpiperplatformios}}

## Older approach

.NET10 does not supply these .NET11 native MTP template/invocation contracts. Keep pure survey calculations in ordinary host tests, and use a compatible existing device-runner app/XHarness arrangement for native APIs. This recipe does not invent an older androidtest/iostest command, port the new template to .NET 10 or claim a desktop cache-path test validates mobile storage. There is no directly equivalent older sample to compile for this specific template capability.

## Migration

1. Separate pure logic tests from checks that require Android/Apple APIs or app lifecycle.
2. Generate the appropriate native test app with the short name above and retain its runner/bootstrap and global.json selection.
3. Replace the generated pass/fail/skip placeholders with an original platform assertion; the canonical samples below check the checkpoint cache location.
4. Restore the pinned MSTest package with compatible workloads and inspect the generated project and platform manifest. Run dotnet test with --project against that app.
5. Check actual discovered/passed/failed counts and native deployment errors. Generation, package retrieval and desktop compilation do not establish that tests ran on a device. Keep these checks out of recurring Playbook CI.

## Constraints

Requires a compatible .NET11 native workload composition and platform toolchain. Source/template package preparation is RC2 evidence, not GA or native execution. Native test apps require MSTest4.3.2 and an MTP runner. Unavailable native checks are sample-specific not-tested exclusions under PIN-699; no Human proof is pending.

## Evidence

Run eng/Test-SandpiperFieldwork.ps1 -Record for exact official input hashes and five actual template generations. Canonical current native compilation and all native runtime behavior are explicitly not-tested under PIN-699. Actual template generation uses the host SDK10 engine with exact RC2 template packages; it does not establish a composed SDK11 native build. Packed stdio list/get verifies offline guidance delivery only. No GA or device-test execution is inferred; no Human proof is pending. The eight .NET10 foundations and comprehensive/GA gates remain unchanged.
