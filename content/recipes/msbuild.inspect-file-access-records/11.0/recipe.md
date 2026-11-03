# Read experimental build file-access records correctly

## Goal

Use this when an existing MSBuild project-cache observer needs to interpret directory enumeration and opened-file attributes correctly. **Experimental, documentation-based guidance.** The original decoder examples have not been compiled or exercised with a live event stream. Pingu authorized this provisional delivery on 3 November 2026; pending execution proof remains explicit.

## Older approach

Cwm Seed Bank's build observer records source reads, generated-report writes and input-directory enumeration. The previous decoder records the operation, requested access, OS error and requested attributes:

{{snippet:older}}

These values do not tell you which search pattern a directory enumeration used, or the attributes the kernel actually observed on the opened object. Keep OS failures alongside paths; do not interpret a failed read as proof that input bytes were consumed.

## Newer approach

The newer `FileAccessData` adds `EnumeratePattern` and `OpenedFileOrDirectoryAttributes`. Keep them distinct from `FlagsAndAttributes`, which records what the caller requested:

{{snippet:newer}}

A null enumeration pattern must remain absent, not be replaced by `*` or treated as proof of an empty directory. Opened attributes describe the opened file/directory; requested flags do not substitute for them. Preserve the raw operation and error rather than classifying every observed path as a successful content read or write. This small decoder deliberately omits process arguments, which can contain credentials.

### Construction and compatibility

The pinned older source has an eleven-argument constructor: operation, requested access, process ID, access ID, correlation ID, error, desired access, requested flags/attributes, path, process arguments and augmented-access flag. The pinned current source **retains that constructor** and adds a thirteen-argument overload appending enumeration pattern and opened-file/directory attributes. The retained overload delegates with `null` and `0` for the new fields. It is wrong to describe it as removed or to infer rich observations from its defaults.

Prefer decoding records delivered by the host. A hand-constructed record can test formatting or constructor defaults; it cannot prove instrumentation, event delivery or actual file access. Do not parse private MSBuild wire packets or claim cross-version serialization compatibility. Reconcile host and plugin binaries together. The source audit compares SDK **10.0.402** and **11.0.100-rc.2.26504.105**; assembly version `15.1.0.0` is not the MSBuild product version.

## Supported host boundary

Microsoft's project-cache specification restricts experimental file-access reporting to **Windows x64 Visual Studio `MSBuild.exe` from the command line**, using `/reportFileAccesses` or `BuildParameters.ReportFileAccesses`. It does not promise that `dotnet msbuild`, x86/Arm64 hosts or the Visual Studio IDE emit these reports. The ordinary .NET SDK can expose the record type without providing the native instrumentation route.

The existing project-cache plugin is discovered through `ProjectCachePlugin` items, or the documented programmatic descriptor. Reporting uses Detours on worker nodes. It disables the in-process node and task yielding so records can be associated with build requests. `/graph` concerns the cache-get scenario; `/reportFileAccesses` enables the file reporting needed by cache-add. Do not enable multithreaded mode or another unrelated experimental switch as a substitute.

The observer fragment belongs **inside an existing supported `ProjectCachePluginBase` subclass**, with imports for `Microsoft.Build.ProjectCache`, `Microsoft.Build.Experimental.ProjectCache`, `Microsoft.Build.Experimental.FileAccess` and `Microsoft.Build.Execution`, plus standard threading/task imports. It is not a complete distributable plugin. Bound and drain the illustrative queue per context in the application before using it on a large build:

{{snippet:observer}}

The callback's `FileAccessContext` preserves request association. Handle finalization only after the host reports project completion; its pipe synchronization flushes access reports before moving a worker to another request. Do not build a cache that publishes partial or cancelled observations as a successful result. The decoder has no cache-hit policy or persistence implementation.

## Migration

1. Identify the actual Visual Studio x64 host, its product version and matching Microsoft.Build/Framework/native instrumentation assets. Keep compiler SDK, app runtime and build-host requirements separate. The example projects reference SDK-local managed DLLs for intended decoder compilation; they are **not** plugin deployment or native-host qualification.
2. Keep the older decoder for an older host. Use the newer decoder only against a host exposing the added fields. Preserve the eleven-argument construction route when callers have no richer data; use the extended overload only when real observations supply its values. Do not invent values to make old records appear complete.
3. Integrate the callback in the existing plugin, keep context with every observation and avoid process-argument logging. Review output paths and access controls because even file names may be sensitive. Keep capture on disposable inputs and application-owned storage.
4. Invoke the supported host with `/reportFileAccesses` and `/nr:false` on the disposable project containing the configured plugin. Select an absolute executable path. No SDK, Visual Studio installation, PATH change or system-wide environment switch is performed by consulting this recipe.
5. In the seed-bank fixture, read an authored seed manifest, write a generated report and enumerate `*.seed` inputs. Capture **real** callback records, including error/operation/context, and compare old/new decoding. Verify a nonmatching entry is not inferred as matching merely because its directory was enumerated.
6. Cancel a build while the fixture is active. Verify cancellation reaches finalization and no successful reusable cache result is published for a partial context. Run an ordinary capture-disabled build as a control. Empty callback output must not be called success without establishing that the host, plugin and instrumentation actually ran.
7. Save actual old/current compilation, host/native identities, commands, inputs and event/cancellation observations through the existing sample-evidence workflow. Until then retain `official-review` and pending proof. Revalidate on relevant host, plugin or instrumentation changes; add no recurring native/OS matrix.

## Constraints

| Claim | Required check, not yet performed |
| --- | --- |
| Previous/current decoding | Compile both actual dependency sets; decode the same supported fixture observations without conflating requested/opened attributes. |
| Constructor compatibility | Compile the old overload on both; verify new-field defaults and the extended overload on the current set. Synthetic fixtures prove only this contract. |
| Read/write events | Observe real successful and failed I/O on disposable inputs through the plugin, preserving request association and OS errors. |
| Directory enumeration | Observe the real pattern and opened-directory attributes; distinguish absent patterns and unmatched files. |
| Opt-in/control | Compare enabled reporting with capture disabled; a successful ordinary build does not prove event delivery. |
| Cancellation | Observe a cancelled active build and ensure partial data is not published as successful cache evidence. |

## Evidence

Read-only discovery on this machine found x64 Visual Studio MSBuild **18.11.0.47511** and **18.10.1.42706**. Both accepted `/reportFileAccesses` on a disposable `WriteLinesToFile` project and exited zero. **No project-cache observer was registered, so this establishes switch acceptance only.** It does not qualify Detours delivery, native binary compatibility, the original decoder or cancellation. The prerelease SDK metadata and those Visual Studio versions are separate artifacts.

No exact external NuGet package closure or diagnostic-suppression ID is invented. Use the chosen host's documented plugin contract and reconcile any experimental diagnostics narrowly; do not globally suppress warnings to force compatibility. The examples use intended net10/C#14 and net11/C#15 library targets, with no external package or workload dependency. Their builds, native host integration and execution remain pending. No Linux/macOS, IDE, rendering, framework-host or Native AOT route is claimed. This target-11 edition preserves existing identities and the exact eight .NET 10 foundations; it adds no older-target discovery route. PIN-959 qualifying evidence and actual-GA reconciliation stay open.

## Official references

- [MSBuild project-cache design and Windows x64 reporting restrictions](https://github.com/dotnet/msbuild/blob/main/documentation/specs/project-cache.md)
- [Pinned current FileAccessData contract](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/FileAccess/FileAccessData.cs)
- [Pinned previous FileAccessData contract](https://github.com/dotnet/dotnet/blob/a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e/src/msbuild/src/Build/FileAccess/FileAccessData.cs)
- [Pinned project-cache callbacks](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/BackEnd/Components/ProjectCache/ProjectCachePluginBase.cs)
