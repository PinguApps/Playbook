// snippet:complete
// Historical fixture; intentionally NOT loaded by index.html.
window.legacyArchive = {
  push(uri) { history.pushState(null, '', uri); }
};
// endsnippet:complete
