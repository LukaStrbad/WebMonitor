const pty = require('node-pty');
const WebSocket = require('ws');
const readline = require('readline');

// Get port from arguments
const shell = process.argv[2];
const port = process.argv[3];

if (!shell) {
    console.log("No shell specified");
    process.exit(1);
}

if (!port) {
    console.log("No port specified");
    process.exit(1);
}

const wss = new WebSocket.Server({ port: parseInt(port) });

console.log(`Terminal server started on port ${port}`);

const ptyProcess = pty.spawn(shell, [], {
    name: 'xterm-color',
    env: process.env,
    cwd: process.env.HOME
});

// If shell exits, exit the process
ptyProcess.onExit(() => {
    process.exit(0);
});

/** @type {NodeJS.Timeout | null} */
let timeout = null;

/** @type {NodeJS.Timer | null} */
let interval = null;

wss.on('connection', ws => {
    ws.isAlive = true;
    // Stop timer on reconnection
    console.log("new session");
    ws.on('error', console.error);

    ws.on('message', command => {
        ptyProcess.write(command);
    });

    ptyProcess.on('data', data => {
        ws.send(data);
    });

    ws.on('pong', () => {
        ws.isAlive = true
    });

    ws.onclose = () => {
        console.log("Closed")
    }
});

interval = setInterval(() => {
    wss.clients.forEach(ws => {
        if (ws.isAlive === false)
            return ws.terminate();

        ws.isAlive = false;
        ws.ping();
    });

    const clientCount = wss.clients.size;
    let allDisconnected = true;
    wss.clients.forEach(ws => {
        if (ws.readyState === WebSocket.OPEN)
            allDisconnected = false;
    });

    if ((clientCount === 0 || allDisconnected)) {
        console.log("No clients connected");
        if (timeout == null) {
            console.log("Exiting in 120 seconds");
            timeout = setTimeout(() => {
                console.log("Exiting");
                process.exit(0);
            }, 120_000);
        }
    } else {
        console.log("At least one client connected");
        if (timeout !== null) {
            console.log("Clearing timeout");
            clearTimeout(timeout);
        }
    }
}, 30_000);

wss.on('close', () => {
    clearInterval(interval);
});

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: false
});

rl.on('line', line => {
    const split = line.split(":").map(x => x.trim());
    const [command, value] = split;

    if (command === "resize") {
        const [cols, rows] = value.split(" ");
        ptyProcess.resize(parseInt(cols), parseInt(rows));
    }
});
