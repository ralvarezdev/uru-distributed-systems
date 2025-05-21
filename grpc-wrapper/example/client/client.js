import path from 'path';
import createClient from '../../wrappers/client';
import config from 'config.json'

// Create gRPC client
const client = createClient(
    path.resolve(__dirname, '../proto/book.proto'),
    config
);

// Simulate a request
const request = { id: "2" };
client.getAllBooks(request, (error, response) => {
    if (error) {
        console.error('Error:', error);
        return;
    }
    console.log('Response:', response);
});
