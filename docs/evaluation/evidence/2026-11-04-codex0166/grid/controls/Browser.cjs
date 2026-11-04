// Disposable Chromium CDP check. No model calls; submissions remain unchanged.
const {spawn} = require('node:child_process');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const chrome = 'C:/Users/pingu/AppData/Local/ms-playwright/chromium-1248/chrome-win64/chrome.exe';
const sleep = ms => new Promise(r => setTimeout(r, ms));
async function main() {
 const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'grid-browser-'));
 const proc = spawn(chrome, ['--headless=new','--no-proxy-server','--no-first-run','--no-default-browser-check','--remote-debugging-port=0',`--user-data-dir=${profile}`,'about:blank'], {stdio:'ignore',windowsHide:true});
 try {
  const active=path.join(profile,'DevToolsActivePort');
  for(let i=0;i<100&&!fs.existsSync(active);i++) await sleep(100);
  const port=fs.readFileSync(active,'utf8').split('\n')[0];
  const version=await (await fetch(`http://127.0.0.1:${port}/json/version`)).json();
  const tab=(await (await fetch(`http://127.0.0.1:${port}/json/list`)).json()).find(t=>t.type==='page'&&t.url==='about:blank');
  const ws=new WebSocket(tab.webSocketDebuggerUrl); await new Promise((r,j)=>{ws.onopen=r;ws.onerror=j});
  let id=0;const pending=new Map();
  ws.onmessage=e=>{const m=JSON.parse(e.data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error(JSON.stringify(m.error))):p.resolve(m.result)}};
  const call=(method,params={})=>new Promise((resolve,reject)=>{const n=++id;pending.set(n,{resolve,reject});ws.send(JSON.stringify({id:n,method,params}))});
  const evaluate=async expression=>{const r=await call('Runtime.evaluate',{expression,returnByValue:true});if(r.exceptionDetails)throw Error(JSON.stringify(r.exceptionDetails));return r.result.value};
  await sleep(1000); await call('Page.enable'); await call('Emulation.setScriptExecutionDisabled',{value:true});
  const assert=(ok,label)=>{if(!ok)throw Error(label);console.log('PASS: '+label)};
  const state=()=>evaluate(`JSON.stringify({url:location.href,north:[...document.querySelectorAll('#north td')].map(x=>x.textContent.trim()).filter(x=>/^N[1-4X]$/.test(x)),south:[...document.querySelectorAll('#south td')].map(x=>x.textContent.trim()).filter(x=>/^S[1-4X]$/.test(x)),text:document.body.innerText})`).then(JSON.parse);
  const wait=async predicate=>{await sleep(150);for(let i=0;i<100;i++){try{const s=await state();if(predicate(s))return s}catch(e){if(i===99)console.error(e.message)}await sleep(50)}throw Error('Navigation did not reach expected state: '+JSON.stringify(await state()))};
  const navigate=async url=>{console.log(JSON.stringify(await call('Page.navigate',{url})));return wait(s=>s.url===url&&s.north.length===2)};
  console.log(JSON.stringify({browser:version.Browser,javaScriptDisabled:true}));
  for(const base of process.argv.slice(2)) {
   const bookmark=base+'?filter=open%20water&north.page=2&north.sort=units&north.direction=asc&south.page=1&south.sort=code&south.direction=desc';
   let s=await navigate(bookmark);assert(s.north.join()==='N1,N3'&&s.south.join()==='S4,S3','direct asymmetric bookmark '+base);
   await call('Page.reload');s=await wait(s=>s.north.join()==='N1,N3');assert(s.south.join()==='S4,S3','refresh restores both');
   for(const scope of ['north','south'])for(const [key,value] of [['page',scope==='north'?'1':'2'],['direction',scope==='north'?'desc':'asc']]){
    await navigate(bookmark);
    const url=await evaluate(`(()=>{const a=[...document.querySelectorAll('#${scope} a')].find(a=>(new URL(a.href).searchParams.get('${scope}.${key}')||('${key}'==='page'?'1':null))==='${value}' && ('${key}'!=='direction'||new URL(a.href).searchParams.get('${scope}.sort')===(new URL(location.href).searchParams.get('${scope}.sort')||'code')));if(!a)throw Error('Missing anchor');const u=a.href;a.click();return u})()`);
    s=await wait(s=>s.url===url);assert(new URL(s.url).searchParams.get('filter')==='open water','link preserves filter '+scope+'.'+key);
    const ownExpected=scope==='north'?(key==='page'?'N2,N4':'N4,N2'):(key==='page'?'S2,S1':'S1,S2');
    assert(s[scope].join()===ownExpected,'requested rows and preserved page '+scope+'.'+key);
    assert((scope==='north'?s.south.join():s.north.join())===(scope==='north'?'S4,S3':'N1,N3'),'other grid unchanged '+scope+'.'+key);
    const h=await call('Page.getNavigationHistory');const target=h.entries[h.currentIndex-1];await call('Page.navigateToHistoryEntry',{entryId:target.id});
    s=await wait(s=>s.url===bookmark&&s.north.join()==='N1,N3');assert(s.south.join()==='S4,S3','browser back restores bookmark');
    const f=await call('Page.getNavigationHistory');await call('Page.navigateToHistoryEntry',{entryId:f.entries[f.currentIndex+1].id});await wait(s=>s.url===url);assert(true,'browser forward restores link');
   }
   for(const scope of ['north','south'])for(const sort of ['code','units'])for(const direction of ['desc','asc']){
    await navigate(base+'?filter=open%20water');
    const click=async(key,value)=>{const url=await evaluate(`(()=>{const a=[...document.querySelectorAll('#${scope} a')].find(a=>(new URL(a.href).searchParams.get('${scope}.${key}')||('${key}'==='page'?'1':null))==='${value}' && ('${key}'!=='direction'||new URL(a.href).searchParams.get('${scope}.sort')===(new URL(location.href).searchParams.get('${scope}.sort')||'code')));if(!a)throw Error('Missing ${scope}.${key}=${value}');const u=a.href;a.click();return u})()`);return wait(s=>s.url===url)};
    s=await click('sort',sort);s=await click('direction',direction);
    const expected={north:{code:{asc:'N1,N2',desc:'N4,N3'},units:{asc:'N2,N4',desc:'N3,N1'}},south:{code:{asc:'S1,S2',desc:'S4,S3'},units:{asc:'S3,S1',desc:'S2,S4'}}};
    assert(s[scope].join()===expected[scope][sort][direction],scope+' '+sort+' '+direction+' from defaults');
    s=await click('page','2');assert(new URL(s.url).searchParams.get(scope+'.page')==='2','next '+scope);s=await click('page','1');assert(s[scope].join()===expected[scope][sort][direction],'previous '+scope);
   }
  }
  ws.close();console.log('BROWSER_PASS');
 } finally {proc.kill();}
}
main().catch(e=>{console.error(e);process.exitCode=1});
