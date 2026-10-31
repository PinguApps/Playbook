# Coordinate mobile file access without relying on advisory locks

## Goal

Serialize a shorebird-observation journal's writes in one app process without depending on FileShare exclusions. Decide ownership first: one singleton per path is the complete boundary here. A SemaphoreSlim in the app cannot coordinate an extension, another process or a second Journal instance.

## Older approach: .NET 10 / C# 14

A short-lived exclusive open can request advisory OS locking on older iOS/tvOS routes. FileShare.None remains meaningful on platforms with sharing enforcement; it is not a portable guarantee against arbitrary native callers. The portable comparison below is compiled on Windows, not an Apple app. Add standard System.IO/Threading/Tasks imports (or enable ImplicitUsings).

{{snippet:older}}

## Newer approach: .NET 11 / C# 15

Current CoreCLR defaults disable advisory locking on **iOS and tvOS, including simulators**, because holding such locks while entering the background can cause OS termination. Those FileShare exclusions are no longer enforced by those locks, even between handles in one process. The reviewed Unix implementation's default excludes Mac Catalyst; other platforms keep their existing defaults. Do not generalize an untested runtime/platform combination from a release headline.

Register exactly one instance of this class per app-private journal path. All writers must use it. Waiting is cancellable; release occurs even when a write fails. The file open uses permissive sharing deliberately because the in-process gate owns serialization. Do not expose the path to another uncoordinated writer.

{{snippet:newer}}

Await every AppendAsync call and finish current work before disposing the singleton. Disposal requires all callers already stopped. Choose the app's sandbox data directory through its framework, rather than a working-directory-relative path. Avoid holding external resource ownership across background suspension; this helper is not a background-execution entitlement or crash-safe transaction. Cancellation after writing begins can leave a partial record; it does not roll back a file.

## Migration

1. Find each FileShare.None/Read assumption and list all writers, including extensions and native libraries.
2. For a single process, route every write for a path through one singleton instance and await tasks; register it with the app's normal service lifetime.
3. For multiple processes, select a platform-supported coordinated-storage protocol or a single writer with message passing. The singleton gate does not satisfy that requirement; this recipe makes no cross-process implementation claim.
4. Keep file opens short and handle I/O failures/cancellation. Test foreground/background transitions on the actual deployed runtime before claiming platform behavior.
5. Retain the compatibility setting only if owners explicitly accept its termination risk; prefer redesigning ownership.

## Compatibility setting and limits

To restore the older advisory-lock default on affected iOS/tvOS hosts, set the **startup runtime configuration property** `System.IO.DisableFileLocking` to `false`, or launch with `DOTNET_SYSTEM_IO_DISABLEFILELOCKING=0`. The setting is cached during initialization; do not toggle it after files are opened. Integrate the property through the app's supported runtime-configuration mechanism and inspect the produced runtime configuration. It is not an ordinary appsettings key.

The runtimeconfig property is placed inside the existing runtimeOptions.configProperties object:

{{snippet:locking-config}}

Restoring locking also restores the suspension-related termination risk. It does not grant background time, establish crash consistency or turn advisory locks into universal exclusion. No compatibility-switch execution on Apple is claimed here. Do not change a machine-wide environment variable to apply this per-app setting.

## Constraints and alternatives

Framework APIs only; no NuGet dependency, provider, rendering mode or experimental opt-in. The default change concerns the actual iOS/tvOS runtime; pin its app workload/native assets separately from the .NET SDK. Mac Catalyst is excluded from the reviewed changed default. Our host projects verify portable code and need no mobile workload; they do not establish an app's native build or execution. Existing platform storage/coordination APIs or a dedicated writer may fit multi-process data; qualify the selected implementation separately.

## Evidence

`eng/Record-NativeBoundaries.ps1 -VerifySources` checks immutable runtime notes and old/new SafeFileHandle.Unix implementation. Actual SDK 10.0.402/C# 14/runtime10.0.13 and SDK 11RC2/C# 15/runtime11RC2 Windows consumers verify journal contents and pre-cancelled writes; thirty concurrent calls through the new singleton preserve thirty distinct records.

**Not tested:** both canonical MobileJournal projects' iOS/tvOS integration, native compilation, advisory-lock enforcement, background suspension/termination and compatibility-setting behavior. This Windows machine has no usable Apple execution/Xcode route; the existing Docker setup does not supply it. These exact sample-specific exclusions follow PIN-699. Intended Apple toolchain pins are not measured versions. No requirement to obtain another environment or claim of Apple execution follows. Pingu owns feasible interactive approvals and refresh coordination. RC2 is not actual-GA proof.
