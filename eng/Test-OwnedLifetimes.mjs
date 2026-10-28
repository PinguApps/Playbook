// Focused real Interactive Server proof. No mocked JS runtime or InputFile renderer.
import { createRequire } from 'node:module';
import { spawn } from 'node:child_process';
import { mkdir, writeFile } from 'node:fs/promises';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const repository = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwright, chromiumPath, net10, net11, output = resolve(repository, 'artifacts/owned-lifetimes/browser.json'), sampleRoot = repository] = process.argv.slice(2);
if (!playwright || !chromiumPath || !net10 || !net11) throw new Error('Supply Playwright, Chromium, and both absolute dotnet paths.');
const { chromium } = createRequire(import.meta.url)(playwright);
const playwrightVersion = createRequire(playwright)('./package.json').version;
const browser = await chromium.launch({ executablePath: chromiumPath, headless: true });
const results = [];
async function serve(version, dotnet, name, action) {
  const cwd = resolve(sampleRoot, `samples/net${version}/${name}`);
  const child = spawn(dotnet, [`bin/Release/net${version}/${name}.dll`, '--urls', 'http://127.0.0.1:0'], {
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
    const result = await action(base);
    const runtime = logs.match(/Sample runtime: ([^\r\n]+)/)?.[1];
    assert.ok(runtime?.startsWith('.NET ' + version + '.'));
    results.push({ version, sample: name, runtime, ...result });
  } catch (error) { throw new Error(`${version} ${name}: ${error.stack}\n${logs}`, { cause: error }); }
  finally {
    child.kill();
    await new Promise(done => { if (child.exitCode !== null) done(); else child.once('exit', done); });
  }
}
try {
  for (const [version, dotnet] of [['10.0', net10], ['11.0', net11]]) {
    await serve(version, dotnet, 'ReadingRoom', async base => {
      const page = await browser.newPage();
      try {
        await page.goto(base);
        await page.waitForFunction(() => document.querySelector('#read')?.disabled === false);
        await page.locator('#read').click();
        await page.locator('#reading').filter({ hasText: 'Shelf R7: maps and atlases' }).waitFor();
        await page.locator('#js-failure').click();
        await page.locator('#reading').filter({ hasText: 'Browser label unavailable; use the printed shelf list' }).waitFor();
        // A failed browser call must not break the circuit or poison the next call.
        await page.locator('#read').click();
        await page.locator('#reading').filter({ hasText: 'Shelf R7: maps and atlases' }).waitFor();
        await page.locator('#app-failure').click();
        await page.locator('#application-error').filter({ hasText: 'InvalidOperationException: Catalogue record has no shelf' }).waitFor();
        await page.waitForFunction(async address => (await (await fetch(address)).json()).disposed === 1, base + '/audit');
        const connectedDisposal = await (await fetch(base + '/audit')).json();
        assert.deepEqual(connectedDisposal, { disposed: 1, disconnected: 0, referenceDisconnects: 0 });
        // A second real circuit retains a live imported JS object, then loses transport.
        await page.goto(base);
        await page.waitForFunction(() => document.querySelector('#read')?.disabled === false);
        await page.evaluate(() => Blazor.disconnect());
        // Explicit disconnect tears down the actual renderer; raw transport loss may await retention.
        // Poll through a separate HTTP request because the interactive circuit is intentionally gone.
        await page.waitForFunction(async address => (await (await fetch(address)).json()).disposed === 2,
          base + '/audit', { timeout: 20000 });
        const disconnectedDisposal = await (await fetch(base + '/audit')).json();
        assert.deepEqual(disconnectedDisposal, { disposed: 2, disconnected: 1, referenceDisconnects: version === '10.0' ? 1 : 0 });
        return { success: true, actualJsExceptionRecovered: true, subsequentCallSucceeded: true,
          applicationErrorVisible: 'InvalidOperationException: Catalogue record has no shelf', connectedDisposal, disconnectedDisposal };
      } finally { await page.close(); }
    });
    await serve(version, dotnet, 'RestorationBench', async base => {
      const page = await browser.newPage();
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      try {
        await page.goto(base);
        await page.waitForFunction(() => !!window.Blazor?._internal?.InputFile);
        // Capture the actual framework callback relay, without replacing its implementation.
        // Test instrumentation only: this is not a supported application integration seam.
        await page.evaluate(() => {
          const original = Blazor._internal.InputFile.init;
          Blazor._internal.InputFile.init = (relay, element) => {
            window.lastInputRelay = relay;
            return original(relay, element);
          };
        });
        const audit = async () => (await fetch(base + '/audit')).json();
        for (let cycle = 1; cycle <= 2; cycle++) {
          await page.locator('#mount').click();
          await page.waitForFunction(() => !!window.lastInputRelay && !!document.querySelector('#manuscript'));
          await page.locator('#manuscript').setInputFiles({ name: `folio-${cycle}.txt`, mimeType: 'text/plain', buffer: Buffer.from(`Restored ink ${cycle}`) });
          await page.locator('#file-result').filter({ hasText: `folio-${cycle}.txt: Restored ink ${cycle}` }).waitFor();
          await page.locator('#publish').click();
          await page.locator('#status').filter({ hasText: 'Proof ready' }).waitFor();
          // Establish this is the live .NET relay, not merely a saved JS identifier.
          await page.evaluate(() => window.lastInputRelay.invokeMethodAsync('NotifyChange', []));
          const mounted = await audit();
          assert.deepEqual(mounted, { active: 1, subscriptions: cycle, cancellations: cycle - 1, callbacks: cycle });
          await page.locator('#remove').click();
          await page.locator('#manuscript').waitFor({ state: 'detached' });
          await page.waitForFunction(async address => (await (await fetch(address)).json()).active === 0, base + '/audit');
          const relayFailure = await page.evaluate(async () => {
            try { await window.lastInputRelay.invokeMethodAsync('NotifyChange', []); return null; }
            catch (error) { return error.message; }
          });
          assert.match(relayFailure, /no tracked object|Cannot find.*object|disposed/i);
          await page.locator('#remove').click();
          await page.locator('#publish').click();
          assert.deepEqual(await audit(), { active: 0, subscriptions: cycle, cancellations: cycle, callbacks: cycle });
          assert.equal(await page.locator('#file-result').textContent(), `folio-${cycle}.txt: Restored ink ${cycle}`);
          await page.evaluate(() => window.lastInputRelay = null);
        }
        assert.deepEqual(errors, []);
        return { realFileSelectionAndRead: true, statusDelivered: true, removalCycles: 2,
          baseRelayRejectedAfterRemoval: true, noCallbacksAfterCancellation: true, completedReadRetained: true, audit: await audit() };
      } finally { await page.close(); }
    });
  }
  await mkdir(dirname(output), { recursive: true });
  await writeFile(output, JSON.stringify({ browser: browser.version(), node: process.version, playwright: playwrightVersion, results }, null, 2) + '\n');
  console.log(JSON.stringify({ browser: browser.version(), playwright: playwrightVersion, results }, null, 2));
} finally { await browser.close(); }
