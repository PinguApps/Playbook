# Endpoint policies evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-815-816.md).

## Source reconciliation

The .NET 11 release notes describe scalar binding failure filters, but official
`v10.0.0` RequestDelegateFactory source already invokes that path. Actual
.NET 10.0.13 and .NET 11 RC2 executions agree. The recipe preserves this useful
existing technique rather than inventing an older workaround. Its introduction
metadata records the verified .NET 10 availability; its served edition and
supported target remain 11.0.

Malformed JSON and unsupported content types return earlier in the tested
RequestDelegateFactory path. Both actual runtimes return before the endpoint
filter: empty 400 and 415 respectively in the uncustomized control. The recipe
therefore limits its filter claim to observed scalar failures and explains that
broader error contracts require a separate error-writing policy.

ShortCircuitAttribute is the newer declarative form of the retained ShortCircuit()
convention. The canonical proof chooses Minimal APIs. Official metadata also
supports controller/action placement; this report does not claim MVC execution.
The pinned routing implementation checks incompatible security metadata.
[Source receipts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-815-816/sources.json) separate this review from execution.

## Actual consumers and requests

Four isolated projects use SDK **10.0.402** / **11.0.100-rc.2.26504.105**, C#
**14.0** / **15.0**, and actual runtime **10.0.13** /
**11.0.0-rc.2.26504.105**. Their empty Directory files prevent root analyzer/package
inheritance; the repository editor configuration still applies. Each console
project references Microsoft.AspNetCore.App directly, with no PackageReferences,
provider package, workloads or native tools. The exact pins reproduce the
evidence rather than establish minimum language requirements. These examples
remain manually maintained; no automated dependency permission was added.

Each project's `verification-evidence.json` records matching-SDK compilation,
exact input hashes, source commit/dirty marker, target/language, dependencies,
host and actual output. [Execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-815-816/execution.json) separately records
the four rebuilt assembly hashes and real Kestrel observations:

- **ParcelIntake, eight requests per target:** bad route value, bad query value
  and missing query value return `application/problem+json`, status 400 and code
  `intake_binding_failed`, with zero handler calls. Good values execute once.
  A handler's `capacity_closed` domain error retains its own body. Malformed JSON
  and wrong media type bypass their filter. A second Development host with
  ThrowOnBadRequest=true returns 400 without running the shaper or handler.
- **SignalCabin, nine requests per target:** attribute/conventional standalone
  responses return their text, keep the before-routing trace and omit the
  after-routing trace. The ordinary response keeps both. Real middleware returns
  401/403/200 for anonymous/denied/permitted crew fixtures. The ordinary chart
  retains its configured CORS response header. Deliberately invalid short-circuit
  plus authorization/CORS endpoints return InvalidOperationException/500 before
  their handlers execute.

[Negative controls](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-815-816/negative-controls.json) remove the scalar shaper
and standalone attribute from separate RC2 builds. The actual request proof
rejects both variants. Sources were restored, lockfiles normalized to LF, all
four compilation records recaptured and the real requests passed again.
Initial compiler/analyzer errors are not counted as successful evidence.
