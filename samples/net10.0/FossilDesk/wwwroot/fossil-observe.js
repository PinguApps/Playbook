// snippet:all
// Test observation only: every instance is a real native browser Worker.
window.fossil = { workers: [], holdInitialization: false };
const NativeWorker = window.Worker;
window.Worker = class extends NativeWorker {
  constructor(url, options) {
    super(url, options);
    const record = { url: String(url), createdAt: performance.now(), events: [], terminatedAt: null };
    const send = this.postMessage.bind(this);
    const terminate = this.terminate.bind(this);
    this.postMessage = (...args) => {
      record.events.push({ direction: 'sent', at: performance.now(), data: args[0] });
      if (window.fossil.holdInitialization && args[0]?.type === 'init') record.release = () => send(...args);
      else send(...args);
    };
    this.terminate = () => { record.terminatedAt = performance.now(); terminate(); };
    record.stop = () => this.terminate();
    this.addEventListener('message', event => record.events.push({ direction: 'received', at: performance.now(), data: event.data }));
    window.fossil.workers.push(record);
  }
};
// endsnippet:all
