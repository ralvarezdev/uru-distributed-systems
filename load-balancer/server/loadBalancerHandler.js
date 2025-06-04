// Heartbeat timeout
const HEARTBEAT_TIMEOUT = 15000;

// Property score percentage
const PROPERTY_SCORE_PERCENTAGE = {
    numCPU: 0.2,
    cpuClockSpeed: 0.2,
    uptime: 0.2,
    memorySize: 0.2,
    memoryUsage: 0.2,
}

// Max properties registered
const maxProperties = new Map([
    ['numCPU', 0],
    ['cpuClockSpeed', 0],
    ['uptime', 0],
    ['memorySize', 0],
    ['memoryUsage', 0],
]);

// Instances map
const instancesProperties = new Map();
const instancesLastHeartbeat = new Map();

// Instances score
const instancesScore = new Map();

// Instances list
let instanceList = [];
let currentIndex = 0;

// Update the instance list based on scores and availability
function updateInstanceList() {
    const now = Date.now();

    // Remove instances that have not sent a heartbeat within the timeout period
    instancesLastHeartbeat.forEach((lastHeartbeat, key) => {
        if (now - lastHeartbeat > HEARTBEAT_TIMEOUT) {
            instancesProperties.delete(key);
            instancesLastHeartbeat.delete(key);
            instancesScore.delete(key);
        }
    });

    // Filter and sort instances by score
    instanceList = Array.from(instancesScore.entries())
        .sort((a, b) => b[1] - a[1]) // Sort by score descending
        .map(([key]) => key);
}

// Get the next available instance key using round-robin
export function getNextInstance() {
    if (instanceList.length === 0) {
        return null; // No available instances
    }

    const instance = instanceList[currentIndex];
    currentIndex = (currentIndex + 1) % instanceList.length; // Move to the next instance
    const instanceProperties = instancesProperties.get(instance);
    return {
        ip: instanceProperties.ip,
        port: instanceProperties.port,
    }
}

export default {
    heartbeat: (call, callback) => {
        // Extract the client's IP and port
        const req = call.request;
        const peer = call.getPeer();
        console.log(`Heartbeat request from ${peer}:`, req);
        const [ip, port] = peer.split(':')

        // Generate a unique key for the instance
        const instanceKey = `${ip}:${port}`;

        // Register heartbeat
        instancesLastHeartbeat.set(instanceKey, Date.now());

        // Register or update instance
        const instanceProperties = {
            ip,
            port,
            numCPU: req.num_cpu,
            cpuClockSpeed: req.cpu_clock_speed,
            uptime: req.uptime,
            memorySize: req.memory_size,
            memoryUsage: req.memory_usage,
        }
        instancesProperties.set(instanceKey, instanceProperties)

        // Update max properties if necessary
        maxProperties.forEach((value, key) => {
            if (key==='ip' || key==='port') return; // Skip IP and port
            if (req[key] > value) {
                maxProperties.set(key, req[key]);
            }
        });

        // Calculate score for the instance
        let score = 0;
        Object.keys(instanceProperties).forEach((property) => {
            const value = instanceProperties[property]
            const maxValue = maxProperties.get(property);
            const percentage = PROPERTY_SCORE_PERCENTAGE[property]
            if (maxValue > 0)
                score += (value / maxValue) * percentage;
        });
        instancesScore.set(instanceKey, score);

        // Update the instance list
        updateInstanceList();

        // Log
        console.log(`Heartbeat received from ${instanceKey}:`, instanceProperties);

        return callback(null, {
            ip,
            port,
            message: `Heartbeat received from ${instanceKey}`,
        })
    },
    getNextInstance: (call, callback) => {
        return callback(null, {
            ...getNextInstance()
        })
    }
}