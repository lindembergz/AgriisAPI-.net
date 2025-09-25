const shell = require('shelljs');

module.exports = {
  execute: command => new Promise((ffl, rej) => {
    const {code, stdout, stderr} = shell.exec(command);
    code === 0 ? ffl(stdout) : rej(stdout || stderr)
  }),
  which: (command, error) => new Promise((ffl, rej) => {
    if (!shell.which(command)) {
      ffl(error);
    } else {
      const {code, stdout, stderr} = shell.which(command);
      code === 0 ? ffl(stdout) : rej(stdout || stderr)
    }
  }),
  mkdir: paths => new Promise((ffl, rej) => {
    const {code, stdout, stderr} = shell.mkdir('-p', paths);
    code === 0 ? ffl(stdout) : rej(stdout || stderr)
  }),
  mv: (source, destination) => new Promise((ffl, rej) => {
    const {code, stdout, stderr} = shell.mv(source, destination);
    code === 0 ? ffl(stdout) : rej(stdout || stderr)
  })
};
