# Choose the application host and device for a local watch loop

## Goal

Start the intended depot application host under a local watch loop. Keep framework, configuration and device selection explicit, and distinguish an Aspire resource model from one managed process or a deployed mobile application.

## Older approach

An ordinary SDK-10 project can be run or watched explicitly. For a multi-process application, existing Aspire tooling and separate project watch commands remain valid choices; watching one entry project does not by itself prove every child is hot-reloaded. The original previous-target host uses this project and program:

{{snippet:older-project}}

{{snippet:older}}

Set DEPOT_DOTNET to the absolute SDK executable used by the child audit command. The example's one executable resource only prints that SDK's information; it does not need a container engine, cloud account or application deployment.

## Newer approach

SDK 11 provides watch integration for Aspire hosts and device selection for supported mobile projects. Use `dotnet watch --project WatchAppHost.csproj --non-interactive --no-hot-reload run -c Debug --no-launch-profile` for the bounded startup check here. The explicit no-hot-reload check establishes host startup, not a full distributed hot-reload claim. The actual newer host is:

{{snippet:newer-project}}

{{snippet:newer}}

Both project variants use central package versions Aspire.Hosting.AppHost 13.4.6 and MessagePack 2.5.303, with versionless PackageReference entries and per-package overrides disabled. The direct MessagePack servicing pin avoids the older vulnerable transitive 2.5.302 baseline. The Aspire.AppHost.Sdk is also 13.4.6. This baseline uses the SDK-provided orchestration/dashboard packages with AspireUseCliBundle=false; no Aspire CLI is required. These are consumer prerequisites, not dependencies of the Playbook host.

For a supported mobile project, select its framework and device explicitly: `dotnet watch --project ./DepotMobile.csproj --framework net11.0-android --device <device-id>`. SDK 11 asks the project's ComputeAvailableDevices target for devices; one can be auto-selected, while several offer a picker. The selected device is forwarded through build/run, with re-restore when the required RuntimeIdentifier changes. This is source-reviewed guidance only: the current work has no compiled mobile sample or connected-device execution, and required proof is pending.

## Migration

1. Read the application's actual project target(s), global.json and host configuration. Use --project explicitly; do not let a neighbouring project determine the loop.
2. Prepare the pinned Aspire SDK/orchestration packages for the host route. DEPOT_DOTNET selects the actual child executable without changing PATH. The authoring harness also supplies DOTNET_HOST_PATH for the pinned build/run host. Use -c Debug for the intended local configuration; sdk.keep-build-configuration-explicit explains inherited defaults.
3. Exercise startup separately from hot reload. Remove --no-hot-reload only when validating changes supported by the application's actual resource model; a host process starting is not proof of child updates.
4. For mobile, install the matching .NET 11 workload and native SDK, provision a compatible device/emulator, confirm its identifier and select the intended target framework. Do not infer device availability from adb being installed.
5. For iOS simulator work, a suitable Mac, matched Xcode and iOS workload are required; the documented watch route requires MtouchLink=None. This Windows check establishes none of those prerequisites.
6. Capture the missing mobile build and device-selection result on the suitable authoring machine before treating this recipe or the combined ticket delivery as complete. Keep native UI/device checks out of ordinary tool CI.

## Constraints

Required mobile proof is pending and blocks publication as verified mobile guidance. PR review may proceed with this recorded gap. The available Android SDK has adb but no emulator directory or connected device. Installed preview workload manifests are not asserted to be the final RC2 composition. No Mac/Xcode route is available here. The primary metadata describes the managed Aspire host; the unverified mobile branch is not represented as a verified compatibility route.

The host's Aspire package contributes its net10.0 assets on target 11; Aspire.Hosting.Orchestration.win-x64 and Aspire.Dashboard.Sdk.win-x64 13.4.6 supply the measured orchestration/dashboard assets. Windows is the measured authoring platform. Actual local dashboard startup may initialize Aspire settings/certificates; stop the watch process when finished. This example uses local-only HTTP endpoints during its bounded check and does not authorize exposing the dashboard externally.

The SDK feature is distinct from the experimental status of any application API. No experimental API is used in this host. RC2 results do not establish GA verification. There is no browser-rendering, database-provider, container or mobile execution claim from this managed host compilation.

## Evidence

The original net10.0/net11.0 Aspire hosts compile against their exact locked SDK, package and orchestration baseline. The bounded host check records real watch startup separately from compilation, with no mock or hot-reload inference. Full installed retrieval preserves this pending Human proof and its selection warning. Mobile compilation, device selection and distributed hot reload remain incomplete evidence; a successful packed-tool check cannot discharge them.
