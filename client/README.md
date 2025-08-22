# Task Management Client

Aplicación React con TypeScript para autenticación con Azure AD y gestión de tareas.

## 🚀 Características

- ✅ Autenticación con Azure AD OAuth2
- ✅ Interfaz moderna y responsiva
- ✅ TypeScript para seguridad de tipos
- ✅ Routing protegido
- ✅ Manejo de estado con Context API
- ✅ Diseño accesible y moderno

## 📋 Prerrequisitos

- Node.js (versión 16 o superior)
- npm o yarn
- Configuración de Azure AD Application

## ⚙️ Configuración

### 1. Instalar dependencias

```bash
npm install
```

### 2. Configurar variables de entorno

Crea un archivo `.env` en la raíz del proyecto con las siguientes variables:

```env
# URL base de la API del backend
VITE_API_BASE_URL=http://localhost:7071/api

# Configuración de Azure AD
VITE_AZURE_CLIENT_ID=tu-client-id-aqui
VITE_AZURE_TENANT_ID=tu-tenant-id-aqui

# URL de redirección (debe coincidir con la configurada en Azure AD)
VITE_REDIRECT_URI=http://localhost:3000/auth/callback
```

### 3. Configurar Azure AD

1. Ve al [Portal de Azure](https://portal.azure.com)
2. Busca "Azure Active Directory"
3. Ve a "App registrations" > "New registration"
4. Configura:
   - **Name**: Task Management Client
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: Web - `http://localhost:3000/auth/callback`
5. Copia el **Application (client) ID** y úsalo como `VITE_AZURE_CLIENT_ID`
6. Copia el **Directory (tenant) ID** y úsalo como `VITE_AZURE_TENANT_ID`
7. Ve a "Certificates & secrets" > "New client secret" y copia el valor
8. Ve a "API permissions" y asegúrate de tener estos permisos:
   - `openid`
   - `profile`
   - `email`

## 🚀 Ejecutar la aplicación

### Modo desarrollo

```bash
npm run dev
```

La aplicación estará disponible en `http://localhost:3000`

### Construir para producción

```bash
npm run build
```

### Preview de producción

```bash
npm run preview
```

## 📁 Estructura del proyecto

```
src/
├── components/           # Componentes reutilizables
│   └── ProtectedRoute.tsx
├── contexts/            # Contextos de React
│   └── AuthContext.tsx
├── pages/              # Páginas principales
│   ├── LoginPage.tsx
│   ├── AuthCallbackPage.tsx
│   └── DashboardPage.tsx
├── services/           # Servicios para APIs
│   └── authService.ts
├── types/              # Tipos TypeScript
│   └── auth.ts
├── App.tsx             # Componente principal
├── main.tsx           # Punto de entrada
└── index.css          # Estilos globales
```

## 🔒 Flujo de autenticación

1. **Login**: Usuario hace clic en "Iniciar sesión con Microsoft"
2. **Redirección**: Se redirige a Azure AD para autenticación
3. **Callback**: Azure AD redirige de vuelta con código de autorización
4. **Intercambio**: Se envía el código al backend para obtener tokens
5. **Almacenamiento**: Se guardan el token JWT y datos del usuario
6. **Acceso**: Usuario puede acceder a rutas protegidas

## 🛠️ Scripts disponibles

- `npm run dev` - Ejecutar en modo desarrollo
- `npm run build` - Construir para producción
- `npm run preview` - Preview de la construcción
- `npm run lint` - Ejecutar ESLint

## 🔧 Configuración del Backend

Asegúrate de que tu backend esté ejecutándose en `http://localhost:7071` (o actualiza `VITE_API_BASE_URL`) y que el endpoint `/api/auth/callback` esté configurado correctamente.

## 🎨 Personalización

### Estilos

Los estilos están organizados en:
- `src/index.css` - Estilos globales
- `src/pages/*.css` - Estilos específicos de páginas

### Colores principales

- Primario: `#0078d4` (Azure Blue)
- Éxito: `#10b981` (Green)
- Error: `#ef4444` (Red)
- Gradiente: `#667eea` a `#764ba2`

## 🚨 Solución de problemas

### Error de CORS

Si encuentras errores de CORS, asegúrate de que tu backend tenga configurado CORS para permitir `http://localhost:3000`.

### Errores de redirección

Verifica que las URLs de redirección en Azure AD coincidan exactamente con `VITE_REDIRECT_URI`.

### Variables de entorno

Las variables de entorno deben empezar con `VITE_` para ser accesibles en Vite.

## 📝 Próximas funcionalidades

- [ ] Gestión de tareas
- [ ] Colaboración en equipo
- [ ] Reportes y analytics
- [ ] Notificaciones
- [ ] Gestión de roles avanzada

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles. 