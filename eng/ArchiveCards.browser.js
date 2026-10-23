// Evaluate on the running ArchiveCards origin in the collaborative browser.
// Run at / on both targets and at /probe/discarded-handler on .NET 11.
(async () => {
  const read = selector => document.querySelector(selector)?.textContent;
  const before = read('#snapshot') ?? read('#parent-count');
  if (!before) throw new Error('Open the reading card or discarded-handler probe');
  const activationDeadline = Date.now() + 10000;
  while (document.querySelector('#add').disabled && Date.now() < activationDeadline) {
    await new Promise(resolve => setTimeout(resolve, 50));
  }
  if (document.querySelector('#add').disabled) throw new Error('Circuit did not activate');
  const count = Number(read('#requests'));
  document.querySelector('#add').click();
  const deadline = Date.now() + 10000;
  while (Number(read('#requests')) !== count + 1 && Date.now() < deadline) {
    await new Promise(resolve => setTimeout(resolve, 50));
  }
  if (Number(read('#requests')) !== count + 1) throw new Error('Interactive control did not update');
  if ((read('#snapshot') ?? read('#parent-count')) !== before) throw new Error('Captured content changed');
  if (document.querySelector('#discarded')) {
    const callsBefore = await (await fetch('/control/callbacks')).json();
    document.querySelector('#discarded').click();
    await new Promise(resolve => setTimeout(resolve, 300));
    if (read('#parent-count') !== 'Parent callbacks: 0') throw new Error('Captured parent handler became live');
    const callsAfter = await (await fetch('/control/callbacks')).json();
    if (callsBefore !== 0 || callsAfter !== callsBefore) throw new Error('Parent callback executed on the server');
  }
  return { passed: true, href: location.href, userAgent: navigator.userAgent,
    heading: read('h2'), captured: before, requests: read('#requests'),
    parentCallbacks: read('#parent-count') ?? null };
})()
