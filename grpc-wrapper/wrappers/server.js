import grpc from '@grpc/grpc-js';
import loadProto from "./proto";

// Create insecure gRPC server
function createInsecureServer(protoPath, implementation, config = {}) {
    // Load protobuf
    const proto = loadProto(protoPath, config?.protoConfig);
    const service = proto[config?.packageName][config?.serviceName].service;

    // Create server
    const server = new grpc.Server();
    server.addService(service, implementation);

    // Start the server
    const port = config?.port || 50051;
    const host = '0.0.0.0:' + port
    server.bindAsync(host, grpc.ServerCredentials.createInsecure(), (err, bindPort) => {
        if (err) {
            console.error('Error binding server:', err);
            return;
        }
        console.log(`gRPC Server running on http://${host}`);
    });
}
export default createInsecureServer