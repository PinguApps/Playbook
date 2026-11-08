# Complete-catalogue agent usability evidence

8 November 2026: A/C/D pass; B finds the requested guidance but exposes conflicting
item-glob evidence wording. Four uncoached first passes took **268.830 seconds**
summed execution; each finished within 74 seconds. No timeout or corrective rerun.
This is a bounded preview usability observation, not GA/release authorization.

## Candidate and coverage

Newer verified local candidate **0.0.0-preview.979**, source
`d38ad115677ab4de6c7d108449d9b2efb8daa19b`, SHA-256
`5bb44f58c8d7c5d200bd6d2c91fd5db42f57f2e236950a491ed929c252eeb19c`.
Build used SDK10.0.403, then release-ready preview pack passed. The installed
executable returned that version plus source SHA. Nothing was published.
Fresh local-only feed/cache/home/tool slot:
`V:\_Scratch\PlaybookVerification\packages\pin979-1`.

[PIN-959's report](../planning/audits/content-readiness.md) is authoritative for
coverage: 406 historical /455 current disposition rows,38 Blazor rows preserving
25+7+3 with the accepted split,23 source/API audits and exactly eight foundations.
`eng/Test-ContentReadiness.ps1` passed current input hashes, row destinations,
foundation identities, preview approval and all15 pending gates. This checkpoint
uses its approved pre-GA completion boundary, not an inference from a recipe count.
The eight identities remain:

- `csharp.extend-uneditable-types`
- `csharp.validate-property-assignment`
- `aspnet-core.validate-requests`
- `ef.include-archived-with-tenant-filter`
- `linq.retain-unmatched-rows`
- `json.reject-malformed-contracts`
- `linq.transform-async-stream`
- `sdk.run-file-utility`

Actual installed stdio discovery from `C:\Windows\Temp` enumerated **485** target11
IDs in ten pages and **20** target10 IDs; their union is **486 distinct IDs**.
[Retained catalogue](agent-usability-complete.catalogue.json). Compared with
[PIN-978's171 IDs](agent-usability-expanded.catalogue.json),315 were added and none
removed. This includes later web/data, runtime, SDK, desktop/mobile and migration
deliveries. Editions are not added to the identity count. Current discovery still
reports `complete:false` and development-catalogue wording for both targets;
that conservative metadata is not a substitute for the accountable coverage audit.
Stable readiness remains gated by the named provisional scopes and GA owners.

## Environment and controls

[Frozen prompts/checks](../agent-usability-complete-catalogue.md) were committed
before launch. Ordinary top-level T3 threads used the existing dedicated project
`mcp:cb660864-7bf7-43f9-b205-1a54c5e763cf`, seed revision
`b4276211cae5872236f0763eace8c46e7067bc10` on `verify/pin979-seed`.
Only its installed executable path changed from the retained PIN-1018 seed.
All four app-created worktrees have no parent/inherited answers and one completed
run. Earlier seeds, packages and results are preserved. No automatic schedule or CI
evaluation, OS/client/provider matrix, native hardware or external service was used.

Windows / T3 Code, provider **codex**, model **gpt-6.1-sol**, **medium** reasoning,
**default** service tier, full-access/default modes. These settings are confirmed
by live capabilities and all four launch results. Earlier checkpoints used high
reasoning; prompts/content also differ. No causal improvement is claimed.
The historical `t3_thread_configuration` tool is no longer callable; capabilities
and launch/read-back records supply the actual settings instead. Client/provider
binary versions and token usage were not measured in this checkpoint.

Unchanged package-free consumer: net10.0/C#14, exact SDK
11.0.100-rc.2.26504.105 with roll-forward disabled and empty build/package boundaries.
Baseline build/run passed before launch. Actual agent commands and independent
coordinator checks used the installed SDK11 executable and system .NET10 host.
Persistent user/machine PATH comparisons after installation and consumer checks
match the saved pre-install snapshot. No persistent settings were intentionally
changed; a before-build PATH snapshot was not captured for the initial ordinary build.

## Observed outcomes

Complete recovered activity, tool arguments/results, answers and command output
are retained in the four JSON receipts below. No reasoning was retained. Every
get returned complete canonical Markdown matching the compiled catalogue exactly,
with selected applicability: eight gets, seven distinct recipes, bodies6286–19499
characters. [Metrics](agent-usability-complete.metrics.json) record every observed
output's serialized character size; these are neither wire sizes nor token counts.
Largest discovery output was92294 characters. No delivery truncation was found.

1. **A PASS,67.036s.** [Receipt](agent-usability-complete.a.json).
   Pages use target10/language csharp, limit3, offsets0 then3; subsequent narrowing
   adds C# area/property keywords. Detail precedes edits and distinguishes the
   optional SDK11 route from the net10/C#14 application. Only Program.cs changes:
   explicit field removed, `field` assignment and sentinel initializer retained.
2. **B PARTIAL,59.459s.** [Receipt](agent-usability-complete.b.json).
   Latest is explicit throughout and resolves11. Invalid human-readable language
   values `F#`/`C#` first return validation errors; the agent corrects them to
   supported identifiers without coaching. It finds experimental F# async-sequence
   and neutral XML item-glob guidance, distinguishes patterns from expanded files,
   and preserves unknown-target/experimental/RC limitations. It detects conflicting
   evidence prose and conservatively declines an execution claim. Its statement
   that structured records show compilation only does not erase the report's
   separately observed happy-path output. Checkout remains clean.
3. **C PASS,73.091s.** [Receipt](agent-usability-complete.c.json).
   Inspects target/language/pin and build boundaries; an empty target10 union
   search is followed by its filter-preserving retry. Explicit latest exploration
   retrieves both C# alternatives, whose net11/C#15 requirements explain rejection
   under this consumer's constraints. No upgrade is recommended. Separate unknown
   Blazor exploration obtains both experimental full recipes, explains restoration
   versus synchronized state and retains browser/provider/pending-proof limits.
   No consumer compatibility or web execution is claimed. Checkout remains clean.
4. **D PASS,69.244s.** [Receipt](agent-usability-complete.d.json).
   Exact requested phrase returns zero matches. Position5's first retry equals
   position4's complete suggestedRequest: target10, empty area/language sets,
   includeShared=true, null query, offset0, limit20. Full target10 property detail
   precedes editing. Same correct Program.cs change as A; no configuration upgrade.

Independent builds of both generated consumers with `--no-restore` passed with
zero warnings/errors; executing each DLL on the .NET10 host passed constructor,
assignment, blank rejection and retained-value assertions. Both diffs contain only
the property simplification; all project/global/build/connection/instruction files
remain unchanged. B/C have empty Git status. Exploratory prose was not compiled;
F#/Blazor/provider behavior is not newly verified by this checkpoint.

## Remaining defects and release implications

- **Unresolved evidence wording:** `msbuild.inspect-item-globs` says in its newer
  approach that none of the commands has run, while its evidence section records
  both targets returning descriptions and the expected payload file. PIN-959's
  report confirms that bounded happy-path observation; negative/mutation/import
  checks remain unperformed. B therefore cannot give one coherent evidence
  interpretation. Narrow follow-up: reconcile that sentence with the observed
  happy path and remaining checks during the existing SDK/MSBuild GA evidence
  reconciliation; do not promote compilation to execution or clear pending proof.
  This report records the defect without changing served content or its approved
  preview hashes. No new follow-up issue was created.
- **Recovery regression did not recur:** PIN-978 D remains historically failed;
  PIN-1018's separate passing receipt is unchanged. This new D independently
  preserves every compatibility filter. One observation establishes no reliability
  guarantee across models or clients.
- Initial language-enum errors were recoverable friction, not a server acceptance
  bug. Existing supported-value feedback was sufficient; no API change is proposed.
- Pending GA proof, native/Maps exclusions and owner-only gates remain exactly as
  reported by PIN-959. No map service calls, hardware matrix or cloud resources.
  PIN-967 still requires affected defect/evidence refresh for the final GA candidate.

All five PIN-979 criteria have evidence: candidate/coverage above; frozen bounded
real-agent exercise and complete calls; decisions plus independent consumers;
recorded settings/timing/sizes/failures; and this report's unresolved defect and
release limits. This is not a uniformly passing product certification. Related
Linear issues read to determine implementation were PIN-959 and PIN-978; neither
was changed. No related Linear issue was created or modified.
