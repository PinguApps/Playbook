// snippet:scroll
export async function reveal(viewport, index, total) {
    const deadline = performance.now() + 60000;
    const until = async predicate => {
        while (!predicate()) {
            if (performance.now() > deadline) throw new Error(`Ticket ${index} was not rendered in time`);
            await new Promise(resolve => setTimeout(resolve, 16));
        }
    };
    // Wait for the provider's total count to establish the full spacer extent.
    await until(() => viewport.scrollHeight >= total * 40 && viewport.querySelector('[data-index]'));
    viewport.scrollTop = index * 40;
    // A scroll offset alone does not prove that a delayed provider supplied the ticket.
    await until(() => {
        const row = viewport.querySelector(`[data-index="${index}"]`);
        if (!row) return false;
        const outer = viewport.getBoundingClientRect(), inner = row.getBoundingClientRect();
        return inner.top >= outer.top && inner.bottom <= outer.bottom;
    });
}
// endsnippet:scroll
