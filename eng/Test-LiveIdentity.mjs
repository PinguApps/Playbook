// Execute actual TypeScript clients and real Interactive Server circuits.
import { createRequire } from 'node:module';
import { spawn } from 'node:child_process';
import { mkdir, writeFile } from 'node:fs/promises';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const repository = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwright, chromiumPath, dotnet, output = resolve(repository, 'artifacts/identity-tooling/behavior.json'), sampleRoot = repository] = process.argv.slice(2);
const { chromium } = createRequire(import.meta.url)(playwright);
const browser = await chromium.launch({ executablePath: chromiumPath, headless: true });
const results = [];
async function serve(major, name, action) {
    const cwd = resolve(sampleRoot, `samples/net${major}.0/${name}`);
    const child = spawn(dotnet, [`bin/Release/net${major}.0/${name}.dll`, '--urls', 'http://127.0.0.1:0', '--serve'], {
        cwd, windowsHide: true, env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', ASPNETCORE_ENVIRONMENT: 'Development' }
    });
    let logs = '';
    try {
        const base = await new Promise((ready, reject) => {
            const timer = setTimeout(() => reject(new Error('Startup timeout: ' + logs)), 30000);
            const consume = data => {
                logs += data;
                const address = logs.match(/Now listening on: (http:\/\/127\.0\.0\.1:\d+)/);
                if (address) { clearTimeout(timer); ready(address[1]); }
            };
            child.stdout.on('data', consume); child.stderr.on('data', consume);
            child.once('error', error => { clearTimeout(timer); reject(error); });
            child.once('exit', code => { clearTimeout(timer); reject(new Error(`Exit ${code}: ${logs}`)); });
        });
        results.push({ major, sample: name, runtime: logs.match(/Sample runtime: ([^\r\n]+)/)?.[1], ...await action(base) });
    } catch (error) { throw new Error(`${major} ${name}: ${error.stack}\n${logs}`, { cause: error }); }
    finally {
        child.kill();
        await new Promise(done => { if (child.exitCode !== null) done(); else child.once('exit', done); });
    }
}
try {
    for (const major of [10, 11]) {
        await serve(major, 'CopperRidgeHub', async base => {
            const checks = [];
            for (const path of major === 11 ? ['/quarry', '/gateway'] : ['/quarry']) {
                const client = spawn(process.execPath, [resolve(sampleRoot, 'artifacts/identity-tooling/ts/check.js'), base + path, String(major)], { windowsHide: true });
                let log = '';
                client.stdout.on('data', data => log += data); client.stderr.on('data', data => log += data);
                const code = await new Promise((done, reject) => { client.once('exit', done); client.once('error', reject); });
                assert.equal(code, 0, log);
                assert.ok(log.includes('PASS: real TypeScript hub'), log);
                checks.push(log.trim());
            }
            return { checks };
        });
        await serve(major, 'ArchiveAccess', async base => {
            const page = await browser.newPage();
            try {
                await page.goto(base);
                await page.locator('#records').filter({ hasText: 'Public: tide tables' }).waitFor();
                const circuit = await page.locator('#circuit').innerText();
                const navigation = await page.evaluate(() => performance.timeOrigin);
                await page.locator('#grant').click();
                await page.locator('#notifications').filter({ hasText: '1' }).waitFor();
                if (major === 10) {
                    assert.equal(await page.locator('#records').innerText(), 'Public: tide tables');
                    await page.locator('#reload').click();
                }
                await page.locator('#records').filter({ hasText: 'Curator: sealed atlas' }).waitFor();
                await page.locator('#revoke').click();
                await page.locator('#notifications').filter({ hasText: '2' }).waitFor();
                if (major === 10) await page.locator('#reload').click();
                await page.locator('#records').filter({ hasText: 'Public: tide tables' }).waitFor();
                assert.equal(await page.locator('#circuit').innerText(), circuit);
                assert.equal(await page.evaluate(() => performance.timeOrigin), navigation);
                return { notifications: 2, unchangedCircuit: true, unchangedNavigation: true, grant: 'Curator: sealed atlas', revoke: 'Public: tide tables', manualRefresh: major === 10 };
            } finally { await page.close(); }
        });
    }
    await mkdir(dirname(output), { recursive: true });
    await writeFile(output, JSON.stringify({ browser: browser.version(), node: process.version, results }, null, 2) + '\n');
    console.log('PASS: actual old/current TypeScript hub behaviour and two Interactive Server circuit updates.');
} finally { await browser.close(); }
