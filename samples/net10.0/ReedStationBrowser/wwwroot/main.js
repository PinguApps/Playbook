import { dotnet } from './_framework/dotnet.js';
try {
  const runtime = await dotnet.create();
  const exports = await runtime.getAssemblyExports('ReedStationBrowser');
  const result = exports.Playbook.Samples.BrowserChecks.Run();
  globalThis.proof = { result };
  document.querySelector('#result').textContent = result;
} catch (error) { globalThis.proof = { error: String(error.stack || error) }; }
