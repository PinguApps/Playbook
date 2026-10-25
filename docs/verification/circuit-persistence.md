# Circuit persistence evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-787.md).

## Actual evidence and limits

- Both Record-Evidence.ps1 -Kind compile captures pass with zero warnings/errors:
  SDK 10.0.402/C# 14 and 11.0.100-rc.2.26504.105/C# 15. Actual NuGet restore and
  compilation use AutoPause RC2; public API source has no experimental annotation.
  [Pinned source hashes](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-787.sources.json) are source review, not execution.
- [RC2 idle browser trace](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-787.browser11-idle.evidence.json): native hidden
  tab -> down/closed -> disposed scoped resource. Returning restores the same
  sheet/readings into a different circuit/resource. A 117 ms return prevents
  pause. A real pending JS call holds the resource past the delay; releasing it
  permits pause. Enabled=false preserves a hidden circuit beyond the delay.
- [Two-session maintenance proof](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-787.browser11-maintenance.evidence.json)
  and [server events](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-787.lifecycle.evidence.json) record two pre-canceled
  false requests, two accepted true requests, later down/closed/disposal and
  completed drain before host exit. Real StoppingAsync is entered by awaiting
  WebApplication.StopAsync. After actual process restart, the two separate
  one/two-reading worksheets keep their own sheet IDs and acquire new circuits
  and scoped resources. The browser document and local Data Protection keys
  remain; no external cache or service is used.
- [The refreshed .NET 10 execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-787.browser10.evidence.json) proves its
  manual timer/bridge, disposal and same-document resume across process restart.
  Its custom true result acknowledges scheduling, unlike the framework API.
- The negative exploratory StopApplication trace is retained separately inside
  the event report. It returned true acceptance but raced transport loss and
  expired the drain budget. It is not final sample proof. The successful route
  does not establish graceful capture for OS signals, forced kills or all hosts.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-787.execution.evidence.json) bind real runs to
  exact source/input/artifact hashes. RC2 ran at clean sample commit 9c1abea;
  the older marker correction was recaptured at that commit with honest dirty
  authoring paths. Those original records are not relabeled as final-head runs.
  Integration reuse requires exact unchanged input/artifact matches.
- Browser operations used existing Chrome through its extension. T3 preview
  opened but two snapshots returned `Preview automation snapshot timed out after
  15000ms.` Chrome native Page.bringToFront with focus emulation disabled
  produced hidden/visible states; no property override or synthetic event.
  Temporary task tabs are closed. All local sample processes are stopped.

No mocks, cross-compilation, DOM-only substitution, additional OS/client/native
matrix or external resource/account provisioning is used. This is local desktop
Interactive Server RC2 evidence. Mobile suspension, distributed/multi-instance
providers/key sharing, other rendering modes, screen readers and GA are not
execution claims. Persistence saves selected serializable values, not scoped
services or arbitrary UI state; full refresh/retention failure may lose a draft.
No required Human-only proof remains for the authored routes.
