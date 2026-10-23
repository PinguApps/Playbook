// Run only against the real standalone BeaconWatch WASM app, before shutdown.
(async () => {
  const assert = (value, message) => { if (!value) throw Error(message); };
  const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
  const count = () => Number(document.querySelector('#ticks').textContent);
  const clicks = () => Number(document.querySelector('#clicks').textContent);
  const resources = performance.getEntriesByType('resource').map(entry => entry.name);
  assert(resources.some(name => /dotnet\.native\..*\.wasm/.test(name)), 'No actual browser WASM runtime loaded');
  assert(window.beacon.events.filter(event => event.kind === 'started').length === 1, 'Registered service must start exactly once');
  const initial = count();
  document.querySelector('#interact').click();
  await pause(1100);
  assert(clicks() === 1 && count() > initial, 'UI and actual periodic work must both advance');
  const running = { ticks: count(), clicks: clicks() };
  document.querySelector('#shutdown').click();
  for (let attempt = 0; attempt < 100 && document.querySelector('#lifetime').textContent !== 'Application disposed'; attempt++) await pause(25);
  assert(document.querySelector('#lifetime').textContent === 'Application disposed', 'Owned host disposal did not complete');
  const stopped = count();
  const shutdown = performance.now();
  await pause(1500);
  assert(count() === stopped, 'Periodic work continued after awaited shutdown');
  const events = window.beacon.events.filter(event => event.kind !== 'tick');
  assert(events.map(event => event.kind).join(',') === 'started,stop-requested,cancelled,stopped,root-disposed', 'Real lifecycle ordering differs');
  assert(window.beacon.events.filter(event => event.kind === 'tick').every(event => event.at <= events.find(event => event.kind === 'stopped').at), 'Tick occurred after service stop');
  return {
    kind: 'real-browser-webassembly', url: location.href, userAgent: navigator.userAgent,
    recordedAt: new Date().toISOString(), initial, running, stopped, final: count(),
    quietMilliseconds: performance.now() - shutdown, lifecycle: events,
    rootDisposed: events.some(event => event.kind === 'root-disposed'),
    renderedCounterWorked: running.clicks === 1, applicationDisposed: true,
    lastTick: window.beacon.events.filter(event => event.kind === 'tick').at(-1),
    resources: resources.filter(name => /dotnet\.(native|runtime)|BeaconWatch|blazor\.webassembly|watch-lifetime/.test(name)),
    limit: 'Owned disposal in a live document; not tab termination, RunAsync completion, durable work, mobile or GA proof.'
  };
})();
