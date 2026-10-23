// snippet:server
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(process.argv[2]);
const profiles = path.join(path.dirname(fileURLToPath(import.meta.url)), 'environments');
const types = { '.html': 'text/html', '.js': 'text/javascript', '.json': 'application/json', '.wasm': 'application/wasm', '.dat': 'application/octet-stream', '.pdb': 'application/octet-stream' };
http.createServer((request, response) => {
    let url;
    let pathname;
    try {
        url = new URL(request.url, 'http://localhost');
        pathname = decodeURIComponent(url.pathname === '/' ? '/index.html' : url.pathname);
    } catch {
        response.writeHead(400).end();
        return;
    }
    let file;
    if (url.pathname === '/public-settings.json') {
        const profile = url.searchParams.get('profile');
        if (!['Testing', 'Production'].includes(profile)) { response.writeHead(400).end(); return; }
        file = path.join(profiles, `${profile}.json`);
    } else {
        file = path.resolve(root, `.${pathname}`);
        if (!file.startsWith(root + path.sep)) { response.writeHead(404).end(); return; }
    }
    if (!fs.existsSync(file) || !fs.statSync(file).isFile()) { response.writeHead(404).end(); return; }
    response.writeHead(200, { 'Content-Type': types[path.extname(file)] ?? 'application/octet-stream', 'Cache-Control': 'no-store' });
    fs.createReadStream(file).pipe(response);
}).listen(Number(process.argv[3]), '127.0.0.1', () => console.log(`Estuary assets: ${root}`));
// endsnippet:server
