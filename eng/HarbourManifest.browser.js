// Evaluate on a newly loaded HarbourManifest page in the collaborative browser.
(async () => {
    const wait = async (test, label) => {
        const deadline = performance.now() + 10000;
        while (!test()) {
            if (performance.now() > deadline) throw new Error(`Timed out: ${label}`);
            await new Promise(resolve => setTimeout(resolve, 25));
        }
    };
    const settle = () => new Promise(resolve => setTimeout(resolve, 350));
    const box = document.getElementById('viewport');
    const rows = () => [...box.querySelectorAll('tbody tr[aria-rowindex]')]
        .filter(row => row.cells.length && row.cells[0].textContent.trim());
    const at = id => rows().find(row => Number(row.cells[0].textContent) === id);
    const y = row => row.getBoundingClientRect().top - box.getBoundingClientRect().top;
    const first = () => rows().find(row => y(row) >= -2 && y(row) <= 33);
    const observations = [];
    const record = label => {
        const row = first();
        const observation = { label, scrollTop: box.scrollTop, scrollHeight: box.scrollHeight,
            firstId: row ? Number(row.cells[0].textContent) : null, y: row ? y(row) : null,
            renderedCount: rows().length };
        observations.push(observation);
        if (!rows().every(row => row.getBoundingClientRect().height === 32)) throw new Error('Row height drift');
        if (rows().length > 60) throw new Error('Grid is not virtualized');
        return observation;
    };
    const click = async id => {
        const revision = document.getElementById('revision').textContent;
        document.getElementById(id).click();
        await wait(() => document.getElementById('revision').textContent !== revision, id);
        await settle();
    };
    await wait(() => !document.getElementById('reveal').disabled && at(401) && Math.abs(y(at(401))) <= 2, 'initial 401');
    record('initial 401');
    document.getElementById('reveal').click();
    await wait(() => at(701) && Math.abs(y(at(701))) <= 2, 'programmatic 701');
    const before = record('revealed 701');
    await click('reload');
    const reload = record('fresh instances');
    if (reload.firstId !== before.firstId || Math.abs(reload.y - before.y) > 2) throw new Error('Reload moved identity');
    await click('prepend');
    const prepend = record('prepend while reading');
    if (prepend.firstId !== before.firstId || Math.abs(prepend.y - before.y) > 2) throw new Error('Prepend lost anchor');
    await click('append');
    const append = record('append while reading');
    if (append.firstId !== before.firstId || Math.abs(append.y - before.y) > 2) throw new Error('Append stole reading position');
    box.dispatchEvent(new KeyboardEvent('keydown', { key: 'Home', bubbles: true }));
    box.scrollTop = 0;
    await wait(() => at(-4), 'Home key loads first row');
    await settle();
    await click('prepend');
    record('prepend at start');
    if (box.scrollTop > 2 || !at(-9)) throw new Error('Start pin failed');
    if (document.querySelector('#blazor-error-ui')?.offsetHeight) throw new Error('Circuit error');
    return { status: 'PASS', url: location.href, userAgent: navigator.userAgent, observations };
})()
