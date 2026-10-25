# Browser workers evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-792.md).

## Real behavior and provenance

Three actual locked SDK builds pass with zero warnings/errors: .NET 10
FossilDesk on SDK 10.0.402/C#14, and .NET 11 FossilDesk plus independently built
FossilWorker on SDK 11.0.100-rc.2.26504.105/C#15. The sample
verification-evidence.json records are **compilation only**.

[Exact execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-792.execution.evidence.json) separately retain
all source/config/verifier inputs, executed Release binaries/WASM/JS/manifests,
actual browser messages, native worker lifecycle and timestamps. Each evaluated
verifier string is byte-identical to [FossilDesk.browser.js](../../eng/FossilDesk.browser.js);
its source and evaluated-expression SHA-256 identities are recorded independently.
The real script was read from an owned loopback capture endpoint and evaluated
through Pingu's existing Chrome extension. No mock Worker, client, runtime or
synthetic response replaces actual browser execution.

The final foreground checks prove:

- A rendered counter changes and animation frames advance while the actual
  1200 ms worker CPU method is still running, on both releases.
- The generated InvokeVoidAsync sets worker-local shelf state. InvokeAsync
  returns calculation output, then a real export reports completed work.
- Invocation timeout and managed cancellation end the wait while the worker
  finishes. Report messages are sent BEFORE the corresponding Score responses;
  completion counts advance from one to two and three. These are not assertions
  based solely on a changed UI label.
- Creation timeout terminates its native worker. For deterministic in-flight
  creation cancellation, observation holds the REAL worker's init message,
  cancels the generated managed await, then releases it. That same worker boots
  real .NET successfully and remains live until the test explicitly terminates
  the intentionally orphaned instance. This controlled timing is disclosed and
  is not labelled unmodified startup timing evidence.
- Owner DisposeAsync terminates during another pending CPU call. No result from
  that call arrives and no further messages appear for at least 1400 ms. Every
  diagnostic/native worker is terminated.
- A real regression check forces a second UI render after timeout while Report
  waits for CPU completion. The first implementation re-enabled Score; the fixed
  owner holds its busy guard through observation. The full real-browser verifier
  asserts that guard, without changing the generated transport.

T3 preview snapshot failed twice with
`Preview automation snapshot timed out after 15000ms.`; verification therefore
used the existing Chrome 155 extension. An early invocation before Ready and a
background-tab animation-frame assertion failed during exploration; neither is
successful evidence. Final runs use fresh finalized development hosts on reserved
5831/5832 and foreground tabs. Input line endings were normalized and compilation
and browser proof refreshed before final fingerprints. Records honestly identify
the actual source state at capture; a later build does not retroactively become
the original execution. SDK11/worker compilation records retain dirty 8959bb6
provenance. SDK10 compilation was refreshed after the final indentation-only project cleanup at dirty 3df9c5e. The final real Chrome execution for BOTH canonical apps also ran at dirty 3df9c5e and was committed in 2df59b0; its exact input/runtime fingerprints were rechecked unchanged during recovery. A separate clean rebuild at ab4ca5f changed copied evidence JSON and
its static asset endpoint manifests, so the final complete browser checks and
all artifact fingerprints were refreshed against those outputs. See the
[rebuild reconciliation](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-792.rebuild.evidence.json). This is new real browser
execution, without an artifact-equality exemption or relabelled earlier run.
Reuse requires exact input and artifact equality or fresh execution.

No GA, AOT, mobile/background suspension, SSR/Interactive Server/Auto composition,
deployment, benchmark/speedup or durable-after-tab-close claim is made. DevServer
is a local host only; RC2 emits its deprecation warning recommending future
Gateway migration. That separate hosting work is outside PIN-792. RC2's concurrent
timeout cleanup race is disclosed; calls are serialized rather than patching
framework code. No required Human-only or resource-access proof remains for this
bounded local route.
