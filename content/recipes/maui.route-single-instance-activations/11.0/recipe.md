# Deliver external requests to the running Windows session

## Goal

Tern Dispatch keeps one Windows session alive when another launch or protocol request arrives. The URI is consumed by that session rather than abandoned with the short-lived redirecting process.

## Older approach

The .NET 10 comparison owns Windows AppInstance registration in the native application constructor, before normal MAUI startup. It subscribes to Activated itself. Its receipt writer consumes activation data on the callback thread and does not touch UI. A production UI integration must marshal to its dispatcher and keep activation data alive until consumption finishes. This remains a valid Windows integration.

## Newer approach

{{snippet:samples-net11-0-ternactivation-activationroute-cs}}

The .NET 11 OnAppInstanceActivated lifecycle hook handles both initial activation and later redirected requests. MAUI dispatches later callbacks onto the application dispatcher and waits until handlers have consumed their data. Register one stable, application-specific key. A secondary instance starts an asynchronous redirect, returns true to suppress its window, and exits only after transfer completes. A failed transfer exits with code1. The owning instance returns false so its initial window is created. Keep the dispatcher free while transfer is pending.

This sample logs the complete URI only when it exactly matches the fixed synthetic callback without credentials; other protocol URIs are redacted. Real callback handlers must validate scheme, host, path and correlation state and avoid logging authorization codes or tokens. Receiving a URI proves delivery, not authentication. MAUI can deliver WebAuthenticator callbacks through this lifecycle path, but this sample does not perform provider sign-in or token exchange.

## Migration

1. Build the Windows sample with its pinned SDK and matching MAUI workload. Keep the explicit unpackaged Windows App SDK dependencies.
2. Replace the sample's instance key and scheme with application-owned values. Register packaged protocols in the package manifest. For unpackaged verification, run the sample executable with --register; it registers only playbook-tern-pin919 through ActivationRegistrationManager.
3. Start one sample executable, then start a second. The secondary must exit and its activation must appear in the primary's activation.txt receipt.
4. Launch playbook-tern-pin919://callback?ticket=919 through Windows. Verify the same primary PID recorded that URI. Protocol registration belongs to installation/setup, not ordinary startup.
5. Stop the sample and run its executable with --unregister. Remove an empty, sample-owned scheme key only after confirming no registrations remain. Do not alter another application's registration.

## Constraints

Windows only, native MAUI/WinUI; no web rendering mode, provider or account. The older app uses SDK10.0.402, C#14 and Controls10.0.110. Current intended pins are SDK11 GA, C#15 and Controls11 GA with matching MAUI Windows workload. Windows App SDK is a separate dependency, not evidence of a usable MAUI workload. No compatible .NET10 route for the new hook is claimed. The API is stable; RC artifact review is limited to the recorded checks.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The actual .NET10 Windows Release app compiled. Separate processes 79376 and 77072 established launch redirection: the second exited with code0 and both launch receipts were written by79376. A real Windows protocol launch delivered playbook-tern-pin919://callback/?ticket=919 to79376. An additional protocol URI with a synthetic code parameter was received as Protocol (redacted), without persisting its query. Temporary registration was removed, including its empty leftover root key. This is protocol delivery only, not WebAuthenticator/provider authentication. Current .NET11 compilation was attempted with SDK11 GA and failed NETSDK1147; current compilation/execution are explicitly not-tested under PIN-699. No MSIX deployment or trust change was tested.

## Canonical inputs


### samples/net10.0/TernActivation/ActivationRoute.cs

{{snippet:samples-net10-0-ternactivation-activationroute-cs}}

### samples/net10.0/TernActivation/MauiProgram.cs

{{snippet:samples-net10-0-ternactivation-mauiprogram-cs}}

### samples/net10.0/TernActivation/Platforms/Windows/App.xaml

{{snippet:samples-net10-0-ternactivation-platforms-windows-app-xaml}}

### samples/net10.0/TernActivation/Platforms/Windows/App.xaml.cs

{{snippet:samples-net10-0-ternactivation-platforms-windows-app-xaml-cs}}

### samples/net10.0/TernActivation/ProtocolRegistration.cs

{{snippet:samples-net10-0-ternactivation-protocolregistration-cs}}

### samples/net10.0/TernActivation/TernActivation.csproj

{{snippet:samples-net10-0-ternactivation-ternactivation-csproj}}

### samples/net11.0/TernActivation/ActivationRoute.cs

{{snippet:samples-net11-0-ternactivation-activationroute-cs}}

### samples/net11.0/TernActivation/MauiProgram.cs

{{snippet:samples-net11-0-ternactivation-mauiprogram-cs}}

### samples/net11.0/TernActivation/Platforms/Windows/App.xaml

{{snippet:samples-net11-0-ternactivation-platforms-windows-app-xaml}}

### samples/net11.0/TernActivation/Platforms/Windows/App.xaml.cs

{{snippet:samples-net11-0-ternactivation-platforms-windows-app-xaml-cs}}

### samples/net11.0/TernActivation/ProtocolRegistration.cs

{{snippet:samples-net11-0-ternactivation-protocolregistration-cs}}

### samples/net11.0/TernActivation/TernActivation.csproj

{{snippet:samples-net11-0-ternactivation-ternactivation-csproj}}

### Complete receipt implementation (.NET10)

{{snippet:activation-complete-10}}

### Complete receipt implementation (.NET11)

{{snippet:activation-complete-11}}
