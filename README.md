# PinguApps.Playbook

[![PinguApps.Playbook version](https://img.shields.io/nuget/v/PinguApps.Playbook?style=for-the-badge&label=PinguApps.Playbook)](https://www.nuget.org/packages/PinguApps.Playbook/)
[![PinguApps.Playbook downloads](https://img.shields.io/nuget/dt/PinguApps.Playbook?style=for-the-badge&label=downloads)](https://www.nuget.org/packages/PinguApps.Playbook/)

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

### 1. Install Playbook

Install Playbook on the machine where your agent runs. The install command needs a .NET SDK. Playbook runs on the .NET 11 GA runtime, or a later stable major runtime when .NET 11 is absent. Install any SDKs, packages, or workloads that your chosen recipes require for your application.

Once the stable release is on NuGet, run:

```powershell
dotnet tool install --global PinguApps.Playbook
playbook --version
```

Before the stable release, use the [local development install](docs/installation.md#build-and-inspect-the-current-development-package).

### 2. Connect your agent

The client launches `playbook mcp` as a local stdio process. Set the command to `playbook` and pass `mcp` as its only argument. If your agent cannot find `playbook`, use the full executable path. A global install normally puts it at `%USERPROFILE%\.dotnet\tools\playbook.exe` on Windows or `~/.dotnet/tools/playbook` on Linux and macOS. Use an expanded absolute path in the config.

#### Codex CLI

Register Playbook and inspect the saved entry:

```powershell
codex mcp add playbook -- playbook mcp
codex mcp get playbook
```

You can also merge the equivalent entry into `~/.codex/config.toml`:

```toml
[mcp_servers.playbook]
command = "playbook"
args = ["mcp"]
```

Start a new Codex session and run `/mcp` to check the connection.

#### T3 Code (Codex provider)

Open **Settings → Providers**, select your Codex instance, and check its **CODEX_HOME path**. Use the Codex setup above on the machine where that instance runs. If it uses a custom home, put the TOML entry in that home's `config.toml`. After saving, refresh providers in T3 Code and start a fresh thread with that Codex instance.

#### Claude Code

Register Playbook for your user account:

```powershell
claude mcp add --transport stdio --scope user playbook -- playbook mcp
claude mcp get playbook
```

Use `--scope project` if you want Claude Code to save a project entry in `.mcp.json`. Start a new session and use `/mcp` to check the connection and approve the server if prompted.

#### VS Code with GitHub Copilot

Add Playbook under `servers` in your workspace's `.vscode/mcp.json`. For a personal setup, open **MCP: Open User Configuration** from the Command Palette and add the same entry.

```json
{
  "servers": {
    "playbook": {
      "type": "stdio",
      "command": "playbook",
      "args": ["mcp"]
    }
  }
}
```

In the Command Palette, use **MCP: List Servers → playbook → Start** and approve the trust prompt. In Copilot chat, make sure Playbook is enabled in the tool picker.

For repository-local tool manifests, remote environments, and more connection checks, see the [client setup guide](docs/clients.md).

### 3. Check the connection

Ask your agent to use Playbook to find .NET 11 guidance for keeping two Blazor QuickGrid views in the URL, then open the best recipe. A working connection returns candidate guidance from `find_guidance` and the full recipe and applicability from `get_recipe`. Use `list_recipes` when you want to browse by filters.

Once connected, ask for a feature as usual. For example:

> Build a .NET 11 Blazor Web App with a sortable, paginated grid. Make the current sort and page reproducible by sharing the URL.

Every tool call requires a target. Use the app's target framework for implementation. `latest` selects the newest release in the installed catalogue for exploration. The [companion instruction](docs/agent-instructions.md) is available if you want to reinforce the workflow.

## Release status and verification

Pre-GA preview packages retain explicitly labelled provisional guidance and unverified behaviour. They do not establish GA compatibility or stable-release readiness. The [release policy](docs/planning/release-plan.md) defines the evidence and named exceptions required for publication. Check each recipe's applicability and evidence rather than treating inclusion as proof that every scenario has been executed.

## Learn more

- [Installation, updates and troubleshooting](docs/installation.md)
- [See exactly what an agent receives](docs/inspect-guidance.md)
- [Connect your agent](docs/clients.md)
- [Contribute and verify recipes](docs/contributing.md)
- [Repository automation and publication](docs/automation.md)
