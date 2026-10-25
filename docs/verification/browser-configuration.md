# Browser configuration evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-793.md).

## Real execution and provenance

- Both Record-Evidence.ps1 -Kind compile records use the actual pinned SDKs:
  10.0.402/C# 14/runtime 10.0.13 and
  11.0.100-rc.2.26504.105/C# 15/matching RC2 runtime. They record compilation
  only, with zero warnings/errors, and retain original authoring provenance.
  Lock-file bytes were normalized before the final capture.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-793.execution.evidence.json) record sample-clean
  inputs at commit 34166b2a4f2b57313158715cbe20c7a5bb1d8e6c, application DLLs,
  and all 213 older/217 newer uncompressed served publish assets. Builds were
  serialized; no changes occurred during the browser matrix. Exact post-run
  input/artifact equality was checked. Compilation records and browser execution
  are distinct; later reuse requires equality or fresh real execution.
- A [separate clean rebuild](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-793.clean-rebuild.evidence.json) at
  002247b218896c3d88a9d27a4fce5f162a21dfd1 republishes both actual SDK targets.
  All 24 inputs per sample, both application DLLs and all 430 uncompressed served
  assets exactly match the browser capture. Explicit project settings disable
  SourceLink/revision stamping; equality is measured rather than assumed.
  The original dirty compile captures remain honestly labelled as such.
- [Actual .NET 10 Chrome runs](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-793.browser10.evidence.json): Testing and
  Production select different environments/public endpoints and JSON fallback
  values, with one environment provider after two JSON providers and before
  the memory override. CreateDefault's initial value is environment JSON.
  skipProvider=1 leaves zero environment providers and the JSON endpoint even
  though native WASM loads and the managed counter works. duplicate=1 adds a
  second provider after memory and changes the caption back to public settings.
- [Actual RC2 Chrome runs](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-793.browser11.evidence.json): both profiles have
  the public endpoint immediately after CreateDefault and exactly one default
  environment provider. The later memory caption wins. duplicate=1 reproduces
  the migration regression: two environment providers, public caption last.
- All seven pages advance the real managed counter to 1 and load native WASM.
  Within each release, browser-fetched application WASM, native WASM and Blazor
  script SHA-256 values match across both profiles and controls and agree with
  the publish inventory. The two releases intentionally have different builds.
  No desktop configuration-host mock or cross-compilation substitutes for this.
- [EstuaryBoard.browser.js](../../eng/EstuaryBoard.browser.js) is the exact
  assertion source. Each receipt separately records its source SHA and the full
  evaluated expression SHA: source + newline + verifyEstuaryBoard with a JSON
  object ordered major, profile, skip, duplicate. It checks actual rendered
  IConfiguration/provider order/values and hashes loaded resources.

Chrome 155 through Pingu's existing extension executed ports 5851/5852. The T3
preview opened, but twice returned `Preview automation snapshot timed out after
15000ms.` before the switch. No fresh Chrome profile or sibling host was used.
No required Human-only proof remains for this local route. Runtime mutation,
reload providers, AOT, mobile/browser matrices and production hosting are not
established by these checks.

The [standalone reconstruction receipt](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-793.consumer.evidence.json) uses
[Test-EstuaryBoardConsumer.ps1](../../eng/Test-EstuaryBoardConsumer.ps1) to write
every application file and SDK pin from the actual Inspector response's expanded
Markdown into an unrelated temporary directory. It restores/publishes both actual
SDK targets without checkout imports. [Four additional real browser runs](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-793.consumer-browser.evidence.json)
verify both profiles in both reconstructed consumers. Their application bytes
differ from checkout builds because snippet markers, source paths and debug
metadata differ; each
consumer's bytes remain identical across its profiles. Those receipts are never
substituted for the canonical seven-run artifact proof. The helper and response
hashes, full reconstructed input hashes, actual dependency closure, DLL hashes
and untrimmed publish output are retained. No generated consumer files were
copied from the checkout.
