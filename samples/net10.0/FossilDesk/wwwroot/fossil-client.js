// snippet:all
let worker, pending;
export function score(milliseconds) {
  if (pending) return Promise.reject(new Error('One specimen batch at a time'));
  if (!worker) worker = new Worker(new URL('./fossil-score.js', import.meta.url));
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => { stop(); reject(new Error('Worker timed out')); }, 5000);
    pending = { reject, timer };
    worker.onmessage = event => { clearTimeout(timer); pending = null; resolve(event.data); };
    worker.onerror = event => { stop(); reject(new Error(event.message)); };
    worker.postMessage(milliseconds);
  });
}
export function stop() {
  worker?.terminate(); worker = null;
  if (pending) { clearTimeout(pending.timer); pending.reject(new Error('Worker terminated')); pending = null; }
}
// endsnippet:all
