# Browser culture evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-794.md).

## Actual browser and compilation proof

Actual SDKs: 10.0.402 / runtime and ASP.NET 10.0.13; SDK
11.0.100-rc.2.26504.105 / matching RC2 runtime and browser assets, C# 14/15.
Client Localization is 10.0.13 in both, with the exact differing resolved graph
in each packages.lock.json and verification record. Both Host and Client project
boundaries isolate root build/package policy; sample dependencies remain manual.

The four [compilation records](../../samples/net11.0/CultureHarbour/Host/verification-evidence.json)
are `compile`, not browser execution. Each was captured using
`eng/Record-Evidence.ps1 -Kind compile` with its matching absolute SDK from
the project directory, locked packages and DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0.

Real Chrome 155 through Pingu's existing extension ran the unchanged canonical
[verifier](../../eng/CultureHarbour.browser.js). T3 preview snapshots twice failed
with `Preview automation snapshot timed out after 15000ms.`; actual Chrome
extension execution supplied the retained records. The verifier text was passed
without transformation to CDP Runtime.evaluate with awaitPromise/returnByValue.
Each report retains actual native WASM, ICU/client/satellite/script hashes,
SSR values, startup/render events, satellite response timing and one button click.

| Actual run | Initial runtime | SSR and hydrated culture/UI | Satellite end / first render (ms) |
| --- | --- | --- | --- |
| .NET 10 aligned | en-US/en-US | en-GB/fr-FR → en-GB/fr-FR | 1508.400 / 1992.700 |
| .NET 11 aligned | en-US/en-US | en-GB/fr-FR → en-GB/fr-FR | 1928.600 / 2360.700 |
| .NET 10 independent | en-US/en-US | en-GB/fr-FR → de-DE/de-DE | 1408.600 / 1851.000 |
| .NET 11 independent | en-US/en-US | en-GB/fr-FR → de-DE/de-DE | 1744.100 / 2162.100 |

All SSR responses contain 19/07/2032, 12,345.67 and
Gardez la caisse bleue debout. Aligned hydration preserves them. Independent
hydration intentionally shows 19.07.2032, 12.345,67 and
Die blaue Kiste aufrecht halten. Both actual resource satellites load after
Program starts and before first render; Confirm manifest increments to 1.
Combined ICU is a real loaded artifact, separate from translated satellites.
The first interactive render also aligns the HTML document language with the
chosen UI culture: fr-FR aligned, de-DE independent. Before the fix, a real
independent run showed Interactive WASM and de-DE UI with document language
fr-FR. All four canonical cases and both reconstructed-client cases were
re-executed after correcting that mismatch; the verifier now checks it.

.NET 11's decoded actual Blazor-Configuration comment contains environmentName
and environmentVariables, without applicationCulture in these samples. en-US
comes from explicit webAssembly JS startup, en-GB/fr-FR from persisted state.
The report does not infer an endpoint ApplicationCulture setting.

Retained [older aligned](../../samples/net10.0/CultureHarbour/Host/browser-aligned.evidence.json),
[older independent](../../samples/net10.0/CultureHarbour/Host/browser-independent.evidence.json),
[newer aligned](../../samples/net11.0/CultureHarbour/Host/browser-aligned.evidence.json)
and [newer independent](../../samples/net11.0/CultureHarbour/Host/browser-independent.evidence.json)
records retain fresh execution at merge commit 23a961e with the document-language
fix and verifier uncommitted. The records explicitly preserve dirty-capture
provenance. Initial execution and the separate clean-source a0cc3f7 rebuild
remain in the *-initial.evidence.json and *-before-document-language.evidence.json
files; neither is relabelled as current execution. A later
[artifact/input binding](../../samples/net11.0/CultureHarbour/Host/execution-binding.evidence.json)
fingerprints settled inputs and verifies every fetched browser artifact against
actual built bytes. It does not relabel the original dirty captures as later execution.
`eng/Test-CultureHarbourEvidence.ps1` checks exact equality and refuses stale
artifacts/inputs/verifiers/reports. The first clean rebuild changed SourceLink/PDB,
client and satellite artifacts, requiring actual re-execution. Integration and
the later language fix likewise received fresh compilation and all four actual
browser runs. No source-only, cross-compiled or mocked result replaces execution.
