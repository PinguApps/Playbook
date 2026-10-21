# Format only the intended file and framework

## Goal

Format a depot source file without changing a neighbouring file, and select one framework when analysing a multi-target project. Keep formatting policy discoverable from the files actually being processed.

## Older approach

SDK 10 supports folder whitespace formatting with an include filter: `dotnet format whitespace . --folder --include Chosen.cs`. A normal project is also a useful explicit workspace when a standalone utility needs semantic formatting. The compiled older project is:

{{snippet:older}}

## Newer approach

SDK 11 accepts a file application directly: `dotnet format slots.cs`. For a multi-target project use `dotnet format Multi.csproj --framework net11.0`. The --framework option chooses the workspace target; it does not retarget the project or change unrelated files into that framework. For folder whitespace work, retain the explicit include command. The newer project remains ordinary SDK source:

{{snippet:newer}}

Place the formatting policy in an ancestor .editorconfig, for example `root = true` followed by a `[*]` section specifying the intended indentation. For global analyzer configuration use an applicable .globalconfig; do not rely on `is_global = true` inside an unrelated subtree's .editorconfig. SDK 11 discovers folder configuration through included files' ancestors rather than scanning unrelated directory trees.

## Migration

1. Choose project, file application or whitespace-folder mode to match the intended operation. Semantic formatting can restore dependencies and execute trusted analyzers.
2. Set --include for a bounded change and --framework for a multi-target project. Run from the directory against which relative include paths are intended.
3. Review the resulting diff. In the supplied check Chosen.cs changes and Untouched.cs remains byte-identical.
4. Repeat with --verify-no-changes; a nonzero exit means the selected scope still needs formatting. This does not certify excluded files.

## Constraints

File-app formatting and --framework are SDK 11 routes here. The previous SDK remains usable for targeted whitespace or an explicit project workspace. The recipe's C# projects and file syntax are not language-neutral metadata. No provider, UI/rendering mode or native workload is involved. The fixture makes no benchmark or repository-wide speed claim. A fresh restore may need the target's reference packs even when only one workspace target is selected; prepare those dependencies before an offline consumer check.

## Evidence

The real command check compares changed and untouched file bytes, verifies idempotence, formats an actual file application and loads a multi-target workspace with --framework net11.0. Both canonical projects compile. Full served Markdown, applicability and target selection are compared through the installed tool. This is actual formatting execution, not source-only validation; SDK 11 is the pinned RC2.
