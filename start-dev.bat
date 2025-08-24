@echo off
setlocal enabledelayedexpansion

echo.
echo 🚀 Starting Task Management Development Environment
echo =================================================
echo.

:: Verificar dependencias
echo 📦 Checking dependencies...

:: Verificar Node.js
node --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Node.js no está instalado
    pause
    exit /b 1
)

:: Verificar .NET
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET no está instalado
    pause
    exit /b 1
)

:: Verificar Azure Functions Core Tools
func --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Azure Functions Core Tools no están instalados
    echo 💡 Instala con: npm install -g azure-functions-core-tools@4 --unsafe-perm true
    pause
    exit /b 1
)

echo ✅ All dependencies are available
echo.

:: Crear directorios necesarios
if not exist "azureFunctions\logs" mkdir "azureFunctions\logs"

echo 🔄 Starting services...
echo.

:: Iniciar Azurite
echo 🗄️  Starting Azurite (Azure Storage Emulator)...
start "Azurite" cmd /c "azurite --silent --location azureFunctions --debug azureFunctions\logs\azurite.log"
timeout /t 2 /nobreak >nul

:: Iniciar Azure Functions
echo ⚡ Starting Azure Functions...
start "Azure Functions" cmd /c "cd azureFunctions && func start --port 7071"
timeout /t 3 /nobreak >nul

:: Iniciar React
echo 🌐 Starting React Development Server...
start "React Client" cmd /c "cd client && npm run dev"
timeout /t 2 /nobreak >nul

echo.
echo 🎉 All services are starting up!
echo =================================================
echo 📋 Services:
echo    🗄️  Azurite (Storage):     http://localhost:10000
echo    ⚡ Azure Functions (API):  http://localhost:7071
echo    🌐 React Client:           http://localhost:5173
echo =================================================
echo.
echo 📁 Services are running in separate windows
echo 🛑 Close the windows manually to stop services
echo.
echo Press any key to exit this script (services will continue running)
pause >nul