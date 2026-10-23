// snippet:complete
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';

// Serve only the explicitly selected disposable legacy-client publish directory.
const root = path.resolve(process.argv[2]);
const types = { '.html': 'text/html', '.js': 'text/javascript', '.json': 'application/json', '.wasm': 'application/wasm', '.dat': 'application/octet-stream', '.pdb': 'application/octet-stream' };
http.createServer((request, response) => {
    const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
    const file = path.resolve(root, `.${pathname === '/' ? '/index.html' : pathname}`);
    if (!file.startsWith(root + path.sep) || !fs.existsSync(file) || !fs.statSync(file).isFile()) {
        response.writeHead(404).end();
        return;
    }
    response.writeHead(200, { 'Content-Type': types[path.extname(file)] ?? 'application/octet-stream', 'Cache-Control': 'no-store' });
    fs.createReadStream(file).pipe(response);
}).listen(Number(process.argv[3]), '127.0.0.1', () => console.log(`Legacy client: ${root}`));
// endsnippet:complete
