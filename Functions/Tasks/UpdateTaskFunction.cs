using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TaskManagement.Services.Interfaces;
using TaskManagement.Utils;
using TaskManagement.Factories;
using TaskManagement.Repositories.Implementations;
using TaskManagement.Services.Implementations;
using TaskManagement.DTOs;

namespace TaskManagement
{
    public static class UpdateTaskFunction
    {
        [FunctionName("update")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "task/{id}")] HttpRequest req,
            string id,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Update Task function processed a request.");

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

                // 4. Leer y validar el body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    return ResponseBuilder.BadRequest("Body de la solicitud es requerido");
                }

                var updateTaskDto = JsonConvert.DeserializeObject<UpdateTaskDto>(requestBody);
                
                if (updateTaskDto == null)
                {
                    return ResponseBuilder.BadRequest("Formato de datos inválido");
                }

                // 5. Inicializar servicios
                var connectionFactory = new SqlConnectionFactory(config);
                var taskRepository = new TaskRepository(connectionFactory);
                var userRepository = new UserRepository(connectionFactory);
                var taskService = new TaskService(taskRepository, userRepository);

                // 6. Actualizar la tarea
                var updatedTask = await taskService.UpdateTaskAsync(taskId, updateTaskDto, userId);

                if (updatedTask == null)
                {
                    return ResponseBuilder.NotFound($"No se encontró la tarea con ID {taskId} o no pertenece al usuario actual");
                }

                // 7. Respuesta exitosa
                log.LogInformation($"Tarea {taskId} actualizada exitosamente para usuario {userId}");
                return ResponseBuilder.Success(updatedTask, "Tarea actualizada exitosamente");

            }
            catch (ArgumentException ex)
            {
                log.LogWarning($"Error de validación: {ex.Message}");
                return ResponseBuilder.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error inesperado al actualizar la tarea");
                return ResponseBuilder.InternalServerError("Error interno del servidor");
            }
        }
    }
}
