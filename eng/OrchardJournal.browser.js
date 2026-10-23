// Run in the real rendered app after Interactive Server connects. No mocked observers.
(async () => {
  const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
  const assert = (value, message) => { if (!value) throw Error(message); };
  const viewport = document.querySelector('#journal');
  const rows = () => [...viewport.querySelectorAll('[data-entry]')];
  const measure = () => {
    const top = viewport.getBoundingClientRect().top + viewport.clientTop;
    const visible = rows().filter(row => row.getBoundingClientRect().bottom > top
      && row.getBoundingClientRect().top < top + viewport.clientHeight);
    return { scrollTop: viewport.scrollTop, scrollHeight: viewport.scrollHeight,
      clientHeight: viewport.clientHeight, rendered: rows().length,
      visible: visible.map(row => ({ id: row.dataset.entry,
        offset: row.getBoundingClientRect().top - top, height: row.getBoundingClientRect().height })) };
  };
  const anchor = () => measure().visible[0];
  const same = (before, message) => {
    const after = anchor();
    assert(after.id === before.id && Math.abs(after.offset - before.offset) <= 2,
      `${message}: ${JSON.stringify({ before, after })}`);
  };
  const mutate = async id => {
    const before = document.querySelector('#revision').textContent;
    document.querySelector(`#${id}`).click();
    for (let i = 0; i < 100 && document.querySelector('#revision').textContent === before; i++) await pause(25);
    assert(document.querySelector('#revision').textContent !== before, `${id} did not reach server`);
    await pause(700); // Allow real observer delivery, render and layout convergence.
  };
  const scroll = async value => {
    // Mark user scroll intent before changing position, so a prior self-restore
    // does not consume this movement. Observers and framework layout remain real.
    const key = value === 0 ? 'Home' : value >= viewport.scrollHeight ? 'End' : 'PageDown';
    viewport.dispatchEvent(new KeyboardEvent('keydown', { key, bubbles: true }));
    viewport.scrollTop = value;
    await pause(800);
  };
  const mode = async value => {
    const select = document.querySelector('select');
    select.value = value;
    select.dispatchEvent(new Event('change', { bubbles: true }));
    await pause(300);
  };
  await pause(1000);
  await mode('Start');
  await scroll(0);
  const initial = measure();
  assert(new Set(rows().map(row => row.offsetHeight)).size >= 3, 'Rows are not variable height');
  await mutate('prepend');
  const start = measure();
  assert(start.scrollTop <= 2 && start.visible[0].id === '0', 'Start failed to expose prepended entry');
  await scroll(2400);
  const middle = measure();
  assert(+middle.visible[0].id > 5, 'No actual distant rendered rows');
  const held = anchor();
  const heldRow = rows().find(row => row.dataset.entry === held.id);
  const input = heldRow.querySelector('input');
  input.value = 'Keep this crate';
  input.dispatchEvent(new Event('change', { bubbles: true }));
  await pause(200);
  await mutate('refresh');
  same(held, 'Fresh provider identities moved the viewport');
  assert(rows().find(row => row.dataset.entry === held.id).querySelector('input').value === 'Keep this crate', 'Keyed row lost local state');
  const refreshed = measure();
  await mutate('prepend');
  same(held, 'Prepend while reading moved the retained row');
  const prepended = measure();
  const preceding = rows().filter(row => row.getBoundingClientRect().bottom <= viewport.getBoundingClientRect().top + 1).at(-1);
  assert(preceding, 'No overscanned entry above viewport to expand');
  const expandedId = preceding.dataset.entry;
  const previousHeight = preceding.offsetHeight;
  preceding.querySelector('.expand').click();
  await pause(1000);
  same(held, 'Expanding an entry above the viewport moved the retained row');
  assert(rows().find(row => row.dataset.entry === expandedId).offsetHeight > previousHeight, 'Expansion did not render');
  const expanded = measure();
  await mutate('append');
  same(held, 'Append while reading moved the retained row');
  const appendedAway = measure();
  await mode('End');
  await scroll(viewport.scrollHeight);
  await scroll(viewport.scrollHeight); // Estimated range converges after the first tail render.
  assert(measure().visible.at(-1).id === '121', 'Actual tail was not rendered');
  await mutate('append');
  const end = measure();
  assert(end.visible.at(-1).id === '122' && end.scrollHeight - end.clientHeight - end.scrollTop <= 2, 'End did not follow append');
  await scroll(0); // Home explicitly leaves tail-follow mode before reading earlier entries.
  await scroll(2400);
  const away = anchor();
  assert(+away.id > 5 && +away.id < 110, 'Scroll away did not load an earlier provider window');
  await mutate('append');
  same(away, 'End mode pulled a reader away from an earlier entry');
  const endAway = measure();
  await mode('None');
  await scroll(0);
  const neutral = anchor();
  await mutate('prepend');
  same(neutral, 'None mode followed new head');
  const none = measure();
  const virtualized = initial.rendered < 120;
  assert(!virtualized || (middle.rendered > middle.visible.length && middle.rendered < 120), 'Deliberate overscan did not bound rendered rows');
  return { kind: 'real-rendered-browser', url: location.href, userAgent: navigator.userAgent,
    recordedAt: new Date().toISOString(), virtualized, overscan: virtualized ? 15 : null,
    initial, start, middle, refreshed, prepended, expandedId, previousHeight, expanded,
    appendedAway, end, endAway, none, retainedReaderNote: 'Keep this crate', toleranceCssPixels: 2 };
})()
