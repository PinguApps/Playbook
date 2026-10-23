// snippet:browser
window.tide = {
    hold: () => new Promise(resolve => { window.tide.release = resolve; }),
    // The old server bridge acknowledges dispatch, then asks the browser to pause.
    requestPause: () => { setTimeout(() => Blazor.pauseCircuit(), 0); return true; }
};
// endsnippet:browser// snippet:idle
// App-owned visibility policy for .NET 10; this simple worksheet has no media or streams.
let timer;
let paused = false;
let pending = Promise.resolve();
document.addEventListener('visibilitychange', () => {
    clearTimeout(timer);
    if (document.hidden) {
        timer = setTimeout(() => {
            pending = pending.then(async () => {
                if (document.hidden) paused = await Blazor.pauseCircuit();
                if (!document.hidden && paused) paused = !(await Blazor.resumeCircuit());
            });
        }, 4000);
    } else {
        pending = pending.then(async () => {
            if (paused) paused = !(await Blazor.resumeCircuit());
        });
    }
});
// endsnippet:idle
