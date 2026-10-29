# Keep direct SQLite access compatible with the application target

## Goal

QuaySeals records a seal code before a harbour locker is handed over. Keep that
parameterized SQL and its transaction rules working while choosing the application
target and SQLite dependency together. A package version matching EF Core's number
does not establish the same framework requirement.

Microsoft.Data.Sqlite 11 targets **net10.0 only**. The wrapper's dependency group
and Core's managed library are net10.0; net11.0 can consume that asset too.
**EF Core 11 separately requires net11.0.** Direct low-level SQLite access on
.NET 10 does not require EF or a .NET 11 runtime. Changing global.json alone picks
a build SDK; it does not retarget the project or restore a missing package asset.

## Choose the route

| Application and dependency | Verified managed asset | Decision |
| --- | --- | --- |
| net10.0 with Sqlite 10.0.13 | Core lib/net8.0 | Valid previous-release comparison; retain deliberately when the upgrade is deferred. |
| net11.0 with Sqlite 11 RC2 | Core lib/net10.0 | Normal current-release comparison; keep direct SQL while moving the application target. |
| net10.0 with Sqlite 11 RC2 | Core lib/net10.0 | Upgrade the direct provider without moving to net11.0. |
| net48 with Sqlite 10.0.13 | Core lib/netstandard2.0 | Verified earlier-dependency alternative on Windows; Framework cannot use Sqlite 11. |

This is one 11.0 migration edition, with conditional target-10 routes. The previous
net10.0 and retained net48 examples are comparisons; neither adds a .NET 10
foundation. The catalogue still has exactly its original eight foundations and
only accepts targets 10.0/11.0; net48 is not a new catalogue target.

For a target below net10.0 there is no Sqlite11 equivalent to demonstrate:
the required managed asset is absent. Upgrade the application target to modern
.NET if feasible. If it must remain on .NET Framework, deliberately keep a
compatible earlier provider and its compatible native dependencies. The complete
net48 example below supplies actual evidence for that limited fallback.

## Constraints

The previous and optional net10 projects pin SDK10.0.402/C#14; the current project
pins SDK11.0.100-rc.2.26504.105/C#15. The retained net48 project uses SDK10.0.402,
C#7.3 and Microsoft.NETFramework.ReferenceAssemblies 1.0.3. Its x64 EXE was compiled
against Framework4.8 and actually executed on Windows with Framework4.8.1
(registry release 533509; CLR4.0.30319.42000). Newer language syntax is unnecessary
for this example. No experimental API or experimental opt-in is used.

Provider pins are 10.0.13 and 11.0.0-rc.2.26504.105. These are reproducible
baselines, not advice to remain on an old servicing patch indefinitely. Review
current servicing/security support when choosing a production dependency, then
refresh the consumer proof after any dependency change. RC2 is not actual GA
verification; the existing GA reconciliation gate remains open.

The old graph uses SQLitePCLRaw 2.1.13 and native SQLite3.53.3; the new graph uses
SQLitePCLRaw 3.0.5 plus the SQLite3.53.4 package and executes native SQLite3.53.4.
The wrapper initializes the bundled e_sqlite3 provider. Do not add unrelated
SQLitePCLRaw bundles or assume a Core-only reference supplies native initialization.
Managed target compatibility and the native library's supported OS/architecture
are separate checks. All executions here are Windows x64.

No EF mapping, web framework, renderer, browser, workload, account or remote
database is required. A writable temporary directory is required. Each program
creates one unique file-backed database, disables connection pooling, disposes
connections and deletes that database in finally. The sample checks correctness,
not throughput, database encryption or production-file conversion.

### UWP and classic Xamarin are separate platform decisions

The reviewed Microsoft source says current bundled native SQLite no longer
includes UWP or classic Xamarin builds. Moving a UWP app to Windows App SDK or a
classic Xamarin app to .NET MAUI is the documented platform migration direction;
it still requires that application's own workload, native and device checks.
No such migration was built or run here.

If a legacy platform cannot move, an earlier native bundle that contains its
assets may be necessary alongside a compatible earlier managed provider. **No
working UWP/classic Xamarin package combination is advertised by this recipe.**
The net48 consumer is a desktop console and does not establish those native paths.
Even the retained 2.1.13 bundle here must not be described as a UWP/Xamarin fallback.

Source reconciliation matters: the reviewed article describes bundle2.1.12,
while the actual Sqlite11 RC2 graph resolves bundle/config/core/provider3.0.5
and SQLite3.53.4. The executed artifact graph governs these sample prerequisites.
Custom/encrypted bundle migration remains separate work; a package label is not
native-platform or encryption proof.

## Older approach: net10.0 and Sqlite10

All six files below form one independent project. Empty local build files isolate
it from unrelated repository defaults; local central package management owns the
pin. Restore creates the lock file on first use; subsequent builds should use
locked restore after reviewing that file. The repository keeps its reviewed
resolved lock files and exact dependency graphs with the samples.

The insert commits the seal code including its apostrophe through a parameter.
An update is rolled back, then a new connection must read the original exact code.
The code is intentionally the same across targets: the migration is the target
and dependency choice, rather than a replacement SQL API.

#### global.json

{{snippet:previous-global-json}}

#### Directory.Build.props

{{snippet:previous-directory-build-props}}

#### Directory.Build.targets

{{snippet:previous-directory-build-targets}}

#### Directory.Packages.props

{{snippet:previous-directory-packages-props}}

#### QuaySeals.csproj

{{snippet:previous-quayseals-csproj}}

#### Program.cs

{{snippet:previous-program-cs}}

## Newer approach: net11.0 and Sqlite11

Use the same observable persistence rules with the new package. No EF package
is involved. Although this app targets net11.0, its provider comes from the
net10.0 library asset.

#### global.json

{{snippet:current-global-json}}

#### Directory.Build.props

{{snippet:current-directory-build-props}}

#### Directory.Build.targets

{{snippet:current-directory-build-targets}}

#### Directory.Packages.props

{{snippet:current-directory-packages-props}}

#### QuaySeals.csproj

{{snippet:current-quayseals-csproj}}

#### Program.cs

{{snippet:current-program-cs}}

## Optional: keep net10.0 and upgrade only Sqlite

This standalone project proves the supported direct-provider route on .NET10.
It uses SDK10, runtime10 and Sqlite11's net10.0 asset. It makes no EF11-on-net10
claim.

#### global.json

{{snippet:net10-current-global-json}}

#### Directory.Build.props

{{snippet:net10-current-directory-build-props}}

#### Directory.Build.targets

{{snippet:net10-current-directory-build-targets}}

#### Directory.Packages.props

{{snippet:net10-current-directory-packages-props}}

#### QuaySeals11.csproj

{{snippet:net10-current-quayseals11-csproj}}

#### Program.cs

{{snippet:net10-current-program-cs}}

## Alternative: retain Sqlite10 on .NET Framework

The following complete net48 project is a valid deliberately retained dependency
route. Its Framework reference-assemblies packages are build-only; running needs
the installed Windows Framework runtime and bundled native library. PlatformTarget
x64 matches the execution checked here. SDK10 compiling net48 is not itself proof
of execution: the EXE was also launched directly, recorded separately.

#### global.json

{{snippet:retained-framework-global-json}}

#### Directory.Build.props

{{snippet:retained-framework-directory-build-props}}

#### Directory.Build.targets

{{snippet:retained-framework-directory-build-targets}}

#### Directory.Packages.props

{{snippet:retained-framework-directory-packages-props}}

#### QuaySeals.csproj

{{snippet:retained-framework-quayseals-csproj}}

#### Program.cs

{{snippet:retained-framework-program-cs}}

## Migration

1. Inspect the application's TargetFramework/TargetFrameworks, global.json,
   central package files and resolved dependency graph. Distinguish direct
   Microsoft.Data.Sqlite usage from EF Core's requirements.
2. Choose one complete project above as a baseline. For Sqlite11, target net10.0
   or net11.0; for the demonstrated Framework alternative, keep Sqlite10.
   Copy every displayed file for that project into one standalone directory.
3. From that directory, restore from nuget.org using the pinned SDK with
   `dotnet restore`. Inspect the resulting packages.lock.json and selected Core
   asset in obj/project.assets.json. This online setup is separate from offline
   Playbook guidance retrieval. Preserve the reviewed lock file.
4. Run `dotnet build -c Release -p:RestoreLockedMode=true`. For a modern target,
   run `dotnet run -c Release --no-build` using the matching runtime installation.
   For net48 on Windows, run `.\\bin\\Release\\net48\\QuaySeals.exe` directly.
5. Expect PASS for the parameterized seal, commit, rollback and file reopen,
   with provider/runtime/native versions. The program deletes its temporary file.
   In the real app, check the deployed native assets and test its actual database
   operations separately before changing its target or dependency lock.

## Evidence

The authoring check `eng/Record-SqliteRuntime.ps1` builds and executes all four
real consumers and checks the managed asset selected by each restore. It records
three compile-and-run results plus separate net48 compilation and EXE execution.
The net48 compilation is not mislabeled cross-compilation or a mock.
The saved records preserve exact input hashes, pins, graphs, host and output;
changed relevant inputs require recapture.

Installed offline list/get retrieval verifies this entire expanded body and its
target-specific requirements. It does not execute SQL or inspect the reader's
application. The real consumer checks establish the bounded provider behavior;
neither those checks nor retrieval establishes other native platforms or GA.

## Official references

- [Reviewed .NET Framework and UWP/classic Xamarin breaking-change baseline](https://github.com/dotnet/EntityFramework.Docs/blob/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/breaking-changes.md)
- [Microsoft release notes: Sqlite net10.0 minimum](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/preview7/efcore.md)
- [Microsoft.Data.Sqlite.Core 11 RC2 package assets](https://www.nuget.org/packages/Microsoft.Data.Sqlite.Core/11.0.0-rc.2.26504.105)
- [Microsoft.Data.Sqlite.Core 10.0.13 package assets](https://www.nuget.org/packages/Microsoft.Data.Sqlite.Core/10.0.13)
- [EF Core 11 requires .NET 11](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
- [Microsoft.Data.Sqlite native-library and bundle configuration](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/custom-versions)
