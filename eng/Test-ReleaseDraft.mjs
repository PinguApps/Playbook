// Run via Test-ReleaseDraft.ps1. Uses upstream source at the workflow's exact pin.
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { createRequire } from 'node:module';

const root = resolve(import.meta.dirname, '..');
const source = resolve(process.argv[2]);
const require = createRequire(resolve(source, 'package.json'));
const { parse } = require('yaml');
const load = (path) => import(pathToFileURL(resolve(source, 'packages/core/src', path)));
const { configSchema } = await load('config/config.schema.ts');
const { mergeInputAndConfig } = await load('config/merge-input-and-config.ts');
const { categorizePullRequests } = await load('release/categorize-pull-requests.ts');
const { generateChangeLog } = await load('release/generate-changelog.ts');
const { resolveVersionKeyIncrement } = await load('release/resolve-version-increment.ts');
const { getVersionInfo } = await load('release/get-version-info.ts');
const { noopLogger: logger } = await load('ports.ts');
const config = mergeInputAndConfig({
  config: configSchema.parse(parse(readFileSync(resolve(root, '.github/release-drafter.yml'), 'utf8'))),
  input: {}, defaultCommitish: 'main', logger,
});
const cases = [
  ['docs', 'patch', 'Documentation', '1.2.4'],
  ['feature', 'minor', 'Features', '1.3.0'],
  ['feature', 'major', 'Features', '2.0.0'],
  ['bug', 'patch', 'Bug Fixes', '1.2.4'],
  ['security', 'patch', 'Security', '1.2.4'],
  ['dependencies', 'patch', 'Dependencies', '1.2.4'],
  ['meta', undefined, 'Meta', '1.2.4'],
];
const version = (pullRequests, input = {}, lastRelease = { tagName: 'v1.2.3' }) => {
  const versionKeyIncrement = resolveVersionKeyIncrement({ pullRequests, config, logger });
  return getVersionInfo({ lastRelease, config, input, versionKeyIncrement, logger }).$RESOLVED_VERSION;
};
for (const [category, increment, section, expected] of cases) {
  const pullRequests = [{ number: 42, title: 'Example change', author: { login: 'example' }, labels: [category, increment].filter(Boolean) }];
  const [, groups] = categorizePullRequests({ pullRequests, config });
  assert(groups.some(group => group.title.includes(section) && group.pullRequests.length === 1));
  const markdown = generateChangeLog({ pullRequests, config, serverUrl: 'https://github.com' });
  assert(markdown.includes(section) && markdown.includes('#42') && markdown.includes('Example change'));
  assert.equal(version(pullRequests), expected);
  console.log(`${category}/${increment ?? 'default'} -> ${section}, ${expected}`);
}
assert.equal(version([{ number: 1, title: 'Mixed', labels: ['patch', 'minor', 'major'] }]), '2.0.0');
assert.equal(version([
  { number: 1, title: 'Correction', labels: ['patch'] },
  { number: 2, title: 'Addition', labels: ['minor'] },
]), '1.3.0');
assert.equal(version([], { version: '' }), '1.2.4');
assert.equal(version([], { version: '1.0.0' }, undefined), '1.0.0');
assert.equal(version([], { version: '1.0.0-preview.1' }, undefined), '1.0.0-preview.1');
console.log('PASS: pinned Release Drafter categories, Markdown, precedence, stable and manual preview versions');
