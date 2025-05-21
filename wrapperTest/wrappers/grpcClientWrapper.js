const path = require('path');
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');

function loadProto(protoPath) {
    const packageDefinition = protoLoader.loadSync(protoPath, {
        keepCase: true,
        longs: String,
        enums: String,
        defaults: true,
        oneofs: true
    });
    return grpc.loadPackageDefinition(packageDefinition);
}

function createClient(protoPath, packageName, serviceName, target = 'localhost:50051') {
    const proto = loadProto(protoPath);
    const ServiceClient = proto[packageName][serviceName];

    return new ServiceClient(target, grpc.credentials.createInsecure());
}

module.exports = { createClient };
