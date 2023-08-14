#!/bin/bash

ROOT_DIR=`pwd`

# Find AssemblyInfo.cs
ASSEMBLY_INFO=`find . -name AssemblyInfo.cs`
# Get version from AssemblyInfo.cs
VERSION=`grep AssemblyVersion $ASSEMBLY_INFO | cut -d \" -f 2`

BASE_DIR=bin/Release/Version_$VERSION

echo "Publishing version $VERSION to $BASE_DIR"

echo "Building portable version"
dotnet publish --configuration Release --output $BASE_DIR/portable

echo "Building self-contained linux-x64 version"
dotnet publish --configuration Release --output $BASE_DIR/linux-x64 --runtime linux-x64 --self-contained true -p:PublishSingleFile=true

echo "Zipping files"
cd $BASE_DIR/portable
tar -czf ../WebMonitor-$VERSION-portable.tar.gz *
cd ../linux-x64
tar -czf ../WebMonitor-$VERSION-linux-x64.tar.gz *

# Terminal plugin
cd $ROOT_DIR/../TerminalPlugin
./build.sh
# Find TerminalPlugin.csproj
TERMINAL_PLUGIN_CSPROJ=`find . -name TerminalPlugin.csproj`
# Get assembly version from TerminalPlugin.csproj (extract from <AssemblyVersion> tag)
TERMINAL_PLUGIN_VERSION=`grep AssemblyVersion $TERMINAL_PLUGIN_CSPROJ | cut -d \> -f 2 | cut -d \< -f 1`
# Get Node.js version
nodeVersion=$(node --version)
nodeVersion=${nodeVersion:1}
cd build/linux-x64-node_$nodeVersion
tar -czf "$ROOT_DIR/$BASE_DIR/TerminalPlugin-$TERMINAL_PLUGIN_VERSION-linux-x64-node_$nodeVersion.tar.gz" *