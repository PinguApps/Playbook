// Evaluate at the fresh Planetarium root on each target in a real browser.
(async () => {
  const wait = async predicate => {
    for (let i = 0; i < 200; i++) {
      if (predicate()) return;
      await new Promise(resolve => setTimeout(resolve, 50));
    }
    throw new Error('Renderer setup or event dispatch timed out');
  };
  const assert = (condition, message) => { if (!condition) throw new Error(message); };
  await wait(() => document.body.dataset.ready === 'true');
  const text = selector => document.querySelector(selector).textContent.trim();
  const first = Number(document.querySelector('#dome button').dataset.renderer);
  const second = Number(document.querySelector('#orrery button').dataset.renderer);
  assert(first !== second && first > 200 && second > 200, 'Distinct host identities');
  assert(text('#dome') === 'Dome advances: 0' && text('#orrery') === 'Orrery advances: 0', 'Initial surfaces');
  document.querySelector('#dome button').click();
  await wait(() => window.planetarium.dispatches.length === 1);
  assert(text('#dome') === 'Dome advances: 1' && text('#orrery') === 'Orrery advances: 0', 'Dome dispatch stays separate');
  document.querySelector('#orrery button').click();
  await wait(() => window.planetarium.dispatches.length === 2);
  assert(text('#dome') === 'Dome advances: 1' && text('#orrery') === 'Orrery advances: 1', 'Orrery dispatch stays separate');
  assert(window.planetarium.dispatches.join(',') === `${first},${second}`, 'Real interop callback sequence');
  const legacy = text('#legacy');
  if (legacy.startsWith('Requested')) assert(legacy === 'Requested init ID: 900; actual hook ID: 203', 'Old init assignment is inert');
  return { passed: true, uri: location.href, browser: navigator.userAgent, first, second,
    attachments: window.planetarium.attachments, dispatches: window.planetarium.dispatches,
    dome: text('#dome'), orrery: text('#orrery'), legacy };
})()
