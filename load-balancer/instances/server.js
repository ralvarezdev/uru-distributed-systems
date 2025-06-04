import createGRPCServer from'../../grpc-wrapper/wrappers/server.js';
import bookHandler from'../../grpc-wrapper/example/server/handlers/bookHandler.js';
import path from 'path';
import createClient from '../../grpc-wrapper/wrappers/client.js';
import {fileURLToPath} from "url";
import * as os from "node:os";
import clientConfig from './clientConfig.json' with { type: 'json' };
import serverConfig from './serverConfig.json' with { type: 'json' };

// Get the directory name of the current module
const filename = fileURLToPath(import.meta.url);
const dirname = path.dirname(filename);

// Heartbeat interval
const HEARTBEAT_INTERVAL = 5000; // Interval in milliseconds

// Retrieve the IP and port from command-line arguments
const PORT = process.argv[2] || 50051; // Default port if not provided

// Create Load Balancer gRPC client
const client = createClient(
    path.resolve(dirname, '../server/loadBalancer.proto'),
    clientConfig
);

// Create gRPC server
createGRPCServer(
    path.resolve(dirname, '../../grpc-wrapper/example/proto/book.proto'),
    bookHandler,
    { ...serverConfig, port: PORT }
);

// Heartbeat function
const heartbeatFn = () => {
    const request = {
        num_cpu: os.cpus().length,
        cpu_clock_speed: os.cpus()[0].speed / 1000,
        memory_size: Math.round(os.totalmem() / (1024 * 1024)),
        memory_usage: Math.round((1 - os.freemem() / os.totalmem()) * 100),
        uptime: os.uptime(), // Uptime in seconds
    }
    client.heartbeat(request, (error, response) => {
        if (error) {
            console.error('Error sending heartbeat:', error);
            return;
        }
        console.log('Heartbeat response:', response);
    });
}

// Create a thread that periodically sends a heartbeat to the Load Balancer
setInterval(heartbeatFn, HEARTBEAT_INTERVAL);
heartbeatFn()