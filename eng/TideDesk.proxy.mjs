// Local authoring fault injector. Real HTTP/WebSocket bytes reach the actual app.
// No fake SignalR connection, reconnect callback or browser option is installed.
import http from 'node:http';
import net from 'node:net';

const [port, upstreamPort] = process.argv.slice(2).map(Number);
if (!port || !upstreamPort) throw new Error('Supply proxy and upstream loopback ports');
let blocked = false;
const sockets = new Set();
const attempts = [];
const server = http.createServer((request, response) => {
    if (request.url.startsWith('/__fault/')) {
        if (request.method === 'POST' && request.url === '/__fault/down') {
            blocked = true;
            attempts.length = 0;
            for (const socket of sockets) socket.destroy();
        } else if (request.method === 'POST' && request.url === '/__fault/up') {
            blocked = false;
        } else if (request.url !== '/__fault/state') {
            response.writeHead(404).end();
            return;
        }
        response.writeHead(200, { 'content-type': 'application/json' });
        response.end(JSON.stringify({ blocked, attempts }));
        return;
    }
    if (blocked && request.url.startsWith('/_blazor')) {
        attempts.push({ url: request.url, at: Date.now() });
        response.writeHead(503).end('Local test interruption');
        return;
    }
    const upstream = http.request({
        hostname: '127.0.0.1', port: upstreamPort, path: request.url,
        method: request.method, headers: request.headers
    }, incoming => {
        response.writeHead(incoming.statusCode, incoming.headers);
        incoming.pipe(response);
    });
    upstream.on('error', () => { if (!response.headersSent) response.writeHead(502); response.end(); });
    request.pipe(upstream);
});
server.on('upgrade', (request, socket, head) => {
    if (blocked) { socket.destroy(); return; }
    const upstream = net.connect(upstreamPort, '127.0.0.1', () => {
        upstream.write(`${request.method} ${request.url} HTTP/${request.httpVersion}\r\n`);
        for (let i = 0; i < request.rawHeaders.length; i += 2) {
            upstream.write(`${request.rawHeaders[i]}: ${request.rawHeaders[i + 1]}\r\n`);
        }
        upstream.write('\r\n');
        if (head.length) upstream.write(head);
        socket.pipe(upstream).pipe(socket);
    });
    sockets.add(socket);
    sockets.add(upstream);
    socket.on('error', () => upstream.destroy());
    upstream.on('error', () => socket.destroy());
    socket.on('close', () => { sockets.delete(socket); upstream.destroy(); });
    upstream.on('close', () => { sockets.delete(upstream); socket.destroy(); });
});
server.listen(port, '127.0.0.1', () => console.log(`TideDesk fault proxy ${port} -> ${upstreamPort}`));
