# Task Management - Azure Functions (.NET 8) + Entity Framework Core

## Descripción

Sistema fullstack de gestión de tareas con backend en Azure Functions (.NET 8) y frontend en React + TypeScript. Incluye autenticación JWT, integración con Azure AD y soporte para SQL Server y SQLite.

## Arquitectura Implementada

### Stack Tecnológico
**Backend:**
- **Azure Functions v4** (isolated worker model)
- **.NET 8**
- **Entity Framework Core 8.0.8**
- **Microsoft.Data.SqlClient** (moderno)
- **SQL Server** (producción) / **SQLite** (desarrollo)
- **JWT Authentication** con Azure AD
- **Dependency Injection** nativo

**Frontend:**
- **React 18** con **TypeScript**
- **Vite** como build tool
- **Context API** para manejo de estado
- **CSS Modules** para estilos


## Instalación y Configuración

### Prerrequisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (instalado vía npm)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) o usar SQLite para desarrollo

### 1. Configuración de Base de Datos

#### Opción A: SQL Server (Recomendado para producción)
```bash
# Actualizar connection string en local.settings.json
"ConnectionStrings__DefaultConnection": "Server=localhost;Database=TaskManagementDB;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true;"
```

#### Opción B: SQLite (Desarrollo rápido)
```bash
# Copiar configuración de desarrollo (REQUERIDO)
cp azureFunctions/local.settings.dev.json azureFunctions/local.settings.json
```

### 2. Aplicar Migraciones de Entity Framework
```bash
cd azureFunctions

# Instalar herramientas EF Core (si no está instalado)
dotnet tool install --global dotnet-ef

# Verificar migraciones existentes
dotnet ef migrations list

# Aplicar migraciones (opcional, se aplican automáticamente al startup)
dotnet ef database update

# Para crear nuevas migraciones después de cambios en modelos
dotnet ef migrations add "NombreMigracion" -o Data/Migrations
```

### 3. Configurar Azurite (Storage Emulator)
```bash
# Instalar Azurite globalmente (REQUERIDO)
npm install -g azurite

# Ejecutar Azurite
azurite --silent --location c:\\azurite --debug c:\\azurite\\debug.log
```

### 4. Ejecutar la Aplicación

#### Opción 1: Con npm (Recomendado - Frontend + Backend)
```bash
# Instalar dependencias del frontend
npm run install:all

# Ejecutar frontend y backend simultáneamente
npm start
# o
npm run dev
```

#### Opción 2: Solo Backend (Azure Functions)
```bash
cd azureFunctions
dotnet build
func start
```

#### Opción 3: Solo Frontend (React + Vite)
```bash
cd client
npm run dev
```

**URLs de la aplicación:**
- Backend (Azure Functions): `http://localhost:7071`
- Frontend (React): `http://localhost:5173`

#### Endpoints Principales
- `GET /api/health` - Health check
- `GET /api/tasks` - Listar tareas del usuario
- `POST /api/tasks` - Crear nueva tarea
- `PUT /api/tasks/{id}` - Actualizar tarea
- `DELETE /api/tasks/{id}` - Eliminar tarea
- `GET /api/auth/callback` - Callback de autenticación OAuth

## Configuración de Variables de Entorno

### local.settings.json (Desarrollo)
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DbProvider": "SqlServer",
    "ConnectionStrings__DefaultConnection": "Server=localhost;Database=TaskManagementDB;Trusted_Connection=true;TrustServerCertificate=true;",
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
- Configurar connection string de SQL Azure

## Solución de Problemas Comunes

### Error: "Value cannot be null (Parameter 'provider')"
- **Causa**: DbProvider no está configurado o connection string falta
- **Solución**: Verificar `DbProvider` y `ConnectionStrings__DefaultConnection` en local.settings.json

### Error: "SQLite Error 1: 'no such table: Users'"
- **Causa**: Migraciones no aplicadas
- **Solución**: Las migraciones se aplican automáticamente al startup. Si persiste, ejecutar `dotnet ef database update`

### Error de Build: "TaskStatus ambiguous reference"
- **Causa**: Conflicto entre `System.Threading.Tasks.TaskStatus` y `TaskManagement.Models.TaskStatus`
- **Solución**: Ya resuelto con alias `using TaskStatus = TaskManagement.Models.TaskStatus;`


## Comandos Útiles de Entity Framework

```bash
# Ver estado de migraciones
dotnet ef migrations list

# Crear nueva migración
dotnet ef migrations add "NombreMigracion" -o Data/Migrations

# Aplicar migraciones
dotnet ef database update

# Revertir a migración específica
dotnet ef database update "NombreMigracion"

# Generar script SQL de migración
dotnet ef migrations script

# Eliminar migración (solo si no se aplicó)
dotnet ef migrations remove
```

## Características Implementadas

### ✅ Completado
- [x] Migración completa a Entity Framework Core
- [x] Configuración robusta de connection string (múltiples rutas)
- [x] DbInitializerHostedService para migraciones automáticas y seeding
- [x] Soporte multi-proveedor (SQL Server + SQLite)
- [x] Repositorios actualizados con EF Core (AsNoTracking, async/await)
- [x] Dependency Injection configurado correctamente
- [x] Frontend React + TypeScript integrado
- [x] Migraciones EF Core generadas

### 🔄 Pendiente/Recomendado
- [ ] Tests unitarios e integración
- [ ] CI/CD pipeline (GitHub Actions/Azure DevOps)
- [ ] Connection pooling y resiliency patterns
- [ ] Health checks más robustos
- [ ] Monitoring y logging estructurado (Serilog)
- [ ] Rate limiting y throttling
- [ ] Caching con Redis
- [ ] Validación de entrada más robusta

## Evaluación Arquitectónica Final

### Decisiones Técnicas Tomadas
1. **EF Core sobre Dapper**: Mayor productividad, type safety, migraciones automáticas
2. **Microsoft.Data.SqlClient**: Reemplazo moderno de System.Data.SqlClient
3. **HostedService**: Aplicación automática de migraciones al startup
4. **Multi-proveedor**: Flexibilidad entre SQL Server (prod) y SQLite (dev)
5. **Connection string robusta**: 4 rutas de resolución para máxima compatibilidad

### Riesgos Identificados
1. **Migraciones en startup**: Puede causar timeouts en startups lentos
2. **SQLite en producción**: No recomendado para cargas altas
3. **Secrets en local.settings.json**: Solo para desarrollo
4. **Connection resiliency**: Implementar retry patterns adicionales
5. **Multi-tenancy**: Arquitectura actual no está preparada

### Próximos Pasos Recomendados
1. Implementar tests automatizados
2. Configurar pipeline CI/CD
3. Agregar monitoring y alertas
4. Implementar connection pooling avanzado
5. Migrar secrets a Azure Key Vault

## Soporte y Mantenimiento

Para problemas o mejoras, seguir este orden:
1. Verificar logs de Azure Functions
2. Revisar estado de migraciones EF Core
3. Validar configuración de connection strings
4. Comprobar estado de Azurite/Storage
5. Verificar versiones de .NET y Azure Functions tools