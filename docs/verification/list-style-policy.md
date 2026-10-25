# List style policy evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-786.md).

## Scope and evidence

Read the full issue, original/current SPEC requirements, proposal WEB_W10C,
web/data inventory, Blazor artifact/disposition audit, contribution/dependency
policies and PIN-772–775 verification patterns. Initial clean branch and freshly
fetched main were both `39bf807c6bb67e645bf70c88e801e524a2c465eb`. The assigned
worktree/branch and same-name remote/upstream were verified before editing.

BeaconReadings is an original fixed-row Interactive Server comparison, distinct
from the list anchoring/reveal work. It does not introduce or test those contracts.
Both samples inherit the isolated samples build/package boundary. They pin SDK
10.0.402 / 11.0.100-rc.2.26504.105, C# 14/15 and SDK-resolved Internal.Assets
10.0.13 / 11.0.0-rc.2.26504.105. Virtualize comes from Microsoft.AspNetCore.App;
there is no added provider/package/workload. Samples stay author-maintained;
root net10.0 consumers, Renovate permissions and CI matrices are unchanged.

Acceptance criteria 1–5 are covered by the original complete comparison,
canonical source snippets, full applicability, actual compilation, source-register
update and existing packed stdio harness. Criteria 6–8 are covered by the newer
host's removed attribute exception, real enforcing headers/security failures,
visible distant IDs and the preserved consumer-browser applicability boundary.
No Human-only proof is outstanding for this bounded local scenario.

The actual reports are [newer strict](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-786.newer.browser.json),
[older explicit exception](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-786.olderLegacy.browser.json) and
[older strict negative control](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-786.olderStrict.browser.json).
[Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-786.execution.evidence.json) record source dirty
provenance, exact input/artifact hashes and the browser test input.
[Pinned official source hashes](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-786.sources.json) are source review, not
execution. The RC2 renderer emits numeric data attributes; its matching JS
validates finite values and uses CSSOM for layout, including base styles.

| Real Chrome 155 route | Distant visible window | Ordinary CSP failures | Explicit controls |
| --- | --- | --- | --- |
| .NET 11 /, strict attributes | 701–710, returns to 1 | zero | inline attribute and stylesheet blocked; CSSOM works |
| .NET 10 /legacy, attribute exception | 701–710, returns to 1 | zero | attribute works; inline stylesheet blocked; CSSOM works |
| .NET 10 /, strict attributes | only 11–20; cannot reach 701 | enforced style-src-attr failures | inline attribute and stylesheet blocked; CSSOM works |

The successful virtual windows have a 32,000px total scroll extent and fewer
than 100 rendered reading nodes. Rows in the visible window contain their actual
IDs and text, measured at 32px height. The failed old route has a 640px extent and
zero computed spacer height despite style attributes containing nonzero heights.
That is browser enforcement evidence, not a guessed failure from source.
The server also appends its separate frame-ancestors policy; the browser reports
retain the combined received header and each violation's original policy.

The test is `eng/BeaconReadings.browser.js`, evaluated via the supported Chrome
extension's developer Runtime.evaluate with awaitPromise/returnByValue. The
native T3 preview opened the app but snapshot twice returned exactly
`Preview automation snapshot timed out after 15000ms.` No framework failure was
inferred from those timeouts. Early browser checks clicked before activation or
used fixed waits that missed a renderer update; final assertions wait for actual
visible rows. A sample rebuild initially failed with MSB3021 because the owned
hosts still locked the DLLs; stopping those hosts allowed the actual builds.
These failed attempts are not passing proof. No failures were suppressed or
headers weakened to obtain the newer result.

## Verification and limits

Compilation, source review, actual local HTTP/browser execution and offline packed
retrieval are separate checks. This is one Windows Chrome/Interactive Server
authoring surface. It does not certify every browser, WebAssembly/Auto, AOT,
deployment/reconnect/script policy, or GA release. The full unrelated sample suite
is not claimed. Consumer browser evidence remains necessary even after using the
source-reviewed migration. The meaningful small JSON reports and lock files stay
reviewable under pragent-ignore; no exclusion or byte-guard change is justified.
