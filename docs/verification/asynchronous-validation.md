# Asynchronous validation evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-774-775.md).

## Executed behavior and proof boundaries

- Both actual projects compile with zero warnings/errors: SDK 10.0.402/C# 14.0
  and 11.0.100-rc.2.26504.105/C# 15.0. Their saved evidence kind is **compile**.
- [Previous HTTP proof](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-774-775.http10.evidence.json) and
  [current HTTP proof](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-774-775.http11.evidence.json) hold real valid/invalid
  HTTP rules. Before release: entered=1, completed=0, accepted=0, request unfinished.
  Afterwards: 204/completed=1/accepted=1 or 400/completed=1/accepted=0.
  Actual static forms accept open passes and reject closed ones.
- [Four Chromium 154 runs](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-774-775.browser.evidence.json) execute both actual
  Interactive Server apps under English/French culture cookies. Each superseded
  HTTP field operation records canceled=1/completed=0/accepted=0. Forced submit
  reruns the rule and records exactly one acceptance only after release.
  Invalid group sizes are rejected with the same translated custom message.
- Resource tests prove member-over-type/global precedence and execution of the
  custom formatter. Other convention fallbacks are pinned-source evidence, not
  separate execution claims. Request culture alone did not set a new circuit's
  language; the sample now sets the standard cookie before reloading.
- The browser check reproduced the initial incorrect pending indicator. The fixed
  sample uses both the form-wide and field-specific pending/fault APIs. It
  disables edits during submission while allowing a field check to be superseded.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-774-775.execution.evidence.json) bind reports to
  exact source/configuration inputs, main/satellite assemblies, deps and runtime
  configuration hashes. No mock or cross-compilation substitutes for execution.
- Pixel inspection showed legible inputs and French errors without clipping.
  Screen-reader, WebAssembly/Auto, trimming/AOT and GA execution are not claimed.
  All render modes' architecture is explained with the live-field/static-POST
  distinction; no additional OS/client/native test matrix is introduced.
