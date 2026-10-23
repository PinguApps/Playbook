// Evaluate on /server through TideDesk.proxy.mjs. Uses the actual framework
// connection and default reconnection handler; only loopback network bytes fail.
(async () => {
    const wait = ms => new Promise(resolve => setTimeout(resolve, ms));
    const until = async predicate => {
        for (let i = 0; i < 200; i++) { if (predicate()) return; await wait(50); }
        throw new Error('Tide desk browser assertion timed out');
    };
    const assert = (condition, message) => { if (!condition) throw new Error(message); };
    const modal = document.querySelector('#tide-reconnect');
    const count = () => document.querySelector('#completed').textContent;
    const click = () => document.querySelector('#check').click();
    assert(document.querySelector('#runtime').textContent === 'Server', 'Requires a real server circuit');
    click();
    await until(() => count() === '1');
    const events = [];
    modal.addEventListener('components-reconnect-state-changed', event => {
        events.push({ at: Date.now(), detail: event.detail, classes: modal.className });
    });
    const started = Date.now();
    await fetch('/__fault/down', { method: 'POST' });
    await until(() => modal.classList.contains('components-reconnect-show'));
    assert(getComputedStyle(modal).display !== 'none', 'Configured dialog must be visible');
    await wait(450);
    await fetch('/__fault/up', { method: 'POST' });
    await until(() => modal.classList.contains('components-reconnect-hide'));
    assert(count() === '1', 'Recovered circuit must retain state');
    click();
    await until(() => count() === '2');
    const recovery = { elapsed: Date.now() - started, count: count(), events: [...events],
        network: await (await fetch('/__fault/state')).json() };

    events.length = 0;
    const failedAt = Date.now();
    await fetch('/__fault/down', { method: 'POST' });
    await until(() => modal.classList.contains('components-reconnect-failed'));
    const network = await (await fetch('/__fault/state')).json();
    assert(document.querySelector('#components-reconnect-max-retries').textContent === '3', 'Configured max retries');
    assert(document.querySelector('#components-reconnect-current-attempt').textContent === '3', 'Three displayed attempts');
    assert(network.attempts.length === 3, `Expected 3 actual failed negotiations, got ${network.attempts.length}`);
    const intervals = network.attempts.slice(1).map((attempt, i) => attempt.at - network.attempts[i].at);
    assert(intervals.every(ms => ms >= 900 && ms < 2000), `1000 ms policy interval: ${intervals}`);
    assert(count() === '2', 'Disconnected view must retain its last rendered count');
    await fetch('/__fault/up', { method: 'POST' });
    return { url: location.href, browser: navigator.userAgent, recordedAt: new Date().toISOString(),
        kind: 'actual-browser-execution', recovery,
        exhausted: { elapsed: Date.now() - failedAt, count: count(), events, network, intervals,
            dialogClasses: modal.className, dialogVisible: getComputedStyle(modal).display !== 'none' } };
})()
