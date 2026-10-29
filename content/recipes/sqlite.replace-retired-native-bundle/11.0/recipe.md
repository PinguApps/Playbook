# Restore SQLite startup after a native bundle is retired

## Goal

A canal office records the remaining passages on a permit. Its desktop application
already uses Windows' SQLite library and stores no encrypted data. After replacing
its convenience bundle, startup must still select that same native engine before
a connection opens. A package restore alone cannot make that selection.

This recipe preserves that existing choice. It is not a recommendation to move an
encrypted application onto plain SQLite. SQLitePCLRaw is an external community
project. Paid builds and encryption alternatives are decisions for the application
owner, not default Playbook product choices.

## Choose the native policy first

Inventory direct and transitive bundle/provider references, the native DLL name,
OS/architecture, required SQLite extensions and version, and whether existing files
use encryption. Keep those requirements while choosing the replacement:

- For the Windows system engine, replace retired `bundle_winsqlite3` with
  `provider.winsqlite3` and explicitly call `raw.SetProvider` before connections.
  The complete, executed examples below cover this free unencrypted route only.
- A system `sqlite3` route uses its matching provider and a separately available
  native library. It needs OS-specific validation; Windows' `winsqlite3.dll` is
  a different library name. This is not a tested substitute for other platforms.
- `bundle_e_sqlite3` is still supported in SQLitePCLRaw3; it is not one of the
  retired system bundles. Retaining it can be appropriate for an app already using
  its bundled plain engine. `config.e_sqlite3` separates initialization from native
  package selection. Neither package name establishes encryption support.
- For retired encryption bundles, retain the cipher/file format, key derivation,
  compatibility settings, extensions and licensing requirements. SQLite3 Multiple
  Ciphers, a self-built open-source SQLCipher engine, supported Zetetic builds, or
  licensed SEE may be candidates. Paid services are optional. They are not installed,
  compiled or executed here. Verify new encrypted writes, reopen with the right key,
  rejection with a wrong key and representative existing files on the actual native
  build before adopting one. A successful plain-text connection proves none of this.
- Replacing `bundle_green` requires a deliberate decision about its iOS system-engine
  behavior versus bundled engines elsewhere. No mobile build or device proof is
  supplied here. UWP/classic Xamarin remain excluded; this modern Windows console
  example does not restore removed platform assets.

## Older approach: .NET10 with the convenience bundle

The old bundle configured the Windows engine conveniently. It still executes in
this pinned historical graph; retirement does not mean every old installation stops
working. Do not carry its 2.x initialization assembly into a 3.x provider graph.
The observed comparison resolves Core10.0.13, bundle/provider2.1.11 and raw core2.1.13.

Create a standalone `CanalPermits` directory with these six files. The empty build
files prevent accidental inheritance of another repository's build policy.

### `global.json`

{{snippet:n10-global-json}}

### `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### `CanalPermits.csproj`

{{snippet:n10-canalpermits-csproj}}

### `Program.cs`

{{snippet:n10-program-cs}}

## Newer approach: .NET11 with the explicit system provider

Replace the convenience bundle with the provider package and keep
`Microsoft.Data.Sqlite.Core`. The `Microsoft.Data.Sqlite` wrapper would introduce
its own native bundle. Register the provider once, synchronously, before creating
connections or starting database workers; the selected provider is process-wide.
Do not race different initialization paths or let a library choose another engine.

Use a separate directory for this current-release sample. Core11 RC2 selects
`lib/net10.0` on net11.0. The system provider selects `netstandard2.0` and raw core
selects `net8.0` managed assets. The native library comes from Windows, not from a
NuGet native asset.

### `global.json`

{{snippet:n11-global-json}}

### `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### `CanalPermits.csproj`

{{snippet:n11-canalpermits-csproj}}

### `Program.cs`

{{snippet:n11-program-cs}}

## Migration

1. Confirm the native policy above fits the app. Back up representative existing
   databases before changing a production dependency graph. This sample creates
   only a new disposable database; it never migrates production files.
2. Remove `SQLitePCLRaw.bundle_winsqlite3` and conflicting automatic bundles from
   direct and transitive references. Add the exact current packages shown above.
   Keep providers/core on compatible release lines; inspect resolved dependencies
   rather than relying on a successful package download.
3. Add explicit startup registration before any `SqliteConnection`. Keep the
   permit SQL and connection lifecycle unchanged. For this comparison, moving from
   Batteries initialization to `SetProvider` preserves the Windows native choice.
4. From each standalone sample directory, run restore, Release build and the
   resulting application with the matching SDK/runtime. Disable first-run tools
   integration in child environments with `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`.
   The canonical SDK pins are 10.0.402 and 11.0.100-rc.2.26504.105. No workload is
   required. NuGet restore may need internet access; consulting this recipe does not.
5. Expect a committed debit from three to two after closing and reopening the
   file, a password-support rejection, and the actual loaded native path/version
   and SHA256. The observed Windows x64 host loaded system SQLite3.53.4. Your
   Windows servicing determines that version; retest required SQL/extensions on
   your real host. Do not assume all Windows engines match this observation.
6. In a fresh current-release process, omit startup registration (the sample's
   `--omit-initialization` control). Expect a failure asking for
   `SQLitePCL.raw.SetProvider`. Re-enable registration and rerun successfully.
   `eng/Record-SqliteInitialization.ps1` captures both actual consumers and this
   negative control. A broken provider graph or missing native DLL is a separate
   diagnosis: inspect the restored graph, library name and architecture first.

For an ordinary SDK command line, the operations from the selected sample directory
are `dotnet restore --locked-mode`, `dotnet build -c Release --no-restore`, and
`dotnet bin/Release/net11.0/CanalPermits.dll` (use `net10.0` for the comparison).
Use the matching installed dotnet executable; the repository helper accepts
absolute `-Net10Dotnet` and `-Net11Dotnet` paths. For files copied from this body,
the first restore creates a new lock file; omit `--locked-mode` for that first restore.

## Constraints

Preview6 notes announced SQLite3MC as a default dependency. The reviewed breaking
article also discusses bundle2.1.12. Those historical statements do not describe
our actual RC2 graph: the ordinary Microsoft.Data.Sqlite11 RC2 wrapper resolves
bundle_e_sqlite3/core3.0.5 and SQLite3.53.4, as recorded separately by
`sqlite.retain-supported-runtime`. This recipe deliberately avoids that wrapper.
Neither that default graph nor this system graph proves encrypted-file compatibility.
The upstream v3 note's SourceGear native package wording is historical too; do not
infer an installed native package identity from that prose.

This edition uses .NET11 RC2 SDK/provider artifacts and stable initialization APIs;
there is no experimental API or opt-in. It is not GA proof. Direct Sqlite11 has
net10.0 assets, whereas EF11 separately requires net11.0. Only the executed net11.0
current route is advertised here; the net10.0 sample is the previous-release
comparison. This adds no .NET10 foundation and changes none of the exact eight.

## Evidence

The saved records distinguish real compilation/execution from source review. Both
samples actually loaded Windows x64 native SQLite, persisted a parameterized value
across a committed transaction and connection reopen, refused a password, and
cleaned up the file. The negative control is a separate fresh-process execution
record. No mock, cross-compilation, encrypted database, other native platform,
production migration or actual GA execution is claimed. Existing catalogue Human
and GA release gates remain pending outside this bounded recipe.

## Official references

- [Reviewed bundle retirement and platform boundaries](https://github.com/dotnet/EntityFramework.Docs/blob/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/breaking-changes.md)
- [SQLitePCLRaw 3 release notes at the provider package source commit](https://github.com/ericsink/SQLitePCL.raw/blob/ed046114d5a30534e13294d94d78eb73de896ad4/v3.md)
- [Microsoft native provider configuration](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/custom-versions)
- [Retained Windows system bundle 2.1.11](https://www.nuget.org/packages/SQLitePCLRaw.bundle_winsqlite3/2.1.11)
- [Windows system provider 3.0.5](https://www.nuget.org/packages/SQLitePCLRaw.provider.winsqlite3/3.0.5)
- [Direct managed provider RC2 target assets](https://www.nuget.org/packages/Microsoft.Data.Sqlite.Core/11.0.0-rc.2.26504.105)
- [Preview 6 SQLite3MC dependency announcement, not the RC2 graph](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/preview6/efcore.md)
