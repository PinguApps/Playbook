# PinguApps.Playbook

Original .NET guidance served offline to coding agents through a client-owned stdio MCP process.

This development slice contains `csharp.validate-property-assignment`, comparing a .NET 9 explicit backing field with the .NET 10 / C# 14 `field` keyword, using one canonical edition for explicitly verified .NET 10 and .NET 11 targets. Both tools require an explicit target release or deliberate `latest` choice; `latest` means the newest target release in the installed catalogue, not an exact SDK version or consumer compatibility. Requirements include a verified .NET 11 SDK route for .NET 10. Coverage distinguishes curated .NET 10 from the comprehensive .NET 11 launch commitment; both remain incomplete. It is not ready for public publication.

Four .NET 11 recipes also cover C# unions, closed hierarchies, union JSON round trips and inferred polymorphism. Their original .NET 10 comparisons and .NET 11 RC2 samples execute real matching and serializer checks. See [the evidence and inspection commands](docs/verification/PIN-711.md), including null/default handling, assembly boundaries, generated metadata and wire-contract migration limits.

The .NET 11 `csharp.static-interface-helpers` recipe explains fixed helper implementations without adding implementer obligations. Its [comparison, compiler checks and inspection commands](docs/verification/PIN-714.md) also show that ordinary static interface helpers already work on .NET 10; C# 15's change removes a restriction for runtimes without default interface methods.

```powershell
playbook --help
playbook --version
playbook mcp
```

The host needs .NET 10, or a later major runtime when .NET 10 is absent. Installed operation needs no workloads, checkout, accounts, keys or network. MCP stdout is protocol-only; diagnostics use stderr. The tools are `list_recipes` and `get_recipe`. Guidance does not inspect or edit consumer projects.

Start with [installation, updates and troubleshooting](docs/installation.md), then [connect T3 Code, Codex CLI, Claude Code or VS Code Copilot](docs/clients.md). Add the [companion agent instruction](docs/agent-instructions.md) to your agent's existing guidance.

To see what an agent receives, follow [the interactive list/get walkthrough](docs/inspect-guidance.md). See [verification and authoring](docs/contributing.md) for adding recipes, and [repository automation](docs/automation.md) for CI and publication.
