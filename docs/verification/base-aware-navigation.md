# Base aware navigation evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-777.md).

## Implementation and exact proof

NavigationAtlas is an original tide-archive example. The previous implementation
uses System.Uri/UriBuilder and request-derived PathBase markup. The newer app
uses the three framework contracts. Ordinary home links deliberately keep the
application base; collection notes/checklist use the current route directory.

Both projects inherit samples/Directory.Build.props and the isolated
samples/Directory.Packages.props boundary. SDKs are 10.0.402/C# 14.0 and
11.0.100-rc.2.26504.105/C# 15.0. Their locked SDK-supplied browser assets are
Internal.Assets 10.0.13 and 11.0.0-rc.2.26504.105. No explicit API package or
workload is added. Root production/test consumers stay net10.0; tests add
canonical embedded files only. Sample pins remain manual under existing Renovate
rules, so no new automatic permission or shared build policy is needed.

- Each sample's verification-evidence.json records actual **compile** evidence,
  exact commands, SDK output, dependencies and per-input hashes.
- [Pinned primary sources](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-777.sources.json) are **source review**, not runtime
  proof. The installed RC2 reference XML independently confirms BasePath's
  Components namespace and the current fragment/relative-navigation signatures.
- [Older HTTP](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-777.http10.evidence.json),
  [newer HTTP](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-777.http11.evidence.json) and
  [alternate prefix](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-777.alternate.evidence.json) execute all three nested
  routes plus the real blazor.web.js asset. Base markup is /atlas/ on both apps
  and /museum/ on the same unchanged newer binary.
- [Three real Chromium 152 runs](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-777.browser.evidence.json) follow declarative,
  imperative and application-base navigation; start from ?sort=date#previous;
  check query/base preservation, optional hash prefix and null removal; then
  follow the real section link and measure its viewport position.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-777.execution.evidence.json) bind those reports to
  exact app/test inputs, main DLL, deps and runtime configuration. No mocks or
  cross-compilation substitute for execution.
- Non-experimental API contracts are verified with RC2, not GA. Actual
  reverse-proxy deployment, WebAssembly/Auto, accessibility focus and delayed
  fragment targets are not executed. Collaborative browser snapshots timed out
  with "Preview automation snapshot timed out after 15000ms."; DOM/click/scroll
  execution succeeded, but pixel/screen-reader inspection is not claimed.
