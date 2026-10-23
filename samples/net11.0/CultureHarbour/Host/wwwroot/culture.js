// snippet:complete
const desk = document.body.dataset;
window.cultureHarbour = {
  prerender: document.querySelector('#manifest').innerText,
  events: [],
  selection() {
    if (desk.independent === 'true') return ['de-DE', 'de-DE'];
    return null; // Framework persisted state owns the aligned choice.
  },
  observe(kind, culture, ui) {
    this.events.push({ kind, culture, ui, at: performance.now() });
  }
};
// Deliberately different from both server cultures; combined ICU permits startup changes.
Blazor.start({ webAssembly: { applicationCulture: 'en-US' } })
  .catch(error => { window.cultureHarbour.error = String(error); });
// endsnippet:complete
