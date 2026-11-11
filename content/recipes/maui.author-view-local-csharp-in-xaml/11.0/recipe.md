# Preview: keep small C# page logic in XAML

## Goal

Use preview SourceGen x:Code and C# expressions for small view-local operations, including namespace-qualified calls and attached-property reads; understand opt-ins and initialization limits.

## Older approach

The .NET10 page uses ordinary code-behind: initialize controls, call BerthText.Ready, read Grid.GetRow and handle a click. These are valid equivalents for this small operation. No verified .NET10 route for the current preview markup is advertised.

## Newer approach

Enable both MauiXamlInflator=SourceGen and EnablePreviewFeatures=true. The current page puts only a tiny synchronous handler in x:Code CDATA. It keeps an explicit local namespace mapping and uses an explicit expression marker before the namespace-qualified helper call. The dollar marker in the markup starts a C# interpolated string; single quotes delimit its text, and braces inside it insert values. This syntax is covered by the pinned SourceGen interpolation tests and expression specification. The interpolation reads Inspect.(Grid.Row), which the generator lowers to the attached property's getter.

{{snippet:n11-boardpage-xaml}}

## Migration

1. Review the pinned project, keep names and custom mappings, then migrate the page.
2. Rebuild the actual project and inspect diagnostics before selecting preview features.
3. Keep the evidence limits below with the delivered guidance.

Keep the same x:Class, named controls and handler signature. A type must resolve through usings, its fully qualified name or an XML prefix; the explicit = marker selects expression parsing when a prefix could otherwise denote a markup extension. Keep larger logic, services and testable business rules in ordinary C#.

Local method/property expressions capture values at initialization. Reading an attached property does not add attached-property change subscriptions or generated setters. Here the row is fixed before the label is initialized. Updating Grid.Row later does not promise an updated label. Use explicit bindings or deliberate updates for changing state.

## Constraints

Preview APIs may change. XamlC and runtime inflation do not implement this expression path; reverting requires moving the handler back into code-behind and restoring ordinary property assignments. Standalone BindingContext attached-property syntax is not implemented. this refers to the page root, not the element holding the expression.

The reviewed source specification describes MAUIX2007 for markup/property ambiguity, MAUIX2008 for page/context name conflicts, MAUIX2009 for unresolved members and MAUIX2013 for unsupported async lambdas. Disambiguate page members with this., context members with a leading dot and expressions with =. Use a normal event method for asynchronous work. These diagnostics were source-reviewed, not reproduced against generated native output here.

## Canonical samples

The project boundary files isolate these version-specific examples from shared repository package/build settings. Create all files under the corresponding sample directory.

### n10-boardpage-xaml

{{snippet:n10-boardpage-xaml}}

### n10-boardpage-xaml-cs

{{snippet:n10-boardpage-xaml-cs}}

### n10-directory-build-props

{{snippet:n10-directory-build-props}}

### n10-directory-build-targets

{{snippet:n10-directory-build-targets}}

### n10-directory-packages-props

{{snippet:n10-directory-packages-props}}

### n10-global-json

{{snippet:n10-global-json}}

### n10-mauiprogram-cs

{{snippet:n10-mauiprogram-cs}}

### n10-platforms-windows-app-xaml

{{snippet:n10-platforms-windows-app-xaml}}

### n10-platforms-windows-app-xaml-cs

{{snippet:n10-platforms-windows-app-xaml-cs}}

### n10-quayboard-csproj

{{snippet:n10-quayboard-csproj}}

### n11-boardpage-xaml

{{snippet:n11-boardpage-xaml}}

### n11-boardpage-xaml-cs

{{snippet:n11-boardpage-xaml-cs}}

### n11-directory-build-props

{{snippet:n11-directory-build-props}}

### n11-directory-build-targets

{{snippet:n11-directory-build-targets}}

### n11-directory-packages-props

{{snippet:n11-directory-packages-props}}

### n11-global-json

{{snippet:n11-global-json}}

### n11-mauiprogram-cs

{{snippet:n11-mauiprogram-cs}}

### n11-platforms-windows-app-xaml

{{snippet:n11-platforms-windows-app-xaml}}

### n11-platforms-windows-app-xaml-cs

{{snippet:n11-platforms-windows-app-xaml-cs}}

### n11-quayboard-csproj

{{snippet:n11-quayboard-csproj}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The .NET10 Windows comparison has actual Release compilation evidence with exact resolved packages and inputs. Compilation does not establish rendering, click handling or accessibility. The .NET11 source is official-review only: the existing SDK root lacks Microsoft.Maui.Sdk and the exact build fails NETSDK1147 before compilation. Its compilation, generated output and native execution are explicitly not-tested under PIN-699, not pending obligations to install a workload.

The .NET11 dependency closure is unresolved because the native SDK pack is unavailable, so no lock file was captured. Its empty dependency evidence means unresolved, not dependency-free. The source manifest retains the separately inspected Controls and Build.Tasks artifact identities.

The source manifest pins the reviewed SourceGen targets, implicit-namespace attribute, generator tests, expression specification and release notes. These establish factual authoring support; upstream tests were read, not run. No provider, account, signing, deployment, Android/Apple, performance or final-GA proof is claimed. Older comparison samples add no .NET10 foundation edition. See the recipe references and samples/QuayBoard/sources.json for provenance.
