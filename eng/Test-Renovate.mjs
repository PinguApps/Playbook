import assert from 'node:assert/strict';
import { execFileSync } from 'node:child_process';
import fs from 'node:fs/promises';
import { createRequire } from 'node:module';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

// Pass an installed Renovate package directory, not its node_modules parent.
const renovateRoot = process.argv[2];
assert(renovateRoot && path.isAbsolute(renovateRoot), 'Pass the absolute installed Renovate package directory.');
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
// Reuse the YAML parser from the pinned policy tool; do not introduce another dependency.
const { parse: parseYaml } = createRequire(path.join(renovateRoot, 'package.json'))('yaml');
const workflows = {};
for (const name of ['publish-release-draft', 'publish', '_run-tests', 'pr-validation']) {
  workflows[name] = parseYaml(await fs.readFile(path.join(root, '.github', 'workflows', `${name}.yml`), 'utf8'));
}
const verificationPermissions = { contents: 'read', actions: 'read', checks: 'write', 'pull-requests': 'write' };
const publishingPermissions = { contents: 'read', 'id-token': 'write' };
const ranks = { none: 0, read: 1, write: 2 };
function checkWorkflowCall(callerName, jobName, calleeName) {
  const caller = workflows[callerName].jobs[jobName];
  const callee = workflows[calleeName];
  assert.equal(caller.uses, `./.github/workflows/${calleeName}.yml`);
  assert(callee.on.workflow_call, `${calleeName} must remain reusable`);
  for (const [name, job] of Object.entries(callee.jobs)) {
    for (const [permission, level] of Object.entries(job.permissions ?? callee.permissions)) {
      const allowed = caller.permissions[permission] ?? 'none';
      assert(ranks[allowed] >= ranks[level],
        `${callerName}.${jobName} -> ${calleeName}.${name}: requests ${permission}: ${level}, caller allows ${allowed}`);
    }
  }
}
// GitHub validates nested permissions even when the publishing jobs will be skipped.
checkWorkflowCall('publish-release-draft', 'publish_packages', 'publish');
checkWorkflowCall('publish', 'verify', '_run-tests');
checkWorkflowCall('pr-validation', 'tests', '_run-tests');
assert.deepEqual(workflows['publish-release-draft'].jobs.publish_packages.permissions,
  { ...verificationPermissions, 'id-token': 'write' });
assert.deepEqual(workflows.publish.jobs.verify.permissions, verificationPermissions);
assert.deepEqual(workflows['pr-validation'].jobs.tests.permissions, verificationPermissions);
assert.deepEqual(workflows['_run-tests'].permissions, verificationPermissions);
assert.deepEqual(workflows.publish.permissions, { contents: 'read' });
assert.deepEqual(workflows.publish.jobs.publish.permissions, publishingPermissions);
assert.deepEqual(workflows.publish.on.release.types, ['published']);
assert.equal(workflows.publish.on.workflow_dispatch.inputs.tag.required, true);
assert.equal(workflows.publish.on.workflow_call.inputs.tag.required, true);
assert.equal(workflows['publish-release-draft'].jobs.publish_draft.if, "vars.ENABLE_WEEKLY_PUBLISH == 'true'");
assert.equal(workflows['publish-release-draft'].jobs.publish_packages.if, "needs.publish_draft.outputs.tag != ''");
console.log('PASS: reusable workflow permission chains, job-level OIDC and existing publication gates.');

const load = (name) => import(pathToFileURL(path.join(renovateRoot, 'dist', name)).href);
const { init } = await load('logger/index.js');
await init();
const { getConfig } = await load('config/defaults.js');
const { GlobalConfig } = await load('config/global.js');
const { massageConfig } = await load('config/massage.js');
const { resolveConfigPresets } = await load('config/presets/index.js');
const { mergeChildConfig } = await load('config/utils.js');
const { validateConfig } = await load('config/validation.js');
const { applyPackageRules } = await load('util/package-rules/index.js');
const { generateBranchConfig } = await load('workers/repository/updates/generate.js');
const { api: nugetVersions } = await load('modules/versioning/nuget/index.js');
GlobalConfig.set({ ...getConfig(), platform: 'github', endpoint: 'https://api.github.com/', localDir: root });
if (process.env.GITHUB_COM_TOKEN) {
  const { add } = await load('util/host-rules.js');
  add({ hostType: 'github', matchHost: 'api.github.com', token: process.env.GITHUB_COM_TOKEN });
}

const repo = JSON.parse(await fs.readFile(path.join(root, 'renovate.json'), 'utf8'));
assert(repo.extends.includes('local>PinguApps/renovate-config'));
const resolved = await resolveConfigPresets(massageConfig(repo));
assert(resolved.visitedPresets.merged.includes('local>PinguApps/renovate-config'));
const validation = await validateConfig('repo', resolved.config);
assert.deepEqual(validation.errors, []);
assert.deepEqual(validation.warnings, []);
const config = mergeChildConfig(getConfig(), resolved.config);
assert.deepEqual(config.enabledManagers.toSorted(), ['github-actions', 'nuget']);
assert.equal(config.configMigration, false);
assert.equal(config.lockFileMaintenance.enabled, false);
assert.equal(config.vulnerabilityAlerts.enabled, false);
assert.equal(config.osvVulnerabilityAlerts, false);
assert(config.ignorePaths.includes('**/.agents/**'), 'Retain the shared preset exclusions.');
assert(config.reviewers.includes('pingu2k4'));

const reviewed = new Map([
  ['Microsoft.CodeAnalysis.BannedApiAnalyzers', 5],
  ['Microsoft.Extensions.Hosting', 10],
  ['Microsoft.NET.Test.Sdk', 18],
  ['Microsoft.SourceLink.GitHub', 10],
  ['ModelContextProtocol', 2],
  ['PinguApps.CodeStyle.Analyzers', 1],
  ['Reqnroll.xunit.v3', 3],
  ['xunit.v3.mtp-off', 4],
  ['xunit.runner.visualstudio', 4]
]);
async function policy(dep, extra = {}) {
  return applyPackageRules({ ...config, ...dep, packageName: dep.packageName ?? dep.depName, ...extra });
}
async function expectEnabled(dep, enabled, extra = {}) {
  const result = await policy(dep, extra);
  assert.equal(result.enabled !== false, enabled, `${dep.packageFile}: ${dep.depName} (${extra.updateType ?? 'lookup'})`);
  assert.equal(result.automerge, false);
  assert.equal(result.ignoreUnstable, true);
  assert.equal(result.ignoreDeprecated, true);
  return result;
}

// Discover with the actual managers; default-disabled dependencies must remain disabled
// before version lookup, as well as when update-type rules are applied afterwards.
const git = process.env.PLAYBOOK_GIT_PATH;
assert(git && path.isAbsolute(git), 'Set PLAYBOOK_GIT_PATH to the absolute Git executable.');
const files = execFileSync(git, ['-C', root, 'ls-files', '--cached', '--others', '--exclude-standard', '-z'], { encoding: 'utf8' }).split('\0').filter(Boolean);
const { extractPackageFile: nuget } = await load('modules/manager/nuget/extract.js');
const { extractPackageFile: actions } = await load('modules/manager/github-actions/extract.js');
const dependencies = [];
for (const file of files) {
  let manager;
  let extracted;
  if (/\.(?:cs|fs|vb)proj$|\.(?:props|targets)$|(?:^|\/)(?:global|dotnet-tools)\.json$/.test(file)) {
    manager = 'nuget';
    extracted = await nuget(await fs.readFile(path.join(root, file), 'utf8'), file);
  } else if (/^\.github\/workflows\/.*\.ya?ml$/.test(file)) {
    manager = 'github-actions';
    extracted = await actions(await fs.readFile(path.join(root, file), 'utf8'), file);
  } else continue;
  for (const dep of extracted?.deps ?? []) dependencies.push({ ...dep, manager, packageFile: file });
}
const roots = dependencies.filter((dep) => dep.packageFile === 'Directory.Packages.props');
for (const [name, major] of reviewed) {
  const dep = roots.find((candidate) => candidate.depName === name);
  assert(dep, `Remove stale update permission after removing ${name}.`);
  const result = await expectEnabled(dep, true);
  assert(nugetVersions.matches(dep.currentValue, result.allowedVersions), `Review ${name}'s changed release line.`);
  assert(nugetVersions.matches(`${major}.99.0`, result.allowedVersions));
  assert(!nugetVersions.matches(`${major + 1}.0.0`, result.allowedVersions));
  assert.equal(result.skipArtifactsUpdate, true);
  for (const updateType of ['minor', 'patch']) await expectEnabled(dep, true, { updateType });
}
for (const dep of roots.filter((candidate) => !reviewed.has(candidate.depName))) await expectEnabled(dep, false);
const samples = dependencies.filter((dep) => dep.packageFile.startsWith('samples/'));
for (const dep of samples) await expectEnabled(dep, false);
for (const dep of dependencies.filter((candidate) => candidate.datasource === 'dotnet-version')) await expectEnabled(dep, false);
const actionRefs = dependencies.filter((dep) => dep.depType === 'action');
for (const dep of actionRefs) {
  const result = await expectEnabled(dep, dep.depName !== 'harupy/auto-labeling');
  if (result.enabled) assert.equal(result.pinDigests, true);
}

// Future repository shapes from the backlog: providers, native workloads, file apps,
// containers, tools and moved/shared projects must not acquire update permission.
const rootDep = roots.find((dep) => dep.depName === 'ModelContextProtocol');
for (const packageFile of ['samples/net10.0/Future/Directory.Packages.props', 'samples/net11.0/Preview/Future.csproj', 'src/Future/Future.csproj', 'src/Moved/Directory.Packages.props', 'Directory.Build.props', '.config/dotnet-tools.json', 'tools/Utility.cs']) {
  await expectEnabled({ ...rootDep, packageFile }, false);
}
for (const depName of ['Microsoft.EntityFrameworkCore', 'Microsoft.Data.Sqlite', 'Aspire.Hosting', 'Microsoft.Maui.Controls', 'FSharp.Compiler.Service', 'Future.Dependency']) {
  await expectEnabled({ ...rootDep, depName, packageName: depName }, false);
}
for (const currentValue of ['0.9.0', '[0.9.0]', '2.3.0-preview.1', '2.3.0-rc.2']) {
  await expectEnabled(rootDep, false, { currentValue });
}
for (const currentValue of ['[2.2.0]', '[2.0.0,3.0.0)', '2.*']) await expectEnabled(rootDep, false, { currentValue });
await expectEnabled(rootDep, false, { depType: 'msbuild-sdk' });
const action = actionRefs.find((dep) => dep.depName === 'actions/checkout');
assert(action);
for (const currentValue of ['main', 'master', 'latest', 'next', 'v0']) await expectEnabled(action, false, { currentValue });
await expectEnabled({ ...action, packageFile: '.github/workflows/future.yml' }, false);
await expectEnabled({ ...action, depName: 'future-owner/action', packageName: 'future-owner/action' }, false);
for (const depType of ['github-runner', 'uses-with', 'container', 'service', 'workflow']) await expectEnabled(action, false, { depType });
for (const manager of ['npm', 'dockerfile', 'devcontainer', 'custom.regex', 'renovate-config']) {
  await expectEnabled({ ...rootDep, manager }, false);
}
for (const dep of [...roots, ...actionRefs]) {
  for (const updateType of ['major', 'replacement', 'rollback', 'bump', 'lockFileMaintenance']) await expectEnabled(dep, false, { updateType });
}

const testNames = ['Microsoft.NET.Test.Sdk', 'Reqnroll.xunit.v3', 'xunit.v3.mtp-off', 'xunit.runner.visualstudio'];
const testUpgrades = [];
for (const dep of roots.filter((candidate) => testNames.includes(candidate.depName))) {
  const result = await expectEnabled(dep, true, { updateType: 'patch', currentVersion: dep.currentValue, newVersion: dep.currentValue, newValue: dep.currentValue });
  assert.equal(result.groupName, 'test dependencies');
  testUpgrades.push(result);
}
assert.equal(generateBranchConfig(testUpgrades).skipArtifactsUpdate, true);
console.log(`PASS: resolved shared preset; ${dependencies.length} extracted dependencies; ${samples.length} protected sample entries; future-project/package/update-type policy cases; grouped artifact exemption.`);
