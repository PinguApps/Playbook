# Keep generated forms editable in the Designer

## Goal

The Fen instrument briefing keeps generated initialization deterministic and runtime behaviour in the user partial. C# and VB have materially different event syntax; both are demonstrated directly.

## Older approach

Explicit InitializeComponent statements and VB WithEvents/Handles work on .NET10. The previous examples already use those conventions. These forms were not broken: .NET11 adds diagnostics that expose unsupported generated structure.

## Newer approach

{{snippet:n11-fendesigner-program-cs}}

Keep unrolled assignments in InitializeComponent, generated control fields after generated methods, and runtime logic in the user partial. Avoid collection expressions, conditional/coalescing/conditional-access expressions and lambdas in Designer initialization. Compute runtime values after InitializeComponent instead. Designer serialization needs a deterministic result rather than arbitrary expressions. Keep named event handlers in the user partial.

In VB, declare Designer controls WithEvents and connect named methods with Handles. A local component can legitimately use AddHandler; do not rewrite every AddHandler because a Designer control needs Handles. The startup Shown handler here is outside generated initialization.

WFO2000 warns about a getter returning a newly allocated reference. Decide the contract first: return a retained collection for stable identity, or expose an explicitly creating method for a snapshot. Caching changes identity/lifetime semantics; replacing a public property with a method changes its public interface and needs consumer migration. These sample members are internal/Friend and establish no compatibility-breaking recommendation.

| Rules | Repair |
| --- | --- |
| WFO3000/3001/3003 | Keep explicit initialization; move runtime logic, custom members and event declarations to the user partial. |
| WFO3002 | Put Designer-owned fields after generated methods. |
| WFO3004 | C# explicit initialization instead of collection expressions. |
| WFO3005/3009 | Serialize deterministic NameOf/interpolated results. |
| WFO3006–3008/3010 | Remove conditional/coalescing/conditional-access/lambda logic from generated initialization. |
| WFO3011/3012 | VB Designer controls use WithEvents and named Handles methods. |

The RC2 framework analyzer assets supply these rules. WFO2000/3002/3005/3009 default to warnings; the other new Designer rules default to errors. The samples treat warnings as errors and use no blanket suppression. Both VB samples pin language16.9; no new VB version is inferred.

## Migration

1. Build the actual .NET10 form and retain its supported explicit initialization.
2. Build on the pinned .NET11 SDK and inspect emitted diagnostics.
3. Move only runtime logic out, retain explicit initialization and put generated fields after methods.
4. Check click wiring and intended object identity in both languages. Reopen/save in a compatible Visual Studio Designer before claiming round-trip proof.

## Constraints

Windows Desktop runtime and STA are required. Current SDK11.0.100-rc.2.26504.105/C#15 is compared with SDK10.0.402/C#14; VB16.9 on both. No additional package, provider account, Blazor rendering mode or experimental opt-in. No .NET10 analyzer backport is claimed. The Designer is a Visual Studio tool and is not proved by the framework compiler alone. RC2 results are not GA proof.

## Evidence

All four actual projects compiled and executed a real shown form, one button click, a stable collection getter and independently allocated snapshots. The first current C# build rejected a field placed before InitializeComponent with WFO3002; moving it after the method fixed the emitted error. Visual Studio reopen/save remains pending. No Designer round-trip execution, complete diagnostic-trigger matrix or code-fix execution is claimed.

## Complete canonical inputs

### 10 / FenDesigner / BriefingForm.Designer.cs

{{snippet:n10-fendesigner-briefingform-designer-cs}}

### 10 / FenDesigner / Directory.Build.props

{{snippet:n10-fendesigner-directory-build-props}}

### 10 / FenDesigner / Directory.Build.targets

{{snippet:n10-fendesigner-directory-build-targets}}

### 10 / FenDesigner / Directory.Packages.props

{{snippet:n10-fendesigner-directory-packages-props}}

### 10 / FenDesigner / FenDesigner.csproj

{{snippet:n10-fendesigner-fendesigner-csproj}}

### 10 / FenDesigner / global.json

{{snippet:n10-fendesigner-global-json}}

### 10 / FenDesigner / Program.cs

{{snippet:n10-fendesigner-program-cs}}

### 10 / FenDesignerVb / BriefingForm.Designer.vb

{{snippet:n10-fendesignervb-briefingform-designer-vb}}

### 10 / FenDesignerVb / Directory.Build.props

{{snippet:n10-fendesignervb-directory-build-props}}

### 10 / FenDesignerVb / Directory.Build.targets

{{snippet:n10-fendesignervb-directory-build-targets}}

### 10 / FenDesignerVb / Directory.Packages.props

{{snippet:n10-fendesignervb-directory-packages-props}}

### 10 / FenDesignerVb / FenDesignerVb.vbproj

{{snippet:n10-fendesignervb-fendesignervb-vbproj}}

### 10 / FenDesignerVb / global.json

{{snippet:n10-fendesignervb-global-json}}

### 10 / FenDesignerVb / Program.vb

{{snippet:n10-fendesignervb-program-vb}}

### 11 / FenDesigner / BriefingForm.Designer.cs

{{snippet:n11-fendesigner-briefingform-designer-cs}}

### 11 / FenDesigner / Directory.Build.props

{{snippet:n11-fendesigner-directory-build-props}}

### 11 / FenDesigner / Directory.Build.targets

{{snippet:n11-fendesigner-directory-build-targets}}

### 11 / FenDesigner / Directory.Packages.props

{{snippet:n11-fendesigner-directory-packages-props}}

### 11 / FenDesigner / FenDesigner.csproj

{{snippet:n11-fendesigner-fendesigner-csproj}}

### 11 / FenDesigner / global.json

{{snippet:n11-fendesigner-global-json}}

### 11 / FenDesigner / Program.cs

{{snippet:n11-fendesigner-program-cs}}

### 11 / FenDesignerVb / BriefingForm.Designer.vb

{{snippet:n11-fendesignervb-briefingform-designer-vb}}

### 11 / FenDesignerVb / Directory.Build.props

{{snippet:n11-fendesignervb-directory-build-props}}

### 11 / FenDesignerVb / Directory.Build.targets

{{snippet:n11-fendesignervb-directory-build-targets}}

### 11 / FenDesignerVb / Directory.Packages.props

{{snippet:n11-fendesignervb-directory-packages-props}}

### 11 / FenDesignerVb / FenDesignerVb.vbproj

{{snippet:n11-fendesignervb-fendesignervb-vbproj}}

### 11 / FenDesignerVb / global.json

{{snippet:n11-fendesignervb-global-json}}

### 11 / FenDesignerVb / Program.vb

{{snippet:n11-fendesignervb-program-vb}}

