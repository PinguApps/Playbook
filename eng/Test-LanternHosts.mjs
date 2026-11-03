import { createRequire } from 'node:module';
import { createServer } from 'node:http';
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { resolve, dirname, extname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';
const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const playwright = 'C:/Users/pingu/AppData/Local/npm-cache/_npx/e41f203b7505f1fb/node_modules/playwright-core';
const executablePath = 'C:/Users/pingu/AppData/Local/ms-playwright/chromium-1248/chrome-win64/chrome.exe';
const { chromium } = createRequire(import.meta.url)(playwright);
const browser = await chromium.launch({ executablePath, headless: true });
const results = [];
try {
 for (const [target,flavor] of [['net10.0','Mono'],['net11.0','Mono'],['net11.0','CoreCLR']]) {
  const directory = resolve(root, `samples/${target}/LanternHost${flavor}/bin/Release/${target}/publish/wwwroot`);
  for (const mode of ['keep','exit','missing','diagnostics','legacy-config']) {
   const requests=[];
   const server = createServer(async (request,response) => {
    try {
     const path = decodeURIComponent(new URL(request.url,'http://localhost').pathname);
     requests.push(path);
     const file=resolve(directory,'.'+(path==='/'?'/index.html':path));
     if (!file.startsWith(directory+sep)) { response.writeHead(403).end();return; }
     if (mode==='missing' && path.includes('dotnet.native') && path.endsWith('.wasm')) { response.writeHead(404).end();return; }
     const data=await readFile(file);
     response.setHeader('Content-Type',({'.html':'text/html','.js':'text/javascript','.json':'application/json','.wasm':'application/wasm'})[extname(file)]||'application/octet-stream');
     response.end(data);
    } catch {response.writeHead(404).end();}
   });
   await new Promise(done=>server.listen(0,'127.0.0.1',done));
   const page=await browser.newPage(); const messages=[];
   page.on('console',m=>messages.push(m.text()));page.on('pageerror',e=>messages.push(String(e)));
   try {
    await page.goto(`http://127.0.0.1:${server.address().port}/?mode=${mode}`);
    await page.waitForFunction(()=>globalThis.proof,undefined,{timeout:60000});
    const proof=await page.evaluate(()=>globalThis.proof);
    console.log(target,flavor,mode,JSON.stringify({...proof,resources:proof.resources.length}));
    if(mode==='missing' || (mode==='legacy-config' && target==='net10.0')) assert.ok(proof.error,'failed resource must reject startup');
    else {
     assert.ok(!proof.error,JSON.stringify({proof,messages}));assert.equal(proof.code,17);
     assert.equal(proof.before,'not-started');
     if(mode==='keep' || mode==='diagnostics' || mode==='legacy-config') {assert.equal(proof.after,'quay:report|east');assert.equal(proof.rejected,false);}
     else assert.equal(proof.rejected,true,'exit must reject exported calls');
     assert.ok(proof.resources.length>0,'custom loader called');
     if(mode==='legacy-config') assert.ok(!requests.some(path=>path.includes('legacy-missing.json')),'current withConfigSrc is ignored, not a configuration override');
     if(mode==='diagnostics') { assert.ok(proof.diagnostics.bytes > 0,JSON.stringify(proof.diagnostics)); assert.equal(proof.diagnostics.providerFound,true,'actual trace contains selected provider'); }
    }
    results.push({target,flavor,mode,proof,requests,messages});
   } finally {await page.close();await new Promise(done=>server.close(done));}
  }
 }
 await mkdir(resolve(root,'artifacts/lantern-hosts'),{recursive:true});
 await writeFile(resolve(root,'artifacts/lantern-hosts/browser.json'),JSON.stringify({browser:browser.version(),executablePath,results},null,2)+'\n');
} finally {await browser.close();}
