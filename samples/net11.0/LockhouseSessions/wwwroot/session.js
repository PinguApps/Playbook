// snippet:browser
window.addEventListener('load', async () => {
  const show = state => {
    window.sessionState = state;
    const label = document.getElementById('connection');
    if (label) label.textContent = state;
  };
  // This observer disables automatic retries only to make expiry visible in the fixture.
  await Blazor.start({ circuit: { reconnectionHandler: {
    onConnectionDown: () => show('Disconnected'),
    onConnectionUp: () => show('Connected')
  } } });
  while (!document.getElementById('connection'))
    await new Promise(resolve => setTimeout(resolve, 50));
  show('Connected');
});
// endsnippet:browser
