import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { once } from 'node:events';
import fs from 'node:fs';
import http from 'node:http';
import net from 'node:net';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createHash } from 'node:crypto';

const repository = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const major = process.argv[2];
assert.ok(['10', '11'].includes(major), 'Select the actual canonical major: 10 or 11');
const directory = path.join(repository, `samples/net${major}.0/EstuaryBoard`);
const server = path.join(directory, 'serve.mjs');
const assets = path.join(directory, `bin/Release/net${major}.0/publish/wwwroot`);
const port = major === '10' ? 5851 : 5852;
const probe = net.createServer();
await new Promise((resolve, reject) => { probe.once('error', reject); probe.listen(port, '127.0.0.1', resolve); });
await new Promise(resolve => probe.close(resolve));
const child = spawn(process.execPath, [server, assets, String(port)], { stdio: ['ignore', 'pipe', 'pipe'] });
let diagnostics = '';
child.stderr.on('data', bytes => { diagnostics += bytes; });
const exited = once(child, 'exit');
const request = target => new Promise((resolve, reject) => {
    const outgoing = http.get({ hostname: '127.0.0.1', port, path: target, signal: AbortSignal.timeout(5000) }, response => {
        let body = '';
        response.setEncoding('utf8');
        response.on('data', text => { body += text; });
        response.on('end', () => resolve({ status: response.statusCode, body }));
        response.on('error', reject);
    });
    outgoing.on('error', reject);
});
const checks = [];
try {
    await Promise.race([
        once(child.stdout, 'data', { signal: AbortSignal.timeout(5000) }),
        exited.then(() => { throw new Error(`Fixture exited before listening: ${diagnostics}`); })
    ]);
    for (const target of ['/%E0', '/%E0%A4%A', 'http://%']) {
        const malformed = await request(target);
        assert.equal(malformed.status, 400, `Malformed target ${target}`);
        const index = await request('/');
        assert.equal(index.status, 200, 'Host must survive and serve the application after every rejected target');
        assert.ok(index.body.includes('Estuary dispatch board'));
        checks.push({ target, rejectedStatus: malformed.status, subsequentIndexStatus: index.status });
    }
    for (const profile of ['Testing', 'Production']) {
        const response = await request(`/public-settings.json?profile=${profile}`);
        assert.equal(response.status, 200);
        const settings = JSON.parse(response.body);
        assert.equal(settings.environment, profile);
        assert.equal(settings.variables.Board__Endpoint, `https://${profile.toLowerCase()}.estuary.invalid/dispatch`);
    }
    const sha = file => createHash('sha256').update(fs.readFileSync(file)).digest('hex');
    const result = { major, node: process.version, recordedAt: new Date().toISOString(), serverSha256: sha(server), verifierSha256: sha(fileURLToPath(import.meta.url)), checks, publicProfiles: ['Testing', 'Production'], scope: 'Actual Node HTTP listener and raw request targets; no mock listener or browser execution claim.' };
    if (process.argv[3]) fs.writeFileSync(process.argv[3], JSON.stringify(result, null, 2) + '\n');
    console.log(`PASS .NET ${major} fixture: malformed paths return 400; listener survives; both public profiles remain available.`);
} catch (error) {
    throw new Error(`${error.message}\nFixture diagnostics: ${diagnostics}`, { cause: error });
} finally {
    if (child.exitCode === null && child.signalCode === null) child.kill();
    await exited;
}
