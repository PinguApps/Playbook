# Quay integration checks

The original Quay samples cover selected BlazorWebView asset policy, C# UI
project creation, the first-party Avalonia template integration and public MAUI
backend contracts. Preparation baseline: SDK11.0.100-rc.2.26504.105,
MAUI11.0.0-rc.2.26505.5; older comparison SDK10.0.402, MAUI10.0.110.
These are RC2 authoring results, not .NET11 GA certification.

## Observed boundaries

| Check | Result and limit |
| --- | --- |
| QuayBackend net10/net11 | Both actual neutral package consumers compile and run. New callback allow-list retains no-store for HTML, queries, foreign origins and wrong MIME. No native WebView cache was exercised. |
| Backend services | Actual MAUI11 assemblies execute custom Blazor handler registration, scoped/idempotent alert contract, distinct disposable gesture managers and the public view screenshot dispatch. The screenshot service is an explicit contract double; it returns no pixels. |
| SingleProject | Actual Controls.Build.Tasks package targets select QuayMarker for the active neutral backend and no platform source for an inactive backend. This checks item selection, not a native renderer. |
| Original C# UI | net10 Windows build passes; a self-closing window reaches Label.Loaded and its managed button event changes Crates checked: 0 to 1 on runtime10.0.13. No pointer, visual or accessibility check is claimed. |
| Native11 C# UI | Actual net11.0-windows10.0.20348.0 build fails before compilation with NETSDK1147 naming maui-tizen. The isolated SDK11 installation lacks the usable MAUI SDK pack. Compilation and interaction are sample-specific not-tested under PIN-699. Restore did not complete, so this native11 head has no resolved lock file; its lock-generation property is a future restore request, not saved dependency evidence. |
| Templates | Actual Microsoft.Maui.Templates.net11 package generates C# UI, retained resource XAML, the single-project Avalonia option and separate desktop head. Both incompatible sample-content combinations produce their documented warnings. Generated Controls/Logging/Avalonia dependencies are pinned before restore. |
| QuayAvalonia | The original neutral package consumer compiles with actual bootstrap generation and raw MauiAsset conversion. It explicitly references Controls/Resizetizer rather than pretending to build the unchanged generated native heads. No Avalonia window, native embedding or real-view capture was executed. |

Pingu approved a **contract demonstration** for the backend: rendering, dialogs,
native gesture recognition and real-view capture are outside its scope. Unsupported
rendering/pixel operations throw rather than pretending to work. No separate Human
approval or provider/account is required for these contract checks. Apple/Android
device and unavailable native checks are not-tested, not promised future proof.

The [source/package manifest](../../samples/QuayIntegration/sources.json) retains
immutable official inputs and exact artifact SHA-256 values. Each canonical
consumer retains its own SDK pin, package lock, verification declaration and saved
evidence. Refresh affected records when inputs change, using the existing
Record-Evidence helper; do not edit hashes. Sample dependencies remain manually
maintained, with no new Renovate permission or native CI matrix.

## Reproduce the focused checks

PowerShell7.4+; use the installed absolute SDK paths below. Source/package review
and template setup require network access. The contract consumers require no
native workload. The Windows check uses only its own self-closing sample process.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2865bc2a'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
& ./eng/Test-QuayContracts.ps1
& ./eng/Test-QuayTemplates.ps1
Push-Location ./samples/net11.0/QuayBackend
try {
    $sdk11 = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
    & $sdk11 build -c Release -p:RestoreLockedMode=true
    & $sdk11 ./bin/Release/net11.0/QuayBackend.dll
    $tasks = 'C:\Users\pingu\.nuget\packages\microsoft.maui.controls.build.tasks\11.0.0-rc.2.26505.5\buildTransitive\netstandard2.0'
    & $sdk11 msbuild ./BackendSelection/Selection.proj -t:VerifySelection "-p:MauiBuildTasksRoot=$tasks"
    & $sdk11 msbuild ./BackendSelection/Selection.proj -t:VerifySelection "-p:MauiBuildTasksRoot=$tasks" -p:MauiActiveBackend=other
} finally { Pop-Location }
Push-Location ./samples/net10.0/QuayCSharp
try { & 'C:\Program Files\dotnet\dotnet.exe' build -c Release -p:RestoreLockedMode=true }
finally { Pop-Location }
& ./eng/Test-QuayWindows.ps1
```

Expect PASS for identities, template options, the cache policy and service
contracts, active/inactive item selection, and the loaded-window/count receipt.
The screenshot double proves registration/dispatch only. No settings, trust,
certificate store, deployments, package publication or account provisioning change.

## Inspect the installed guidance

The existing packed-tool harness installs the actual package using an isolated
feed/cache and unrelated working directory. Real stdio list/get responses retain
the full expanded Markdown, ID/title/goal, applicability, evidence and target
edition. Retrieval uses unusable proxies: this establishes offline content
delivery/proxy independence, not OS network isolation or native app behavior.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2865bc2a'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.cache-hybrid-static-assets','maui.create-csharp-ui-project','maui.include-avalonia-template-output','maui.build-community-platform-backends') `
  -Query 'quay-integration' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin911-913' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/quay/inspection'
```

Expect four edition11 recipes and no_applicable_edition for target10. .NET10
comparisons do not create additional foundation editions or a supported older-target
route. Outputs remain under ignored artifacts; the harness publishes nothing.
