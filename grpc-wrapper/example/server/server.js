import path from'path';
import createServer from'../../wrappers/server';
import bookHandler from'./handlers/bookHandler';
import config from'config.json';

// Create gRPC server
createServer(
    path.resolve(__dirname, '../proto/book.proto'),
    bookHandler,
    config
);
