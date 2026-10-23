// snippet:all
self.onmessage = event => {
  const milliseconds = event.data;
  if (!Number.isInteger(milliseconds) || milliseconds < 1 || milliseconds > 5000) throw Error('Invalid duration');
  const started = performance.now();
  let score = 17;
  do {
    for (let specimen = 0; specimen < 10000; specimen++) score = ((score * 33 + specimen) >>> 0) % 1000003;
  } while (performance.now() - started < milliseconds);
  self.postMessage(score);
};
// endsnippet:all
