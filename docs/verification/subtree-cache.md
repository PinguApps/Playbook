# Subtree cache evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-780.md).

## Acceptance coverage and evidence

- Criteria 1–2: original offline prose, canonical snippets and complete runtime,
  SDK, language, package/asset, provider, platform, rendering and stability metadata.
  .NET 10 caches public data and renders each user's markup freshly. .NET 11 caches
  rendered output with explicit identity/query/permission/revision dimensions.
- Criteria 3 and 6–8: both actual projects compile. Real cookie-authenticated
  loopback HTTP checks bind input and DLL/deps/runtime hashes to executed results:
  [older](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-780/older.evidence.json), [memory](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-780/memory.evidence.json),
  [HybridCache](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-780/hybrid.evidence.json), [disabled](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-780/disabled.evidence.json).
  There are 47 checks: complete hits/skipped lifecycle; user/query/anonymous and
  changed-permission isolation; multi-user revision invalidation; eight concurrent
  same-key requests and concurrent distinct users; absolute expiry in both
  providers; memory sliding/absolute bounds; HybridCache sliding refusal; bypass.
  Sliding observations verify accesses stayed inside the idle lifetime while
  absolute expiry won. A Development-only unsafe fixture reproduces the omitted
  user-key leak with fictional identities; the safe page never reproduces it.
- Criterion 4: extend the existing packed offline scenario to reconstruct the
  entire body independently from canonical embedded inputs and check ID/title/goal,
  full applicability, current evidence, edition 11.0, matching structured/text
  results and .NET 10 rejection. Existing maximum-page guards remain unchanged.
- Criterion 5: web inventory G10 and Blazor cache disposition updated; regenerate
  the actual artifact/source audit. [Pinned source review](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-780.sources.json)
  is distinct from compilation and execution.

No mock renderer or cross-compilation substitutes for SSR execution. Only Windows
x64 default-memory and HybridCache-local providers run. Distributed/multi-node,
other variation dimensions, non-GET/streaming restrictions and built-in live
annotations are source-supported constraints, not additional execution claims.
Interactive/browser behavior, other OS, trimming/AOT and GA are unverified.
No required Human-only proof remains outstanding.
