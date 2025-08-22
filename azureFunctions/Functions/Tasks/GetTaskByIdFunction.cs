using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// revisar que hace cada uno de estos
using Microsoft.Extensions.Configuration;           // ← AGREGAR
using TaskManagement.Services.Interfaces;          // ← AGREGAR
using TaskManagement.Utils;                        // ← AGREGAR
using TaskManagement.Factories;                    // ← AGREGAR
using TaskManagement.Repositories.Implementations; // ← AGREGAR
using TaskManagement.Services.Implementations;     // ← AGREGAR

// Analizar con  detalle como funciona el codigo.
namespace TaskManagement
{
    public static class GetTaskByIdFunction
    {
        [FunctionName("getById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "task/{id}")] HttpRequest req,
            string id,
            ILogger log,
            ExecutionContext context) // revisar que hace esto
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try{
                // 1. Configuración
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                // 2. Obtener el ID de la ruta
                // string taskIdString = req.RouteValues["id"]?.ToString();
                

                // 2. Validar y convertir ID - CORREGIDO
                if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int taskId))
                {
                    return ResponseBuilder.BadRequest("ID de tarea inválido. Debe ser un número entero.");
                }


                // 3. Validar Authorization Header
                if (!req.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    return ResponseBuilder.Unauthorized("Token de autorización requerido");
                }

                var token = authHeader.ToString().Replace("Bearer ", "");
                var jwtValidator = new JwtValidator(config);
                var userId = jwtValidator.GetUserIdFromToken(token);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return ResponseBuilder.Unauthorized("Token inválido");
                }

                // 4. Inicializar servicios
                var connectionFactory = new SqlConnectionFactory(config);
                var taskRepository = new TaskRepository(connectionFactory);
                var userRepository = new UserRepository(connectionFactory);
                var taskService = new TaskService(taskRepository, userRepository);

                // 5. Obtener la tarea
                var task = await taskService.GetTaskByIdAsync(taskId, userId);

                if (task == null)
                {
                    return ResponseBuilder.NotFound($"No se encontró la tarea con ID {taskId} o no pertenece al usuario actual");
                }

                // 6. Respuesta exitosa
                log.LogInformation($"Tarea {taskId} obtenida exitosamente para usuario {userId}");
                return ResponseBuilder.Success(task, "Tarea obtenida exitosamente");

            }
            catch (Exception ex){

                log.LogError(ex, $"Error inesperado al obtener la tarea");
                return ResponseBuilder.InternalServerError("Error interno del servidor");
            }
        }
    }
}
