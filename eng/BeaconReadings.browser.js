// Execute on a freshly loaded BeaconReadings page through the real browser's
// developer Runtime.evaluate (awaitPromise/returnByValue), not a DOM-only shim.
(async () => {
    const wait = async (test, label) => {
        const deadline = performance.now() + 10000;
        while (!test()) {
            if (performance.now() > deadline) throw new Error(`Timed out: ${label}`);
            await new Promise(resolve => setTimeout(resolve, 25));
        }
    };
    const settle = () => new Promise(resolve => setTimeout(resolve, 500));
    const viewport = document.querySelector('#viewport');
    const rows = () => [...viewport.querySelectorAll('[data-reading]')];
    const snapshot = label => {
        const bounds = viewport.getBoundingClientRect();
        const visible = rows().filter(row => {
            const rect = row.getBoundingClientRect();
            return rect.top >= bounds.top + viewport.clientTop - 1 && rect.bottom <= bounds.bottom + 1;
        }).map(row => ({ id: Number(row.dataset.reading), text: row.textContent,
            y: row.getBoundingClientRect().top - bounds.top, height: row.getBoundingClientRect().height }));
        return { label, scrollTop: viewport.scrollTop, scrollHeight: viewport.scrollHeight,
            renderedIds: rows().map(row => Number(row.dataset.reading)), visible,
            spacers: [...viewport.querySelectorAll('[aria-hidden]')].map(el => ({
                reservedHeight: el.getAttribute('data-blazor-virtualize-reserved-height'),
                style: el.getAttribute('style'), computedHeight: getComputedStyle(el).height
            })) };
    };
    const response = await fetch(location.pathname, { cache: 'no-store' });
    const policy = response.headers.get('Content-Security-Policy');
    const prerender = await response.text();
    const strict = policy?.includes("style-src-attr 'none'");
    const newer = prerender.includes('data-blazor-virtualize-reserved-height');
    if (!policy?.includes("style-src 'self'; style-src-elem 'self'; style-src-attr")) throw new Error('Missing enforcing style policy');
    if (response.headers.has('Content-Security-Policy-Report-Only')) throw new Error('Report-only is not enforcement');
    await wait(() => rows().length > 0, 'initial real items');
    document.querySelector('#confirm').click();
    await wait(() => document.querySelector('#confirmations').textContent === '1', 'live circuit');
    await settle();
    const initial = snapshot('initial');
    const beforeScrollViolations = [...window.beaconPolicy.violations];
    viewport.scrollTop = 22400;
    if (newer || !strict) await wait(() => snapshot('waiting').visible.some(row => row.id === 701), 'visible reading 701');
    await settle();
    const distant = snapshot('distant');
    viewport.scrollTop = 0;
    await wait(() => snapshot('waiting').visible.some(row => row.id === 1), 'visible reading 1 after return');
    await settle();
    const returned = snapshot('return');
    const ordinaryViolations = [...window.beaconPolicy.violations];
    const controls = window.beaconPolicy.controls();
    await settle();
    const controlViolations = window.beaconPolicy.violations.slice(ordinaryViolations.length);
    const usable = distant.visible.some(row => row.id === 701 && row.text.includes('Reading 701: Westmere light'));
    const assert = (value, message) => { if (!value) throw new Error(message); };
    assert(initial.visible.some(row => row.id === 1), 'Initial reading missing');
    assert(returned.visible.some(row => row.id === 1), 'Return reading missing');
    assert([initial, distant, returned].every(s => s.renderedIds.length < 100 && s.visible.every(r => r.height === 32)), 'Not a bounded fixed-height virtual window');
    assert(controls.elementWidth === '20px' && controls.cssomWidth === '88px', 'Style-element/CSSOM controls failed');
    assert(controlViolations.some(v => v.effectiveDirective === 'style-src-elem' && v.blockedURI === 'inline' && v.disposition === 'enforce'), 'Inline stylesheet was not blocked');
    if (strict) {
        assert(controls.attributeWidth === '20px', 'Inline attribute was not blocked');
        assert(controlViolations.some(v => v.effectiveDirective === 'style-src-attr' && v.blockedURI === 'inline' && v.disposition === 'enforce'), 'Missing attribute violation');
        if (newer) {
            assert(usable && ordinaryViolations.length === 0, 'Newer strict list failed or violated CSP');
        } else {
            assert(!usable && ordinaryViolations.some(v => v.effectiveDirective === 'style-src-attr'), 'Older strict negative control did not fail as expected');
        }
    } else {
        assert(!newer && policy.includes("style-src-attr 'unsafe-inline'"), 'Unexpected exception route');
        assert(usable && ordinaryViolations.length === 0 && controls.attributeWidth === '77px', 'Older explicit exception route failed');
    }
    return { status: 'PASS', url: location.href, userAgent: navigator.userAgent, policy, newer, strict,
        usable, beforeScrollViolations, ordinaryViolations, controls, controlViolations,
        observations: [initial, distant, returned] };
})()
