Remove-Item build -Recurse

cd .\node-backend
npm install
ncc build .\src\index.js -o ..\build
cd ..

mkdir build\win-x64 -Force

dotnet publish TerminalPlugin.csproj --configuration Release -o build\plugin

mv .\build\build .\build\win-x64
mv .\build\index.js .\build\win-x64
mv .\build\plugin\TerminalPlugin.dll .\build\win-x64
