// Run this exact expression through the existing browser extension's CDP Runtime.evaluate.
async function verifyEstuaryBoard(expected) {
    const read = id => document.getElementById(id)?.textContent.trim();
    const assert = (condition, message) => { if (!condition) throw new Error(message); };
    const endpoint = `https://${expected.profile.toLowerCase()}.estuary.invalid/dispatch`;
    const count = expected.major === 10 && expected.skip ? 0 : expected.duplicate ? 2 : 1;
    assert(read('environment') === expected.profile, 'Wrong managed environment');
    assert(read('initial') === (expected.major === 11 ? endpoint : `${expected.profile} JSON endpoint`), 'Wrong CreateDefault precedence');
    assert(read('endpoint') === (expected.skip ? `${expected.profile} JSON endpoint` : endpoint), 'Wrong effective endpoint');
    assert(read('caption') === (expected.duplicate ? `${expected.profile} public caption` : 'Operator override'), 'Wrong final-provider precedence');
    assert(read('json') === `${expected.profile} JSON` && read('base') === 'Base JSON', 'JSON fallback missing');
    assert(read('count') === String(count), 'Wrong environment provider count');
    assert(read('clicks') === '1', 'Managed interaction failed');
    const providers = [...document.querySelectorAll('#providers li')].map(element => element.textContent.trim());
    const names = providers.map(value => value.split('|')[0].trim());
    const wanted = ['JsonStreamConfigurationProvider', 'JsonStreamConfigurationProvider'];
    if (!expected.skip) wanted.push('EnvironmentVariablesConfigurationProvider');
    wanted.push('MemoryConfigurationProvider');
    if (expected.duplicate) wanted.push('EnvironmentVariablesConfigurationProvider');
    assert(JSON.stringify(names) === JSON.stringify(wanted), 'Wrong provider ordering');
    const resources = performance.getEntriesByType('resource').filter(entry => entry.name.includes('/_framework/'));
    const native = resources.find(entry => /dotnet\.native.*\.wasm/.test(entry.name));
    const application = resources.find(entry => /EstuaryBoard.*\.wasm/.test(entry.name));
    assert(native && application && native.decodedBodySize > 0, 'Real native WASM/application assets not loaded');
    const hashes = {};
    for (const url of [native.name, application.name, new URL('_framework/blazor.webassembly.js', document.baseURI).href]) {
        const response = await fetch(url, { cache: 'no-store' });
        assert(response.ok, `Asset unavailable: ${url}`);
        const bytes = await response.arrayBuffer();
        hashes[new URL(url).pathname] = [...new Uint8Array(await crypto.subtle.digest('SHA-256', bytes))].map(value => value.toString(16).padStart(2, '0')).join('');
    }
    return { expected, href: location.href, userAgent: navigator.userAgent, environment: read('environment'), initial: read('initial'), endpoint: read('endpoint'), caption: read('caption'), json: read('json'), base: read('base'), count: read('count'), clicks: read('clicks'), providers, hashes, nativeBytes: native.decodedBodySize, recordedAt: new Date().toISOString() };
}
