# Keep a monitoring tile readable as Windows preferences change

## Goal

The Fen station display paints a status message using the owner's system foreground and background. A small accent strip is optional decoration; the message never relies on that color to convey status.

## Older approach

The .NET10 control uses system colors and invalidates on system-color changes. That remains a useful, supported design. It has no Net11 renderer selection or consolidated visual-settings snapshot; do not invent a package backport for those framework APIs.

## Newer approach

{{snippet:n11-fenstatus-program-cs}}

The .NET11 control reads EffectiveVisualStylesMode at paint time. The public requested property can still say Inherit. Selecting a modern renderer is not permission to bypass accessibility: High Contrast uses classic rendering for styled controls, while Disabled remains disabled.

Override the control-lifetime settings hook and call base. This avoids a static application subscription retaining a disposed control. Application-wide listeners need explicit unsubscription. Inspect the Changed category flags when expensive category-specific work is needed; this small control only invalidates its inexpensive paint surface.

GetVisualStylesModeChangeImpact returns Repaint because this tile changes decoration without changing preferred size or its native handle. A control with different metrics needs the appropriate impact; these enum values are alternatives, not a bitmask. Keep the base notification path so descendants and framework bookkeeping still run.

The status tile reads the immutable current settings snapshot rather than caching it. It suppresses the accent decoration in High Contrast and always paints semantic content with system colors. It does not animate, so the animation preference requires no new animation mechanism. Text scaling and DPI geometry are separate concerns; this sample does not claim a full adaptive layout or accessibility certification.

## Owner check: recorded PIN-897

Run the current compiled sample with --manual. Keep it open while changing Windows Settings > Accessibility > Contrast themes, then restore the original theme. On 11 November 2026 Pingu repeated this check on the GA build and confirmed it worked as expected: system-colour text stayed readable, the decorative accent disappeared in high contrast and returned after restoration. The actual .NET11.0.0 log records disabled/Net11 -> enabled/Classic -> restored disabled/Net11. Visual readability is owner-reported, not an automated pixel assertion or broad accessibility certification. This bounded Human gate is satisfied; unrelated comprehensive coverage gates remain separate.

## Migration

1. Build the previous example with its pinned SDK and retain its behavior as the comparison.
2. Use the current canonical implementation with the declared Windows Desktop assets.
3. Run Test-FenDesktop.ps1 and inspect the bounded evidence; refresh owner evidence after relevant sample or toolchain changes.

## Constraints

Windows desktop and STA execution are required. Current C# uses language15; previous C# uses14. VB uses16.9 on both releases. No extra NuGet package, cloud provider or workload installation is needed. No experimental API suppression is required. This .NET11 edition has no supported .NET10 package backport.

## Complete canonical inputs

### n10-fenstatus-directory-build-props

{{snippet:n10-fenstatus-directory-build-props}}

### n10-fenstatus-directory-build-targets

{{snippet:n10-fenstatus-directory-build-targets}}

### n10-fenstatus-directory-packages-props

{{snippet:n10-fenstatus-directory-packages-props}}

### n10-fenstatus-fenstatus-csproj

{{snippet:n10-fenstatus-fenstatus-csproj}}

### n10-fenstatus-global-json

{{snippet:n10-fenstatus-global-json}}

### n10-fenstatus-program-cs

{{snippet:n10-fenstatus-program-cs}}

### n11-fenstatus-directory-build-props

{{snippet:n11-fenstatus-directory-build-props}}

### n11-fenstatus-directory-build-targets

{{snippet:n11-fenstatus-directory-build-targets}}

### n11-fenstatus-directory-packages-props

{{snippet:n11-fenstatus-directory-packages-props}}

### n11-fenstatus-fenstatus-csproj

{{snippet:n11-fenstatus-fenstatus-csproj}}

### n11-fenstatus-global-json

{{snippet:n11-fenstatus-global-json}}

### n11-fenstatus-program-cs

{{snippet:n11-fenstatus-program-cs}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Real control handle, renderer selection, inherited requested mode and paint. Pingu separately executed the Contrast-theme change/restoration recorded in owner-settings.evidence.json; visual judgment is owner-reported. Exact SDK pins, locked dependency closure and input hashes are recorded beside each sample. The older build is a comparison, not a new .NET10 foundation. The current records cover Windows x64 GA; they do not establish other platforms. No service account, remote provider, workload installation or package publication is required.

## References

[Official RC2 contracts](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/winforms.md).
