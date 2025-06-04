import http from 'http';
import path from'path';
import { fileURLToPath } from 'url';
import createServer from'../grpc-wrapper/wrappers/server.js';
import loadBalancerHandler, {getNextInstance} from'./loadBalancerHandler.js';
import config from '../grpc-wrapper/example/server/config.json' with { type: 'json' };

/// Get the directory name of the current module
const filename = fileURLToPath(import.meta.url);
const dirname = path.dirname(filename);

// Create gRPC server
createServer(
    path.resolve(dirname, './loadBalancer.proto'),
    loadBalancerHandler,
    config
);

const PORT = 8080; // Port for the HTTP server

// Create an HTTP server
const server = http.createServer((req, res) => {
    const nextInstance = getNextInstance();

    // Check if there is an available instance
    if (!nextInstance) {
        res.writeHead(503, { 'Content-Type': 'text/plain' });
        res.end('No available instances');
        return;
    }

    // Parse the next instance address
    const [host, port] = nextInstance.split(':');
    const options = {
        hostname: host,
        port: port,
        path: req.url,
        method: req.method,
        headers: req.headers,
    };

    // Forward the request to the next instance
    const proxy = http.request(options, (proxyRes) => {
        res.writeHead(proxyRes.statusCode, proxyRes.headers);
        proxyRes.pipe(res, { end: true });
    });

    // Handle errors from the proxy request
    proxy.on('error', (err) => {
        console.error('Error forwarding request:', err);
        res.writeHead(500, { 'Content-Type': 'text/plain' });
        res.end('Error forwarding request');
    });

    req.pipe(proxy, { end: true });
});

// Start the HTTP server
server.listen(PORT, () => {
    console.log(`HTTP server is running on port ${PORT}`);
});
