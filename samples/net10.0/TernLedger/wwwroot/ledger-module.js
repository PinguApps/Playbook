// snippet:module
window.tern.evaluations.push(import.meta.url);
export function describe(ledger) {
    return `${ledger.ready} dispatches ready for ${ledger.quay}`;
}
// endsnippet:module
