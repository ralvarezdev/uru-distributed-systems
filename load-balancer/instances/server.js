import createServer from'../../grpc-wrapper/wrappers/server.js';
import bookHandler from'../../grpc-wrapper/example/server/handlers/bookHandler.js';
import path from 'path';
import createClient from '../../grpc-wrapper/wrappers/client.js';
import {fileURLToPath} from "url";
import * as os from "node:os";
import loadBalancerClientConfig from './clientConfig.json' with { type: 'json' };

export function createServer(bookConfig = {
    heartbeatInterval: 5000,
    packageName: "book",
    serviceName: "BOOK_SERVICE",
    port: 50052,
    protoOptions:{
      keepCase: true,
      longs: "String",
      enums: "String",
      defaults: true,
      oneofs: true
    }
}) {
    // Get the directory name of the current module
    const filename = fileURLToPath(import.meta.url);
    const dirname = path.dirname(filename);

    // Create Load Balancer gRPC client
    const client = createClient(
        path.resolve(dirname, './loadBalancer.proto'),
        loadBalancerClientConfig
    );

    // Create gRPC server
    createServer(
        path.resolve(dirname, '../../grpc-wrapper/wrappers/proto/book.proto'),
        bookHandler,
        bookConfig
    );

    // Create a thread that periodically sends a heartbeat to the Load Balancer
    setInterval(() => {
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
    }, bookConfig.heartbeatInterval);
}

