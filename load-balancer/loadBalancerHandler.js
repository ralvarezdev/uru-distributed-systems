// Heartbeat timeout
const HEARTBEAT_TIMEOUT = 10000;

// Property score percentage
const propertyScorePercentage = {
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

    // Filter and sort instances by score
    instanceList = Array.from(instancesScore.entries())
        .filter(([key]) => now - (instancesLastHeartbeat.get(key) || 0) <= HEARTBEAT_TIMEOUT)
        .sort((a, b) => b[1] - a[1]) // Sort by score descending
        .map(([key]) => key);
}

// Get the next available instance using round-robin
function getNextInstance() {
    if (instanceList.length === 0) {
        return null; // No available instances
    }

    const instance = instanceList[currentIndex];
    currentIndex = (currentIndex + 1) % instanceList.length; // Move to the next instance
    return instance;
}

// Example usage
setInterval(() => {
    updateInstanceList();
    const nextInstance = getNextInstance();
    if (nextInstance) {
        console.log(`Selected instance: ${nextInstance}`);
    } else {
        console.log('No available instances');
    }
}, 5000); // Update and select every 5 seconds

export default {
    heartbeat: (call, callback) => {
        // Extracting request metadata
        const req = call.request
        const ip = req.ip || req.connection.remoteAddress;
        const port = req.connection.remotePort;

        // Generate a unique key for the instance
        const instanceKey = `${ip}:${port}`;

        // Register heartbeat
        instancesLastHeartbeat.set(instanceKey, Date.now());

        // Register or update instance
        const instanceProperties = {
            numCPU: req.num_cpu,
            cpuClockSpeed: req.cpu_clock_speed,
            uptime: req.uptime,
            memorySize: req.memory_size,
            memoryUsage: req.memory_usage,
        }
        instancesProperties.set(instanceKey, instanceProperties)

        // Update max properties if necessary
        maxProperties.forEach((value, key) => {
            if (req[key] > value) {
                maxProperties.set(key, req[key]);
            }
        });

        // Calculate score for the instance
        let score = 0;
        Object.keys(instanceProperties).forEach((property) => {
            const value = instanceProperties[property]
            const maxValue = maxProperties.get(property);
            const percentage = propertyScorePercentage[property]
            if (maxValue > 0)
                score += (value / maxValue) * percentage;
        });
        instancesScore.set(instanceKey, score);

        // Update the instance list
        updateInstanceList();

        return callback(null, {
            success: true,
        })
    }
}