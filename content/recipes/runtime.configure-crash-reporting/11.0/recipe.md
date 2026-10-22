# Capture a crash report inside a failing process

## Goal

Collect useful evidence when a Unix CoreCLR dispatch worker terminates and an external diagnostics process is unsuitable. Configure capture before starting the worker and verify the produced artifact instead of relying on an ordinary exception message.

## Older approach

.NET 10 can launch createdump on a crash. Enable its minidump path and request a report only; DOTNET_EnableCrashReportOnly suppresses the memory dump while retaining the crash report. The valid external helper approach can require permissions to inspect the failing process and availability of createdump. It is not an in-process reporter.

{{snippet:older}}

## Newer approach

On supported Unix CoreCLR .NET 11 builds, DOTNET_EnableCrashReport=1 or DOTNET_EnableCrashReportOnly=1 enables the in-process reporter when DOTNET_DbgEnableMiniDump is not enabled. This original configuration selects report-only capture and an explicit private writable root.

{{snippet:newer}}

When DOTNET_DbgEnableMiniDump is enabled, createdump retains ownership of these report switches; do not claim the same in-process path. DOTNET_DbgMiniDumpName configures createdump naming, whereas DOTNET_CrashReportRootPath is the in-process reporter's root. In RC2 the latter creates `.dotnet/crash-reports/report-<timestamp>-<pid>.crashreport.json` underneath that root and also emits readable diagnostic output. Treat naming/schema as versioned diagnostics, not a durable ingestion contract. Without the explicit root, do not assume this fixture's file lifecycle or location.

## Migration

1. Create a net11.0/C# 15.0 console project with nullable and implicit usings. Add Configuration.cs above and this complete Program.cs. Run it only in a disposable Linux environment: it deliberately terminates a child with FailFast, checks the generated file and deletes its unique temporary directory. The parent is the verifier, not the process being diagnosed. No system environment variable is changed.

{{snippet:usage}}

The same program compiles on net10.0/C# 14.0 with the older Configuration.cs. Invoke the built DLL through its selected dotnet host, as the verification commands do. The program uses Environment.ProcessPath as that host and supplies its own DLL path; an apphost or NativeAOT invocation is not this fixture's route. Configuration is in ProcessStartInfo.Environment before startup, not Environment.SetEnvironmentVariable after the runtime has initialized. Deployment can supply the same variables in its service/container environment before launching the real worker.

## Constraints

The demonstrated route is Linux x64 CoreCLR. macOS support is source-reviewed, not executed here. Windows has different dump collection mechanisms; these Unix instructions do not establish Windows behavior. Mobile-derived implementation does not establish MAUI/device integration. NativeAOT has a distinct diagnostics/runtime path, and this example makes no NativeAOT capture claim. The pinned design documents DOTNET_DbgCreateDumpToolPath as a NativeAOT-only directory containing a separately supplied createdump binary; CoreCLR ignores that switch. Its declaration is source-review evidence, not a compiled or executed NativeAOT recipe route. No external tool, account or provisioning is needed for installed Playbook guidance.

A report is not a heap/core dump or a substitute for debugger-ready memory. Keep the older createdump/dump route when full memory is necessary and permissions/storage permit it; scope that separate configuration deliberately. Reporter failure, insufficient disk space, unwritable paths, abrupt kill, process corruption and external time limits can prevent complete diagnostics. SIGKILL cannot run an in-process reporter. Never depend on crash reporting for reliable application cleanup or business-data persistence.

Keep reports private: stacks, modules, exception messages and memory diagnostics can reveal sensitive data. Choose writable application-owned storage, retention/access controls and a collection policy. The fixture captures only its own synthetic message and uses no elevated privileges or host mounts for crash output. Existing production services are not crashed or reconfigured. Restore service/container configuration to undo rollout; existing captured reports require their normal retention policy.

## Evidence

Two network-disabled disposable Linux x64 containers compile the actual canonical projects under SDK 10.0.402 and 11.0.100-rc.2.26504.105. A real child FailFast exits 134. The .NET 10 log identifies createdump and its report; RC2 identifies the in-process reporter, managed DispatchFailure frame and module list. The generated report file, rather than stderr alone, contains DispatchFailure. The helper records exact output/artifacts and removes the temporary crash directory. This is real capture, not a mock or cross-compilation. Other platforms, full memory dumps and production failure modes are not executed; GA reconciliation remains required.
