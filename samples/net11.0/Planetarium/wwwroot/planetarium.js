// snippet:complete
// Deliberate framework adapter: keep this and managed framework assets aligned.
// Observe and forward the REAL attachment, preserving Blazor's own registry.
const renderers = new Map();
const originalAttach = Blazor._internal.attachWebRendererInterop;
window.planetarium = {
  attachments: [],
  dispatches: [],
  complete(ids) {
    for (const id of ids) {
      if (!renderers.has(id)) throw new Error(`Renderer ${id} did not attach during construction`);
    }
    document.body.dataset.ready = 'true';
  },
  render(id, selector, handler, text) {
    const interop = renderers.get(id);
    if (!interop) throw new Error(`Missing renderer ${id}`);
    const button = document.createElement('button');
    button.textContent = text;
    button.dataset.renderer = id;
    button.onclick = async event => {
      await interop.invokeMethodAsync('DispatchEventAsync',
        { eventHandlerId: handler, eventName: 'click', eventFieldInfo: null },
        { type: 'click', detail: event.detail });
      window.planetarium.dispatches.push(id);
    };
    document.querySelector(selector).replaceChildren(button);
  }
};
Blazor._internal.attachWebRendererInterop = (id, interop, ...configuration) => {
  originalAttach(id, interop, ...configuration);
  if (id > 200) {
    if (renderers.has(id)) throw new Error(`Duplicate renderer ${id}`);
    renderers.set(id, interop);
    window.planetarium.attachments.push(id);
  }
};
// endsnippet:complete
