import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { createServer } from 'node:http';
import { mkdtemp, mkdir, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const [dotnet, net10Dotnet, outputPath] = process.argv.slice(2);
assert(dotnet && path.isAbsolute(dotnet), 'Supply an absolute dotnet executable.');
assert(outputPath && path.isAbsolute(outputPath), 'Supply an absolute result path.');
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const home = await mkdtemp(path.join(tmpdir(), 'playbook-cli-telemetry-'));
const events = [];
// Fail if the canonical wrapper's port is occupied; never reuse another service.
const server = createServer(async (request, response) => {
  const chunks = [];
  for await (const chunk of request) chunks.push(chunk);
  const body = Buffer.concat(chunks);
  events.push({ path: request.url, bytes: body.length, service: body.includes(Buffer.from('dotnet-cli')) });
  response.writeHead(200, { 'content-type': 'application/x-protobuf' });
  response.end();
});
await new Promise((resolve, reject) => {
  server.once('error', reject);
  server.listen(4318, '127.0.0.1', resolve);
});
const results = [];
try {
  for (const [release, mode] of [['10', 'opt-out'], ['11', 'opt-out'], ['11', 'export']]) {
    const env = { ...process.env };
    for (const key of Object.keys(env)) if (key.startsWith('OTEL_')) delete env[key];
    Object.assign(env, {
      DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', DOTNET_GENERATE_ASPNET_CERTIFICATE: 'false',
      DOTNET_CLI_HOME: home, DOTNET_CLI_TELEMETRY_DISABLE_TRACE_EXPORT: 'true',
      DOTNET_CLI_TELEMETRY_STORAGE_PATH: path.join(home, 'storage'),
    });
    const directory = path.join(root, `samples/net${release}.0/CliTelemetry`);
    const hostDotnet = release === '10' ? net10Dotnet : dotnet;
    const args = [path.join(directory, `bin/Release/net${release}.0/CliTelemetry.dll`), hostDotnet];
    if (mode === 'export') args.push('export');
    events.length = 0;
    const output = await new Promise((resolve, reject) => {
      const child = spawn(hostDotnet, args, { cwd: directory, env, windowsHide: true });
      let text = '';
      child.stdout.on('data', chunk => { text += chunk; });
      child.stderr.on('data', chunk => { text += chunk; });
      const timeout = setTimeout(() => { child.kill(); reject(new Error('Telemetry wrapper timed out.')); }, 30000);
      child.once('error', error => { clearTimeout(timeout); reject(error); });
      child.once('close', code => {
        clearTimeout(timeout);
        if (code !== 0) reject(new Error(`Wrapper failed (${code}): ${text}`));
        else resolve(text);
      });
    });
    assert(output.includes(release === '10' ? '10.0.403' : '11.0.100'), 'Unexpected selected SDK.');
    if (mode === 'export') {
      for (const endpoint of ['/v1/traces', '/v1/metrics']) {
        assert(events.some(event => event.path === endpoint && event.bytes > 0 && event.service), `Missing SDK payload at ${endpoint}.`);
      }
    } else assert.equal(events.length, 0, 'Opt-out wrapper exported to the collector.');
    results.push({ release, mode, events: [...events] });
  }
} finally {
  server.closeAllConnections();
  await new Promise(resolve => server.close(resolve));
}
await mkdir(path.dirname(outputPath), { recursive: true });
await writeFile(outputPath, JSON.stringify({ results, scope: 'Local OTLP HTTP receipt only; no global network absence, Azure Monitor behavior, retry or production collector guarantee.' }, null, 2) + '\n');
console.log('PASS: paired opt-out controls; GA wrapper delivered traces and metrics to the local OTLP endpoint.');
