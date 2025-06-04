import path from'path';
import { fileURLToPath } from 'url';
import createServer from'../grpc-wrapper/wrappers/server.js';
import loadBalancerHandler from'./loadBalancerHandler.js';
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
