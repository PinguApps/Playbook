# Run database design commands for a file app

## Goal

Target an EF file-based app for migrations, or use a file app as the startup host for a conventional model project, and verify generated schema and persisted rows.

## When to use

EF11 tools accept --file for the target app and --startup-file for the startup host. EF10 uses an ordinary project as the tested alternative; no EF10 file-target option is claimed. The separate startup-file route deliberately targets a conventional model library through a project reference. Pingu approved this mixed route on 5 November 2026: the pinned SDK rejects #:project Marsh.cs with MSB4025. No file-to-file project reference is advertised. Disable PublishAot for this reflection-based EF design-time route. Explicitly include generated Migrations/**/*.cs in the file target and pass --output-dir Migrations plus an unambiguous namespace. Restore/build both target and startup before tooling metadata lookup. File and project selectors for the same role are mutually exclusive.

## Migration

1. Retain the working EF10 project route. Restore each pinned graph separately.
2. For a single file target, retain the package/property directives and migration include below. Use matching dotnet-ef and Design versions.
3. Build the file app before invoking migrations add Initial --file Marsh.cs --namespace CurlewMigrations --output-dir Migrations. Include those generated files on subsequent builds. With --namespace CurlewMigrations, this pinned tool writes MarshModelSnapshot.cs under CurlewMigrations while the migration files go under Migrations; retain both canonical include directives.
4. Run migrations list, migrations script and database update with the same selection. Execute the app with --check to verify actual writes to the generated table.
5. For the separate startup role, build the model library and Startup.cs, then select --project ../CurlewFileModel --startup-file Startup.cs. Repeat all migration and persistence checks; this is a separate database.
6. Run eng/Test-CurlewDesign.ps1 -Record for isolated copies and exclusive-selector failure controls. Repeat on actual GA artifacts before claiming GA verification.

## Older approach

The conventional EF10 project keeps database tooling explicit.

### CurlewDesign

#### Directory.Build.props

{{snippet:n10-curlewdesign-directory-build-props}}

#### Directory.Build.targets

{{snippet:n10-curlewdesign-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n10-curlewdesign-directory-packages-props}}

#### global.json

{{snippet:n10-curlewdesign-global-json}}

#### CurlewDesign.csproj

{{snippet:n10-curlewdesign-curlewdesign-csproj}}

#### Program.cs

{{snippet:n10-curlewdesign-program-cs}}

## Newer approach

Use the standalone file route or the separate mixed model/startup route. Each folder retains its own package/SDK boundary.

### CurlewFile

#### Directory.Build.props

{{snippet:n11-curlewfile-directory-build-props}}

#### Directory.Build.targets

{{snippet:n11-curlewfile-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n11-curlewfile-directory-packages-props}}

#### global.json

{{snippet:n11-curlewfile-global-json}}

#### Marsh.cs

{{snippet:n11-curlewfile-marsh-cs}}

### CurlewFileModel

#### Directory.Build.props

{{snippet:n11-curlewfilemodel-directory-build-props}}

#### Directory.Build.targets

{{snippet:n11-curlewfilemodel-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n11-curlewfilemodel-directory-packages-props}}

#### global.json

{{snippet:n11-curlewfilemodel-global-json}}

#### CurlewFileModel.csproj

{{snippet:n11-curlewfilemodel-curlewfilemodel-csproj}}

#### Model.cs

{{snippet:n11-curlewfilemodel-model-cs}}

### CurlewStartup

#### Directory.Build.props

{{snippet:n11-curlewstartup-directory-build-props}}

#### Directory.Build.targets

{{snippet:n11-curlewstartup-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n11-curlewstartup-directory-packages-props}}

#### global.json

{{snippet:n11-curlewstartup-global-json}}

#### Startup.cs

{{snippet:n11-curlewstartup-startup-cs}}

## Constraints

EF10.0.13/SDK10.0.403/C#14/net10 versus EF11 RC2/SDK11 RC3/C#15/net11. EF11 has no older-runtime route. Matching Design and tool packages are required separately. Bundled SQLite3.53.3/3.53.4 executed on Windows x64. No workload, rendering mode or experimental opt-in applies. This reflection-based design-time route disables NativeAOT; production trimming/AOT is outside scope.

## Evidence

eng/Test-CurlewDesign.ps1 -Record generates fresh migrations in isolated copies, verifies list/script output, applies real schema and persists one original nest row on each route. Generated output is disposable; saved sample receipts retain exact dependencies, inputs and results. Compilation is separate from tooling execution. Ordinary installed offline list/get inspection checks served content only. PIN-959 and PIN-965 remain open; no ninth foundation or public publication is added.

## References

[Official EF11 tooling guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
