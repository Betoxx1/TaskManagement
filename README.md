# Task Management

## Prerrequisitos

**Para el Backend:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite)

**Para el Frontend:**
- [Node.js 18+](https://nodejs.org/)

## Instalación y Configuración

### 1. Configurar Backend
```bash
# Instalar Azurite globalmente
npm install -g azurite

# Copiar archivo de configuración (REQUERIDO)
cp azureFunctions/local.settings.dev.json azureFunctions/local.settings.json
```

### 2. Instalar dependencias del Frontend
```bash
# Instalar paquetes del frontend
npm run install:all
```

### 3. Ejecutar la aplicación
```bash
# Ejecutar frontend y backend simultáneamente
npm start
```

**URLs:**
- Backend: `http://localhost:7071`
- Frontend: `http://localhost:5173`

## Configuración de Variables de Entorno

### local.settings.json (Desarrollo)
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DbProvider": "SQLite",
    "ConnectionStrings__DefaultConnection": "Data Source=taskmanagement.db",
    "Jwt__SecretKey": "your-super-secret-key-here-at-least-32-characters-long",
    "Jwt__Issuer": "TaskManagement",
    "Jwt__Audience": "TaskManagement.Users",
    "Jwt__ExpirationMinutes": "60",
    "AzureAd__ClientId": "your-azure-ad-client-id",
    "AzureAd__ClientSecret": "your-azure-ad-client-secret",
    "AzureAd__TenantId": "your-azure-ad-tenant-id",
    "AzureAd__RedirectUri": "http://localhost:7071/api/auth/callback"
  }
}
```

### Variables de Producción (Azure)
- Configurar en Application Settings del Function App
- Usar Azure Key Vault para secretos sensibles
- Para producción, cambiar a SQL Azure y actualizar el ConnectionString y DbProvider