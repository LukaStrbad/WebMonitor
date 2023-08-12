#!/bin/bash

rm -rf build

cd node-backend
rm -rf node_modules
npm install
ncc build ./src/index.js -o ../build
cd ..

mkdir -p build/linux-x64

dotnet publish TerminalPlugin.csproj --configuration Release -o build/plugin

mv build/build build/linux-x64
mv build/index.js build/linux-x64
mv build/plugin/TerminalPlugin.dll build/linux-x64