# Obsolete diagnostic ownership — PIN-689

Reviewed **25 October 2026**. This closes the finite index-to-owner reconciliation,
not the remaining recipes. Every published entry has one bounded accountable
owner; there is no catch-all obsolete-API recipe or newly authored replacement.
Keep the stable inventory identity `migration.obsolete-api-index`.

## Exact source boundary

[The source manifest](obsolete-index.sources.json) pins **72 complete official
text files**, with immutable URLs and SHA-256 hashes:

- dotnet/docs **`5e19883f3aca93e649d9e3fb72b0b89091d7079f`**: the .NET 11
  [core obsoletion table][core] (0064, 0066), [cryptography table][crypto] (0065),
  [global obsolete overview][global] (**SYSLIB0001–0066, EXTOBS0001–0002**), and
  all 68 linked diagnostic articles. Overlapping references are deduplicated by ID.
- dotnet/runtime **`f0c7a08c8dea85b0271b5ed128530169b9ad79ff`**:
  [the complete diagnostic allocation registry][registry]. Its 66 obsolete IDs
  match the overview. Its other **179** allocated rows have separate family
  ownership: 167 generator allocations, three suppressors and nine experimental
  allocations. These counts include reserved/retired slots, not 179 features.

These are the published custom-obsolete indexes, not a claim that the .NET 11
breaking-change index or every framework's diagnostics are complete. Default
CS0618/CS0619 obsoletions and framework-specific removals remain in the existing
actual-asset family audits. PIN-682 retains compiler compatibility; PIN-687
retains deployment; PIN-707 retains SDK/build/tooling; PIN-691/PIN-708/PIN-709
retain Blazor/web removals; PIN-693/PIN-694 retain native UI/workload changes.
No API-presence or no-change conclusion is inferred for an uninspected framework.

## Exhaustive map and meaning

[The ownership map](obsolete-index.ownership.json) is the complete entry ledger:
one row per diagnostic with its short API subject, accountable issue, disposition
and local supporting evidence. Join `entries[].id` to `sources[].id` for the
exact official article. `owners` records the verified PinguApps team/Playbook
project IDs, issue URLs and bounded family. `otherDiagnostics` explicitly lists
every non-obsolete registry ID without treating reserved slots as implemented APIs.

| Outcome | Entries | Meaning |
| --- | --- | --- |
| Historical | 64: SYSLIB0001–0062, EXTOBS0001–0002 | Existing pre-.NET 11 diagnostic mechanisms, retained as context; no new standalone release recipe or ninth .NET 10 foundation is inferred. |
| Grouped; delivery outstanding | SYSLIB0063 | Existing pipe delivery owner, not an additional proposal. |
| Grouped; proposal outstanding | SYSLIB0064, SYSLIB0065 | Existing cryptography migration audit has two narrow estimated proposals. Audit Done is not recipe delivery. |
| Delivered | SYSLIB0066 | Existing custom async-startup guidance and recorded managed checks; limits remain explicit. |

Historical describes the diagnostic's introduction, not permission to ignore a
newly annotated member in .NET 11. The family owner still evaluates actual
declaration deltas, changed severity and compatibility behavior. For example,
new formatter-support overrides under historical SYSLIB0051 remain restrictions
of the new exception shape in PIN-703, while PIN-704 owns the formatter mechanism.
Cross-family consumers retain their existing ownership; one primary index owner
does not transfer their work or establish complete execution evidence.

### New .NET 11 entries and remaining delivery

| Diagnostic and exact affected boundary | Accountable owner and existing evidence | Remaining obligation |
| --- | --- | --- |
| [SYSLIB0063][0063]: `NamedPipeClientStream(PipeDirection, bool, bool, SafePipeHandle)` | [PIN-741](https://linear.app/pinguapps/issue/PIN-741), established by [PIN-703](data-io.md#obsolete-index-ownership) and [PIN-684](archive-path-handle-clock.md). | The existing ticket must deliver the three-argument constructor migration, real connection/handle ownership checks and installed offline guidance. No duplicate ticket. |
| [SYSLIB0064][0064]: Boolean-padding `RSACryptoServiceProvider.Encrypt` and `Decrypt` | [PIN-686](https://linear.app/pinguapps/issue/PIN-686), [proposal F](cryptography-platforms.md#f-make-legacy-rsa-encryption-padding-explicit--m-3), **M (3)**. | Approval and actual Windows provider/interoperability evidence plus recipe delivery remain outstanding. Explicit padding must preserve the agreed protocol/provider; the audit rejects an unsupported OAEP-SHA256 substitution. |
| [SYSLIB0065][0065]: `AsnEncodedData.RawData` **setter**, not getter | [PIN-686](https://linear.app/pinguapps/issue/PIN-686), [proposal E](cryptography-platforms.md#e-update-decoded-asn1-extension-state-through-supported-apis--s-2), **S (2)**. | Approval, typed constructor/CopyFrom state checks and recipe delivery remain outstanding. Getter-only use is not a newly obsolete goal. |
| [SYSLIB0066][0066]: `IStartupValidator` | [PIN-746](https://linear.app/pinguapps/issue/PIN-746), [served source](../../../content/recipes/options.validate-asynchronously/11.0/recipe.md), [recorded evidence](../../verification/PIN-745-747.md). | Already delivered: custom `IAsyncStartupValidator` registration and awaited startup. Existing custom-factory/legacy-precedence limitations remain source-only. No fresh consumer execution or new suppression fixture is claimed by this audit. |

The .NET 11 core summary omits **0063**. Its dedicated article explicitly names
.NET 11, and PIN-703's actual RC2 reference comparison already establishes it.
The cryptography table supplies **0065** separately; a core-only scan misses it.
The global overview and runtime registry reconcile both omissions without
inventing 0067 or future reserved obsoletions. This is an index discrepancy,
not an unresolved semantic migration delegated back to PIN-689.

### Historical and non-obsolete boundaries

- PIN-703 owns the indexed text/URI/regex/numeric rows; PIN-704 owns historical
  serialization/XML rows; PIN-705 owns remaining runtime/interop/transport/security
  rows. Their existing full artifact manifests and source dispositions remain the
  implementation evidence; this map does not repeat their family analysis.
- PIN-693 owns SYSLIB0016 (`Graphics.GetContextInfo`) and SYSLIB0059
  (`SystemEvents.EventsThreadShutdown`), plus experimental SYSLIB5002. Its native
  audit is still pending; these are historical context, not invented new .NET 11
  desktop features or a claim that the pending desktop audit is complete.
- PIN-706 owns **EXTOBS0001**, documented as .NET 9, and **EXTOBS0002**, documented
  as package **10.1.0**. The latter's three `AddServiceLogEnricher` overloads have
  an exact dedicated reference even though the earlier Extensions report did not
  name that row. Neither is introduced as a .NET 11 diagnostic. Package majors
  are not runtime requirements. Any newly changed release-family behavior still
  belongs to that bounded family owner, not a blanket historical exclusion.
- Generator allocations are assigned to PIN-703 (Regex), PIN-704 (JSON), PIN-705
  (LibraryImport/JS/COM) and PIN-706 (logging/configuration/options). In particular
  **SYSLIB1064** remains PIN-705's family responsibility, with its established
  PIN-715 unsafe-authoring delivery; **SYSLIB1205** has PIN-746's executed
  generated/explicit conflict check. No generator allocation is an obsolete API.
- Experimental allocations retain PIN-703 (5001), PIN-693 (5002), PIN-704 (5005)
  or PIN-705 (5003/5004/5006–5009). Removed experimental annotations, reserved
  generator slots and the three analyzer suppressors do not imply a new recipe.
  Current parameter/platform/experimental restrictions stay with the existing
  family and concrete recipe owners.

## Completion and publication ownership

PIN-689 can close when this map is exhaustive. Family owners must resolve their
factual gaps and propose narrow native-scale delivery slices; no new gap is
assigned to an unbounded index task. PIN-686 already supplies E/F and their
prerequisites; PIN-741 already owns 0063. No additional follow-on is justified by
this enumeration and no proposal is approved or created here.

[PIN-986](https://linear.app/pinguapps/issue/PIN-986) retains review, overlap
reconciliation, Pingu's approval and creation of qualifying delivery tickets with
native publication-blocking links. [PIN-959](https://linear.app/pinguapps/issue/PIN-959)
retains the comprehensive coverage/evidence gate; missing pipe/crypto guidance
still blocks public publication. Existing native PIN-689 → PIN-986/PIN-959
relations remain unchanged. GA owners must refresh the published indexes and
actual artifacts, without relabelling this RC preparation/source evidence as GA.
No related issue, release flag, recipe or completion flag changes in this task.

## Reproduce and inspect

Requires PowerShell 7 and public network access. No SDK, package restore, workload,
account or inspected-code execution is needed. Run from any directory; the block
selects this task's literal worktree first:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-303b1bfe'
& ./eng/Inspect-ObsoleteIndex.ps1
$map = Get-Content -LiteralPath './docs/planning/audits/obsolete-index.ownership.json' -Raw | ConvertFrom-Json
$map.entries | Format-Table id, subject, owner, outcome -AutoSize
$map.otherDiagnostics | Select-Object owner, kind, @{n='Count';e={$_.ids.Count}}
```

Expected: four new-release rows (741/686/686/746), outcome counts **64/1/2/1**,
then `PASS: 72 immutable sources; 68 obsolete entries; 179 non-obsolete registry
entries; no unowned or duplicate IDs.` The following tables expose every mapping.
A failed download/hash, missing index ID, duplicate assignment, unknown owner or
missing evidence file fails the replay. Hashes prove bytes, not the correctness of
the semantic dispositions: review those against the linked family evidence.

The block was verified from an unrelated directory. It changes no served MCP
tool/recipe, so there is no changed discovery/Markdown/applicability response to
demonstrate. Future delivery must use the existing actual installed-MCP harness.
[The verification ledger](../../verification/PIN-689.md) records criterion
coverage, negative controls, repository checks and tracker boundaries.

[core]: https://github.com/dotnet/docs/blob/5e19883f3aca93e649d9e3fb72b0b89091d7079f/docs/core/compatibility/core-libraries/11/obsolete-apis.md
[crypto]: https://github.com/dotnet/docs/blob/5e19883f3aca93e649d9e3fb72b0b89091d7079f/docs/core/compatibility/cryptography/11/obsolete-apis.md
[global]: https://github.com/dotnet/docs/blob/5e19883f3aca93e649d9e3fb72b0b89091d7079f/docs/fundamentals/syslib-diagnostics/obsoletions-overview.md
[registry]: https://github.com/dotnet/runtime/blob/f0c7a08c8dea85b0271b5ed128530169b9ad79ff/docs/project/list-of-diagnostics.md
[0063]: https://github.com/dotnet/docs/blob/5e19883f3aca93e649d9e3fb72b0b89091d7079f/docs/fundamentals/syslib-diagnostics/syslib0063.md
[0064]: https://github.com/dotnet/docs/blob/5e19883f3aca93e649d9e3fb72b0b89091d7079f/docs/fundamentals/syslib-diagnostics/syslib0064.md
[0065]: https://github.com/dotnet/docs/blob/5e19883f3aca93e649d9e3fb72b0b89091d7079f/docs/fundamentals/syslib-diagnostics/syslib0065.md
[0066]: https://github.com/dotnet/docs/blob/5e19883f3aca93e649d9e3fb72b0b89091d7079f/docs/fundamentals/syslib-diagnostics/syslib0066.md
