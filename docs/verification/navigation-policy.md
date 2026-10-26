# Navigation policy evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-798.md).

## Actual browser proof

[Final canonical receipts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-798.canonical.browser.json) contain eight passing
real Chrome 155 checks with their exact evaluated expression.
[Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-798.execution.evidence.json) record all 64 canonical
inputs and 914 output artifacts, raw verifier and exact expression/payload hashes.
The source commit/dirty state is retained as captured, independently of the earlier
dirty compilation records. A later clean commit is not relabelled execution proof.

| Consumer and policy | Collapse | Actual navigation | Enforcement |
| --- | --- | --- | --- |
| .NET 11 static shell, strict | works | Fern/Reed, two enhanced updates | zero ordinary failures; separate inline control blocked |
| .NET 11 global WASM, strict | works | browser=True, live confirmation, Fern/Reed | same; Layout module 200 |
| .NET 10 static/global, matching hash | works | actual content and URL changes | only the exact handler allowed; different handler blocked |
| .NET 10 static/global, strict negative | blocked | links still navigate | enforced script-src-attr failure |
| .NET 10 static/global, external backport | works | actual content and URL changes | zero ordinary failures; separate inline control blocked |

Static modules resolve through `Components/Layout/NavMenu.<fingerprint>.razor.js`;
global client modules use `Layout/NavMenu.<fingerprint>.razor.js`. All actual
requests retain `/bracken/` and return the expected module with HTTP 200. The tests
also cover outside clicks, nested link clicks, closed-menu retention and listener
removal/reconnection. Header inspection, app initialization, actual expected content
and securitypolicyviolation disposition=enforce establish behavior beyond DOM or
click counts. WebAssembly confirms OperatingSystem.IsBrowser after activation;
prerender alone is not counted as client execution.

[Historical pre-cleanup receipts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-798.preformat.browser.json) preserve an
initial initialization-timing failure and earlier superseded checks. They are not
current artifact proof. Initial T3 preview snapshot twice returned exactly
`Preview automation snapshot timed out after 15000ms.` The supported existing
Chrome extension supplied the real browser checks. A first path-base attempt
returned 405 for circuit negotiation; explicit UseRouting after UsePathBase fixed
it. A reused client route initially retained its old heading; explicit route
parameters fixed the actual navigation. Final proof was rerun after relevant
sample changes, including byte normalization and removal of the newer host's old
exception configuration. No CSP was broadened to obtain a passing result.
