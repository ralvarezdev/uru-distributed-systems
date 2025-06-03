const express = require('express');
const cluster = require('cluster');
const os = require('os');
const numCPUs = os.cpus().length;

const port = 3000;

// Get system metrics
function getSystemMetrics() {
    return {
        cpuSpeed: os.cpus()[0].speed,
        freeMemory: os.freemem(),
    };
}

function getScore(metrics) {
    const maxCpuSpeed = 4000;
    const cpuScore = metrics.cpuSpeed / maxCpuSpeed;
    const totalMemory = os.totalmem();
    const memoryUsage = (totalMemory - metrics.freeMemory) / totalMemory;
    return (cpuScore * 0.5) + (memoryUsage * 0.5);
}

if (cluster.isMaster) {
    console.log(`Master ${process.pid} is running`);

    let workers = [];
    let resMap = {}; // Map resId to res

    // Fork workers
    for (let i = 0; i < numCPUs; i++) {
        const worker = cluster.fork();
        workers.push(worker);

        // Listen for responses from workers
        worker.on('message', (message) => {
            if (message.type === 'response') {
                const res = resMap[message.resId];
                if (res) {
                    res.send(message.data);
                    delete resMap[message.resId];
                }
            }
            if (message.type === 'metrics') {
                worker.metrics = message.metrics;
                worker.score = getScore(worker.metrics);
                console.log(
                    `\x1b[36m[Worker ${worker.process.pid}]\x1b[0m Score: \x1b[33m${worker.score.toFixed(4)}\x1b[0m`
                );
            }
        });
    }

    // Assign requests to the worker with the highest score
    const app = express();
    app.get('/key', (req, res) => {
        let bestWorker = workers.reduce((best, worker) => {
            if (!best || (worker.score || 0) > (best.score || 0)) {
                return worker;
            } else {
                return best;
            }
        }, null);

        if (bestWorker) {
            const resId = Math.random().toString(36).slice(2);
            resMap[resId] = res;
            bestWorker.send({
                type: 'request',
                req: { url: req.url, headers: req.headers, method: req.method },
                resId
            });
        } else {
            res.status(503).send('No available workers');
        }
    });

    app.listen(port, () => console.log(`Master listening on port ${port}`));

} else {
    // Worker process
    console.log(`Worker ${process.pid} started`);

    // Send metrics to master periodically
    setInterval(() => {
        const metrics = getSystemMetrics();
        process.send({ type: 'metrics', metrics: metrics });
    }, 5000);

    // Handle requests from the master
    process.on('message', (message) => {
        if (message.type === 'request') {
            process.send({
                type: 'response',
                resId: message.resId,
                workerId: process.pid,
                data: `Handled by worker ${process.pid}`
            });
        }
    });
}