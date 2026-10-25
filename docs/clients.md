# Connect an agent to Playbook

First [install Playbook](installation.md) in the environment where the agent runs and check `playbook --version`. All four connections use a client-owned **stdio** process: command `playbook`, argument `mcp`. There is no URL, port, daemon, API key or Playbook account. The examples below are instructions for you to apply; Playbook does not register itself or edit client settings.

For the unpublished development package, replace `playbook` with the absolute executable path from the installation walkthrough. On Windows use `playbook.exe`; on Linux/macOS use `playbook`. Put the executable alone in `command`, and `mcp` in `args`. JSON Windows paths need doubled backslashes (or forward slashes). GUI clients may not inherit your shell's PATH. Install and configure on the executing environment, including a remote machine/container when applicable.

## T3 Code

Use T3 Code's **Codex provider** for this connection path. T3 Code runs providers on the selected environment's machine; configure the same Codex home that provider uses.

1. Open **Settings → Providers**, select the environment and Codex instance, and check its **CODEX_HOME path**. With the default home, follow the Codex CLI setup below on that machine. For a custom home, merge the TOML entry into that home's `config.toml` instead. Do not register it in an unrelated local Codex home.
2. Start a fresh T3 Code thread with that Codex instance after saving the configuration. Ask the agent to discover Playbook's tools and perform the list/get check below. Check that both the Markdown and applicability reach the agent.
3. Add the [companion instruction](agent-instructions.md) to the project's existing `AGENTS.md` or the selected provider's personal instructions. Include its empty-search recovery paragraph: replay the suggestion with the original selection filters before any separately explained narrower search. Refresh an older copied instruction when updating Playbook.

T3 Code's Claude provider instead uses Claude Code's configuration; follow the Claude Code section with the same **CLAUDE_CONFIG_DIR path** selected in provider settings. This is an alternative, not an additional required client check. Other providers may have different MCP configuration; these examples do not establish support for every provider.

Sources: T3 Code's [provider setup](https://github.com/pingdotgg/t3code/blob/main/docs/user/install.md#providers), [Codex homes](https://github.com/pingdotgg/t3code/blob/main/docs/user/providers-codex.md) and [Claude configuration](https://github.com/pingdotgg/t3code/blob/main/docs/user/providers-claude.md). T3 Code's own orchestration MCP server is separate from the Playbook server being connected here.

For owner-authorized fresh-thread checks, use the [isolated verification project](t3-verification.md). Its tested project-scoped Codex connection needs trust for that specific seed repository; launching a T3 thread alone does not inject Playbook. The [setup receipt](verification/PIN-976.md) records actual calls, package identity and the initial missing-trust failure.

## Codex CLI

Register the installed command, then inspect the saved entry:

```powershell
Set-Location <consumer-repository>
codex mcp add playbook -- playbook mcp
codex mcp get playbook
codex mcp list
```

Alternatively, merge this into `~/.codex/config.toml` (or the selected custom Codex home's `config.toml`):

```toml
[mcp_servers.playbook]
command = "playbook"
args = ["mcp"]
```

Use an absolute executable path for an explicit-path install. A repository-local manifest needs `command = "dotnet"`, `args = ["tool", "run", "playbook", "--", "mcp"]`, and `cwd = "/absolute/path/to/consumer-repository"`. Restore that manifest first. For Windows TOML paths, forward slashes avoid backslash escapes. Start a new Codex session and use `/mcp` to inspect the connection. Add the companion instruction to existing `AGENTS.md` guidance.

Source: [official Codex MCP documentation](https://developers.openai.com/codex/mcp/).

## Claude Code

This example deliberately uses user scope, making the server available across repositories. Use `--scope project` instead for a shared repository `.mcp.json`; review that configuration before committing it.

```powershell
Set-Location <consumer-repository>
claude mcp add --transport stdio --scope user playbook -- playbook mcp
claude mcp get playbook
claude mcp list
```

For an explicit-path install, replace the second `playbook` with the absolute executable path, quoting paths containing spaces. For a local manifest, use `--scope project` and launch `dotnet tool run playbook -- mcp` after `--`; start Claude in that manifest's repository. Begin a new session, open `/mcp`, and approve/enable the server and its tools as prompted. Add the companion instruction to existing `CLAUDE.md` guidance. A native Playbook executable needs no `cmd /c` or `npx` wrapper on Windows.

Source: [Claude Code MCP setup, scopes and status](https://code.claude.com/docs/en/mcp).

## VS Code Copilot

For workspace setup, merge this entry into `.vscode/mcp.json`, preserving other servers:

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

For a local manifest replace the command with `dotnet`, use `args: ["tool", "run", "playbook", "--", "mcp"]` and add `"cwd": "${workspaceFolder}"`; restore from that folder first. For personal setup use **MCP: Open User Configuration**, or **MCP: Add Server → Command (stdio)** and select the desired scope. Workspace remote configurations run on the remote environment; install the package there.

Use **MCP: List Servers → playbook → Start** (or the inline Start action in `mcp.json`), review the trust prompt, and inspect server output if startup fails. In Copilot's agent chat, enable the Playbook tools in the tool picker and run the check below. Add the companion instruction to existing `.github/copilot-instructions.md` guidance.

Sources: [VS Code MCP setup and lifecycle](https://code.visualstudio.com/docs/agent-customization/mcp-servers) and [configuration reference](https://code.visualstudio.com/docs/agents/reference/mcp-configuration).

## Check the connection and consultation

Ask the agent:

> Use Playbook to call `list_recipes` with `targetVersion: "10.0"`. Show a returned ID, title and goal. Then call `get_recipe` for `csharp.validate-property-assignment` with `targetVersion: "10.0"`. Show its Markdown and full applicability routes, distinguishing requirements from verification baselines. Report coverage and limitations.

The agent should receive the property recipe's guidance and .NET 10 routes, including the optional SDK 11 alternative. Discovery alone is insufficient. The server also returns a complete JSON text fallback for clients that do not display structured content. If tools are unavailable, inspect client trust/tool settings, executable path and logs. Close/restart the connection through the client rather than keeping a separate terminal server alive.

Copy the [companion consultation instruction](agent-instructions.md) into the relevant instruction file; connecting a server alone does not ensure the agent consults it. Do not overwrite existing instructions. Use the [Inspector walkthrough](installation.md#build-and-inspect-the-current-development-package) to isolate launch/transport problems from agent behaviour.

These configurations were checked against current primary documentation/source on **19 October 2026**, and launch forms were exercised against the actual packed tool. This is **not real client-version evidence**. The separate checks [PIN-942 (T3 Code)](https://linear.app/pinguapps/issue/PIN-942), [PIN-943 (Codex CLI)](https://linear.app/pinguapps/issue/PIN-943), [PIN-944 (Claude Code)](https://linear.app/pinguapps/issue/PIN-944) and [PIN-945 (VS Code Copilot)](https://linear.app/pinguapps/issue/PIN-945) own that evidence. Each should record client/OS/tool versions, artifact identity, configuration, agent-visible discovery/detail and shutdown on one available supported OS; no client-by-OS matrix is implied. See [packed-launch verification](verification/PIN-675.md).
