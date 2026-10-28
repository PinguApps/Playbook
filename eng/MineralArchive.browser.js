// Evaluate at the fresh MineralArchive root on each target in a real browser.
(async () => {
  const wait = async predicate => {
    for (let i = 0; i < 200; i++) {
      if (predicate()) return;
      await new Promise(resolve => setTimeout(resolve, 50));
    }
    throw new Error('Navigation/observer timed out');
  };
  const assert = (condition, message) => { if (!condition) throw new Error(message); };
  const entries = () => [...document.querySelectorAll('#observations li')].map(x => x.textContent);
  const expected = [];
  const check = async (path, state, intercepted) => {
    expected.push(`${expected.length + 1}|${path}|${state ?? '(null)'}|intercepted=${intercepted}`);
    await wait(() => entries().length === expected.length && document.querySelector('#location').textContent === location.href);
    assert(location.pathname === path, 'Browser URI');
    assert((history.state?.userState ?? null) === state, 'Actual browser history state');
    assert(entries().join('\n') === expected.join('\n'), 'Exact committed observer sequence');
    assert(document.querySelector('#state').textContent === `State: ${state ?? '(null)'}`, 'Managed history state');
    assert(document.querySelector('#specimen').textContent === `Specimen: ${path === '/' ? 'catalogue' : path.split('/').at(-1)}`, 'Routed page');
  };
  await wait(() => document.querySelector('#open-specimen'));
  assert(location.pathname === '/' && entries().length === 0, 'Fresh catalogue');
  document.querySelector('#open-specimen').click();
  await check('/specimens/fluorite', 'return-selection=drawer-7', 'False');
  document.querySelector('#intercepted').click();
  await check('/specimens/galena', null, 'True');
  history.back();
  await check('/specimens/fluorite', 'return-selection=drawer-7', 'True');
  history.back();
  await check('/', null, 'True');
  history.forward();
  await check('/specimens/fluorite', 'return-selection=drawer-7', 'True');
  history.forward();
  await check('/specimens/galena', null, 'True');
  return { passed: true, uri: location.href, browser: navigator.userAgent, observations: entries() };
})()
