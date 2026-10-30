# Verification plan

Choose checks and retain results using [the contribution policy](../contributing.md#choose-the-check-and-retain-its-result). Routine setup/build output belongs in ignored `artifacts/` or existing CI artifacts. These bounded manual evaluations retain actual calls, frozen controls, outcomes and meaningful failures for the named release/product decision; trim duplicate setup and package logs. Required proof must outlive expiring CI artifacts.


Status: evidence principles, minimal CI/client/evaluation boundaries and native authoring cadence accepted through Round 8. No implementation or executable checks have run during planning.

## Tool and portable checks

Keep the focused core Reqnroll scenarios in one test project. Cover meaningful caller behavior: required target schemas, omitted/null/blank rejection, deliberate catalogue-only `latest` and independent list/get target safety, requested/resolved metadata, filters/shared inclusion, stable paging/editions, errors versus valid empty results, and complete structured/text responses. PIN-971 supersedes assumed-target defaults. Avoid a broad collection of tests that mirror internal methods.

Pack once. Install that exact artifact through an isolated local feed/cache and exercise real stdio initialization, list/get and shutdown. Verify .NET 10 hosting and .NET 11 major roll-forward using the actual artifact with appropriate runtime isolation. Pass the same package to one brief Windows smoke and one brief macOS smoke, then publish those verified files without repacking. Linux remains the main focused core/portable lane.

Each of T3 Code, Codex CLI, Claude Code and VS Code Copilot gets one brief real check on one available supported OS, recording actual versions and confirming both recipe body and applicability reach the agent. Do not create a client-by-OS matrix. The separate [manual evaluation](evaluation-plan.md) is four task pairs, capped at 80 execution minutes plus preparation/review.

## Content and compilation

Validate metadata, source links, snippet markers, IDs/editions, target prerequisites and sample references during authoring/build. Extract rendered snippets from the canonical sample files. Recipes must contain the essential instructions, configuration and code for their goal offline; external links are optional evidence or further reading, not required missing implementation steps.

Examples must compile against their declared SDK/target/dependencies before meeting the release gate unless explicitly excluded under the native testing policy below. Distinguish source review, compilation, cross-compilation and actual runtime execution. Use focused behavior checks where useful, including actual request/browser/database evidence where a claim depends on that behavior; do not infer execution from a successful build or mock.

Locally verified native samples need the actual SDK/workload/native toolchain. Unavailable platforms follow the explicit exclusions in the native testing policy below. A workload-free tool smoke cannot provide this evidence. Record concrete sample/source and dependency/toolchain baselines, the build performed, and its result.

## Accepted native authoring cadence

**Updated by Pingu through PIN-699, 30 October 2026:** the
[native testing policy](../contributing.md#native-testing-policy) supersedes the
historical requirement to obtain other native authoring environments. Use the
current Windows machine and existing Docker where feasible; retain hosted
package CI. Source-reviewed guidance for unavailable Mac/Android/native routes
can ship with explicit sample-specific `not-tested` disclosures. No Mac, device,
emulator or additional hardware prerequisite remains. Source/compile/execution
classifications and input freshness still apply; feasible local checks and
genuinely pending Human work remain required.

Perform feasible local native compilation during authoring. Reuse a recorded build baseline while the relevant example, target, dependency and toolchain inputs remain unchanged. Changes affecting that baseline require fresh compilation before claiming it remains verified; a previous build is not proof for new source or new pins. Refresh source review for excluded samples without inventing compilation.

This keeps weekly docs/maintenance releases on the standard flow without repeating unchanged native builds. Explicit excluded checks do not block release readiness; other missing/stale compilation and pending Human proof still do. No hidden manual-dispatch native matrix is planned.
