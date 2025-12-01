# Project Overview

WebMonitor is a comprehensive system monitoring solution that provides a web-based interface to view real-time statistics of the host machine. It uses a split architecture with a high-performance .NET backend and a modern Angular frontend.

**Key Features:**
*   **System Stats:** Monitoring of CPU, Memory, Disk, Network, GPU, and Battery.
*   **Process Management:** View and manage running processes.
*   **File Browser:** Navigate and manage the server's file system.
*   **Terminal:** Integrated web-based terminal access (via `TerminalPlugin`).
*   **Extensibility:** Plugin system (e.g., `TerminalPlugin`).

## Tech Stack

*   **Backend:** .NET 10.0 (ASP.NET Core Web API)
    *   `LibreHardwareMonitorLib` for hardware metrics.
    *   Entity Framework Core (SQLite) for data persistence (Users, Settings).
    *   `Microsoft.AspNetCore.SpaProxy` for integrated frontend development.
*   **Frontend:** Angular 17
    *   Bootstrap 5 & Material Design.
    *   xterm.js for the terminal interface.
*   **Terminal Plugin:**
    *   C# Plugin wrapper.
    *   Node.js backend (`node-pty`, `ws`) for handling PTY sessions.

# Building and Running

## Prerequisites
*   .NET 7.0 SDK
*   Node.js (LTS recommended)
*   Angular CLI (optional, as scripts delegate to local installation)

## Development

The project is configured to run the frontend seamlessly alongside the backend.

### Run the full application (Backend + Frontend)
From the project root:
```bash
dotnet run --project WebMonitor/WebMonitor.csproj
```
*This command starts the ASP.NET Core server, which in turn starts the Angular development server (`npm start`) via the `SpaProxy`.*

### Run Frontend Independently
If you need to work on the frontend specifically:
```bash
cd WebMonitor/ClientApp
npm start
```

### Run Tests
```bash
dotnet test
```

## Release / Production Build

The project includes scripts to create portable and self-contained builds.

**Windows:**
```powershell
cd WebMonitor
.\release.ps1
```

**Linux:**
```bash
cd WebMonitor
./release.sh
```
*These scripts publish the .NET application, build the frontend, and package the Terminal Plugin.*

# Architecture & Directory Structure

*   `WebMonitor/` - Main ASP.NET Core application.
    *   `Controllers/` - API endpoints.
    *   `Native/` - Hardware monitoring implementations (`SysInfo`, `Cpu`, `Memory`, etc.).
    *   `ClientApp/` - Angular frontend source code.
*   `TerminalPlugin/` - Separate project for the terminal functionality.
    *   `node-backend/` - Node.js server for PTY management.
*   `WebMonitorTests/` - xUnit test suite.

# Development Conventions

*   **Code Style:** Follow standard C# naming conventions (PascalCase for public members, camelCase for locals).
*   **Frontend:** Angular strict mode is enabled. Adhere to TypeScript best practices.
*   **Configuration:** Application settings are in `appsettings.json`.
*   **Git:** `release.ps1/sh` scripts read versions from `AssemblyInfo.cs`. Ensure this is updated before release.
