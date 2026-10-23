// Execute in the real browser after the client page has rendered.
(async () => {
    const main = document.querySelector('main[data-renderer="client"]');
    if (!main) throw new Error('Real WebAssembly component has not rendered');
    const environment = main.dataset.environment;
    const review = environment.toLowerCase() === 'review';
    const expected = review
        ? ['identity', 'review', 'rehearse', 'whitespace', 'empty-list']
        : ['identity', 'receipts', 'whitespace', 'empty-list'];
    const actual = [...main.querySelectorAll('[data-chrome]')].map(node => node.dataset.chrome);
    if (JSON.stringify(actual) !== JSON.stringify(expected)) throw new Error(`Wrong chrome for ${environment}: ${actual}`);
    if (!main.textContent.includes('Hidden chrome is not authorization.')) throw new Error('Missing authorization boundary');
    const resources = performance.getEntriesByType('resource').map(item => item.name);
    const wasm = resources.find(url => /dotnet\.native\..*\.wasm/.test(url));
    if (!wasm) throw new Error('No actual WASM runtime resource');
    const hash = async url => {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`Asset HTTP ${response.status}: ${url}`);
        const bytes = await response.arrayBuffer();
        const digest = [...new Uint8Array(await crypto.subtle.digest('SHA-256', bytes))]
            .map(value => value.toString(16).padStart(2, '0')).join('');
        return { url: String(url), bytes: bytes.byteLength, sha256: digest };
    };
    return {
        kind: 'real-chrome-wasm-rendering', url: location.href,
        environment, runtime: main.dataset.runtime, actual, expected,
        runtimeAsset: await hash(wasm),
        startupAsset: await hash(new URL('chrome-start.js', location.href)),
        componentAssets: await Promise.all(resources.filter(url => /\/(Client|Microsoft\.AspNetCore\.Components\.Web(Assembly)?)\.[^.]+\.wasm$/.test(url)).map(hash)),
        resources: resources.filter(url => /\/dotnet\.|\/Client\.|\/Microsoft\.AspNetCore\.Components\.Web/.test(url)),
        userAgent: navigator.userAgent,
        scope: 'Real browser rendering and downloaded runtime bytes; no HtmlRenderer, DI stub, mocked host or access-control proof.'
    };
})()
