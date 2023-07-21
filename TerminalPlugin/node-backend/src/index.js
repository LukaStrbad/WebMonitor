const os = require('os');
const pty = require('node-pty');
const WebSocket = require('ws');

// Get port from arguments
const port = process.argv[2];

if (!port) {
    console.log("No port specified");
    process.exit(1);
}

const wss = new WebSocket.Server({ port: parseInt(port) });

console.log(`Terminal server started on port ${port}`);

const shell = os.platform() === 'win32' ? 'powershell.exe' : 'bash';

wss.on('connection', ws => {
    console.log("new session");

    const ptyProcess = pty.spawn(shell, [], {
        name: 'xterm-color',
        env: process.env
    });

    ws.on('message', command => {
        ptyProcess.write(command);
    });

    ptyProcess.on('data', data => {
        ws.send(data);
    });
});
