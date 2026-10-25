# Javascript startup evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-789.md).

## Evidence

- Six independently declared projects compile against SDK 10.0.402/C# 14 and
  SDK 11.0.100-rc.2.26504.105/C# 15. All have locked dependency graphs and
  explicit sample package boundaries. Both LegacyClient apps also publish.
  WasmBuildNative=false selects prebuilt runtime assets without a workload.
- Saved compilation records distinguish compile from execution. They identify
  source commit `89e09cdefbbbca51eed0b0cc3f2fcd06fecc4c3d`, actual dirty flags,
  normalized per-input hashes, commands, dependencies and SDK output.
  The first source milestone was rebuilt before final browser proof because
  its Git-stamped assemblies changed. No prior binary result is relabelled.
- [Previous host](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-789.browser10host.evidence.json),
  [previous legacy WASM](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-789.browser10wasm.evidence.json),
  [current host](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-789.browser11host.evidence.json) and
  [current legacy WASM](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-789.browser11wasm.evidence.json) are real Chromium 152
  runs on Windows. Eight primary paths and two current flat legacy comparisons
  all dispatch a parcel (0 to 1). Server callbacks observe the actual built
  connection's timeout 45000; browser callbacks receive real boot requests
  and the executing component reports browser/fr-FR.
- [Previous HTTP](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-789.http10.evidence.json) and
  [current HTTP](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-789.http11.evidence.json) verify script bytes against actual
  aligned packages, autostart=false, document order and both canonical startup
  copies. `eng/Test-ParcelStartup.ps1` reruns these assertions.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-789.execution.evidence.json) retain actual
  assemblies/runtime configuration and served boot resources: 242 previous
  and 250 current HTTP resource hashes, in addition to local artifact hashes.
  These are execution identities, not compilation claims.
- All six projects were reconstructed in an unrelated temporary directory
  exclusively from expanded served Markdown, its stated imports and pins.
  Both hosts (including their client references) and both standalone clients
  build there with zero warnings/errors. No repository sample/build imports
  or omitted source files supply setup. This reconstruction checks compilation;
  real runtime evidence above uses the canonical worktree artifacts.

Actual .NET 11 proof is RC2, not GA. Auto, Hybrid, other browsers, production
hosting and native/AOT optimization are untested. No required Human-only
step remains for this bounded managed/browser contract.
