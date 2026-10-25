# Static child content evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-778.md).

## Decisions and evidence boundaries

- A static Penmor Archive reading card projects two non-generic fragments into a
  prerendered Interactive Server panel. The .NET 10 comparison constructs them
  within an interactive wrapper, passing only a string across the boundary.
- Both actual consumers compile with zero warnings/errors under SDK 10.0.402 /
  C# 14.0 and SDK 11.0.100-rc.2.26504.105 / C# 15.0. Saved evidence is **compile**,
  not execution. Framework browser assets resolve to 10.0.13 / RC2 respectively.
- [HTTP 10](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-778.http10.evidence.json) and
  [HTTP 11](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-778.http11.evidence.json) execute actual ASP.NET Core endpoints.
  The old direct fragment fails. The new generic template, ordinary Action,
  disabled-prerender and unused-fragment probes fail with measured diagnostics.
  Positive heading/body markup and the captured-handler probe render successfully.
  Reports bind exact source/configuration and assembly/deps/runtimeconfig hashes.
- [Chromium 152 execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-778.browser.evidence.json) checks both real circuits:
  panel button updates preserve the heading and body. The new captured parent
  handler is inactive after activation; the panel-owned handler still updates.
  The framework log reports that the captured onclick was skipped. Browser
  checks also read /control/callbacks before/after the click and require zero,
  independently of the captured text. The panel button remains disabled until
  RendererInfo.IsInteractive, and the browser waits for activation before clicking.
  Browser execution uses the same artifacts/inputs recorded by the companion HTTP checks.
- [Pinned RC2 source hashes](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-778.sources.json) support capture lifetime and
  skipped reference/named-event frames. Those latter restrictions are source
  review, not separately executed claims. No arbitrary delegate serialization,
  live parent callback, generic variance change or older-target package route.
- Windows x64 SSR and Interactive Server are executed. WebAssembly/Auto, other
  OS execution, trimming/AOT, pixel/screen-reader checks and GA verification are
  not claimed. preview_snapshot timed out; DOM behavior was checked successfully.
  No required Human-only proof remains for this bounded recipe.

The two Web SDK consumers inherit samples/Directory.Build.props and
samples/Directory.Packages.props, with individual SDK/language/asset locks.
The root production host remains net10.0 and workload-free. The test project adds
canonical embedded inputs only. Sample dependencies remain author-maintained;
no Renovate permission/range changes are needed. Small generated evidence and
lockfiles remain reviewable under the existing .pragentignore policy.
