// Authoring-only real browser checks. Build/publish the four pinned projects first.
import { chromium } from '../samples/WetlandIdentity/node_modules/playwright-core/index.mjs';
import { startIdentity } from '../samples/WetlandIdentity/provider.mjs';
import { createServer } from 'node:http';
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { resolve, dirname, extname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [executablePath, output = resolve(root, 'artifacts/authentication/browser.json')] = process.argv.slice(2);
if (!executablePath) throw new Error('Supply an absolute Chromium executable path.');
const mime = { '.html': 'text/html', '.js': 'text/javascript', '.json': 'application/json', '.wasm': 'application/wasm', '.css': 'text/css' };
async function serve(directory, port) {
    const server = createServer(async (req, res) => {
        const file = resolve(directory, '.' + decodeURIComponent(new URL(req.url, 'http://localhost').pathname));
        if (!file.startsWith(directory + sep) && file !== directory) { res.writeHead(403); res.end(); return; }
        try {
            const actual = extname(file) ? file : resolve(directory, 'index.html');
            const bytes = await readFile(actual);
            res.writeHead(200, { 'Content-Type': mime[extname(actual)] ?? 'application/octet-stream', 'Cache-Control': 'no-store' });
            res.end(bytes);
        } catch { res.writeHead(404); res.end(); }
    });
    await new Promise((ok, fail) => { server.once('error', fail); server.listen(port, '127.0.0.1', ok); });
    return () => new Promise(ok => server.close(ok));
}

const identity = await startIdentity([5981, 5982]);
let browser;
const results = [];
try {
    browser = await chromium.launch({ executablePath, headless: true });
    for (const version of ['10.0', '11.0']) {
        const port = version === '10.0' ? 5981 : 5982;
        const origin = `http://localhost:${port}`;
        const close = await serve(resolve(root, `samples/net${version}/WetlandPermits/bin/Release/net${version}/publish/wwwroot`), port);
        try {
            for (const mode of [...(version === '10.0' ? ['legacy'] : []), 'supported', 'default', 'invalid', 'missing']) {
                const context = await browser.newContext();
                const observations = [];
                await context.exposeBinding('recordAuthProof', (_source, data) => observations.push(data));
                // Observe framework history and storage operations without changing their values.
                await context.addInitScript(() => {
                    const record = data => { void window.recordAuthProof(data); };
                    for (const method of ['pushState', 'replaceState']) {
                        const original = history[method];
                        history[method] = function(state, title, url) { record({ kind: 'history', method, state, url: String(url) }); return original.apply(this, arguments); };
                    }
                    for (const method of ['setItem', 'removeItem', 'getItem']) {
                        const original = Storage.prototype[method];
                        Storage.prototype[method] = function(key) {
                            if (this === sessionStorage && key.includes('SignOut')) record({ kind: 'session', method, key });
                            return original.apply(this, arguments);
                        };
                    }
                });
                const page = await context.newPage();
                const errors = [];
                page.on('pageerror', error => errors.push(error.message));
                const paths = [];
                page.on('request', req => { const url = new URL(req.url()); if (req.isNavigationRequest()) paths.push({ origin: url.origin, path: url.pathname, queryKeys: [...url.searchParams.keys()].sort() }); });
                try {
                    await page.goto(origin + '/permits');
                    await page.locator('#identity').filter({ hasText: 'Signed out' }).waitFor();
                    await page.locator('#login').click();
                    await page.locator('#identity-login').click();
                    await page.locator('#identity').filter({ hasText: 'Reed Keeper' }).waitFor();
                    assert.ok(identity.events.some(e => e.path === '/auth' && e.queryKeys.includes('code_challenge')));
                    assert.ok(identity.events.some(e => e.path === '/token' && e.method === 'POST'));
                    const start = identity.events.length;
                    if (mode === 'legacy') await page.locator('#logout').click();
                    else if (mode === 'supported') await page.locator(version === '10.0' ? '#supported-logout' : '#logout').click();
                    else {
                        await page.goto(origin + '/proof');
                        await page.locator(mode === 'default' ? '#default-logout' : '#' + mode).click();
                    }
                    const rejected = mode === 'invalid' || (mode === 'missing' && version === '10.0');
                    if (rejected) {
                        await page.locator('#logout-error').filter({ hasText: 'not initiated from within the page' }).waitFor();
                        assert.equal(identity.events.slice(start).some(e => e.path.startsWith('/session/end')), false);
                    } else {
                        await page.locator('#confirm-logout').click();
                        await page.locator('#identity').filter({ hasText: 'Signed out' }).waitFor();
                        assert.equal(new URL(page.url()).pathname, '/permits');
                        assert.ok(paths.some(p => p.path === '/authentication/logout-callback'));
                        assert.ok(identity.events.slice(start).some(e => e.path === '/session/end' && e.queryKeys.includes('id_token_hint') && e.queryKeys.includes('state')));
                        // A new login must require provider interaction: the OP session really ended.
                        await page.locator('#login').click();
                        await page.locator('#identity-login').waitFor();
                    }
                    // Don't retain authentication callback state/code or arbitrary request values.
                    const history = observations.filter(x => x.kind === 'history' && String(x.url).includes('authentication/logout'))
                        .map(x => ({ method: x.method, path: new URL(x.url, origin).pathname, state: x.state }));
                    if (mode === 'supported' || mode === 'default') assert.ok(history.some(x => JSON.stringify(x.state).toLowerCase().includes('signout')), JSON.stringify(history));
                    const session = observations.filter(x => x.kind === 'session');
                    if (mode === 'legacy') assert.ok(session.some(x => x.method === 'setItem') && session.some(x => x.method === 'removeItem'));
                    assert.deepEqual(errors, []);
                    results.push({ version, sample: 'WetlandPermits', mode, outcome: rejected ? 'rejected before provider logout' : 'provider session ended; callback and public /permits; next login requires interaction', history, session, provider: identity.events.slice(start), navigation: paths.filter(p => p.path.includes('logout')) });
                } catch (error) {
                    await mkdir(dirname(output), { recursive: true });
                    await writeFile(output + '.failure.txt', `${version} ${mode} ${page.url()}\n${error.stack}\n${await page.locator('body').innerText()}\n${JSON.stringify(identity.events.slice(-15))}`);
                    throw error;
                } finally { await context.close(); }
            }
        } finally { await close(); }

        const closeBadge = await serve(resolve(root, `samples/net${version}/AstronomyBadges/bin/Release/net${version}/publish/wwwroot`), port);
        const page = await browser.newPage();
        try {
            await page.goto(origin);
            await page.locator('#badge').filter({ hasText: 'night-observer' }).waitFor();
            assert.equal(await page.locator('#same-service').textContent(), 'True');
            await page.locator('#token').click();
            await page.locator('#token-result').filter({ hasText: 'Success: observations.read' }).waitFor();
            await page.evaluate(() => badgeProof.tokenAvailable = false);
            await page.locator('#token').click();
            await page.locator('#token-result').filter({ hasText: `RequiresRedirect: ${origin}/badge; observations.read` }).waitFor();
            const calls = await page.evaluate(() => badgeProof.calls);
            const init = calls.filter(c => c.method === 'init');
            assert.equal(init.length, 1);
            assert.equal(init[0].options.client_id ?? init[0].options.clientId, 'astronomy-badges');
            assert.equal(init[0].logging.debugEnabled, version === '11.0');
            assert.deepEqual(calls.filter(c => c.method === 'getAccessToken').map(c => c.options.scopes), [['observations.read'], ['observations.read']]);
            let supportedLogging = null;
            if (version === '10.0') {
                await page.evaluate(() => badgeProof.tokenAvailable = true);
                await page.locator('#supported').click();
                await page.locator('#supported-result').filter({ hasText: 'Supported service resolved and returned token' }).waitFor();
                const supportedInit = await page.evaluate(() => badgeProof.calls.filter(c => c.method === 'init').at(-1));
                assert.equal(supportedInit.logging.debugEnabled, true);
                supportedLogging = supportedInit.logging;
            }
            results.push({ version, sample: 'AstronomyBadges', outcome: 'real WASM DI/options/logging; same service aliases; mapped badge; success and requiresRedirect preserve scopes/return URL', calls, supportedLogging });
        } finally { await page.close(); await closeBadge(); }
    }
    await mkdir(dirname(output), { recursive: true });
    await writeFile(output, JSON.stringify({ recordedAt: new Date().toISOString(), browser: browser.version(), node: process.version, identityProvider: 'oidc-provider/9.5.1; fixed fictional account, in-memory signing/session state; loopback only', results }, null, 2) + '\n');
    console.log(`PASS: both actual WASM releases, real local OIDC logout/failure branches and controlled badge integration (${browser.version()}).`);
} finally { await browser?.close(); await identity.close(); }
