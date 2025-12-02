const { spawn } = require('child_process');
const path = require('path');

const port = process.env.PORT || 4200;
console.log(`Starting Angular on port ${port}...`);

const cmd = process.platform === 'win32' ? 'ng.cmd' : 'ng';
const args = ['serve', '--port', port, '--disable-host-check', '--host', '0.0.0.0'];

const child = spawn(cmd, args, { stdio: 'inherit', shell: true });

child.on('error', (err) => {
  console.error('Failed to start child process.', err);
});

child.on('close', (code) => {
  process.exit(code);
});