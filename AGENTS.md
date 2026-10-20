# Agent instructions

## MCP changes

For every addition or change to MCP tools or served recipes, include a tested, copyable PowerShell example in the final handoff that shows what an agent receives. Follow [the MCP Inspector walkthrough](docs/inspect-guidance.md), adapting the target, recipe ID and arguments to the change.

Include the setup/build commands, call the affected tools through the real built or installed server, and display the relevant discovery description, Markdown and applicability. State the expected result and any verification gap. Include the commands in the response as well as any documentation link.

Every copyable PowerShell example must select the task's worktree with `Set-Location -LiteralPath '...'` before using relative paths. Write the actual, verified absolute worktree path literally in the command. Never use Git commands or repository-root discovery to choose the location, or assume the reader starts in the correct checkout. Verify the example from an unrelated directory.

## Recipe discovery

Write recipe titles and goals around the reusable capability and when to apply it. Use the existing `goal` field as the explanatory discovery description, and show it alongside ID and title in inspection examples. Keep fictional domains in the worked examples; preserve stable recipe IDs when improving discovery wording.
