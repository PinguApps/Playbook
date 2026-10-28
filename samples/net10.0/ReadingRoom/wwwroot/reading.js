// snippet:complete
let open = true;
export function readLabel(broken) {
    if (!open) throw new Error('Reading session is closed');
    if (broken) throw new Error('Reading room scanner cannot read the shelf label');
    return 'Shelf R7: maps and atlases';
}
export function closeReader() { open = false; }
// endsnippet:complete
