# Prepare the observation desk before first presentation

## Goal

Fen's observation desk creates its initial content before entering the message loop. Choose initial window presentation separately from the time needed to fetch later observations.

## Older approach

The .NET10 C# and VB examples prepare controls before startup and use the normal existing window lifecycle. They do not simulate the newer reveal API with opacity tricks. A valid older startup flow is still valid when immediate presentation is desired.

## Newer approach

{{snippet:n11-fenstartup-program-cs}}

Select Deferred before creating the main form to delay initial presentation in both light and dark modes. The sample checks the application's resolved Boolean policy, the form's resolved Deferred property, an explicit Classic override and the real Shown lifecycle.

Inherit on the application default restores automatic selection: dark color mode defers, light/Classic color mode presents immediately. The Form getter returns the resolved choice rather than Inherit and resolves against the application default, not an owner/parent-form chain. IsFormRevealDeferred is a property describing the application default; an individual form override may differ.

An immediate splash can request Classic while the main window defers. Top-level forms participate; MDI children do not. This feature does not wait for arbitrary asynchronous data work or guarantee that a deep tree never changes visibly after startup. The sample executes a main window and checks an unshown Classic override; splash timing is source-reviewed, not executed.

## Current VB ApplicationEvents approach

Use the VB application framework's ApplyApplicationDefaults event, as shown in ApplicationEvents.vb. The standalone sample explicitly supplies its WindowsFormsApplicationBase subclass and entry point instead of relying on IDE-generated startup files. In an IDE framework project, put the same handler in the existing MyApplication partial under My Project/ApplicationEvents.vb.

VB's framework retains Classic startup defaults for compatibility. Select Deferred explicitly here, or select Inherit for automatic color-mode behavior. The event-argument settings are applied before the main/splash presentation and take precedence over protected defaults on the derived application class. EnableVisualStyles still enables styles; selecting a renderer does not enable them. VB16.9 is the declared language mode on both runtimes, not a newly invented VB release.

## Migration

1. Build the previous example with its pinned SDK and retain its behavior as the comparison.
2. Use the current canonical implementation with the declared Windows Desktop assets.
3. Run Test-FenDesktop.ps1 and inspect the bounded evidence; retain pending owner checks.

## Constraints

Windows desktop and STA execution are required. Current C# uses language15; previous C# uses14. VB uses16.9 on both releases. No extra NuGet package, cloud provider or workload installation is needed. No experimental API suppression is required. This .NET11 edition has no supported .NET10 package backport.

## Complete canonical inputs

### n10-fenstartup-directory-build-props

{{snippet:n10-fenstartup-directory-build-props}}

### n10-fenstartup-directory-build-targets

{{snippet:n10-fenstartup-directory-build-targets}}

### n10-fenstartup-directory-packages-props

{{snippet:n10-fenstartup-directory-packages-props}}

### n10-fenstartup-fenstartup-csproj

{{snippet:n10-fenstartup-fenstartup-csproj}}

### n10-fenstartup-global-json

{{snippet:n10-fenstartup-global-json}}

### n10-fenstartup-program-cs

{{snippet:n10-fenstartup-program-cs}}

### n10-fenstartupvb-applicationevents-vb

{{snippet:n10-fenstartupvb-applicationevents-vb}}

### n10-fenstartupvb-directory-build-props

{{snippet:n10-fenstartupvb-directory-build-props}}

### n10-fenstartupvb-directory-build-targets

{{snippet:n10-fenstartupvb-directory-build-targets}}

### n10-fenstartupvb-directory-packages-props

{{snippet:n10-fenstartupvb-directory-packages-props}}

### n10-fenstartupvb-fenstartupvb-vbproj

{{snippet:n10-fenstartupvb-fenstartupvb-vbproj}}

### n10-fenstartupvb-global-json

{{snippet:n10-fenstartupvb-global-json}}

### n11-fenstartup-directory-build-props

{{snippet:n11-fenstartup-directory-build-props}}

### n11-fenstartup-directory-build-targets

{{snippet:n11-fenstartup-directory-build-targets}}

### n11-fenstartup-directory-packages-props

{{snippet:n11-fenstartup-directory-packages-props}}

### n11-fenstartup-fenstartup-csproj

{{snippet:n11-fenstartup-fenstartup-csproj}}

### n11-fenstartup-global-json

{{snippet:n11-fenstartup-global-json}}

### n11-fenstartup-program-cs

{{snippet:n11-fenstartup-program-cs}}

### n11-fenstartupvb-applicationevents-vb

{{snippet:n11-fenstartupvb-applicationevents-vb}}

### n11-fenstartupvb-directory-build-props

{{snippet:n11-fenstartupvb-directory-build-props}}

### n11-fenstartupvb-directory-build-targets

{{snippet:n11-fenstartupvb-directory-build-targets}}

### n11-fenstartupvb-directory-packages-props

{{snippet:n11-fenstartupvb-directory-packages-props}}

### n11-fenstartupvb-fenstartupvb-vbproj

{{snippet:n11-fenstartupvb-fenstartupvb-vbproj}}

### n11-fenstartupvb-global-json

{{snippet:n11-fenstartupvb-global-json}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Real C# default/resolved form selection and explicit override; real VB ApplyApplicationDefaults handler; both main forms reach Shown. No pixel timing or splash window execution. Exact SDK pins, locked dependency closure and input hashes are recorded beside each sample. The older build is a comparison, not a new .NET10 foundation. Windows x64 GA verification is limited to the recorded checks or other-platform proof. No service account, remote provider, workload installation or package publication is required.

## References

[Official GA contracts](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/winforms.md).
