import express from 'express';
import path from "path";
import createClient from '../../grpc-wrapper/wrappers/client.js';
import {fileURLToPath} from "url";
import bookClientConfig from './bookClientConfig.json' with { type: 'json' };
import loadBalancerClientConfig from './loadBalancerClientConfig.json' with { type: 'json' };

// Get the directory name of the current module
const filename = fileURLToPath(import.meta.url);
const dirname = path.dirname(filename);

// Port for the HTTP server
const PORT = 8080;

// Create gRPC clients
const clients = new Map();

// Create Load Balancer gRPC client
const loadBalancerClient = createClient(
    path.resolve(dirname, '../server/loadBalancer.proto'),
    loadBalancerClientConfig
);

// Create an HTTP server
const app = express();
app.get('/api/book/:id', (req, res) => {
    const bookId = req.params.id;

    // Log the request
    console.log(`Received request for book ID: ${bookId}`);

    // Get the next available instance from the load balancer
    loadBalancerClient.getNextInstance({ id: bookId }, (error, nextInstance) => {
        if (error) {
            console.error('Error getting next instance :', error);
            res.status(500).send('Error getting next instance')
            return
        }

        // Check if there is an available instance
        if (!nextInstance) {
            res.status(503).send('No available instances');
            return;
        }

        // Get the target for the next instance
        const target = `${nextInstance.ip}:${nextInstance.port}`;
        console.log(`Forwarding request to instance at ${target}`);

        // Check if the client for the next instance already exists
        if (!clients.has(target)) {
            // Create a new client for the next instance
            const client = createClient(
                path.resolve(dirname, '../../grpc-wrapper/example/proto/book.proto'),
                {
                    ...bookClientConfig,
                    target,
                }
            );
            clients.set(target, client);
        }

        // Get the client for the next instance
        const client = clients.get(target);

        // Forward the request to the next instance
        client.getBookByID({ id: bookId }, (error, book) => {
            if (error) {
                console.error('Error fetching book:', error);
                res.status(500).send('Error fetching book');
                return;
            }
            res.json(book);
        });
    })
});

// Start the HTTP server
app.listen(PORT, 'localhost', () => {
    console.log(`HTTP server is running on port ${PORT}`);
});