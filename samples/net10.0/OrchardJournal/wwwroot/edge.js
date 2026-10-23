// snippet:compensation
const snapshots = new WeakMap();
export function capture(viewport, mode) {
  const bounds = viewport.getBoundingClientRect();
  const row = [...viewport.querySelectorAll('[data-entry]')]
    .find(row => row.getBoundingClientRect().bottom > bounds.top + viewport.clientTop);
  snapshots.set(viewport, {
    start: mode === 'Start' && viewport.scrollTop <= 2,
    end: mode === 'End' && viewport.scrollHeight - viewport.clientHeight - viewport.scrollTop <= 2,
    id: row?.dataset.entry,
    offset: row ? row.getBoundingClientRect().top - bounds.top : 0
  });
}
export function restore(viewport) {
  const snapshot = snapshots.get(viewport);
  if (!snapshot) return;
  if (snapshot.start) viewport.scrollTop = 0;
  else if (snapshot.end) viewport.scrollTop = viewport.scrollHeight;
  else {
    const row = [...viewport.querySelectorAll('[data-entry]')]
      .find(row => row.dataset.entry === snapshot.id);
    if (row) viewport.scrollTop += row.getBoundingClientRect().top
      - viewport.getBoundingClientRect().top - snapshot.offset;
  }
  snapshots.delete(viewport);
}
// endsnippet:compensation
