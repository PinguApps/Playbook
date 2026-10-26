# Grid positioning evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-783.md).

## Actual execution and limits

- SDK 10.0.402/C# 14.0/net10.0 and SDK 11.0.100-rc.2.26504.105/C# 15.0/net11.0
  compile the real Web SDK projects with zero warnings/errors. Locked QuickGrid
  and SDK Internal.Assets are 10.0.13 and 11.0.0-rc.2.26504.105 respectively.
  Sample MSBuild/package inheritance stays isolated under samples; existing
  manual Renovate policy applies. Root host/test dependencies remain net10.
- [Older browser report](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-783.browser10.evidence.json) and
  [RC2 browser report](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-783.browser11.evidence.json) are actual Chrome 155
  Interactive Server execution on Windows x64. They assert initial row 401,
  awaited reveal of 701, fresh-object reload, same identity/offset after five
  prepends, no jump on append, and Home/start pinning with newly prepended -9.
  All data rows measure 32px; rendered windows are bounded rather than all 1000
  records. Row 701 remains at y=1px (older) / y=0px (newer); prepend changes
  scrollTop by exactly 160px. These are real DOM measurements and row contents,
  not an offset calculation standing in for rendering.
- The rerunnable expression is eng/HarbourManifest.browser.js. Browser clicks
  invoke real Blazor handlers; the Home gesture is dispatched to the actual
  browser scroll container and its scroll offset is changed to zero. Actual
  Chrome keyboard navigation was also exercised during diagnosis. No mocked
  QuickGrid, renderer or JS interop is used.
- T3 preview snapshot failed twice with `Preview automation snapshot timed out
  after 15000ms.` A standalone IntersectionObserver also never fired there.
  Failed preview moves did not load distant windows and are not attributed to
  the app. The integrated browser was unavailable; existing Chrome extension
  access supplied working observer/rendering proof. A combined Start|End
  exploratory sequence is not a passed canonical mode test. Final proof uses
  explicit Start. No unsupported suppression or framework workaround is shipped.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-783.execution.evidence.json) bind the final browser
  reports to exact source/configuration inputs, compilation records, assemblies,
  deps and runtimeconfig artifacts. Compilation records honestly retain dirty
  authoring provenance. A later package build is not a clean reviewed-head
  reproduction of those samples.
- No remote provider/database, other operating system, assistive technology,
  WebAssembly/Auto, other anchor mode or GA execution is claimed. No required
  Human-only proof remains for this sample's stated behavior.
