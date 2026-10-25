# Install and update Playbook

`PinguApps.Playbook` installs the `playbook` command. This development package is **not ready for public publication**. Use the local package below today; NuGet.org commands describe installation after a release is published.

## Prerequisites and host runtime

Install the [.NET SDK](https://dotnet.microsoft.com/download) to use `dotnet tool install/update/restore`. The documentation server targets `net10.0` and needs the .NET runtime, with no desktop/mobile workloads. Its packaged `Major` roll-forward policy prefers .NET 10 when available and permits a higher major when .NET 10 is absent. Preview runtimes require separate opt-in; installing an RC SDK alone does not make its runtime eligible. Prefer a supported .NET 10 runtime for ordinary use. See [.NET runtime selection](https://learn.microsoft.com/en-us/dotnet/core/versions/selection).

The host runtime, application's target release and recipe's sample toolchains are separate. Reading .NET 11 guidance does not require SDK 11. Applying a recipe may require particular SDKs, packages, platforms or workloads: check full applicability first. Exact evidence versions describe tests, not mandatory installations. RC evidence does not establish GA verification.

## Published package: global install and update

After publication, run these commands from a trusted directory. The default selects the latest stable package; use `--version <published-version>` on install/update to pin one, or `--prerelease` deliberately to include previews.

```powershell
Set-Location <trusted-directory>
dotnet tool install --global PinguApps.Playbook
playbook --help
playbook --version

# Later, when you choose to update:
dotnet tool update --global PinguApps.Playbook
```

The global executable directory is `%USERPROFILE%\.dotnet\tools` on Windows or `$HOME/.dotnet/tools` on Linux/macOS. A GUI client may have an older or different PATH: use the absolute executable path in its configuration. Updating the package changes the bundled catalogue; restart its MCP connection afterwards. Nothing updates guidance during a request.

## Published package: repository-local manifest

Run in your consuming repository. Create the manifest only if it does not already exist; preserve its other tools.

```powershell
Set-Location <consumer-repository>
dotnet new tool-manifest --output .config
dotnet tool install --local PinguApps.Playbook
dotnet tool run playbook -- --help
dotnet tool run playbook -- --version

# Later, update the version recorded in .config/dotnet-tools.json:
dotnet tool update --local PinguApps.Playbook

# On another checkout, restore the committed manifest before starting a client:
dotnet tool restore
```

Commit `.config/dotnet-tools.json` when the team should share the version. Launch MCP as `dotnet tool run playbook -- mcp` with the client working directory set to that repository. `dotnet tool restore` may need a package feed/cache; do it before going offline. The local manifest is resolved from the working directory, so a global absolute executable is simpler when a client cannot set one.

## Build and inspect the current development package

Requires PowerShell 7 and the repository-pinned SDK **10.0.402**. Node **22.19+** is needed only for the optional Inspector commands. No .NET 9/11 SDK or workloads are needed to build the host from saved evidence and inspect its guidance; full [sample verification](contributing.md) has additional SDK prerequisites.

From a clean PowerShell 7 window, substitute your checkout in the first line:

```powershell
Set-Location <path-to-Playbook>
$root = (Get-Location).Path
dotnet build ./src/PinguApps.Playbook -c Release -p:Version=1.0.0-preview.1
dotnet pack ./src/PinguApps.Playbook -c Release --no-build -p:PackageVersion=1.0.0-preview.1 --output ./artifacts

# Use only this feed, rather than racing an identically versioned remote package.
$feed = (Resolve-Path ./artifacts).Path
$config = Join-Path $feed 'local-only.config'
@"
<configuration><packageSources><clear /><add key="local" value="$([System.Security.SecurityElement]::Escape($feed))" /></packageSources></configuration>
"@ | Set-Content -LiteralPath $config
dotnet tool install PinguApps.Playbook --version 1.0.0-preview.1 --tool-path ./artifacts/manual-tool --configfile $config
$playbook = (Resolve-Path ./artifacts/manual-tool/playbook* | Where-Object { -not $_.PSIsContainer } | Select-Object -First 1).Path
& $playbook
& $playbook -h
& $playbook --help
& $playbook --version

# After rebuilding/repacking a newer development version, update this installation:
# dotnet tool update PinguApps.Playbook --version <new-version> --tool-path ./artifacts/manual-tool --configfile $config
```

Bare invocation, `-h` and `--help` must show `playbook mcp`; `--version` must begin with `1.0.0-preview.1`. This uses a separate installation and leaves global tools and client settings alone. Choose a fresh tool directory/cache when verifying a repack with the same version: NuGet may reuse that identity from cache. To exercise global/local install commands with this unpublished package, add `--version 1.0.0-preview.1 --configfile $config` to install/update, using the absolute config path.

To inspect an agent's actual response, continue in the same window:

```powershell
$inspector = @('--yes', '@modelcontextprotocol/inspector@2.11.0', '--cli', '--format', 'json', '--quiet')
npx @inspector --method tools/list -- $playbook mcp
$list = npx @inspector --method tools/call --tool-name list_recipes --tool-arg 'targetVersion="10.0"' -- $playbook mcp | ConvertFrom-Json
$list.result.structuredContent.recipes | Format-List id, title, goal
$list.result.structuredContent.coverage | Format-Table target, scope, complete, description -Wrap
$detail = npx @inspector --method tools/call --tool-name get_recipe --tool-arg id=csharp.validate-property-assignment --tool-arg 'targetVersion="10.0"' -- $playbook mcp | ConvertFrom-Json
$detail.result.structuredContent.recipe.markdown
$detail.result.structuredContent.recipe.applicability | ConvertTo-Json -Depth 15
```

Expect exactly `list_recipes` and `get_recipe`, then the property recipe's ID, reusable title/goal, full Markdown and explicit applicability routes. Both tools require a target on every request. Current .NET 10 and .NET 11 coverage is incomplete. Inspector downloads on first use; that download is separate from Playbook's offline operation. Inspector initializes and closes each server connection; no process needs stopping afterwards. See [the extended walkthrough](inspect-guidance.md) for filters, paging and errors, and [PIN-675 verification](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-675.md) for tested launch paths and limits.

## Offline operation and completeness

Once installed, Playbook serves embedded metadata, complete recipe Markdown, canonical examples, migration/configuration steps, applicability, sources and saved verification evidence. It needs no checkout, account, key or network request. Source links provide attribution; they must not replace essential implementation steps.

Offline-complete recipes do **not** mean a complete catalogue or an offline development environment. Installation, updates, restore, Inspector downloads, sample dependencies and agent/model services may need network access. Packages do not bundle SDKs, workloads, databases or native toolchains. An empty search cannot prove a capability is unavailable; inspect coverage and any recovery suggestion. Playbook never inspects or edits the consumer project or client configuration.

## Troubleshooting

- **`playbook` not found:** run `dotnet tool list --global` (or `--local` in the consumer repository). Check the chosen installation and executable path. Restart the GUI client after installation or configure the absolute path; do not rely on an interactive shell alias.
- **Runtime missing / wrong architecture:** run `dotnet --info` and `dotnet --list-runtimes` in the environment where the client launches processes. Install the matching .NET 10 runtime/SDK. A higher major can satisfy `Major` roll-forward; an RC is not selected by default. For intentional preview testing only, set `DOTNET_ROLL_FORWARD_TO_PRERELEASE=1` in that server process's environment and record the limitation. Do not change machine-wide runtime policy to fix a client connection.
- **Package/version not found:** this repository's development package is unpublished. Use the local-only feed above. For published previews, specify the exact version or `--prerelease`. Check the selected feed and authentication for any private feed.
- **Local tool cannot resolve:** restore from the manifest's repository, set the client's working directory there, and retry `dotnet tool run playbook -- --version`. A relative path or manifest in another checkout may select the wrong installation.
- **MCP appears to hang in a terminal:** `playbook mcp` waits for protocol messages; it is not an interactive command prompt. Use a client or Inspector. Ctrl+C stops a manually started server; normally the client owns startup and shutdown.
- **Connection closes / invalid protocol output:** use command `playbook` with argument `mcp` (or an equivalent launch below), not bare `playbook`, which prints help. Keep shell banners and other text off stdout; diagnostics belong on stderr. Check the client's server logs and separate command/argument fields.
- **No tools or permission denied:** enable/trust the server and its tools in the client. Confirm the package is installed on the executing machine, container or remote environment, then restart the connection/session. Use [client setup](clients.md); installation does not register a server.
- **Wrong/empty guidance:** check `playbook --version`, coverage, the application's target and filters. Repeat `targetVersion` on both tools; missing/null/blank values fail. Deliberate `latest` selects the newest installed catalogue target, not the application's compatibility. Use the [companion recovery instruction](agent-instructions.md): first retry with the original selection filters, then explain any separate narrowing. Retrieve full detail before choosing a route.

Command references: [.NET tool install](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install) and [update](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-update).
