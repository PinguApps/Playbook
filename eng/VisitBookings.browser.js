// Evaluate on /interactive after the circuit connects. Real HTTP rules; no mocks.
(async () => {
  const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
  const assert = (ok, message) => { if (!ok) throw Error(message); };
  const wait = async (condition, message) => {
    for (let i = 0; i < 200; i++) { if (await condition()) return; await pause(25); }
    throw Error(message);
  };
  const control = (action, code) => fetch(`/control/${action}/${code}`, { method: 'POST' });
  const journal = code => fetch(`/control/${code}`).then(response => response.json());
  const change = (id, value) => {
    const input = document.getElementById(id);
    input.value = value;
    input.dispatchEvent(new Event('change', { bubbles: true }));
  };
  await pause(1000); // Allow the Interactive Server circuit to attach after navigation.
  const code = `closed-browser-${Date.now()}`;
  await control('arm', code);
  change('pass', code);
  await wait(async () => (await journal(code)).entered === 1, 'Field rule never entered');
  await wait(() => document.getElementById('pending').textContent === 'Checking', 'No field progress');
  assert(document.querySelector('button').disabled, 'Pending field did not disable submit');
  change('pass', 'open-browser');
  await wait(async () => (await journal(code)).canceled === 1, 'Superseded HTTP rule was not canceled');
  await wait(() => document.getElementById('pending').textContent === 'Idle', 'Replacement field never completed');
  await control('release', code);
  assert(!document.body.textContent.includes('This visit is closed.'), 'Stale field error published');

  // Bypass disabled UI to exercise submit's own authoritative awaited pass.
  const submit = `open-submit-${Date.now()}`;
  await control('arm', submit);
  change('pass', submit);
  await wait(async () => (await journal(submit)).entered === 1, 'Pre-submit field check missing');
  document.querySelector('form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
  await wait(async () => (await journal(submit)).entered >= 2, 'Submit did not rerun the rule');
  const before = await journal(submit);
  assert(before.accepted === 0 && before.completed === 0, 'Submit accepted while rule was held');
  await control('release', submit);
  await wait(() => document.getElementById('result').textContent === 'Accepted', 'Valid submit not accepted');
  const after = await journal(submit);
  assert(after.accepted === 1 && after.completed >= 1, 'Incorrect submission count');

  change('guests', '9');
  document.querySelector('form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
  await wait(() => document.getElementById('result').textContent === 'Rejected', 'Invalid submit accepted');
  const expected = document.documentElement.lang === 'fr'
    ? 'Taille du groupe : choisissez de 1 à 6 visiteurs.'
    : 'Party size: choose between 1 and 6 guests.';
  assert(document.body.textContent.includes(expected), 'Interactive localized formatter failed');
  return { kind: 'real-browser-loopback', url: location.href, userAgent: navigator.userAgent,
    recordedAt: new Date().toISOString(), superseded: await journal(code), before, after, localizedMessage: expected };
})()
