# Browser telemetry evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-797.md).

## Canonical compilation and browser receipts

[Browser evidence](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-797.browser.evidence.json) records each input and all
**879 executed bin files** (653 older, 226 newer), evaluated properties, actual
boot runtimeConfig, exact verifier source/expression, raw output and payload hashes.
Compilation records remain in each sample. Their actual source commit is
`aa55af65bb83b2e2513b4231e09dda36747af972`, **dirty**, at
11:05:49/11:05:52 UTC on 2026-10-23. Actual browser capture began at
`2026-10-23T12:09:35.2649714+01:00`, at that same dirty source state. These are not
relabeled as execution of a later clean head. SourceLink/SCM revision metadata
was disabled before capture; proof files are outside served wwwroot assets.

Real Chrome through the existing Matthew extension ran both WASM consumers.
T3 preview navigation worked but snapshot twice returned
`Preview automation snapshot timed out after 15000ms.`; the permitted existing
Chrome fallback was used. Each page showed its actual runtime, accepted Schedule
shuttle, reached Scheduled: 1 / Shuttles ready: 1, and retained 1 at /dispatch.
Rendered layout was inspected. The official collector received, for each version:

- Positive navigation/event measurements from Microsoft.AspNetCore.Components.
- Positive parameter-duration, render-diff-duration and render-diff-size
  measurements from Microsoft.AspNetCore.Components.Lifecycle.
- Event identities LoomActivity.Board / ScheduleShuttle / onclick.
- Actual / and /dispatch framework spans; loom.schedule shared the event traceId
  and used its spanId as parentSpanId; the event linked to the received / span.

This establishes receipt beyond subscription or exporter configuration. It is
neither a fake receiver nor mocked telemetry. Original raw payload SHA256 is
`e284d821487818085da6d6b99bf461fefac0028308e81bdf098d7040161efcab`.
[Selected raw lines](browser-telemetry.receipts.jsonl) preserve all trace lines and the last
complete metric line per version byte-for-byte; their hash is
`84b52c9b0d755486955338f79cf5737676ea3d8fb62e2cae434df0106a3f32cc`.
The reviewed verifier passes on both full and selected payloads.

Both coordinator-authorized main integrations retained exact input and executed
artifact equality. The latest comparison at combined head
`6fbd83c3e6fd7dfc97818c101c6bfd635a0bdcca` includes main
`3048fc393385e8dbf1562e9bc1fcbf3c95424fed`. This justifies reuse of the original
canonical browser capture; it does not claim that a later head was rerun.
