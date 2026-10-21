# Choose preview templates deliberately and keep exact pins reproducible

## Goal

Try a preview depot-audit scaffold while keeping the selected package version and generated application observable. Installing a template does not install a workload or prove that a generated native project can build.

## Older approach

SDK 10 can install a known prerelease version explicitly: `dotnet new install Estuary.Depot.Templates@1.1.0-preview.1 --nuget-source ./feed`. The package name here is an original local verification fixture, not a public dependency to download. An explicit pin remains useful for reproducible builds on either SDK.

The older fixture generates an ordinary project:

{{snippet:older}}

## Newer approach

SDK 11 can select the latest available version including previews when the version is omitted: `dotnet new install Estuary.Depot.Templates --prerelease --nuget-source ./feed`. Without --prerelease the fixture selects stable 1.0.0. An explicit `Estuary.Depot.Templates@1.0.0 --prerelease` still selects exactly stable 1.0.0; the flag does not override a version pin.

{{snippet:newer}}

The generated source makes the selected channel observable. The template package replaces __CHANNEL__ with stable or preview before installation:

{{snippet:program}}

For this offline-complete fixture, the package root has a nuspec with ID Estuary.Depot.Templates, the chosen version, authors, description and package type Template. Under content/ put DepotAudit.csproj and Program.cs above, plus .template.config/template.json containing identity Estuary.Depot.Template, name Depot audit fixture, shortName depot-audit, sourceName DepotAudit and preferNameDirectory true. Zip those files into a .nupkg. No package upload is needed. eng/Test-SdkWorkflows.ps1 assembles both original packages in a local-only feed and executes the commands.

## Migration

1. Decide whether you want exploration of available previews or an exact package pin. Use an explicit version for a reproducible recorded baseline.
2. Inspect the actual package's source and supported targets. Prepare the local feed/cache if package installation itself must be offline.
3. Install the chosen version; then generate with `dotnet new depot-audit --name DepotAudit -o preview-app` in this fixture.
4. Compile and run the generated project, rather than treating successful installation as build evidence. The real fixture emits template=preview or template=stable.
5. Keep preview template storage isolated during experiments. The verification uses a fresh DOTNET_CLI_HOME and disables automatic global-tool PATH integration for every child; it does not change the user's installed templates.

## Constraints

--prerelease changes package selection; it does not grant access to a private feed, install a matching SDK/workload, create a device or guarantee generated-code stability. Templates and application dependencies are independent. Pin a real vendor package and its prerequisites when applying this technique outside the local fixture. No specific vendor mobile preview template is claimed verified here.

The fixture targets managed net10.0/net11.0 with no packages, providers, UI/rendering mode or native workload. The CLI feature is not an experimental API; a chosen template package may itself be prerelease. This recipe makes no package-publication or provisioning request. SDK 11 execution is RC2.

## Evidence

Both original generated applications compile and execute. Real isolated installs show default stable selection, SDK-10 exact-preview selection, SDK-11 unversioned prerelease selection and an exact stable pin winning over --prerelease. Installed Playbook retrieval remains offline and returns the entire canonical body and requirements; it does not require this template feed at retrieval time.
