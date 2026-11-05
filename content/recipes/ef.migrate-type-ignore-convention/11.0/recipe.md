# Keep an ignored-type convention working

## Goal

Migrate an ignored-type notification convention and check callback dispatch, model inclusion, schema and persistence on actual SQLite.

## When to use

The legacy IEntityTypeIgnoredConvention is already obsolete on EF10 and its registered callback is not dispatched by this measured graph. ITypeIgnoredConvention is available on both releases. Use the replacement before upgrading; do not describe it as newly introduced in EF11. Register through ConfigureConventions. Verify ignoring both before and after explicit entity addition in separate processes. This observer records notification; ModelBuilder.Ignore owns exclusion. It does not silently enforce a policy against later explicit re-addition. No annotation-removal call is needed in this observer; HasNoAnnotation is the builder replacement only where such a call exists.

## Migration

1. Identify registrations implementing IEntityTypeIgnoredConvention; the pinned EF10 callback is already obsolete and is not dispatched by this measured graph.
2. Pin and restore each complete comparison independently; EF11 requires .NET11.
3. Implement ITypeIgnoredConvention.ProcessTypeIgnored and register the replacement through ConfigureConventions. Keep ModelBuilder.Ignore responsible for exclusion. Use HasNoAnnotation only when an existing convention actually removes annotations; this observer has no such call.
4. Run eng/Test-CurlewModels.ps1, including the convention ordering controls. Inspect actual schema and fresh-context results.
5. Repeat affected checks on your provider and GA artifacts.

## Older approach

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### CurlewConventions.csproj

{{snippet:n10-curlewconventions-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### CurlewConventions.csproj

{{snippet:n11-curlewconventions-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Constraints

Windows x64 console/SQLite execution only; no rendering mode, NativeAOT or native JSON-index route. Public extension contracts remain upgrade-sensitive.

## Evidence

Independent SDK10.0.403 / C#14 / EF10.0.13 and SDK11 RC3 / C#15 / EF11 RC2 graphs. SQLite3.53.3/3.53.4 are the measured bundled engines. Compilation and execution records identify inputs and locked dependencies. This is RC preparation evidence, not GA proof. No rendering mode or experimental opt-in is involved. No older-runtime EF11 route is supported. PIN-959 and PIN-965 remain separate gates.

## References

[Official conventions](https://learn.microsoft.com/en-us/ef/core/modeling/bulk-configuration#conventions)
