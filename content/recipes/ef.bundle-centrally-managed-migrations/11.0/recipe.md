# Produce a migration bundle with central package management

## Goal

Build and execute a migration bundle with centrally managed application packages; inspect the temporary project and retain an EF10 workaround.

## When to use

Real Windows x64 framework-dependent SQLite migration bundles; matching Design/tool versions, inherited CPM and non-CPM control. SDK10.0.403 / EF10.0.13 versus SDK11 RC3 / EF11 RC2. No self-contained, NativeAOT, multi-context, encrypted provider, other-platform or GA claim.

The Design tooling graph includes System.Security.Cryptography.Pkcs. The sample deliberately pins its patched 10.0.13 version, following the existing tooling consumers; this is a dependency qualification pin, rather than an application cryptography API used by the example.

## Migration

1. Keep application PackageReference entries versionless under CPM. Pin SQLite, Design and dotnet-ef to matching versions. Restore/build before EF metadata lookup.
2. Generate and retain an Initial migration and snapshot. The original Plover model and .NET10 generated migration are included below. Generate the .NET11 migration and snapshot with the matching current tool before building its bundle.
3. Build with `dotnet <tool-path>/dotnet-ef.dll migrations bundle --output migrate.exe`. This is one selected context; wildcard context creation, self-contained and NativeAOT are outside this route.
4. The generated temporary project must select the matching Design package. EF11 emits conditional Design references: versionless when CPM is inherited, explicitly versioned otherwise. The inherited central Design pin must match the tool. CPM remains enabled in your application. EF10 already works with an ordinary external temp directory; that valid route remains supported. If a temporary directory inherits CPM, EF10 reproduces NU1008. Isolate that temporary project's build properties and disable central management there, keeping the application's local CPM boundary intact.
5. Run `./migrate.exe --connection 'Data Source=plover.db'` in a disposable working directory. Run it twice to check no pending migrations remain, then run the consumer with `--check` to insert and read PLO-7 from the migrated table. A framework-dependent bundle requires the matching runtime and the measured native SQLite assets.
6. Run `eng/Test-PloverBundles.ps1` for isolated CPM/non-CPM copies, real generated-project capture, old NU1008 control, bundle execution and persistence. This recipe does not establish GA behavior or deploy to production.

## Older approach

The independently pinned .NET10 consumer remains a valid comparison.

### .NET10 Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET10 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET10 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET10 global.json

{{snippet:n10-global-json}}

### .NET10 PloverBundle.csproj

{{snippet:n10-ploverbundle-csproj}}

### .NET10 Program.cs

{{snippet:n10-program-cs}}

### .NET10 20261105214826_Initial.cs

{{snippet:n10-migrations-20261105214826-initial-cs}}

### .NET10 20261105214826_Initial.Designer.cs

{{snippet:n10-migrations-20261105214826-initial-designer-cs}}

### .NET10 SurveyContextModelSnapshot.cs

{{snippet:n10-migrations-surveycontextmodelsnapshot-cs}}

## Newer approach

The independently pinned .NET11 consumer records the actual RC boundary.

### .NET11 Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET11 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET11 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET11 global.json

{{snippet:n11-global-json}}

### .NET11 PloverBundle.csproj

{{snippet:n11-ploverbundle-csproj}}

### .NET11 Program.cs

{{snippet:n11-program-cs}}

## Evidence

Real Windows x64 framework-dependent SQLite migration bundles; matching Design/tool versions, inherited CPM and non-CPM control. SDK10.0.403 / EF10.0.13 versus SDK11 RC3 / EF11 RC2. No self-contained, NativeAOT, multi-context, encrypted provider, other-platform or GA claim.

## Constraints

No final-artifact support is inferred from RCs. Source audit baselines, the eight foundations and existing recipe identities remain unchanged.

## References

- [Official EF11 intended behavior](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
- [EF migrations deployment](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Bounded E24/E25/E26 source audit](https://github.com/PinguApps/Playbook/blob/a7fd46cd658708c25a7e0d032735760c3fe6d63f/docs/planning/audits/ef.md)
