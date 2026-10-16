# Verification plan

Status: accepted evidence principles and minimal CI/client/evaluation boundaries, with the native authoring cadence below proposed in Round 8. No implementation or checks have run during planning.

## Tool and portable checks

Keep the focused core Reqnroll scenarios in one test project. Cover meaningful caller behavior: explicit-target safety, assumed-target metadata, filters/shared inclusion, stable paging/editions, errors versus valid empty results, and complete structured/text responses. Avoid a broad collection of tests that mirror internal methods.

Pack once. Install that exact artifact through an isolated local feed/cache and exercise real stdio initialization, list/get and shutdown. Verify .NET 10 hosting and .NET 11 major roll-forward using the actual artifact with appropriate runtime isolation. Pass the same package to one brief Windows smoke and one brief macOS smoke, then publish those verified files without repacking. Linux remains the main focused core/portable lane.

Each of T3 Code, Codex CLI, Claude Code and VS Code Copilot gets one brief real check on one available supported OS, recording actual versions and confirming both recipe body and applicability reach the agent. Do not create a client-by-OS matrix. The separate [manual evaluation](evaluation-plan.md) is four task pairs, capped at 80 execution minutes plus preparation/review.

## Content and compilation

Validate metadata, source links, snippet markers, IDs/editions, target prerequisites and sample references during authoring/build. Extract rendered snippets from the canonical sample files. Recipes must contain the essential instructions, configuration and code for their goal offline; external links are optional evidence or further reading, not required missing implementation steps.

Examples must compile against their declared SDK/target/dependencies before meeting the release gate. Distinguish source review, compilation, cross-compilation and actual runtime execution. Use focused behavior checks where useful, including actual request/browser/database evidence where a claim depends on that behavior; do not infer execution from a successful build or mock.

Native samples need the appropriate SDK/workload/native toolchain, with a Mac and matching Xcode for affected Apple examples. A workload-free tool smoke cannot provide this evidence. Record concrete sample/source and dependency/toolchain baselines, the build performed, and its result.

## Proposed native authoring cadence

Perform native sample compilation as content-authoring verification on an appropriate machine, rather than adding native builds or UI/device suites to routine Windows/macOS CI. Reuse a recorded build baseline while the relevant example, target, dependency and toolchain inputs remain unchanged. Changes affecting that baseline require fresh compilation before publication; a previous build is not proof for new source or new pins.

This keeps weekly docs/maintenance releases on the standard flow without repeating every native build. It does not waive the compile gate or allow a complete catalogue to ship with missing native evidence. Suitable native authoring environments remain a delivery prerequisite; none has been established by this planning work. No hidden manual-dispatch native matrix is planned.
