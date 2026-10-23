// Evaluate this complete expression on a freshly loaded real BrackenMenu page.
(async () => {
    const assert = (condition, message) => { if (!condition) throw new Error(message); };
    const wait = async (test, label) => {
        const until = performance.now() + 30000;
        while (!test()) {
            if (performance.now() > until) throw new Error('Timed out: ' + label);
            await new Promise(resolve => setTimeout(resolve, 50));
        }
    };
    const pause = () => new Promise(resolve => setTimeout(resolve, 150));
    const response = await fetch(location.href, { cache: 'no-store' });
    const policy = response.headers.get('Content-Security-Policy');
    assert(policy && !response.headers.has('Content-Security-Policy-Report-Only'), 'Missing enforcing policy');
    const exception = policy.includes("'unsafe-hashes'");
    const module = !!document.querySelector('script[type="module"]');
    const client = location.pathname.includes('/client/');
    assert(!policy.includes("'unsafe-inline'"), 'Broad inline exception');
    assert(exception || policy.includes("script-src-attr 'none'"), 'No strict handler policy');
    if (module) await wait(() => customElements.get('bracken-menu'), 'external module registered');
    await wait(() => document.querySelector('#confirm'), 'confirmation rendered');
    await wait(() => Number(document.querySelector('#confirmations')?.textContent) > 0 ||
        (document.querySelector('#confirm').click(), false), 'actual application initialized');
    if (client) assert(document.querySelector('#renderer').textContent.includes('True'), 'WASM did not initialize in browser');
    const menu = () => document.querySelector('bracken-menu');
    const toggle = () => menu().querySelector('input');
    const open = () => { if (!toggle().checked) document.querySelector('label[for="menu-open"]').click(); };
    open();
    assert(getComputedStyle(menu().querySelector('.nav-scrollable')).display === 'block', 'Menu did not open');
    document.querySelector('#outside').click();
    assert(toggle().checked, 'Outside click collapsed menu');
    menu().querySelector('.nav-scrollable').click();
    const collapsed = !toggle().checked;
    assert(collapsed === (module || exception), 'Menu collapse disagrees with enforced policy');
    await pause();
    const menuViolations = [...window.brackenPolicy.violations];
    if (!module && !exception) assert(menuViolations.some(v => v.effectiveDirective === 'script-src-attr' && v.disposition === 'enforce'), 'Old strict control lacks enforced violation');
    else assert(menuViolations.length === 0, 'Working menu violated CSP');
    const token = window.brackenPolicy.pageToken;
    const enhancedBefore = window.brackenPolicy.enhanced;
    open();
    document.querySelector('#reed-link span').click();
    const immediatelyClosed = !toggle().checked;
    await wait(() => document.querySelector('#trail')?.textContent === 'Bracken reed trail', 'actual Reed link navigation');
    assert(location.pathname.endsWith('/reed'), 'Link URL did not change');
    assert(window.brackenPolicy.pageToken === token, 'Unexpected full document replacement');
    if (!client) assert(window.brackenPolicy.enhanced > enhancedBefore, 'Static route did not use enhanced navigation');
    assert(immediatelyClosed === (module || exception), 'Nested link collapse failed');
    open();
    document.querySelector('#fern-link').click();
    await wait(() => document.querySelector('#trail')?.textContent === 'Bracken fern trail', 'return link navigation');
    if (module) {
        // An enhanced update may replace the custom element. Exercise the current one.
        const element = menu();
        const parent = element.parentNode;
        const next = element.nextSibling;
        element.remove();
        element.querySelector('input').checked = true;
        element.querySelector('.nav-scrollable').click();
        assert(element.querySelector('input').checked, 'Disconnected listener still active');
        parent.insertBefore(element, next);
        element.querySelector('.nav-scrollable').click();
        assert(!element.querySelector('input').checked, 'Reconnected listener absent');
        element.querySelector('.nav-scrollable').click();
        assert(!element.querySelector('input').checked, 'Closed menu reopened');
    }
    const script = document.querySelector('script[type="module"]');
    let moduleResponse = null;
    if (script) {
        const asset = await fetch(script.src);
        moduleResponse = { url: script.src, status: asset.status, body: await asset.text() };
        assert(asset.ok && moduleResponse.body.includes("customElements.define(tag, BrackenMenu)"), 'Actual module asset missing');
        assert(new URL(script.src).pathname.startsWith('/bracken/'), 'Lost path-base prefix');
        assert(new URL(script.src).pathname.includes(client ? '/Layout/NavMenu' : '/Components/Layout/NavMenu'), 'Wrong layout module path');
    }
    const ordinaryViolations = [...window.brackenPolicy.violations];
    if (module) assert(ordinaryViolations.length === 0, 'External navigation violated CSP');
    const control = document.createElement('button');
    control.setAttribute('onclick', 'window.brackenInlineControl = true');
    document.body.append(control);
    control.click();
    await pause();
    const controlViolations = window.brackenPolicy.violations.slice(ordinaryViolations.length);
    assert(!window.brackenInlineControl && controlViolations.some(v => v.effectiveDirective === 'script-src-attr' && v.blockedURI === 'inline' && v.disposition === 'enforce'), 'Different inline handler was not blocked');
    control.remove();
    return { status: 'PASS', url: location.href, userAgent: navigator.userAgent, policy, module, client, exception,
        collapsed, immediatelyClosed, initialized: true, enhancedUpdates: window.brackenPolicy.enhanced,
        pageTokenPreserved: window.brackenPolicy.pageToken === token, moduleResponse, menuViolations, ordinaryViolations, controlViolations };
})()
