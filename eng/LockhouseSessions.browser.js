(async () => {
  const assert = (value, message) => { if (!value) throw new Error(message); };
  const wait = async (predicate, milliseconds) => {
    const deadline = Date.now() + milliseconds;
    while (!predicate()) {
      if (Date.now() > deadline) throw new Error('Timed out waiting for circuit state');
      await new Promise(resolve => setTimeout(resolve, 50));
    }
  };
  await wait(() => document.getElementById('relay') && window.sessionState === 'Connected', 10000);
  const started = new Date().toISOString();
  const ticket = await (await fetch('/ticket')).json();
  assert(ticket.authenticated, 'Cookie ticket must be authenticated');
  const policy = await (await fetch('/policy')).json();
  assert(policy.length > 0 && policy.every(p => p.transports === 'WebSockets' &&
    p.applicationMaxBufferSize === 8192 && p.transportMaxBufferSize === 16384), 'Actual mapped policy differs');
  const negotiate = await (await fetch('/_blazor/negotiate?negotiateVersion=1', {method: 'POST'})).json();
  assert(negotiate.availableTransports.length === 1 && negotiate.availableTransports[0].transport === 'WebSockets', 'Negotiation must exclude fallback');
  const fallback = await fetch('/_blazor?id=' + encodeURIComponent(negotiate.connectionToken));
  const fallbackText = await fallback.text();
  assert(!fallback.ok, 'LongPolling request must fail on WebSockets-only endpoint');
  const cargo = 'Westmere-'.repeat(3072); // 27,648 ASCII bytes: larger than both pipe thresholds.
  const input = document.getElementById('cargo');
  input.value = cargo;
  input.dispatchEvent(new Event('change', {bubbles: true}));
  document.getElementById('relay').click();
  await wait(() => document.getElementById('returned').textContent === cargo, 10000);
  const receipt = document.getElementById('receipt').textContent;
  assert(receipt === 'Receipt 1: 27648 characters', 'Real incoming event/outgoing render failed');
  const identity = document.getElementById('identity').textContent;
  const close = policy[0].closeOnAuthenticationExpiration;
  if (close) {
    await wait(() => window.sessionState === 'Disconnected', 35000);
  } else {
    const waitMs = Math.max(0, Date.parse(ticket.expires) + 2500 - Date.now());
    await new Promise(resolve => setTimeout(resolve, waitMs));
    assert(window.sessionState === 'Connected', 'Negative control unexpectedly disconnected');
    document.getElementById('relay').click();
    await wait(() => document.getElementById('receipt').textContent === 'Receipt 2: 27648 characters', 5000);
  }
  const expired = await (await fetch('/ticket')).json();
  assert(!expired.authenticated, 'Fixture cookie must actually expire');
  const rejected = await fetch('/_blazor/negotiate?negotiateVersion=1', {method: 'POST'});
  assert(rejected.status === 401, 'New expired-cookie connection must be rejected');
  return {status: 'PASS', started, completed: new Date().toISOString(), browser: navigator.userAgent,
    target: location.origin, identity, policy, ticket, negotiate: negotiate.availableTransports,
    fallback: {status: fallback.status, text: fallbackText}, echoedCharacters: cargo.length, receipt,
    closeOnExpiry: close, afterExpiry: window.sessionState, newConnectionStatus: rejected.status,
    controlReceipt: document.getElementById('receipt').textContent};
})()
