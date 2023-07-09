#!/bin/bash

# Find AssemblyInfo.cs
ASSEMBLY_INFO=`find . -name AssemblyInfo.cs`
# Get version from AssemblyInfo.cs
VERSION=`grep AssemblyVersion $ASSEMBLY_INFO | cut -d \" -f 2`

BASE_DIR=bin/Release/Version_$VERSION

echo "Publishing version $VERSION to $BASE_DIR"

echo "Building portable version"
dotnet publish --configuration Release --output $BASE_DIR/portable

echo "Building self-contained linux-x64 version"
# Allow trimming on linux because there are no COM dependencies on linux
dotnet publish --configuration Release --output $BASE_DIR/linux-x64 --runtime linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

echo "Zipping files"
cd $BASE_DIR/portable
tar -czf ../WebMonitor-$VERSION-portable.tar.gz *
cd ../linux-x64
tar -czf ../WebMonitor-$VERSION-linux-x64.tar.gz *