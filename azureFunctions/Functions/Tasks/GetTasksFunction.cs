using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TaskManagement.Services.Interfaces;
using TaskManagement.Utils;
using TaskManagement.Factories;
using TaskManagement.Repositories.Implementations;
using TaskManagement.Services.Implementations;

namespace TaskManagement
{
    public static class GetTasksFunction
    {
        [FunctionName("get")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "task")] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Get Tasks function processed a request.");

            try
            {
                // 1. Configuración
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                // 2. Validar Authorization Header
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

                // string userId = "user1";

                // 3. Inicializar servicios
                var connectionFactory = new SqlConnectionFactory(config);
                var taskRepository = new TaskRepository(connectionFactory);
                var userRepository = new UserRepository(connectionFactory);
                var taskService = new TaskService(taskRepository, userRepository);

                // 4. Obtener todas las tareas del usuario
                var tasks = await taskService.GetAllTasksAsync(userId);

                // 5. Respuesta exitosa
                log.LogInformation($"Se obtuvieron las tareas exitosamente para el usuario {userId}");
                return ResponseBuilder.Success(tasks, "Tareas obtenidas exitosamente");

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error inesperado al obtener las tareas");
                return ResponseBuilder.InternalServerError("Error interno del servidor");
            }
        }
    }
}
