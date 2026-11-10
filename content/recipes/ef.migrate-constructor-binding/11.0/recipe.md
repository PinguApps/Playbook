# Migrate a constructor-binding consumer to binding selection

## Goal

Replace a removed constructor-binding call and verify scalar and service parameters plus invalid bindings on the actual provider.

## When to use

EF10 TryBindConstructor accepts a specific ConstructorInfo and reports false plus unbound parameters. EF11 removes it; GetBindings selects constructor and service-only bindings for the type and throws for an unbindable type. GetBindings already exists in EF10 and is a valid earlier migration. It is not a drop-in replacement for arbitrary constructor preference: this bounded consumer has one constructor with two mapped scalar parameters and an injected DbContext. Preserve custom selection requirements before choosing this route. An original Bird is inserted with SQL and materialized in a fresh context, verifying scalar values and injected context identity. Independent mutable metadata checks the public factory and invalid Uri unknown parameter. Factory inspection does not itself install a custom factory or change EF conventions. The example consumes the public extension contract and never subclasses internal EF implementations. No lazy-loading, arbitrary service injection or generic object mapper is promised.

## Migration

1. Identify your exact old callable contract and retain a before-upgrade fixture. Do not revive error-obsolete APIs.
2. Apply the contract and behaviour distinction described above. Keep application-specific selection and integrity decisions explicit.
3. Keep each complete project below in its own directory, including its isolated SDK and package files.
4. Run eng/Test-LarkContracts.ps1; check the observed results and failure controls before adapting the code.
5. Repeat affected checks on your production provider and actual GA artifacts.

## Older approach

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### LarkBindings.csproj

{{snippet:n10-larkbindings-csproj}}

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

### LarkBindings.csproj

{{snippet:n11-larkbindings-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Constraints

Independent locked EF10.0.13/net10.0/SDK10.0.403/C#14 and EF11.0.0/net11.0/SDK11.0.100/C#15 graphs. EF11 requires .NET11; no older-runtime EF11 route. Current proof uses RC2, not GA. No experimental attribute or opt-in is required on the exercised public contracts.

Windows x64 bundled SQLite 3.53.3/3.53.4. Executed on Windows x64. SQL2025 runs in the existing Linux container. Other providers/platforms and NativeAOT are unverified. No workload or rendering mode applies. Public extension contracts remain upgrade-sensitive.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

eng/Test-LarkContracts.ps1 -Record: pinned real compilation and provider checks. No GA, NativeAOT or other-platform claim. Compilation receipts and behaviour records retain exact inputs, resolved dependencies, commands and actual outputs. The ordinary installed stdio harness checks full offline Markdown, canonical snippets, edition and applicability. No ninth .NET10 foundation is added. PIN-959 comprehensive readiness and PIN-965 actual GA reconciliation remain open.

## References

- [Official EF guidance](https://learn.microsoft.com/en-us/ef/core/modeling/constructors)

