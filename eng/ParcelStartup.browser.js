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
            // Retry a click only while prerendered markup has not become interactive.
            await wait(() => {
                if (doc.querySelector('#count')?.textContent === '1') return true;
                if (doc.querySelector('#count')?.textContent === '0') doc.querySelector('#dispatch').click();
                return false;
            });
            assert(!proof.error, proof.error);
            assert(server ? proof.circuit === 1 : proof.circuit === 0, `Wrong callback for ${route}`);
            const culture = doc.querySelector('#culture').textContent;
            if (!server) assert(culture === 'fr-FR', `WASM culture ignored on ${route}: ${culture}`);
            results.push({ route, proof: structuredClone(proof), count: doc.querySelector('#count').textContent, culture, execution: doc.querySelector('#execution').textContent });
        } finally { frame.remove(); }
    }
    return { kind: 'real-browser', recordedAt: new Date().toISOString(), origin: location.origin, userAgent: navigator.userAgent, results };
})()
