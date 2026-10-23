// Execute against a fresh real FossilDesk app after the Ready state.
(async () => {
  const assert = (value, message) => { if (!value) throw Error(message); };
  const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
  const until = async test => { for (let n = 0; n < 600; n++) { if (test()) return; await pause(25); } throw Error('Browser assertion timed out'); };
  const state = () => document.querySelector('#state').textContent;
  const result = () => document.querySelector('#result').textContent;
  const newer = !!document.querySelector('#timeout');
  const trace = [];
  await until(() => state() === 'Ready');
  assert(performance.getEntriesByType('resource').some(item => /dotnet\.native\..*\.wasm/.test(item.name)), 'No real WASM runtime');
  document.querySelector('#score').click();
  await until(() => state() === 'Computing');
  await pause(150);
  let frames = 0;
  const end = performance.now() + 300;
  const animate = () => { frames++; if (performance.now() < end) requestAnimationFrame(animate); };
  requestAnimationFrame(animate);
  document.querySelector('#interact').click();
  await pause(100);
  assert(document.querySelector('#clicks').textContent === '1' && state() === 'Computing', 'UI must render DURING actual worker CPU work');
  await pause(300);
  assert(frames >= 2 && state() === 'Computing', 'Rendering must advance before computation finishes');
  trace.push({ phase: 'responsive-during-cpu', frames, state: state(), clicks: 1 });
  await until(() => state() === 'Completed' && (!newer || result().endsWith('Westmere-A:1')));
  assert(result().includes('Score '), 'Actual computation result missing');
  trace.push({ phase: 'completed', result: result() });
  if (newer) {
    document.querySelector('#timeout').click();
    await pause(100);
    document.querySelector('#interact').click();
    await pause(50);
    assert(document.querySelector('#score').disabled, 'Another UI render must not re-enable calls while timed-out work is being observed');
    await until(() => state().includes('timed out'));
    assert(!window.fossil.workers[0].terminatedAt, 'Invocation timeout unexpectedly terminated worker');
    await until(() => result().endsWith('Westmere-A:2'));
    trace.push({ phase: 'invocation-timeout-work-continued', state: state(), result: result() });
    document.querySelector('#cancel').click();
    await until(() => state() === 'Wait cancelled');
    assert(!window.fossil.workers[0].terminatedAt, 'Managed cancellation unexpectedly terminated worker');
    await until(() => result().endsWith('Westmere-A:3'));
    trace.push({ phase: 'invocation-cancel-work-continued', state: state(), result: result() });
    const events = window.fossil.workers[0].events;
    const scores = events.filter(event => event.direction === 'sent' && event.data.method?.endsWith('.Score'));
    for (const score of scores.slice(1, 3)) {
      const report = events.find(event => event.direction === 'sent' && event.data.method?.endsWith('.Report') && event.at > score.at);
      const completed = events.find(event => event.direction === 'received' && event.data.requestId === score.data.requestId);
      assert(report && completed && report.at < completed.at, 'Wait must end and enqueue Report BEFORE timed/cancelled work completes');
    }
    trace.push({ phase: 'both-waits-ended-before-actual-worker-completion', verifiedCalls: 2 });
    const beforeTimeout = window.fossil.workers.length;
    const creationTimeout = await DotNet.invokeMethodAsync('FossilDesk', 'CreationTimeout');
    assert(creationTimeout.includes('initialization timed out'), 'Expected actual creation timeout');
    assert(window.fossil.workers.length === beforeTimeout + 1 && window.fossil.workers.at(-1).terminatedAt, 'Creation timeout must terminate its actual worker');
    trace.push({ phase: 'creation-timeout-terminated', message: creationTimeout });
    // Hold only the real worker's init message to make the cancellation race deterministic.
    // No transport replacement or synthetic worker reply is used.
    window.fossil.holdInitialization = true;
    const beforeCancel = window.fossil.workers.length;
    const cancelled = DotNet.invokeMethodAsync('FossilDesk', 'CreationCancellation');
    await until(() => window.fossil.workers.length === beforeCancel + 1 && window.fossil.workers.at(-1).release);
    await DotNet.invokeMethodAsync('FossilDesk', 'CancelCreation');
    assert(await cancelled === 'creation-await-cancelled', 'Expected actual managed creation await cancellation');
    const orphan = window.fossil.workers.at(-1);
    assert(!orphan.terminatedAt, 'Creation cancellation was mistaken for termination');
    window.fossil.holdInitialization = false;
    orphan.release();
    await until(() => orphan.events.some(event => event.data.type === 'ready'));
    assert(!orphan.events.find(event => event.data.type === 'ready').data.error, 'Cancelled-await worker failed to boot real .NET');
    trace.push({ phase: 'creation-await-cancelled-worker-still-booted', observerCleanupRequired: true });
    orphan.stop();
    document.querySelector('#score').click();
    await until(() => state() === 'Computing');
    await pause(100);
  }
  document.querySelector('#stop').click();
  await until(() => state() === 'Worker released');
  assert(window.fossil.workers[0].terminatedAt, 'Owned DisposeAsync must terminate actual worker');
  if (newer) {
    const owned = window.fossil.workers[0];
    const lastScore = owned.events.filter(event => event.direction === 'sent' && event.data.method?.endsWith('.Score')).at(-1);
    assert(!owned.events.some(event => event.direction === 'received' && event.data.requestId === lastScore.data.requestId), 'Termination control must interrupt pending CPU work');
  }
  const messages = window.fossil.workers[0].events.length;
  await pause(1400);
  assert(window.fossil.workers[0].events.length === messages, 'Worker emitted more messages after termination');
  assert(window.fossil.workers.every(worker => worker.terminatedAt), 'Live test worker leaked');
  return {
    kind: 'real-browser-webassembly-native-worker', recordedAt: new Date().toISOString(),
    url: location.href, userAgent: navigator.userAgent, newer, trace, finalState: state(),
    workers: window.fossil.workers.map(({ url, createdAt, events, terminatedAt }) => ({ url, createdAt, events, terminatedAt })),
    resources: performance.getEntriesByType('resource').map(item => item.name).filter(name => /dotnet\.|fossil|Fossil|web-worker|blazor.webassembly/.test(name)),
    limit: 'Real foreground browser; deterministic init-message hold for creation cancellation only. No mock worker; no GA/AOT/mobile/SSR/Auto/deployment or durable-work proof.'
  };
})();
