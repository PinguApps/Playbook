// snippet:browser
window.tide = {
    hold: () => new Promise(resolve => { window.tide.release = resolve; }),
    // The old server bridge acknowledges dispatch, then asks the browser to pause.
    requestPause: () => { setTimeout(() => Blazor.pauseCircuit(), 0); return true; }
};
// endsnippet:browser