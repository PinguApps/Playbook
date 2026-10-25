# Browser service lifetime evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-791.md).

## Actual execution and limits

- Both Record-Evidence.ps1 -Kind compile captures use their actual SDKs,
  locked packages and language modes: SDK 10.0.402/C# 14 and
  11.0.100-rc.2.26504.105/C# 15. Both pass with zero warnings/errors.
  These records honestly retain their original dirty authoring provenance.
- Both samples were rebuilt at clean commit 33747e3cab30329dad40a0b6662701d3881122b8
  before final browser navigation. [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-791.execution.evidence.json)
  retain exact input hashes and all executed framework/webcil/application
  artifact hashes. Inputs were normalized before capture. Post-execution hashes
  matched; reuse after later changes requires exact input/artifact equality.
- [RC2 browser trace](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-791.browser11.evidence.json): automatic single start,
  ticks 133 to 138 while the counter reaches 1; stop-requested, cancelled,
  stopped and root-disposed in order; ticks remain 138 for 1515 ms after awaited
  application disposal in the still-loaded document.
- [.NET 10 browser trace](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-791.browser10.evidence.json): manual single start,
  ticks 164 to 169 while the counter reaches 1; the same real cancellation/stop/
  disposal order; ticks remain 169 for 1509 ms.
- [.NET 10 registration-only control](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-791.browser10-unowned.evidence.json):
  ticks 0 and no lifecycle events for 1602 ms while the counter reaches 1.
  This distinguishes mere DI registration from actual service startup.
- [Fresh-host repeat](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-791.fresh-host.evidence.json) reopens both applications
  after their static manifests are finalized. All lifecycle assertions pass again:
  .NET 11 ticks 239 to 243, then holds for 1503 ms; .NET 10 ticks 241 to 246,
  then holds for 1508 ms. This eliminates the exploratory host's stale development
  manifest warning. Sample inputs/artifacts remain exactly equal to the original
  clean fingerprint; the later run honestly records documentation/test-only dirt.
- The reusable assertions are [BeaconWatch.browser.js](../../eng/BeaconWatch.browser.js).
  They invoke the actual rendered controls, await the real managed shutdown and
  assert loaded native WASM resources, ordering and no post-stop work. Chrome
  155 through Pingu's existing extension ran both applications on 5811/5812.
  T3 preview twice returned `Preview automation snapshot timed out after 15000ms.`
  The initial beacon.js request did not load in this browser (zero transfer);
  renaming the fixture to watch-lifetime.js fixed boot without changing browser
  preferences. Exploratory boot failures are not successful lifetime evidence.

There are no .NET-host mocks, synthetic service replacements or additional
OS/client/native matrices. Root disposal leaves the last DOM visible; the
recorded managed Dispose callback, not disappearance of markup, proves disposal.
This does not prove RunAsync completion, runtime exit, tab-close cleanup,
background-tab timing, mobile suspension, AOT, hosted/Auto composition or GA.
The sample has no persistence/provider/account requirement and makes no durable
work claim. No required Human-only proof remains for this local route.
