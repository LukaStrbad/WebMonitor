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

wss.on('connection', ws => {
    console.log("new session");

    ws.on('message', command => {
        ptyProcess.write(command);
    });

    ptyProcess.on('data', data => {
        ws.send(data);
    });

    ws.on('close', () => {
        process.exit(0);
    });
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
