# PinguApps.Playbook

Original .NET guidance served offline to coding agents through a client-owned stdio MCP process.

This first development slice contains `csharp.validate-property-assignment`, comparing a .NET 9 explicit backing field with the .NET 10 / C# 14 `field` keyword. It is not the complete launch catalogue and is not ready for public publication.

```powershell
playbook --help
playbook --version
playbook mcp
```

The host needs .NET 10, or a later major runtime when .NET 10 is absent. Installed operation needs no workloads, checkout, accounts, keys or network. MCP stdout is protocol-only; diagnostics use stderr. The tools are `list_recipes` and `get_recipe`. Guidance does not inspect or edit consumer projects.

To see what an agent receives, follow [the interactive list/get walkthrough](docs/inspect-guidance.md). See [verification and authoring](docs/contributing.md) for building, installing the development package and adding recipes, and [repository automation](docs/automation.md) for CI and publication.
