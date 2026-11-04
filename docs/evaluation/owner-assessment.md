# Four-pair assessment recommendation

Status: **accepted by Pingu under PIN-955 on 5 November 2026**.
Release authorization remains pending. This agent-assisted assessment concerns the
eight retained RC3 attempts, not a new benchmark or GA verification.

## Recommendation

Accept the older implementations as behaviourally correct within the recorded
checks. Playbook was consulted in all four with conditions, and each adopted the
relevant newer capability. That is useful bounded evidence of discovery and
applicability, not evidence that consultation caused correctness or that the
baseline needed Playbook. Do not require four wins or infer broad model knowledge.

## Evidence by pair

| Pair | Consultation with Playbook | Correctness, without / with | Applicability and newer capability | Code assessment |
| --- | --- | --- | --- | --- |
| [C# outcomes](csharp-outcomes/results.md) | One list and three get calls, including repeated JSON detail | Both pass 13 common assertions and reject unrelated values/external fourth cases | Both preserve net11.0/C# 15; with uses RC3 unions, without uses a readonly struct and Match | Union removes manual representation/dispatch. Custom JSON validation remains necessary because two cases share a property shape; no structural JSON classifier adoption |
| [Reservation](reservation-validation/results.md) | One list and one get | Both pass 15 HTTP assertions, including cancellation and no booking after canceled checks | Same RC3 pins/assets; with uses AsyncValidationAttribute/AddValidation, without awaits a model method from a filter | Older implementation needs manual error collection/mapping and an adapter; that necessary code is not a correctness defect |
| [Static grids](grid-links/results.md) | One list and one get | Frozen oracle: without 15 passes/one failure; with 12 passes/two failures/two dependent skips. Supplementary JavaScript-disabled browser checks pass both | Same RC3 QuickGrid pins; with uses independent QueryParameterNameOptions/PaginationState and Paginator, without manual tables/query links | Manual parsing/sorting/paging is necessary for the older approach. Static rendering still requires explicit heading links in the with submission |
| [Manifest](manifest-reconciliation/results.md) | One list and one get | Both pass all 10 frozen assertions, including duplicate rejection and missing sides | Same net11.0/C# 15/RC3 pins; with uses Enumerable.FullJoin, without dictionaries/Union/OrderBy/TryGetValue | Duplicate validation remains necessary with FullJoin. The older dictionaries serve validation and lookup; neither adds unrelated dependencies |

Without conditions expose no Playbook server/calls. All with conditions retrieve
edition 11.0 for target 11. Retained results link actual configs, calls, submitted
sources, compiler output, oracle receipts and browser evidence.

The C# compiler probes have an important limit: omitting Rejected in the union
switch gives CS8509, but omitting absence compiles and throws at runtime. Production
switches handle absence. The older Match requires handlers (CS7036); arbitrary
switches do not acquire an exhaustiveness guarantee.

## Time and exposed usage

Each cell below is without / with. Cached input is included in input; reasoning
output is an exposed submetric, not an additional disjoint billing quantity.

| Pair | Seconds | Input tokens | Cached input | Output tokens | Reasoning output |
| --- | --- | --- | --- | --- | --- |
| C# | 203.457256 / 235.8594103 | 251376 / 781217 | 222976 / 745984 | 8017 / 7846 | 1051 / 902 |
| Reservation | 139.4623859 / 198.5460576 | 236624 / 650590 | 200576 / 589312 | 4187 / 6180 | 517 / 916 |
| Grids | 261.8750946 / 140.0253117 | 435943 / 460164 | 408960 / 427136 | 9144 / 4295 | 1530 / 520 |
| Manifest | 155.7301164 / 157.3195993 | 259989 / 429613 | 228096 / 404224 | 5071 / 5151 | 835 / 710 |

All eight attempts total 1492.2752318 seconds (24.87 minutes), below 4800; each
finishes before 600 seconds. With is faster only for grids and reports more input
tokens in every pair. These single observations include latency, consultation,
failed calls, builds and checks. They establish neither algorithm performance nor
general efficiency. Usage excludes preparation/evaluation and is not a billing audit.

## Failures, corrective work and release implications

- Preserve the grid oracle failures and skips. It selects the wrong sort-column
  anchor for a direction assertion and fails to recognize an omitted default
  first-page key. Supplementary browser evidence supports the intended behaviour;
  it does not turn the frozen oracle receipts into passes. Before reusing this
  oracle in a future comparison, fix those two selection assumptions and calibrate
  against both valid link forms. Do not rerun or replace these eight scored attempts.
- All agents encounter a denied SDK launcher download and recover through the
  pinned local MSBuild host. Retain these failures; no coaching or repair occurred.
- Reservation's SDK relocation was accepted after its with launch, conditional on
  identical assets, before its without launch. Later pairs have prior relocation
  approval. Keep this protocol amendment visible; do not describe the exercise as
  having no deviations. The protocol records the exact identities and approvals.
- Assessment verification found one retention defect: runtime-inventory.txt had LF
  bytes while its existing receipt describes CRLF. Restoring CRLF reproduces the
  exact recorded SHA-256 F85E8FA919D3FFAFB44ACEA9B6F7A8207EAD971432A60654D57B985797DFB48A.
  The receipt and inventory text are unchanged. All four integrity scripts then
  pass. This is a repaired evidence defect, not a newly executed runtime inventory.
- No concrete served-recipe/tool defect is established by these bounded results.
  The oracle and retention defects concern evaluation infrastructure. No product
  corrective blocker is proposed from them; Pingu accepted this assessment.
- Preserve existing GA reconciliation, source/API findings and release gates.
  RC3 behaviour and this recommendation do not clear them or authorize release.

## Verification and owner action

This assessment reads the frozen protocol, four result reports and their retained
receipts. All four Verify.ps1 scripts were executed successfully after the lossless
retention repair. They check retained integrity/contexts/budgets, not current
behaviour. No replay, model run, GA check or new browser exercise was performed;
behaviour findings above reuse the linked unchanged historical evidence.

Pingu approved the recommendation in the implementation conversation, including
the grid failure disposition and reservation amendment. This satisfies the owner
assessment gate; downstream release decisions retain their own prerequisites.

Sources: [frozen protocol](comparison-protocol.md),
[approved evaluation plan](../planning/evaluation-plan.md), SPEC.md stories
152–158 and 160 at commit 0a8989814ea11346354464b845c79c15bb71c5e9,
and ADR 0004's distinction between GA targets and RC evidence.
