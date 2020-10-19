#!/usr/bin/env node

const fs = require('fs')
const { getProjectFiles, getProjectPath } = require('./lib')

const files = getProjectFiles()
files.forEach(link)


function link(file) {
    console.log('file:', file)
    let contents = fs.readFileSync(file).toString()
    const re = /Include="(Annium[^"]*)"/g
    const references = Array.from(contents.matchAll(re)).map(x => x[1])
    for (const reference of references) {
        const path = getProjectPath(reference).trim()
        contents = contents.replace(
            `<PackageReference Include="${reference}" Version="0.1.0" />`,
            `<ProjectReference Include="${path}" />`
        )
    }
    fs.writeFileSync(file, contents)
}
