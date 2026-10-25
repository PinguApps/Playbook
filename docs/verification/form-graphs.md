# Form graphs evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-772-773.md).

## Evidence

- Actual .NET 10/11 compilation: zero warnings/errors, exact SDKs 10.0.402 and
  11.0.100-rc.2.26504.105, language 14/15. Saved verification-evidence.json files
  record inputs, dependencies, host, SDK output and commands as **compile**.
- Actual HTTP execution: [10 previous-release posts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-772-773.http10.evidence.json),
  [11 newer-release posts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-772-773.http11.evidence.json), and
  [11 global-opt-out posts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-772-773.global.evidence.json). Each checks valid
  acceptance, root/nested/collection rejection, skipped reference data, the
  intentionally undiscovered root and real localized resource loading.
- Actual Chromium 152 execution: [older](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-772-773.browser10.evidence.json) and
  [newer](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-772-773.browser11.evidence.json) reports. Invalid client checks cause
  no POST; valid enhanced/plain posts succeed; only enhanced posts preserve a
  window sentinel. Bypassed checks still reject nested/collection errors.
  label.control, repeated IDs, precedence/fallbacks and French captions pass.
  The rerunnable expression is eng/SurveyForms.browser.js, evaluated on each
  running app's origin in the collaborative browser. DOM inspection is used;
  preview_snapshot timed out, so pixel/screen-reader inspection is not claimed.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-772-773.execution.evidence.json) tie reports to
  exact app artifacts and source inputs. [Pinned official source hashes](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-772-773.sources.json)
  are source-review evidence, separate from execution.
- One existing installed-tool harness scenario checks all four complete bodies
  against independently embedded canonical sources (including Razor/JS/resources),
  discovery goals, target/edition, applicability, current evidence, matching
  structured/text results and rejection for target 10.0. Installation uses an
  isolated feed/cache and unrelated working directory. No new client/OS matrix.
