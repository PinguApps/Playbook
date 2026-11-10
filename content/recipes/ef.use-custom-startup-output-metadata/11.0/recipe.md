# Run design-time tooling with customized output names

## Goal

Run database tooling when startup dependency or runtime configuration output names differ from the assembly name; select the real generated files.

## Older approach

EF10 works with conventional output names. Keep those names when its tool derives the deps and runtimeconfig paths from AssemblyName.

## Newer approach

EF11 reads the startup project output metadata. The example keeps AvocetStore as the target while using custom generated files from AvocetStartup.

{{snippet:n11-samples-net11-0-avocetstartup-avocetstartup-csproj}}

## Migration

1. Keep the target model project separate from the executable startup project. Restore both with their local SDK pins, and match EF, Design and dotnet-ef package versions.
2. In the current startup project, declare the actual generated deps and runtimeconfig names. These are build outputs, not hand-written JSON replacements. EF11 reads ProjectDepsFileName and ProjectRuntimeConfigFileName rather than deriving both from AssemblyName.
3. Run migrations with --project ../AvocetStore --startup-project . from AvocetStartup. Use migrations add, migrations list and database update. Inspect verbose output: --depsfile and --runtimeconfig must select avocet-launch files; the target assembly remains AvocetStore.dll.
4. Keep conventional output names on EF10. Its tool guesses AssemblyName.deps.json and fails with custom names. Returning to conventional names is a working older approach; changing AssemblyName alone is not a fix for arbitrary output paths.
5. Treat missing files as a build/configuration error. Rebuild the actual startup project, inspect the evaluated properties and confirm both files exist. Do not manufacture a deps file.
6. Direct application launch also needs the custom runtimeconfig/deps arguments: dotnet exec --runtimeconfig bin/Debug/net11.0/avocet-launch.runtimeconfig.json --depsfile bin/Debug/net11.0/avocet-launch.deps.json bin/Debug/net11.0/AvocetStartup.dll --check. Ordinary dotnet run guesses the conventional runtimeconfig filename on the measured SDK.

## Constraints

Original Windows x64 comparisons: SDK10.0.403/C#14/EF10.0.13 versus SDK11.0.100-rc.3.26522.148/C#15. EF11 tooling uses RC2; Cosmos uses RC3. EF11 requires net11.0; this is not an older-runtime EF11 route or GA proof. No language-preview opt-in. Console/database guidance has no rendering mode. Keep exact locks; restore requires network access. The installed guidance is offline. No additional .NET10 foundation or recurring platform matrix is introduced.

SQLite uses the actual bundled native Windows x64 engines. Matching EF/Design/dotnet-ef and patched Pkcs10.0.13 are tooling dependencies. Other platforms, encrypted engines, PMC and NativeAOT are not established.

## samples/net10.0/AvocetStartup/AvocetStartup.csproj

{{snippet:n10-samples-net10-0-avocetstartup-avocetstartup-csproj}}

## samples/net10.0/AvocetStartup/Directory.Build.props

{{snippet:n10-samples-net10-0-avocetstartup-directory-build-props}}

## samples/net10.0/AvocetStartup/Directory.Build.targets

{{snippet:n10-samples-net10-0-avocetstartup-directory-build-targets}}

## samples/net10.0/AvocetStartup/Directory.Packages.props

{{snippet:n10-samples-net10-0-avocetstartup-directory-packages-props}}

## samples/net10.0/AvocetStartup/Program.cs

{{snippet:n10-samples-net10-0-avocetstartup-program-cs}}

## samples/net10.0/AvocetStore/AvocetStore.csproj

{{snippet:n10-samples-net10-0-avocetstore-avocetstore-csproj}}

## samples/net10.0/AvocetStore/Directory.Build.props

{{snippet:n10-samples-net10-0-avocetstore-directory-build-props}}

## samples/net10.0/AvocetStore/Directory.Build.targets

{{snippet:n10-samples-net10-0-avocetstore-directory-build-targets}}

## samples/net10.0/AvocetStore/Directory.Packages.props

{{snippet:n10-samples-net10-0-avocetstore-directory-packages-props}}

## samples/net10.0/AvocetStore/Model.cs

{{snippet:n10-samples-net10-0-avocetstore-model-cs}}

## samples/net10.0/AvocetStore/Migrations/20261105233303_Initial.cs

{{snippet:n10-samples-net10-0-avocetstore-migrations-20261105233303-initial-cs}}

## samples/net10.0/AvocetStore/Migrations/20261105233303_Initial.Designer.cs

{{snippet:n10-samples-net10-0-avocetstore-migrations-20261105233303-initial-designer-cs}}

## samples/net10.0/AvocetStore/Migrations/AvocetContextModelSnapshot.cs

{{snippet:n10-samples-net10-0-avocetstore-migrations-avocetcontextmodelsnapshot-cs}}

## samples/net11.0/AvocetStartup/AvocetStartup.csproj

{{snippet:n11-samples-net11-0-avocetstartup-avocetstartup-csproj}}

## samples/net11.0/AvocetStartup/Directory.Build.props

{{snippet:n11-samples-net11-0-avocetstartup-directory-build-props}}

## samples/net11.0/AvocetStartup/Directory.Build.targets

{{snippet:n11-samples-net11-0-avocetstartup-directory-build-targets}}

## samples/net11.0/AvocetStartup/Directory.Packages.props

{{snippet:n11-samples-net11-0-avocetstartup-directory-packages-props}}

## samples/net11.0/AvocetStartup/Program.cs

{{snippet:n11-samples-net11-0-avocetstartup-program-cs}}

## samples/net11.0/AvocetStore/AvocetStore.csproj

{{snippet:n11-samples-net11-0-avocetstore-avocetstore-csproj}}

## samples/net11.0/AvocetStore/Directory.Build.props

{{snippet:n11-samples-net11-0-avocetstore-directory-build-props}}

## samples/net11.0/AvocetStore/Directory.Build.targets

{{snippet:n11-samples-net11-0-avocetstore-directory-build-targets}}

## samples/net11.0/AvocetStore/Directory.Packages.props

{{snippet:n11-samples-net11-0-avocetstore-directory-packages-props}}

## samples/net11.0/AvocetStore/Model.cs

{{snippet:n11-samples-net11-0-avocetstore-model-cs}}

## samples/net11.0/AvocetStore/Migrations/20261105233129_Initial.cs

{{snippet:n11-samples-net11-0-avocetstore-migrations-20261105233129-initial-cs}}

## samples/net11.0/AvocetStore/Migrations/20261105233129_Initial.Designer.cs

{{snippet:n11-samples-net11-0-avocetstore-migrations-20261105233129-initial-designer-cs}}

## samples/net11.0/AvocetStore/Migrations/AvocetContextModelSnapshot.cs

{{snippet:n11-samples-net11-0-avocetstore-migrations-avocetcontextmodelsnapshot-cs}}


## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Independent compilation uses each SDK pin and lock. Focused SQLite tooling and Cosmos emulator checks are recorded separately; compilation alone does not establish database behavior. Actual GA and other platforms remain unverified.

## Exact toolchain

Current EF package: 11.0.0-rc.2.26504.105; older package: 10.0.13. Tooling consumers match Design and dotnet-ef to the EF package. SDK pins are canonical below; release-family prerequisites do not relabel these RC checks as GA proof.

### .NET10 AvocetStartup SDK pin

{{snippet:sdk-10-avocetstartup}}

### .NET10 AvocetStore SDK pin

{{snippet:sdk-10-avocetstore}}

### .NET11 AvocetStartup SDK pin

{{snippet:sdk-11-avocetstartup}}

### .NET11 AvocetStore SDK pin

{{snippet:sdk-11-avocetstore}}

The retained initial migrations use namespace AvocetStartup.Migrations because they were first scaffolded before separating the model project. Their compiled target assembly is AvocetStore; a namespace does not select the startup assembly.
