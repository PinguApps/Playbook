// snippet:startup
Blazor.start({
    circuit: {
        reconnectionOptions: {
            maxRetries: 3,
            retryIntervalMilliseconds: 1000,
            dialogId: 'tide-reconnect'
        }
    },
    ssr: { disableDomPreservation: false, circuitInactivityTimeoutMs: 120000 },
    webAssembly: { environment: 'TideDesk' }
});
// endsnippet:startup
