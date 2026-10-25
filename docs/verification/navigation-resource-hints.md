# Navigation resource hints evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-800.md).

## Actual verification

- Actual canonical compile: SDK 10.0.402/C# 14/runtime 10.0.13 and SDK 11.0.100-rc.2.26504.105/C# 15/matching runtime. The two verification-evidence.json files retain original sourceCommit/dirty provenance and exact input hashes. SourceLink/SCM embedding was disabled before accepted capture.
- Actual Chrome 155 on Windows: static SSR enhanced fetches carry `Accept: text/html; blazor-enhanced-nav=on`, preserve document identity, increment enhancedload, and receive authored preload/as=fetch and modulepreload resources before consumption. Modules evaluate only on actual import. The control has no hints or resource arrivals before its button, then produces the same “7 dispatches ready for Tern Quay” result. Warm data reuse has zero transfer in this run. The opt-out produces an ordinary document request and a new identity.
- Canonical browser receipts retain original dirty c905a5f438dcc75d0744b68be3c500047c990f6d provenance. The same-branch merge of b67e41d5ffefa0ecfceb15160ee4f03e81134f94 is 8e1fdadac21db6b136393763e77a308997bae3ec. Exact relevant canonical input and every executed artifact hash remained equal after integration; no base-only browser rerun or clean-head relabelling is claimed.
- Independent apps were reconstructed solely from that returned Markdown: 12 complete code blocks, both inline SDK pins and three inline imports; no canonical application/configuration copy. Both actual SDK builds passed with zero warnings/errors. Both actual browser/network sequences passed again on these independent consumers. Served blazor.web.js bytes equal each runtime manifest's actual selected asset; JSON/JS MIME and public,max-age=600 cache headers were checked. Every executed consumer artifact remained unchanged.
- Normal Test-Content and Test-Evidence passed. Their changed-input packed negative baselines passed again against the settled combined v4 catalogue; no catalogue/build race. TestDependencyPolicy passed. Actual Renovate 44.172.1 strict config/extraction passed, including 510 extracted dependencies, 467 protected entries and synthetic policy cases. No dependency-update permission or root baseline was broadened.

## Receipts and limits

[Canonical browser](navigation-resource-hints.browser.json) and [network](navigation-resource-hints.network.json) captures, and their [consumer browser](navigation-resource-hints.consumer-browser.json) / [network](navigation-resource-hints.consumer-network.json) counterparts are the retained inputs to `eng/Test-TernLedgerReceipts.ps1`. Detailed original inputs and evaluated scripts remain available at the immutable original-capture link above. Routine package, Inspector, cleanup and raw transport receipts are not needed by that check.

Raw canonical logs also retain historical failed observers (a self-matching diagnostic URL and stale evaluator binding) and a Production build-output setup lacking Development static assets. They are not accepted proof. A first negative-pack invocation used a PowerShell function wrapper that dropped -p arguments; the actual absolute-executable alias rerun passed. A guessed framework-asset path was corrected using the actual runtime manifest before the accepted served-byte comparison. No failures were reclassified as successful execution.

This establishes selected Chrome/Windows/static-SSR behavior, not GA, WebAssembly boot preloading, interactive SPA routing, another OS/browser, universal request counts or performance gains. Compilation, source review, actual browser execution and packed offline retrieval are separate evidence. No mocks/cross-compilation replace execution. No workloads, accounts or resources were provisioned; workloads:[] means none required, not none installed. No Human-only proof is required for this local scope.
