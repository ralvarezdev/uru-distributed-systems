const path = require('path');
const { createClient } = require('../wrappers/grpcClientWrapper');
const config = require('../config/grpcConfig.json');

const client = createClient(
    path.resolve(__dirname, '..', config.protoPath),
    config.packageName,
    config.serviceName,
    '127.0.0.1:50051',
    config.protoOptions
);

const request = { id: "2" };

client.getAllBooks(request, (error, response) => {
    if (error) {
        console.error('Error:', error);
        return;
    }
    console.log('Response:', response);
});
