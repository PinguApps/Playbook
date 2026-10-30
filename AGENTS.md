# Agent instructions

## Verification

Name the behaviour/claim at risk and choose the narrowest meaningful existing check. Reuse evidence only while relevant inputs remain valid; refresh affected evidence. Stop when relevant checks pass unless a change, failure or unresolved concern warrants more. Documentation-only work does not trigger an unrelated package/browser/native matrix; runtime, dependency, generated-consumer and deployment claims require the affected real consumer/artifact path.

Do not create per-ticket verification documents or commit raw run output by default. Report commands, results and limitations in the PR and final handoff; put disposable output in ignored `artifacts/` or existing CI artifacts. Commit evidence only for a named ongoing check, supported claim or release decision, including exceptional manual/evaluation proof. Preserve sample compilation/freshness records and pending Human gates. For capture, retention and refresh decisions, follow [the contribution guide](docs/contributing.md#choose-the-check-and-retain-its-result).

For native/platform guidance, apply the [native testing policy](docs/contributing.md#native-testing-policy): use feasible checks on this Windows machine and existing Docker, retain package CI, and disclose unavailable platform checks as sample-specific `not-tested` exclusions rather than requiring new hardware or claiming verification. Pingu owns feasible Human approvals and refresh coordination.

## MCP changes

For every addition or change to MCP tools or served recipes, include a tested, copyable PowerShell example in the final handoff that shows what an agent receives. Follow [the MCP Inspector walkthrough](docs/inspect-guidance.md), adapting the target, recipe ID and arguments to the change.

Include the setup/build commands, call the affected tools through the real built or installed server, and display the relevant discovery description, Markdown and applicability. State the expected result and any verification gap. Include the commands in the response as well as any documentation link.

Every copyable PowerShell example must select the task's worktree with `Set-Location -LiteralPath '...'` before using relative paths. Write the actual, verified absolute worktree path literally in the command. Never use Git commands or repository-root discovery to choose the location, or assume the reader starts in the correct checkout. Verify the example from an unrelated directory.

## Recipe discovery

Write recipe titles and goals around the reusable capability and when to apply it. Use the existing `goal` field as the explanatory discovery description, and show it alongside ID and title in inspection examples. Keep fictional domains in the worked examples; preserve stable recipe IDs when improving discovery wording.

## Project and dependency changes

Before creating, moving, retargeting or otherwise changing a project, its dependencies, shared build/package files, workloads, tools, containers or CI workflows, read [the dependency-update policy](docs/dependency-updates.md). Review effective inheritance and update `renovate.json` and its policy checks in the same PR when needed. Keep version-specific examples pinned and grant automated updates only to explicitly reviewed packages, locations and release lines. Verify both intended updates and protected baselines with the documented checks; account for every affected project in the PR's validation evidence.
