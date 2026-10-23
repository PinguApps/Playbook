(async () => {
    const wait = async predicate => {
        const deadline = performance.now() + 45000;
        while (performance.now() < deadline) {
            if (predicate()) return;
            await new Promise(resolve => setTimeout(resolve, 50));
        }
        throw new Error('Parcel startup condition timed out');
    };
    const assert = (value, message) => { if (!value) throw new Error(message); };
    const legacyWasm = document.body.dataset.startup === 'wasm';
    const newer = location.port === '5391' || location.port === '5393';
    const cases = legacyWasm ? ['/'] : ['/legacy-server', '/web-server', '/web-wasm'];
    if (newer) cases.push(...(legacyWasm ? ['/?shape=flat'] : ['/legacy-server?shape=flat']));
    const results = [];
    for (const route of cases) {
        const frame = document.createElement('iframe');
        frame.src = route;
        document.body.append(frame);
        try {
            await wait(() => frame.contentDocument?.querySelector('#dispatch') && frame.contentWindow.parcelProof);
            const doc = frame.contentDocument;
            const proof = frame.contentWindow.parcelProof;
            const server = !legacyWasm && !route.startsWith('/web-wasm');
            await wait(() => server ? proof.timeout === 45000 : proof.resources.length > 0 && doc.querySelector('#execution')?.textContent === 'browser');
            // Prerender keeps the button disabled until the first interactive render.
            await wait(() => !doc.querySelector('#dispatch').disabled);
            const button = doc.querySelector('#dispatch');
            let clicks = 0;
            let delayedSends = 0;
            let pendingSends = 0;
            button.addEventListener('click', () => clicks++);
            const socket = frame.contentWindow.WebSocket.prototype;
            const send = socket.send;
            if (server) {
                // Hold real outgoing event traffic beyond the old 50ms retry interval.
                socket.send = function (payload) {
                    delayedSends++;
                    pendingSends++;
                    frame.contentWindow.setTimeout(() => {
                        try { send.call(this, payload); }
                        finally { pendingSends--; }
                    }, 200);
                };
            }
            try {
                button.click();
                await wait(() => doc.querySelector('#count')?.textContent === '1');
                assert(clicks === 1, `Repeated dispatch click on ${route}`);
                if (server) assert(delayedSends > 0, 'Expected actual delayed WebSocket traffic');
            } finally {
                socket.send = send;
                await wait(() => pendingSends === 0);
            }
            assert(!proof.error, proof.error);
            assert(server ? proof.circuit === 1 : proof.circuit === 0, `Wrong callback for ${route}`);
            const culture = doc.querySelector('#culture').textContent;
            if (!server) assert(culture === 'fr-FR', `WASM culture ignored on ${route}: ${culture}`);
            results.push({ route, proof: structuredClone(proof), clicks, delayedSends, count: doc.querySelector('#count').textContent, culture, execution: doc.querySelector('#execution').textContent });
        } finally { frame.remove(); }
    }
    return { kind: 'real-browser', recordedAt: new Date().toISOString(), origin: location.origin, userAgent: navigator.userAgent, results };
})()
