# Circuit transport evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-790.md).

| Real Windows / Chrome 155 check | .NET 10 metadata convention | .NET 11 ConfigureConnection |
| --- | --- | --- |
| Actual WebSocket upgrade | 101 | 101 |
| Negotiated transports | WebSockets only | WebSockets only |
| Long Polling attempt | 404, transport unsupported | 404, transport unsupported |
| Incoming component event / outgoing render | exact 27,648-character note | exact 27,648-character note |
| Pipe thresholds | 8,192 / 16,384 bytes | 8,192 / 16,384 bytes |
| Ticket-expiry closure enabled | Disconnected; new connection 401 | Disconnected; new connection 401 |
| Closure disabled, same artifact | existing circuit relays receipt 2; new 401 | existing circuit relays receipt 2; new 401 |

Reports: [older](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-790.net10.browser.json), [newer](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-790.net11.browser.json),
[older control](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-790.net10.control.browser.json),
[newer control](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-790.net11.control.browser.json).
`eng/LockhouseSessions.browser.js` is the bounded actual browser check, executed
through the connected Chrome extension's supported CDP Runtime.evaluate with
awaitPromise/returnByValue. Handshake records retain status/timing only, no cookies
or request headers. The T3 preview opened the app, but snapshot twice returned
`Preview automation snapshot timed out after 15000ms.` These are browser-tool
failures, not framework failures; Chrome supplied the actual execution evidence.

The payload exceeds both pipe thresholds and succeeds, establishing that they are
not message-size caps. No peak-memory, throughput, slow-reader backpressure,
universal safe limit, security guarantee or load-capacity claim is made. Expiry is
periodically observed, not a millisecond deadline. No auth-refresh, other-provider,
proxy, distributed-hosting, WebAssembly/Auto, other-browser or GA proof is claimed.
No Human-only prerequisite remains for the demonstrated local routes.
