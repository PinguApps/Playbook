# PinguApps.Playbook

Give your coding agent practical guidance for modern .NET.

An agent can build a working app while overlooking newer framework features or choosing patterns from an older release. Playbook gives it a searchable catalogue of original implementation recipes for .NET 11 and selected .NET 10 foundations, with examples, setup steps, previous-release comparisons and verification limits.

Connect it to your agent through MCP, then ask for your feature as usual. The agent can discover relevant recipes and read the full guidance before writing code. For example, a Blazor task might call for bookmarkable grid sorting and paging, browser validation for a server-rendered form, or a confirmation that appears once after a redirect.

## What you get

- **Guidance for your application's target.** Each request selects .NET 10 or .NET 11 explicitly, independently of the SDK used to build your app.
- **Implementation detail the agent can use.** Recipes include complete Markdown, canonical examples, configuration and migration steps, sources and applicability requirements.
- **Visible limits.** Requirements, optional routes and saved verification evidence help distinguish a supported approach from provisional or unverified behaviour.
- **Offline operation.** Once installed, Playbook needs no account, API key, checkout or network connection. Updating the package updates the bundled catalogue.

Playbook supplies guidance; your agent implements and tests the changes. It does not inspect or edit your project itself. Coverage is incomplete, and an empty search does not mean a framework capability is unavailable.

## Get started

Until a release is published, use the [local development package walkthrough](docs/installation.md#build-and-inspect-the-current-development-package). After publication, install the NuGet tool:

```powershell
dotnet tool install --global PinguApps.Playbook
playbook --version
```

The server needs .NET 11 GA, or a later stable major runtime when .NET 11 is absent. No desktop or mobile workloads are required to run it. Your application may need additional SDKs, packages or workloads specified by a recipe.

Follow the [client setup guide](docs/clients.md) for T3 Code, Codex CLI, Claude Code or VS Code Copilot. Each client launches `playbook mcp` as its own stdio process. Use `find_guidance` to discover recipes for several implementation goals at once, `list_recipes` to browse the catalogue with filters, and `get_recipe` to read the full guidance.

Playbook tells the agent how to search for guidance as part of normal feature work. Tool use depends on the agent and client. The [companion instruction](docs/agent-instructions.md) is available if you want to reinforce the workflow in your own projects.

Try asking your agent:

> Build a .NET 11 Blazor Web App with a sortable, paginated grid. Make the current sort and page reproducible by sharing the URL.

Every tool call requires a target. A deliberate `latest` choice selects the newest release in the installed catalogue; it does not establish application compatibility.

## Release status and verification

Pre-GA preview packages retain explicitly labelled provisional guidance and unverified behaviour. They do not establish GA compatibility or stable-release readiness. The [release policy](docs/planning/release-plan.md) defines the evidence and named exceptions required for publication. Check each recipe's applicability and evidence rather than treating inclusion as proof that every scenario has been executed.

## Learn more

- [Installation, updates and troubleshooting](docs/installation.md)
- [See exactly what an agent receives](docs/inspect-guidance.md)
- [Connect your agent](docs/clients.md)
- [Contribute and verify recipes](docs/contributing.md)
- [Repository automation and publication](docs/automation.md)
