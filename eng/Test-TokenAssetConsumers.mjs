// Authoring-only real-browser/request checks. No external token issuance is claimed.
import { createRequire } from 'node:module';
import { createServer } from 'node:http';
import { readFile, readdir, mkdir, writeFile } from 'node:fs/promises';
import { spawn } from 'node:child_process';
import { resolve, dirname, extname } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';
import { createHash } from 'node:crypto';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwright, net10, net11, output = resolve(root, 'artifacts/token-assets/behavior.json')] = process.argv.slice(2);
if (!playwright || !net10 || !net11) throw new Error('Supply absolute playwright-core and both dotnet executable paths.');
const { chromium } = createRequire(import.meta.url)(playwright);
const browser = await chromium.launch({ headless: true });
const results = [];
const hash = bytes => createHash('sha256').update(bytes).digest('hex');
async function files(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  return (await Promise.all(entries.map(e => e.isDirectory() ? files(resolve(directory, e.name)) : [resolve(directory, e.name)]))).flat();
}
try {
  for (const [version, dotnet] of [['10.0', net10], ['11.0', net11]]) {
    const sample = resolve(root, `samples/net${version}/HydrophoneArchive`);
    const endpoints = JSON.parse(await readFile(resolve(sample, `obj/Release/net${version}/staticwebassets.build.json`), 'utf8')).Endpoints;
    // Serve the actual built WASM files, with a fixed application-owned SPA fallback.
    const server = createServer(async (request, response) => {
      const path = new URL(request.url, 'http://localhost').pathname;
      if (!path.startsWith('/archive/')) { response.writeHead(404).end(); return; }
      const relative = decodeURIComponent(path.slice('/archive/'.length));
      if (relative.split('/').includes('..')) { response.writeHead(400).end(); return; }
      const endpoint = endpoints.find(e => e.Route === relative && e.Selectors.length === 0 && !e.ResponseHeaders.some(h => h.Name === 'Content-Encoding'));
      if (endpoint) {
        const headers = Object.fromEntries(endpoint.ResponseHeaders.map(h => [h.Name, h.Value]));
        response.writeHead(200, headers); response.end(await readFile(endpoint.AssetFile)); return;
      }
      for (const directory of [`bin/Release/net${version}/wwwroot`, 'wwwroot']) {
        try {
          const bytes = await readFile(resolve(sample, directory, relative));
          const mime = { '.js': 'text/javascript', '.json': 'application/json', '.wasm': 'application/wasm', '.html': 'text/html' }[extname(relative)] ?? 'application/octet-stream';
          response.writeHead(200, { 'content-type': mime }); response.end(bytes); return;
        } catch (error) { if (!['ENOENT', 'EISDIR'].includes(error.code)) throw error; }
      }
      if (relative.includes('.')) { response.writeHead(404).end(); return; }
      response.writeHead(200, { 'content-type': 'text/html' }); response.end(await readFile(resolve(sample, 'wwwroot/index.html')));
    });
    await new Promise(done => server.listen(0, '127.0.0.1', done));
    const base = `http://127.0.0.1:${server.address().port}/archive/`;
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    try {
      const branches = [];
      for (const mode of ['success', 'redirect', 'exception', 'exception-default', 'fallback', ...(version === '10.0' ? ['legacy', 'modern-fallback'] : [])]) {
        await page.goto(base);
        await page.locator('#outcome').filter({ hasText: 'Ready' }).waitFor();
        const initial = await page.evaluate(() => ({ length: history.length, state: history.state, marker: window.archiveDocument = Math.random() }));
        await page.locator(`#${mode}`).click();
        if (mode === 'modern-fallback') {
          await page.locator('#outcome').filter({ hasText: 'Old fallback cannot read the current URL' }).waitFor();
          assert.equal(page.url(), base);
          assert.deepEqual(await page.evaluate(() => history.state), initial.state);
          branches.push({ mode, result: 'Actual .NET 10 exception fallback rejects null legacy URL from four-argument result; no navigation' });
        } else if (mode === 'success') {
          await page.locator('#outcome').filter({ hasText: 'Token available; no navigation' }).waitFor();
          assert.equal(page.url(), base);
          assert.equal(await page.evaluate(() => history.length), initial.length);
          assert.deepEqual(await page.evaluate(() => history.state), initial.state);
          assert.equal(await page.locator('#request').textContent(), `recordings.read → ${base}recordings/reef-17`);
          branches.push({ mode, result: 'token branch; URL/history unchanged; requested scope and app-owned return preserved' });
        } else {
          await page.waitForURL(`${base}authentication/login`);
          assert.equal(await page.evaluate(() => window.archiveDocument), initial.marker, 'Navigation stays in the actual WASM document');
          const state = await page.evaluate(() => history.state);
          if (mode === 'fallback' || mode === 'legacy') {
            assert.equal(state.userState ?? null, null);
            branches.push({ mode, state, result: 'ordinary navigation; no interactive options/scopes invented' });
          } else {
            const options = JSON.parse(state.userState);
            assert.equal(options.returnUrl, `${base}recordings/reef-17`);
            assert.deepEqual(options.scopes, ['recordings.read']);
            assert.equal(options.interaction, 'GetToken');
            if (mode === 'exception') assert.equal(options.additionalRequestParameters.prompt, 'consent');
            branches.push({ mode, options, result: 'real NavigateToLogin history state; no identity provider contacted' });
          }
        }
        branches.at(-1).url = page.url();
      }
      assert.deepEqual(errors, []);
      results.push({ version, sample: 'HydrophoneArchive', branches });
    } catch (error) { console.error({ version, errors, body: await page.locator('body').innerText() }); throw error; }
    finally { await page.close(); await new Promise(done => server.close(done)); }

    const published = resolve(root, `artifacts/token-assets/gallery-${version}`);
    const child = spawn(dotnet, ['MaritimeGallery.dll', '--urls', 'http://127.0.0.1:0'], {
      cwd: published, env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', ASPNETCORE_ENVIRONMENT: 'Production' }, windowsHide: true
    });
    let logs = '';
    const origin = await new Promise((done, reject) => {
      const timer = setTimeout(() => reject(new Error(logs)), 30000);
      const consume = data => { logs += data; const match = logs.match(/Now listening on: (http:\/\/127\.0\.0\.1:\d+)/); if (match) { clearTimeout(timer); done(match[1]); } };
      child.stdout.on('data', consume); child.stderr.on('data', consume);
      child.on('error', reject); child.on('exit', code => { clearTimeout(timer); reject(new Error(`Server exited ${code}: ${logs}`)); });
    }).catch(error => { child.kill(); throw error; });
    const gallery = await browser.newPage();
    try {
      const response = await gallery.goto(`${origin}/museum/`);
      assert.equal(response.status(), 200);
      const component = await gallery.locator('#exhibit').first().getAttribute('src');
      const custom = await gallery.locator('#custom').getAttribute('source');
      const builtin = await gallery.locator('#builtin').getAttribute('src');
      assert.match(component, /^images\/sounding\.[a-z0-9]+\.svg$/);
      assert.equal(custom, component); assert.equal(builtin, component);
      assert.equal(await gallery.locator('#caption').first().textContent(), '~/images/sounding.svg');
      assert.equal(await gallery.locator('#plain').getAttribute('data-path'), '~/images/sounding.svg');
      assert.equal(await gallery.locator('#bare').getAttribute('source'), 'images/sounding.svg');
      assert.equal(await gallery.locator('#unknown').getAttribute('source'), 'images/unknown.svg');
      assert.equal(await gallery.locator('#arbitrary').getAttribute('source'), 'collection:lead-line');
      const asset = await fetch(`${origin}/museum/${component}`);
      assert.equal(asset.status, 200); assert.match(asset.headers.get('content-type'), /image\/svg\+xml/);
      const bytes = Buffer.from(await asset.arrayBuffer());
      assert.deepEqual(bytes, await readFile(resolve(root, `samples/net${version}/MaritimeGallery/wwwroot/images/sounding.svg`)));
      assert.equal((await fetch(`${origin}/museum/images/unknown.svg`)).status, 404);
      const generatedPath = version === '11.0'
        ? resolve(root, `samples/net${version}/MaritimeGallery/obj/Release/net${version}/Razor/App.razor.g.cs`)
        : (await files(resolve(root, `samples/net${version}/MaritimeGallery/obj/Release/net${version}/generated`))).find(p => p.endsWith('App_razor.g.cs'));
      const generated = await readFile(generatedPath, 'utf8');
      assert.match(generated, /Assets\[@?"images\/sounding\.svg"\]/, 'Real generated C# must resolve opted-in asset values');
      if (version === '11.0') assert.match(generated, /Assets\[@?"images\/unknown\.svg"\]/, 'Unknown key uses normal collection fallback');
      const manifestPath = resolve(published, 'MaritimeGallery.staticwebassets.endpoints.json');
      const manifest = await readFile(manifestPath, 'utf8');
      assert.ok(manifest.includes(component));
      const restorePath = resolve(root, `samples/net${version}/MaritimeGallery/obj/project.assets.json`);
      const restore = JSON.parse(await readFile(restorePath, 'utf8'));
      results.push({ version, sample: 'MaritimeGallery', url: `/museum/${component}`, retrievedSha256: hash(bytes),
        controls: 'unmarked ~/ strings, bare path and arbitrary string unchanged; unknown ~/ key lowers to lookup fallback and HTTP 404',
        generatedSha256: hash(generated), publishManifestSha256: hash(manifest), dependencies: Object.keys(restore.libraries).sort() });
    } finally { await gallery.close(); child.kill(); await new Promise(done => child.exitCode === null ? child.once('exit', done) : done()); }
  }
  await mkdir(dirname(output), { recursive: true });
  await writeFile(output, JSON.stringify({ browser: browser.version(), results }, null, 2) + '\n');
  console.log(`PASS: ${results.length} real matching consumers, WASM history-state branches and published non-root fingerprinted asset requests.`);
} finally { await browser.close(); }
