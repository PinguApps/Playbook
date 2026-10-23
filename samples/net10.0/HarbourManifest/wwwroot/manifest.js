// snippet:scroll
// A bounded fixed-height alternative, specific to this table's 32px rows/header.
let saved;
const frame = () => new Promise(resolve => requestAnimationFrame(resolve));
export async function reveal(id, index, size) {
    const box = document.getElementById(id);
    // Wait for the initial virtualizer spacer, then allow its scroll event to load rows.
    for (let i = 0; i < 120 && box.scrollHeight < (index + 1) * size; i++) await frame();
    box.scrollTop = size + index * size;
}
export function capture(id) {
    const box = document.getElementById(id);
    saved = { top: box.scrollTop, atStart: box.scrollTop <= 2,
              atEnd: box.scrollHeight - box.clientHeight - box.scrollTop <= 2 };
}
export async function restore(id, insertedBefore, size) {
    const box = document.getElementById(id);
    // RefreshDataAsync requests a render; wait for browser layout before restoring.
    await frame(); await frame();
    box.scrollTop = saved.atEnd ? box.scrollHeight : saved.atStart ? 0 : saved.top + insertedBefore * size;
}
// endsnippet:scroll
