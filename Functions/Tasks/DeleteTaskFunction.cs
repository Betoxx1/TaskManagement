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
    public static class DeleteTaskFunction
    {
        [FunctionName("delete")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "task/{id}")] HttpRequest req,
            string id,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Delete Task function processed a request.");

            try
            {
                // 1. Configuración
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                // 2. Validar y convertir ID
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

                // 5. Eliminar la tarea
                var wasDeleted = await taskService.DeleteTaskAsync(taskId, userId);

                if (!wasDeleted)
                {
                    return ResponseBuilder.NotFound($"No se encontró la tarea con ID {taskId} o no pertenece al usuario actual");
                }

                // 6. Respuesta exitosa
                log.LogInformation($"Tarea {taskId} eliminada exitosamente para usuario {userId}");
                return ResponseBuilder.Success("Tarea eliminada exitosamente");

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error inesperado al eliminar la tarea");
                return ResponseBuilder.InternalServerError("Error interno del servidor");
            }
        }
    }
}
