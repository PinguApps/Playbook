# Build a selected part of a solution with a maintained filter

## Goal

Build the depot audit project while leaving an unrelated project alone. Keep the main solution intact so the rest of the team can still build the full repository.

## Older approach

.NET 10 can build a hand-authored .slnf. For a Depot.slnx containing Selected.csproj and Unselected.csproj, save Depot.slnf as `{"solution":{"path":"Depot.slnx","projects":["Selected.csproj"]}}`. Paths in the project list are relative to the parent solution, not arbitrary working-directory paths. Build it with `dotnet build Depot.slnf -c Release`. This remains a valid way to maintain a small filter.

The older compiled consumer uses this ordinary project shape:

{{snippet:older}}

## Newer approach

SDK 11 can create and maintain the filter from the CLI. From the solution directory, run `dotnet new slnf --name Depot --parent-solution Depot.slnx`, then `dotnet sln Depot.slnf add Selected.csproj`. Inspect with `dotnet sln Depot.slnf list`; remove with `dotnet sln Depot.slnf remove Selected.csproj`. Use an explicit parent solution; do not depend on a default filename. The newer consumer is independently compiled:

{{snippet:newer}}

Build Depot.slnf in Release. A filter narrows entry projects, not their required project-reference dependencies. It does not promise that every project outside its textual list is skipped when selected projects depend on it.

## Migration

1. Identify the parent solution and the projects needed for the task, including referenced dependencies.
2. Preserve the main solution. Create a separate named filter under SDK 11, or retain an existing handwritten filter.
3. Add paths deliberately, list the saved result and build the filter by its explicit filename.
4. Compare the selected build with a full-solution build when checking scope. The supplied fixture's excluded project intentionally fails if built; its filter succeeds and its full solution fails.

## Constraints

These are SDK commands, separate from the application's target runtime. This edition declares target 11 using SDK 11; the .NET 10 project/filter is the previous-release comparison, not another historical foundation. No packages, platform workloads, provider, rendering mode or experimental opt-in are needed for the portable fixture. Restore or MSBuild targets of selected dependencies still run. No elapsed-time or CI-cost saving is inferred from selecting fewer projects.

## Evidence

Both declared project targets compile. Real SDK commands create/add/list/remove the newer filter, and both SDKs build only the selected fixture while the full solution hits the intentional unselected-project error. Actual installed stdio retrieval checks discovery goals, full Markdown and the selected target edition. SDK 11 proof uses RC2 artifacts.
