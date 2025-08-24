# TaskManagement

## Setup

1. Copy `local.settings.dev.json` to `local.settings.json`
2. Install Azurite globally: `npm install -g azurite`
3. Start Azurite for local Azure Storage emulation
4. The application uses SQLite for data storage

## Running the Application

1. Start Azurite (if not already running):
   ```bash
   azurite --silent --location c:\azurite --debug c:\azurite\debug.log
   ```

2. Start the Azure Functions runtime:
   ```bash
   func start
   ```

3. The functions will be available at `http://localhost:7071`

## Prerequisites

- Azure Functions Core Tools
- Node.js
- .NET 6.0 or later
