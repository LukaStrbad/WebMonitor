const { env } = require('process');

const target = process.env["services__webmonitor__http__0"] || "http://127.0.0.1:5002";

const PROXY_CONFIG = [
  {
    context: [
      "/SysInfo",
      "/FileBrowser",
      "/Manager",
      "/Terminal",
      "/User"
   ],
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    }
  }
]

module.exports = PROXY_CONFIG;
