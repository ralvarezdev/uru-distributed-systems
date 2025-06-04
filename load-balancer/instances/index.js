import { fork } from 'child_process';
import path from 'path';

const serverScript = path.resolve('./server.js'); // Path to the server script
const basePort = 50052; // Starting port

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