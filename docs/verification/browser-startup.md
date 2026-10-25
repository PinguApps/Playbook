# Browser startup evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.md).

## Actual behavior and evidence

- Four saved verification-evidence.json records are **compile** evidence with
  exact inputs, commands, SDK/language, resolved dependencies and source state.
  They are not cross-compilation, source-only or browser execution claims.
- [HTTP report](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.http.evidence.json): real /server, /wasm and /auto
  responses emit retries=3, interval=1000 ms, dialog=tide-reconnect,
  disableDomPreservation=false, inactivity=120000 ms and environmentName=TideDesk.
  This proves component non-null overrides and retained endpoint fields.
- [Previous](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.browser10.evidence.json) and
  [current](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.browser11.evidence.json) Server runs close the actual WebSocket
  through a loopback TCP/HTTP proxy. Restoring reachability before the retry
  recovers counter 1 and permits the next click to reach 2. Sustained failure
  causes exactly three actual refused negotiations, displayed attempts 1/2/3,
  a visible failed dialog and measured intervals near one second (exact timings
  remain in the reports, including scheduler delays).
- The same actual fault check passes for Auto while it is executing on Server:
  [older](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.auto10.evidence.json), [newer](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.auto11.evidence.json).
  No reconnect callback or SignalR connection is mocked.
- The Auto reconnect reports above assert Server on a fresh origin. The
  [mode report](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.modes.evidence.json) records both versions running
  WebAssembly on a subsequent Auto visit.
  Direct WebAssembly routes execute actual interactive clicks as well. These
  are startup/runtime observations, not a claim of migrating a running instance.
- The independent, deliberately request-varying RC2 probe emits **7, 1, 1**
  across three separate HTTP requests. The third supplies no override.
  Repeated static lookup and endpoint metadata have reference identity.
  A first static-only diagnostic emitted no browser configuration; the retained
  successful probe uses a real Blazor Web App interactive marker. The canonical
  sample uses uniform, non-secret application options. No framework-wide
  security leakage or final GA behavior is inferred.
- [Returned-body consumer](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.consumer.evidence.json): Inspector's actual
  expanded Markdown supplies 14 files. Both old/new host/client pairs build under
  an unrelated disposable directory, without repository build inheritance.
  The resulting RC2 host executes the same generated-options HTTP assertions.
- [Execution fingerprints](browser-startup.execution.json) bind managed binaries,
  runtime/dependency/static-asset files, actual browser assets, source inputs and
  probe scripts. Informational versions omit the Git revision and these isolated
  comparison projects disable SDK Source Link/source-control queries so
  documentation changes do not alter sample assemblies. The first clean-head
  comparison detected changed DLL/PDB/browser assets despite the informational
  setting alone; that earlier browser proof was not reused for changed artifacts.
  Real behavior and compilation evidence were refreshed after the correction.
  Exact equality is required before
  reusing browser proof after a clean current-head rebuild. The browser capture's
  original provenance is commit `54f02c212a8633e1af19e35afe4887d23b7f8d13` with
  `sourceDirty: true`; it is not a browser run from the PR head. The manifest's
  separate `cleanRebuild` record names clean commit
  `da2223db7df6a195a4c36c4db6f665e511ba2cc6`, the actual build commands and
  successful complete input/artifact/probe hash comparison. This establishes
  reuse of the earlier execution, not new browser execution at that commit.
- [Source reconciliation](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-788.sources.json): actual RC2 Endpoints runtime/ref
  artifacts and Internal.Assets identify VMR commit
  820230c386db5b0106c5430b3e5aee92a7826bf5. Its immutable source manifest maps
  ASP.NET Core to 4a70870818bf5662f21b0c28f9a8c267dccb77f0; BrowserOptions content
  hashes match between those roots. Source analysis remains separate from runtime
  observations. API inspection found no experimental diagnostic for these APIs.

One preview_snapshot attempt timed out with
`Preview automation snapshot timed out after 15000ms.`
DOM evaluation and actual network/interactive execution succeeded. No pixel,
screen-reader, other-browser/OS, AOT, deployment or GA proof is claimed. Timers
are scheduling delays; retained-circuit recovery does not prove process-restart
recovery.
