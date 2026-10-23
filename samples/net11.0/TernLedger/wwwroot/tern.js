// snippet:browser
window.tern = { documentId: crypto.randomUUID(), enhancedLoads: 0, evaluations: [] };
Blazor.start().then(() => {
    Blazor.addEventListener("enhancedload", () => window.tern.enhancedLoads++);
    document.addEventListener("click", async event => {
        const button = event.target.closest("button[data-ledger]");
        if (!button) return;
        button.disabled = true;
        try {
            const view = encodeURIComponent(button.dataset.ledger);
            const [module, response] = await Promise.all([
                import(`/ledger-module.js?view=${view}`),
                fetch(`/ledger-data.json?view=${view}`, { credentials: "same-origin" })
            ]);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            document.querySelector("#ledger-result").textContent = module.describe(await response.json());
        } catch (error) {
            document.querySelector("#ledger-result").textContent = `Failed: ${error.message}`;
        } finally {
            button.disabled = false;
        }
    });
});
// endsnippet:browser
