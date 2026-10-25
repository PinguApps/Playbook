# PinguApps.Playbook

Original .NET guidance served offline to coding agents through a client-owned stdio MCP process.

The development catalogue covers .NET 11 goals with original previous-release comparisons and selected .NET 10 foundations. Coverage and release proof remain incomplete; it is not ready for public publication. Both tools require an explicit application target or deliberate `latest` choice. `latest` means the newest release in the installed catalogue, not an SDK build or consumer compatibility.

```powershell
playbook --help
playbook --version
playbook mcp
```

The host needs .NET 10, or a later major runtime when .NET 10 is absent. Installed operation needs no workloads, checkout, accounts, keys or network. MCP stdout is protocol-only; diagnostics use stderr. The tools are `list_recipes` and `get_recipe`. Guidance does not inspect or edit consumer projects.

Start with [installation, updates and troubleshooting](docs/installation.md), then [connect T3 Code, Codex CLI, Claude Code or VS Code Copilot](docs/clients.md). Add the [companion agent instruction](docs/agent-instructions.md) to your agent's existing guidance.

To see what an agent receives, follow [the interactive list/get walkthrough](docs/inspect-guidance.md). See [verification and authoring](docs/contributing.md) for adding recipes, and [repository automation](docs/automation.md) for CI and publication.
