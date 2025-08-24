#!/bin/bash

# Colores para los logs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Starting Task Management Development Environment${NC}"
echo -e "${BLUE}=================================================${NC}"

# Función para limpiar procesos al salir
cleanup() {
    echo -e "\n${RED}🛑 Stopping all services...${NC}"
    kill $(jobs -p) 2>/dev/null
    exit
}

# Capturar Ctrl+C
trap cleanup INT

# Verificar dependencias
echo -e "${YELLOW}📦 Checking dependencies...${NC}"

# Verificar Node.js
if ! command -v node &> /dev/null; then
    echo -e "${RED}❌ Node.js no está instalado${NC}"
    exit 1
fi

# Verificar .NET
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET no está instalado${NC}"
    exit 1
fi

# Verificar Azure Functions Core Tools
if ! command -v func &> /dev/null; then
    echo -e "${RED}❌ Azure Functions Core Tools no están instalados${NC}"
    echo -e "${YELLOW}💡 Instala con: npm install -g azure-functions-core-tools@4 --unsafe-perm true${NC}"
    exit 1
fi

# Verificar Azurite
if ! command -v azurite &> /dev/null; then
    echo -e "${YELLOW}⚠️  Azurite no está instalado globalmente, instalando localmente...${NC}"
    npm install azurite@latest
fi

echo -e "${GREEN}✅ All dependencies are available${NC}"

# Crear directorios necesarios
mkdir -p azureFunctions/logs

echo -e "${BLUE}🔄 Starting services...${NC}"

# Iniciar Azurite
echo -e "${BLUE}🗄️  Starting Azurite (Azure Storage Emulator)...${NC}"
if command -v azurite &> /dev/null; then
    azurite --silent --location azureFunctions --debug azureFunctions/logs/azurite.log &
else
    npx azurite --silent --location azureFunctions --debug azureFunctions/logs/azurite.log &
fi
AZURITE_PID=$!
sleep 2

# Iniciar Azure Functions
echo -e "${YELLOW}⚡ Starting Azure Functions...${NC}"
cd azureFunctions
func start --port 7071 > logs/functions.log 2>&1 &
FUNCTIONS_PID=$!
cd ..
sleep 3

# Iniciar React
echo -e "${GREEN}🌐 Starting React Development Server...${NC}"
cd client
npm run dev > ../azureFunctions/logs/client.log 2>&1 &
CLIENT_PID=$!
cd ..

# Esperar un poco para que todo arranque
sleep 3

echo -e "${GREEN}🎉 All services are starting up!${NC}"
echo -e "${BLUE}=================================================${NC}"
echo -e "📋 Services:"
echo -e "   🗄️  Azurite (Storage):     ${YELLOW}http://localhost:10000${NC}"
echo -e "   ⚡ Azure Functions (API):  ${YELLOW}http://localhost:7071${NC}"
echo -e "   🌐 React Client:           ${YELLOW}http://localhost:5173${NC}"
echo -e "${BLUE}=================================================${NC}"
echo -e "📁 Logs available in: ${YELLOW}azureFunctions/logs/${NC}"
echo -e "🛑 Press ${RED}Ctrl+C${NC} to stop all services"
echo ""

# Función para mostrar logs en tiempo real
show_logs() {
    echo -e "${BLUE}📊 Real-time logs (last 10 lines):${NC}"
    echo -e "${BLUE}===================================${NC}"
    
    while true; do
        clear
        echo -e "${BLUE}🚀 Task Management - Development Environment${NC}"
        echo -e "${BLUE}============================================${NC}"
        echo -e "Services: 🗄️  Azurite | ⚡ Functions | 🌐 React"
        echo -e "URLs: ${YELLOW}http://localhost:7071${NC} (API) | ${YELLOW}http://localhost:5173${NC} (Client)"
        echo -e "${BLUE}============================================${NC}"
        
        if [ -f "azureFunctions/logs/functions.log" ]; then
            echo -e "${YELLOW}📋 Azure Functions (last 5 lines):${NC}"
            tail -5 azureFunctions/logs/functions.log 2>/dev/null | sed 's/^/   /'
        fi
        
        if [ -f "azureFunctions/logs/client.log" ]; then
            echo -e "${GREEN}🌐 React Client (last 5 lines):${NC}"
            tail -5 azureFunctions/logs/client.log 2>/dev/null | sed 's/^/   /'
        fi
        
        echo -e "${BLUE}============================================${NC}"
        echo -e "🛑 Press ${RED}Ctrl+C${NC} to stop all services"
        
        sleep 5
    done
}

# Mostrar logs
show_logs &
LOGS_PID=$!

# Esperar indefinidamente
wait