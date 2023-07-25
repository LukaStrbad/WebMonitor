# Find AssemblyInfo.cs
$assemblyInfo = Get-ChildItem -Path . -Recurse -Filter "AssemblyInfo.cs" -ErrorAction SilentlyContinue
# Get version from AssemblyInfo.cs
$version = (Get-Content $assemblyInfo | Select-String "AssemblyVersion" -Context 0, 1).Context.PostContext[0].Split('"')[1]

$baseDir = "bin\Release\Version_$version"

Write-Output "Publishing version $version to $baseDir"

Write-Output "Building portable version"
dotnet publish --configuration Release --output "$baseDir\portable"

Write-Output "Building self-contained win-x64 version"
dotnet publish --configuration Release --output "$baseDir\win-x64" --runtime win-x64 --self-contained true -p:PublishSingleFile=true

Write-Output "Zipping files"
Compress-Archive -Path "$baseDir\portable\*" -DestinationPath "$baseDir\WebMonitor-$version-portable.zip" -CompressionLevel Optimal -Update
Compress-Archive -Path "$baseDir\win-x64\*" -DestinationPath "$baseDir\WebMonitor-$version-win-x64.zip" -CompressionLevel Optimal -Update

# Terminal plugin
cd ..\TerminalPlugin
.\build.ps1
# Find TerminalPlugin.csproj
$terminalPluginCsproj = Get-ChildItem -Path . -Recurse -Filter "TerminalPlugin.csproj" -ErrorAction SilentlyContinue
# Get assembly version from TerminalPlugin.csproj
$terminalPluginVersion = (Get-Content $terminalPluginCsproj | Select-String "AssemblyVersion" -Context 0, 1).Context.PostContext[0].Split('>')[1].Split('<')[0]
Compress-Archive -Path "build\win-x64\*" -DestinationPath "..\WebMonitor\$baseDir\TerminalPlugin-$terminalPluginVersion-win-x64.zip" -CompressionLevel Optimal -Update