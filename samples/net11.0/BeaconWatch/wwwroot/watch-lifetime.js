// snippet:browser
(() => {
  const events = [];
  window.beacon = {
    events,
    record(kind, count) {
      events.push({ kind, count, at: performance.now() });
      if (kind === 'tick') document.querySelector('#ticks').textContent = count;
      else {
        const item = document.createElement('li');
        item.textContent = `${kind}: ${count}`;
        document.querySelector('#events').append(item);
      }
    },
    attach(owner) {
      const button = document.querySelector('#shutdown');
      button.disabled = false;
      document.querySelector('#lifetime').textContent = 'Running';
      button.addEventListener('click', async () => {
        button.disabled = true;
        try {
          await owner.invokeMethodAsync('ShutdownAsync');
          document.querySelector('#lifetime').textContent = 'Application disposed';
        } catch (error) {
          document.querySelector('#lifetime').textContent = `Shutdown failed: ${error}`;
        }
      }, { once: true });
    }
  };
})();
// endsnippet:browser
