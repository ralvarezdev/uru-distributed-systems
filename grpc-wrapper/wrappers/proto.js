import grpc from '@grpc/grpc-js'
import protoLoader from '@grpc/proto-loader'

// Load protobuf
function loadProto(protoPath, protoConfig={}) {
    const packageDefinition = protoLoader.loadSync(protoPath, {
        keepCase: protoConfig?.keepCase || true,
        longs: protoConfig?.longs || String,
        enums: protoConfig?.enums || String,
        defaults: protoConfig?.defaults || true,
        oneofs: protoConfig?.oneofs || true
    });
    return grpc.loadPackageDefinition(packageDefinition);
}
export default loadProto