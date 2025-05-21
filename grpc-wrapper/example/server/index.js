import path from'path';
import { fileURLToPath } from 'url';
import createServer from'../../wrappers/server.js';
import bookHandler from'./handlers/bookHandler.js';
import config from './config.json' with { type: 'json' };

/// Get the directory name of the current module
const filename = fileURLToPath(import.meta.url);
const dirname = path.dirname(filename);

// Create gRPC server
createServer(
    path.resolve(dirname, '../proto/book.proto'),
    bookHandler,
    config
);
