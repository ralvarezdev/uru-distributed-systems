import grpc from '@grpc/grpc-js'
import loadProto from "./proto.js";

// Create insecure gRPC client
function createInsecureClient(protoPath, config={}) {
    // Load protobuf
    const proto = loadProto(protoPath, config?.protoConfig);
    const ServiceClient = proto[config?.packageName][config?.serviceName];

    // Create client
    return new ServiceClient(config?.target, grpc.credentials.createInsecure());
}
export default createInsecureClient;