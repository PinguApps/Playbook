// Real browser/HTTP authoring proof; no persistent environment changes or external services.
import { createRequire } from 'node:module';
import { spawn } from 'node:child_process';
import { mkdir, writeFile } from 'node:fs/promises';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwright, executable, net10, net11, output = resolve(root, 'artifacts/garden-library/behavior.json')] = process.argv.slice(2);
if (!playwright || !executable || !net10 || !net11) throw new Error('Supply absolute Playwright module, Chromium executable and .NET 10/11 executables.');
const { chromium } = createRequire(import.meta.url)(playwright);
const browser = await chromium.launch({ headless: true, executablePath: executable });
const results = [];
async function withServer(version, dotnet, sample, args, action) {
  const cwd = resolve(root, `samples/net${version}/${sample}`);
  const child = spawn(dotnet, [`bin/Release/net${version}/${sample}.dll`, '--urls', 'http://127.0.0.1:0', ...args], {
    cwd, windowsHide: true,
    env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', ASPNETCORE_ENVIRONMENT: 'Development' }
  });
  let logs = '';
  try {
    const base = await new Promise((ready, reject) => {
      const timer = setTimeout(() => reject(new Error('Startup timeout: ' + logs)), 30000);
      const consume = data => {
        logs += data;
        const match = logs.match(/Now listening on: (http:\/\/127\.0\.0\.1:\d+)/);
        if (match) { clearTimeout(timer); ready(match[1]); }
      };
      child.stdout.on('data', consume); child.stderr.on('data', consume);
      child.on('error', error => { clearTimeout(timer); reject(error); });
      child.on('exit', code => { clearTimeout(timer); reject(new Error(`Exit ${code}: ${logs}`)); });
    });
    await action(base);
    const runtime = logs.match(/Sample runtime: ([^\r\n]+)/)?.[1];
    assert.ok(runtime?.startsWith('.NET ' + version + '.'));
    for (const result of results.filter(r => r.version === version && r.sample === sample)) result.runtime = runtime;
  } catch (error) { throw new Error(`${version} ${sample}: ${error.message}\n${logs}`, { cause: error }); }
  finally {
    child.kill();
    await new Promise(done => { if (child.exitCode !== null) done(); else child.once('exit', done); });
  }
}
try {
  for (const [version, dotnet] of [['10.0', net10], ['11.0', net11]]) {
    await withServer(version, dotnet, 'GardenAccession', [], async base => {
      const page = await browser.newPage();
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      const counts = async () => (await fetch(base + '/counts')).json();
      try {
        for (const mode of ['enhanced', 'plain']) {
          await page.goto(base + '/' + mode);
          await page.waitForFunction(() => !!window.Blazor);
          if (version === '11.0') {
            await page.waitForFunction(() => !!window.Blazor.formValidation);
            const rules = await page.locator('blazor-client-validation-data').getAttribute('data-rules');
            const field = JSON.parse(rules).fields.find(f => f.name === 'Model.Code');
            const rule = field.rules.find(r => r.name === 'garden.accession');
            assert.deepEqual(rule.params, { letters: '2', digits: '4', separator: '-' });
          }
          const before = await counts();
          // Exercise case, separator, length, Unicode digit and whitespace boundaries in the real browser.
          for (const invalid of ['', 'ab-1234', 'AB_1234', 'AB-123', 'AB-１２３４', ' AB-1234']) {
            await page.locator('#code').fill(invalid);
            await page.locator('#code').blur();
            await page.locator('#save').click();
            await page.waitForFunction(() => document.body.textContent.includes('Enter an accession code.')
              || document.body.textContent.includes('Use two uppercase bed letters'));
            assert.deepEqual(await counts(), before);
          }
          await page.locator('#code').fill('AB-1234');
          await page.locator('#code').blur();
          const sentinel = await page.evaluate(() => window.gardenSentinel = Math.random());
          await page.locator('#save').click();
          await page.locator('#result').filter({ hasText: 'Accepted' }).waitFor();
          assert.equal((await counts()).accepted, before.accepted + 1);
          assert.equal((await page.evaluate(() => window.gardenSentinel)) === sentinel, mode === 'enhanced');
          // formnovalidate bypass still reaches authoritative custom server rule.
          await page.goto(base + '/' + mode);
          await page.waitForFunction(() => !!window.Blazor);
          await page.locator('#code').fill('ab-1234');
          await page.locator('#code').blur();
          const bypass = await counts();
          await page.locator('#bypass').click();
          await page.locator('#result').filter({ hasText: 'Rejected' }).waitFor();
          assert.equal((await counts()).posts, bypass.posts + 1);
          assert.equal((await counts()).accepted, bypass.accepted);
          // Direct HTTP POST bypasses every JS validator, using a real antiforgery token.
          const request = await browser.newContext();
          try {
            const get = await request.request.get(base + '/' + mode);
            const token = (await get.text()).match(/name="__RequestVerificationToken" value="([^"]+)"/)[1];
            for (const code of ['AB-1234', 'ab-1234', 'AB_1234', 'AB-123', 'AB-１２３４', ' AB-1234', '']) {
              const post = await request.request.post(base + '/' + mode, { form: {
                _handler: 'accession', __RequestVerificationToken: token, 'Model.Code': code
              } });
              assert.equal(post.status(), 200);
              assert.ok((await post.text()).includes(`<output id="result">${code === 'AB-1234' ? 'Accepted' : 'Rejected'}</output>`));
            }
          } finally { await request.close(); }
          results.push({ version, sample: 'GardenAccession', mode, ruleMapping: version === '11.0',
            invalidClientBlocked: 6, directServerCases: 7, bypassRejected: true, enhancedDocument: mode === 'enhanced' });
        }
        if (version === '11.0') {
          await page.goto(base + '/optout');
          assert.equal(await page.locator('blazor-client-validation-data').count(), 0);
          await page.locator('#code').fill('bad');
          await page.locator('#save').click();
          await page.locator('#result').filter({ hasText: 'Rejected' }).waitFor();
          results.push({ version, sample: 'GardenAccession', perFormOptout: 'No rules; server rejects invalid submit' });
        }
        assert.deepEqual(errors, []);
      } finally { await page.close(); }
    });
    if (version === '11.0') await withServer(version, dotnet, 'GardenAccession', ['--DisableClientValidation=true'], async base => {
      const page = await browser.newPage();
      try {
        await page.goto(base + '/enhanced');
        assert.equal(await page.locator('blazor-client-validation-data').count(), 0);
        await page.locator('#code').fill('bad');
        await page.locator('#save').click();
        await page.locator('#result').filter({ hasText: 'Rejected' }).waitFor();
        results.push({ version, sample: 'GardenAccession', globalOptout: 'No rules; server rejects invalid submit' });
      } finally { await page.close(); }
    });
    await withServer(version, dotnet, 'LibraryBadges', [], async base => {
      const ada = await browser.newContext(), ben = await browser.newContext();
      try {
        for (const [context, name] of [[ada, 'ada'], [ben, 'ben']])
          assert.equal((await context.request.post(base + '/control/signin/' + name)).status(), 204);
        const read = async (context, mode) => {
          const response = await context.request.get(base + '/' + mode);
          assert.equal(response.status(), 200);
          const html = await response.text();
          const value = (id, attribute) => html.match(new RegExp(`id="${id}"[^>]*${attribute}="([^"]+)"`))[1];
          return { html, request: value('request', 'data-ticket'), run: value('live', 'data-run'),
            ticket: value('live', 'data-ticket'), shelf: value('shelf', 'data-build') };
        };
        const first = await read(ada, 'live'), second = await read(ben, 'live'), third = await read(ada, 'live');
        assert.notEqual(first.run, second.run); assert.notEqual(second.run, third.run);
        assert.notEqual(first.request, second.request); assert.equal(first.shelf, second.shelf);
        assert.ok(first.html.includes('Reader ada')); assert.ok(second.html.includes('Reader ben'));
        assert.ok(!second.html.includes('Reader ada')); assert.ok(!third.html.includes('Reader ben'));
        assert.equal(second.ticket, version === '11.0' ? first.ticket : second.request);
        assert.equal(third.ticket, version === '11.0' ? first.ticket : third.request);
        if (version === '11.0') {
          const get = async context => {
            const response = await context.request.get(base + '/guarded');
            assert.equal(response.status(), 200); return response.text();
          };
          const a1 = await get(ada), a2 = await get(ada), b1 = await get(ben), b2 = await get(ben);
          const badge = html => html.match(/<p id="guarded"[^>]*>[^<]+<\/p>/)[0];
          assert.equal(badge(a1), badge(a2)); assert.equal(badge(b1), badge(b2));
          assert.notEqual(badge(a1), badge(b1));
          assert.ok(badge(a1).includes('ada')); assert.ok(badge(b1).includes('ben'));
          const beforeRefusal = await (await ada.request.get(base + '/control/counts')).json();
          assert.equal(beforeRefusal.guarded, 2);
          const refusal = await ada.request.get(base + '/refuse');
          assert.equal(refusal.status(), 500);
          assert.match(await refusal.text(), /GuardedBadge/);
          const counts = await (await ada.request.get(base + '/control/counts')).json();
          assert.equal(counts.live, 3); assert.equal(counts.guarded, 3); // Refusal happens during capture, after child initialization.
          assert.equal(counts.shelf, 3); // shared live boundary plus one per user in guarded boundary.
          results.push({ version, sample: 'LibraryBadges', counts, refusalStatus: 500,
            capturedParameterReplayed: true, liveUserFresh: true, userVariationIsolated: true });
        } else {
          const counts = await (await ada.request.get(base + '/control/counts')).json();
          assert.deepEqual(counts, { shelf: 1, live: 3 });
          results.push({ version, sample: 'LibraryBadges', counts, publicDataReused: true, explicitLiveBoundary: true });
        }
      } finally { await ada.close(); await ben.close(); }
    });
  }
  await mkdir(dirname(output), { recursive: true });
  await writeFile(output, JSON.stringify({ recordedAt: new Date().toISOString(), browser: browser.version(), results }, null, 2) + '\n');
  console.log(`PASS: ${results.length} actual garden browser/POST and library authenticated SSR cases. Report: ${output}`);
} finally { await browser.close(); }
