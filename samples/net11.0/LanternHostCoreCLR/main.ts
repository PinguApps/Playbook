// snippet:browser-host
import { dotnet } from './_framework/dotnet.js';

declare global { var proof: unknown; }
const resources: string[] = [];
const mode = new URLSearchParams(location.search).get('mode');
try {
    const builder = dotnet.withEnvironmentVariable('LANTERN_ROUTE', 'quay')
        .withApplicationArguments('report', 'east')
        .withResourceLoader((type, name, uri) => {
            resources.push(`${type}:${name}`);
            return uri;
        });
    if (mode === 'legacy-config') builder.withConfigSrc('./legacy-missing.json');
    const runtime = await builder.create();
    const name = runtime.getConfig().mainAssemblyName!;
    const exports = await runtime.getAssemblyExports(name);
    const station = exports.Playbook.Samples.Lantern;
    const before = station.Read();
    const diagnosticOptions: NonNullable<Parameters<typeof runtime.collectMetrics>[0]> = {
        durationSeconds: 0.5, skipDownload: true,
        extraProviders: [{ providerName: 'Lantern-Station', keywords: [0, 0], logLevel: 4, arguments: null }]
    };
    let diagnostics: unknown;
    if (mode === 'diagnostics') {
        try {
            let enabled = false;
            const timer = setInterval(() => { enabled = station.Emit() || enabled; }, 20);
            let chunks: Uint8Array[];
            try { chunks = await runtime.collectMetrics(diagnosticOptions); }
            finally { clearInterval(timer); }

            const bytes = new Uint8Array(chunks.reduce((sum, value) => sum + value.length, 0));
            let offset = 0;
            for (const chunk of chunks) { bytes.set(chunk, offset); offset += chunk.length; }
            const text = new TextDecoder('utf-16le').decode(bytes);
            const textShift = new TextDecoder('utf-16le').decode(bytes.subarray(1));
            diagnostics = { enabled, bytes: bytes.length, providerFound: text.includes('Lantern-Station') || textShift.includes('Lantern-Station') || new TextDecoder().decode(bytes).includes('Lantern-Station') };
        }
        catch (error) { diagnostics = String(error); }
    }
    const code = mode === 'exit' ? await builder.runMainAndExit() : await builder.runMain();
    let after: string | undefined;
    let rejected = false;
    try { after = station.Read(); } catch { rejected = true; }
    globalThis.proof = { code, before, after, rejected, resources, diagnostics, build: runtime.runtimeBuildInfo };
} catch (error) { globalThis.proof = { error: String(error), resources }; }
document.querySelector('#result')!.textContent = JSON.stringify(globalThis.proof);
// endsnippet:browser-host
