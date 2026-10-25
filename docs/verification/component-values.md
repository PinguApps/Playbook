# Component values evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-779.md).

## Executed evidence

- SDK **10.0.402**, C# **14.0**, runtime/browser assets **10.0.13**.
- SDK **11.0.100-rc.2.26504.105**, C# **15.0**, runtime/browser assets
  **11.0.0-rc.2.26504.105**. No preview language switch or experimental suppression.
- `samples/net{10,11}.0/SeedLabels/verification-evidence.json` retain exact
  effective inputs, SDK information, locked dependencies and successful compilation.
  `Record-Evidence.ps1 -Kind compile` deliberately does not label web-host startup
  as a completed application behavior check.
- [Older HTTP](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-779-net10-http.evidence.json),
  [newer HTTP](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-779-net11-http.evidence.json),
  [older browser](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-779-net10-browser.evidence.json) and
  [newer browser](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-779-net11-browser.evidence.json) reports retain exact
  sample input and built DLL/deps/runtimeconfig hashes. Browser checks used the
  existing Chrome extension: both panels displayed `Restored: True`, then both
  buttons displayed their exact echoed choice. The fragment probe displayed
  `Interop rejected RenderFragment`. HTTP checks independently observed a real
  PersistAsJson rejection and HTTP 500 for union-wrapped fragment boundary transfer.
- T3 preview navigation opened the app but snapshot failed twice with
  `Preview automation snapshot timed out after 15000ms.` The integrated-browser
  fallback was unavailable; the existing Chrome extension completed the checks.
- `Test-Content.ps1` and `Test-Evidence.ps1` passed valid/offline schema and
  rejection/freshness checks. Dependency policy passed; real Renovate validation
  passed **324** extracted dependencies and **281** protected sample entries.
  Samples inherit the isolated samples package/build boundary and remain manual;
  root production/test consumers remain net10.0 with unchanged package permissions.
- Integrated main `5a93d7172110b80efba19965cb47869423502c88`, preserving NavigationAtlas/ArchiveCards. The complete actual packed suite passed **36/36**, zero failures/skips. The maximum 50-item fixture result is **169,949 / 170,000 bytes**, with thresholds unchanged. Repeated PIN-779 discovery wording is concise; complete details remain in prerequisites, constraints and Markdown. [Exact package/test artifact hashes and measurements](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-779.package.evidence.json) identify ordinary nupkg SHA-256 `08e555c4a40b81228f28dba0e8c938227e117762452e79da3bf2b755d7a9d026`.
- Package-run provenance: the 36/36 run was captured at `f9af66811ef931b40504df904d247c6321b89694` with uncommitted final recipe-metadata edits (`sourceDirty: true`). Those edits and the evidence/handoff were subsequently committed as `189aaa5ea3593ff9bae03b0f079f90e1085c742e`. The saved hashes identify the actual tested nupkg, test assembly and TRX; this is **not a clean-checkout rebuild of the reviewed head commit**. The package record has no complete build-input manifest, so its commit/dirty fields alone cannot establish exact head reproducibility. The separately hashed sample-input records and canonical body/applicability assertions retain their stated scope; they do not close that package provenance gap.
- `git diff --check` passed. Process/user/machine PATH compared unchanged after
  setup/tests; every .NET child inherited `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`.

Compilation, HTTP and browser execution are separate evidence. No mock renderer,
mock JS runtime or cross-compilation is used as execution proof. Windows x64
Static SSR/Interactive Server is the executed scope. WebAssembly/Auto, other OSes,
trimming/AOT, arbitrary JSON classifier permutations and .NET 11 GA are unverified.
General serializer restrictions are source review; no new OS/client/native matrix
was added. Small readable generated evidence and lockfiles remain reviewable;
no PR Agent exclusion rule is added.
