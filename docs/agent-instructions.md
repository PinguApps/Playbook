# Companion agent instruction

Copy this into your agent's existing project or personal instructions when Playbook is connected:

> Before choosing .NET APIs or patterns, inspect the application's target frameworks, project/build files, and `global.json`. Use the application's target release. Call `find_guidance` with one short goal per implementation decision. Use `list_recipes` for filtered catalogue browsing. Supply `targetVersion` on every call, including each page and each `get_recipe` request. Use `latest` deliberately for exploration or an unknown target. It selects the newest installed catalogue target and does not establish compatibility. Read the full recipes and check the chosen routes' requirements and stability before applying and verifying guidance. Primary and optional routes are separate choices. Evidence baselines record the versions used in testing. RC evidence does not establish GA verification.
>
> If `find_guidance` returns a `suggestedRequest`, replay its `targetVersion` and `goals`. For a suggested `list_recipes` retry, preserve the target, areas, languages, and `includeShared`, and change the keywords or paging as suggested. An unmatched result is limited to the request and the installed catalogue coverage.

Accepted concrete aliases are `10` / `10.0` / `net10.0` and `11` / `11.0` / `net11.0`. An application targeting `net10.0` still uses `10.0` when built with SDK 11. Inspect `TargetFramework`/`TargetFrameworks`, relevant project files, `Directory.Build.props` and `global.json`. Missing, null and blank targets fail. Exact SDK builds, RC strings and platform-qualified TFMs are not target-release inputs; inspect platform and SDK requirements separately. For multi-targeted applications, check each relevant target independently. If a target is unknown, make that uncertainty clear when using `latest` rather than claiming compatibility. Discovery has compact selection conditions, not complete prerequisites or evidence; detail is required before applying guidance.

The server repeats this workflow in its initialization instructions and tool descriptions. The [Inspector walkthrough](inspect-guidance.md) shows actual arguments and responses; [client setup](clients.md) identifies the instruction file for each client.

For C#, distinguish the target framework's default language version from an explicit
`LangVersion`, inherited `Directory.Build.props`/targets and SDK selection through
`global.json`. Diagnose the evaluated project, not just the installed SDK. Temporarily
add `#error version` to a compiled source file: CS8304 reports the compiler and selected
language version. Remove the directive and rebuild successfully before committing.
Prefer the TFM default (omit `LangVersion`) or a supported explicit version such as
`15.0` for net11.0. Avoid `LangVersion=latest`: it varies with the compiler installation.
`preview` deliberately enables experimental language features and is a separate opt-in;
it is not interchangeable with `latest`. Do not recommend newer-language/older-TFM
combinations. Playbook's `targetVersion=latest` selects catalogue content, not a compiler
language setting. See [Microsoft's language-version guidance](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version).
