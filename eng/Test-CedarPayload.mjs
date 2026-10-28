// Authoring-only real Interactive Server and protected-session-storage checks.
import { spawn } from 'node:child_process';
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { resolve, dirname } from 'node:path';
import { pathToFileURL, fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwright, executablePath, net10, net11, output, sampleRoot = resolve(root, 'samples')] = process.argv.slice(2);
if (!output) throw new Error('Supply Playwright entry, Chromium, .NET 10/11 executables and report path.');
const { chromium } = await import(pathToFileURL(playwright));
const results = [];
const browser = await chromium.launch({ executablePath, headless: true });
try {
    for (const major of [10, 11]) {
        const directory = resolve(sampleRoot, `net${major}.0/CedarPlanetarium`);
        const server = spawn(major === 10 ? net10 : net11,
            [resolve(directory, `bin/Release/net${major}.0/CedarPlanetarium.dll`), '--urls', 'http://127.0.0.1:0'],
            { cwd: directory, windowsHide: true, env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', ASPNETCORE_ENVIRONMENT: 'Development' } });
        let log = '';
        server.stdout.on('data', data => { log += data; });
        server.stderr.on('data', data => { log += data; });
        try {
            const origin = await new Promise((ok, fail) => {
                const timer = setTimeout(() => fail(new Error(`Server startup timeout: ${log}`)), 30000);
                const observe = data => {
                    const match = String(data).match(/Now listening on: (http:\/\/127\.0\.0\.1:\d+)/);
                    if (match) { clearTimeout(timer); server.stdout.off('data', observe); ok(match[1]); }
                };
                server.stdout.on('data', observe);
                server.once('error', error => { clearTimeout(timer); fail(error); });
                server.once('exit', code => { clearTimeout(timer); fail(new Error(`Server exited ${code}: ${log}`)); });
            });
            // Two tabs in the same browser context share the host, but own circuits/session storage.
            const context = await browser.newContext();
            try {
                const first = await context.newPage();
                const second = await context.newPage();
                const failures = [];
                for (const page of [first, second]) {
                    page.on('pageerror', error => failures.push(error.message));
                    await page.goto(origin);
                    await page.locator('#status').filter({ hasText: 'ready' }).waitFor();
                }
                const scope1 = await first.locator('#scope').innerText();
                const scope2 = await second.locator('#scope').innerText();
                assert.notEqual(scope1, scope2);
                await first.locator('#run').click();
                await first.waitForFunction(() => document.querySelector('#status')?.textContent !== 'ready');
                assert.equal(await first.locator('#status').innerText(), `passed:Cedar-${scope1}:7`);
                assert.equal(await second.locator('#runs').innerText(), '0');
                await second.locator('#read').click();
                await second.locator('#status').filter({ hasText: 'missing' }).waitFor();
                await second.locator('#run').click();
                await second.locator('#status').filter({ hasText: `passed:Cedar-${scope2}:7` }).waitFor();
                await first.locator('#read').click();
                await first.locator('#status').filter({ hasText: `stored:Cedar-${scope1}` }).waitFor();
                const protectedValue = await first.evaluate(() => sessionStorage.getItem('cedar.ticket'));
                assert(protectedValue && !protectedValue.includes(scope1) && !protectedValue.includes('Cedar'));
                let trace = [];
                if (major === 11) {
                    trace = JSON.parse(await second.locator('#trace').innerText());
                    const ticket = trace.filter(event => event.Type === 'ShowTicket');
                    // Independent interop/storage JSON options in each of two circuits.
                    assert.equal(new Set(ticket.map(event => event.Options)).size, 4);
                    assert(ticket.every(event => event.Resolver === 'first'));
                    const fallback = trace.filter(event => event.Type === 'ExtraNote');
                    for (const option of new Set(ticket.map(event => event.Options))) {
                        assert.deepEqual(fallback.filter(event => event.Options === option).map(event => event.Resolver), ['first', 'second']);
                    }
                }
                await first.evaluate(() => {
                    const value = sessionStorage.getItem('cedar.ticket');
                    sessionStorage.setItem('cedar.ticket', value.substring(0, 20) + (value[20] === 'A' ? 'B' : 'A') + value.substring(21));
                });
                await first.locator('#read').click();
                await first.locator('#status').filter({ hasText: 'tamper rejected' }).waitFor();
                assert.deepEqual(failures, []);
                await mkdir(dirname(output), { recursive: true });
                await first.screenshot({ path: resolve(dirname(output), `cedar-${major}.png`) });
                results.push({ major, scope1, scope2, roundTrips: 2, independentScopes: true, independentStorage: true,
                    protectedCiphertext: true, tamperRejected: true, elementReferenceConverter: true, trace,
                    runtime: log.match(/^\.NET .+$/m)?.[0], ephemeralProtection: log.includes('Using ephemeral data protection provider') });
            } finally { await context.close(); }
        } catch (error) {
            await mkdir(dirname(output), { recursive: true });
            await writeFile(resolve(dirname(output), `failure-${major}.log`), log);
            console.error(log.split('\n').filter(line => /Exception|Error|failed|at Cedar/.test(line)).join('\n'));
            throw error;
        } finally {
            const exited = new Promise(ok => server.once('exit', ok));
            server.kill();
            await exited;
        }
    }
    await mkdir(dirname(output), { recursive: true });
    await writeFile(output, JSON.stringify({ browser: browser.version(), node: process.version, results }, null, 2) + '\n');
    console.log('PASS: both real circuit consumers, two independent scopes each, generated resolver precedence, reflection fallback, retained framework converter, protected storage and tamper rejection.');
} finally { await browser.close(); }
