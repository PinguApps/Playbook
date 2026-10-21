# Run a repository utility directly from a C# file

## Goal

Total the slot counts in a fictional depot manifest. Keep the utility in a tools directory beside the maintained projects, with the same input validation whether it uses a project or a file. This is the approved .NET 10 foundation, not an additional .NET 10 backfill.

## Older approach

.NET 9 has no SDK file-application route. A small console project is a valid, explicit home for this tool. Save the following as Program.cs in an SDK console project targeting net9.0, with LangVersion 13.0, ImplicitUsings enabled and Nullable enabled. Run `dotnet run --project RepositoryUtility.csproj -c Release -- north:7 south:4`.

{{snippet:older}}

## Newer approach

Save the complete source as slots.cs. .NET 10 supplies the generated project; its SDK can build and run this file directly. Use `dotnet run --file slots.cs -c Release -- north:7 south:4`. The explicit --file avoids accidentally running a project in the current directory and passing slots.cs to that project's application.

{{snippet:newer}}

Both commands print `slots=11`. A negative count or malformed row fails; checked addition also refuses integer overflow. This route deliberately disables Native AOT publishing. No native compiler, package, provider, rendering mode or mobile workload is needed. It establishes managed execution only.

## Migration

1. Keep the original console project if its configuration or multiple entry points make that clearer. A file application is an optional smaller packaging choice.
2. Put slots.cs outside the source cone of another project's default Compile glob. Keep the explicit target/language directives when reproducing this example.
3. Inspect the nearest global.json, Directory.Build.props, Directory.Build.targets, Directory.Packages.props and NuGet.Config. File applications inherit them; a generated project does not isolate the tool from repository policy.
4. Run the same valid and rejected inputs before removing any old project. Separate SDK options from application arguments with --.
5. For pinned packages under central package management, use a versionless package directive and a matching central PackageVersion. For a standalone pinned directive, deliberately disable CPM in that file. The companion sdk.compose-file-apps recipe demonstrates and executes both choices.

## Constraints

The foundation requires a .NET 10 SDK, not merely a .NET 10 runtime. Canonical pins are 10.0.402 and, for retained use on target 11, 11.0.100-rc.2.26504.105. A repository global.json can select a different SDK even when another dotnet executable is launched. .NET 9 remains comparison-only; Playbook has no .NET 9 catalogue target.

Build with `dotnet build slots.cs -c Release`; generated output normally lives in the SDK's per-user temporary runfile cache. Do not assume a conventional bin directory beside the file. Keep package locks and repository-wide properties intentional. The serviced include directive and newer DLL/reference metadata have separate requirements explained by sdk.compose-file-apps.

## Evidence

Actual net9.0 project and net10.0/net11.0 file applications were compiled and executed. The command check runs the original sources with nondefault inputs and verifies rejected negative counts. Saved compilation records hash the source, effective shared configuration and capture helper; the installed-tool scenario checks complete canonical Markdown and target selection. .NET 11 evidence is RC2, not GA verification.
