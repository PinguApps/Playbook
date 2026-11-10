# Migrate callable query-root and structural materialization consumers

## Goal

Update a parameter query-root consumer while retaining working structural materialization and identifying already unavailable entity-only hooks.

## When to use

EF10 already error-obsoletes IEntityMaterializerSource and EntityMaterializerSourceParameters. CollectionResultExpression.Navigation and ProjectionBindingExpression are also already error-obsolete: use StructuralProperty and QueryExpression on both versions. There is no valid EF10 entity-only comparison to revive. Pingu approved the callable parameter-root comparison on 5 November 2026. The older ParameterExpression constructor and property are warning-obsolete but callable; EF11 takes QueryParameterExpression. Structural materialization already exists in EF10. This bounded extension consumer obtains the public structural service and its entity materializer; real provider reads verify nested complex values and nullable scalar results. Root construction is a contract check; the subsequent collection query exercises the normal provider, not a custom query translator. No replacement provider, custom SQL translator or internal service implementation is taught.

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

### LarkMaterializers.csproj

{{snippet:n10-larkmaterializers-csproj}}

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

### LarkMaterializers.csproj

{{snippet:n11-larkmaterializers-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Constraints

Independent locked EF10.0.13/net10.0/SDK10.0.403/C#14 and EF11.0.0/net11.0/SDK11.0.100/C#15 graphs. EF11 requires .NET11; no older-runtime EF11 route. Current proof uses RC2, not GA. No experimental attribute or opt-in is required on the exercised public contracts.

Windows x64 bundled SQLite 3.53.3/3.53.4. Executed on Windows x64. SQL2025 runs in the existing Linux container. Other providers/platforms and NativeAOT are unverified. No workload or rendering mode applies. Public extension contracts remain upgrade-sensitive.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

eng/Test-LarkContracts.ps1 -Record: pinned real compilation and provider checks. No GA, NativeAOT or other-platform claim. Compilation receipts and behaviour records retain exact inputs, resolved dependencies, commands and actual outputs. The ordinary installed stdio harness checks full offline Markdown, canonical snippets, edition and applicability. No ninth .NET10 foundation is added. PIN-959 comprehensive readiness and PIN-965 actual GA reconciliation remain open.

## References

- [Official EF guidance](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.parameterqueryrootexpression?view=efcore-10.0)

