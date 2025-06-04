import { fork } from 'child_process';
import path from 'path';
import {fileURLToPath} from "url";

// Get the directory name of the current module
const filename = fileURLToPath(import.meta.url);
const dirname = path.dirname(filename);

// Resolve the path to the server script
const serverScript = path.resolve(dirname, './server.js');

// Starting port of the server instances
const basePort = 50052;

for (let i = 0; i < 5; i++) {
    const port = basePort + i;
    const child = fork(serverScript, [port]);

    child.on('message', (msg) => {
        console.log(`Server on port ${port}:`, msg);
    });

    child.on('error', (err) => {
        console.error(`Error in server on port ${port}:`, err);
    });

    child.on('exit', (code) => {
        console.log(`Server on port ${port} exited with code ${code}`);
    });
}