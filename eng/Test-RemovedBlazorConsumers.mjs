// Authoring check: run the four actual built consumers with their matching runtimes.
// Supply the absolute playwright-core module directory; no browser installation or PATH mutation.
import { createRequire } from 'node:module';
import { spawn } from 'node:child_process';
import { mkdir, writeFile } from 'node:fs/promises';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwrightPath, net10, net11, output = resolve(root, 'artifacts/removed-api/behavior.json')] = process.argv.slice(2);
if (!playwrightPath || !net10 || !net11) throw new Error('Supply absolute playwright-core, .NET 10 and .NET 11 executable paths.');
const { chromium } = createRequire(import.meta.url)(playwrightPath);
const browser = await chromium.launch({ headless: true });
const results = [];
try {
  for (const [version, dotnet] of [['10.0', net10], ['11.0', net11]]) {
    for (const sample of ['TidalStations', 'SailRepairs']) {
      const cwd = resolve(root, `samples/net${version}/${sample}`);
      const child = spawn(dotnet, [`bin/Release/net${version}/${sample}.dll`, '--urls', 'http://127.0.0.1:0'], {
        cwd, env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', ASPNETCORE_ENVIRONMENT: 'Development' }, windowsHide: true
      });
      let logs = '';
      const base = await new Promise((resolveReady, reject) => {
        const timer = setTimeout(() => reject(new Error(`Startup timed out: ${logs}`)), 30000);
        const consume = data => {
          logs += data;
          const match = logs.match(/Now listening on: (http:\/\/127\.0\.0\.1:\d+)/);
          if (match) { clearTimeout(timer); resolveReady(match[1]); }
        };
        child.stdout.on('data', consume);
        child.stderr.on('data', consume);
        child.on('exit', code => { clearTimeout(timer); reject(new Error(`Server exited ${code}: ${logs}`)); });
        child.on('error', reject);
      }).catch(error => { child.kill(); throw error; });
      const page = await browser.newPage();
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      try {
        if (sample === 'TidalStations') {
          for (const [path, expected] of [['new', 'Create station'], ['SA7', 'Station SA7']]) {
            const response = await fetch(`${base}/stations/${path}`);
            assert.equal(response.status, 200);
            assert.ok((await response.text()).includes(`<h1 id="outcome">${expected}</h1>`));
          }
          // No prerender: these outcomes require a live circuit and client Router.
          await page.goto(`${base}/stations/new?interactive=1`);
          await page.locator('#outcome').filter({ hasText: 'Create station' }).waitFor();
          const marker = await page.evaluate(() => window.routeDocument = Math.random());
          await page.locator('#to-code').click();
          await page.locator('#outcome').filter({ hasText: 'Station SA7' }).waitFor();
          assert.equal(await page.evaluate(() => window.routeDocument), marker);
          await page.locator('#to-new').click();
          await page.locator('#outcome').filter({ hasText: 'Create station' }).waitFor();
          assert.equal(await page.evaluate(() => window.routeDocument), marker);
          results.push({ version, sample, staticRequests: ['new=Create station', 'SA7=Station SA7'], interactive: 'new → SA7 → new; same document; prerender disabled' });
        } else {
          await page.goto(base);
          await page.locator('#material').waitFor();
          const scope = await page.locator('#scope').textContent();
          for (let generation = 0; generation < 3; generation++) {
            const before = Number(await page.locator('#checks').textContent());
            await page.locator('#material').fill('plastic');
            await page.locator('#material').blur();
            await page.getByText('Choose canvas or sailcloth.', { exact: true }).waitFor();
            assert.equal(await page.getByText('Choose canvas or sailcloth.', { exact: true }).count(), 1, `${version} ${await page.locator('body').innerHTML()}`);
            await page.waitForFunction(n => Number(document.querySelector('#checks').textContent) === n + 1, before);
            await page.locator('#submit').click();
            await page.locator('#result').filter({ hasText: 'Rejected' }).waitFor();
            await page.waitForFunction(n => Number(document.querySelector('#checks').textContent) === n + 2, before);
            assert.equal(await page.getByText('Choose canvas or sailcloth.', { exact: true }).count(), 1, `${version} ${await page.locator('body').innerHTML()}`);
            await page.locator('#material').fill('canvas');
            await page.locator('#material').blur();
            await page.getByText('Choose canvas or sailcloth.', { exact: true }).waitFor({ state: 'detached' });
            await page.locator('#submit').click();
            await page.locator('#result').filter({ hasText: 'Accepted' }).waitFor();
            await page.waitForFunction(n => Number(document.querySelector('#checks').textContent) === n + 4, before);
            await page.locator('#replace').click();
            await page.locator('#result').filter({ hasText: 'Ready' }).waitFor();
            assert.equal(await page.locator('#detached').textContent(), 'Old context detached');
            assert.equal(Number(await page.locator('#checks').textContent()), before + 4);
            assert.equal(await page.locator('#material').inputValue(), '');
            assert.equal(await page.locator('#scope').textContent(), scope);
          }
          results.push({ version, sample, scope, fieldAndSubmit: 'plastic rejected; canvas accepted; one service call/event and one message', replacement: 'three replacements; old field/submit callbacks detached; scoped service retained' });
        }
        assert.deepEqual(errors, []);
      } finally {
        await page.close();
        child.kill();
        await new Promise(resolveExit => child.exitCode !== null ? resolveExit() : child.once('exit', resolveExit));
      }
    }
  }
  await mkdir(dirname(output), { recursive: true });
  await writeFile(output, JSON.stringify({ recordedAt: new Date().toISOString(), browser: browser.version(), kind: 'execution', results }, null, 2) + '\n');
  console.log(`PASS: actual .NET 10/11 HTTP routing, interactive navigation and scoped service-backed field/submit/replacement checks (${browser.version()}).`);
} finally { await browser.close(); }
