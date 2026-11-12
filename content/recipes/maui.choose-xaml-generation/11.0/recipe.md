# Migrate a MAUI page to SourceGen

## Goal

Move an existing MAUI page to SourceGen, simplify standard namespace declarations and preserve custom type mappings without claiming automatic performance gains.

## Older approach

The fictional Quay Board page uses explicit MAUI and x namespace mappings, a custom BerthBadge mapping and a code-behind event handler. This is a valid .NET10 XamlC page. Preserve working code-behind and named-control links when changing the inflator.

## Newer approach

The reviewed GA Controls targets select SourceGen by default. Set MauiXamlInflator=SourceGen explicitly when making the migration intentional. Ordinary SourceGen does not need EnablePreviewFeatures. The companion page uses GA expressions and x:Code without preview consent.

Implicit standard namespaces allow the current root to omit its default and x declarations. Keep xmlns:local for the custom badge and helper; custom libraries, ambiguous names and external assemblies can still require explicit mappings. Do not remove every xmlns declaration mechanically. The old AllowImplicitXmlnsDeclarationAttribute opt-out is obsolete and ignored in the reviewed source.

{{snippet:n11-boardpage-xaml}}

## Migration

1. Review the pinned project, keep names and custom mappings, then migrate the page.
2. Rebuild the actual project and inspect diagnostics when changing generator options.
3. Keep the evidence limits below with the delivered guidance.

First keep the explicit namespaces and code-behind, select SourceGen and resolve actual generator diagnostics. Preserve x:Class and x:Name values and event signatures. Only then remove redundant standard mappings. The companion current sample also moves its small handler into x:Code; that is an independent optional step, described by maui.author-view-local-csharp-in-xaml.

To retain the XamlC route for ordinary markup, choose MauiXamlInflator=XamlC and restore explicit namespace declarations. The pinned GA targets emit MAUI1001 when selecting XamlC or Runtime because those inflators are deprecated; this is a compatibility fallback, not the preferred authoring route. Keep the warning visible. Runtime inflation is a separate fallback; neither fallback supports these expressions. Remove those expressions/x:Code first and restore the older handler/property assignments. Rebuild rather than suppressing unknown diagnostics.

## Constraints

This is a generator and namespace migration, not evidence of faster startup or a performance benchmark. Keep custom mappings and supported fallback code. The current sample below combines the migration with GA SourceGen page logic; using SourceGen alone is not a preview C# opt-in.

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
