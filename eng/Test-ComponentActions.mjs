// Focused authoring checks against the actual built Interactive Server consumers.
import { createRequire } from 'node:module';
import { spawn } from 'node:child_process';
import { createServer } from 'node:net';
import { mkdir, writeFile } from 'node:fs/promises';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwright, net10, net11, output] = process.argv.slice(2);
if (!playwright || !net10 || !net11 || !output) throw new Error('Supply playwright-core, both absolute dotnet paths and output JSON.');
const { chromium } = createRequire(import.meta.url)(playwright);
const browser = await chromium.launch({ channel: 'chrome', headless: true });
const results = [];
try {
  for (const [version, dotnet] of [['10.0', net10], ['11.0', net11]]) {
    for (const mode of ['before', 'after', 'missing-timer-render']) {
      const directory = resolve(root, `samples/net${version}/WillowQuay`);
      const args = ['build', '-c', 'Release', '--no-restore', '-t:Rebuild'];
      if (mode === 'before') args.push('-p:BeforeMigration=true', '-p:TreatWarningsAsErrors=false');
      if (mode === 'missing-timer-render') args.push('-p:OmitTimerRender=true');
      const build = spawn(dotnet, args, { cwd: directory, env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0' }, windowsHide: true });
      let diagnostics = '';
      build.stdout.on('data', data => diagnostics += data);
      build.stderr.on('data', data => diagnostics += data);
      assert.equal(await new Promise(done => build.on('exit', done)), 0, diagnostics);
      console.log(`Built ${version}/${mode}`);
      for (const id of ['BL0012', 'BL0014', 'BL0015']) {
        assert.equal(diagnostics.includes(id), mode === 'before' && version === '11.0', `${version}/${mode}/${id}: ${diagnostics}`);
      }
      const socket = createServer();
      await new Promise(done => socket.listen(0, '127.0.0.1', done));
      const port = socket.address().port;
      await new Promise(done => socket.close(done));
      const url = `http://127.0.0.1:${port}`;
      const host = spawn(dotnet, [resolve(directory, `bin/Release/net${version}/WillowQuay.dll`), '--urls', url], {
        cwd: directory, windowsHide: true,
        env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', ASPNETCORE_ENVIRONMENT: 'Development', DOTNET_ENVIRONMENT: 'Development' }
      });
      let logs = '';
      host.stdout.on('data', data => logs += data);
      host.stderr.on('data', data => logs += data);
      const page = await browser.newPage();
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      try {
        let ready = false;
        for (let i = 0; i < 100; i++) {
          if (host.exitCode !== null) throw new Error(logs);
          try { ready = (await fetch(`${url}/audit`)).ok; } catch { /* host starting */ }
          if (ready) break;
          await new Promise(done => setTimeout(done, 100));
        }
        assert.ok(ready, logs);
        const runtime = (await (await fetch(`${url}/audit`)).json()).runtime;
        assert.match(runtime, new RegExp(`^\\.NET ${version === '10.0' ? '10\\.0\\.13$' : '11\\.0\\.0-rc\\.2\\.26504\\.105$'}`));
        await page.goto(url);
        await page.locator('#ticket').filter({ hasText: '40' }).waitFor();
        // Wait for the interactive connection: static HTML is not behavior proof.
        await page.locator('#next-ticket').click();
        await page.locator('#ticket').filter({ hasText: '41' }).waitFor();
        await page.locator('.feeding').first().click();
        const first = mode === 'before' ? '4' : '1';
        await page.locator('#selected-tank').filter({ hasText: first }).waitFor();
        await page.locator('.feeding').last().click();
        const last = mode === 'before' ? '4' : '3';
        await page.locator('#selected-tank').filter({ hasText: last }).waitFor();
        await page.locator('.foreach-feeding').first().click();
        await page.locator('#selected-tank').filter({ hasText: '1' }).waitFor();
        await page.locator('.foreach-feeding').last().click();
        await page.locator('#selected-tank').filter({ hasText: '3' }).waitFor();
        await page.locator('#start-timer').click();
        for (let i = 0; i < 100; i++) {
          if ((await (await fetch(`${url}/audit`)).json()).timerFired === 1) break;
          await new Promise(done => setTimeout(done, 50));
        }
        assert.equal((await (await fetch(`${url}/audit`)).json()).timerFired, 1);
        if (mode === 'missing-timer-render') {
          assert.equal(await page.locator('#timer').textContent(), '0', 'External callback alone does not request a render.');
          await page.locator('#next-ticket').click();
        }
        await page.locator('#timer').filter({ hasText: '1' }).waitFor();
        await page.locator('#read-panel').click();
        await page.locator('#instrument').filter({ hasText: mode === 'before' ? 'Callback unavailable' : 'Level 137 cm' }).waitFor();
        if (mode !== 'before') {
          assert.equal(await page.evaluate(() => willowPanel.lastReference.invokeMethodAsync('AcceptLevel', 138)), 'Level 138 cm');
          const invalid = await page.evaluate(async () => { try { await willowPanel.lastReference.invokeMethodAsync('AcceptLevel', -1); return false; } catch { return true; } });
          assert.equal(invalid, true);
          const wrongName = await page.evaluate(async () => { try { await willowPanel.lastReference.invokeMethodAsync('ReceiveLevel', 137); return false; } catch { return true; } });
          assert.equal(wrongName, true);
        }
        await page.locator('#toggle-panel').click();
        await page.locator('#instrument').waitFor({ state: 'detached' });
        const disposed = await page.evaluate(async () => { try { await willowPanel.lastReference.invokeMethodAsync('AcceptLevel', 137); return false; } catch { return true; } });
        assert.equal(disposed, true, 'Removed panel must release its DotNetObjectReference.');
        assert.equal((await (await fetch(`${url}/audit`)).json()).disposedPanels, 1);
        assert.deepEqual(errors, []);
        results.push({ version, mode, runtime, buildCommand: [dotnet, ...args], diagnostics: [...new Set(diagnostics.match(/BL00\d\d/g) ?? [])],
          ticket: '40 → 41', feeding: [first, last], foreach: ['1', '3'], timer: mode === 'missing-timer-render' ? 'Callback ran; stayed 0 until framework event rendered 1' : 'External callback rendered 1',
          callback: mode === 'before' ? 'Non-public method rejected by real JS interop' : 'Declared AcceptLevel identifier returned 137/138; invalid input and C# name rejected', disposedReferenceRejected: disposed, disposedPanels: 1 });
      } catch (error) { console.error({ version, mode, logs, body: await page.locator('body').innerText().catch(() => '') }); throw error; }
      finally { await page.close(); host.kill(); await new Promise(done => host.exitCode !== null ? done() : host.on('exit', done)); }
    }
  }
  await mkdir(dirname(resolve(output)), { recursive: true });
  await writeFile(output, JSON.stringify({ browser: browser.version(), results }, null, 2) + '\n');
  console.log('PASS: both matching consumers; actual diagnostics, closure failure, render distinction, JS visibility and disposal.');
} finally { await browser.close(); }
