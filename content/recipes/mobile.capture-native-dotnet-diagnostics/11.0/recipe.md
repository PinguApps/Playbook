# Collect native crash reports and retain matching symbols

## Goal

Enable the workload crash writer and deliberately retain matching diagnostic artifacts when investigating a native app failure.

## Newer approach

Sandpiper investigates a failed marsh-survey checkpoint on Android and iOS. Merge the appropriate configuration fragment into the owning app. EnableCrashReport opts into the runtime writer; generating symbols or an archive alone does not collect a crash. Android applies the property to CoreCLR or NativeAOT and ignores it for Mono. The Apple workload sets DOTNET_EnableCrashReport at startup and writes JSON reports beneath the app cache directory. Cache files may be evicted; arrange explicit consent and secure export after restarting the app. No upload service is configured here.

For the Apple distribution, ArchiveOnBuild and NoDSymUtil=false express archive/dSYM retention intent. Keep portable managed PDBs and the exact published binary/dSYM/archive together with target, RID, runtime and build identity. IpaIncludeSymbols=true requests Apple's converted Symbols output in an IPA; the dSYM directory itself is not embedded. Its default inclusion is not proof a report was collected, uploaded or symbolicated. Keep the default unless a specific distribution policy requires opting out; retain the matching symbols externally when opting out. These configuration-library samples expose the authored choices; they do not produce an IPA or a deliberate crash.

{{snippet:net11-sandpipercrashandroid}}

{{snippet:net11-sandpipercrashapple}}

## Older approach

The .NET10 fragments preserve archive and symbol intent without claiming the new crash-writer property exists. Use the established Android logcat/tombstone or Apple device/Xcode crash-log collection for the exact deployed build. Those remain valid approaches on newer apps too. OS reports and managed in-process JSON have different content and collection paths. No backported EnableCrashReport route is claimed.

{{snippet:net10-sandpipercrashandroid}}

{{snippet:net10-sandpipercrashapple}}

## Migration

1. Identify the failing operation, runtime, RID and published build. Select the Android or Apple fragment for that app.
2. Enable the writer on the .NET11 app. Do not substitute a symbol-generation switch or a late managed environment assignment for startup configuration.
3. On Apple, retain the exact archive, dSYM and PDB outputs; choose IPA symbol inclusion intentionally before signing. On Android retain the matching APK/AAB, managed symbols and native symbols where produced by that runtime.
4. On an authorized disposable device/build, reproduce the failure and export the actual JSON or OS report before cache eviction. Never deliberately crash a production session.
5. Match reports to build identity, inspect managed/native frames and symbolize with matching artifacts. Confirm collection independently from symbol packaging. These device actions were not executed here.

## Constraints

Requires a compatible .NET11 native workload composition and platform toolchain. Source preparation is RC2 evidence, not GA or native execution. Crash configuration requires no service account. Unavailable native checks are sample-specific not-tested exclusions under PIN-699; no Human proof is pending.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Run eng/Test-SandpiperFieldwork.ps1 -Record for exact official input hashes and feasible older configuration compilation. Canonical current native compilation and all native runtime behavior are explicitly not-tested under PIN-699. The current fragments have not been restored: their lock-file setting requests a lock file on a future supported restore, but no resolved dependency set or committed lock file is claimed. Packed stdio list/get verifies offline guidance delivery only. No GA, symbolication or crash collection is inferred; no Human proof is pending. The eight .NET10 foundations and comprehensive/GA gates remain unchanged.
