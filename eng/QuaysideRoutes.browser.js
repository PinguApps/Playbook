// Evaluate this exact expression with Runtime.evaluate in the real browser tab.
(async () => {
    const wait = async predicate => {
        const deadline = Date.now() + 20000;
        while (Date.now() < deadline) {
            if (predicate()) return;
            await new Promise(resolve => setTimeout(resolve, 50));
        }
        throw new Error('Quayside browser condition timed out');
    };
    const check = (condition, message) => { if (!condition) throw new Error(message); };
    const result = () => document.querySelector('#result')?.textContent ?? '';
    const berth = location.pathname.split('/').at(-1);
    await wait(() => result().startsWith(`Quay ${berth}; cargo=slate; read=`));
    const initial = result();
    document.querySelector('#refresh').click();
    await wait(() => result() !== initial && result().startsWith(`Quay ${berth}; cargo=slate; read=`));
    const refreshed = result();
    const api = await fetch(`/api/quays/${berth}?cargo=granite`);
    const body = await api.text();
    check(api.status === 200 && api.headers.get('X-Quayside-Backend') === 'real-loopback', 'Real backend response/header missing');
    check(body.startsWith(`Quay ${berth}; cargo=granite; read=`), 'Proxy lost path/query');
    const missingApi = await fetch('/api/not-found');
    const missingFile = await fetch('/absent.js');
    check(missingApi.status === 404 && missingFile.status === 404, 'SPA fallback swallowed API/file failure');
    const resources = performance.getEntriesByType('resource').map(entry => entry.name);
    check(resources.some(url => url.includes('/_framework/') && url.includes('.wasm')), 'No real WASM runtime assets');
    check(resources.some(url => url === `${location.origin}/api/quays/${berth}?cargo=slate`), 'Managed client did not use entry origin');
    let configuration = null;
    if (location.port === '5912') {
        check(resources.some(url => url.includes('/_content/QuaysideRoutes.Defaults/') && url.includes('.lib.module.js')), 'Generated initializer missing');
        check(resources.includes(`${location.origin}/_blazor/_configuration`), 'Generated configuration endpoint not requested');
        configuration = await (await fetch('/_blazor/_configuration')).json();
        check(configuration.webAssembly.environment.services__quays__http__0 === location.origin, 'Logical service does not resolve through entry origin');
        check(!resources.some(url => new URL(url).hostname === 'quays'), 'Logical service escaped browser service discovery');
    }
    return { kind: 'real-browser-wasm-and-loopback-http', url: location.href, userAgent: navigator.userAgent, initial, refreshed, api: { status: api.status, body, backend: api.headers.get('X-Quayside-Backend') }, missingApi: missingApi.status, missingFile: missingFile.status, configuration, resources, text: document.body.innerText };
})()
