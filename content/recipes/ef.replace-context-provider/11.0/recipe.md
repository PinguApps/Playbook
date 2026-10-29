# Replace a configured database provider deliberately

## Goal

A reed-bed visitor centre tests permit edits against a disposable database. Its
application already registered a context. Adding another provider is insufficient:
the old configuration callbacks still run, and even a single selected provider can
retain settings such as sensitive-data logging. Replace the registration before
building the test container, then inspect the options and execute the test store.

The examples never read production configuration. A closed SQLite in-memory
connection stands in for the original registration and must remain closed. Every
successful case uses a separate service collection and test database. This is
test setup, not a live application's provider switch or a database data migration.

## Choose the operation

| Operation | Effect | Use here |
| --- | --- | --- |
| EF11 `RemoveDbContext<PermitContext>()` | Removes the known context registrations, typed options and accumulated configuration callbacks | Register fresh Scoped context/options for the test |
| EF11 `RemoveDbContext<PermitContext>(removeConfigurationOnly: true)` | Removes only `IDbContextOptionsConfiguration<PermitContext>` callbacks | Keep the existing Transient context and Singleton options descriptors, then call `ConfigureDbContext` |
| EF11 `WithoutExtension<TExtension>()` | Returns a new options object without that exact extension type; the original is unchanged | Copy options before changing the provider |
| EF11 infrastructure `RemoveExtension<TExtension>()` | Replaces the builder's options; absent extension is a no-op | Implement an options extension, rather than calling infrastructure throughout application setup |

The Boolean defaults to `false`. Full removal is preferable when original settings
must be discarded. Configuration-only removal preserves registrations and their
lifetimes; it does not preserve the removed callback settings. In these examples,
Singleton options contain no scoped dependencies and Transient contexts are still
resolved and disposed inside scopes. A Singleton context is not a thread-safety
solution. Do not share a context across parallel operations.

Options removal does not remove DI callbacks. Conversely, DI removal cannot alter
an already built container, resolved options, an existing context or a provider set
by `OnConfiguring`. Perform the replacement before any resolutions and build a new
provider. Remove or redesign unconditional `OnConfiguring` provider selection first.

The provider's concrete options classes live in unsupported `Internal` namespaces.
The current infrastructure helper therefore discovers the exact configured type
through public `Extensions`/`Info.IsDatabaseProvider`, and invokes the public generic
removal method with that type using reflection. It never references those internal
classes or suppresses EF1001. The helper is intentionally bounded to one provider;
its builder route identifies SQLite by the installed provider assembly. Reflection
is not a trimming/NativeAOT contract. Prefer DI removal for ordinary test setup.
Removing one extension preserves unrelated options, including potentially unsafe
logging, interceptors, a cached model or an explicit internal service provider.
Audit those independently; an options copy is not a clean-room configuration.

## Older approach

EF10 lacks all three new removal APIs. For this simple `AddDbContext<TContext>`
registration, remove every accumulated `IDbContextOptionsConfiguration<TContext>`
descriptor. For full replacement also remove the context and typed options; for
configuration-only replacement keep them and register a fresh callback. Removing
only `DbContextOptions<TContext>` leaves the old callbacks active. A fresh options
builder with an explicit allowlist of safe settings remains a valid older route.
No general-purpose clone of arbitrary EF10 registrations is claimed.

These complete, independently pinned files solve the same goal on .NET 10. Local
empty build files prevent inherited repository build settings. Copy each version
into its own standalone directory; do not combine the projects.

### global.json

{{snippet:n10-global-json}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### ReedPermits.csproj

{{snippet:n10-reedpermits-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

Use the EF11 APIs against the actual .NET 11 consumer. Both full and configuration-only
removal run against SQLite and InMemory. The options-copy and builder-extension
cases separately exercise the new APIs and read actual InMemory writes. The whole
program is included so its assertions, lifetimes and failure controls remain offline.

### global.json

{{snippet:n11-global-json}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### ReedPermits.csproj

{{snippet:n11-reedpermits-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Create a new test service collection using disposable configuration. Do not
   import production credentials, connections or an already built provider. Inspect
   the actual registration shape; this recipe exercises simple `AddDbContext`, not
   custom service aliases, factories, pooling or `UseInternalServiceProvider`.
2. Choose full replacement when you want new registrations and lifetimes. Remove
   the context, then call `AddDbContext` with only test settings. On EF10 use the
   three explicit `RemoveAll` calls shown above for this bounded registration.
3. If preserving registrations is deliberate, remove only configuration callbacks
   and call `ConfigureDbContext`. Match its lifetime to the retained options; do not
   capture a scoped dependency in this example's Singleton configuration.
4. For an independently owned options object, EF11 can remove the selected extension
   from a copy or through the extension-author builder API. Check preserved core
   settings before use. On EF10 build fresh options from approved settings instead.
5. Keep the test SQLite connection open across scopes. Build a new provider with
   scope validation, inspect descriptor retention and resolved context/options
   identities, and require exactly one selected provider with only the intended
   connection. Reject retained original settings even if provider count is one.
6. In each standalone directory, use its pinned SDK to run
   `dotnet build ReedPermits.csproj --configuration Release -p:RestoreLockedMode=true`,
   then `dotnet ./bin/Release/net10.0/ReedPermits.dll` for EF10 or
   `dotnet ./bin/Release/net11.0/ReedPermits.dll` for EF11. These are the recorded
   locked-build and built-assembly execution steps. Expect four replacement PASS
   lines, CRUD in fresh scopes,
   the mixed/retained configuration PASS line and the version-specific options PASS
   line. Investigate any assertion failure before using the setup in your tests.
7. Dispose contexts/scopes and the provider before the externally owned connection.
   The in-memory databases disappear with their owners. No production schema or
   data changes, external account or persistent test resources are required.

## Constraints

The previous consumer uses SDK 10.0.402, .NET 10.0.13, C# 14 and EF SQLite/InMemory
10.0.13. The current consumer uses SDK 11.0.100-rc.2.26504.105, runtime
11.0.0-rc.2.26504.105, C# 15 and EF SQLite/InMemory 11.0.0-rc.2.26504.105. Those
are measured artifacts, separate from the route's .NET/SDK 11 release-family
prerequisite. No C#15-specific syntax or experimental suppression is required;
these APIs are not publisher-designated experimental. RC2 is not actual GA proof.
EF11 selects EF/Relational/InMemory net11.0 assets and requires .NET11. Direct
Microsoft.Data.Sqlite.Core11 selects net10.0, which does not make EF11 run on net10.0.
The older comparison adds no .NET10 discovery route or ninth approved foundation.

Both projects lock their full dependency graphs. The measured Windows x64 SQLite
engines are 3.53.3 (SQLitePCLRaw 2.1.13 bundle/core/provider/native graph) and 3.53.4
(SQLitePCLRaw 3.0.5 bundle/config/core/provider with the separate SQLite package
3.53.4). The normal provider package initializes the bundled engine; no manual
`SetProvider`, retired bundle, paid vendor, encryption or external database is used.
Matching native assets are required on the host. Other OS/architectures, providers
and native initialization choices are not established by these runs. No workloads,
browser/rendering mode, deployment or Human check is required for this console path.

SQLite enforces the model's unique permit-code index: the duplicate insert must
fail, and a new scope must still see one row. InMemory accepts that duplicate.
Both execute create/read/update/delete, but only the SQLite case proves this
relational constraint. Neither proves production-provider SQL translation,
collation, transaction/isolation behavior or schema-migration compatibility. Use
the actual production provider separately for those requirements, with isolated
test resources and explicit authorization.

The retained callback control reproduces the two-provider `InvalidOperationException`
without opening the original connection. The same-provider control retains sensitive
logging and is rejected before database use. These controls demonstrate why a
successful build or a single provider name alone is insufficient. The generic
removal helpers also do not promise removal of arbitrary custom registrations or
all untyped `DbContextOptions` aliases in a multi-context container. Those shapes
need their own registration/service checks; none is advertised here.

## Evidence

The canonical sample `verification-evidence.json` records contain fresh actual
build/run commands, SDK/runtime output, exact resolved dependencies, input hashes,
host, SQLite version/source ID and all PASS lines. They cover eight replacement
containers across the two releases, resolved lifetimes, descriptor retention,
unrelated-service preservation, original callback/connection exclusion, CRUD,
duplicate-index controls and current options-removal semantics. These are real
SQLite/InMemory executions, not mocks or cross-compilation. Lock files preserve
package content hashes; the evidence does not claim another native platform or GA.

The ordinary packed tool is separately installed through the existing local-only
feed/cache inspector and queried over stdio from an unrelated directory. That check
compares discovery ID/title/goal, target11/edition11, complete canonical Markdown,
applicability and current evidence. It proves embedded offline retrieval with
unusable proxies, not network isolation. PIN-959 comprehensive coverage and PIN-965
actual-GA reconciliation remain open. M (3) covers the bounded DI/provider pair,
registration lifetimes and realistic failure controls; no provider matrix is added.

## Official references

- [Official introduction of provider removal](https://github.com/dotnet/core/blob/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/preview3/efcore.md).
- [Package-matched DI removal](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore/Extensions/EntityFrameworkServiceCollectionExtensions.cs).
- [Immutable options removal](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore/DbContextOptions.cs) and [builder infrastructure contract](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore/DbContextOptionsBuilder.cs).
- [DbContext configuration and lifetime](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/) and [testing strategy limitations](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy).
