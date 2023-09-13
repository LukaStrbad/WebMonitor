#!/bin/bash

rm -rf build

cd node-backend
rm -rf node_modules
npm install
npx ncc build ./src/index.js -o ../build
cd ..

# Get node version and trim leading 'v'
nodeVersion=$(node --version)
nodeVersion=${nodeVersion:1}

linuxDir="linux-x64-node_$nodeVersion"

mkdir -p build/$linuxDir

dotnet publish TerminalPlugin.csproj --configuration Release -o build/plugin

mv build/build build/$linuxDir
mv build/index.js build/$linuxDir
mv build/plugin/TerminalPlugin.dll build/$linuxDir
echo $nodeVersion > build/$linuxDir/node-version.txt
