const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');

function loadProto(protoPath, options = {}) {
    const packageDefinition = protoLoader.loadSync(protoPath, options);
    return grpc.loadPackageDefinition(packageDefinition);
}

function createServer(protoPath, packageName, serviceName, implementation, port = '0.0.0.0:50051', protoOptions = {}) {
    const proto = loadProto(protoPath, protoOptions);
    const service = proto[packageName][serviceName].service;

    const server = new grpc.Server();
    server.addService(service, implementation);

    server.bindAsync(port, grpc.ServerCredentials.createInsecure(), (err, bindPort) => {
        if (err) {
            console.error('Error binding server:', err);
            return;
        }
        console.log(`gRPC Server running on http://${port}`);
        server.start();
    });
}

module.exports = { createServer };
