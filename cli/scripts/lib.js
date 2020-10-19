const path = require('path')
const cp = require('child_process')

function getProjectFiles() {
    return exec(`find ${path.dirname(__dirname)} -type f -name '*.csproj'`).trim().split('\n')
}

function getProjectPath(name) {
    return cp.execSync(`find ${process.argv[2]} -type f -name ${name}.csproj`).toString()
}

function exec(cmd) {
    return cp.execSync(cmd).toString()
}

exports.getProjectFiles = getProjectFiles
exports.getProjectPath = getProjectPath