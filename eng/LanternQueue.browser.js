// Run this expression through the connected browser on either LanternQueue origin.
// Real Interactive Server circuits, DOM geometry and held ItemsProvider requests.
(async () => {
    const checks = [];
    const run = `reveal-${crypto.randomUUID()}`;
    const control = async (key, action) => {
        const response = await fetch(`/control/${key}/${action}`, { method: 'POST' });
        if (!response.ok) throw new Error(`Control failed: ${response.status}`);
    };
    const journal = async key => (await fetch(`/journal/${key}`)).json();
    const until = async (predicate, label) => {
        const deadline = performance.now() + 15000;
        while (!(await predicate())) {
            if (performance.now() > deadline) throw new Error(`Timed out: ${label}`);
            await new Promise(resolve => setTimeout(resolve, 30));
        }
    };
    const assert = (condition, label) => { if (!condition) throw new Error(label); checks.push(label); };
    const frame = document.createElement('iframe');
    frame.style.cssText = 'width:1100px;height:600px;border:0';
    document.body.prepend(frame);
    const doc = () => frame.contentDocument;
    const row = index => doc()?.querySelector(`[data-index="${index}"]`);
    const geometry = index => {
        const viewport = doc()?.querySelector('#queue'), target = row(index);
        if (!viewport || !target) return null;
        const outer = viewport.getBoundingClientRect(), inner = target.getBoundingClientRect();
        return { index, text: target.textContent, visible: inner.top >= outer.top - 1 && inner.bottom <= outer.bottom + 1,
            rowTop: inner.top, rowBottom: inner.bottom, viewportTop: outer.top, viewportBottom: outer.bottom,
            scrollTop: viewport.scrollTop, rendered: viewport.querySelectorAll('[data-index]').length };
    };
    const choose = index => {
        const input = doc().querySelector('#target');
        input.value = index;
        input.dispatchEvent(new Event('change', { bubbles: true }));
        doc().querySelector('#reveal').click();
    };
    const positions = [];
    try {
        await control(run, 'hold');
        frame.src = `/?run=${run}&initial=70`;
        await until(async () => (await journal(run)).pending > 0, 'initial provider held');
        assert(!row(70), 'initial ticket is absent while its real provider is held');
        const initialHeld = await journal(run);
        await control(run, 'release');
        await until(() => geometry(70)?.visible, 'initial ticket rendered and visible');
        await until(() => doc().querySelector('#reveal')?.disabled === false, 'interactive readiness');
        positions.push(geometry(70));
        assert(row(70).textContent === 'Lantern collection 070', 'initial index resolves to the correct rendered ticket');

        await control(run, 'hold');
        choose(150);
        await until(async () => (await journal(run)).pending > 0, 'programmatic provider held');
        await until(() => doc().querySelector('#status').textContent === 'Revealing 150', 'circuit status render');
        assert(!row(150), 'programmatic ticket is absent while its real provider is held');
        assert(doc().querySelector('#status').textContent === 'Revealing 150', 'awaited reveal has not reported completion before provider readiness');
        const programmaticHeld = await journal(run);
        await control(run, 'release');
        await until(() => geometry(150)?.visible && doc().querySelector('#status').textContent === 'Revealed 150', 'programmatic rendered ticket');
        positions.push(geometry(150));
        assert(row(150).textContent === 'Lantern collection 150', 'programmatic index resolves to the correct rendered ticket');

        for (const index of [-1, 200]) {
            const before = (await journal(run)).events.length;
            choose(index);
            await until(() => doc().querySelector('#status').textContent === 'Choose an index from 0 to 199', 'invalid index feedback');
            assert((await journal(run)).events.length === before, `application rejects index ${index} without calling the provider`);
        }
        for (const index of [0, 199]) {
            choose(index);
            await until(() => geometry(index)?.visible && doc().querySelector('#status').textContent === `Revealed ${index}`, `boundary ${index}`);
            positions.push(geometry(index));
            assert(row(index).textContent === `Lantern collection ${String(index).padStart(3, '0')}`, `boundary ${index} resolves to its actual ticket`);
        }
        const completed = await journal(run);
        for (const [value, expected] of [[-20, 0], [900, 199]]) {
            frame.src = `/?run=${run}-bound-${expected}&initial=${value}`;
            await until(() => doc()?.querySelector('#queue') && frame.contentWindow.location.search.includes(`initial=${value}`) && geometry(expected)?.visible, `initial clamp ${value}`);
            positions.push(geometry(expected));
            assert(true, `initial index ${value} clamps to rendered ticket ${expected}`);
        }
        assert(positions.every(position => position.rendered < 200), 'list remains virtualized rather than rendering all tickets');
        return { result: 'PASS', origin: location.origin, userAgent: navigator.userAgent, run, checks, positions,
            initialHeld, programmaticHeld, completed };
    } finally {
        await control(run, 'release');
        frame.remove();
    }
})();
