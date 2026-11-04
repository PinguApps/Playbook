# Manifest comparison: retained evidence

PIN-954 records runs 7–8, with Playbook then without, one fresh attempt each.
See [observations and limitations](../../../manifest-reconciliation/results.md)
and the [owner-approved protocol](../../../comparison-protocol.md).

Both exports come from samples/ManifestEvaluation at
6d53662cdba54551a74b3ade063c49009c63561c. Codex CLI 0.166.0,
gpt-6.1-sol, medium reasoning; SDK 11.0.100-rc.3.26522.148,
net11.0 and C# 15 remain frozen. The shared exact-hash Playbook 0.1.0-pin950
package in ../controls was extracted without rebuilding. Pingu approved relocation
before scoring; controls/sdk-identity.json records all 5,467 ZIP files matching.

Each condition retains its exact prompt/config/input hashes, sanitized native
captures (committed but excluded from source review), timing/termination/usage and unchanged submitted source. Authentication,
bin/obj caches and generated module-analysis caches are excluded. Project/build
files have .txt suffixes to prevent repository build discovery. The agent's
checks/Program.cs.txt filename is original and its project explicitly includes it.
Replay.ps1 restores only project/build suffixes in disposable copies. Source is
never repaired. submission.patch records the implementation diff from the scaffold.

Controls retain the original oracle/reference/baseline and full evaluator assertion outputs,
actual supervisor/tools, probes, host network control and PATH comparison. Both
preflights succeeded before either scored launch. Verify.ps1 checks exact retained
file-set integrity, package identity, equal exports, input preservation, independent
contexts, order, consultation, preflight and the complete eight-run budget.
Replay.ps1 re-executes the private oracle for both preserved submissions and the
baseline/reference calibration without model calls. Expect ten assertions each:
nine failures for the unfinished baseline, zero for reference and both submissions.
The full logs were recaptured after review identified that the original PowerShell
redirection retained only temporary paths; native child output bypassed it. This
is evaluator recapture, not new scored attempts or backdated pre-launch evidence.

Native JSONL duplicates tool responses and patches and has narrow review exclusions;
configs, submitted source, consultation projection and assessment remain reviewable.
Actual absolute paths and inherited PATH are deliberately retained for audit and
contain no credentials. Historical CLI identity is a receipt; replay needs the
recorded SDK, not the original CLI. No model reruns are authorized. Retain failures
and missing metrics honestly. Final owner assessment remains PIN-955.

From PowerShell 7, with the recorded RC3 SDK installed:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-02322dc8'
& ./docs/evaluation/evidence/2026-11-04-codex0166/manifest/Verify.ps1
& ./docs/evaluation/evidence/2026-11-04-codex0166/manifest/Replay.ps1
```

The absolute worktree in the example is this task's verified checkout; substitute
your checkout location after merge. The scripts themselves resolve retained inputs
relative to their own directory. This evidence changes no served recipe or MCP tool. Estimate remains M (3), a
bounded two-run action plus evidence handoff. No related Linear issues changed.
