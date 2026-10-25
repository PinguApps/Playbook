# List item reveal evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-785.md).

## Actual evidence

- Both `Record-Evidence.ps1 -Kind compile` captures passed with zero warnings
  and errors: SDK 10.0.402/C# 14/net10.0 and
  11.0.100-rc.2.26504.105/C# 15/net11.0. Exact sources, lock files, SDK assets,
  framework/language declarations and test-fixture inputs are hashed in each
  sample's saved evidence. These are compilation records, not browser proof.
- [Older browser execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-785.browser10.evidence.json) and
  [newer browser execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-785.browser11.evidence.json) each passed thirteen
  assertions on real Windows Chrome 155 Interactive Server circuits. The
  canonical `eng/LanternQueue.browser.js` bytes ran unchanged through the
  existing Chrome extension's supported CDP capability. A bounded temporary
  loopback listener supplied the fixture and saved the returned reports.
- The provider was held before initial loading and again before revealing 150.
  The jump button stayed disabled until the initial provider returned its count.
  Targets were absent while held; the later operation had not reported
  completion. After release, actual rows 70/150 had the expected text and full
  viewport visibility. Valid endpoints 0/199, invalid -1/200 rejection without
  new provider calls, initial -20/900 clamping and continued virtualization
  passed. The .NET 10 path rendered 15 rows; measured .NET 11 windows were 7–28.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-785.execution.evidence.json) bind those reports
  to exact sample inputs and DLL/deps/runtimeconfig/static-asset artifacts.
  Final captures honestly retain source commit 2f97d99 and dirty authoring paths;
  they are not relabeled as clean reviewed-head rebuilds. Subsequent authoring
  changes outside those inputs do not invalidate unchanged evidence.
- [Pinned official source hashes](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-785.sources.json) establish that both APIs
  exist in RC2 and neither exists in .NET 10. Neither selected source has an
  experimental annotation. Actual warnings-as-errors builds use no suppression.
  Source review is separate from compilation and browser execution.
- Native T3 preview opened, but two snapshots returned exactly
  `Preview automation snapshot timed out after 15000ms.` The permitted existing
  Chrome extension fallback ran the successful proof. A screenshot showed
  readable controls, borders and visible ticket 70 without clipping. Fixture
  frames were removed after checks; no persistent browser modification remains.

No mocks, cross-compilation, database/provider service, accounts or Human-only
proof are substituted. These claims cover local Interactive Server only, not
WebAssembly/Auto, other OS hosts, screen readers, variable-height policy or GA.
Real provider delay, cancellation journals and circuit/DOM behavior are exercised.
The full unrelated sample suite and unchanged .NET 11 tool-host roll-forward lane
are not claimed.
