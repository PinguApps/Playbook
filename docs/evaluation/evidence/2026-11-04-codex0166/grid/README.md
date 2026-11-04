# Grid comparison: retained evidence

PIN-953 records runs 5–6: without Playbook, then with, one fresh attempt each.
See [results and limitations](../../../grid-links/results.md) and the
[frozen protocol](../../../comparison-protocol.md). No replacement run is authorized.

Scaffold commit: 6d53662cdba54551a74b3ade063c49009c63561c, subtree
samples/GridLinksEvaluation. CLI 0.166.0 / gpt-6.1-sol / medium; SDK
11.0.100-rc.3.26522.148 and QuickGrid 11.0.0-rc.3.26522.148. The shared exact-hash
Playbook 0.1.0-pin950 package remains retained in ../controls, without rebuilding.
The protocol records its hash/source and the owner-approved pre-launch relocation.

Each condition directory retains prompts, effective config, input/source manifests,
sanitized native JSONL, termination/timing and usage, the agent's own reports/checks,
and unchanged submitted source. Authentication and generated bin/obj/module caches
are excluded. Project/build files use .txt suffixes to avoid repository discovery;
Replay.ps1 restores filenames only in disposable copies. Necessary original code,
configuration and meaningful failures remain reviewable.

Controls retain SDK identity, host reachability, matched dependency-cache hashes,
preflight scripts, actual supervisor/tools, PATH comparisons, unchanged oracle and
reference, calibration and scored failures, and the supplementary browser check.
The browser evidence resolves oracle link-selection false negatives; it does not
erase or relabel the raw frozen oracle result. Verify.ps1 checks retained integrity,
fresh contexts, order, pins, consultation and budgets. Replay.ps1 launches no model
calls, reproduces known oracle failures and checks actual browser navigation with
JavaScript disabled. Submitted sources are never repaired.

The machine transcripts duplicate source patches/tool responses and have narrow
review exclusions; hashes, effective configs, submitted source, consultation
projection and assessment remain reviewable. Pingu owns final assessment under
PIN-955. Estimate remains M (3), one bounded pair plus capture/review handoff.
No related Linear issues changed. No release or deployment is authorized.

Replay is intentionally machine-specific: the recorded SDK, Node and Chromium
paths must exist. The historical CLI hash is a capture receipt; Verify.ps1 hashes
the retained package bytes and checks the retained evaluator against repository
originals, without requiring the historical CLI to remain installed. Effective
configs retain absolute local paths and the inherited PATH to make the isolation
controls auditable. These reveal the local username/tool layout, contain no
credentials, and are retained deliberately. The redundant process-PATH snapshot
is omitted from the before/after control, which compares user and machine scope.
