// Evaluate on the running sample's origin in the collaborative browser.
// Navigation is real: each click waits for the actual URL and rendered page.
(async () => {
    const base = new URL(document.baseURI).pathname;
    const root = `${base}collections/kelp/`;
    const wait = async (predicate, description) => {
        for (let attempt = 0; attempt < 200; attempt++) {
            if (predicate()) return;
            await new Promise(resolve => setTimeout(resolve, 50));
        }
        throw new Error(`Timed out: ${description}`);
    };
    const assert = (condition, message) => { if (!condition) throw new Error(message); };
    await wait(() => document.querySelector('#ready')?.textContent.includes('True'), 'interactive circuit');
    assert(location.pathname === root + 'overview', 'Start on the overview route');
    assert(location.hash === '#previous' && location.search === '?sort=date', 'Start with query and existing fragment');
    assert(document.querySelectorAll('base').length === 1, 'Exactly one base element');
    assert(document.querySelector('#sibling').href === location.origin + root + 'notes', 'Sibling resolves from current directory');
    assert(document.querySelector('#application-home').href === location.origin + root + 'overview', 'Default link resolves from application base');
    const fragment = document.querySelector('#section-link').href;
    assert(fragment === location.origin + root + 'overview?sort=date#packing', 'Replace existing fragment; retain base, path and query');
    assert(document.querySelector('#prefixed-section-link').href === fragment, 'Optional hash prefix gives the same link');
    assert(document.querySelector('#clear-fragment').href === location.origin + root + 'overview?sort=date', 'Null removes fragment only');
    document.querySelector('#section-link').click();
    await wait(() => location.hash === '#packing' && Math.abs(document.querySelector('#packing').getBoundingClientRect().top) < 5, 'fragment followed and scrolled');
    const sectionTop = document.querySelector('#packing').getBoundingClientRect().top;
    document.querySelector('#clear-fragment').click();
    await wait(() => location.hash === '', 'clear fragment');
    document.querySelector('#sibling').click();
    await wait(() => location.pathname === root + 'notes' && document.querySelector('h1')?.textContent.includes('/ notes'), 'declarative sibling route');
    document.querySelector('#imperative').click();
    await wait(() => location.pathname === root + 'checklist' && document.querySelector('h1')?.textContent.includes('/ checklist'), 'imperative sibling route');
    document.querySelector('#application-home').click();
    await wait(() => location.pathname === root + 'overview' && document.querySelector('h1')?.textContent.includes('/ overview'), 'application-base home');
    return { kind: 'execution', recordedAt: new Date().toISOString(), browser: navigator.userAgent, base,
        fragment, sectionTop, finalUri: location.href,
        passed: ['base markup', 'current-directory link', 'application-base link', 'existing fragment replacement',
            'query preservation', 'hash prefix', 'null removal', 'real fragment scroll', 'declarative route', 'imperative route', 'home route'] };
})()
