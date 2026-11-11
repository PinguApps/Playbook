// Real interactive-server views, experimental AI packages and browser session storage.
// Pass an absolute playwright-core module path; no install or profile integration occurs.
const { chromium } = require(process.argv[2]);
const { spawn } = require('node:child_process');
const http = require('node:http');
const assert = require('node:assert/strict');
const path = require('node:path');
const root = path.resolve(__dirname, '..');
const dotnet = process.argv[3];
const children = [];
let mode = 'snapshot', received, aborted = 0;
const endpoint = http.createServer(async (req, res) => {
    let body = ''; for await (const part of req) body += part;
    received = JSON.parse(body);
    if (mode === 'failure') { res.writeHead(503); res.end('Scripted unavailable endpoint'); return; }
    res.writeHead(200, { 'Content-Type': 'text/event-stream' });
    const emit = event => res.write(`data: ${JSON.stringify(event)}\n\n`);
    const ids = { threadId: received.threadId, runId: received.runId || 'local-run' };
    emit({ type: 'RUN_STARTED', ...ids });
    if (received.messages.some(message => message.role === 'tool')) { emit({ type: 'RUN_FINISHED', ...ids }); res.end(); return; }
    if (mode === 'hold') { res.on('close', () => aborted++); return; }
    if (mode === 'delta') emit({ type: 'STATE_DELTA', delta: [{ op: 'replace', path: '/Caption', value: 'delta' }] });
    else if (mode === 'proposal') {
        emit({ type: 'TOOL_CALL_START', toolCallId: 'proposal', toolCallName: 'propose_caption' });
        emit({ type: 'TOOL_CALL_ARGS', toolCallId: 'proposal', delta: JSON.stringify({ caption: 'Proposed tide' }) });
        emit({ type: 'TOOL_CALL_END', toolCallId: 'proposal' });
    } else emit({ type: 'STATE_SNAPSHOT', snapshot: { Caption: 'Incoming tide', Revision: mode === 'stale' ? 0 : received.state.Revision + 1 } });
    emit({ type: 'RUN_FINISHED', ...ids }); res.end();
});
async function start(name, port, args = []) {
    const cwd = path.join(root, 'samples/net11.0', name);
    const child = spawn(dotnet, [path.join(cwd, `bin/Release/net11.0/${name}.dll`), '--urls', `http://127.0.0.1:${port}`, '--environment', 'Development', ...args], {
        cwd, windowsHide: true, env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', DOTNET_GENERATE_ASPNET_CERTIFICATE: 'false' }, stdio: ['ignore', 'pipe', 'pipe']
    });
    children.push(child);
    let output = ''; child.stdout.on('data', x => output += x); child.stderr.on('data', x => output += x);
    for (let i = 0; i < 100; i++) {
        if (child.exitCode !== null) throw Error(output);
        try { if ((await fetch(`http://127.0.0.1:${port}`)).ok) return; } catch {}
        await new Promise(resolve => setTimeout(resolve, 100));
    }
    throw Error(`Host startup timeout: ${output}`);
}
async function status(page, text) {
    try { await page.getByRole('status').filter({ hasText: text }).waitFor({ timeout: 5000 }); }
    catch (error) { console.error(await page.locator('body').innerText()); throw error; }
}
async function click(page, name) { await page.getByRole('button', { name, exact: true }).click(); }
(async () => {
    let browser;
    try {
        await new Promise(resolve => endpoint.listen(0, '127.0.0.1', resolve));
        await start('ObservatoryLog', 5619);
        await start('MuseumCaptions', 5620, ['--CaptionAgentEndpoint', `http://127.0.0.1:${endpoint.address().port}/agent`]);
        browser = await chromium.launch({ headless: true });
        const page = await browser.newPage();
        await page.goto('http://127.0.0.1:5619'); await status(page, 'Restored; ready');
        await page.locator('input').fill('Saturn'); await click(page, 'Send'); await status(page, 'Completed turns saved');
        await page.reload(); await status(page, 'Restored; ready');
        assert.equal(await page.getByText('Observation noted: Saturn', { exact: true }).count(), 1);
        await page.locator('input').fill('Jupiter'); await click(page, 'Send'); await status(page, 'Completed turns saved');
        await page.reload(); await status(page, 'Restored; ready');
        const body = await page.locator('body').innerText();
        assert(body.indexOf('Observation noted: Saturn') < body.indexOf('Observation noted: Jupiter'));
        assert.equal(await page.getByText('Observation noted: Saturn', { exact: true }).count(), 1);
        await page.evaluate(() => { window.originalSetItem = Storage.prototype.setItem; window.failStorage = true; Storage.prototype.setItem = function(...args) { if(window.failStorage) throw Error('Scripted quota failure'); return window.originalSetItem.apply(this,args); }; });
        await page.locator('input').fill('Mars'); await click(page, 'Send'); await status(page, 'Save failed');
        assert(await page.getByRole('button', { name: 'Send', exact: true }).isDisabled());
        await page.evaluate(() => { window.failStorage = false; });
        await click(page, 'Retry save'); await status(page, 'Completed turns saved');
        await page.reload(); await status(page, 'Restored; ready');
        assert.equal(await page.getByText('Observation noted: Mars', { exact: true }).count(), 1);
        await page.locator('input').fill('Canceled moon'); await click(page, 'Send'); await click(page, 'Cancel'); await status(page, 'Incomplete turn excluded');
        await click(page, 'Recreate view'); await status(page, 'Restored; ready');
        assert.equal(await page.getByText('Observation noted: Canceled moon', { exact: true }).count(), 0);
        await page.locator('input').fill('Disposed moon'); await click(page, 'Send'); await click(page, 'Recreate view'); await status(page, 'Restored; ready');
        assert.equal(await page.getByText('Observation noted: Disposed moon', { exact: true }).count(), 0);
        console.log('PASS: rendered completed turns, recreate/restore/continue, incomplete-turn exclusion, ordering, no replay duplicates, actual protected session storage failure/retry and component disposal');
        await page.goto('http://127.0.0.1:5620');
        await page.getByLabel('Caption', { exact: true }).waitFor();
        await page.getByLabel('Caption', { exact: true }).fill('User tide'); await page.getByLabel('Request').click();
        await click(page, 'Send'); await status(page, 'Run completed');
        assert.equal(received.state.Caption, 'User tide');
        assert.equal(await page.getByLabel('Caption', { exact: true }).inputValue(), 'Incoming tide');
        mode = 'delta'; await click(page, 'Send'); await status(page, 'full snapshots only');
        mode = 'stale'; await click(page, 'Send'); await status(page, 'Invalid, stale');
        mode = 'proposal'; await click(page, 'Send'); await page.getByRole('button', { name: 'Accept', exact: true }).waitFor({timeout:5000});
        assert(await page.getByLabel('Caption', { exact: true }).isDisabled());
        await click(page, 'Accept'); await status(page, 'Run completed');
        assert.equal(await page.getByLabel('Caption', { exact: true }).inputValue(), 'Proposed tide');
        await page.getByLabel('Caption', { exact: true }).fill('Concurrent edit'); await page.getByLabel('Request').click();
        await click(page, 'Send'); await page.getByRole('button', { name: 'Reject', exact: true }).waitFor(); await click(page, 'Reject'); await status(page, 'Run completed');
        assert.equal(await page.getByLabel('Caption', { exact: true }).inputValue(), 'Concurrent edit');
        mode = 'failure'; await click(page, 'Send'); await status(page, '503');
        mode = 'hold'; await click(page, 'Send'); await click(page, 'Cancel'); await status(page, 'Canceled');
        assert(aborted > 0);
        const previous = aborted;
        await click(page, 'Send'); await page.waitForTimeout(100); await click(page, 'Recreate view');
        await page.getByLabel('Caption', { exact: true }).waitFor();
        await page.waitForTimeout(300); assert(aborted > previous);
        assert.equal(await page.getByLabel('Caption', { exact: true }).inputValue(), 'Tide clock');
        console.log('PASS: rendered typed/outbound/snapshot state, delta and stale rejection, predictive acceptance, cancellation');
    } finally {
        if (browser) await browser.close();
        for (const child of children) child.kill();
        endpoint.closeAllConnections(); endpoint.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
