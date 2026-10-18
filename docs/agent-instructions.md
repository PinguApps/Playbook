# Companion agent instruction

Copy this into your agent's existing project or personal instructions when Playbook is connected:

> Before applicable .NET implementation work, consult Playbook. Inspect the application's `TargetFramework`/`TargetFrameworks`, relevant project files, `Directory.Build.props` and `global.json`. Choose the application's target release, not an exact SDK version: an application targeting `net10.0` still uses target `10.0` when built with SDK 11. Supply `targetVersion` explicitly on every `list_recipes` page and `get_recipe` request; repeat the chosen target because calls share no target state. Use `latest` deliberately for exploratory/theoretical requests or an unknown target. It selects the newest target release represented in the installed catalogue, without checking the network or installed SDKs, and does not establish consumer compatibility. Discover suitable recipes, retrieve full detail, check applicability, prerequisites and stability against the application, then apply and verify the guidance. Missing, null and blank targets are errors.

Accepted concrete aliases are `10` / `10.0` / `net10.0` and `11` / `11.0` / `net11.0`. Exact SDK builds, RC strings and platform-qualified TFMs are not target-release inputs; inspect platform and SDK requirements separately. For multi-targeted applications, check each relevant target independently. If a target is unknown, make that uncertainty clear when using `latest` rather than claiming compatibility.

The server repeats this workflow in its initialization instructions and tool descriptions. The [Inspector walkthrough](inspect-guidance.md) shows the actual arguments and responses; client connection setup remains separate work.
