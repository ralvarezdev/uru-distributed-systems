const path = require('path');
const { createServer } = require('../wrappers/grpcServerWrapper');
const bookHandler = require('./handlers/bookHandler');
const config = require('../config/grpcConfig.json');

createServer(
    path.resolve(__dirname, '..', config.protoPath),
    config.packageName,
    config.serviceName,
    bookHandler,
    config.port,
    config.protoOptions
);
