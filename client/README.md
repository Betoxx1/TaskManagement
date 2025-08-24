# 📋 Task Management - Cliente Frontend

Aplicación React con TypeScript para gestión de tareas con autenticación Azure AD y diseño moderno inspirado en Microsoft To Do.

## 🚀 Características Principales

- ✅ **Autenticación Azure AD OAuth2** - Login seguro con Microsoft
- ✅ **Gestión completa de tareas** - CRUD con filtros avanzados
- ✅ **Diseño Microsoft To Do** - UI moderna y profesional
- ✅ **Interfaz responsiva** - Adaptada para desktop, tablet y móvil
- ✅ **TypeScript** - Tipado fuerte para mayor seguridad
- ✅ **Iconos profesionales** - Lucide React (sin emojis del sistema)
- ✅ **Estados y prioridades** - Sistema completo de categorización
- ✅ **Filtros inteligentes** - Búsqueda por texto, estado, prioridad y fechas
- ✅ **Validación en tiempo real** - Formularios con validación instantánea
- ✅ **Operaciones en lote** - Selección múltiple para acciones masivas

## 📋 Prerrequisitos

- **Node.js** (versión 16 o superior)
- **npm** o **yarn**
- **Azure AD Application** configurada
- **Backend Azure Functions** ejecutándose

## ⚙️ Configuración del Proyecto

### 1. Instalar dependencias

```bash
npm install
```

### 2. Configurar variables de entorno

Crea un archivo `.env` en la raíz del proyecto:

```env
# URL base de la API del backend
VITE_API_BASE_URL=http://localhost:7071/api

# Configuración de Azure AD
VITE_AZURE_CLIENT_ID=tu-client-id-aqui
VITE_AZURE_TENANT_ID=tu-tenant-id-aqui

# URL de redirección
VITE_REDIRECT_URI=http://localhost:7071/api/auth/callback
```

### 3. Configurar Azure AD

1. Ve al [Portal de Azure](https://portal.azure.com)
2. Busca **"Azure Active Directory"**
3. Ve a **"App registrations"** > **"New registration"**
4. Configura:
   - **Name**: Task Management Client
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: Web - `http://localhost:7071/api/auth/callback`
5. Copia el **Application (client) ID** → `VITE_AZURE_CLIENT_ID`
6. Copia el **Directory (tenant) ID** → `VITE_AZURE_TENANT_ID`
7. Ve a **"API permissions"** y configura permisos:
   - `openid`
   - `profile` 
   - `email`

## 🚀 Ejecutar la Aplicación

### Modo desarrollo

```bash
npm run dev
```

La aplicación estará disponible en `http://localhost:3001` (o el puerto disponible)

### Construir para producción

```bash
npm run build
```

### Preview de producción

```bash
npm run preview
```

## 📁 Estructura del Proyecto

```
src/
├── components/              # Componentes reutilizables
│   ├── ProtectedRoute.tsx   # Rutas protegidas
│   ├── TaskForm.tsx         # Formulario crear/editar tareas
│   ├── TaskList.tsx         # Lista principal de tareas
│   ├── TaskListItem.tsx     # Item individual de tarea
│   └── TaskFilters.tsx      # Filtros avanzados
├── contexts/                # Contextos de React
│   └── AuthContext.tsx      # Contexto de autenticación
├── pages/                   # Páginas principales
│   ├── LoginPage.tsx        # Página de login
│   ├── AuthCallbackPage.tsx # Callback OAuth
│   ├── AuthSuccessPage.tsx  # Éxito de autenticación
│   ├── AuthErrorPage.tsx    # Error de autenticación
│   └── DashboardPage.tsx    # Dashboard principal
├── services/                # Servicios para APIs
│   ├── authService.ts       # Servicio de autenticación
│   └── taskService.ts       # Servicio de tareas
├── types/                   # Tipos TypeScript
│   ├── auth.ts              # Tipos de autenticación
│   └── task.ts              # Tipos de tareas
├── App.tsx                  # Componente raíz
├── main.tsx                 # Punto de entrada
└── index.css               # Estilos globales
```

## 🎨 Características de la UI

### Diseño Microsoft To Do

- **Layout horizontal** - Tareas en lista vertical con información inline
- **Iconos profesionales** - Lucide React en lugar de emojis del sistema
- **Colores semánticos** - Estados y prioridades con colores consistentes
- **Micro-interacciones** - Hover states y transiciones suaves

### Componentes Principales

1. **TaskList** - Container principal con:
   - Ordenamiento por fecha, prioridad, estado, título
   - Agrupación por estados
   - Selección múltiple
   - Acciones en lote

2. **TaskListItem** - Item individual con:
   - Status checkbox clickeable 
   - Título y priority badge
   - Tags inline
   - Descripción colapsable
   - Meta información (fechas, categoría)

3. **TaskForm** - Modal para crear/editar:
   - Validación en tiempo real
   - Campos requeridos marcados (*)
   - Custom selects con iconos
   - Responsive design

4. **TaskFilters** - Filtros avanzados:
   - Búsqueda de texto
   - Filtro por estado
   - Filtro por prioridad
   - Rango de fechas
   - Categoría

## 🔒 Flujo de Autenticación

1. **Login** → Usuario hace clic en "Iniciar sesión con Microsoft"
2. **Redirección** → Azure AD para autenticación
3. **Callback** → Backend procesa código de autorización
4. **Tokens** → JWT y datos del usuario se almacenan
5. **Dashboard** → Acceso a funcionalidades completas

## 🛠️ Scripts Disponibles

- `npm run dev` - Modo desarrollo con hot reload
- `npm run build` - Build para producción
- `npm run preview` - Preview del build
- `npm run lint` - Ejecutar ESLint

## 📊 Gestión de Tareas

### Estados Disponibles
- 🕒 **Pendiente** - Tareas nuevas
- 🚀 **En Progreso** - Tareas siendo trabajadas
- ✅ **Completada** - Tareas finalizadas
- ❌ **Cancelada** - Tareas canceladas

### Prioridades
- 🟢 **Baja** - Sin urgencia
- 🟡 **Media** - Prioridad normal
- 🟠 **Alta** - Requiere atención
- 🔴 **Crítica** - Máxima urgencia

### Funcionalidades
- ✅ Crear, editar, eliminar tareas
- ✅ Cambiar estados con un click
- ✅ Filtrar por múltiples criterios
- ✅ Ordenar por diferentes campos
- ✅ Operaciones en lote
- ✅ Fechas de vencimiento
- ✅ Categorización
- ✅ Sistema de tags

## 🎨 Personalización

### Colores del Sistema
```css
--primary: #0078d4;      /* Azure Blue */
--success: #107c10;      /* Green */
--warning: #f59e0b;      /* Yellow */  
--error: #d13438;        /* Red */
--info: #0078d4;         /* Blue */
```

### Componentes Personalizables
- Todos los componentes usan CSS modules
- Fácil personalización de colores y espaciado
- Variables CSS para consistencia
- Responsive design con breakpoints móviles

## 🚨 Solución de Problemas

### Error de conexión con backend
- Verifica que Azure Functions esté ejecutándose en puerto 7071
- Revisa las variables de entorno `VITE_API_BASE_URL`

### Errores de autenticación
- Confirma configuración Azure AD
- Verifica URLs de redirección
- Revisa permisos de la aplicación

### Tareas no se muestran
- Verifica logs en consola del navegador
- Confirma que el token JWT sea válido
- Revisa transformación de datos backend → frontend

### Errores de CORS
- Backend debe permitir origen `http://localhost:3001`
- Configura headers CORS correctamente

## 🔄 Backend Integration

Este frontend se integra con Azure Functions backend que maneja:
- Autenticación OAuth2 con Azure AD
- CRUD operations de tareas
- Validación y autorización
- Storage en base de datos

### Endpoints principales:
- `GET /api/task` - Obtener todas las tareas
- `POST /api/task/create` - Crear nueva tarea
- `PUT /api/task/{id}` - Actualizar tarea
- `DELETE /api/task/{id}` - Eliminar tarea
- `POST /api/auth/callback` - Callback OAuth

## 📝 Próximas Mejoras

- [ ] Arrastrar y soltar tareas
- [ ] Subtareas anidadas
- [ ] Recordatorios y notificaciones
- [ ] Colaboración en tiempo real
- [ ] Plantillas de tareas
- [ ] Exportar/importar datos
- [ ] Métricas y reportes
- [ ] Tema oscuro/claro
- [ ] Offline support

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/Nueva-Funcionalidad`)
3. Commit cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/Nueva-Funcionalidad`)
5. Crear Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver [LICENSE](LICENSE) para detalles.

---

**Desarrollado con ❤️ usando React + TypeScript + Azure**