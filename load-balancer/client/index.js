// IP and port of the API gateway
const IP = 'localhost'
const PORT = 8080

// Simulate an HTTP request
fetch(`http://${IP}:${PORT}/api/book/2`, {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json'
    },
}).then(response => {
    if (!response.ok)
        throw new Error(`HTTP error! status: ${response.status}`);
    return response.json();
}).then(json=>{
    console.log(json)
}).catch(error => {
    console.error('Error fetching book:', error);
})