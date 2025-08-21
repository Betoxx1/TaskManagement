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
    public static class CreateTaskFunction
    {
        [FunctionName("create")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "task/create")] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Create Task function processed a request.");

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

                // 3. Leer y validar el body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    return ResponseBuilder.BadRequest("Body de la solicitud es requerido");
                }

                var createTaskDto = JsonConvert.DeserializeObject<CreateTaskDto>(requestBody);
                
                if (createTaskDto == null)
                {
                    return ResponseBuilder.BadRequest("Formato de datos inválido");
                }

                // 4. Validaciones de negocio
                if (string.IsNullOrWhiteSpace(createTaskDto.Title))
                {
                    return ResponseBuilder.BadRequest("El título es requerido");
                }

                // 5. Inicializar servicios
                var connectionFactory = new SqlConnectionFactory(config);
                var taskRepository = new TaskRepository(connectionFactory);
                var userRepository = new UserRepository(connectionFactory);
                var taskService = new TaskService(taskRepository, userRepository);

                // 6. Crear la tarea
                var createdTask = await taskService.CreateTaskAsync(createTaskDto, userId);

                // 7. Respuesta exitosa
                log.LogInformation($"Tarea creada exitosamente con ID: {createdTask.Id}");
                return ResponseBuilder.Created(createdTask, "Tarea creada exitosamente");

            }
            catch (ArgumentException ex)
            {
                log.LogWarning($"Error de validación: {ex.Message}");
                return ResponseBuilder.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error inesperado al crear la tarea");
                return ResponseBuilder.InternalServerError("Error interno del servidor");
            }
        }
    }
}
