// Real installed stdio inspection; no production-code import or source fallback.
import { spawn } from 'node:child_process';
import { createInterface } from 'node:readline';
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { resolve, dirname } from 'node:path';
import assert from 'node:assert/strict';

const [tool, root, net10, net11] = process.argv.slice(2);
if (!tool || !root || !net10 || !net11) throw new Error('Supply installed executable, repository and matching dotnet paths.');
const output = resolve(root, 'artifacts/component-actions/inspection');
await mkdir(output, { recursive: true });
const child = spawn(tool, ['mcp'], { cwd: process.cwd(), windowsHide: true,
  env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', HTTP_PROXY: 'http://127.0.0.1:1', HTTPS_PROXY: 'http://127.0.0.1:1', ALL_PROXY: 'http://127.0.0.1:1' } });
const pending = new Map();
let sequence = 0;
let stderr = '';
child.stderr.on('data', data => stderr += data);
createInterface({ input: child.stdout }).on('line', line => {
  const message = JSON.parse(line);
  const resolveMessage = pending.get(message.id);
  if (resolveMessage) { pending.delete(message.id); resolveMessage(message); }
});
child.on('exit', code => { for (const resolveMessage of pending.values()) resolveMessage({ error: { message: `Server exited ${code}: ${stderr}` } }); });
async function request(method, params) {
  const id = ++sequence;
  let timer;
  const reply = await Promise.race([
    new Promise(done => { pending.set(id, done); child.stdin.write(JSON.stringify({ jsonrpc: '2.0', id, method, params }) + '\n'); }),
    new Promise((_, reject) => { timer = setTimeout(() => reject(new Error(`Timed out: ${method}`)), 30000); })
  ]).finally(() => clearTimeout(timer));
  if (reply.error) throw new Error(JSON.stringify(reply.error));
  return reply.result;
}
async function call(name, args) {
  const result = await request('tools/call', { name, arguments: { ...args, targetVersion: '11.0' } });
  assert.ok(!result.isError, JSON.stringify(result));
  const text = result.content.filter(item => item.type === 'text').map(item => item.text).join('');
  assert.deepEqual(JSON.parse(text), result.structuredContent);
  return result;
}
try {
  await request('initialize', { protocolVersion: '2025-06-18', capabilities: {}, clientInfo: { name: 'component-actions-inspection', version: '1.0.0' } });
  child.stdin.write(JSON.stringify({ jsonrpc: '2.0', method: 'notifications/initialized' }) + '\n');
  assert.deepEqual((await request('tools/list', {})).tools.map(t => t.name).sort(), ['get_recipe', 'list_recipes']);
  const entries = [];
  let offset = 0;
  do {
    const page = (await call('list_recipes', { offset, limit: 50 })).structuredContent;
    entries.push(...page.recipes);
    offset = page.nextOffset;
  } while (offset !== null && offset !== undefined);
  let reconstructed;
  for (const id of ['blazor.remove-redundant-render-notifications', 'blazor.capture-loop-items-safely', 'blazor.expose-js-invokable-callbacks']) {
    const meta = JSON.parse(await readFile(resolve(root, `content/recipes/${id}/11.0/recipe.json`), 'utf8'));
    const discovery = entries.filter(entry => entry.id === id);
    assert.equal(discovery.length, 1);
    assert.equal(discovery[0].title, meta.title);
    assert.equal(discovery[0].goal, meta.goal);
    console.log(JSON.stringify({ id, title: meta.title, goal: meta.goal }));
    const result = await call('get_recipe', { id });
    const recipe = result.structuredContent.recipe;
    assert.equal(recipe.edition, '11.0');
    assert.equal(recipe.markdown.includes('{{snippet:'), false);
    assert.deepEqual(recipe.applicability, meta.applicability);
    assert.equal(recipe.evidence.length, 4);
    assert.ok(recipe.evidence.every(e => e.freshness === 'current'));
    for (const snippet of Object.values(meta.snippets)) {
      const bytes = await readFile(resolve(root, snippet.path), 'utf8');
      const match = bytes.match(/(?:\/\/ snippet:complete|<!-- snippet:complete -->)\r?\n([\s\S]*?)(?:\/\/ endsnippet:complete|<!-- endsnippet:complete -->)/);
      assert.ok(match, snippet.path);
      assert.ok(recipe.markdown.includes(match[1].trimEnd()), `Canonical snippet mismatch: ${snippet.path}`);
    }
    console.log(recipe.markdown);
    console.log(JSON.stringify({ applicability: recipe.applicability, evidence: recipe.evidence, humanProof: recipe.humanProof }, null, 2));
    await writeFile(resolve(output, `${id}.json`), JSON.stringify({ result }, null, 2) + '\n');
    const blocks = [...recipe.markdown.matchAll(/^### \.NET (10|11): `([^`]+)`\n\n```[^\n]*\n([\s\S]*?)^```$/gm)];
    assert.equal(blocks.length, 32);
    // Compare all three independent returned bodies before reusing one identical consumer.
    const serialized = JSON.stringify(blocks.map(b => [b[1], b[2], b[3]]));
    if (reconstructed) assert.equal(serialized, reconstructed);
    else {
      reconstructed = serialized;
      for (const block of blocks) {
        assert.match(block[2], /^(?:Before\/|wwwroot\/)?[A-Za-z][A-Za-z0-9.-]*$/);
        const file = resolve(process.cwd(), `consumer/net${block[1]}.0/WillowQuay`, block[2]);
        await mkdir(dirname(file), { recursive: true });
        await writeFile(file, block[3]);
      }
    }
  }
  for (const [version, dotnet] of [['10.0', net10], ['11.0', net11]]) {
    const directory = resolve(process.cwd(), `consumer/net${version}/WillowQuay`);
    const build = spawn(dotnet, ['build', '-c', 'Release'], { cwd: directory, windowsHide: true, env: { ...process.env, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0' } });
    let log = '';
    build.stdout.on('data', data => log += data);
    build.stderr.on('data', data => log += data);
    assert.equal(await new Promise(done => build.on('exit', done)), 0, log);
    const assets = JSON.parse(await readFile(resolve(directory, 'obj/project.assets.json'), 'utf8'));
    const expected = JSON.parse(await readFile(resolve(root, `samples/net${version}/WillowQuay/verification.json`), 'utf8')).dependencies;
    assert.deepEqual(Object.keys(assets.libraries).sort(), expected);
    console.log(`PASS: returned-body net${version} consumer compiles with exact dependency closure.`);
  }
  console.log('PASS: three installed offline responses, discovery, canonical files, applicability, evidence and both returned-body consumers. Network proxies blocked; this is not OS network isolation.');
} finally { child.stdin.end(); child.kill(); }
