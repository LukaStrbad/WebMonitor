if (Test-Path build)
{
    Remove-Item build -Recurse
}

Set-Location .\node-backend
Remove-Item -Recurse .\node_modules
npm install
npx ncc build .\src\index.js -o ..\build
Set-Location ..

# Get node version and trim leading 'v'
$nodeVersion = (node -v).TrimStart("v")

$winDir = ".\build\win-x64-node_$nodeVersion"

mkdir $winDir -Force

dotnet publish TerminalPlugin.csproj --configuration Release -o build\plugin

Move-Item .\build\build $winDir
Move-Item .\build\index.js $winDir
Move-Item .\build\plugin\TerminalPlugin.dll $winDir
Write-Output $nodeVersion > $winDir\node-version.txt
