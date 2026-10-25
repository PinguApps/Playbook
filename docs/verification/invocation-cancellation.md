# Invocation cancellation evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-814.md).

## Actual behavior and artifacts

The SDKs are **10.0.402** / **11.0.100-rc.2.26504.105**, languages C# **14.0** /
**15.0**, runtimes **10.0.13** / **11.0.0-rc.2.26504.105**, and client packages
**10.0.13** / **11.0.0-rc.2.26504.105**. The new consumer resolves actual
`lib/net11.0` SignalR.Client, Client.Core and Http.Connections.Client assets.
Each independent sample has empty local build/package boundaries and a committed
lock file. These are manually maintained examples; root update permissions,
host/test targets and release policies remain unchanged.

The two `verification-evidence.json` files record actual matching-SDK compilation,
input hashes, resolved dependencies, source commit/dirty marker, host and output.
They do not relabel compilation as execution. [Execution](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-814/execution.json)
records input hashes, DLL hashes and real .NET clients calling ephemeral Kestrel
servers over WebSockets from an unrelated temporary directory:

| Client | Server | After client cancellation | After connection stop |
| --- | --- | --- | --- |
| 10 | 10 | Caller stops waiting; server remains running | Server observes connection cancellation |
| 11 | 10 | Caller stops waiting; server remains running | Server observes connection cancellation |
| 10 | 11 | Caller stops waiting; server remains running | Server observes connection cancellation |
| 11 | 11 | Server observes invocation cancellation; Echo still works | Not needed for cancellation |

The matching new pair also completes an uncanceled invocation on the same
connection. The old hub forwards `Context.ConnectionAborted`; the new hub
forwards SignalR's supplied invocation parameter into its actual cancellable
wait. Client cancellation is passed through `InvokeAsync`, not serialized as an
ordinary argument. A same-connection Echo is an ordering barrier in the older
controls; HTTP observations distinguish caller cancellation from server state.

An [isolated failure control](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-814/negative.json) replaces the new hub's
invocation token with `Context.ConnectionAborted`. It compiles, cancels the
client Task, and then fails the independent server-completion observation with
HTTP 500 after its bounded wait. The canonical checkout is preserved. This
establishes that the behavior check rejects the central wrong-token defect.
The held calculation and HTTP controls are local instrumentation, not mocked
SignalR decisions, a CPU benchmark or a production cancellation service.

Initial compilation exposed URI-overload/async-cancellation analyzer findings;
those were corrected. Evidence validation caught CRLF normalization and a missing
authoring-script copy in the evidence fixture; both were fixed and affected proof
was recaptured. An initial full Windows suite passed the ordinary-package cases,
including this complete body, but failed five synthetic contract cases because
the separate fixture package had not been supplied. That setup failure is not
counted as successful full-suite evidence.
