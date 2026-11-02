import { createRequire } from 'node:module';
import { createServer } from 'node:http';
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { resolve, dirname, extname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwright, executablePath, output = resolve(root, 'artifacts/float-soundings/browser.json')] = process.argv.slice(2);
if (!playwright || !executablePath) throw new Error('Supply absolute Playwright package and Chromium executable paths.');
const { chromium } = createRequire(import.meta.url)(playwright);
const browser = await chromium.launch({ executablePath, headless: true });
const results = [];
try {
    for (const target of ['net10.0', 'net11.0']) {
        const sample = resolve(root, `samples/${target}/FloatSoundings`);
        const directory = resolve(sample, `bin/Release/${target}/publish/wwwroot`);
        const server = createServer(async (request, response) => {
            try {
                const path = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
                const file = resolve(directory, '.' + (path === '/' ? '/index.html' : path));
                if (!file.startsWith(directory + sep)) { response.writeHead(403).end(); return; }
                const fallback = path === '/' ? resolve(sample, 'wwwroot/index.html') : path === '/main.js' ? resolve(root, 'samples/FloatSoundings/main.js') : null;
                let data;
                try { data = await readFile(file); } catch (error) { if (!fallback) throw error; data = await readFile(fallback); }
                response.setHeader('Content-Type', ({ '.html': 'text/html', '.js': 'text/javascript', '.json': 'application/json', '.wasm': 'application/wasm' })[extname(file)] || 'application/octet-stream');
                response.end(data);
            } catch { response.writeHead(404).end(); }
        });
        await new Promise(done => server.listen(0, '127.0.0.1', done));
        const page = await browser.newPage();
        const messages = [];
        page.on('console', message => messages.push(message.text()));
        page.on('pageerror', error => messages.push(error.stack));
        try {
            await page.goto(`http://127.0.0.1:${server.address().port}`);
            await page.waitForFunction(() => globalThis.proof, { timeout: 60000 });
            const proof = await page.evaluate(() => globalThis.proof);
            assert.ok(!proof.error, JSON.stringify({ proof, messages: messages.slice(0, 5) }));
            assert.ok(proof.result.startsWith('PASS FloatSoundings: .NET ' + (target === 'net10.0' ? '10.0.13' : '11.0.0-rc.2.26504.105')));
            assert.equal(proof.arrayType, target === 'net10.0' ? 'Float64Array' : 'Float32Array');
            results.push({ target, ...proof, messages });
            console.log(target, proof);
        } finally { await page.close(); await new Promise(done => server.close(done)); }
    }
    await mkdir(dirname(output), { recursive: true });
    await writeFile(output, JSON.stringify({ browser: browser.version(), executablePath, results }, null, 2) + '\n');
} finally { await browser.close(); }
