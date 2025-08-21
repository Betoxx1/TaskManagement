│   │   ├── CreateTaskFunction.cs
│   │   ├── GetTasksFunction.cs
│   │   ├── GetTaskByIdFunction.cs
│   │   ├── UpdateTaskFunction.cs
│   │   ├── DeleteTaskFunction.cs
│   │   └── FilterTasksFunction.cs 


taskdbazure.database.windows.net
admin_user
adm1n2025*


# Compilar
dotnet build

# Ejecutar
func start --port 7071

# Probar
curl -X GET "http://localhost:7071/api/task/1" \
  -H "Authorization: Bearer your-jwt-token"