import { createRequire } from 'node:module';
import { spawn } from 'node:child_process';
import { mkdir, mkdtemp, readdir, rm, writeFile } from 'node:fs/promises';
import { dirname, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const [playwright, executablePath, net10, net11, publication, output] = process.argv.slice(2);
if (!output) throw new Error('Supply Playwright, Chromium, both dotnet hosts, publication directory and report path.');
const { chromium } = createRequire(import.meta.url)(playwright);
const ownedRoot = resolve(root, 'artifacts/volunteer-browser');
await mkdir(ownedRoot, { recursive: true });
const browser = await chromium.launch({ executablePath, headless: true });
const results = [];
try {
  for (const [target, name, dotnet] of [['10.0', 'volunteer10', net10], ['11.0', 'volunteer11', net11], ['11.0', 'volunteer11-default', net11]]) {
    const temporary = await mkdtemp(resolve(ownedRoot, 'fixture-'));
    const directory = resolve(publication, name);
    const child = spawn(dotnet, [resolve(directory, 'VolunteerAccess.dll'), '--serve', 'true'], {
      cwd: directory,
      env: { ...process.env, TMP: temporary, TEMP: temporary, DOTNET_ADD_GLOBAL_TOOLS_TO_PATH: '0', DOTNET_GENERATE_ASPNET_CERTIFICATE: 'false' },
      windowsHide: true,
    });
    let logs = '';
    child.stdout.on('data', data => { logs += data; });
    child.stderr.on('data', data => { logs += data; });
    try {
      const login = await new Promise((accept, reject) => {
        const timeout = setTimeout(() => { clearInterval(poll); reject(new Error(`Startup timed out: ${logs}`)); }, 60000);
        const poll = setInterval(() => {
          const match = logs.match(/VOLUNTEER-URL (http:\/\/127\.0\.0\.1:\d+\/Identity\/Account\/Login)/);
          if (match) { clearInterval(poll); clearTimeout(timeout); accept(match[1]); }
          else if (child.exitCode !== null) { clearInterval(poll); clearTimeout(timeout); reject(new Error(`Host exited: ${logs}`)); }
        }, 50);
      });
      assert.ok((await readdir(temporary)).some(path => path.startsWith('volunteer-access-')), 'Fixture state must stay in its owned temporary directory.');
      const context = await browser.newContext();
      try {
        const page = await context.newPage();
        const errors = [];
        page.on('pageerror', error => errors.push(String(error)));
        await page.goto(login);
        assert.equal(await page.title(), 'Bracken Rescue volunteers');
        assert.equal(await page.locator('#login-title').innerText(), 'Volunteer sign-in');
        for (const width of [390, 1280]) {
          await page.setViewportSize({ width, height: 800 });
          assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `Horizontal clipping at ${width}px`);
          const panel = await page.locator('.volunteer-panel').boundingBox();
          assert.ok(panel && panel.width > 200 && panel.x >= 0 && panel.x + panel.width <= width);
        }
        for (const path of ['/css/volunteers.css', '/Identity/lib/bootstrap/dist/css/bootstrap.min.css']) {
          const response = await context.request.get(new URL(path, login).href);
          assert.equal(response.status(), 200);
          assert.ok((await response.body()).length > 100);
        }
        await page.locator('#Input_Email').fill('not-an-email');
        await page.getByRole('button', { name: 'Sign in', exact: true }).click();
        await page.locator('#email-error').waitFor({ state: 'visible' });
        assert.match(await page.locator('#email-error').innerText(), /valid e-mail/);
        assert.match(await page.locator('#password-error').innerText(), /Password is required/);
        await page.goto(login);
        await page.locator('#Input_Email').fill('pending@bracken.example');
        await page.locator('#Input_Password').fill('Bracken.Rota42!');
        await page.getByRole('button', { name: 'Sign in', exact: true }).click();
        await page.locator('[role=alert]').waitFor({ state: 'visible' });
        assert.match(await page.locator('[role=alert]').innerText(), /confirmation/);
        assert.equal(await (await context.request.get(new URL('/session', login).href)).text(), 'anonymous');
        await page.goto(login + '?returnUrl=https%3A%2F%2Funtrusted.invalid');
        await page.locator('#Input_Email').fill('ready@bracken.example');
        await page.locator('#Input_Password').fill('Bracken.Rota42!');
        await Promise.all([page.waitForURL(new URL('/session', login).href), page.getByRole('button', { name: 'Sign in', exact: true }).click()]);
        assert.equal(await page.locator('body').innerText(), 'signed-in');
        assert.deepEqual(errors, []);
        results.push({ target, publication: name, viewports: [390, 1280], assets: 'passed', validation: 'passed', unconfirmed: 'denied', confirmed: 'signed-in', externalReturn: 'rejected', pageErrors: errors });
      } finally { await context.close(); }
    } finally {
      if (child.exitCode === null) {
        const exited = new Promise(accept => child.once('exit', accept));
        child.kill();
        await exited;
      }
      assert.ok(temporary.startsWith(ownedRoot + sep) && dirname(temporary) === ownedRoot, 'Cleanup must remain inside the owned fixture directory.');
      await rm(temporary, { recursive: true, force: true });
    }
  }
  await mkdir(dirname(resolve(output)), { recursive: true });
  await writeFile(output, JSON.stringify({ browser: browser.version(), results }, null, 2) + '\n');
  console.log('PASS three real published VolunteerAccess browser flows; private fixture processes and data removed.');
} finally { await browser.close(); }
