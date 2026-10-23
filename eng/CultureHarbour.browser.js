// Evaluate against a fresh navigation to the real CultureHarbour Web App.
(async () => {
  const assert = (ok, why) => { if (!ok) throw Error(why); };
  const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
  const journal = window.cultureHarbour;
  assert(journal && !journal.error, 'Browser startup failed');
  const independent = document.body.dataset.independent === 'true';
  const read = () => Object.fromEntries(['culture', 'ui', 'date', 'number', 'translation', 'phase'].map(id => [id, document.getElementById(id).textContent]));
  const before = journal.events.find(event => event.kind === 'before-run');
  const render = journal.events.find(event => event.kind === 'first-render');
  assert(before?.culture === 'en-US' && before.ui === 'en-US', 'Initial runtime culture must differ from server');
  assert(render && before.at < render.at, 'Missing actual WASM render');
  const hydrated = read();
  const expected = independent
    ? { culture: 'de-DE', ui: 'de-DE', date: '19.07.2032', number: '12.345,67', translation: 'Die blaue Kiste aufrecht halten.' }
    : { culture: 'en-GB', ui: 'fr-FR', date: '19/07/2032', number: '12,345.67', translation: 'Gardez la caisse bleue debout.' };
  for (const [key, value] of Object.entries(expected)) assert(hydrated[key] === value, `${key}: ${hydrated[key]} != ${value}`);
  assert(hydrated.phase === 'Interactive WASM', 'Not interactive');
  const html = await (await fetch('/', { cache: 'no-store' })).text();
  const ssr = new DOMParser().parseFromString(html, 'text/html');
  const prerendered = Object.fromEntries(['culture', 'ui', 'date', 'number', 'translation', 'phase'].map(id => [id, ssr.getElementById(id).textContent]));
  assert(prerendered.culture === 'en-GB' && prerendered.ui === 'fr-FR' && prerendered.phase === 'Prerendered', 'SSR culture policy absent');
  assert(prerendered.date === '19/07/2032' && prerendered.number === '12,345.67' && prerendered.translation === 'Gardez la caisse bleue debout.', 'SSR formatting/translation incorrect');
  assert(journal.prerender.includes(prerendered.translation), 'Initial document snapshot did not see SSR');
  const resources = performance.getEntriesByType('resource').map(e => ({ url: e.name, start: e.startTime, end: e.responseEnd, bytes: e.decodedBodySize }));
  const satellite = resources.find(e => e.url.includes(`/${expected.ui}/CultureHarbour.Client.resources.`));
  assert(satellite && satellite.bytes > 0 && satellite.start > before.at && satellite.end <= render.at, 'Satellite must actually load after Program and before first render');
  assert(resources.some(e => /dotnet\.native\..*\.wasm/.test(e.url)), 'No real WASM runtime');
  document.querySelector('#confirm').click();
  await pause(150);
  assert(document.querySelector('#confirmations').textContent === '1', 'Interactive handler did not execute');
  const artifacts = [];
  for (const resource of resources.filter(e => /CultureHarbour.Client|dotnet\.(native|runtime)|icudt|blazor\.web\.js|culture\.js/.test(e.url))) {
    const response = await fetch(resource.url, { cache: 'no-store' });
    assert(response.ok, `Artifact fetch failed: ${resource.url}`);
    const data = await response.arrayBuffer();
    const digest = await crypto.subtle.digest('SHA-256', data);
    artifacts.push({ ...resource, sha256: [...new Uint8Array(digest)].map(b => b.toString(16).padStart(2, '0')).join(''), fetchedBytes: data.byteLength });
  }
  const configuration = html.match(/<!--Blazor-Configuration:([^>]+)-->/);
  return { kind: 'real-browser-webassembly', recordedAt: new Date().toISOString(), url: location.href, userAgent: navigator.userAgent,
    navigatorLanguage: navigator.language, edition: document.body.dataset.edition, independent, prerendered, hydrated,
    capturedInitialDocument: journal.prerender, events: journal.events, satellite, artifacts, confirmations: 1,
    emittedBrowserConfiguration: configuration ? JSON.parse(atob(configuration[1])) : null,
    limits: 'Local interpreted development assets; no AOT, Auto, production publication, remote provider, tab-lifetime or GA execution claim.' };
})();
