# Preserve custom value generation after selector changes

## Goal

Keep custom generated keys working and handle missing generators explicitly when upgrading an EF extension.

## When to use

EF10 already supports TrySelect and TryCreate. Its obsolete Select/Create members disappear in EF11. Move both selection callers and creation overrides; true must return a non-null generator and false reports no generator. EF10 persistence already invokes TryCreate: a legacy Create override alone compiled but did not generate the intended keys in the real fixture. Keep the working TryCreate override on EF10 too. The explicit legacy Select control throws NotSupportedException for decimal; the current TrySelect control returns false with null. The selector delegates unrelated properties to the base and registers through ReplaceService. The random application prefix is fictional; these keys offer uniqueness checks in the fixture, not ordered or globally coordinated identifiers.

## Migration

1. Keep a before-upgrade fixture with the same values and failure controls.
2. Migrate the extension contracts described above; preserve unrelated provider services.
3. Copy each complete comparison project into its own directory and restore its locked dependencies.
4. Run the real provider checks with eng/Test-TernExtensions.ps1; compare stored values and the explicit controls.
5. Repeat on your actual provider and reconcile GA artifacts before claiming broader support.

## Older approach

The independent .NET10 project preserves the working older route. Keep these complete files together.

### global.json

{{snippet:n10-global-json}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### TernPermits.csproj

{{snippet:n10-ternpermits-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

The independent .NET11 project uses the current pinned extension contracts. Run its controls before applying the migration to another provider.

### global.json

{{snippet:n11-global-json}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### TernPermits.csproj

{{snippet:n11-ternpermits-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Constraints

EF10.0.13/net10.0 versus EF11.0.0-rc.2.26504.105/net11.0. SDK10.0.402/C#14 versus SDK11.0.100-rc.2.26504.105/C#15. SQL graphs pin Pkcs10.0.13 and SqlClient6.1.7/7.1.0. SQLite uses the locked platform bundle; actual native DLL hashes are in execution records. EF11 requires .NET11; no older-runtime route is offered. No workload or rendering mode applies. No experimental opt-in or NativeAOT claim. RC2 proof is not actual GA reconciliation.

Windows x64 bundled SQLite. Executed on Windows x64; SQL engine runs in the existing Linux container. No other native platform verified.

## Evidence

The saved compilation and execution records identify exact commands, inputs and dependency graphs. Run eng/Test-TernExtensions.ps1 -Record to refresh them. PIN-959 comprehensive coverage and PIN-965 GA reconciliation remain open. No new .NET10 foundation is added.

## References

- [Official guidance](https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties)


