import path from 'path';
import createClient from '../../wrappers/client.js';
import config from './config.json' with { type: 'json' };
import {fileURLToPath} from "url";

/// Get the directory name of the current module
const filename = fileURLToPath(import.meta.url);
const dirname = path.dirname(filename);

// Create gRPC client
const index = createClient(
    path.resolve(dirname, '../proto/book.proto'),
    config
);

// Simulate a request
const request = { id: "2" };
index.getAllBooks(request, (error, response) => {
    if (error) {
        console.error('Error:', error);
        return;
    }
    console.log('Response:', response);
});
